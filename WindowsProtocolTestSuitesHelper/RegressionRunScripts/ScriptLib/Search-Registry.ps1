#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Search-Registry.ps1
## Purpose:        Search registry key for specified pattern, in specified path.
## Version:        1.0 (14 Jul, 2009)
##
##############################################################################

param(
[string]$path = $null,
[string]$pattern = $null
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Search-Registry.ps1] ..." -foregroundcolor cyan
Write-Host "`$path = $path"
Write-Host "`$pattern = $pattern"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Search registry key in specified path for specified pattern."
    Write-host "This function has the same behavior as Ctrl+F in Registry Editor."
    Write-host
    Write-host "Usage: .\Search-Registry.ps1 -path <path> -pattern <pattern> [-nk] [-nv] [-nd] [-w]"
    Write-host
    Write-host "Parm1: path, Registry path of the search root. (Required)"
    Write-host "Parm2: pattern, the seeking text. Support regular expression but not wildcard. (Required)"
    Write-host "Parm3: -nk, not to search in registry key names. (Optional)"
    Write-host "Parm4: -nv, not to search in registry value names. (Optional)"
    Write-host "Parm5: -nd, not to search in registry value data. (Optional)"
    Write-host "Parm6: -w, to match whole string only. (Optional)"
    Write-host
    Write-host "Return: Returns an array of objects, each object has following properties: "
    Write-host "       [Microsoft.PowerShell.Commands.Internal.TransactedRegistryKey]Key, the registry key;"
    Write-host "       [System.String]Name, the registry key's value name, can be null;"
    Write-host "       [System.Type]Type, the registry key's value type, can be null;"
    Write-host "       [System.Object]Data, the registry key's value data, can be null."
    Write-host
    Write-host "Examples: "
    Write-host "       .\Search-Registry.ps1 hklm:\system\ControlSet001\Control\Network `"File and Printer`""
    Write-host "       .\Search-Registry.ps1 hklm:\system\CurrentControlSet\Control\ Network -nv -nd -w"
    Write-host
    Write-host "Note: This script is not performance optimized yet. If you search in a path contains "
    Write-host "      too many subkeys, such as`"\HKLM:\Software\`" or `"HKLM:\system`", PS might crash."
    Write-host "      Please restrict your searching scope of path."
    Write-host 
}

function Compare-Pattern ([string] $compared)
{
    if ($matchWholeStringOnly)
    {
        return ($compared -like $pattern);
    }
    else
    {
        return ($compared -match $pattern);
    }
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
if ($pattern -eq $null -or $pattern -eq "")
{
    Throw "Parameter `$pattern is required."
}

#----------------------------------------------------------------------------
# Process optional arguments
#----------------------------------------------------------------------------
$lookAtKeys = $true;
$lookAtValues = $true;
$lookAtData = $true;
$matchWholeStringOnly = $false;

foreach ($arg in $args)
{
    if ($arg -eq "-nk")
    {
        $lookAtKeys = $false;
    }

    if ($arg -eq "-nv")
    {
        $lookAtValues = $false;
    }

    if ($arg -eq "-nd")
    {
        $lookAtData = $false;
    }

    if ($arg -eq "-w")
    {
        $matchWholeStringOnly = $true;
    }
}

Write-Host "`$lookAtKeys = $lookAtKeys"
Write-Host "`$lookAtValues = $lookAtValues"
Write-Host "`$lookAtData = $lookAtData"
Write-Host "`$matchWholeStringOnly = $matchWholeStringOnly"

#----------------------------------------------------------------------------
# Search the registry
#----------------------------------------------------------------------------

$results = @();

if ((!$lookAtKeys) -and (!$lookAtValues) -and (!$lookAtData))
{
    Write-Host "Warning: all switchs '-nk -nv -nd' are set, that means the search will"
    Write-Host "         neither to look at keys, values nor data. No result will be found!"
    
    return $results;
}

#if ($path.Provider.Name -ne "Registry") { throw "$path is not valid registry path." }

$keys = @(Get-Item $path -ErrorAction SilentlyContinue) `
        + @(Get-ChildItem -recurse $path -ErrorAction SilentlyContinue);

foreach ($key in $keys) {

    $keyLeafName = Split-Path -Leaf $key.Name -ErrorAction SilentlyContinue;
    
    if ($lookAtKeys -and (Compare-Pattern -compared $keyLeafName))
    {
        Write-Host "Matched registry key found:"
        Write-Host "    Key = $($key.Name)"
        $r = @{};
        $r.Key = $key;
        $results += $r;
    }

    foreach ($valueName in $key.GetValueNames()) {
        $valueData = $key.GetValue($valueName);
        
        $found = $false;
        
        if ($lookAtValues -and (Compare-Pattern -compared $valueName))
        {
            Write-Host "Matched registry key value name found:"
            $found = $true;
        }
            
        if (!$found -and $lookAtData -and (Compare-Pattern -compared $valueData))
        {
            Write-Host "Matched registry key value data found:"
            $found = $true;
        }
        
        if ($found) {
            $r = @{};
            $r.Key = $key;
            $r.Name = $valueName;
            $r.Type = $valueData.GetType();
            $r.Data = $valueData;

            Write-Host "    Key = $($r.key.Name)"
            Write-Host "    Value Name = $($r.Name)"
            Write-Host "    Value Type = $($r.Type)"
            Write-Host "    Value Data = $($r.Data)"

            $results += $r;
        }
    }
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Search-Registry.ps1] SUCCEED." -foregroundcolor Green
return $results
