#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           [SCRIPT FILENAME].ps1
## Purpose:        [DESCRIPTION OF THIS SCRIPT]
## Version:        1.0 ([DATE ex. 18 May, 2008])
##
##############################################################################

param(
[string]$computerName,
[string]$usr,
[string]$pwd
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [SCRIPT FILENAME].ps1 ..." -foregroundcolor cyan
[DUMP ALL THE PARAMETER VALUE HERE]
[PAY ATTENTION TO THE FORMAT]
Write-Host "`$computerName = $computerName"
Write-Host "`$user         = $user"
Write-Host "`$password     = $password"

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

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ([PARAM ex. $computerName] -eq $null -or [PARAM ex. $computerName] -eq "")
{
    Throw "Parameter [PARAM ex. $computerName] is required."
}
[MORE PARAMETER VERIFICATION]

#----------------------------------------------------------------------------
# Using global username/password when caller doesnot provide.
#----------------------------------------------------------------------------
if ($usr -eq $null -or $usr -eq "")
{
    $usr = $global:usr
    $pwd = $global:pwd
}
#----------------------------------------------------------------------------
# Make username prefixed with domain/computername
#----------------------------------------------------------------------------
if ($usr.IndexOf("\") -eq -1)
{
    if ($global:domain  -eq $null -or $global:domain -eq "")
    {
        $usr = "$computerName\$usr"
    }
    else
    {
        $usr = "$global:domain\$usr"
    }
}

Write-Host "[STATEMENT OF BEGIN EXECUTION, IT'S BETTER TO SHOW PARAMETERS HERE ...]" -foregroundcolor Yellow

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
