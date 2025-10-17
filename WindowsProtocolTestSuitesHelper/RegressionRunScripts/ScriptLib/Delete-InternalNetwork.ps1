#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Delete-InternalNetwork.ps1
## Purpose:        Delete a Internal Network from Hyper-V host.
## Version:        1.0 (7 Oct, 2008)
##
##############################################################################

param(
[string]$networkName
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Delete-InternalNetwork]..." -foregroundcolor cyan
Write-Host "`$networkName = $networkName" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage:  Delete a Internal Network from Hyper-V host."
    Write-host "Param1: Internal Network name. (Required)"
    Write-host
    Write-host "Example1: Delete-InternalNetwork  Internal"
    Write-host "                Delete the Internal Network 'Internal'"
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
if ($networkName -eq $null -or $networkName -eq "")
{
    Throw "Parameter `$networkName is required."
}


Write-Host "Deleting Internal Network `"$networkName`" ..."
#----------------------------------------------------------------------------
# Get Switch Manager
#----------------------------------------------------------------------------
$switchManager = get-wmiobject -namespace root\virtualization Msvm_VirtualSwitchManagementService
if ($switchManager -eq $null)
{
    Throw "Cannot get Switch Manager."
}

#----------------------------------------------------------------------------
# Get Virtual Switch
#----------------------------------------------------------------------------
$query = "SELECT * FROM msvm_VirtualSwitch WHERE Name='" + $networkName + "'"
$virtualSwitch = get-wmiobject -namespace root\virtualization -query $query
if ($virtualSwitch -eq $null)
{
    Write-host "Specified Internal Network `"$networkName`" do not exist." -ForegroundColor Yellow
    return
}
Write-host "Deleting existing Virtual Network Switch."
$result = $switchManager.DeleteSwitch($virtualSwitch)

if ($result.ReturnValue -ne 0)
{
    Throw "EXECUTE [Delete-InternalNetwork] FAILED. Error code: " + $result.ReturnValue
}

#----------------------------------------------------------------------------
# Delete internal ethernet port
#----------------------------------------------------------------------------
$networkAdapterName = "HyperV_" + $networkName + "_Network"
$query = "SELECT * FROM Msvm_InternalEthernetPort WHERE ElementName='" + $networkAdapterName + "'"
$internalPort = get-wmiobject -namespace "root\virtualization" -query $query

if ($internalPort -eq $null)
{
    return
}

$result = $switchManager.DeleteInternalEthernetPort($internalPort)

$internalPort = get-wmiobject -namespace "root\virtualization" -query $query
if($internalPort -ne $null)
{
	throw "Delete internal ethernet port failed"
}

Write-Host "EXECUTE [Delete-InternalNetwork] SUCCEED." -foregroundcolor green

