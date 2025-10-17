#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Modify-Registry.ps1
## Purpose:        Modify a registry value of a path, which name is specified.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$path,
[string]$name,
[string]$value
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Modify-Registry.ps1] ..." -foregroundcolor cyan
Write-Host "`$path = $path"
Write-Host "`$name = $name"
Write-Host "`$value = $value"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Modify a registry value of a path, which name is specified."
    Write-host "Parm1: Path of the registry item. (Required)"
    Write-host "Parm2: Registry item name. (Required)"
    Write-host "Parm3: Registry item value. (Required)"
    Write-host
    Write-host "Example: .\Modify-Registry.ps1  HKLM:\Software\Microsoft\WindowsNT myItemName myValue"
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
if ($name -eq $null -or $name -eq "")
{
    Throw "Parameter `$name is required."
}
if ($value -eq $null -or $value -eq "")
{
    Throw "Parameter `$value is required."
}

#----------------------------------------------------------------------------
# Modify the registry
#----------------------------------------------------------------------------
set-itemproperty -path $path -name $name -value $value

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Modify-Registry.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Modify-Registry.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Yellow
