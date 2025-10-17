#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           New-Volume.ps1
## Purpose:        Create a new Voluem with a answer file.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$DiskpartScript
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [New-Volume.ps1]..." -foregroundcolor cyan
Write-Host "`$DiskpartScript = $DiskpartScript" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This scripts will new a Voluem with a answer file."
    Write-host
    Write-host "Example: New-Volume.ps1 pathofanswerfile."
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
if ($DiskpartScript -eq $null -or $DiskpartScript -eq "")
{
    Throw "Parameter `$DiskpartScript is required."
}

#----------------------------------------------------------------------------
# EXECUTION
#----------------------------------------------------------------------------
Write-Host "New a volume with the answer file: $DiskpartScript" 
diskpart /s $DiskpartScript

#----------------------------------------------------------------------------
# VERIFY THE RESULT
#----------------------------------------------------------------------------
Write-Host "Verifying [New-Volume.ps1]..." -foregroundcolor yellow
Write-Host "EXECUTE [New-Volume.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit
