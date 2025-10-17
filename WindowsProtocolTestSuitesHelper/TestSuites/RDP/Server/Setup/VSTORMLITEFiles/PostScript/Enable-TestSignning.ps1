# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$rootPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$DSFPath = "$rootPath\Tools\DSF"
#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
Start-Transcript -Path "$rootPath\Enable-TestSignning.ps1.log" -Append -Force
 
#-------------------------------------
# Enable testsigning
#-------------------------------------
Write-Host "Enable testsigning: cmd /c Bcdedit /set testsigning on"
cmd /c Bcdedit /set testsigning on  

#-------------------------------------
# Install certification
#-------------------------------------
Write-Host "Install certification: MSFTTestRoot.cer"
cmd /c certutil -addstore "Root" "$DSFPath\MSFTTestRoot.cer" > addcert.log

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript

exit 0



