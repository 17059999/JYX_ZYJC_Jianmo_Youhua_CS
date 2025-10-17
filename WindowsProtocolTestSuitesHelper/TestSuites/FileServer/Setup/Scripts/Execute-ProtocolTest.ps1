#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Execute-ProtocolTest.ps1
## Purpose:        Protocol Test Suite Entry Point for FileServer
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 8
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
	[string]$protocolName          = "FileServer", 
	[string]$WorkingDirOnHost      = "C:\WinteropProtocolTesting", 
	[string]$testResultDirOnHost   = "C:\WinteropProtocolTesting\TestResults\FileServer",
	[string]$EnvironmentName       = "FileServer.xml",
	[string]$TestCaseScript        = "Execute-TestCaseByContext.ps1",
	[string]$TestCaseTimeout       = "28800",
	[string]$ContextName           = "Win2016_Domain_Cluster_SMB311",
	[string]$TestDirInVM           = "C:\Test",
	[string]$CategoryName          = "",
	[string]$TestName			   = "",
	[string]$runTests              = "true"

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
$global:callStackLogFile = "$testResultDirOnHost\Execute-ProtocolTest.ps1.CallStack.log"
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
try { Stop-Transcript -ErrorAction SilentlyContinue } catch {} # Ignore Stop-Transcript error messages
Start-Transcript -Path "$testResultDirOnHost\Execute-ProtocolTest.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Define source folders
#----------------------------------------------------------------------------
Write-Host "Define source folders..." -foregroundcolor Yellow

$srcScriptLibPathOnHost = "$WorkingDirOnHost\ScriptLib"
$global:testResultDir  = $testResultDirOnHost

#----------------------------------------------------------------------------
# Get VM configuration 
#----------------------------------------------------------------------------
Write-Host "Get VM configuration ..." -foregroundcolor Yellow
Set-StrictMode -v 2

$VMConfigFile =  "$WorkingDirOnHost\ProtocolTestSuite\$ProtocolName\VSTORMLITEFiles\XML\$EnvironmentName"
[xml]$config = get-content $VMConfigFile
$driver = $config.lab.servers.vm | Where-Object {$_.role -eq "DriverComputer"}

$global:userNameInVM    = $config.lab.core.username
$global:userPwdInVM     = $config.lab.core.password
$global:domainInVM      = $driver.domain
$global:testDriverCtrlIp = $driver.ip[0]
$global:testDirInVM     = $TestDirInVM
if($config.lab.core.timeoutinminutes -ne $null)
{
	[string]$TestCaseTimeout = [int]$config.lab.core.timeoutinminutes * 60
}

if(!(Test-Path $testResultDirOnHost))
{
	md $testResultDirOnHost
}

Push-Location $srcScriptLibPathOnHost
.\WaitFor-ComputerReady.ps1 $testDriverCtrlIp "$DomainInVM\$userNameInVM" "$userPwdInVM"

If($testDirInVM -match "SYSTEMDRIVE")
{
	$VmSystemDrive = .\Get-RemoteSystemDrive.ps1 $testDriverCtrlIp "$DomainInVM\$userNameInVM" "$userPwdInVM"
	$TestDirInVM = $testDirInVM.Replace("SYSTEMDRIVE", $VmSystemDrive )
}

#----------------------------------------------------------------------------
# Run test cases on test driver
#----------------------------------------------------------------------------
Write-Host "Run test cases on test driver ..." -foregroundcolor Yellow
if($driver.SelectSingleNode("os") -and ($driver.os -eq "Linux" -or $driver.os -eq "RPMBasedLinux")){
	[string]$driverFullUserName   = $userNameInVM + "@" + $testDriverCtrlIp
	Write-Host "ssh $driverFullUserName pwsh /home/$userNameInVM/Temp/Scripts/$TestCaseScript $ContextName $CategoryName $TestName $runTests"
	$task = ""

	if($ContextName -ne $null -or $ContextName.trim() -ne "")
	{
		$task += " -ContextName '$ContextName'"
	}

	if($CategoryName -ne "") 
	{
		$task += " -CategoryName '$CategoryName'"
	}

	if($TestName -ne "")
	{
		$task += " -TestName '$TestName'"
	}

	$task += " -runTests '$runTests'"

	ssh $driverFullUserName pwsh /home/$userNameInVM/Temp/Scripts/$TestCaseScript$task > $null
	# Test Done
    Write-Host "Run Test Suite Done" -foregroundcolor Yellow
	# Copy test result to host
	Write-Host "Copy results from VM ..." -foregroundcolor Yellow
	Write-Host "cmd /c scp $($driverFullUserName):$testDirInVM/TestResults/* $TestResultDirOnHost"
	cmd /c scp "$($driverFullUserName):$testDirInVM/TestResults/*" $TestResultDirOnHost 
} else {
	# Create task for execute protocol test suite
	$taskName = "RunTestCases"
	$task = "C:\PROGRA~1\POWERS~1\7\pwsh.exe c:\temp\Scripts\$TestCaseScript"
	
	if($ContextName -ne $null -or $ContextName.trim() -ne "")
	{
		$task += " -ContextName '$ContextName'"
	}

	if($CategoryName -ne "") 
	{
		$task += " -CategoryName '$CategoryName'"
	}

	if($TestName -ne "")
	{
		$task += " -TestName '$TestName'"
	}

	$task += " -runTests '$runTests'"

	$createTask = "CMD /C schtasks /Create /RU Administrators /RP $($global:userPwdInVM) /SC ONCE /ST 00:00  /TN $taskName /TR `"$Task`" /IT /F" #/SD default value is current date
	Write-Host "$createTask"
	.\RemoteExecute-Command.ps1 $testDriverCtrlIp "$createTask" "$domainInVM\$userNameInVM" "$userPwdInVM"

	# Execute the task to execute protocol test suite
	$exeTask = "cmd /c schtasks /Run /TN $taskName"
	# Execute task with limited retry times
	for($i=0;$i -lt 5;$i++) 
	{
		Write-Host "$exeTask"
		.\RemoteExecute-Command.ps1 $testDriverCtrlIp "$exeTask" "$domainInVM\$userNameInVM" "$userPwdInVM"    
		Start-sleep 15
		Write-Host "wait for test to start"
		try
		{
			& $WorkingDirOnHost\CommonScripts\WaitFor-ComputerReady.ps1 $testDriverCtrlIp "$domainInVM\$userNameInVM" "$userPwdInVM" $testDirInVM "test.started.signal" 5
		}
		catch
		{
			continue
		}
		if($lastexitcode -eq 0)
		{
			break
		}
	}

	Write-Host "Wait for test to complete"
	Start-sleep 120
	& .\WaitFor-ComputerReady.ps1 $testDriverCtrlIp "$DomainInVM\$userNameInVM" "$userPwdInVM" $testDirInVM "test.finished.signal" $TestCaseTimeout


	#----------------------------------------------------------------------------
	# Copy results from VM 
	#----------------------------------------------------------------------------
	Write-Host "Copy results from VM ..." -foregroundcolor Yellow
	.\Copy-TestResult.ps1 $testDriverCtrlIp "$testDirInVM\TestResults\*.trx" "$testResultDirOnHost" "$DomainInVM\$userNameInVM" "$userPwdInVM"
	.\Copy-TestResult.ps1 $testDriverCtrlIp "$testDirInVM\TestResults\*.txt" "$testResultDirOnHost" "$DomainInVM\$userNameInVM" "$userPwdInVM"
	.\Copy-TestResult.ps1 $testDriverCtrlIp "$testDirInVM\TestResults\*.ptfconfig" "$testResultDirOnHost" "$DomainInVM\$userNameInVM" "$userPwdInVM"
	.\Copy-TestResult.ps1 $testDriverCtrlIp "$testDirInVM\TestLog" "$testResultDirOnHost" "$DomainInVM\$userNameInVM" "$userPwdInVM"
	# Clean up signals after copy out from VM
	.\RemoteExecute-Command.ps1 $testDriverCtrlIp "c:\temp\Scripts\CleanupSignals.cmd" "$DomainInVM\$userNameInVM" "$userPwdInVM"

	Push-Location $scriptPath
	& .\Fix-XmlLogFile.ps1 -logPath "$testResultDirOnHost"
	Pop-Location
}

Write-Host "$protocolName execute completed." -foregroundcolor Green

Pop-Location

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
