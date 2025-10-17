#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Convert-IPAddress.ps1
## Purpose:        Convert IP address from version 4 to version 6
## Version:        1.0 (2 June, 2011)
## Requirements:   Windows Powershell 2.0
##
##############################################################################

param(
[string]$IPAddress
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Convert-IPAddress.ps1] ..." -foregroundcolor cyan
Write-Host "`$IPAddress		= $IPAddress"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Convert IP address from version 4 to version 6."
    Write-host
    Write-host "Example : Convert-IPAddress.ps1 192.168.0.1"
	Write-host "Return value: 2008::1"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#----------------------------------------------------------------------------
# Convert IP address
#----------------------------------------------------------------------------
$newIP = "2008::"
$IP = [System.Net.IPAddress]::Parse($IPAddress)
$IPBytes = $IP.GetAddressBytes()
$index = $IPBytes[$IPBytes.length-1].ToString("X2")
$newIP = $newIP+$index

return $newIP