#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Set-Gateway.ps1
## Purpose:        Set gateway address.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

Param(
[string]$gateway, 
[string]$gatewaymetric
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-Gateway.ps1] ... " -foregroundcolor cyan
Write-Host "`$Gateway       = $Gateway"
Write-Host "`$gatewaymetric = $gatewaymetric"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script will Set gateway address."
    Write-host
    Write-host "Example: Set-Gateway.ps1 192.168.0.1 9999"
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
if ($gateway -eq $null -or $gateway -eq "")
{
    Throw "Parameter Gateway is required."
}

if ($gatewaymetric -eq $null -or $gatewaymetric -eq "")
{
    Throw "Parameter Gatewaymetric is required."
}

#----------------------------------------------------------------------------
# Set gateway address.
#----------------------------------------------------------------------------
$errOccured = $False
$objNetAdptSet = Get-WmiObject Win32_NetworkAdapterConfiguration
foreach($objNetAdpt in $objNetAdptSet)
{
    if($objNetAdpt.IPEnabled -eq $True)
    {
        $errGateway = $objNetAdpt.SetGateways($gateway, $gatewaymetric)
        if($errGateway.ReturnValue -eq 0)
        {
            Write-Host "IP address and subnet mask are successfully set! No reboot required" -foregroundcolor Green
        }
        elseif($errGateway.ReturnValue -eq 1)
        {
            Write-Host "IP address and subnet mask are successfully set! Reboot required" -foregroundcolor Green
        }
        elseif($errGateway.ReturnValue -eq 71)
        {
            Write-Host "Invalid gateway IP address!" -foregroundcolor Red
            $errOccured = $True
        }
        elseif($errGateway.ReturnValue -eq 91)
        {
            Write-Host "Access denied!" -foregroundcolor Red
            $errOccured = $True
        }
        else
        {
            Write-Host "Unknown Failure! Gateway address set failed!" -foregroundcolor Red
            $errOccured = $True
        }
        
    }
}

if($errOccured -eq $True)
{
    Write-Host "Failed to set gateway address!" -foregroundcolor Red
}
else
{
    Write-Host "EXECUTE [Set-Gateway.ps1] SUCCEED." -foregroundcolor green
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Set-Gateway.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow
