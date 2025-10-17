#############################################################################
## Copyright (c) Microsoft. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.
##############################################################################

Param(
[String]$FileName,
[String]$OldString,
[String]$NewString
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Replace-FileContent.ps1] ..." -foregroundcolor cyan

((Get-Content -path $fileName -Raw) -replace $OldString, $NewString) | Set-Content -Path $FileName
