# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$rootPath = Split-Path $MyInvocation.MyCommand.Definition -Parent
$VHIDPath = "$rootPath\Tools\VHidPen"

#----------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#----------------------------------------------------------------------------
try {
   Stop-Transcript -ErrorAction SilentlyContinue | Out-Null
}
catch [System.InvalidOperationException] {}
Start-Transcript -Path "$rootPath\Setup-VHID.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Install certificate
#----------------------------------------------------------------------------
cmd /c bcdedit -set loadoptions DDISABLE_INTEGRITY_CHECKS
cmd /c bcdedit -set TESTSIGNING ON

cmd /c certutil -f -addstore ROOT $VHIDPath\testroot-sha2.cer

#----------------------------------------------------------------------------
# Setup Virtual HID Digitizer Tablet
#----------------------------------------------------------------------------
Push-Location $VHIDPath
cmd /c .\devcon.exe install vhidpen.inf HID\VirtualHidTablet
popd

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript

exit 0
