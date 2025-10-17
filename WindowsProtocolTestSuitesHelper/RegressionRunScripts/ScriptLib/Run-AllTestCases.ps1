#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Run-AllTestCases.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 7 / Windows 2008 R2
##
##############################################################################

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "Get the scripts path from MSIInstalled.signal file"
$signalFile  = "$env:HOMEDRIVE\MSIInstalled.signal"
if (Test-Path -Path $signalFile)
{
    $TestSuiteScriptsFullPath = Get-Content $signalFile
}
else
{
    Write-Host "MSI has not been installed. please check"
    exit 0
}

$BatchFolder = $TestSuiteScriptsFullPath + "\..\Batch"
$testResultsPath= "$env:HOMEDRIVE\temp\TestResults"
$scriptsPath = "$env:HOMEDRIVE\temp"
$logFile = $scriptsPath + "\Run-AllTestCases.ps1.log"
if (!(Test-Path -Path $logFile))
{
    New-Item -Type File -Path $logFile -Force
}
Start-Transcript $logFile -Append

Write-Host "EXECUTING [Run-AllTestCases.ps1]..." -foregroundcolor cyan

Write-Host "Run All Test cases"

Write-Host "Locate to $BatchFolder "
pushd $BatchFolder
cmd /c "$BatchFolder\RunAllTestCases.cmd"

cmd /c ECHO testfinished >$env:HOMEDRIVE\test.finished.signal

exit 0

