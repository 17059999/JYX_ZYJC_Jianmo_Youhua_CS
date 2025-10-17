#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Remove-RegistryPath.ps1
## Purpose:        Remove a registry path.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$path
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Remove-RegistryPath.ps1] ..." -foregroundcolor cyan
Write-Host "`$path = $path"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Remove a registry path."
    Write-host "Parm1: Path of the registry item. (Required)"
    Write-host
    Write-host "Example: .\Remove-RegistryPath.ps1  HKLM:\Software\Microsoft\WindowsNT"
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

#----------------------------------------------------------------------------
# Remove the registry path
#----------------------------------------------------------------------------
remove-item -path $path

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Remove-RegistryPath.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Remove-RegistryPath.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Yellow
