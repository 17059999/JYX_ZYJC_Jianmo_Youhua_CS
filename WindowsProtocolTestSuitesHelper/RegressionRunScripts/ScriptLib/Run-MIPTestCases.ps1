#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Run-MIPTestCases.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 7 / Windows 2008 R2
##
##############################################################################
Param(
[string]$protocolName,
[string]$ClientCPUArchitecture,
[string]$RSPath,
[string]$RSinScope         = "Server+Both",
[string]$RSoutOfScope      = "Client",
[string]$BatchToRun         = "RunAllTestCases.cmd",
[string]$Filter         = "",
[string]$DriverOSType         = "Windows"
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "Get the scripts path from MSIInstalled.signal file"
$signalFile  = "$env:HOMEDRIVE/MSIInstalled.signal"
if (Test-Path -Path $signalFile)
{
    $TestSuiteScriptsFullPath = Get-Content $signalFile
}
else
{
    Write-Host "MSI has not been installed. please check"
    exit 0
}

$BatchFolder = $TestSuiteScriptsFullPath + "/../Batch"
$testResultsPath= $BatchFolder + "/TestResults"
$scriptsPath = $RSPath + "/../Scripts"
if (!(Test-Path -Path $testResultsPath) )
{
    New-Item -Type Directory -Path $testResultsPath -Force
}
$logFile = $testResultsPath + "/Run-MIPTestCases.ps1.log"
if (!(Test-Path -Path $logFile))
{
    New-Item -Type File -Path $logFile -Force
}
Start-Transcript $logFile -Append

Write-Host "EXECUTING [Run-MIPTestCases.ps1]..." -foregroundcolor cyan
Write-Host "`$protocolName       = $protocolName"
Write-Host "`$RSPath            = $RSPath"
Write-Host "`$RSinScope          = $RSinScope"
Write-Host "`$RSoutOfScope       = $RSoutOfScope"
Write-Host "`$BatchToRun          = $BatchToRun"
Write-Host "`$Filter            = $Filter"
Write-Host "`$DriverOSType       = $DriverOSType"

Write-Host "Run All Test cases"

Write-Host "Locate to $BatchFolder "
pushd $BatchFolder
&$BatchFolder/$BatchToRun $Filter

Write-Host "Start to run script: $scriptsPath/Generate-Report.ps1"
$programFolder = $env:ProgramFiles
if ($ClientCPUArchitecture -eq "x64")
{
    $programFolder = $programFolder + " (x86)"
}

if($DriverOSType -ne "Linux"){
    $reportingToolPath  = $programFolder + "/Protocol Test Framework/Bin"
    $resultGarthererPath = $RSPath + "/../Tools/ResultGatherer"
    robocopy $reportingToolPath $resultGarthererPath ReportingTool.exe
    powershell $scriptsPath\Generate-Report.ps1 $protocolName $RSPath $testResultsPath $RSinScope $RSoutOfScope
}

$signFileName = "test.finished.signal"
Write-Output "testfinished" > $env:HOMEDRIVE/$signFileName

exit 0

