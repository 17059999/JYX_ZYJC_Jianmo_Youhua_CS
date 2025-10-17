# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$rootPath = Split-Path $MyInvocation.MyCommand.Definition -Parent
$SetDisplayPath = "$rootPath\Tools\SetDisplay"

#----------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#----------------------------------------------------------------------------
try {
   Stop-Transcript -ErrorAction SilentlyContinue | Out-Null
}
catch [System.InvalidOperationException] {}
Start-Transcript -Path "$rootPath\Setup-VHID.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Set Screen Resolution
#----------------------------------------------------------------------------
$Width = 1024
$height = 768
$refreshRate = 60

if($IsWindows){
   cmd /c $SetDisplayPath\SetDisplay.exe /W $Width /H $height
}
elseif ($IsLinux) {
   bash $SetDisplayPath/XRandR.sh $Width $height $refreshRate
}
elseif ($IsMacOS) {
   # compile using gcc. it creates a new program called "SetDisplay"
   # call SetDisplay program with parameters: width height pixels and refreshrate
   gcc -O3 -Wall -o SetDisplay $SetDisplayPath/SetDisplayMac.c -framework Cocoa
   ./SetDisplay $Width $height 32 $refreshRate      
}

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript

exit 0
