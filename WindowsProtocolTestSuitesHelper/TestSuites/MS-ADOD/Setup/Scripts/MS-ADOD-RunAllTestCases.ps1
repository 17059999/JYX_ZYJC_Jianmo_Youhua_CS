#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

param(
[string]$protocolName          = "MS-ADOD"
)

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
$logPath = $env:SystemDrive + "Test\TestLog"
if ((Test-Path -Path $logPath) -eq $false)
{
    md $logPath
}
Start-Transcript -Path "$logPath\MS-ADOD-RunAllTestCases.ps1.log" -Append -Force

#-------------------------
# Execute Test Suite
#-------------------------
$endPointPath = "$env:SystemDrive\MicrosoftProtocolTests\MS-ADOD\OD-Endpoint"
$version = Get-ChildItem $endPointPath | where {$_.Attributes -eq "Directory" -and $_.Name -match "\d+\.\d+\.\d+\.\d+"} | Sort-Object Name -descending | Select-Object -first 1

$binDir = "$endPointPath\$version\bin"
# Get mstest
$tempDir = "$env:SystemDrive\Temp"
[string]$mstest = & "$tempDir\GetTestPath.ps1" mstest
$testDir = "$env:SystemDrive\Test"
if(!(Test-Path $testDir))
{
	md $testDir
}

$testResultDir = $testDir + "\TestResults"
if(Test-Path $testResultDir)
{
	remove-item -path $testResultDir -Filter "*.*" -Recurse
}
else
{
    md $testResultDir
}

cd $testDir


$finishSignalFile = "$testDir\test.finished.signal"
if(Test-Path $finishSignalFile)
{
	Remove-Item $finishSignalFile
}

$startSignalFile = "$testDir\test.started.signal"
echo "test.started.signal" > $startSignalFile

# Start to run test cases
& $mstest /testcontainer:"$binDir\MS-ADOD_ODTestSuite.dll" /runconfig:"$binDir\ODLocalTestRun.testrunconfig" /resultsfile:"$testResultDir\MS-ADOD_TestResults.trx"

echo "test.finished.signal" > $finishSignalFile

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")