###############################################################################
##
## Microsoft Windows Powershell Sripting
## File:            Initiate-SuperMachine.ps1
## Purpose:         Initiate Super Machine, including configure hpyer-v setting
##                  and configure Schedule-TestJob.config file. Pleae make sure
##                  the Super Machine has been cleaned up before running this
##                  Script
## Version:         1.0 (12 Aug, 2011)
## Requirements:    Windows Powershell 2.0
## Supported OS:    Windows
##
###############################################################################

param (
[string]$workingDrive,
[string]$srcScriptLibPath,
[string]$srcVMPath
)

#------------------------------------------------------------------------------
# Verify required parameters
#------------------------------------------------------------------------------
if ($workingDrive -eq $null -or $workingDrive -eq "")
{
    Throw "Parameter workingDrive is required."
}

if ($srcScriptLibPath -eq $null -or $srcScriptLibPath -eq "")
{
    Throw "Parameter srcScriptLibPath is required."
}

if ($srcVMPath -eq $null -or $srcVMPath -eq "")
{
    Throw "Parameter srcVMPath is required."
}

#------------------------------------------------------------------------------
#
# Change hpyer-v setting: change VM default path, change VHD default path
#
#------------------------------------------------------------------------------
$path = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Virtualization"
$vmName = "DefaultExternalDataRoot"
$vhdName = "DefaultVirtualHardDiskPath"
$value = "$workingDrive\PCTLabTest\VM\"

Set-ItemProperty -path $path -name $vmName -value $value
Set-ItemProperty -path $path -name $vhdName -value $value

#------------------------------------------------------------------------------
#
# Robocopy all VHDs from lab server
#
#------------------------------------------------------------------------------
Robocopy $srcVMPath $workingDrive\PCTLabTest\VMLib *.vhd /s /purge

#------------------------------------------------------------------------------
#
# Rename and Initiate Schedule-Testjob.config
#
#------------------------------------------------------------------------------
Write-Host "Copying Schedule-TestJob.config from central server ..." -ForegroundColor Yellow
Copy-Item -Path "$srcScriptLibPath\Template\Template_Schedule-TestJob.config" `
-Destination "$workingDrive\PCTLabTest\ScriptLib\Schedule-TestJob.config" -Force

# get file attribute
$fileItem = Get-ItemProperty $workingDrive\PCTLabTest\ScriptLib\Schedule-TestJob.config
if ($fileItem.attributes.ToString() -match "ReadOnly")
{
	# Remove readonly attribute
	Set-ItemProperty -Path $fileItem -Name IsReadonly -Value $false
}

Write-Host "Copy Schedule-TestJob.config from central server completed." -ForegroundColor Green
Write-Host
Write-Host "Initiating Schedule-TestJob.config based on current physical machine ..." -ForegroundColor Yellow
Write-Host "Retrieving hard disk free space ..." -ForegroundColor Yellow

$driveData = Get-WmiObject -Class Win32_LogicalDisk -ComputerName "." -Filter "Name = '$workingDrive'"
[int]$freeSpace = [int]($driveData.FreeSpace/1GB)
Write-Host "Free Space of the hard disk $workingDrive is $freeSpace GB" -ForegroundColor Green
Write-Host
Write-Host "Retrieving free memory ..." -ForegroundColor Yellow

$memoryData = Get-WmiObject -Class Win32_OperatingSystem -ComputerName "."

# Convert free memory resource to GB
# FreephysicalMemory field saves the memory value in KB, so use 1MB as the dividend to convert free memory
# resource to GB
[int]$freeMemory = [int]($memoryData.FreePhysicalMemory/1MB)

Write-Host "Free memroy is $freeMemory GB" -ForegroundColor Green
Write-Host
Write-Host "Updating Schedule-TestJob.config ..." -ForegroundColor Yellow
$resconfig = "$workingDrive\PCTLabTest\ScriptLib\Schedule-TestJob.config"
[xml]$res = get-content $resconfig

$res.Resource.Max.Memory = $freeMemory.ToString()
$res.Resource.Available.Memory = $freeMemory.ToString()
$res.Resource.Max.Harddisk = $freeSpace.ToString()
$res.Resource.Available.Harddisk = $freeSpace.ToString()
$res.Save($resconfig)
Write-Host "Initiate Schedule-TestJob.config completed." -ForegroundColor Green