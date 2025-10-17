#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Disable-Account.ps1
## Purpose:        Disable a user account (Domain user or Local user).
## Version:        1.1 (26 June, 2008)
##
##############################################################################

Param (
[string]$AccountName,
[string]$AccountType
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Disable-Account.ps1]." -foregroundcolor cyan
Write-Host "`$AccountName = $AccountName"
Write-Host "`$AccountType = $AccountType"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script disables an account (Domain or Workgroup)."
    Write-host
    Write-host "First Parameter              `t:Account Name   : The name of the user account you want to disable."
    Write-host "Second parameter             `t:Account Type   : The type of the user account. (Domain or Workgroup)" 
    Write-host
    Write-host "Example: Disable-Account.ps1 Guest Domain"
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
# Check Parameter AccountType
#----------------------------------------------------------------------------
function AccountTypeCheck($AccountType)
{
    if (($AccountType -eq "Domain") -or ($AccountType -eq "Workgroup"))
    {
        return $True
    }
    else
    {
        Throw "Parameter AccountType is unlegal, only Domain or Workgroup is accredited."
    }
}

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($AccountName -eq $null -or $AccountName -eq "")
{
    Throw "Parameter AccountName is required."
}
if ($AccountType -eq $null -or $AccountType -eq "")
{
    Throw "Parameter AccountType is required."
}

#----------------------------------------------------------------------------
# Execute Local Command
#----------------------------------------------------------------------------
function ExecuteLocalCommand([string] $command)
{
    cmd.exe /c $command 2>&1 | Write-Host
}

#----------------------------------------------------------------------------
# Combine Local Command by Parameter AccountType
#----------------------------------------------------------------------------
[string]$CommandAddGroup = ""
if (($AccountType -eq "Domain"))
{
    $CommandAddGroup = "net.exe user $AccountName /active:no /Domain"
}
elseif ($AccountType -eq "Workgroup")
{
    $CommandAddGroup = "net.exe user $AccountName /active:no"
}
else
{
    Throw "Parameter AccountType is unlegal, only 'Domain' or 'Workgroup' is accredited."
}

ExecuteLocalCommand($CommandAddGroup)

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Disable-Account.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

return 0