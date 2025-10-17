#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Config-VirtualDirectory.ps1
## Purpose:        Config a virtual directory in WebSite with the config file.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[string]$ConfigFilePath = $null,
[string]$VDPath = $null
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-VirtualDirectory.ps1] ..." -foregroundcolor cyan
Write-Host "`$ConfigFilePath = $ConfigFilePath"
Write-Host "`$VDPath         = $VDPath"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script used to config a virtual directory with a config file"
    Write-host
    Write-host "Example: Config_VirtualDirectory.ps1 config.xml /LM/W3SVC/1/ROOT/WebDav"
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
if ($ConfigFilePath -eq $null -or $ConfigFilePath -eq "")
{
    Throw "the path of config file is required."
}

if ($VDPath -eq $null -or $VDPath -eq "")
{
    Throw "the path of virtual directory is required."
}

$ConfigFileExist = Test-Path -Path $ConfigFilePath

if($ConfigFileExist -eq $False)
{
    Throw "the path of config file is invalid."
}

#----------------------------------------------------------------------------
# Config the Virtual Directory with the config file
#----------------------------------------------------------------------------
cmd.exe /c "cscript //H:CScript"
cmd.exe /c "iiscnfg /import /f $ConfigFilePath /sp $VDPath /dp $VDPath /merge"

#----------------------------------------------------------------------------
# Verfiy if config success
#----------------------------------------------------------------------------
Write-Host "Verifying [Config-VirtualDirectory.ps1] ..." -foregroundcolor yellow

$VDSettingArr = Get-WmiObject -Class IISWebVirtualDirSetting -Namespace "root/MicrosoftIISV2"
$VDName = $VDPath.replace("/LM/","")
$IsSuccess = $false

if($VDSettingArr)
{
    foreach($VDSetting in $VDSettingArr)
    {
        if($VDSetting.Name -eq $VDName)
        {
            if($VDSetting.AnonymousPasswordSync)
            {        
                $IsSuccess = $true
            }else
            {
                $IsSuccess = $false
            }
            break
        }
    }
}
if($IsSuccess)
{
    Write-Host "$VDPath config successfully." -ForegroundColor green
}
else
{
    Throw "EXECUTE [Config-VirtualDirectory.ps1] FAILED."
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Config-VirtualDirectory.ps1] SUCCEED." -foregroundcolor green

exit


