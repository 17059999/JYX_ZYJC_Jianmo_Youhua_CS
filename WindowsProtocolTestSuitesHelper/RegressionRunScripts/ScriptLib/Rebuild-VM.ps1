#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Rebuild-VM.ps1
## Purpose:        Copy a VM Image and Use sysprep to Rebuild a new VM
## Version:        1.0 (28 Oct, 2008)
##
##############################################################################

param(
[string]$srcVMPath,
[string]$VMDir,
[string]$ISOPath,
[string]$sysprepPathOnHost,
[string]$OSVersion         ="W2k3", 

[string]$CPUArchitecture   ="x86", 
[string]$srcVMIndex        = "1",
[string]$newVMIndex        = "3",
[string]$userInVM          = "administrator",
[string]$pwdInVM           = "Password01!"
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Rebuild-VM.ps1] ..." -foregroundcolor cyan
Write-Host "`$srcVMPath           = $srcVMPath"
Write-Host "`$VMDir               = $VMDir"
Write-Host "`$ISOPath             = $ISOPath"
Write-Host "`$sysprepPathOnHost   = $sysprepPathOnHost"
Write-Host "`$OSVersion           = $OSVersion"
Write-Host "`$CPUArchitecture     = $CPUArchitecture"
Write-Host "`$srcVMIndex          = $srcVMIndex"
Write-Host "`$newVMIndex          = $newVMIndex"
Write-Host "`$userInVM            = $userInVM"
Write-Host "`$pwdInVM             = $pwdInVM"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script will copy a old VM , wipe its' system info, then rebuild a new WM."
    Write-host
    Write-host "Example: Rebuild-VM.ps1 \\WSEATC-PROTOCOL\PETLabStore\VMLib D:\TestRebuildVM\VM D:\Automation\ISO D:\TestRebuildVM\Sysprep W2k3 x86 1 3 administrator Password01!"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($srcVMPath -eq $null -or $srcVMPath -eq "")
{
    Throw "Parameter scrVMPath is required."
}
if ($VMDir -eq $null -or $VMDir -eq "")
{
    Throw "Parameter VMDir in local host is required."
}
if ($ISOPath -eq $null -or $ISOPath -eq "")
{
    Throw "Parameter ISOPath is required."
}
if ($sysprepPathOnHost -eq $null -or $sysprepPathOnHost -eq "")
{
    Throw "Parameter sysprepPathOnHost is required."
}

#----------------------------------------------------------------------------
# Function Modify AnswerFile (for xp ,2k3)
#----------------------------------------------------------------------------
Function Modify-InfAnswerfile([string]$filepath,[string]$computerBiosName,[string]$IpAddress)
{
    $answerInform = gc $filepath
    for($index=0;$index -lt $answerInform.count; $index++)
    {
        if($answerInform[$index] -ne $null)
        {
            if($answerInform[$index].contains("ComputerName=") -and ($computerBiosName -ne $null))
            {
                $answerInform[$index]= "    ComputerName="+"$computerBiosName"
            }
            if($answerInform[$index].contains("IPAddress=")-and ($IpAddress -ne $null))
            {
                $answerInform[$index]= "IPAddress="+"$IpAddress"
            }
        }
    }
    $answerInform |Set-Content $filepath
}
#----------------------------------------------------------------------------
# Function Modify AnswerFile (for vista 2k8 Win7)
#----------------------------------------------------------------------------
Function Config-XMLAnswerFile([string]$AnswerFile,[string]$IPVar,[string]$BiosName)
{
    [xml]$answerContent = get-content $AnswerFile
    #get the IPAddress Node 
    $IpAddressList = $answerContent.GetElementsByTagName("IpAddress")
    foreach($IpAddress in $IpAddressList)
    {
        $IpAddress.InnerText ="$IPVar"
    }
    #get the ComputerName Node 
    $computerNameNode = $answerContent.GetElementsByTagName("AuditComputerName")
    $nameNode = $computerNameNode.Item(0)
    $name = $nameNode.childnodes.Item(0)
    $name.InnerText = $BiosName
    $answerContent.InnerXML | Set-Content $AnswerFile
    #(Get-Content $AnswerFile)| Set-Content $AnswerFile
}

#----------------------------------------------------------------------------
# Function Delete Exist VM Directory in Disk
#----------------------------------------------------------------------------
Function Delete-ExistVMDirectory([string]$path)
{
    if (($path -eq $null) -or( $path.Length -le 0))
    {
        Write-Host "Delete Exist VM completed." -foregroundcolor Green
        return
    }
    if(-not(Test-Path -path $path))
    {
        Write-Host "Delete Exist VM completed." -foregroundcolor Green
        return
    }
    if(@(get-childitem $path).Count -le 0)
    {
        Remove-Item @($path) -force -Recurse
        return
    }
    get-childitem $path | where-object { $_.PsIsContainer} | foreach { Remove-Item @($_.FullName) -force -Recurse}
    if(-not(Test-Path -path $path))
    { 
        Write-Host "Delete Exist VM completed." -foregroundcolor Green
        return
    }
    Remove-Item @($path) -force -Recurse
    Write-Host "Delete Exist VM completed." -foregroundcolor Green
}

#----------------------------------------------------------------------------
# Vefity parameters
#----------------------------------------------------------------------------

Write-Host "$newVMIndex"
$srcVMIndex = "0$srcVMIndex"
$newVMIndex = "0$newVMIndex"
$VMBiosNamePrefix = "SUT"
if($OSVersion -eq "WXP" -or $OSVersion -eq "Win7" )
{
    $VMBiosNamePrefix = "EndPoint"
}
$VMBiosName = $VMBiosNamePrefix +  $newVMIndex
$SyspreIsoName = "Sysprep.iso"
$sysprepFilePathOnHost = "$sysprepPathOnHost\$OSVersion\$CPUArchitecture"

#----------------------------------------------------------------------------
# Get VM folder name, computer name 
#----------------------------------------------------------------------------
Write-Host " Collection information for Rebuild VM ..."  -foregroundcolor Yellow

$scrVMName       = .\Get-VMName.ps1 $OSVersion $CPUArchitecture $VMBiosNamePrefix $srcVMIndex
$newVMName       = .\Get-VMName.ps1 $OSVersion $CPUArchitecture $VMBiosNamePrefix $newVMIndex
$scrVMIpAddress  = .\Get-ComputerIP.ps1 IPv4 $VMBiosNamePrefix $srcVMIndex 
$newVMIpAddress  = .\Get-ComputerIP.ps1 IPv4 $VMBiosNamePrefix $newVMIndex 

$ipV4Address     = .\Get-ComputerIP.ps1 IPv4 $VMBiosNamePrefix $newVMIndex
$ipV4GateWay     = "192.168.0.201"
$ipV4DNS         = "192.168.0.201"

$ipV6Address     = .\Get-ComputerIP.ps1 IPv6 $VMBiosNamePrefix $newVMIndex $true
$ipV6GateWay     = "2008::c9"
$ipV6DNS         = "2008::c9"

$copyFileScriptOnHost = "$sysprepPathOnHost\CopyFile\CopySysprepFiles.bat"
#Vista,2k8,Win7 answerfile
$sysprepAnswerFilePath = "$sysprepFilePathOnHost\SysprepAutoUNattend.xml"
$ipAnswerFilePath = $null
if($OSVersion -eq "W2K3" -or $OSVersion -eq "WXP")
{
    $sysprepAnswerFilePath = "$sysprepFilePathOnHost\sysprep.inf"
    $ipAnswerFilePath = "$sysprepFilePathOnHost\IPAnswerFile.inf"
}

$scrVHDFilePath = "$VMDir\$newVMName\Virtual Hard Disks\$scrVMName.vhd" 
$newVHDFilePath = "$VMDir\$newVMName\Virtual Hard Disks\$newVMName.vhd" 

if($OSVersion -eq "W2K3" -or $OSVersion -eq "WXP")
{
    .\TurnOff-FileReadonly.ps1 "$sysprepAnswerFilePath"
    Modify-InfAnswerfile "$sysprepAnswerFilePath" "$VMBiosName" $null
}
else
{
    .\TurnOff-FileReadonly.ps1 "$sysprepAnswerFilePath"
    Config-XMLAnswerFile "$sysprepAnswerFilePath" "$newVMIpAddress" "$VMBiosName"
}

if($ipAnswerFilePath -ne $null)
{
    .\TurnOff-FileReadonly.ps1 "$ipAnswerFilePath" 
    Modify-InfAnswerfile "$ipAnswerFilePath" $null "$newVMIpAddress"
}

Write-Host "Copy $SyspreIsoName from $ISOPath to $VMDir ..." -foregroundcolor Yellow
cmd /c robocopy  $ISOPath  $VMDir  $SyspreIsoName

##----------------------------------------------------------------------------
## Turn off and Remove exist source VM
##----------------------------------------------------------------------------
Write-Host "Remove existing VM in HyperV which has the same name as the $scrVMName and $newVMName..." -foregroundcolor Yellow
.\Remove-VM.ps1 $scrVMName
.\Remove-VM.ps1 $newVMName

#----------------------------------------------------------------------------
# Copy and import VM
#----------------------------------------------------------------------------
Write-Host "Copy and import VMs..."  -foregroundcolor Yellow

Write-Host "Delete Exist VM Directory["$VMDir\$newVMName"]..."
Delete-ExistVMDirectory "$VMDir\$newVMName"

Write-Host "Copy VM from $scrVMName to $newVMName ..." 
cmd /c robocopy /MIR /NFL /NDL "$srcVMPath\$scrVMName" "$VMDir\$newVMName" 2>&1 |Write-Host
#.\Copy-VM.ps1 $srcVMPath $VMDir $scrVMName
#RENAME drive:$VMDir $scrVMName $newVMName

Write-Host  "TurnOff FileReadonly for the VHD file[$scrVHDFilePath]..."  -foregroundcolor Yellow
.\TurnOff-FileReadonly "$scrVHDFilePath"

Write-Host "Import VM: $scrVMName ..." -foregroundcolor Yellow
.\Import-VM.ps1 "$VMDir" $newVMName
Write-Host "Remame VM in HyperV from $scrVMName to $newVMName ..." -foregroundcolor Yellow
.\Rename-VM.ps1 "$scrVMName" "$newVMName"

#----------------------------------------------------------------------------
# Mount Sysprep.iso to VM
#----------------------------------------------------------------------------
Write-Host "Mount $SyspreIsoName to VM ..."
.\Mount-VMISO.ps1 $newVMName ($VMDir + "\" + $SyspreIsoName) 1 0 . 

#----------------------------------------------------------------------------
# run  VM
#----------------------------------------------------------------------------
Write-Host "Start to run VM: $newVMName ..."  -foregroundcolor Yellow
.\Run-VM.ps1 $newVMName

Write-Host "Waiting for $scrVMIpAddress starting up ..."
.\WaitFor-ComputerReady.ps1 $scrVMIpAddress $userInVM $pwdInVM

#----------------------------------------------------------------------------
#  Collection information for Rebuild VM 
#----------------------------------------------------------------------------
$sysDriveInVM         = .\Get-RemoteSystemDrive.ps1 $scrVMIpAddress $userInVM $pwdInVM
$cdRomDriveInVM       = .\Get-CDROMDrive.ps1 $scrVMIpAddress $userInVM $pwdInVM
$sysprepCopyFileScript= $sysDriveInVM + "\CopyFile\CopySysprepFiles.bat"
$sysprepPathInVM      = $cdRomDriveInVM + "\Sysprep\$OSVersion\$CPUArchitecture"
$sysprepCleanUpScript = $sysDriveInVM + "\SysprepCleanUp.bat"
#$sysprepFactoryScript = $sysDriveInVM + "\SysprepWipe.bat"
$sysprepFactoryScript = $sysDriveInVM + "\SysprepTemp\$OSVersion\$CPUArchitecture\SysprepWipe.bat"
$sysprepAuditScript   = $sysDriveInVM + "\SysprepConfigMachine.bat $OSVersion $sysDriveInVM $ipV4Address $ipV4GateWay $ipV4DNS $ipV6Address $ipV6GateWay $ipV6DNS"
if($OSVersion -eq "W2K3" -or $OSVersion -eq "WXP")
{
    $sysprepFactoryScript = $sysprepPathInVM + "\SysprepWipe.bat"
    $sysprepAuditScript = "$sysDriveInVM\sysprep\sysprep.exe /reseal /activated /quiet /reboot"
}

#----------------------------------------------------------------------------
# Config AutoLogon For VMs
#----------------------------------------------------------------------------
Write-Host "Start to set autologon for $scrVMIpAddress ..."  -foregroundcolor Yellow
.\Config-AutoLogon.ps1 $scrVMIpAddress $userInVM $pwdInVM 

Write-Host "Waiting for VM starting up ..." 
.\WaitFor-ComputerReady.ps1 $scrVMIpAddress $userInVM $pwdInVM 

#----------------------------------------------------------------------------
# Copy Sysprep file to VM
#----------------------------------------------------------------------------
Write-Host "Start to copy Sysprep files to VM ..."  -foregroundcolor Yellow
Write-Host "Copy sysprep file to $scrVMIpAddress ..." 
.\Copy-File.ps1 "$sysprepPathOnHost\CopyFile" $scrVMIpAddress "$sysDriveInVM\CopyFile" $userInVM $pwdInVM
.\Copy-File.ps1 $sysprepFilePathOnHost $scrVMIpAddress "$sysDriveInVM\SysprepAnswerFile" $userInVM $pwdInVM
#if($OSVersion -eq "W2K3" -or $OSVersion -eq "WXP")
#{
#    .\Copy-File.ps1 $ipAnswerFilePath $scrVMIpAddress "$sysDriveInVM\SysprepAnswerFile" $userInVM $pwdInVM
#}
Write-Host "Waiting for VM starting up ..." 
.\WaitFor-ComputerReady.ps1 $scrVMIpAddress $userInVM $pwdInVM 

Write-Host "Copy files from $cdRomDriveInVM to $scrVMIpAddress , SECTION=CopyFiles..." -foregroundcolor Yellow
.\RemoteExecute-Command.ps1 $scrVMIpAddress "$sysprepCopyFileScript $cdRomDriveInVM\Sysprep $sysDriveInVM\SysprepTemp $sysDriveInVM\SysprepTemp\$OSVersion\$CPUArchitecture $sysDriveInVM" $userInVM $pwdInVM  
#----------------------------------------------------------------------------
# Run Sysprep Command to rebuild VM
#----------------------------------------------------------------------------
Write-Host "Start to rebuild VM..."  -foregroundcolor Yellow

Write-Host "Waiting for VM starting up ..." 
.\WaitFor-ComputerReady.ps1 $scrVMIpAddress $userInVM $pwdInVM 
sleep 60

Write-Host "ReInstall System  on $scrVMIpAddress , SECTION=Factory-Mode..." -foregroundcolor Yellow
.\RemoteExecute-Command.ps1 $scrVMIpAddress "$sysprepFactoryScript $sysprepPathInVM $sysDriveInVM $ipV4Address $ipV4GateWay $ipV4DNS $ipV6Address $ipV6GateWay $ipV6DNS" $userInVM $pwdInVM  

Write-Host "Sleep 5 minutes ,wait for Sysprep Factory  mode executed" -foregroundcolor Yellow
Sleep 300
Write-Host "Waiting for Factory-Mode  executed finished..." -foregroundcolor Yellow
.\WaitFor-ComputerReady.ps1 $newVMIpAddress $userInVM $pwdInVM 

#----------------------------------------------------------------------------
# Run Sysprep Command to rebuild VM (Release Mode)
#----------------------------------------------------------------------------
$sysDriveInVM         = .\Get-RemoteSystemDrive.ps1 $newVMIpAddress $userInVM $pwdInVM
$sysprepCleanUpScript = $sysDriveInVM + "\SysprepCleanUp.bat"
$sysprepFactoryScript = $sysDriveInVM + "\SysprepWipe.bat"
$sysprepAuditScript   = $sysDriveInVM + "\SysprepConfigMachine.bat $OSVersion $sysDriveInVM $ipV4Address $ipV4GateWay $ipV4DNS $ipV6Address $ipV6GateWay $ipV6DNS"
if($OSVersion -eq "W2K3" -or $OSVersion -eq "WXP")
{
    $sysprepFactoryScript = $sysDriveInVM + "\SysprepFile\$OSVersion\SysprepFile\SysprepWipe.bat"
    $sysprepAuditScript = "$sysDriveInVM\sysprep\sysprep.exe /reseal /activated /quiet /reboot"
}
Write-Host "Initialize System on $newVMIpAddress, SECTION=Reseal-Mode..." -foregroundcolor Yellow
.\RemoteExecute-Command.ps1 $newVMIpAddress "$sysprepAuditScript" $userInVM $pwdInVM
Write-Host "Sleep 5 minutes ,wait for Sysprep reseal mode executed" -foregroundcolor Yellow
Sleep 300
Write-Host "Waiting for Rebuild Finished ..." -foregroundcolor Yellow
.\WaitFor-ComputerReady.ps1 $newVMIpAddress $userInVM $pwdInVM
#----------------------------------------------------------------------------
# Clean up  sysprep files  after rebuild VM finished
#----------------------------------------------------------------------------
Write-Host "Clean up sysprep files on VM: $newVMIpAddress , SECTION=Cleanup-Sysprepfiles..." 
.\RemoteExecute-Command.ps1 $newVMIpAddress "$sysprepCleanUpScript $OSVersion $sysDriveInVM $ipV4Address $ipV4GateWay $ipV4DNS $ipV6Address $ipV6GateWay $ipV6DNS" $userInVM $pwdInVM   

#----------------------------------------------------------------------------
# Unmount Syspre.iso in VM
#----------------------------------------------------------------------------
Write-Host "Unmount $SyspreIsoName in VM ..."
.\Remove-VMISO.ps1 $newVMName

Write-Host "$newVMName build completed." -foregroundcolor Green
exit 0
