#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           [SCRIPT FILENAME].ps1
## Purpose:        [DESCRIPTION OF THIS SCRIPT]
## Version:        1.0 ([DATE ex. 18 May, 2008])
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: [DESCRIPTION]"
    Write-host
    Write-host "Example: [EXAMPLE]"
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

Write-Host "[STATEMENT OF BEGIN EXECUTION ...]" -foregroundcolor Yellow

#----------------------------------------------------------------------------
# [COMMENTS]
#----------------------------------------------------------------------------
[EXECUTION]

#----------------------------------------------------------------------------
# [COMMENTS]
#----------------------------------------------------------------------------
[VERIFY THE RESULT]


Write-Host "[STATEMENT OF END EXECUTION, VERIFIED OR NOT .]" -foregroundcolor Green

exit
