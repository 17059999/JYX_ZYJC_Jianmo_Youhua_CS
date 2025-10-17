#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

param(
[string]$ProtocolName          = "BranchCache",
[string]$WorkingDirOnHost      = "C:\WinteropProtocolTesting",
[string]$TestResultDirOnHost   = "C:\WinteropProtocolTesting\TestResults\BranchCache",
[string]$EnvironmentName       = "BranchCache.xml",
[string]$TestDirInVM           = "$env:SystemDrive\Test" 
)

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath"

#----------------------------------------------------------------------------
# Define call stack functions
#----------------------------------------------------------------------------
[int]$global:indentControl = 0
$global:callStackLogFile = "$TestResultDirOnHost\Execute-ProtocolTest.ps1.CallStack.log"
function global:EnterCallStack($scriptName)
{
	if($scriptName -ne "Execute-ProtocolTest.ps1")
	{
		$global:indentControl++
	}
	$tab = ""
	for ($i=1; $i -le $global:indentControl; $i++)
	{
		$tab += "    "
	}
	$date = get-date -Format "M/d/yyyy hh:mm:ss"
	("") | Out-File -FilePath $callStackLogFile -Append -Force
	($tab + "Start $scriptName    " + $date) | Out-File -FilePath $callStackLogFile -Append -Force
}

function global:ExitCallStack($scriptName)
{

	$tab = ""
	for ($i=1; $i -le $global:indentControl; $i++)
	{
		$tab += "    "
	}
	if($scriptName -ne "Execute-ProtocolTest.ps1")
	{
		$global:indentControl--
	}
	$date = get-date -Format "M/d/yyyy hh:mm:ss"
	($tab + "Exit $scriptName    " + $date) | Out-File -FilePath $callStackLogFile -Append -Force
}

if($function:EnterCallStack -ne $null)
{
	EnterCallStack "Execute-ProtocolTest.ps1"
}

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
Start-Transcript -Path "$TestResultDirOnHost\Execute-ProtocolTest.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Verify input parameters
#----------------------------------------------------------------------------
Write-Host "Verify input parameters..." -foregroundcolor Yellow
if ($ProtocolName -ne "BranchCache")
{
    Throw "$ProtocolName is not correct. Only BranchCache is allowed."
}
else
{
	Write-Host "Current test protocol is $ProtocolName" -foregroundcolor Green
}

#----------------------------------------------------------------------------
# Define source folders
#----------------------------------------------------------------------------
.\Write-Info.ps1 "Define source folders..." -foregroundcolor Yellow
$srcScriptLibPathOnHost = $WorkingDirOnHost + "\ScriptLib" 
$srcToolPathOnHost      = $WorkingDirOnHost + "\Tools" 
$srcScriptPathOnHost    = $WorkingDirOnHost + "\ProtocolTestSuite\$ProtocolName\Scripts"
$requirementSpecPath     = $WorkingDirOnHost + "\ProtocolTestSuite\$ProtocolName\Scripts"
$srcDeployPathOnHost    = $WorkingDirOnHost + "\ProtocolTestSuite\$ProtocolName\Deploy"
$srcTestSuitePathOnHost = $WorkingDirOnHost + "\ProtocolTestSuite\$ProtocolName\Bin"
$global:testResultDir   = $TestResultDirOnHost 
$paramConfigFile = $WorkingDirOnHost + "\ProtocolTestSuite\$protocolName\VSTORMLITEFiles\XML\$EnvironmentName"

if(!(Test-Path $TestResultDirOnHost))
{
	md $TestResultDirOnHost
}

#----------------------------------------------------------------------------
# The following parameters are special for BranchCache
#----------------------------------------------------------------------------
Push-Location $srcScriptLibPathOnHost
.\Write-Info.ps1 "Get VM configuration ..." -foregroundcolor Yellow

# Get Driver Infomation
[xml]$Content = Get-Content $paramConfigFile

$testCaseScript 			= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "TestCase"}).Value
$testCaseTimeout 			= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "TestCaseTimeout"}).Value
$global:userNameInVM    	= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "userName"}).Value
$global:userPwdInVM     	= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "password"}).Value
$global:domainInVM      	= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "domain"}).Value
$global:testDriverName    	= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "testDriverName"}).Value
$global:testDriverCtrlIp 	= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "testDriverCtrlIp"}).Value
$global:testDirInVM     	= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "testDirInVM"}).Value
$global:testServerCtrlIp 	= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "testServerCtrlIp"}).Value
$global:testDCCtrlIp 		= ($Content.lab.Parameters.Parameter | where {$_.Name -eq "testDCCtrlIp"}).Value

#.\WaitFor-ComputerReady.ps1 $testDriverCtrlIp "$DomainInVM\$userNameInVM" "$userPwdInVM"

If($testDirInVM -match "SYSTEMDRIVE")
{
$VmSystemDrive = .\Get-RemoteSystemDrive.ps1 $testDriverCtrlIp "$DomainInVM\$userNameInVM" "$userPwdInVM"
$TestDirInVM = $testDirInVM.Replace("SYSTEMDRIVE", $VmSystemDrive )
}

#----------------------------------------------------------------------------
# Copy test files.
#----------------------------------------------------------------------------
.\Write-Info.ps1 "Copy test files..." -foregroundcolor Yellow
.\Copy-TestFile.ps1 $srcScriptLibPathOnHost $srcScriptPathOnHost $srcToolPathOnHost $srcTestSuitePathOnHost $testDriverCtrlIp $testDirInVM "$DomainInVM\$userNameInVM" "$userPwdInVM"

#----------------------------------------------------------------------------
# Run test cases on test drivers
#----------------------------------------------------------------------------
#----------------------------------------------------------------------------
# Run test cases on test driver
#----------------------------------------------------------------------------
.\Write-Info.ps1 "Run test cases on test driver ..." -foregroundcolor Yellow

# Create task for execute protocol test suite
$taskName = "RunTestCases"
$task = "PowerShell $testDirInVM\Scripts\$testCaseScript"
$createTask = "CMD /C schtasks /Create /RU Administrators /SC ONCE /ST 00:00 /TN $taskName /TR `"$Task`" /IT /F"

.\Write-Info.ps1 "$createTask"
.\RemoteExecute-Command.ps1 $testDriverCtrlIp "$createTask" "$DomainInVM\$userNameInVM" "$userPwdInVM"

.\Write-Info.ps1 "Sleep 2 minutes to wait task ready"
Start-Sleep -s 120

# Execute the task to execute protocol test suite
$exeTask = "cmd /c schtasks /Run /TN $taskName"
# Execute task with limited retry times
for($i=0;$i -lt 5;$i++) 
{
    .\Write-Info.ps1 "$exeTask"
    .\RemoteExecute-Command.ps1 $testDriverCtrlIp "$exeTask" "$DomainInVM\$userNameInVM" "$userPwdInVM"    
    Start-Sleep 20
    .\Write-Info.ps1 "wait for test to start"
    .\WaitFor-ComputerReady.ps1 $testDriverCtrlIp "$DomainInVM\$userNameInVM" "$userPwdInVM" $testDirInVM "test.started.signal" 120
    if($lastexitcode -eq 0)
    {
        break
    }
}

.\Write-Info.ps1 "Wait for test to complete"
Start-sleep 120
.\WaitFor-ComputerReady.ps1 $testDriverCtrlIp "$DomainInVM\$userNameInVM" "$userPwdInVM" $testDirInVM "test.finished.signal" $testCaseTimeout


#----------------------------------------------------------------------------
# Copy results from VM 
#----------------------------------------------------------------------------
.\Write-Info.ps1 "Copy results from VM ..." -foregroundcolor Yellow
.\Copy-TestResult.ps1 $testDriverCtrlIp "$testDirInVM\TestResults\*.trx" "$TestResultDirOnHost" "$DomainInVM\$userNameInVM" "$userPwdInVM"
.\Copy-TestResult.ps1 $testDriverCtrlIp "$testDirInVM\TestResults\*.txt" "$TestResultDirOnHost" "$DomainInVM\$userNameInVM" "$userPwdInVM"
.\Copy-TestResult.ps1 $testDriverCtrlIp "$testDirInVM\TestResults\*.ptfconfig" "$TestResultDirOnHost" "$DomainInVM\$userNameInVM" "$userPwdInVM"
.\Copy-TestResult.ps1 $testDriverCtrlIp "$testDirInVM\*.log" "$TestResultDirOnHost" "$DomainInVM\$userNameInVM" "$userPwdInVM"

# Clean up signals after copy out from VM
.\RemoteExecute-Command.ps1 $testDriverCtrlIp "$testDirInVM\Scripts\CleanupSignals.cmd" "$DomainInVM\$userNameInVM" "$userPwdInVM"

#----------------------------------------------------------------------------
# Fix trx result if it has content format issue
#----------------------------------------------------------------------------
.\Fix-XmlLogFile.ps1 -logPath "$TestResultDirOnHost"

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Stop-Transcript
# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Execute-ProtocolTest.ps1"
}
exit 0
