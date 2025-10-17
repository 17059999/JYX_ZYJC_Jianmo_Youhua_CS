#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Run-TestCase.ps1
## Purpose:        Run test cases
## Version:        1.1 (30 Apr, 2009)
##
##############################################################################

param(
[string]$protocolName,
[int]$rerunTimes,
[string]$rerunTypes,

[string]$testListName,
[string]$rootListName,

[string]$refVSmdi,
[string]$configFile,

[string]$MSTestPath,
[string]$ScriptsPath,
[string]$logPath,

[string]$rerunCleanScript
)
#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script configures a computer to logon automatically by specified credential."
    Write-host
    Write-host "Example:  .\Run-TestCase.ps1 `"MS-IMSA`" 1"
    Write-host "        or .\Run-TestCase.ps1 `"MS-IMSA`" 1 `"Failed,NotExecuted`" `"UnderTest`" `"Lists of Tests`" `"$env:SYSTEMDRIVE\Test\Data\MS-IMSA.vsmdi`" `"$env:SYSTEMDRIVE\Test\bin\LocalTestRun.testrunconfig`" `"$env:ProgramFiles\MSTest\mstest.exe`" `"c:\test\Scripts`" `"c:\test\TestResults`""
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
if ($protocolName -eq $null -or $protocolName -eq "")
{
    Throw "Parameter protocolName is required."
    return
}
if ($rerunTypes -eq $null -or $rerunTypes -eq "")
{
    $rerunTypes = "Failed,NotExecuted,Inconclusive"
}

if ($testListName -eq $null -or $testListName -eq "")
{
    $testListName = "underTest"
}
if ($rootListName -eq $null -or $rootListName -eq "")
{
    $rootListName = "Lists of Tests"
}

if ($refVSmdi -eq $null -or $refVSmdi -eq "")
{
    $refVSmdi = "$env:SYSTEMDRIVE\Test\Data\$protocolName"+".vsmdi"
}
if ($configFile -eq $null -or $configFile -eq "")
{
    $configFile = "$env:SYSTEMDRIVE\Test\bin\LocalTestRun.testrunconfig"
}

if ($MSTestPath -eq $null -or $MSTestPath -eq "")
{
    $MSTestPath = "$env:SYSTEMDRIVE\Program Files\MSTest\mstest.exe"
    $pathExist  = Test-Path $MSTestPath
    if( $pathExist -eq $false)
    {
        $MSTestPath = "$env:SYSTEMDRIVE\Program Files (x86)\MSTest\mstest.exe"
    }
    $pathExist  = Test-Path $MSTestPath
    if( $pathExist -eq $false)
    {
        Throw "MSTest is not Installed!"
        return
    }
}
if ($ScriptsPath -eq $null -or $ScriptsPath -eq "")
{
    $ScriptsPath = "$env:SYSTEMDRIVE\Test\Scripts"
}
if ($logPath -eq $null -or $logPath -eq "")
{
    $logPath = "$env:SYSTEMDRIVE\Test\TestResults"
}

if ($rerunCleanScript -eq $null -or $rerunCleanScript -eq "")
{
    $rerunCleanScript = ""
}
#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Run-TestCase.ps1]." -foregroundcolor cyan
Write-Host "`$protocolName  = $protocolName"
Write-Host "`$rerunTimes    = $rerunTimes"
Write-Host "`$rerunTypes    = $rerunTypes"

Write-Host "`$refVSmdi      = $refVSmdi"
Write-Host "`$configFile    = $configFile"
Write-Host "`$testListName  = $testListName"
Write-Host "`$rootListName  = $rootListName"

Write-Host "`$MSTestPath    = $MSTestPath"
Write-Host "`$ScriptsPath   = $ScriptsPath"
Write-Host "`$logPath       = $logPath"
Write-Host "`$rerunCleanScript= $rerunCleanScript"

#----------------------------------------------------------------------------
# Run cases ...
#----------------------------------------------------------------------------

$runCommand  = "`"$MSTestPath`" /runconfig:$configFile /testmetadata:"

Write-Host "call mstest to run cases ..." -foregroundcolor yellow
$resultsFile = "$logPath\$protocolName" +".trx"
cmd /c "$runCommand$refVSmdi /testlist:$testListName /resultsfile:$resultsFile" 2>&1 | Write-Host

[int]$runTimes =1

while($rerunTimes -gt 0){
    $rerunVSmdi    = "$logPath\$protocolName"+"-rerun$runTimes"+".vsmdi"
    $rerunListName = "rerun$runTimes"
    $failedCases   = &"$ScriptsPath\Generate-VSmdiFromTrx.ps1" $rerunTypes $rerunListName $rerunVSmdi $resultsFile $refVSmdi $testListName $rootListName
    
    if($failedCases -gt 0){
        $signalPath = "$logPath\$protocolName-rerun$runTimes.signal"
        $signalFlag = Test-Path $signalPath
        if( ($rerunCleanScript -ne "") -and ( $signalFlag -eq $false ) )
        {
           "$rerunListName" > $signalPath
           cmd /c "powershell `"$rerunCleanScript`"" 2>&1 | Write-Host
        }

        Write-Host "call mstest to rerun cases(rerun times: $runTimes) ..." -foregroundcolor yellow
        $resultsFile = "$logPath\$protocolName-rerun$runTimes.trx"
        cmd /c "$runCommand$rerunVSmdi /testlist:$rerunListName /resultsfile:$resultsFile" 2>&1 | Write-Host
    }
    else
    {
        break
    }
    $rerunTimes --
    $runTimes ++
}
Write-Host "Total run times: $runTimes"

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Run-TestCase.ps1] SUCCEED." -foregroundcolor green

exit
