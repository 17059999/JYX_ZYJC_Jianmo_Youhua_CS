# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$rootPath = Split-Path $MyInvocation.MyCommand.Definition -parent
#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
try {
   Stop-Transcript -ErrorAction SilentlyContinue | Out-Null
}
catch [System.InvalidOperationException] {}

Start-Transcript -Path "$rootPath\Set-StartCommand.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Set Start-USBSimulator.ps1 auto start after reboot
#----------------------------------------------------------------------------
Write-Host "Update registry to set Start-USBSimulator.ps1 auto start after reboot" 
$startCommand = "cmd /c powershell `"$rootPath\Start-Desktop.ps1`"" 

New-ItemProperty HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run "Start-USBSimulator" -value "$startCommand" -PropertyType string -Force

#Workaround for D3D TH2 client EGFX case failure. Should delete this line when bug fixed
New-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services\Client" -Name "EnableHardwareMode" -Value "0" -PropertyType DWORD -Force | Out-Null

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript

exit 0