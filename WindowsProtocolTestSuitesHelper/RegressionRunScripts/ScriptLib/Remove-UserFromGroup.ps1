#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Remove-UserFromGroup.ps1
## Purpose:        Remove a user from specified group.
## Version:        1.2 (18 July, 2008)
##           
##############################################################################

Param (
$userName, 
$groupName = "Administrators", 
$groupType = "Workgroup"
)

Write-Host "EXECUTING [Remove-UserFromGroup.ps1]..." -foregroundcolor cyan
Write-Host "`$userName  = $userName"
Write-Host "`$groupName = $groupName"
Write-Host "`$groupType = $groupType"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script removes a user from specified group."
    Write-host
    Write-host "First Parameter              `t:User Name  : The name of the User to move."
    Write-host "Second Parameter             `t:Group Name : The name of the group to move out. (By default is Administrators group)"
    Write-host "Third Parameter              `t:Group Type : The type of the group to move out. (Domain or Workgroup, by default is Workgroup)"
    Write-host
    Write-host "Example: Remove-UserFromGroup user1 Administrators"
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
    Throw "Parameter userName is required."
}

#----------------------------------------------------------------------------
#Function: ExecuteLocalCommand
#
#Usage   : Execute command on cmd console
#----------------------------------------------------------------------------

function ExecuteLocalCommand([string] $command)
{
    cmd.exe /c $command 2>&1 | Write-Host
}

if ($groupType -eq "Workgroup")
{
    $strCommand = "net.exe localgroup $groupName $userName /DELETE"
}
elseif ($groupType -eq "Domain")
{
    $strCommand = "net.exe group $groupName $userName /DELETE /Domain"
}
else
{
    Throw "Unsupported group type:$groupType. The value must be Domain/Workgroup."
}

ExecuteLocalCommand($strCommand)

Write-Host "EXECUTE [Remove-UserFromGroup.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

return 0
