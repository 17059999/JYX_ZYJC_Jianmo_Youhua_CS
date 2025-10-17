#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Restart-Service.ps1
## Purpose:        Restart a service.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$serviceName
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Restart-Service.ps1] ..." -foregroundcolor cyan
Write-Host "`$serviceName = $serviceName"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Restart a service."
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
# Restart the service
#----------------------------------------------------------------------------
$errorOccured = $False
$objServiceSet = Get-WmiObject Win32_Service
$serviceFound = $False

foreach($objService in $objServiceSet)
{
    if($objService.Name -eq $serviceName)
    {
        $serviceFound = $True
        if($objService.Started -eq $False)
        #The service is now stopped, it should be started.
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
        else
        #The service is now started, it should be restarted(stop and then start).
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
            if($errorOccured -eq $False)
            {
                $objService = Get-WmiObject Win32_Service | Where-Object {$_.Name -eq $serviceName}
                while(-not($objService.Started -eq $False))
                {
                    $objService = Get-WmiObject Win32_Service | Where-Object {$_.Name -eq $serviceName}
                }
                $ReturnValue = $objService.StartService()
                switch($ReturnValue.ReturnValue)
                {
                    0 {Write-Host "Service" $serviceName "is Successfully Started!" -foregroundcolor Green}
                    2 {Wrie-Host "Access Denied! Cannot Start!" -foregroundcolor Red
                        $errorOccured = $True}
                    3 {Write-Host "Dependent Services are Running! Cannot Start!" -foregroundcolor Red
                        $errorOccured = $True}
                    10 {Write-Host "Service already running! Cannot Start!" -foregroundcolor Red
                        $errorOccured = $True}
                    14 {Write-Host "Service is disabled! Cannot Start!" -foregroundcolor Red
                        $errorOccured = $True}
                    default {Write-Host "Unknown Failure! Cannot Start!" -foregroundcolor Red
                        $errorOccured = $True}
                }
                
            }
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
    Throw "Failed to restart service $serviceName"
}
else
{
    Write-Host "Service $serviceName has successfully restarted!" -foregroundcolor Green
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Restart-Service.ps1] SUCCEED." -foregroundcolor Green
