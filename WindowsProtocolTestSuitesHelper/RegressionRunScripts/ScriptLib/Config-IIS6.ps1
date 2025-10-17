#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Config-IIS6.ps1
## Purpose:        Config IIS 6.0 with the config file.
## Version:        1.0 (14 July, 2008)
##
##############################################################################

param(
[string]$configFilePath = $null,
[string]$vDPath = $null
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-IIS6.ps1] ..." -foregroundcolor cyan
Write-Host "`$configFilePath = $configFilePath"
Write-Host "`$vDPath         = $vDPath"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Config IIS 6.0 with a config file"
    Write-host
    Write-host "Example: Config_IIS6.ps1 c:\config.xml /LM/W3SVC/1/ROOT/WebDav"
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
if ($configFilePath -eq $null -or $configFilePath -eq "")
{
    Throw "the path of config file is required."
}

if ($vDPath -eq $null -or $vDPath -eq "")
{
    Throw "the path of virtual directory is required."
}

$configFileExist = Test-Path -Path $configFilePath

if($configFileExist -eq $False)
{
    Throw "the path of config file is invalid."
}

#----------------------------------------------------------------------------
# Config the IIS 6 ith the config file
#----------------------------------------------------------------------------
cmd.exe /c "cscript //H:CScript"  2>&1 | Write-Host
cmd.exe /c "iiscnfg /import /f $configFilePath /sp $vDPath /dp $vDPath /merge"  2>&1 | Write-Host

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Config-IIS6.ps1] Finished(NO VERIFIED)." -foregroundcolor green

exit


