#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Configure-SambaToLinux.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Linux
##
##############################################################################

$logFile = "./" + $MyInvocation.MyCommand.Name + ".log"
Start-Transcript -Path $logFile -Append -Force

$computerName = Get-Content "Temp/Name.txt"
$ConfigureFile = "Temp/Protocol.xml"
[Xml]$Script:Setup = Get-Content $ConfigureFile
if($Script:Setup -eq $null)
{
    Write-Host "Protocol configure file $ConfigureFile is invalid." -ForegroundColor Red
	Stop-Transcript
    exit 1
}
$scriptName = "Configure-SambaToLinux.sh"
$domainAccount = $Script:Setup.lab.core.username

#------------------------------------------------------------------------------------------
# Add shared folder sh script
#------------------------------------------------------------------------------------------
Function AddSharedFolder{
    param(
        [string]$SharedFolderName,
        [string]$PhysicalFolderName,
        [string]$ParentFolderName,
        [string]$SmbGroup
    )

    $shScript = [System.Text.StringBuilder]::new()

    $rootPathName = "srv"
    $fullSharedFolderPath = $PhysicalFolderName

    if($null -ne $ParentFolderName -and $ParentFolderName.Length -gt 0){
        $fullSharedFolderPath = "$ParentFolderName/$PhysicalFolderName"
    }

    # Add shared folder configuration to the smb.conf file
    $smbConfigFilePathInLinux = " /etc/samba/smb.conf";
    [void]$shScript.AppendLine("echo `"[$SharedFolderName]`" >> $smbConfigFilePathInLinux");
    [void]$shScript.AppendLine("echo `"  comment = needs username and password to access`" >>$smbConfigFilePathInLinux");
    [void]$shScript.AppendLine("echo `"  path = /$rootPathName/$fullSharedFolderPath/`" >>$smbConfigFilePathInLinux");
    [void]$shScript.AppendLine("echo `"  browseable = yes`" >>$smbConfigFilePathInLinux");
    [void]$shScript.AppendLine("echo `"  guest ok = no`" >>$smbConfigFilePathInLinux");
    [void]$shScript.AppendLine("echo `"  writable = yes`" >>$smbConfigFilePathInLinux");
    [void]$shScript.AppendLine("echo `"  valid users = @$SmbGroup`" >>$smbConfigFilePathInLinux");

    if($null -ne $ParentFolderName -and $ParentFolderName.Length -gt 0){
        $parentFolderPath = "/$rootPathName/$ParentFolderName"
    
        # Validate whether the parent folder is existed, if it is not existed, create this folder
        [void]$shScript.AppendLine("if [ ! -d `"$parentFolderPath`" ]; then");
        [void]$shScript.AppendLine("sudo mkdir $parentFolderPath ");
        # The samba group needs to have read, write and execute permission on the shared folder
        [void]$shScript.AppendLine("sudo setfacl -R -m `"g:$SmbGroup"+":rwx`"$parentFolderPath"); 
        [void]$shScript.AppendLine("fi");
    }

    $sharedFolderPath = "/$rootPathName/$fullSharedFolderPath/"
    # Validate whether the shared folder is existed, if it is not existed, create this folder
    [void]$shScript.AppendLine("if [ ! -d `"$sharedFolderPath`" ]; then");
    # Create the shared folder
    [void]$shScript.AppendLine("sudo mkdir $sharedFolderPath"); 
    # The samba group needs to have read, write and execute permission on the shared folder
    [void]$shScript.AppendLine("sudo setfacl -R -m `"g:$SmbGroup"+":rwx`" $sharedFolderPath"); 
    [void]$shScript.AppendLine("fi");

    return $shScript.ToString()
}

#------------------------------------------------------------------------------------------
# Generate the sh file
#------------------------------------------------------------------------------------------
Function GenerateConfigurationScriptFileForLinux{    
    $shSCriptParamBuilder = [System.Text.StringBuilder]::new()
    [void]$shSCriptParamBuilder.AppendLine("#!/bin/sh");

    # Switch to 'root' account
    [void]$shSCriptParamBuilder.AppendLine("sudo -i");

    # Disable Firewall
    [void]$shSCriptParamBuilder.AppendLine("sudo ufw disable");

    [void]$shSCriptParamBuilder.AppendLine("sudo apt-get update");

    # Install Samba service
    [void]$shSCriptParamBuilder.AppendLine("sudo apt-get install -y samba");

    # Install acl packages
    [void]$shSCriptParamBuilder.AppendLine("sudo apt-get install acl");

    $ignoreValidUserName = "--force-badname"
    $defaultPassword = "Password01!"

    # Add Administrator account
    $administrator = $domainAccount
    [void]$shSCriptParamBuilder.AppendLine("sudo adduser $administrator $ignoreValidUserName");
    [void]$shSCriptParamBuilder.AppendLine("(echo $defaultPassword; echo $defaultPassword) | smbpasswd -s -a $administrator");

    # Add a Guest account
    $guest = "Guest"
    [void]$shSCriptParamBuilder.AppendLine("sudo adduser $guest $ignoreValidUserName");
    [void]$shSCriptParamBuilder.AppendLine("(echo $defaultPassword; echo $defaultPassword) | smbpasswd -s -a $guest");

    # Add a nonadmin account
    $noadmin = "noadmin"
    [void]$shSCriptParamBuilder.AppendLine("sudo adduser $noadmin $ignoreValidUserName");
    [void]$shSCriptParamBuilder.AppendLine("(echo $defaultPassword; echo $defaultPassword) | smbpasswd -s -a $noadmin"); 

    $smbGroup = "samba"
    # Create a 'samba' group
    [void]$shSCriptParamBuilder.AppendLine("sudo groupadd $smbGroup");

    # Add the users to 'samba' group
    [void]$shSCriptParamBuilder.AppendLine("sudo gpasswd -a $administrator $smbGroup");
    [void]$shSCriptParamBuilder.AppendLine("sudo gpasswd -a $guest $smbGroup");
    [void]$shSCriptParamBuilder.AppendLine("sudo gpasswd -a $noadmin $smbGroup");

    # Add SMBBasic shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "SMBBasic" -PhysicalFolderName "SMBBasic" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);

    # Add SameWithSMBBasic shared folder in samba service, the shared folder points to the physical folder SMBBasic 
    $subShScript = AddSharedFolder -SharedFolderName "SameWithSMBBasic" -PhysicalFolderName "SMBBasic" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);

    # Add ShareForceLevel2 shared folder in samba service
    # TODO: need do some special configuration for the ShareForceLevel2 folder
    $subShScript = AddSharedFolder -SharedFolderName "ShareForceLevel2" -PhysicalFolderName "ShareForceLevel2" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);

    # Add SMBEncrypted shared folder in samba service
    # TODO: need do some special configuration for the SMBEncrypted folder
    $subShScript = AddSharedFolder -SharedFolderName "SMBEncrypted" -PhysicalFolderName "SMBEncrypted" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);

    # Add FileShare shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "FileShare" -PhysicalFolderName "FileShare" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);

    # Add SMBReFSShare shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "SMBReFSShare" -PhysicalFolderName "SMBReFSShare" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);

    # Add SMBFAT32Share shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "SMBFAT32Share" -PhysicalFolderName "SMBFAT32Share" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);   

    # Add ExistingFolder shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "ExistingFolder" -PhysicalFolderName "ExistingFolder" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);   

    # Add AzCBAC shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "AzCBAC" -PhysicalFolderName "AzCBAC" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);  

    # Add AzFolder shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "AzFolder" -PhysicalFolderName "AzFolder" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);  

    # Add AzShare shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "AzShare" -PhysicalFolderName "AzShare" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);  

    # Add AzFile shared folder in samba service
    $subShScript = AddSharedFolder -SharedFolderName "AzFile" -PhysicalFolderName "AzFile" -ParentFolderName $null -SmbGroup $smbGroup
    [void]$shSCriptParamBuilder.AppendLine($subShScript);  

    $streamWriter = [System.IO.StreamWriter] $scriptName
    $streamWriter.Write($shSCriptParamBuilder.ToString());
    $streamWriter.Close();
}

Function Invoke-Sudo { 
    Write-Host "invoke-sudo $args"
    & /usr/bin/env sudo pwsh -command "& $args" 
}

Function Main{    
    GenerateConfigurationScriptFileForLinux
    Invoke-Sudo "chmod 777 $scriptName"
    Invoke-Sudo "./$scriptName"
}

Main

Pop-Location