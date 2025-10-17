# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$rootPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$DSFPath = "$rootPath\Tools\DSF"
#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
try {
    Stop-Transcript -ErrorAction Continue | Out-Null
}
catch{}
Start-Transcript -Path "$rootPath\Install-DSF.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Install DSFInternal
#----------------------------------------------------------------------------
Write-Host "Install DSFInternal" 
cmd /c msiexec.exe /quiet /norestart /i "$DSFPath\DSFInternal.msi" EHCISIM=1 /log "$rootPath\DSFInstall.log"

#----------------------------------------------------------------------------
# Create simulated USB 2.0 EHCI controller
#----------------------------------------------------------------------------
Write-Host "Create simulated USB 2.0 EHCI controller" 
cmd /c $env:ProgramFiles\dsf\softehci\softehcicfgex.exe /install
Start-Sleep -Milliseconds 5000

#----------------------------------------------------------------------------
# Set Start-USBSimulator.ps1 auto start after reboot
#----------------------------------------------------------------------------
Write-Host "Update registry to set Start-USBSimulator.ps1 auto start after reboot" 
$startCommand = "cmd /c powershell `"$rootPath\Start-USBSimulator.ps1`"" 

New-ItemProperty HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run "Start-USBSimulator" -value "$startCommand" -PropertyType string -Force

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript

exit 0