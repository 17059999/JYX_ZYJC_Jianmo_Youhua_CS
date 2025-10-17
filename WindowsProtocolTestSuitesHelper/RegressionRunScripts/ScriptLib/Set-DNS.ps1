#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Set-DNS.ps1
## Purpose:        Set DNS address to specified machine.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

Param(
[string]$dnsAddress
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-DNS.ps1]... " -foregroundcolor cyan
Write-Host "`$dnsAddress = $dnsAddress"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script will set DNS address to specified machine."
    Write-host
    Write-host "Example: Set-DNS.ps1 192.168.0.1"
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
if ($dnsAddress -eq $null -or $dnsAddress -eq "")
{
    Throw "Parameter computerName is required."
}

$errOccured = $False
$objNetAdptSet = Get-WmiObject Win32_NetworkAdapterConfiguration
foreach($objNetAdpt in $objNetAdptSet)
{
    if($objNetAdpt.IPEnabled -eq $True)
    {
        Write-Host "Set DNS Address..."
        $errSetDNS = $objNetAdpt.SetDNSServerSearchOrder($dnsAddress)
        if($errSetDNS.ReturnValue -eq 0)
        {
            Write-Host "DNS address is successfully set! No reboot required" -foregroundcolor Green
        }
        elseif($errSetDNS.ReturnValue -eq 1)
        {
            Write-Host "DNS address is successfully set! Reboot required" -foregroundcolor Green
        }
        elseif($errSetDNS.ReturnValue -eq 91)
        {
            Write-Host "Access denied!" -foregroundcolor Red
            $errOccured = $True
        }
        else
        {
            Write-Host "Unknown Failure!" -foregroundcolor Red
            $errOccured = $True
        }

    }
}

if($errOccured -eq $True)
{
    Write-Host "Failed to set DNS address!" -foregroundcolor Red
}
else
{
    Write-Host "EXECUTE [Set-DNS.ps1] SUCCEED." -foregroundcolor green    
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Set-DNS.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow
