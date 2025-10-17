#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Execute-ProtocolTest.ps1
## Purpose:        Protocol Test Suite Entry Point for MS-WSP
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2012 R2 and above
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
	[string]$ProtocolName = "MS-WSP", 
	[string]$WorkingDirOnHost = "C:\WinteropProtocolTesting", 
	[string]$TestResultDirOnHost = "C:\WinteropProtocolTesting\TestResults\MS-WSP",
	[string]$EnvironmentName = "MS-WSP.xml",
	[string]$TestCaseScript = "Execute-ProtocolTestOnDriver.ps1",
	[string]$TestCaseTimeout = "28800" # Timeout in seconds for test case execution
)

#----------------------------------------------------------------------------
# Define call stack functions
#----------------------------------------------------------------------------
[int]$global:indentControl = 0
$global:callStackLogFile = "$TestResultDirOnHost\Execute-ProtocolTest.ps1.CallStack.log"

function global:EnterCallStack($scriptName) {
	if ($scriptName -ne "Execute-ProtocolTest.ps1") {
		$global:indentControl++
	}
	$tab = ""
	for ($i = 1; $i -le $global:indentControl; $i++) {
		$tab += "    "
	}
	$date = Get-Date -Format "M/d/yyyy hh:mm:ss"
	("") | Out-File -FilePath $callStackLogFile -Append -Force
	($tab + "Start $scriptName    " + $date) | Out-File -FilePath $callStackLogFile -Append -Force
}

function global:ExitCallStack($scriptName) {
	$tab = ""
	for ($i = 1; $i -le $global:indentControl; $i++) {
		$tab += "    "
	}
	if ($scriptName -ne "Execute-ProtocolTest.ps1") {
		$global:indentControl--
	}
	$date = Get-Date -Format "M/d/yyyy hh:mm:ss"
	($tab + "Exit $scriptName    " + $date) | Out-File -FilePath $callStackLogFile -Append -Force
}

if ($function:EnterCallStack -ne $null) {
	EnterCallStack "Execute-ProtocolTest.ps1"
}

#----------------------------------------------------------------------------
# Start logging using Start-Transcript cmdlet
#----------------------------------------------------------------------------
try { Stop-Transcript -ErrorAction SilentlyContinue } catch {} # Ignore Stop-Transcript error messages
Start-Transcript -Path "$TestResultDirOnHost\Execute-ProtocolTest.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Get VM configuration 
#----------------------------------------------------------------------------
Write-Host "Get VM configuration..." -ForegroundColor Yellow
Set-StrictMode -Version 2

$vmConfigFile = "$WorkingDirOnHost\ProtocolTestSuite\$ProtocolName\VSTORMLITEFiles\XML\$EnvironmentName"
[xml]$config = Get-Content $vmConfigFile
$client = $config.lab.servers.vm | Where-Object { $_.role -eq "DriverComputer" }

$userNameInVM = $config.lab.core.username
$userPwdInVM = $config.lab.core.password
$clientName = $client.name
$clientIp = $client.ip
if($config.lab.core.timeoutinminutes -ne $null)
{
	[string]$TestCaseTimeout = [int]$config.lab.core.timeoutinminutes * 60
}

#----------------------------------------------------------------------------
# Set current directory to ScriptLib directory
#----------------------------------------------------------------------------
Write-Host "Define ScriptLib directory..." -ForegroundColor Yellow
$srcScriptLibPathOnHost = "$WorkingDirOnHost\ScriptLib"

Push-Location $srcScriptLibPathOnHost

#----------------------------------------------------------------------------
# Run test cases on WSP client
#----------------------------------------------------------------------------
Write-Host "Run test cases on WSP client..." -ForegroundColor Yellow

if (!(Test-Path $TestResultDirOnHost)) {
	New-Item -ItemType Directory $TestResultDirOnHost
}

$global:testResultDir = $TestResultDirOnHost
& .\WaitFor-ComputerReady.ps1 $clientIp "$clientName\$userNameInVM" "$userPwdInVM"

# Create task for execute protocol test suite
$taskName = "RunTestCases"
$task = "powershell C:\temp\Scripts\$TestCaseScript"

$createTask = "cmd /c schtasks /create /ru $userNameInVM /rp $userPwdInVM /sc once /st 00:00 /tn $taskName /tr `"$task`" /it /f"
Write-Host "$createTask"
& .\RemoteExecute-Command.ps1 $clientIp "$createTask" "$clientName\$userNameInVM" "$userPwdInVM"

$tsTargetFolder = $client.tools.TestsuiteZip.targetFolder

# Execute the task to execute protocol test suite
$exeTask = "cmd /c schtasks /run /tn $taskName"
# Execute task with limited retry times
for ($i = 0; $i -lt 5; $i++) {
	Write-Host "$exeTask"
	& .\RemoteExecute-Command.ps1 $clientIp "$exeTask" "$clientName\$userNameInVM" "$userPwdInVM"    
	Start-sleep -Seconds 15
	Write-Host "Wait for test to start"
	try {
		& .\WaitFor-ComputerReady.ps1 $clientIp "$clientName\$userNameInVM" "$userPwdInVM" "$tsTargetFolder" "test.started.signal" 5
	}
	catch {
		continue
	}
	if ($lastexitcode -eq 0) {
		break
	}
}

Write-Host "Wait for test to complete"
Start-sleep -Seconds 120
& .\WaitFor-ComputerReady.ps1 $clientIp "$clientName\$userNameInVM" "$userPwdInVM" "$tsTargetFolder\TestResults" "test.finished.signal" $TestCaseTimeout

#----------------------------------------------------------------------------
# Copy results from VM 
#----------------------------------------------------------------------------
Write-Host "Copy results from VM..." -ForegroundColor Yellow
$tsTargetShare = $tsTargetFolder.Replace(":", "$")
$slashIndex = $tsTargetShare.IndexOf("\")
$tsTargetRoot = if ($slashIndex -gt 0) {
	$tsTargetShare.SubString(0, $slashIndex)
}
else {
	$tsTargetShare
}

Write-Host "Try to copy test result from \\$clientIp\$tsTargetRoot by `"$clientName\$userNameInVM`" / $userPwdInVM..." -Foregroundcolor Yellow

net.exe use "\\$clientIp\$tsTargetRoot" "$userPwdInVM" /user:"$clientName\$userNameInVM" 2>&1 | Write-Host

Copy-Item -Path "\\$clientIp\$tsTargetShare\TestResults\*.trx" -Destination $TestResultDirOnHost -Recurse -Force
Copy-Item -Path "\\$clientIp\$tsTargetShare\Bin\TestLog\*" -Destination $TestResultDirOnHost -Recurse -Force
Copy-Item -Path "\\$clientIp\$tsTargetShare\Bin\MS-WSP_ServerTestSuite.ptfconfig" -Destination $TestResultDirOnHost -Force
Copy-Item -Path "\\$clientIp\$tsTargetShare\Bin\MS-WSP_ServerTestSuite.deployment.ptfconfig" -Destination $TestResultDirOnHost -Force

net.exe use "\\$clientIp\$tsTargetRoot" /delete 2>&1 | Write-Host

& .\Fix-XmlLogFile.ps1 -LogPath "$TestResultDirOnHost"

Write-Host "$ProtocolName execution completed." -ForegroundColor Green

Pop-Location

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction SilentlyContinue

# Write Call Stack
if ($function:ExitCallStack -ne $null) {
	ExitCallStack "Execute-ProtocolTest.ps1"
}

exit 0
