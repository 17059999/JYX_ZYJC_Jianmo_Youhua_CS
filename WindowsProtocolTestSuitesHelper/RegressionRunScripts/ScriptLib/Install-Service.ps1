#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-Service.ps1
## Purpose:        Install a service.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$serviceName,
[string]$pathName
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Install-Service.ps1] ..." -foregroundcolor cyan
Write-Host "`$serviceName = $serviceName"
Write-Host "`$pathName = $pathName"


#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Install a service."
    Write-host "Parm1: Service name. (Required)"
    Write-host "Parm2: Service's binary path. (Required)"    
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
if ($pathName -eq $null -or $pathName -eq "")
{
    Throw "Parameter `$pathName is required."
}

#----------------------------------------------------------------------------
# Install the service
#----------------------------------------------------------------------------
$objServiceSet = Get-WmiObject Win32_Service
$serviceFound = $False
$errorOccured = $False

foreach($objService in $objServiceSet)
{
    if($objService.Name -eq $servicename)
    {
        $serviceFound = $True
    }

}

if($serviceFound -eq $True)
{
    Throw "service $serviceName already exists, please change a service name"
    $errorOccured = $True
}
else
{
    New-Service -name $serviceName -binaryPathName $path
}

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
if($errorOccured -eq $True)
{
    Throw "Failed to install service $serviceName"
}
else
{
    Write-Host "Service $serviceName installed!" -foregroundcolor Green
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Install-Service.ps1] SUCCEED." -foregroundcolor Green
