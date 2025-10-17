###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

param(
    [string]$vmName, 
    [string]$vhdPath, 
    [string]$vhdName,
    [string]$baseVHDName,
    [string]$azureImageName,
    [string]$resourceGroupName,
    [string]$VHDUploadUrl,
    [string]$azureImageLocation,
    [bool]$overwriteVHD,
    [bool]$overwriteImage,
    [string]$storageAccount,
    [String]$token
)

#--------------------------------------------------------------------------------------------------
# Upload Base VHD and Create Azure Image
#--------------------------------------------------------------------------------------------------
function Create-AzureImage(
    [string]$vmName, 
    [string]$vhdPath, 
    [string]$vhdName, 
    [string]$baseVHDName, 
    [string]$azureImageName,
    [string]$resourceGroupName,
    [string]$VHDUploadUrl,
    [string]$azureImageLocation,
    [bool]$overwriteVHD,
    [bool]$overwriteImage,
    [string]$storageAccount,
    [string]$token)
{
    #Ensure that VM is off
    $startTime = Get-Date
    $currentTime = Get-Date
    $timeSpan = New-TimeSpan -Start $startTime -End $currentTime

    While((Get-VM $vmName).state -ne "Off" -and $timeSpan.TotalMinutes -lt 15)
    {
        Start-Sleep -Seconds 60
        $currentTime = Get-Date
        $timeSpan = New-TimeSpan -Start $startTime -End $currentTime
    }

    if((Get-VM $vmName).state -ne "Off")
    {
        Write-Host "Timeout. VM failed to switch off" -ForegroundColor Red
        #return
        try
        {
            Stop-VM -Name (Get-VM $vmName).name -TurnOff -ErrorAction Stop
        }
        catch
        {
            Write-Host "Failed to force shutdown VM $vmName" -ForegroundColor Red
        }
    }
    else
    {
        Write-Host "VM $vMName is confirmed off" -ForegroundColor Green
    }

    try
    {
        #Preparing Base VHD
        Write-Host "Preparing $vhdName as Base VHD"

        # Convert VHD to fixed size
        $preparedVHDPath = $vhdPath + "\" + $vhdName
        $fixedVHDPath = $vhdPath + "\fixed\" +$vhdName
        Convert-VHD -VHDType Fixed -Path $preparedVHDPath -DestinationPath $fixedVHDPath

        # Resize to 90GB
        $fileSizeBeforeResize = (Get-Item $fixedVHDPath).length
        Resize-VHD -Path $fixedVHDPath -SizeBytes 90GB        
        $fileSizeAfterResize = (Get-Item $fixedVHDPath).length

        if($fileSizeAfterResize -lt 90GB -and $fileSizeAfterResize -eq $fileSizeBeforeResize)
        {
            Write-Host "Could not resize VHD" -ForegroundColor Red
            return
        }

        # Upload Base VHD
         Write-Host "Uploading Base VHD..."
         $outputFile = ".\azcopy.zip"  
         $destinationFolder = ".\azcopy"
         Expand-Archive -Path $outputFile -DestinationPath $destinationFolder -Force  
         $urlOfUploadedImageVhd = $VHDUploadUrl + $baseVHDName
         $token="?"+ $token
         $urlOfUploadedImageVhdwithtoken="$urlOfUploadedImageVhd" + "$token"
         write-host " ./azcopy/azcopy.exe copy  $fixedVHDPath   $urlOfUploadedImageVhdwithtoken" -ForegroundColor Green
         ./azcopy/azcopy.exe copy $fixedVHDPath $urlOfUploadedImageVhdwithtoken --recursive                                                   
        
        # Create Azure image
        Write-Host "Creating Azure Image...: $azureImageName"

        $imageConfig = New-AzImageConfig -Location $azureImageLocation  #-HyperVGeneration "V2"
        $imageConfig = Set-AzImageOsDisk -Image $imageConfig -OsType 'Windows' -OsState 'Generalized' -BlobUri $urlOfUploadedImageVhd
        write-host "Set-AzImageOsDisk -Image $imageConfig -OsType 'Windows' -OsState 'Generalized' -BlobUri $urlOfUploadedImageVhd"
        New-AzImage -Image $imageConfig -ImageName $azureImageName -ResourceGroupName $resourceGroupName

        $azureImage = Get-AzImage -ResourceGroupName $resourceGroupName -ImageName $azureImageName -ErrorAction SilentlyContinue
        if($null -eq $azureImage -or $azureImage -eq "")
        {
            Write-Host "Unable to create Azure Image: $azureImageName " -ForegroundColor Red
            return
        }
        
        $Global:stageCompleted = 4
        Write-Host "Create Azure Image completed" -ForegroundColor Green
    }
    catch
    {
        Write-Error "Could not create azure image: "$_
    }
}

Install-Module -Name Az -AllowClobber
Import-Module -Name Az
write-host "Create-AzureImage $vmName $vhdPath $vhdName $baseVHDName $azureImageName $resourceGroupName $VHDUploadUrl $azureImageLocation $overwriteVHD $overwriteImage $storageAccount $token"
Create-AzureImage $vmName $vhdPath $vhdName $baseVHDName $azureImageName $resourceGroupName $VHDUploadUrl $azureImageLocation $overwriteVHD $overwriteImage $storageAccount $token