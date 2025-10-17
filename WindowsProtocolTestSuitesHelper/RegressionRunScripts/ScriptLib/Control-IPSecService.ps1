#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Control-IPSecService.ps1
## Purpose:        Start/Stop a services, such as IPSec.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

Param(
[string]$servicename = "PolicyAgent",
[string]$setservice
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Control-IPSecService.ps1] ..." -foregroundcolor cyan
Write-Host "`$servicename = $servicename"
Write-Host "`$setservice = $setservice"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script will Find the IPSec service and Start or stop it."
    Write-host
    Write-host "Example: Control-IPSecService.ps1 PolicyAgent Start"
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
if (($setservice -eq $null) -or ($setservice -eq ""))
{
    Throw "Parameter computerName is required."
}

if (($setservice -ne "Start") -and ($setservice -ne "Stop"))
{
    Throw "Parameter computerName is required 'Start' or 'Stop'."
}

#----------------------------------------------------------------------------
# Find the service 
#----------------------------------------------------------------------------
Write-Host "Get service $servicename..."
$objServiceSet = Get-WmiObject Win32_Service

$theService =$null
foreach($objService in $objServiceSet)
{
    if($objService.Name -eq $servicename)
    {
        Write-Host "Service $servicename was found successfully."
        $theService = $objService
        break
    }
}
if($theService -eq $null)
{
    Throw "Service $servicename is not installed on this computer."
}

#----------------------------------------------------------------------------
# Control the service 
#----------------------------------------------------------------------------
$serviceStarted = $theService.Started
Write-Host "Service $servicename is started: $serviceStarted."
if($setservice -eq "Start")
{
    if(-not $serviceStarted )
    {   
        Write-Host "Try to start services $servicename..."
        $ReturnValue = $theService.StartService()
    }
    else
    {
        Write-Host "Services $servicename is already started." 
    }
}
elseif($setservice -eq "Stop")
{
    if($serviceStarted )
    {   
        Write-Host "Try to stop services $servicename..."
        $ReturnValue = $theService.StopService()
    }
    else
    {
        Write-Host "Services $servicename is already stoped."
    }
}
else
{
    Throw "Not supported operation."
}

#----------------------------------------------------------------------------
# Parse the return code 
#----------------------------------------------------------------------------
if ($ReturnValue -ne $null)
{
    $returnValue = $ReturnValue.ReturnValue
    Write-Host "Return code from service operation: $returnValue."
    switch($returnValue)
    {
        0 {Write-Host "Services $servicename is Successfully configured." -foregroundcolor Green}
        2 {Write-Host "Access Denied from service operation." -foregroundcolor Red}
        3 {Write-Host "Dependent Services are Running." -foregroundcolor Red}
        14 {Write-Host "Service is disabled." -foregroundcolor Red}
        default {Write-Host "Unknown Failure." -foregroundcolor Red}
    }
}

Write-Host "EXECUTE [Control-IPSecService.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow
return
