#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Config-MSServer.ps1
## Purpose:        Install/Uninstall MS_Server.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2008 R2
##
##############################################################################
Param(
[string]$action = "Install"
)
#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-MSServer.ps1] ..." -foregroundcolor cyan
Write-Host "`$action      = $action"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host " Install/Uninstall MS_Server."
    Write-host
    Write-host "Example: Config-MSServer.ps1 UnInstall"
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
if($action -eq "Install")
{
    #----------------------------------------------------------------------------
    # Stop Server and Workstation services
    #----------------------------------------------------------------------------
    Write-Host "Install File and Printer feature for network adapter ..." -ForegroundColor Yellow
    netcfg -c s -i MS_Server
    cmd /c ECHO "Install MS_Server Finished" >$env:HOMEDRIVE\InstallMSServer.signal
}
if($action -eq "UnInstall")
{
    #----------------------------------------------------------------------------
    # Stop Server and Workstation services
    #----------------------------------------------------------------------------
        Write-Host "Uninstall File and Printer feature for network adapter ..." -ForegroundColor Yellow
    netcfg -c s -u MS_Server
    cmd /c ECHO "Unintall MS_Server Finished" >$env:HOMEDRIVE\UninstallMSServer.signal
}
cmd /c shutdown -r -t 3
exit 0

