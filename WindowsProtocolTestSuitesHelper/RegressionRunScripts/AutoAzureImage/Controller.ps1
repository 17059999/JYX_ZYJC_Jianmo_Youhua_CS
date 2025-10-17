###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

#--------------------------------------------------------------------------------------------------
# Entry script to Automate Creation of Azure Image(s)
#--------------------------------------------------------------------------------------------------
param(
[string]$basePath,
[string]$storageAccount,
[String]$token
)

Write-Host "*******************************************************************"
Write-Host "********Starting New Local Agent Run @ $(Get-Date)*********"
Write-Host "*******************************************************************"
write-host "$basePath, $storageaccount $token"
#verify script is running as administrator else elevate
$invocationDefinition = $MyInvocation.MyCommand.Definition
$scriptPath = split-path -parent $invocationDefinition

$user = New-Object Security.Principal.WindowsPrincipal $([Security.Principal.WindowsIdentity]::GetCurrent())
$userName = $user.Identities.Name
$userIsAdmin = $user.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)

if ($userIsAdmin -eq $false)
{
        Write-Host "Attemting to elevate user $userName"
        Start-Process powershell.exe -Verb RunAs -ArgumentList ('-noprofile -noexit -file "{0}" -elevated' -f ($invocationDefinition))
        exit
}
else
{
    Write-Host "Executing script as Administrator for user $userName"
}

Set-Location $scriptPath
$csvFileName = "AutoAzureImage.csv"

function Main($basePath, $csvFileName) 
{
	#verify base path
	if(-not ($null -eq $basePath -or $basePath -eq "") -and (Test-Path $basePath\$csvFileName))
	{
		Write-Host "Base directory path: $basePath"
	}
	else
	{
		throw "Invalid Base Path. Can't find CSV."
	}

	$Global:basePath = $basePath
	$createAzureImageStatus = @()

	# Start new transcript
	$now = (Get-Date).ToString().Replace(" ","_").Replace("/","_").Replace(":","_")
	$logFile = "$basePath\logs\LocalAgent$now.log"
	Write-Host "Log file path: $logFile"
	Start-Transcript -Path $logFile -Append -Force

	#Read CSV
	Write-Host "Reading CSV: $token"
	$userSettings = Import-CSV -Path $basePath/$csvFileName

	#process each image entry
	foreach($setting in $userSettings)
	{
		$Global:stageCompleted = 0
		
		#User settings
		$vmName = $setting.VMName
		$osType = $setting.OSType
		$imageSourceType = $setting.SourceType #Media Type
		$sourceImagePath = $setting.SourcePath
		$baseVHDName = $setting.BaseVHDName
		$azureImageName = $setting.AzureImageName
		$isoEdition = $setting.ISOEdition
		$productKey = $setting.ProductKey
		$resourceGroupName = $setting.ResourceGroupName
		$vhdUploadUrl = $setting.VHDUploadUrl
		$azureImagelocation = $setting.AzureImagelocation
		$overwriteVHD = [bool]::Parse($setting.OverwriteVHD)
		$overwriteImage = [bool]::Parse($setting.OverwriteImage)

		#Begin by creating VM
		. $basePath\Create-VM.ps1 $vmName $osType $imageSourceType $sourceImagePath $baseVHDName $azureImageName $isoEdition $productKey $Global:basePath

		if($Global:stageCompleted -eq 1)
		{        
			#Prepare OS and restart
			. $basePath\Prepare-OS.ps1 $vmName $Global:username $Global:password $Global:basePath $Global:vmPath
		}
		else
		{
			Write-Host "Could not create VM $vmName." -ForegroundColor Red
			Write-Host "****** Aborting creation of Azure Image for $vmName. See log file for further infomation. ******" -ForegroundColor Red
			$createAzureImageStatus += @([pscustomobject]@{VMName=$vmName;AzureImageStatus='Failed'})
			continue
		}
		
		if($Global:stageCompleted -eq 2)
		{        
			#Generalize the OS and shutdown
			. $basePath\Generalize-OS.ps1 $vmName $Global:username $Global:password
		}
		else
		{
			Write-Host "Could not prepare OS on VM $vmName. " -ForegroundColor Red
			Write-Host "****** Aborting creation of Azure Image for $vmName. See log file for further infomation. ******" -ForegroundColor Red
			$createAzureImageStatus += @([pscustomobject]@{VMName=$vmName;AzureImageStatus='Failed'})
			continue
		}
		
		if($Global:stageCompleted -eq 3)
		{        
			#Upload Base VHD and Create Azure Image
			. $basePath\Create-AzureImage.ps1 $vmName $Global:vmPath "$Global:sourceVHDName.vhd" $baseVHDName $azureImageName $resourceGroupName $vhdUploadUrl $azureImagelocation $overwriteVHD $overwriteImage $storageAccount $token
		} else {
			Write-Host "Could not generalize OS on VM $vmName." -ForegroundColor Red
			Write-Host "****** Aborting creation of Azure Image for $vmName. See log file for further infomation. ******" -ForegroundColor Red
			$createAzureImageStatus += @([pscustomobject]@{VMName=$vmName;AzureImageStatus='Failed'})
			continue
		}
		
		if($Global:stageCompleted -eq 4)
		{
			Write-Host "Creation of Azure Image for $vmName successful" -ForegroundColor Green
		}
		else
		{
			Write-Host "Could not create azure image for VM $vmName." -ForegroundColor Red
			Write-Host "****** Aborting creation of Azure Image for $vmName. See log file for further infomation. ******" -ForegroundColor Red
			$createAzureImageStatus += @([pscustomobject]@{VMName=$vmName;AzureImageStatus='Failed'})
			continue
		}

		$createAzureImageStatus += @([pscustomobject]@{VMName=$vmName;AzureImageStatus='Successful'})
		
		#Cleanup
		#DeleteVM and resources folder
		. $basePath\Cleanup.ps1 $Global:vmPath $vmName

		Write-Host "****** Finished creating Azure Image for $vmName @ $(Get-Date) ******"
	}

	#Print create azure image status for all supplied images
	$createAzureImageStatus

	Write-Host "*******************************************************************"
	Write-Host "**********Finished Local Agent Run @ $(Get-Date)***********"
	Write-Host "*******************************************************************"

	$null = Stop-Transcript
}
write-host "Main $basePath $csvFileName $token"
Main $basePath $csvFileName $token