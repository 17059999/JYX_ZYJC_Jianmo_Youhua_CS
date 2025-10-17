#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Add-UserToGroup.ps1
## Purpose:        Add a user to the specified user group.
## Version:        1.2 (18 July, 2008)
##           
##############################################################################

Param (
$strUserName, 
$strGroupName = "Administrators", 
$strGroupType = "Workgroup"
)

Write-Host "EXECUTING [Add-UserToGrop.ps1]..." -foregroundcolor cyan
Write-Host "`$strUserName  = $strUserName"
Write-Host "`$strGroupName = $strGroupName"
Write-Host "`$strGroupType = $strGroupType"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script adds a user to specified group."
    Write-host
    Write-host "First Parameter              `t:User Name  : The name of the User to join."
    Write-host "Second Parameter             `t:Group Name : The name of the group to join in. (By default is Administrators group)"
    Write-host "Third Parameter              `t:Group Type : The type of the group to join in. (Domain or Workgroup, by default is Workgroup)"
    Write-host
    Write-host "Example: Add-UserToGroup user1 Administrators"
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
if ($strUserName -eq $null -or $strUserName -eq "")
{
    Throw "Parameter strUserName is required."
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

if ($strGroupType -eq "Workgroup")
{
    $strCommandAddGroup = "net.exe localgroup $strGroupName $strUserName /ADD"
}
elseif ($strGroupType -eq "Domain")
{
    $strCommandAddGroup = "net.exe group $strGroupName $strUserName /ADD /Domain"
}
else
{
    Throw "Unsupported group type:$strGroupType. The value must be Domain/Workgroup."
}

ExecuteLocalCommand($strCommandAddGroup)

#if (ExecuteLocalCommand($strCommandAddGroup) -eq "The command completed successfully.")
#{
#    Write-Host "Add user to group successfully." -foregroundcolor Green
#}
#else
#{
#    Throw "Add user to group failed."
#}

Write-Host "EXECUTE [Add-UserToGrop.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

return 0
