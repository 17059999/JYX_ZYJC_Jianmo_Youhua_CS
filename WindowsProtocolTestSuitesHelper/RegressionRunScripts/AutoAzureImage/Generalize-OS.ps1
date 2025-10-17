###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

param(
[string]$vmName, 
[string]$usr, 
[string]$pass
)

#--------------------------------------------------------------------------------------------------
# Generalize Operating System
#--------------------------------------------------------------------------------------------------
function Generalize-OS([string]$vmName, [string]$usr, [string]$pass)
{		
	$Pwrd = New-Object System.Security.SecureString
	if($null -ne $pass -and $pass -ne ""){
		$Pwrd = ConvertTo-SecureString -String $pass -AsPlainText -Force
	}
	$Credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $usr, $Pwrd
	
	#Attempt to connect with OS and timeout if unable to connect after 5 minutes
	$startTime = Get-Date
	$currentTime = Get-Date
	$timeSpan = New-TimeSpan -Start $startTime -End $currentTime

	Write-Host "Establishing connection with OS on VM $vmName.."
	$vmSession = New-PSSession -VMName $vmName -Credential $Credential -ErrorAction SilentlyContinue

	While(($null -eq $vmSession -or $vmSession.State -ne "Opened") -and $timeSpan.TotalMinutes -lt 10)
	{	
		Start-Sleep -Seconds 60
		Write-Host "Establishing OS connection on VM $vmName..."
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

	#Remove stage 1 scripts
	$vmScriptPath = "C:\PrepareOS"
	$unattendPath = "C:\unattend.xml"
	Invoke-Command -Session $vmSession -ScriptBlock {
		if(Test-Path $Using:vmScriptPath)
		{
			Remove-Item $Using:vmScriptPath -Recurse -Force -ErrorAction SilentlyContinue

			Write-Host "$Using:vmScriptPath deleted on second attempt" -ForegroundColor Green
		}
		if(Test-Path $Using:unattendPath)
		{
			Write-Host "Unable to delete Unattend File $Using:unattendPath" -ForegroundColor Red
			return
		}
		else
		{
			Write-Host "Unattend file $Using:unattendPath is confirmed as deleted" -ForegroundColor Green
		}
	}

	#Delete Panther and run sysprep
	$pantherPath = "C:\Windows\Panther"
	$sysprepPath = "C:\Windows\System32\Sysprep\sysprep.exe"

	Write-Host "Generalizing OS on VM $vmName..."
	
	Invoke-Command -Session $vmSession -ScriptBlock {
		if(Test-Path $Using:pantherPath)
		{
			Remove-Item $Using:pantherPath -Recurse -Force
		}
	}
	Invoke-Command -Session $vmSession -ScriptBlock {. $Using:sysprepPath /generalize /shutdown /oobe}

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

	$Global:stageCompleted = 3
	Write-Host "Generalize OS on VM $vmName completed" -ForegroundColor Green
}

Generalize-OS $vmName $usr $pass