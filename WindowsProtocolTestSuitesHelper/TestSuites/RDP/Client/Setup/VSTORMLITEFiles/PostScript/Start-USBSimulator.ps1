# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$rootPath = Split-Path $MyInvocation.MyCommand.Definition -parent

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
try {
    Stop-Transcript -ErrorAction Continue | Out-Null
}
catch{}
Start-Transcript -Path "$rootPath\Start-USBRedirection.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Start a USB device simulator 
#----------------------------------------------------------------------------
Write-Host "Go to desktop (in case on Start Menu currently)" 
explorer.exe 
Start-Sleep -Milliseconds 1000

Write-Host "Start a USB device simulator" 
cmd /c 'start "simulator" cscript "%ProgramFiles%\dsf\USBLoopback\RunLoopbackSample.wsf"'
Start-Sleep -Milliseconds 2000

$shell = New-Object -ComObject WScript.Shell
Write-Host "Activate command window for USB device simulator!" 
$shell.AppActivate("simulator") | out-null
Start-Sleep -Milliseconds 500

Write-Host "Select Poll(P) mode!" 
$shell.Sendkeys("P")
Start-Sleep -Milliseconds 500
$shell.SendKeys( "{ENTER}" )

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript

exit 0