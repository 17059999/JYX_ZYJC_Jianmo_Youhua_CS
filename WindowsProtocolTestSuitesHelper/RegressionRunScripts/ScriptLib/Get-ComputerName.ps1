#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-ComputerName.ps1
## Purpose:        Retrive the IP address (IPv4 or IPv6) according to the VM role and index.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(

[String]$role, 
[int]$index
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-ComputerName.ps1] ..." -foregroundcolor cyan
Write-Host "`$role      = $role"
Write-Host "`$index     = $index"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script will retrive the Computer Name according to the VM name"
    Write-host
    Write-host "Example: Get-ComputerName.ps1 SUT 1"
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
if ($role -eq $null -or $role -eq "")
{
    Throw "Parameter role is required."
}
if ($index -eq $null -or $index -eq "")
{
    Throw "Parameter index is required."
}

#----------------------------------------------------------------------------
# Get full Name
#----------------------------------------------------------------------------
$nameIndex = [System.String]::Format("{0:D2}", $index)
$retVal = $role + $nameIndex

Write-Host "The Computer Name is $retVal."

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Get-ComputerName.ps1] FINISHED."

return $retVal