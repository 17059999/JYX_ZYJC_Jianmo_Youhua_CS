#############################################################################
## Copyright (c) Microsoft. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.
##
#############################################################################

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$sourcePath = Split-Path $scriptPath -Parent
$winFullPath = $sourcePath+"\install.wim:4"
$env:Path += ";$scriptPath;$scriptPath\Scripts"

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
[string]$logFile = $MyInvocation.MyCommand.Path + ".log"
Start-Transcript -Path "$logFile" -Append -Force

#----------------------------------------------------------------------------
# Install Windows SMBv1 Feature
#----------------------------------------------------------------------------
Write-Info.ps1 "Install Windows SMBv1 Feature"

Write-Info "Check FS-SMB installed" Client
$SMBState = Get-WindowsFeature FS-SMB1
if ($SMBState.Installstate -ne "Installed")
{
    Add-WindowsFeature FS-SMB1 -IncludeAllSubFeature -IncludeManagementTools -Source "WIM:$winFullPath" 
}

#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
Write-Info.ps1 "Completed install SMBv1 feature."
Stop-Transcript
exit 0