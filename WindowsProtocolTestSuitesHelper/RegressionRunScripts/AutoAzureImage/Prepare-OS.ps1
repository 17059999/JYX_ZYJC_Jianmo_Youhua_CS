###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

param(
[string]$vmName, 
[string]$usr, 
[string]$pass,
[string]$basePath,
[string]$vmPath
)

#--------------------------------------------------------------------------------------------------
# Prepare VHD to upload to Azure
#--------------------------------------------------------------------------------------------------
function Prepare-OS ([string]$vmName, [string]$usr, [string]$pass, [string]$basePath)
{	
	$PWord = ConvertTo-SecureString -String $pass -AsPlainText -Force
	$Credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $usr, $PWord

	$startTime = Get-Date
	$currentTime = Get-Date
	$timeSpan = New-TimeSpan -Start $startTime -End $currentTime

	#Attempt to connect to OS and timeout if unable to connect after 5 minutes

	Write-Host "Establishing connection with OS on VM $vmName.."
	$vmSession = New-PSSession -VMName $vmName -Credential $Credential -ErrorAction SilentlyContinue

	While(($null -eq $vmSession -or $vmSession.State -ne "Opened") -and $timeSpan.TotalMinutes -lt 10)
	{	
		Start-Sleep -Seconds 60
		Write-Host "Establishing connection with OS on VM $vmName..."
		$vmSession = New-PSSession -VMName $vmName -Credential $Credential -ErrorAction SilentlyContinue
		$currentTime = Get-Date
		$timeSpan = New-TimeSpan -Start $startTime -End $currentTime
	}

	if($null -eq $vmSession -or $vmSession.State -ne "Opened")
	{
		Write-Host "Timeout. Unable to establish connection with Guest OS on VM $vmName using $usr credential" -ForegroundColor Red
		return
	}
	else
	{
		Write-Host "Established connection with OS on VM $vmName successfully" -ForegroundColor Green
	}

	#Remove unattend file
	$unattendPath = "C:\unattend.xml"
	Invoke-Command -Session $vmSession -ScriptBlock {Remove-Item $Using:unattendPath -Force -ErrorAction SilentlyContinue}

	#Prepare OS for use in Base VHD
	Write-Host "Preparing Operating System on VM $vmName and adding OEM artifact"

	#Copy SetupComplete2 to enable admin users
	$hostSetupCompletePath = "$basePath\Utility\OEM"
	$vmSetupComplete2Path = "C:\Windows\OEM"

	Copy-Item -ToSession $vmSession -Path $hostSetupCompletePath -Destination $vmSetupComplete2Path -Recurse -Force

	$hostScriptPath = "$basePath\PrepareOS"
	$vmScriptPath = "C:\PrepareOS"

	Invoke-Command -Session $vmSession -ScriptBlock{
		if(Test-Path $Using:vmScriptPath)
		{
			Remove-Item $Using:vmScriptPath -Recurse -Force -ErrorAction SilentlyContinue
		}
	}
	Copy-Item -ToSession $vmSession -Path $hostScriptPath -Destination $vmScriptPath -Recurse -Force

	Invoke-Command -Session $vmSession -ScriptBlock {Set-ExecutionPolicy -ExecutionPolicy Unrestricted}
	Invoke-Command -Session $vmSession -ScriptBlock {
		Set-Location $Using:vmScriptPath
		. "$Using:vmScriptPath\prepare_os.ps1"
	}

	#Verify PrepareOS successful
	$remotesuccessFile = "$vmScriptPath\prepare_os.signal.txt"
	$localSuccessFile = "$vmPath\PrepareOSVerification\$vmName.txt"
	if(-not (Test-Path "$vmPath\PrepareOSVerification"))
	{
		Write-Host "Creating $vmPath\PrepareOSVerification"
		mkdir "$vmPath\PrepareOSVerification"
	}
	Copy-Item -FromSession $vmSession -Path $remotesuccessFile -Destination $localSuccessFile

	if(-not(Test-Path -Path $localSuccessFile) -or (Get-Content -Path $localSuccessFile) -ne "Prepare OS Completed")
	{
		Write-Host "Preparing OS on VM $vmName failed" -ForegroundColor Red
		return
	}
	else
	{
		Write-Host "Preparing OS on VM $vmName was successful" -ForegroundColor Green
	}

	#Remove PrepareOS folder and restart VM
	Invoke-Command -Session $vmSession -ScriptBlock {
		Set-Location C:\
		Remove-Item $Using:vmScriptPath -Recurse -Force -ErrorAction SilentlyContinue
	}
	Invoke-Command -Session $vmSession -ScriptBlock {Restart-Computer localhost -Force}

	Write-Host "Restarting VM $vmName"

	#Wait for VM to shutdown
	$startTime = Get-Date
	$currentTime = Get-Date
	$timeSpan = New-TimeSpan -Start $startTime -End $currentTime
	while($null -ne $vmSession -and $vmSession.State -eq "Opened" -and $timeSpan.TotalMinutes -lt 10)
	{
		Start-Sleep -Seconds 1
		$vmSession = New-PSSession -VMName $vmName -Credential $Credential -ErrorAction SilentlyContinue
		$currentTime = Get-Date
		$timeSpan = New-TimeSpan -Start $startTime -End $currentTime
	}

	$Global:stageCompleted = 2
}

Prepare-OS $vmName $usr $pass $basePath