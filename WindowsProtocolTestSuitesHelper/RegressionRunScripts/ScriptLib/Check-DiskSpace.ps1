#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Check-DiskSpace.ps1
## Purpose:        Check if there is enough free disk space for test.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 8
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
[string]$workingDir = "D:\PCTLabTest",
[double]$expectedDiskSpaceInGB  = "60",
[string]$logPath = "D:\PCTLabTest\TestResults"
)

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
if((Test-Path -Path $logPath) -eq $false)
{
	md $logPath
}
Start-Transcript -Path "$logPath\Check-DiskSpace.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Check Disk Space
#----------------------------------------------------------------------------
$driveLetter = $workingDir.Substring(0,2)
$volume = Get-WmiObject -Class Win32_Volume | where {$_.DriveLetter -eq $driveLetter}
[double]$freeSizeInGB = $volume.FreeSpace / 1GB
Write-Host "Current free disk space is $freeSizeInGB GB"
Write-Host "Expected free disk space is $expectedDiskSpaceInGB GB"
[int]$exitCode = 0
if($freeSizeInGB -lt $expectedDiskSpaceInGB)
{
    $exitCode = 1
    Write-Host "Current free disk space in $driveLetter is less than expected size, please get more free space for test."
}
else
{
    Write-Host "Current free disk space in $driveLetter is greater than expected size, it is good enough for test."
}

#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
Stop-Transcript
exit $exitCode