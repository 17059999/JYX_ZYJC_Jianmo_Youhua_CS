#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Parse-TrxLog.ps1
## Purpose:        Parse and deal the test result log file(.trx) for rerun the testcases failed
## Version:        1.0 (28 Sep, 2008)
##
##############################################################################

param(
[string]$testResultLog,
[ref]$failedCount  = [ref]([ref]$init = 0),       # Optional 
[ref]$testCommand  = [ref]([ref]$command = ""),   # Optional 
[bool]$isDelFailed = $false                       # Optional
)
#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Parse- TrxLog.ps1] ..." -foregroundcolor cyan
Write-Host "`$testResultLog          = $testResultLog"
Write-Host "`$failedCount            = $failedCount"
Write-Host "`$testCommand            = $testCommand"
Write-Host "`$isDelFailed            = $isDelFailed"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script parse the test result log file (filename.trx),and return the path of the Parsed log file."
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "testResultLog(Required parameter) : The full path of the log file to be parsed."
    Write-host "failedCount(Optional parameter)   : The count of the failed test cases in the testResultLog,"
    Write-host "                this parameter is required to be [ref] parameter."
    Write-host "testCommand(Optional parameter)   : The testCommand for the failed test cases, used to rerun the failed test cases,"
    Write-host "                this parameter is required to be [ref] parameter." 
    Write-host "isDelFailed(Optional parameter)   : The flag to show wether delete the failed record from the original log file,"
    Write-host "                If this parameter is true ,the original log will be backup and the failed case record will be delete."     
    Write-host
    Write-host "Example: Parse- TrxLog.ps1 c:\test\testresult\MS-SMB2.trx [ref]failedCount [ref]testCommand $true"
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
if ($testResultLog -eq $null -or $testResultLog -eq "")
{
    Throw "Parameter testResultLog is required."
}

#----------------------------------------------------------------------------
#Starting to parse and deal the Test Result
#----------------------------------------------------------------------------
Write-Host "Parse Result of Log file: $testResultLog "
$resultFile = ".\ParsedTestResult.log"
"Test Completed: " >$resultFile
$allPassed = $True
$testCount = 0
$testfailedCount = 0
$testCommand.value = "" 

#backup the file before Delete the failed logs
if($isDelFailed)
{
    $backupFile ="$testResultLog.old"	
    New-Item -ItemType file  "$backupFile" -Force
    (Get-Content $testResultLog)| Set-Content $backupFile
}

[xml]$list = get-content $testResultLog	
#get the Result Node and retrive the test result, as string type    
$testResultList = $list.GetElementsByTagName("UnitTestResult")	
#Parse and deal the result file
for($nodeIndex = 0;$nodeIndex -lt $testResultList.count;)
{
    $testResult=$testResultList.Item($nodeIndex)
    $testCount ++
    $testcaseName = $testResult.GetAttribute("testName")
    $testOutcome = $testResult.GetAttribute("outcome")
    #if not passed ,remove the item from the result log file
    if ( $testOutcome -ne "Passed")
    {
        $allPassed = $False
        $testfailedCount ++       
        $testCommand.value += " /test:$testcaseName"
        if($isDelFailed)
        {
            $testResult.ParentNode.RemoveChild($testResult)
            $nodeIndex --
        }
    }
    "$testcaseName :  $testOutcome" >>$resultFile
    "$testCommand.value" >>$resultFile
    $nodeIndex ++
}
#if deleted the Failed cases ,Reset the content of the TestResult .trx file 
#if all test case failed , delete the testResultLog
if($isDelFailed)
{
    if ($allPassed -eq $True)
    {
        cmd /c del /f $backupFile
    }
    elseif($testResultList.Count -gt 0)
    {
        set-content $testResultLog $list.OuterXml
        (Get-Content $testResultLog)| Set-Content $testResultLog
    }
    else
    {
        cmd /c del /f $testResultLog
    }
}
$failedCount.value = $testfailedCount
$failedCount.value >>$resultFile	
if ($allPassed -eq $True)
{      
    Write-Host "All the" $testCount "tests passed." -foregroundcolor green
    "AllTestPassed" >>$resultFile
}
else
{
    Write-Host $testfailedCount "of" $testCount "test(s) failed." -foregroundcolor Red
}
# return the $testCommand as part of return Value
return $testCommand.value

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Parse- TrxLog.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit