#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Remove-Registry.ps1
## Purpose:        Remove a registry item from a path.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$path,
[string]$name
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Remove-Registry.ps1] ..." -foregroundcolor cyan
Write-Host "`$path = $path"
Write-Host "`$name = $name"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Remove a registry item from a path."
    Write-host "Parm1: Path of the registry item. (Required)"
    Write-host "Parm2: Registry item name. (Required)"
    Write-host
    Write-host "Example: .\Remove-Registry.ps1  HKLM:\Software\Microsoft\WindowsNT myItemName"
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

#----------------------------------------------------------------------------
# Remove the registry
#----------------------------------------------------------------------------
remove-itemproperty -path $path -name $name

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Remove-Registry.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Remove-Registry.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Yellow

