#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Execute-ProtocolTest.ps1
## Purpose:        Protocol Test Suite Entry Point for MS-SMBD
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 8
## Copyright (c) Microsoft Corporation. All rights reserved.
## This Powershell script used to install MS-SMBD test suite and run test suite
## 
##############################################################################

param(
	[string]$SharedPath = $(throw "Parameter missing: -MachineName is missing"),
	[string]$MachineName = $(throw "Parameter missing: -MachineName is missing"),
	[string]$UserName = "Administrator",
	[string]$Password = "Password01!",
	[string]$TestSharedPath = "SMBDTest",
	[string]$TestPathInHost = "C:\SMBDTest",
	[string]$ZipName = "MS-SMBD-TestSuite-ServerEP.zip",
	[string]$TestSuiteName = "MS-SMBD",
	[string]$TestSuiteInstalledPath = "C:\Server-Endpoint",
	[string]$EnvironmentName = $(throw "Parameter missing: -EnvironmentName is missing"),
	[string]$TestResultsPath = $(throw "Parameter missing: -TestResultsPath is missing"),
	[string]$ApplicationId,
	[string]$ThumbPrint,
	[string]$TenantId,
	[string]$FileShareResourceGroup,
	[string]$ResultStorageAccount,
	[string]$OperatingSystem = "Windows",
	$AzureParams
)

if ($AzureParams -ne $null) {
	$ApplicationId = $AzureParams.ApplicationId
	$ThumbPrint = $AzureParams.ThumbPrint
	$TenantId = $AzureParams.TenantId
	$FileShareResourceGroup = $AzureParams.FileShareResourceGroup
	$ResultStorageAccount = $AzureParams.ResultStorageAccount
}

$smbdPath = "$SharedPath\*"

Write-Host "Execute-ProtocolTest.ps1 started..."

if ($OperatingSystem -eq "Windows") {
    Set-Item WSMan:\localhost\Client\TrustedHosts -Value * -Force

    $pwdConverted = ConvertTo-SecureString $Password -AsPlainText -Force
    $cred = New-Object System.Management.Automation.PSCredential ($UserName, $pwdConverted)

    $session = New-PSSession -ComputerName $MachineName -Credential $cred -ErrorAction Stop
    $testFullPath = "\\$MachineName\$TestSharedPath"
    Remove-Item $testFullPath -Recurse -Force
    Write-Host "Creating PSDrive for Windows"
    New-PSDrive -PSProvider FileSystem -Name "TestDrive" -Root $testFullPath -Credential $cred -ErrorAction Stop
}
elseif ($OperatingSystem -eq "Linux") {
    $session = New-PSSession -HostName $MachineName -UserName $UserName.ToLower() -KeyFilePath "~/.ssh/id_rsa"

	$TestPathInHost = "/mnt/smbdtest"
	$TestSuiteInstalledPath = "/mnt/server-endpoint"

    $testFullPath = "/mnt/testsharedpath"

    Write-Host "Creating PSDrive for Linux"
    New-PSDrive -PSProvider FileSystem -Name "TestDrive" -Root $testFullPath -ErrorAction Stop
}

Write-Host "Begin to copy test files to \\$MachineName\$TestSharedPath..."

# Copy build artifacts to remote share
Copy-Item -Path $smbdPath -Destination "TestDrive:\" -Recurse -Force

Write-Host "Finished copying test files to \\$MachineName\$TestSharedPath."

$RunCommand = [ScriptBlock] {

	Param ($TestSuiteName, $ZipPath, $TestSuiteInstalledPath, $TestPath, $EnvironmentName, $OperatingSystem, $MachineName, $UserName)

	try {
		Write-Host "TestSuiteName: $TestSuiteName"
		Write-Host "ZipPath: $ZipPath"
		Write-Host "TestSuiteInstalledPath: $TestSuiteInstalledPath"
		Write-Host "TestPath: $TestPath"

		Write-Host "Check uninstall zip test suite..."
		if (Test-Path -Path "$TestSuiteInstalledPath" -PathType Container) {
			Write-Host "Begin to uninstall zip test suite..."
			Remove-Item $TestSuiteInstalledPath -Recurse -Force
		}

		if (-not (Test-Path -Path "$TestSuiteInstalledPath" -PathType Container)) {
			$directoryInfo = Get-ChildItem "$TestSuiteInstalledPath" -ErrorAction SilentlyContinue | Measure-Object
			if ($directoryInfo.count -eq 0) {
				Write-Host 'Uninstall zip test suite successfully!'
			}
			else {
				Write-Host 'Failed to uninstall zip test suite!'
				return $false
			}
		}
		else {
			Write-Host "Failed to uninstall zip test suite: $TestSuiteInstalledPath exists!"
			return $false
		}

		# Install test suite
		Write-Host "Begin to install zip test suite..."
		Expand-Archive $ZipPath -DestinationPath $TestSuiteInstalledPath

		# Step 2: Rename all folders and files to lowercase
		# First, rename folders (deepest first to avoid path conflicts)
		if ($OperatingSystem -eq "Linux")
		{
			Get-ChildItem -Path $TestSuiteInstalledPath -Recurse -Directory |
				Sort-Object { $_.FullName.Length } -Descending |
				ForEach-Object { Rename-Item -Path $_.FullName -NewName $_.Name.ToLower() -ErrorAction SilentlyContinue }

			# Refresh folder list after renaming
			$folders = Get-ChildItem -Path $TestSuiteInstalledPath -Directory

			# Rename files inside folders (except Bin)
			foreach ($folder in $folders) {
				if ($folder.Name.ToLower() -ne "bin") {
					Get-ChildItem -Path $folder.FullName -File -Recurse | ForEach-Object {
						$lowerName = $_.Name.ToLower()
						Rename-Item -Path $_.FullName -NewName $lowerName -ErrorAction SilentlyContinue
					}
				}
			}
		}

		if (Test-Path -Path "$TestSuiteInstalledPath" -PathType Container) {
			$directoryInfo = Get-ChildItem "$TestSuiteInstalledPath" -ErrorAction SilentlyContinue  | Measure-Object
			if ($directoryInfo.count -ne 0) {
				Write-Host 'Install zip test suite successfully!'
			}
			else {
				Write-Host 'Failed to install zip test suite!'
				return $false
			}
		}
		else {
			Write-Host "Failed to install zip test suite: $TestSuiteInstalledPath does not exist!"
			return $false
		}

		$testSuitePath = $TestSuiteInstalledPath
		Write-Host "testSuitePath: $testSuitePath"
		Write-Host "Copy configuration files to $testSuitePath\Bin"
		
		if ($OperatingSystem -eq "Windows")
		{
			Copy-Item -Path $TestPath\Environments\$EnvironmentName\* -Destination $testSuitePath\Bin -Force
		}
		else
		{
			Copy-Item -Path "$TestPath\environments\$($EnvironmentName.ToLower())\*" -Destination $testSuitePath\bin -Force
		}
		
		Write-Host "Begin to execute test cases..."	

		Write-Host "Clean up test results and signal of previous regression run..."	
		# Clean up test results and signal of previous regression run
		Remove-Item "$TestPath\TestResults" -Recurse -Force -ErrorAction SilentlyContinue
		$signal = "$TestPath\testFinished.signal"
		Remove-Item $signal -Force -ErrorAction SilentlyContinue

		Write-Host "Change current directory to C:\SMBDTest..."	
		# Change current directory to C:\SMBDTest
		Set-Location $TestPath

		if($OperatingSystem -eq "Linux")
		{
			$session = New-PSSession -HostName $MachineName -UserName $UserName.ToLower() -KeyFilePath "~/.ssh/id_rsa"

			Invoke-Command -Session $session -FilePath "/mnt/server-endpoint/Batch/RunAllTestCasesLinux.ps1".ToLower()
			# Test Done
			Write-Host "Run Test Suite Done" -foregroundcolor Yellow

			$trxPath = Get-ChildItem -Path "$TestSuitePath\TestResults\" -Recurse -Filter *.trx -File -ErrorAction SilentlyContinue | Sort-Object -Property CreationTime -Descending | Select-Object -First 1
			if ($trxPath -eq $null) {
				Write-Host "Failed to locate test results!"
				return $false
			}

			Write-Host "Copy test results to $TestPath\TestResults"
			New-Item -Path $TestPath\TestResults\ -ItemType Directory
			Copy-Item -Path $trxPath.FullName -Destination $TestPath\TestResults\
		}
		else
		{
			Write-Host "Start to create the task for tests execution..."	
			# Start to create the task for tests execution
			$filePath = "$testSuitePath\Batch\RunAllTestCases.ps1"
			$taskName = "SMBD Test"
			# Push to the parent folder first, then run
			$parentDir = [System.IO.Directory]::GetParent($filePath)
			$command = "{Push-Location $parentDir; Invoke-Expression $filePath; Set-Content -Path $signal -Value finished}"

			Write-Host $parentDir
			Write-Host $signal
			Write-Host $command

			# Guarantee commands run in powershell environment
			$task = "powershell `'powershell -Command $command`'"

			Write-Host "Delete existing task..."
			# Delete existing task
			cmd /c schtasks /delete /tn $taskName /f

			Write-Host "Create task..."
			# Create task
			cmd /c schtasks /create /sc once /st 00:00 /tn $taskName /tr $task /it /f
			Start-Sleep -Seconds 5

			Write-Host "Start the tests execution task..."
			# Start the tests execution task
			# Start-ScheduledTask -TaskName $taskName
			cmd /c schtasks /run /tn $taskName

			#Push-Location $parentDir;
			#Invoke-Expression $filePath
			#Set-Content -Path $signal -Value finished

			Write-Host "Wait for the signal..."
			# Wait for the signal
			$testLoop = 120
			$sleepTime = 60

			while ($testLoop -gt 0) {
				if ($testLoop % 10 -eq 1) {
					Write-Host "."
				}
				else {
					Write-Host -NoNewline "."
				}

				Start-Sleep -Seconds $sleepTime

				if (Test-Path -Path $signal) {
					break
				}
			
				$testLoop -= 1;
			}

			if ($testLoop -eq 0) {
				Write-Host "Timed out when waiting for the finish signal of executing test cases!"
				return $false
			}

			Write-Host "Executing test cases finished!"

			$trxPath = Get-ChildItem -Path "$TestSuitePath\TestResults\" -Recurse -Filter *.trx -File -ErrorAction SilentlyContinue | Sort-Object -Property CreationTime -Descending | Select-Object -First 1
			if ($trxPath -eq $null) {
				Write-Host "Failed to locate test results!"
				return $false
			}

			Write-Host "Copy test results to $TestPath\TestResults"
			New-Item -Path $TestPath\TestResults\ -ItemType Directory
			Copy-Item -Path $trxPath.FullName -Destination $TestPath\TestResults\
		}
		
		return $true
	}
	catch {
		Write-Host "Failed: $_"
		return $false
	}
} 

$zipPath = "$testFullPath\$ZipName";

try {
	$cmdResult = Invoke-Command -Session $session -ScriptBlock $RunCommand -ArgumentList $TestSuiteName, $zipPath, $TestSuiteInstalledPath, $TestPathInHost, $EnvironmentName, $OperatingSystem, $MachineName, $UserName
}
catch {
	Write-Host "Test cases execution failed: $_"
	$cmdResult = $false
}

# Locate and copy test results
Write-Host "Copy test result files to the pipeline build agent"
if ($cmdResult) {
	$trxPath = Get-ChildItem -Path "TestDrive:\TestResults\" -Recurse -Filter *.trx -File -ErrorAction SilentlyContinue | Sort-Object -Property CreationTime -Descending | Select-Object -First 1
	if ($trxPath -ne $null) {
		Copy-Item -Path $trxPath.FullName -Destination $TestResultsPath
		Write-Host "Rename: $TestResultsPath\$($trxPath.Name) => $EnvironmentName.trx"
		if (Test-Path("$EnvironmentName.trx")) {
			Remove-Item "$EnvironmentName.trx"
		}
		Rename-Item -Path "$TestResultsPath\$($trxPath.Name)" -NewName "$EnvironmentName.trx"
	}
	else {
		Write-Host "Failed to copy trx file."
		$cmdResult = $false
	}
}

# Clean up
Remove-PSDrive -Name "TestDrive"
Remove-PSSession -Session $session

return $cmdResult