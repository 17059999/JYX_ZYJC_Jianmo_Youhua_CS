#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Read-RegistryPath.ps1
## Purpose:        Show all direct registry paths of the path.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$path
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Read-RegistryPath.ps1] ..." -foregroundcolor cyan
Write-Host "`$path = $path"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Show all direct registry paths of the path."
    Write-host "Parm1: Path of the registry item. (Required)"
    Write-host
    Write-host "Example: .\Read-RegistryPath.ps1  HKLM:\Software\Microsoft\WindowsNT"
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
# Read the registry path
#----------------------------------------------------------------------------
$result = get-childitem -path $path

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Read-RegistryPath.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Read-RegistryPath.ps1] SUCCEED." -foregroundcolor Green
return $result
