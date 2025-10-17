#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Config-Service.ps1
## Purpose:        Config a service.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$serviceName,
[string]$displayName,
[string]$pathName,
[string]$serviceType = 16,
[string]$errorControl = 1,
[string]$startMode = "Manual",
[string]$desktopInteract = $False,
[string]$startName = $null,
[string]$startPassword = $null, 
[string]$loadOrderGroup = $null, 
[string]$loadOrderGroupDependencies = $null, 
[string]$serviceDependencies = $null
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-Service.ps1] ..." -foregroundcolor cyan
Write-Host "`$serviceName = $serviceName"
Write-Host "`$displayName = $displayName"
Write-Host "`$pathName = $pathName"
Write-Host "`$serviceType = $serviceType"
Write-Host "`$errorControl = $errorControl"
Write-Host "`$startMode = $startMode"
Write-Host "`$desktopInteract = $desktopInteract"
Write-Host "`$startName = $startName"
Write-Host "`$startPassword = $startPassword"
Write-Host "`$loadOrderGroup = $loadOrderGroup"
Write-Host "`$loadOrderGroupDependencies = $loadOrderGroupDependencies"
Write-Host "`$serviceDependencies = $serviceDependencies"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Config a service."
    Write-host "Parm1: Service name. (Required)"
    Write-host "Parm2: Service's display name. (Required)"
    Write-host "Parm3: Path name. (Required)"
    Write-host "Parm4: Service type. (Optional)"
    Write-host "Parm5: Error control. (Optional)"
    Write-host "Parm6: Start mode. (Optional)"
    Write-host "Parm7: Desktop interact. (Optional)"
    Write-host "Parm8: Start name. (Optional)"
    Write-host "Parm9: Start password. (Optional)"
    Write-host "Parm10: Load order group. (Optional)"    
    Write-host "Parm11: Load order group dependencies. (Optional)"
    Write-host "Parm12: Service dependencies. (Optional)"    
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
if ($displayName -eq $null -or $displayName -eq "")
{
    Throw "Parameter `$displayName is required."
}
if ($pathName -eq $null -or $pathName -eq "")
{
    Throw "Parameter `$pathName is required."
}

#----------------------------------------------------------------------------
# Config the service
#----------------------------------------------------------------------------
$objServiceSet = Get-WmiObject Win32_Service
$serviceFound = $False
$errorOccured = $False
$returnValue = $null

foreach($objService in $objServiceSet)
{
    if($objService.Name -eq $serviceName)
    {
        $serviceFound = $True
        $returnValue = $objService.change($displayName, $pathName, $serviceType, $errorControl, $startMode, $desktopInteract, $startName, $startPassword, $loadOrderGroup, $loadOrderGroupDependencies, $serviceDependencies)
        break
    }
}

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
Write-Host "Verifying [Config-Service.ps1] ..." -foregroundcolor Yellow
if ($returnValue -ne $null)
{
    switch($returnValue.ReturnValue)
    {
        0 {Write-Host "Service" $serviceName "is Successfully Changed!" -foregroundcolor Green}
        2 {Write-Host "Access Denied! Cannot Change!" -foregroundcolor Red
                $errorOccured = $True}
        3 {Write-Host "Dependent Services are Running! Cannot Change!" -foregroundcolor Red
                $errorOccured = $True}
        14 {Write-Host "Service is disabled! Cannot Change!" -foregroundcolor Red
                $errorOccured = $True}
        default {Write-Host "Unknown Failure! Cannot Change!" -foregroundcolor Red
                $errorOccured = $True}
    }
}
else
{
    Throw "EXECUTE [Config-Service.ps1] FAILED."
}

if($serviceFound -eq $False)
{
    Throw "Cannot find service $serviceName!"
}
elseif($errorOccured -eq $True)
{
    Throw "Failed to change service $serviceName"
}
else
{
    Write-Host "Service $serviceName has successfully changed!" -foregroundcolor Green
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Config-Service.ps1] SUCCEED." -foregroundcolor Green
