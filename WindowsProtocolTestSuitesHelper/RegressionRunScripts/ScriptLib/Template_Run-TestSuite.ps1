#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Template_Run-TestSuite.ps1
## Purpose:        Run [***PROTOCOL***] test suite on client VM in lab automation.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

Param(
[String]$testSuitesPath,
[String]$logPath
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
$logFile = $logPath+"\Run-TestSuite.ps1.log"
Start-Transcript $logFile -force

Write-Host "EXECUTING [Run-TestSuite.ps1] ..." -foregroundcolor cyan
Write-Host "`$testSuitesPath = $testSuitesPath"
Write-Host "`$logFile        = $logFile"

Write-Host "Set $testSuitesPath as current dir."
pushd $testSuitesPath

Write-Host "Begin to run test suite..."

#----------------------------------------------------------------------------
# Starting running script
#----------------------------------------------------------------------------
[***DO YOUR STUFF HERE, IF HAS***]

$harness       = "$env:ProgramFiles\MSTest\mstest.exe" 
$configFile    = "/runconfig:$testSuitesPath\[***YOUR TESTRUNCONFIG***].testrunconfig"
$testContainer = "/testcontainer:$testSuitesPath\Testsuite\Bin\[***YOUR TEST DLL***].dll" 
$resultFile    = "/resultsfile:$logPath\[***PROTOCOL***].trx"
cmd /c  $harness $configFile $testContainer $resultFile

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
popd

Write-Host "Run Test Suite finished."
Write-Host "EXECUTE [Run-TestSuite.ps1] FINISHED (NOT VERIFIED)."
Stop-Transcript

exit 0
