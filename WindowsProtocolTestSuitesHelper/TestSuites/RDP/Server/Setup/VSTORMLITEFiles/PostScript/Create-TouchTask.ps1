# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$rootPath = Split-Path $MyInvocation.MyCommand.Definition -Parent
$TouchTestPath = "$rootPath\Tools\TouchTest"
$scriptsPath ="$rootPath\Scripts"

Push-Location $scriptsPath
#----------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
Start-Transcript -Path "$rootPath\Create-TouchTask.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
$settingFile = "$scriptsPath\ParamConfig.xml"
$userNameInTC = .\Get-Parameter.ps1 $settingFile userNameInTC
$workgroupDomain = .\Get-Parameter.ps1 $settingFile workgroupDomain
$domainName   = .\Get-Parameter.ps1 $settingFile domainName

#----------------------------------------------------------------------------
# Create Windows tasks for RDPEI SUT control adapter
#----------------------------------------------------------------------------
$taskUser= $userNameInTC
if ($workgroupDomain.ToUpper() -eq "DOMAIN")
{
    $taskUser = "$domainName\$taskUser"
}
Write-Host "Creating task to trigger trigger one touch event on SUT..."
cmd /c schtasks /Create /RU $taskUser /SC Weekly /TN TriggerOneTouchEvent /TR "$TouchTestPath\TouchTest.exe SingleTouch" /IT /F

Write-Host "Creating task to trigger trigger multitouch event on SUT..."
cmd /c schtasks /Create /RU $taskUser /SC Weekly /TN TriggerMultiTouchEvent /TR "$TouchTestPath\TouchTest.exe MultiTouch" /IT /F

Write-Host "Creating task to trigger trigger position specified single touch event on SUT..."
cmd /c schtasks /Create /RU $taskUser /SC Weekly /TN TriggerSingleTouchPositionEvent /TR "$TouchTestPath\TouchTest.exe SingleTouchPosition" /IT /F

Write-Host "Creating task to trigger trigger continuous touch event on SUT..."
cmd /c schtasks /Create /RU $taskUser /SC Weekly /TN TriggerContinuousTouchEvent /TR "$TouchTestPath\TouchTest.exe ContinuousTouch" /IT /F

Write-Host "Creating task to trigger trigger continuous touch event on SUT..."
cmd /c schtasks /Create /RU $taskUser /SC Weekly /TN TriggerTouchHoverEvent /TR "$TouchTestPath\TouchTest.exe Hover" /IT /F

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript
popd
exit 0