#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Enable-WMIInRule.ps1
## Purpose:        Enable WMI-related inbound rules.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Enable-WMIInRule.ps1] ..." -foregroundcolor cyan

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Enable WMI-related inbound rules."
    Write-host
    Write-host "Example: .\Enable-WMIInRule.ps1 "
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
# Enable the WMI Inbound Rule
#----------------------------------------------------------------------------
netsh.exe advfirewall firewall set rule group="Windows Management Instrumentation (WMI)" new enable=yes 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Enable-WMIInRule.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Enable-WMIInRule.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Yellow
