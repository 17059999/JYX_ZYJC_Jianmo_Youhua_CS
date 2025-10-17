#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Set-UserPassword.ps1
## Purpose:        Set a new password for a user(Domain user or Local user).
## Version:        1.1 (26 June, 2008)
##
##############################################################################

Param (
[string]$UserName, 
[string]$password,
[string]$accountType
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-UserPassword.ps1]." -foregroundcolor cyan
Write-Host "`$userName    = $UserName"
Write-Host "`$password    = $password"
Write-Host "`$accountType = $accountType"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script sets a new password for a use.(Domain or Workgroup)"
    Write-host
    Write-host "First Parameter              `t:User Name      : The name of the user account you want to change password."
    Write-host "Second Parameter             `t:User Password  : The new password of the user."
    Write-host "Third parameter              `t:Account Type   : The type of the user account. (Domain or Workgroup)" 
    Write-host
    Write-host "Example: Set-UserPassword.ps1 Administrator Password01! Domain"
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
if ($userName -eq $null -or $userName -eq "")
{
    Throw "Parameter UserName is required."
}
if ($password -eq $null -or $password -eq "")
{
    Throw "Parameter Password is required."
}
if ($accountType -eq $null -or $accountType -eq "")
{
    Throw "Parameter accountType is required."
}

#----------------------------------------------------------------------------
# Execute Local Command
#----------------------------------------------------------------------------
function ExecuteLocalCommand([string] $command)
{
    cmd.exe /c $command 2>&1 | Write-Host
}

#----------------------------------------------------------------------------
# Combine Local Command by Parameter accountType
#----------------------------------------------------------------------------
[string]$CommandAddGroup = ""
if ($accountType -eq "Domain")
{
    $CommandAddGroup = "net.exe user $userName $password /Domain"
}
elseif ($accountType -eq "Workgroup")
{
    $CommandAddGroup = "net.exe user $userName $password"
}
else
{
    Throw "Parameter accountType is unlegal, only Domain or Workgroup is accredited."
}

ExecuteLocalCommand($CommandAddGroup)

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Set-UserPassword.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

return 0
