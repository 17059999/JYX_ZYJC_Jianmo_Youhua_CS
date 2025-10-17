###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

Param(
    [Parameter(Mandatory=$True)]
    [string]$targetVM,
    [string]$userNameInVM="IOLab",
    [string]$userPwdInVM="Password01!",
    [boolean]$isLinux = $false
)

$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent;
$sourceFolder = "$scriptPath\Source"
if(-not(Test-Path -Path $sourceFolder)){
    Write-Host "Should copy source file to $sourceFolder"
    return;
}
$fileNames = New-Object System.Collections.ArrayList
Get-ChildItem $sourceFolder | `
    Foreach-Object {
    $fileNames.Add($_.FullName)
}


Function Get-RemoteSession {
    Param
    (
        [string]$FullUserName, 
        [string]$UserPassword,
        [string]$RemoteIP
    )

    #check if Remote machine is in TrustedHosts
    $originalValue = (get-item WSMan:\localhost\Client\TrustedHosts).value
    [string]$originalValue = $originalValue.Replace("*","");

    if($originalValue.Length -gt 0){
        if($originalValue -match "$RemoteIP,+"){
            Write-Host "$RemoteIP already exists in trust hosts list"
        }else{
            $originalValue = $originalValue + ",$RemoteIP"
            Write-Host "Add $RemoteIP to Trusted Hosts"
            set-item WSMan:\localhost\Client\TrustedHosts -Value $originalValue -force
        }
    }else{
        Write-Host "Add $RemoteIP to Trusted Hosts"
        set-item WSMan:\localhost\Client\TrustedHosts -Value $RemoteIP -force
    }

    $retryCount = 1
    While($retryCount -lt 10)
    {
        Try
        {
            $RemoteCredential = New-Object System.Management.Automation.PSCredential -ArgumentList $FullUserName,(ConvertTo-SecureString $UserPassword -AsPlainText -Force)
            Write-Host "Try To Connect to remote machine $RemoteIP"
            $RemoteSession = New-PSSession -ComputerName $RemoteIP -Credential $RemoteCredential -ErrorAction Stop
                
            if($RemoteSession -eq $null)
            {
                Write-Host "Failed to connect to $remoteIP, Retry $retryCount" -ForegroundColor Yellow
                Start-Sleep -s 60
                $retryCount++
            }
            else{
                Write-Host "Connected to remote session"
                break
            }
        }
        Catch
        {
            $ErrorMessage = $_.Exception.Message
            $errorMsg = "Failed to connect to $remoteIP. Retry count $retryCount, The error message was $ErrorMessage"

            Write-Host $errorMsg -ForegroundColor Yellow
            Start-Sleep -s 20
            $retryCount++
        }
    }

    IF($RemoteSession -eq $null)
    {
        $errorMsg = "Failed to connect to $remoteIP with userName: $FullUserName, password: $UserPassword after retry $retryCount times"
        Write-Error $errorMsg
    }

    return $RemoteSession
}
Function CopyFilesForLinux{
    Param
    (
        [string]$source, 
        [string]$target
    )
    Write-Host "cmd /c scp -r $($source) $target"
    cmd /c scp -r "$($source)" "$target"
    Write-Host "Copy completed"
}
#------------------------------------------------------------------------------------------
# Copy source files to Azure VM 
#------------------------------------------------------------------------------------------
Function CopyFiles-VM{
    Write-Host "Copy source files to destination VM: $targetVM"
    
    if($isLinux){
        [string]$fullUserName = $userNameInVM + "@"+ $targetVM

        Write-Host "remove $($fullUserName):/home/$userNameInVM"
        cmd /c ssh $fullUserName "rm -r $($fullUserName):/home/$userNameInVM/Source"
        CopyFilesForLinux $($sourceFolder) "$($fullUserName):/home/$userNameInVM"
    } else {
        [string]$fullUserName = $targetVM + "\"+$userNameInVM

        Write-Host "Create PSSession to connect to Azure VM: $targetVM"
        
        $vmSession = Get-RemoteSession -FullUserName $fullUserName -UserPassword $userPwdInVM -RemoteIP $targetVM
        
        Write-Host "Remove TempSource folder if exists on remote vm"
        Invoke-Command -Session $vmSession -ScriptBlock {Push-Location "C:\"; if(Test-Path -Path "C:\TempSource"){Remove-Item C:\TempSource\ -Force -Recurse;} }

        Copy-Item $sourceFolder -Destination "C:\TempSource" -ToSession $vmSession -Force -Recurse
        Write-Host "Copy completed"    
        Remove-PSSession $vmSession
    }

}

Function ExpandSource-VM{
    Write-Host "Copy TempSource folder to destination VM: $targetVM"
    $fileNames | ForEach-Object {  
        $currFile = [IO.FileInfo]$($_);
        if($currFile.Extension -eq '.zip') {  
            $tempFolder = ".\$($currFile.Basename)"
            if(Test-Path -Path $tempFolder) {
                Remove-Item $tempFolder -Force -Recurse;
            }
            Write-Host "Expand Archive $($currFile.Name)"                        
            if ($psversiontable.PSVersion.Major -ge 5)
            {
                Expand-Archive $currFile.FullName -DestinationPath $tempFolder	
            }
            else
            {
                $shell = New-Object -com shell.application
                $zip = $shell.NameSpace("$($currFile.FullName)")
                if(!(Test-Path -Path $tempFolder))
                {
                    New-Item -ItemType directory -Path $tempFolder
                }
                $shell.Namespace($tempFolder).CopyHere($zip.items(), 0x14)
            }

            if($isLinux){
                [string]$fullUserName = $userNameInVM + "@"+ $targetVM
                CopyFilesForLinux $($tempFolder) "$($fullUserName):/home/$userNameInVM"
            } else {
                [string]$fullUserName = $targetVM + "\"+$userNameInVM

                Write-Host "Create PSSession to connect to Azure VM: $targetVM"
                
                $vmSession = Get-RemoteSession -FullUserName $fullUserName -UserPassword $userPwdInVM -RemoteIP $targetVM
                
                Write-Host "Remove $($currFile.Basename) folder if exists on remote vm"
                $tarFolder = "C:\$($currFile.Basename)"
                Invoke-Command -Session $vmSession -ScriptBlock {param([string]$tarFolder) Push-Location "C:\"; if(Test-Path -Path $tarFolder){Remove-Item $tarFolder -Force -Recurse;} } -ArgumentList $tarFolder
                
                Copy-Item $tempFolder -Destination "C:\$($currFile.Basename)" -ToSession $vmSession -Force -Recurse
                Write-Host "Copy completed"    
                Remove-PSSession $vmSession
            }

            if(Test-Path -Path $tempFolder) {
                Remove-Item $tempFolder -Force -Recurse;
            }
        }
    } 
}

# Run MSI package.
[ScriptBlock]$RunMSI = {
    Param([string]$FilePath)
    # Guarantee commands run in powershell environment
    $Task = "cmd /c msiexec -i $FilePath -q"
    # Create task
    cmd /c $Task
}

Function InstallMSI-VM{
    if(-not $isLinux){
        [string]$fullUserName = $targetVM + "\"+$userNameInVM
        $fileNames | ForEach-Object {  
                $currFile = [IO.FileInfo]$($_);
                if($currFile.Extension -eq '.msi') {  
                Write-Host "install $($currFile.Name)"
                $vmSession = Get-RemoteSession -FullUserName $fullUserName -UserPassword $userPwdInVM -RemoteIP $targetVM
                Invoke-Command -Session $vmSession -ScriptBlock $RunMSI -ArgumentList "C:\TempSource\$($currFile.Name)"
                Remove-PSSession $vmSession
            }
        }
    }
}
Function Main{  
    CopyFiles-VM
    ExpandSource-VM
    InstallMSI-VM
}

Main

Pop-Location