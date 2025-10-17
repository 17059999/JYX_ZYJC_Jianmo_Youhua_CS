#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:       Delete-Group.ps1
## Purpose:    Delete specified group (Domain group or Local group).
## Version:    1.1 (26 June, 2008)
##       
##############################################################################

Param (
$strGroupName, 
$groupType
)

Write-Host "EXECUTING [Delete-Group.ps1]..." -foregroundcolor cyan
Write-Host "`$strGroupName = $strGroupName"
Write-Host "`$groupType    = $groupType"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script creates a new group with specified name.(Domain or Workgroup)"
    Write-host
    Write-host "First Parameter              `t:Group Name : The name of the group that you want to delete."
    Write-host "Second Parameter             `t:Group Type : The type of the group that you want to delete. (Domain or Workgroup)"
    Write-host
    Write-host "Example: Delete-Group.ps1 mynewgroup Domain"
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
if ($groupType -eq $null -or $groupType -eq "")
{
    Throw "Parameter groupType is required."
}

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function ExecuteLocalCommand([string] $command)
{
    cmd.exe /c $command 2>&1 | Write-Host
}

if (groupType -eq "Domain")
{
    $strCommandAddGroup = "net.exe group $strGroupName /DELETE"
}
elseif (groupType -eq "Workgroup")
{
    $strCommandAddGroup = "net.exe localgroup $strGroupName /DELETE"
}
else
{
    Throw "Unsupported group type:$groupType. The value must be Domain/Workgroup."
}

ExecuteLocalCommand($strCommandAddGroup)

Write-Host "EXECUTE [Delete-Group.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

return 0
