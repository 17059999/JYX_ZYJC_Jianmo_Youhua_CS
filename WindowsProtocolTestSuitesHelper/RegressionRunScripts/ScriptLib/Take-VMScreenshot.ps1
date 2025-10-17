#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

param(
    [string]$VMName="TestVM",
    [string]$ScreenshotPath="D:\WinteropProtocolTesting\TestResults\FileSharing\VMScreenshots\VMScreenshot_$VMName.jpg",
    [int]$XResolution = 1024,
    [int]$YResolution = 768,
    [string]$HyperVHost = "localhost"
)

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath"
# Should not use Resolve-Path cmdlet, because it will throw exception if the path does not exist.
Write-Info.ps1 "Get VMScreenshotFullPath from $ScreenshotPath"
$VMScreenshotFullPath = [System.IO.Path]::GetFullPath($ScreenshotPath)
$parentPath = Split-Path "$VMScreenshotFullPath" -Parent
if(!(Test-Path $parentPath))
{
    CMD /C MKDIR $parentPath 2>&1 | Write-Info.ps1 
}

#----------------------------------------------------------------------------
# Load references
#----------------------------------------------------------------------------
[System.Reflection.Assembly]::LoadWithPartialName("System.Drawing")

#----------------------------------------------------------------------------
# Get VM and VMSettingData
#----------------------------------------------------------------------------
Write-Info.ps1 "Get VM object by name $VMName."
$Vm = Get-WmiObject -Namespace "root\virtualization\v2" -ComputerName $HyperVHost -Query "Select * From Msvm_ComputerSystem Where ElementName='$VMName'" 
if($vm -eq $null)
{
    Write-Info.ps1 "Cannot find VM by name $VMName."
    exit 1
}

Write-Info.ps1 "Get VM Setting Data."
$VMSettingData = Get-WmiObject -Namespace "root\virtualization\v2" -Query "Associators of {$Vm} Where ResultClass=Msvm_VirtualSystemSettingData AssocClass=Msvm_SettingsDefineState" -ComputerName $HyperVHost 
if($VMSettingData -eq $null)
{
    Write-Info.ps1 "Cannot retrieve VM setting data."
    exit 1
}

#----------------------------------------------------------------------------
# Take VM screenshot
#----------------------------------------------------------------------------
Write-Info.ps1 "Take VM screenshot."
$VMManagementService = Get-WmiObject -class "Msvm_VirtualSystemManagementService" -namespace "root\virtualization\v2" -ComputerName $HyperVHost 
$RawImageData = $VMManagementService.GetVirtualSystemThumbnailImage($VMSettingData, "$XResolution", "$YResolution")

#----------------------------------------------------------------------------
# Save VM screenshot
#----------------------------------------------------------------------------
Write-Info.ps1 "Save VM screenshot $VMScreenshotFullPath"
$VMThumbnail = New-object System.Drawing.Bitmap($XResolution, $YResolution, [System.Drawing.Imaging.PixelFormat]::Format16bppRgb565) 
$rectangle = New-object System.Drawing.Rectangle(0,0,$XResolution,$YResolution) 
[System.Drawing.Imaging.BitmapData] $VMThumbnailBitmapData = $VMThumbnail.LockBits($rectangle, [System.Drawing.Imaging.ImageLockMode]::WriteOnly, [System.Drawing.Imaging.PixelFormat]::Format16bppRgb565) 
[System.Runtime.InteropServices.marshal]::Copy($RawImageData.ImageData, 0, $VMThumbnailBitmapData.Scan0, $XResolution*$YResolution*2) 
$VMThumbnail.UnlockBits($VMThumbnailBitmapData); 
$VMThumbnail.Save("$vmScreenshotFullPath")

#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
exit 0