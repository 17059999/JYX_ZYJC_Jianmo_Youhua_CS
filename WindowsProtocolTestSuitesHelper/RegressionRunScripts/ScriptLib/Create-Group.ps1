#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-Group.ps1
## Purpose:        Create a user group with specified name and specified group type (Domain or Workgroup).
## Version:        1.1 (26 June, 2008)
##           
##############################################################################

Param (
$strGroupName, 
$groupType
)

Write-Host "EXECUTING [Create-Group.ps1]..." -foregroundcolor cyan
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
    Write-host "First Parameter              `t:Group Name : The name of the group that you want to create."
    Write-host "Second Parameter             `t:Group Type : The type of the group that you want to create. (Domain or Workgroup)"
    Write-host
    Write-host "Example: Create-Group.ps1 mynewgroup Domain"
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
if ($strGroupName -eq $null -or $strGroupName -eq "")
{
    Throw "Parameter strGroupName is required."
}
if ($groupType -eq $null -or $groupType -eq "")
{
    Throw "Parameter groupType is required."
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

if ($groupType -eq "Domain")
{
    $strCommandAddGroup = "net.exe group $strGroupName /ADD"
}
elseif ($groupType -eq "Workgroup")
{
    $strCommandAddGroup = "net.exe localgroup $strGroupName /ADD"
}
else
{
    Throw "Unsupported group type:$groupType. The value must be Domain/Workgroup."
}

ExecuteLocalCommand($strCommandAddGroup)

Write-Host "EXECUTE [Create-Group.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

return 0
