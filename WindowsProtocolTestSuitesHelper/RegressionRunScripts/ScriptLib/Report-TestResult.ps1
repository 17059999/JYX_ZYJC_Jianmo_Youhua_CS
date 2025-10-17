#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Report-TestResult.ps1
## Purpose:        Copy test results to file server, 
##                 then call ProtocolTestReporting.exe to summarize test result and send mail.
## Version:        1.0 (19 Nov, 2008)
##
##############################################################################

param(
[string]$protocolName,
[string]$cluster,
[string]$contextName,
[string]$workingDir,
[string]$testResultDir,
[string]$testLogRepository,
[string]$reportTo, 
[string]$reportCC,
[string]$configFileName,
[string]$frequency = "Daily"
)

Set-StrictMode -v 2
#Stop-Transcript
#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Copy test results to file server, then call ProtocolTestReporting.exe to summarize test result and send mail."
    Write-host "Param1: Protocol Name.  (Required)"  
    Write-host "Param2: Cluster Number. (Required)"      
    Write-host "Param3: Context Name.   (Required)"
    Write-host "Param4: Working Directory. (Required)"
    Write-host "Param5: Src Directory of test logs. (Required)"
    Write-host "Param6: Dest Directory of test logs (File server path). (Required)"
    Write-host "Param7: Send Mail To.   (Required)"
    Write-host "Param8: Send Mail CC.   (Optional). Default value is the same as Send mail To."
    Write-host "Param9: Config File Name.   (Required)"
    Write-host "Param10: Schedule frequency. (Optional) Default value: 'Daily'. Valid value: 'Daily', 'Weekly', 'Monthly'"    
    Write-host
    Write-host "Example1: Report-TestResult.ps1 MS-SMB2 C7 Vista-W2K8-Workgroup-IPv6-x86-x86 D:\PCTLabTest D:\PCTLabTest\TestResults \\wseatc-protocol\Win7WeeklyLog sixiao sixiao,jihuang MS-SMB2-Weekly-Report.config.xml Weekly"
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
# Change the working directory.
#----------------------------------------------------------------------------
Write-Host "Set $workingDir\ScriptLib as current dir."
Push-Location $workingDir\ScriptLib

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($protocolName -eq $null -or $protocolName -eq "")
{
    Throw "Parameter `$protocolName is required."
}
if ($cluster -eq $null -or $cluster -eq "")
{
    Throw "Parameter `$cluster is required."
}
if ($contextName -eq $null -or $contextName -eq "")
{
    Throw "Parameter `$contextName is required."
}
if ($workingDir -eq $null -or $workingDir -eq "")
{
    Throw "Parameter `$workingDir is required."
}
if ($testResultDir -eq $null -or $testResultDir -eq "")
{
    Throw "Parameter `$testResultDir is required."
}
if ($testLogRepository -eq $null -or $testLogRepository -eq "")
{
    Throw "Parameter `$testLogRepository is required."
}
if ($reportTo -eq $null -or $reportTo -eq "")
{
    $reportTo = ""
}
if ($reportCC -eq $null -or $reportCC -eq "")
{
    $reportCC = $reportTo
}
if ($frequency -eq $null -or $frequency -eq "")
{
    $frequency = "Daily"
}
if ($configFileName -eq $null -or $configFileName -eq "")
{
    Throw "Parameter `$configFileName is required."
}
if (!($frequency.ToLower() -eq "daily" -or $frequency.ToLower() -eq "weekly" -or $frequency.ToLower() -eq "monthly"))
{
    Throw "Parameter `$frequency value is invalid."
}

#----------------------------------------------------------------------------
# Check overriden value for parameters from test machine's [WTTBin] folder
#----------------------------------------------------------------------------
function Check-OverridenParam([string]$paramName, [string]$defaultValue)
{ 
    $newValue = .\Get-OverriddenLabParam.ps1 $paramName
    if ($newValue -ne "")
    {
        Write-Host "Replace $variable with $newValue ..." 
        return $newValue
    }
    else
    {
        return $defaultValue
    }
}

$testLogRepository = Check-OverridenParam "TestLogRepository" $testLogRepository
$reportTo          = Check-OverridenParam "TestReportTo"      $reportTo
$reportCC          = Check-OverridenParam "TestReportCC"      $reportCC
$configFileName    = Check-OverridenParam "ConfigFileName"    $configFileName

#----------------------------------------------------------------------------
# Set working directory
#----------------------------------------------------------------------------
$toolDir    = "$workingDir\Tools\ResultGatherer"
$configDir  = "$toolDir\Config"

#----------------------------------------------------------------------------
# Set log folder
#----------------------------------------------------------------------------
$year = (Get-Date).Year
if ($frequency.ToLower() -eq "monthly")
{
    $month = (Get-Date).Month
    $formattedDate = "{0}-{1:00}" -f $year, $month # 2008-08
    
}
elseif ($frequency.ToLower() -eq "weekly")
{
    $gCalendar = New-Object "System.Globalization.GregorianCalendar"

    # set 6:00PM on Fridays as beginning of next week
    #[int] $weekOfTheYear = $gCalendar.GetWeekOfYear(($gCalendar.AddHours((Get-Date),54)), [System.Globalization.CalendarWeekRule]0, [System.DayOfWeek]1)
    # set Monday as the first day of week
    [int] $weekOfTheYear = $gCalendar.GetWeekOfYear((Get-Date), [System.Globalization.CalendarWeekRule]0, [System.DayOfWeek]1)


    $formattedDate = "{0}-Week{1:00}" -f $year, $weekOfTheYear # 2008-Week01
}
else # Daily Schedule
{
    $month = (Get-Date).Month
    $day   = (Get-Date).Day
    $formattedDate = "{0}-{1:00}-{2:00}" -f $year, $month, $day # 2008-01-01
}
$logPathOnFileServer = "$testLogRepository\$formattedDate\$ProtocolName\$ContextName"
if((Test-Path $logPathOnFileServer) -eq $false)
{
    New-Item $logPathOnFileServer -itemtype directory
}

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Start-Transcript $logPathOnFileServer\$protocolName.analysis.ps1.log -force
Write-Host "`$protocolName          = $protocolName"
Write-Host "`$cluster               = $cluster"
Write-Host "`$contextName           = $contextName"
Write-Host "`$reportTo              = $reportTo"
Write-Host "`$reportCC              = $reportCC"
Write-Host "`$workingDir            = $workingDir"
Write-Host "`$testResultDir         = $testResultDir"
Write-Host "`$testLogRepository     = $testLogRepository"
Write-Host "`$logpathOnFileServer   = $logpathOnFileServer"
Write-Host "`$configFileName        = $configFileName"
Write-Host "`$frequency             = $frequency"
Write-Host

#----------------------------------------------------------------------------
# Copy test result file to FileServer\LogRepository
#----------------------------------------------------------------------------
Write-Host "Copying test results to file server:"
Write-Host $logPathOnFileServer
robocopy /mir $testResultDir $logPathOnFileServer 

#----------------------------------------------------------------------------
# Call Requirement Coverage Analyzing Tool
#----------------------------------------------------------------------------
Write-Host "Sumarize test result and send mail ..."
cmd /c $toolDir\ProtocolTestReporting.exe "$configDir\$configFileName" /mt:$reportTo /mc:$reportCC /p:$protocolName /lp:$testLogRepository /sf:$frequency 2>&1 | Write-Host
Write-Host "Result Analyzed." -foregroundcolor Green

Stop-Transcript
popd
exit $lastExitCode
