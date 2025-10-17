#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Config-VMSetting.ps1
## Purpose:        A dumy file which is reserved for VM config (before RunVM and after ImportVM).
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$VMName
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-VMSetting.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 

#-----------------------------------------------------------------------
# This is a placeholder for special VM configuration before starting
#------------------------------------------------------------------------

#[TODO Add your logic here]

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Config-VMSetting.ps1]..." -foregroundcolor yellow
Write-Host "EXECUTE [Config-VMSetting.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow
exit 0