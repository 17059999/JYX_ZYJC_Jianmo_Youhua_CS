#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Modify-RegistryDefaultValue.ps1
## Purpose:        Modify the default value of a registry path.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$path,
[string]$defaultValue
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Modify-RegistryDefaultValue.ps1] ..." -foregroundcolor cyan
Write-Host "`$path = $path"
Write-Host "`$defaultValue = $defaultValue"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Modify the default value of a registry path."
    Write-host "Parm1: Path of the registry item. (Required)"
    Write-host "Parm2: Default value. (Required)"
    Write-host
    Write-host "Example: .\Modify-RegistryDefaultValue.ps1  HKLM:\Software\Microsoft\WindowsNT myNewValue"
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
if ($path -eq $null -or $path -eq "")
{
    Throw "Parameter `$path is required."
}
if ($defaultValue -eq $null -or $defaultValue -eq "")
{
    Throw "Parameter `$defaultValue is required."
}

#----------------------------------------------------------------------------
# Modify the registry default value
#----------------------------------------------------------------------------
set-item -path $path -value $defaultValue

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Modify-RegistryDefaultValue.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Modify-RegistryDefaultValue.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Yellow
