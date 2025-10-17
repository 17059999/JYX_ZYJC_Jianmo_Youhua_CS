#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################
#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Generate-HTMLReport.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2012 +
## Purpose:        Summarize the environment setup and case run result to a HTML file
##
##############################################################################
Param (
[string]$OSBuildNumber = "9926",
[string]$protocolName = "FileSharing", 
[string]$testResultDir = "D:\WinteropProtocolTesting\TestResults\$protocolName"
)

$screenShotFolder = "$testResultDir\VMScreenshots"
#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath"

#----------------------------------------------------------------------------
# Define common variables
#----------------------------------------------------------------------------
$environmentValidateStatusFile = "$testResultDir\EnvironmentStatus.txt"

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Start-Transcript -Path "$testResultDir\Generate-HTMLReport.ps1.log" -Append -Force

Function GetEnvironmentStatus($computer,$feature,$status)
{
    $statusObj = New-Object PsObject -Property @{
        Computer = "$computer"
        Feature = "$feature"
        Status = "$status"
    }
    return $statusObj
}

Function GenerateEnvironmentStatusTable($inputFile)
{    
    $environmentStatus = @()   
    
    foreach ($line in [System.IO.File]::ReadLines($inputFile)) {

        $segments = $line.split("|")
        if($segments.length -gt 1)
        {
             $environmentStatus += GetEnvironmentStatus $segments[0] $segments[1] $segments[2] 
        }
    }
   
    return $environmentStatus
}

Function GenerateTestCaseRunTable($trxFolder)
{    
    $testResults = "<table>
    <colgroup><col/><col/><col/><col/><col/></colgroup>
    <tr><th>Test Result File(.trx)</th><th>Total</th><th>Passed</th><th>Failed</th></tr>"
    Get-ChildItem $trxFolder -Filter "*.trx" | `
    Foreach-Object{
        $trxFileName = [System.IO.Path]::GetFileName($_.FullName)
        [xml]$resultContent = get-content $_.FullName

        $resultSummary = $resultContent.TestRun.ResultSummary.Counters
        [int]$totalNum = $resultSummary.total
        [int]$passedNum = $resultSummary.passed
        [int]$failedNum = 0

        if($totalNum -gt 0 -and $passedNum -gt 0 -and $totalNum -eq $passedNum)
        {
            $hasFailure = $false;                        
        }
        else
        {        
            $failedNum = $totalNum - $passedNum
            $hasFailure = $true                    
        }

        $trxFile =  [System.IO.Path]::GetFileName( $_.FullName)        
        $testResults += "<tr><td> <a href=`".`"> $trxFile</td><td>$totalNum</td><td>$passedNum</td><td>$failedNum</td></tr>"
    }    
    $testResults += "</table>"

    return $testResults
}


Function GenerateImageTable($imageDir)
{    
    $screenShots = "<table>
    <colgroup><col/><col/><col/><col/><col/></colgroup>
    <tr><th>VMName</th><th>Screenshots</th></tr>"
    
    Get-ChildItem $imageDir -Filter "*.jpg" | `
    Foreach-Object{
        $vmName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)        
        $screenShots += "<tr><td> $vmName </td><td><a href=`"./VMScreenshots`">$_</a></td></tr>"
    }    
    $screenShots += "</table>"
    return $screenShots
}

Function GenerateVMSetupLogsTable($logDir)
{
    $vmSetupLogs = "<table>
    <colgroup><col/><col/><col/><col/><col/></colgroup>
    <tr><th>Category</th><th>Log Location</th></tr>"
    
    Get-ChildItem $logDir -File -Recurse | where {$_.Name -eq "controller.log" } |`
    Foreach-Object{        
        $VMName  = [System.IO.Path]::GetDirectoryName($_.FullName)
        $VMName  = $VMName.Substring($VMName.LastIndexOf("\")+1)
        $category = [string]::Format("{0} {1} {2}", "VM", $VMName, "setup log")
        $logFile =  "controller.log"
        $vmSetupLogs += "<tr><td> $category </td><td><a href=`"./$VMName/$logFile`">controller.log</a></td></tr>"        
    }   

    $category = "Execute Protocol Test Suite Log "
    $logFile = "./Execute-ProtocolTest.ps1.log"
    $vmSetupLogs += "<tr><td> $category </td><td><a href=`"./$logFile`">Execute-ProtocolTest.ps1.log</a></td></tr>"

    $vmSetupLogs += "</table>"
    return $vmSetupLogs
}

$head = @'
<script type="text/javascript">
function OpenFile(filePath) 
{
    var fso = new ActiveXObject("Scripting.FileSystemObject");
    //specify the local path to Open
    var file = fso.OpenTextFile(filePath, 1,2);        
}
</script>
<style>
h1 {font-family:Arial;
       font-size:25pt;
       color:black;
    }
h2 {font-family:Arial;
       font-size:15pt;
       color:black;
   }

body { background-color:#dddddd;
       font-family:Arial;
       font-size:12pt; }
td, th { border:1px solid black; 
         border-collapse:collapse; }
th { color:black;
     background-color:#73D0F5; }
table, tr, td, th { padding: 2px; margin: 0px }
table { margin-left:50px; }
</style>
'@

$bodyString = $null

#Convert the trx files result into HTML table
$environmentIsReady = $false
if(Test-Path -Path $testResultDir)
{
    $trxList = Get-ChildItem $testResultDir -Filter "*.trx"
    if($trxList -ne $null -and $trxList.Count -gt 0)
    {
        $environmentIsReady = $true
        $caseRunResultsReport = "<h2>Test Case Run Result</h2>"
        $caseRunResultsReport += GenerateTestCaseRunTable -trxFolder $testResultDir
        $bodyString += $caseRunResultsReport
    }
}

#Convert the environment validation status file into HTML table
if(Test-Path -Path $environmentValidateStatusFile)
{
    $environmentStatusReport = "<h2>Environment Health Status</h2>"
    $environmentStatusReport += "Validate the $protocolName Test Suite required feature status.<br> For those features are not running, related test cases will be excluded."    
    $environmentStatusLog = "$testResultDir\Validate-Environment.ps1.log"
    if(Test-Path -Path $environmentStatusLog)
    {
        $environmentStatusReport +=  "<br> For more information about the feature validation, please check the log: <a href=`"./Validate-Environment.ps1.log`">Validate-Environment.ps1.log</a>"
    }
    $environmentStatusReport += GenerateEnvironmentStatusTable -inputFile $environmentValidateStatusFile | Select-Object -Property Computer,Feature,Status | ConvertTo-HTML -Fragment 
    $bodyString += $environmentStatusReport
}

#Insert the screen shots into the HTML report if any
if((Test-Path -Path $screenShotFolder) -and $environmentIsReady -eq $false)
{
    $imageList = Get-ChildItem $screenShotFolder -Filter "*.jpg"
    if($imageList -ne $null -and $imageList.Count -gt 0 )
    {
        $imageReport = "Error(s) occured during test environment setup. VM Screenshots were taken for issue diagnosis and trouble shooting."
        $imageReport += GenerateImageTable -imageDir $screenShotFolder
        if(($environmentStatusReport -eq $null) -or ( $environmentStatusReport.Length -lt 1))
        {
            $bodyString += "<h2>Environment Health Status</h2>"
        }
        $bodyString += $imageReport
    }
}

#Generate a table to list the vm setup log files
$bodyString += "<h2>Test Environment Setup Logs</h2>"
$bodyString += GenerateVMSetupLogsTable -logDir $testResultDir

$htmlPage = [string]::Format("{0}\{1}{2}", $testResultDir, $protocolName,"TestSuiteResult.html")

ConvertTo-HTML -head $head -PostContent $bodyString -Body "<h1 align=`"center`">$protocolName Test Suite Run Result Summary Against Win Build $OSBuildNumber</h1>"  | Out-File $htmlPage