#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-DomainOrWorkgroupName.ps1
## Purpose:        Get current computer system's domain or workgroup name.
## Version:        1.0 (25 Jun, 2008)
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-DomainOrWorkgroupName.ps1] ..." -foregroundcolor cyan

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Get current computer system's domain or workgroup name."
    Write-host
    Write-host "Example: `$name = Get-DomainOrWorkgroupName.ps1"
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
# Get domain or workgroup name
#----------------------------------------------------------------------------
$computerSysObj = Get-WmiObject -Class Win32_ComputerSystem
if ($computerSysObj -eq $null)
{
    Throw "Error: Cannot get WMI Object. EXECUTE [Get-DomainOrWorkgroupName.ps1] FAILED."
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Get-DomainOrWorkgroupName.ps1] SUCCEED." -foregroundcolor green
return $computerSysObj.Domain

