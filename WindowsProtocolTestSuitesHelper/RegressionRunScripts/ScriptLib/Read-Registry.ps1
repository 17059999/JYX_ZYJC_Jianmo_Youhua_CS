#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Read-Registry.ps1
## Purpose:        Show name and value of registry items in the path.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$path,
[string]$name = $null
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Read-Registry.ps1] ..." -foregroundcolor cyan
Write-Host "`$path = $path"
Write-Host "`$name = $name"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Show name and value of registry items in the path."
    Write-host "Parm1: Path of the registry item. (Required)"
    Write-host "Parm2: Registry item name. (Optional)"
    Write-host
    Write-host "Example: .\Read-Registry.ps1  HKLM:\Software\Microsoft\WindowsNT myItemName "
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
# Read the registry
#----------------------------------------------------------------------------
$result = get-itemproperty -path $path -name $name

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Read-Registry.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Read-Registry.ps1] SUCCEED." -foregroundcolor Green
return $result
