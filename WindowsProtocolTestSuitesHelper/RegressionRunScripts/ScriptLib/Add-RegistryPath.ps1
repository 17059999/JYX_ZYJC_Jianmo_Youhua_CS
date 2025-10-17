#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Add-RegistryPath.ps1
## Purpose:        Add a registry path.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$path
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Add-RegistryPath.ps1] ..." -foregroundcolor cyan
Write-Host "`$path = $path"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Add a registry path."
    Write-host "Parm1: Path of the registry. (Required)"
    Write-host
    Write-host "Example: .\Add-RegistryPath.ps1  HKLM:\Software\Microsoft\WindowsNT"
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
# Add the registry path
#----------------------------------------------------------------------------
new-item -path $path

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Add-RegistryPath.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Add-RegistryPath.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Yellow
