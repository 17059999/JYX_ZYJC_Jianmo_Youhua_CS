#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath;$scriptPath\Scripts"

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
[string]$logFile = $MyInvocation.MyCommand.Path + ".log"
try { Stop-Transcript -ErrorAction SilentlyContinue } catch {} # Ignore Stop-Transcript error messages
Start-Transcript -Path "$logFile" -Append -Force

#-----------------------------------------------------
# Begin to install remote access
#-----------------------------------------------------
Import-Module ServerManager

Write-Info.ps1 "Begin to install RemoteAccess feature..."
Add-WindowsFeature RemoteAccess -IncludeAllSubFeature -IncludeManagementTools

#-----------------------------------------------------
# Begin to config routing function
#-----------------------------------------------------
Write-Info.ps1 "Begin to configure RemoteAccess service..."
CMD /C NETSH ras set type lanonly lanonly IPv4 2>&1 | Write-Info.ps1
CMD /C NETSH ras set conf ENABLED 2>&1 | Write-Info.ps1
CMD /C NET stop RemoteAccess 2>&1 | Write-Info.ps1
CMD /C NET start sstpsvc 2>&1 | Write-Info.ps1
CMD /C NET start rasman 2>&1 | Write-Info.ps1
CMD /C NET start wanarpv6 2>&1 | Write-Info.ps1
CMD /C SC config RemoteAccess start=Auto 2>&1 | Write-Info.ps1
CMD /C NET start RemoteAccess  2>&1 | Write-Info.ps1

Write-Info.ps1 "Retry to start RemoteAccess service until succeed."
$service = Get-Service -Name "RemoteAccess"
while($service.Status -ne "Running")
{
    Write-Info.ps1 "NET start RemoteAccess."
    CMD /C NET start RemoteAccess  2>&1 | Write-Info.ps1
    $service = Get-Service -Name "RemoteAccess"
    Sleep 5
}

#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
Write-Info.ps1 "Completed enable routing and remote access."
Stop-Transcript
exit 0