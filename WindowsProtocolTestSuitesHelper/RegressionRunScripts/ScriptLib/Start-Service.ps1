#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Start-Service.ps1
## Purpose:        Start a service.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$serviceName
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Start-Service.ps1] ..." -foregroundcolor cyan
Write-Host "`$serviceName = $serviceName"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Start a service."
    Write-host "Parm1: Service name. (Required)"
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
if ($serviceName -eq $null -or $serviceName -eq "")
{
    Throw "Parameter `$serviceName is required."
}

#----------------------------------------------------------------------------
# Start the service
#----------------------------------------------------------------------------
$objServiceSet = Get-WmiObject Win32_Service
$serviceFound = $False
$errorOccured = $False

foreach($objService in $objServiceSet)
{
    if($objService.Name -eq $Servicename)
    {
        $serviceFound = $True
        if($objService.Started -eq $False)
        {
            $ReturnValue = $objService.StartService()
            switch($ReturnValue.ReturnValue)
            {
                0 {Write-Host "Service" $serviceName "is Successfully Started!" -foregroundcolor Green}
                2 {Write-Host "Access Denied! Cannot Start!" -foregroundcolor Red
                    $errorOccured = $True}
                3 {Write-Host "Dependent Services are Running! Cannot Start!" -foregroundcolor Red
                    $errorOccured = $True}
                14 {Write-Host "Service is disabled! Cannot Start!" -foregroundcolor Red
                    $errorOccured = $True}
                default {Write-Host "Unknown Failure! Cannot Start!" -foregroundcolor Red
                    $errorOccured = $True}
            }
            return
        }
    }
}

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
if($serviceFound -eq $False)
{
    Throw "Cannot find service $serviceName!"
}
elseif($errorOccured -eq $True)
{
    Throw "Failed to start service $serviceName"
}
else
{
    Write-Host "Service $serviceName has already started!" -foregroundcolor Green
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Start-Service.ps1] SUCCEED." -foregroundcolor Green

