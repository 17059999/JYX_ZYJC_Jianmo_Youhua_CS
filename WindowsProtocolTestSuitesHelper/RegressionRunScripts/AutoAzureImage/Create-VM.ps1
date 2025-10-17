###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

param(
	[string]$vmName,
	[string]$osType,
	[string]$imageSourceType,
	[string]$sourceImagePath,
	[string]$baseVHDName,
	[string]$azureImageName,
	[string]$isoEdition,
	[string]$productKey,
	[string]$basePath
)

#--------------------------------------------------------------------------------------------------
# Add Answer File to VHD
#--------------------------------------------------------------------------------------------------
function AddAnswerFileToVHD([string]$vhdPath, [string]$OSType, [string]$productKey)
{

	Write-Host "Adding answer file to VHD $vhdPath"

	if (StringIsNullOrEmpty($vhdPath, "VHD Path") -or StringIsNullOrEmpty($OSType, "OS Type"))
	{
		return
	}

	$mountedVHD = Mount-VHD -Path $vhdPath -Passthru

	[String]$driveLetter =  (($MountedVHD | Get-Disk | Get-Partition | Select-Object -ExpandProperty DriveLetter) + ":").Replace(" ", "")

	Start-Sleep -Seconds 2

	$vhdUnattendFilePath = "$driveLetter\unattend.xml"
	if($OSType -eq "Client")
	{
		$unattendFilePath = "$basePath\Unattend\Client.xml"
	}
	else
	{
		$unattendFilePath = "$basePath\Unattend\Server.xml"
	}	

	Write-Host "Writing answer file of $OSType OS from $unattendFilePath to $vhdPath"
	#Modify Product Key
	[XML]$Xml = Get-Content($unattendFilePath)	
    [string]$productKeyTemp = $Xml.unattend.settings[1].component[0].ProductKey
	if($null -eq $productKeyTemp -or $productKeyTemp -eq "")
	{
		Dismount-VHD $mountedVHD.Path
		Throw "Unable to process unattend file"
		return
	}
	else
	{
		$Xml.unattend.settings[1].component[0].ProductKey = $productKey
	}

	#Load Credential
	[string]$passwordTemp
	
	$passwordTemp = $Xml.unattend.settings[0].component[0].UserAccounts.AdministratorPassword.Value

	if($null -eq $passwordTemp)
	{
		Dismount-VHD $mountedVHD.Path
		Throw "Unable to process unattend file"
	}
	
	$Global:username ="Administrator"
	$Global:password = $passwordTemp

	#Save on VHD
    $Xml.Save($vhdUnattendFilePath)

	Start-Sleep -Seconds 2 

	Dismount-VHD $mountedVHD.Path
	
	Write-Host "Adding AnswerFile to VHD completed" -ForegroundColor Blue
}

#--------------------------------------------------------------------------------------------------
# Convert ISO File to VHD for specified edition
#--------------------------------------------------------------------------------------------------
function ConvertISOToVHD([string]$isoMediaPath, [string]$vhdMediaPath, [string]$edition)
{
	Write-Host "Converting ISO $isoMediaPath to VHD $vhdMediaPath"

	if (StringIsNullOrEmpty($isoMediaPath, "ISO path") -or StringIsNullOrEmpty($vhdMediaPath, "VHD path") -or StringIsNullOrEmpty($edition, "ISO OS edition"))
	{
		return
	}
	
	. $basePath\Convert-WindowsImage.ps1 -SourceISOPath $isoMediaPath -DestinationVHDPath $vhdMediaPath -Edition $edition -DiskLayout "BIOS" -VHDFormat VHD -VHDSize 82GB
	
	Write-Host "Converting ISO to VHD Completed" -ForegroundColor Blue
}

function StringIsNullOrEmpty($value, $name){
	if($null -eq $value -or $value -eq "")
	{
		Write-Host "Parameter $name is required" -ForegroundColor Red
		return $true
	}
	return $false
}

#--------------------------------------------------------------------------------------------------
# Script to Create Virtual Machine
#--------------------------------------------------------------------------------------------------
function Create-VM(
	[string]$vmName,
	[string]$osType,
	[string]$imageSourceType,
	[string]$sourceImagePath,
	[string]$baseVHDName,
	[string]$azureImageName) {
	#----------------------------------------------------------------------------
	# Verify required parameters
	#----------------------------------------------------------------------------

	if (StringIsNullOrEmpty($vmName, "VMName") -or StringIsNullOrEmpty($imageSourceType, "ImageSourceType") -or StringIsNullOrEmpty($sourceImagePath, "SourceImagePath") -or StringIsNullOrEmpty($baseVHDName, "BaseVHDName") -or StringIsNullOrEmpty($azureImageName, "AzureImageName"))
	{
		return
	}

	if($osType -ne "Client" -and $osType -ne "Server")
	{
		Write-Host "Invalid OS Type. OS must be either a Client or Server." -ForegroundColor Red
	}

	if (-not ($azureImageName  -match  "^[^_\W][\w-._]{0,79}(?<![-.])$"))
	{
		Write-Host "Parameter AzureImageName does not match the pattern ^[^_\W][\w-._]{0,79}(?<![-.])$" -ForegroundColor Red
		Write-Host "Valid name should be like 19043.985.amd64fre.21h1_release_svc.210517-1932_client_enterprise_en-us" -ForegroundColor Red
		return
	}

	#validate source type
	if($imageSourceType -ne "VHD" -and $imageSourceType -ne "ISO")
	{
		Write-Host "Invalid source image type $imageSourceType" -ForegroundColor Red
		return
	}

	#Application settings
	$vmPath = "$basePath\autovm\$vmName"	
	$networkSwitch = 'External'
	$memorySize = 8192MB
	
	Write-Host "Creating VM $vmName"
	
	#Remove VM and resources if already exists
	if(Get-VM -Name $vmName -ErrorAction SilentlyContinue)
	{
		Stop-VM -Name $vmName -Force
		Remove-VM -Name $vmName -Force
		Write-Warning "Existing VM $vmName has been removed"
	}
	else
	{
		Write-Host "No existing resources found for VM $vmName" -ForegroundColor Blue
	}

	if(Test-Path $vmPath)
	{
		Remove-Item $vmPath -Recurse
		Write-Warning "Existing directory $vmPath has been deleted"
	}

	#create new VM resources
	mkdir $vmPath
	
	$pathLength = $sourceImagePath.length;
	$imageNameStartIndex = $sourceImagePath.lastIndexOf('\') + 1
	$imageNameEndIndex = $sourceImagePath.lastIndexOf('.') + 1
	$sourceVHDName = $sourceImagePath.substring($imageNameStartIndex, $imageNameEndIndex - $imageNameStartIndex - 1)
	$destinationImagePath = "$vmPath\$sourceVHDName.$($imageSourceType.toLower())"
	$sourceImageExtension = $sourceImagePath.substring($imageNameEndIndex, $pathLength - $imageNameEndIndex)
	$newVHDPath =  "$vmPath\$sourceVHDName.vhd"

	$sourceImage =  $sourceImageExtension.toLower()
	$PathTest = Test-Path $sourceImagePath;
	$ImageSType = $imageSourceType.toLower()

	Write-Host "Source Image Extension : $sourceImage" 
	Write-Host "Image Source Type : $ImageSType"
	Write-Host "Path Test : $PathTest"

	#Copy source image to local drive
	if ((Test-Path $sourceImagePath) -and ($sourceImageExtension.toLower() -eq $imageSourceType.toLower()))
	{
		Write-Host "Coyping media from $sourceImagePath to $destinationImagePath"
		Copy-Item $sourceImagePath $destinationImagePath
		Write-Host "Copy media completed" -ForegroundColor Green
	}
	else
	{
		Write-Host "Invalid source image $sourceImagePath" -ForegroundColor Red
		return
	}
	
	if (-not (Test-Path $destinationImagePath))
	{
		Write-Host "Unable to copy source image $sourceImagePath" -ForegroundColor Red
		return
	}

	$autoVMSwitch = Get-VMSwitch $networkSwitch -ErrorAction SilentlyContinue
	if($null -eq $autoVMSwitch -or $autoVMSwitch -eq "")
	{
		Write-Host "External switch with name External not found on Hyper-V manager. 
		Please create an external switch with name External for internet connection." -ForegroundColor Red
		return
	}

	#Setup VM
	if ($imageSourceType -eq 'VHD')
	{
		#Resize VHD
		Resize-VHD $destinationImagePath -SizeBytes 82GB
		
		#Add AnswerFile to VHD
		AddAnswerFileToVHD $destinationImagePath $osType $productKey -ErrorAction Stop

		#Create VM
		New-VM -Name $vmName -Generation 1 -MemoryStartupBytes $memorySize -Path $vmPath -VHDPath $destinationImagePath -Switch $networkSwitch
	}
	elseif ($imageSourceType -eq 'ISO')
	{
		#convert Windows ISO image to VHD
		ConvertISOToVHD $destinationImagePath $newVHDPath $isoEdition

		Start-Sleep -Seconds 2
		if (-not (Test-Path $newVHDPath))
		{
			Write-Host "Unable to convert ISO to VHD" -ForegroundColor Red
			return
		}

		#Add AnswerFile to VHD
		AddAnswerFileToVHD $newVHDPath $osType $productKey -ErrorAction Stop

		#Create VM
		New-VM -Name $vmName -Generation 1 -MemoryStartupBytes $memorySize -Path $vmPath -VHDPath $newVHDPath -Switch $networkSwitch
	}
	else
	{
		Write-Host "Unsupported image source type $imageSourceType" -ForegroundColor Red
		return
	}

	Write-Host "VM $vmName has been created successfully" -ForegroundColor Green

	#Start VM
	Start-VM -Name $vmName
	Start-Sleep -Seconds 2
	if((Get-VM $vmName).state -eq "Running")
	{
		Write-Host "VM $vmName has been started successfully" -ForegroundColor Green
	}
	else
	{
		Write-Host "Unable to start VM $vmName" -ForegroundColor Red
		return
	}

	#Install OS on VM
	Write-Host "Installing Windows on $vmName"

	Start-Sleep -Seconds 120 #Wait for unattended Windows installation to complete
	
	$Global:vmPath = $vmPath
	$Global:sourceVHDName = $sourceVHDName
	$Global:stageCompleted = 1
}

Create-VM $vmName $osType $imageSourceType $sourceImagePath $baseVHDName $azureImageName $basePath