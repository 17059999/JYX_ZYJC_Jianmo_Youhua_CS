##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################
###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           PushDataToAnalyzer.ps1
## Purpose:        Send data to result analyzer
## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later.
## Input parameter:
##      requestUrl          :  Result analyzer url
##      token               :  The Token for send post request to Analyze serivces
##      TestSuiteName       :  Test Suite Name
##      Branch              :  The branch of the triggering repo the build was queued for
##      DefinitionName      :  The name of the build pipeline.	
##      BuildNumber         :  The name of the completed build, also known as the run number
##      TriggeredBy         :  The person who triggered the job.
##      WorkingFolder       :  Regression reuslt and log path
##      ResultStorageAccount:  Azure Storage Account Name, this account is used to save Testsuite logs
###########################################################################################
Param
(
    # Result analyzer url
    [string]$requestUrl = "https://resultanalyzer.azurewebsites.net",
    [string]$token = "CustomToken",
    [Parameter(Mandatory=$true)]
    [string]$TestSuiteName = "RDPServer",    
    [Parameter(Mandatory=$true)]
    [string]$Branch,
    [Parameter(Mandatory=$true)]
    [string]$DefinitionName,
    [Parameter(Mandatory=$true)]
    [string]$BuildNumber,
    [Parameter(Mandatory=$true)]
    [string]$TriggeredBy,
    [Parameter(Mandatory=$true)]
    [string]$WorkingFolder,
    [Parameter(Mandatory=$true)]
    [string]$ResultStorageAccount
)
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
[string]$Global:StartTime = [System.dateTime]::UtcNow.ToString("MM/dd/yyyy HH:mm:ss")
if(Test-Path $WorkingFolder\StartTime.txt)
{
    [string]$Global:StartTime = Get-Content "$WorkingFolder\StartTime.txt" -Raw
}

$Title = $DefinitionName + '#' + $BuildNumber
Write-Host "TestSuite:                     $TestSuiteName"
Write-Host "WorkingFolder:                 $WorkingFolder"
Write-Host "Branch:                        $Branch"
Write-Host "TriggeredBy:                   $TriggeredBy"
Write-Host "Title:                         $Title"
Write-Host "ResultStorageAccount:          $ResultStorageAccount"

$pushJson = @{
    Title          = $Title;
    TestSuite      = $TestSuiteName;
    Branch         = $Branch;
    TriggeredBy    = $TriggeredBy;
    StartTime      = $Global:StartTime;    
    StorageAccount = $ResultStorageAccount;
}

#------------------------------------------------------------------------------------------
# Generate Report Data from json file
#------------------------------------------------------------------------------------------
function GenerateReport {
    Get-ChildItem $WorkingFolder -Filter "*.json" | ForEach-Object {
        $jsonInfo = (Get-Content $_.fullname | ConvertFrom-Json)
        $pushJson.TestSuite     = $jsonInfo.testSuiteName;
        $pushJson.OSVersion     = $jsonInfo.platform
        $pushJson.Environment   = $jsonInfo.envType 
        $pushJson.ContainerName = $jsonInfo.containerName
        $pushJson.EndTime       = $jsonInfo.endTime
    }
    GenerateTrxList
}

#------------------------------------------------------------------------------------------
# Generate trx list from Regression result path
#------------------------------------------------------------------------------------------
function GenerateTrxList {
    $trxFileNames = New-Object System.Collections.ArrayList
    Get-ChildItem $WorkingFolder -Filter "*.trx" | `
        Foreach-Object {
        $trxFileNames.Add($_.Name)
    }

    $pushJson.trxFileNames = $trxFileNames;
}

#------------------------------------------------------------------------------------------
# Send Post Request to Analyze serivces
#------------------------------------------------------------------------------------------
function PushDataAnalyzeFile {
    $request_url = "$requestUrl/api/report";
    Write-Host "request url:         $request_url"    

    $request_body = ConvertTo-Json -InputObject $pushJson;
    Write-Host "request body:         $request_body"
    
    $headers = @{CustomToken = $token}
    $ret = (Invoke-RestMethod -Uri $request_url -Method POST -Body $request_body -ContentType "application/json" -Headers $headers -TimeoutSec 1200)    
    return $ret;
}

#------------------------------------------------------------------------------------------
# Main
#------------------------------------------------------------------------------------------
function Main {
    GenerateReport

    try{
        $response = PushDataAnalyzeFile
        Write-Host "Publish to Analyzer: Success";
        Write-Host "Analyzer Report url: $requestUrl/report/$response"
    }catch {
        Write-Host "Publish to Analyzer: Fail";
        Write-Host "StatusCode:" $_.Exception.Response.StatusCode.value__ 
        Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription
    }
}

Main