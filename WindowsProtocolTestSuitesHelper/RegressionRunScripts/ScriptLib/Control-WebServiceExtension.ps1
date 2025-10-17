#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Control-WebServiceExtension.ps1
## Purpose:        Enable or disable one Web Service extension of IIS, such as:WEBDAV,ASP,etc.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[string]$ServiceName,
[string]$EnableOrDisable = "Enable"
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Control-WebServiceExtension.ps1] ..." -foregroundcolor cyan
Write-Host "`$ServiceName     = $ServiceName"
Write-Host "`$EnableOrDisable = $EnableOrDisable"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Enable or disable one Web Service extension of IIS, such as:WEBDAV,ASP,etc."
    Write-host
    Write-host "Example: Control-WebServiceExtension.ps1 WEBDAV Enable"
    Write-host "         Control-WebServiceExtension.ps1 WEBDAV Disable"
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
if ($ServiceName -eq $null -or $ServiceName -eq "")
{
    Show-ScriptUsage
    Throw "Parameter Service name is required."
}

#----------------------------------------------------------------------------
# Enable or Disable
#----------------------------------------------------------------------------
$IISWebSVCObj = Get-WmiObject -Class IISWebService -Namespace "root/MicrosoftIISV2"

if($EnableOrDisable -eq "Disable" -or $EnableOrDisable -eq "D")
{
    $IISWebSVCObj.DisableWebServiceExtension($ServiceName)
    Write-Host "$ServiceName was Disabled." -ForegroundColor green
}
else
{
    $IISWebSVCObj.EnableWebServiceExtension($ServiceName)
    Write-Host "$ServiceName was Enabled." -ForegroundColor green
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Control-WebServiceExtension.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit
