#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Convert-IPAddressToHexadecimalFormat.ps1
## Purpose:        Convert IPv4 Address to hexadecimal format.
## Version:        1.0 (7 Apr, 2009)
##
##############################################################################

Param(
[String]$IPAddress
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Convert-IPAddressToHexadecimalFormat.ps1] ..." -ForegroundColor cyan
Write-Host "`$IpAddress = $IpAddress"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Convert IP (both IPv4 and IPv6) address to hexadecimal format."
    Write-host
    Write-host "Example 1: Convert-IPAddressToHexadecimalFormat.ps1 192.168.0.1"
    Write-host "Example 2: Convert-IPAddressToHexadecimalFormat.ps1 2008::1"
    Write-host "Example 2: Convert-IPAddressToHexadecimalFormat.ps1 2008::1:192.168.0.1%1"
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
# Verify required parameters
#----------------------------------------------------------------------------
if ($IPAddress -eq $null -or $IPAddress -eq "")
{
    throw "Parameter IPAddress cannot be empty."
}

#----------------------------------------------------------------------------
# Start to convert
#----------------------------------------------------------------------------
[String]$retVal = "0x"

$IP = [System.Net.IPAddress]::Parse($IPAddress)
$IPBytes = $ip.GetAddressBytes()

foreach($IPByte in $IPBytes)
{
    $retVal = $retVal + $IPByte.ToString("X2")
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Convert-IPAddressToHexadecimalFormat.ps1] FINISHED (NOT VERIFIED)."

Return $retVal