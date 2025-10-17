#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Stop-Service.ps1
## Purpose:        Stop a service.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$serviceName
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Stop-Service.ps1] ..." -foregroundcolor cyan
Write-Host "`$serviceName = $serviceName"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Stop a service."
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
# Stop the service
#----------------------------------------------------------------------------
$objServiceSet = Get-WmiObject Win32_Service
$serviceFound = $False
$errorOccured = $False

foreach($objService in $objServiceSet)
{
    if($objService.Name -eq $serviceName)
    {
        $serviceFound = $True
        if($objService.Started -eq $True)
        {
            $ReturnValue = $objService.StopService()
            switch($ReturnValue.ReturnValue)
            {
                0 {Write-Host "Service" $serviceName "is Successfully Stopped!" -foregroundcolor Green}
                2 {Write-Host "Access Denied! Cannot Stop!" -foregroundcolor Red
                    $errorOccured = $True}
                3 {Write-Host "Dependent Services are Running! Cannot Stop!" -foregroundcolor Red
                    $errorOccured = $True}
                14 {Write-Host "Service is disabled! Cannot Stop!" -foregroundcolor Red
                    $errorOccured = $True}
                default {Write-Host "Unknown Failure! Cannot Stop!" -foregroundcolor Red
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
    Throw "Failed to stop service $serviceName" 
}
else
{
    Write-Host "Service" $serviceName "has already stopped" -foregroundcolor Green
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Stop-Service.ps1] SUCCEED." -foregroundcolor Green
