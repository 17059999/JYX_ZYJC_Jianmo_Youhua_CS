#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Restart-CertificateAuthority.ps1
## Purpose:        Restart the Certificate Service.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Restart-CertificateAuthority.ps1] ..." -foregroundcolor cyan

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Restart the Certificate Service."
    Write-host
    Write-host "Example: Restart-CertificateAuthority.ps1"
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
# Restart Certificate Service
#----------------------------------------------------------------------------
Write-Host "Restart Certificate Service ..."
net.exe stop certsvc
net.exe start certsvc 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Restart-CertificateAuthority.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Restart-CertificateAuthority.ps1] SUCCEED." -foregroundcolor Green
