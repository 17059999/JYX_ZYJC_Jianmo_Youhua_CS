#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Execute-TestSuite.ps1
## Purpose:        Executes the test suite on Hyper-V host machine. This is the entry script invoked by WTT Job.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[string]$protocolName, 
[string]$cluster, 

[string]$srcVMPath        = "\\nmtest\PETLabStore\VMLib",
[string]$srcScriptLibPath = "\\nmtest\PETLabStore\ScriptLib",
[string]$srcToolPath      = "\\nmtest\PETLabStore\Tools",
[string]$srcTestSuitePath = "\\nmtest\PETLabStore\TestSuite",
[string]$ISOPath          = "\\pt3controller\ISO",

#The following is for WTT Mix parameters
[string]$clientOS              = "Vista",
[string]$serverOS              = "W2K8",
[string]$workgroupDomain       = "Workgroup",   #"Domain"
[string]$IPVersion             = "IPv4",        #"IPv6"
[string]$ClientCPUArchitecture = "x86",         #"x64"
[string]$ServerCPUArchitecture = "x86",         #"x64"
#End of WTT Mix parameters

[string]$workingDir       = "D:\PCTLabTest", 
[string]$VMDir            = "D:\VM",
[string]$testResultDir    = "D:\PCTLabTest\TestResults",

[string]$userNameInVM     = "administrator",
[string]$userPwdInVM      = "Password01!",
[string]$domainInVM       = "contoso.com", 
[string]$testDirInVM      = "C:\Test",

#For SQL/Exchange productline
[string]$CustomizeScenario= ""
)


#----------------------------------------------------------------------------
# Start a clear new log.
#----------------------------------------------------------------------------
Write-Host "Stop previous transcript if it is not stopped in previous running."
Trap [Exception]
{
    Stop-Transcript
    continue
}
#Stop-Transcript
# Start Transcript. Please make sure the folder exists before this.
if(!(Test-Path $testResultDir))
{
    New-Item $testResultDir -ItemType directory
}
Start-Transcript $testResultDir\ExecuteTestSuite.log -force

#----------------------------------------------------------------------------
# Change the working directory.
#----------------------------------------------------------------------------
Write-Host "Set $workingDir\ScriptLib as current dir."
Push-Location $workingDir\ScriptLib

#----------------------------------------------------------------------------
# Check overriden value for parameters from test machine's [WTTBin] folder
#----------------------------------------------------------------------------
function Check-OverridenParam([string]$paramName, [string]$defaultValue)
{ 
    $newValue = .\Get-OverriddenLabParam.ps1 $paramName
    if ($newValue -ne "")
    {
        Write-Host "Replace $paramName with $newValue ..." 
        return $newValue
    }
    else
    {
        return $defaultValue
    }
}

$srcVMPath        = Check-OverridenParam "VMBasePath"    $srcVMPath
$srcScriptLibPath = Check-OverridenParam "ScriptLibPath" $srcScriptLibPath
$srcToolPath      = Check-OverridenParam "ToolsPath"     $srcToolPath
$srcTestSuitePath = Check-OverridenParam "TestSuitePath" $srcTestSuitePath
$ISOPath          = Check-OverridenParam "ISOPath"       $ISOPath

#----------------------------------------------------------------------------
# Copy tool used for PET Testing, based on parameters (maybe overriden)
#----------------------------------------------------------------------------
Write-Host "Copy PET Tools ..." -foregroundcolor cyan
robocopy /MIR /NFL /NDL $srcToolPath $workingDir\Tools 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Copy reportingtool.exe from \PETLabStore\Tools\ResultGatherer\Cx, according to the $cluster
#----------------------------------------------------------------------------
Write-Host "Copy $cluster ReportingTool.exe ..." -foregroundcolor cyan
xcopy $srcToolPath\ResultGatherer\$cluster\*.* $workingDir\Tools\ResultGatherer /s /y  2>&1 | Write-Host
 

#----------------------------------------------------------------------------
# Not used here now: Copy TestSuite binaries used for PET Testing, based on parameters (maybe overriden)
#
# WTT task "Copy Protocol's folder - Robocopy" will Copy binaries used for PET Testing based on parameters instead.
# Command used in this WTT task: robocopy [WinTestSuitePath]\[ProtocolName]\[Endpoint] [WorkingDir]\[ProtocolName] /MIR
#        parameter [Endpoint] will specify which endpoint's test will be run, "Server" or "Client"
#----------------------------------------------------------------------------
#Write-Host "Copy PET TestSuite Binaries ..." -foregroundcolor cyan
#robocopy /MIR /NFL /NDL $srcTestSuitePath\$protocolName $workingDir\$protocolName 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Execute-TestSuite.ps1] ..." -foregroundcolor cyan
Write-Host "`$protocolName          = $protocolName"
Write-Host "`$cluster               = $cluster"

Write-Host "`$srcVMPath             = $srcVMPath"
Write-Host "`$srcScriptLibPath      = $srcScriptLibPath"
Write-Host "`$srcToolPath           = $srcToolPath"
Write-Host "`$srcTestSuitePath      = $srcTestSuitePath"
Write-Host "`$ISOPath               = $ISOPath"

Write-Host "`$clientOS              = $clientOS"
Write-Host "`$serverOS              = $serverOS"
Write-Host "`$workgroupDomain       = $workgroupDomain"
Write-Host "`$IPVersion             = $IPVersion"
Write-Host "`$ClientCPUArchitecture = $ClientCPUArchitecture"
Write-Host "`$ServerCPUArchitecture = $ServerCPUArchitecture"

Write-Host "`$workingDir            = $workingDir"
Write-Host "`$VMDir                 = $VMDir"
Write-Host "`$testResultDir         = $testResultDir"

Write-Host "`$userNameInVM          = $userNameInVM"
Write-Host "`$userPwdInVM           = $userPwdInVM"
Write-Host "`$domainInVM            = $domainInVM"
Write-Host "`$testDirInVM           = $testDirInVM"

Write-Host "`$CustomizeScenario     = $CustomizeScenario"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script executes the test suite on Hyper-V host machine."
    Write-host
    Write-host "Example: Execute-TestSuite.ps1 MS-SMB2 C6 \\pt3controller\VMLib \\pt3controller\ScriptLib \\pt3controller\Tools \\pt3controller\TestSuite \\pt3controller\ISO Vista W2K8 Workgroup IPv4 x86 x86 d:\PCTLabTest d:\VM d:\PCTLabTest\TestResult Administrator Password01! contoso.com C:\Test"
    Write-host
    Write-host "Note: Only protocolName and cluster are required. Others are optional."
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
}
$srcTestSuitePath = "$srcTestSuitePath\$protocolName"

#----------------------------------------------------------------------------
# Cache the domain, username and password, so that other functions can use it if not provided by their caller.
#----------------------------------------------------------------------------
if ($workgroupDomain -eq "Domain")
{
    $global:domain = $domainInVM
}
else
{
    $global:domain = $null
}
$global:usr = $userNameInVM
$global:pwd = $userPwdInVM

#----------------------------------------------------------------------------
# Cache the test result dir
#----------------------------------------------------------------------------
$global:testResultDir = $testResultDir

#----------------------------------------------------------------------------
# Clean up. (Remove previously VMs)
#----------------------------------------------------------------------------
.\CleanUp-TestEnvironment.ps1 localhost

#----------------------------------------------------------------------------
# Make a copy of etc\hosts file from VM_Base.
#----------------------------------------------------------------------------
Copy-Item $srcVMPath\hosts $env:windir\system32\drivers\etc\hosts -force


#----------------------------------------------------------------------------
# Execute protocol entry script.
#----------------------------------------------------------------------------
Write-Host "Execute protocol entry script ..." -foregroundcolor Yellow
Write-Host "debug $CustomizeScenario" -foregroundcolor Yellow

if($CustomizeScenario -eq "")
{
    & ..\$protocolName\Scripts\Execute-ProtocolTest.ps1 `
    $protocolName $cluster `
    $srcVMPath $srcScriptLibPath $srcToolPath $srcTestSuitePath $ISOPath  `
    $clientOS $serverOS $workgroupDomain $IPVersion $ClientCPUArchitecture $ServerCPUArchitecture `
    $workingDir $VMDir $testResultDir `
    $userNameInVM $userPwdInVM $domainInVM $testDirInVM
	
}
elseif($CustomizeScenario.Contains("Plugfest"))
{
	
	& ..\$protocolName\Scripts\Execute-ProtocolTest-Plugfest.ps1 `
    $protocolName $cluster `
    $srcVMPath $srcScriptLibPath $srcToolPath $srcTestSuitePath $ISOPath  `
    $clientOS $serverOS $workgroupDomain $IPVersion $ClientCPUArchitecture $ServerCPUArchitecture `
    $workingDir $VMDir $testResultDir `
    $userNameInVM $userPwdInVM $domainInVM $testDirInVM $CustomizeScenario
}
else
{
    & ..\$protocolName\Scripts\Execute-ProtocolTest.ps1 `
    $protocolName $cluster `
    $srcVMPath $srcScriptLibPath $srcToolPath $srcTestSuitePath $ISOPath  `
    $clientOS $serverOS $workgroupDomain $IPVersion $ClientCPUArchitecture $ServerCPUArchitecture `
    $workingDir $VMDir $testResultDir `
    $userNameInVM $userPwdInVM $domainInVM $testDirInVM $CustomizeScenario
}



Pop-Location
#Stop-Transcript
Trap [Exception]
{
    Stop-Transcript
    continue
}
#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------

Write-Host "EXECUTE [Execute-TestSuite.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit 0
