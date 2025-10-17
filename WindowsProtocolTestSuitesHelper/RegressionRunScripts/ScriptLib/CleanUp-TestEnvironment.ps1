#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           CleanUp-TestEnvironment.ps1
## Purpose:        Cleans up the test machine, such as delete all the existing VMs.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [CleanUp-TestEnvironment.ps1] ..." -foregroundcolor cyan

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script cleans up the test machine. Remove any Hyper-V virtual machine."
    Write-host
    Write-host "Example: CleanUp-TestEnvironment.ps1"
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
# Clean Up
#----------------------------------------------------------------------------
Write-Host "Begin to clean up the test environment..." 

net.exe use * /del /y

#.\Delete-AllVM.ps1
.\TurnOff-AllVM.ps1

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "Environment clean up finished." -foregroundcolor Green
#Write-Host "EXECUTE [CleanUp-TestEnvironment.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit