###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Generate-RegressionInfo.ps1
## Purpose:        Generate json files with regression information
## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later.
## Input parameter is
##      logFilePath           :  Folder containing trx files and result logs
##      configFile            :  Environment xml full path
###########################################################################################

param(
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$logFilePath,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$configFile,
    [Parameter(ValueFromPipeline = $True)]
    [string]$EnvironmentName,
    [string]$VMHostName,
    [string]$blobContainerName,
    [string]$errorMessage = ""
)

Write-Host "logFilePath : $logFilePath"
Write-Host "configFile : $configFile"
Write-Host "EnvironmentName : $EnvironmentName"
Write-Host "VMHostName : $VMHostName"
Write-Host "blobContainerName : $blobContainerName"

[string]$platform = "12R2" # default platform
if ($EnvironmentName -match "_") {
    $platform = $EnvironmentName.Substring($EnvironmentName.LastIndexOf("_") + 1).Replace(".xml", "")   
}

[int] $all_totalNum = 0
[int] $all_failedNum = 0
[int] $all_passedNum = 0
[int] $all_inconclusiveNum = 0
[string] $status = "Not Run"
[string] $result = "Success"

Write-Host "Parse test result and create corresponding json files"

$trx = Get-ChildItem $logFilePath -Filter "*.trx"
if($trx.Count -gt 0 ) {
    Write-Host "$($trx.count) trx files found in total"
}
else {
    Write-Host "No trx files were found."
    $errorMessage = "No trx files were found."
}

$trx | Foreach-Object{
    Write-Host "Start parse trx :" + $_.FullName
    [xml]$resultContent = Get-Content $_.FullName    
    $resultCounters = $resultContent.TestRun.ResultSummary.Counters
    Write-Host "Start parse trx : total-$($resultCounters.total) | passed-$($resultCounters.passed) | failed-$($resultCounters.failed)" 
    $totalNum = $resultCounters.total
    $passedNum = $resultCounters.passed
    $failedNum = $resultCounters.failed
    $inconclusiveNum = $totalNum - $passedNum - $failedNum
    $status = "Completed"

    $all_totalNum += $totalNum
    $all_passedNum += $passedNum
    $all_failedNum += $failedNum
    $all_inconclusiveNum += $inconclusiveNum
}

$envType = ""
$machine = ""
$outFileName = ""
if($configFile -match "MS-SMBD"){
    $TestSuiteName = "MS-SMBD_DotNetCore"
    $envType = "Local"
    $machine = "SMBD-SUT,SMBD-Driver"
    $outFileName = "MS-SMBD_Local"
}
else{
    [xml]$configContent = Get-Content $configFile
    $outFileName = [System.IO.Path]::GetFileNameWithoutExtension($configFile)
    $TestSuiteName = $configContent.lab.core.TestSuiteName
    $envType = $configContent.lab.core.regressiontype
    $machineList = $configContent.lab.servers.vm
    $machineList | ForEach-Object {
        $machine += $_.name + ","
    }
    $machine = $machine.Substring(0,$machine.length-1)
}

if($VMHostName -ne "")
{
    $machine = $VMHostName
}

if(![string]::IsNullOrEmpty($errorMessage)){
    $result = "Failed"
}

$jsonContent = @"
    {
        "testSuiteName"   : "$TestSuiteName",
        "envType"         : "$envType",        
        "platform"        : "$platform",
        "jobUrl"          : "",
        "startTime"       : "$Global:StartTime",
        "endTime"         : "$([System.dateTime]::UtcNow.ToString("MM/dd/yyyy HH:mm:ss"))",
        "totalNum"        : "$all_totalNum",
        "passedNum"       : "$all_passedNum",
        "failedNum"       : "$all_failedNum",
        "inconclusiveNum" : "$all_inconclusiveNum",
        "status"          : "$status",
        "machine"         : "$machine",
        "resultLogPath"   : "$logFilePath",
        "containerName"   : "$blobContainerName",
        "result"          : "$result",
        "errorMessage"    : "$errorMessage"
    } 
"@
    $jsonPath = [string]::Format("{0}\{1}", "$logFilePath", "$outFileName.json")

    $jsonContent.Replace('\','\\') | Out-File $jsonPath -Encoding utf8
