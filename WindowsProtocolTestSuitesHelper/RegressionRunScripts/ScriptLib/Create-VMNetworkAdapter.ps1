#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-VMNetworkAdapter.ps1
## Purpose:        Create a new Network Adapter and add to Virtural Machine.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$VMName,
[string]$virtualSwitchName,
[string]$type = "normal"
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-VMNetworkAdapter.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
Write-Host "`$virtualSwitchName = $virtualSwitchName" 
Write-Host "`$type = $type" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Create a new Network Adapter and add to Virtural Machine."
    Write-host "Param1: Virtual Machine name which install the new Network Adapter. (Required)"
    Write-host "Param2: Virtual Switch name which used by the new Network Adapter to access network. (Required)"
    Write-host "Param3: Network Adapter Type. (Optional. Legal value: 'normal' | 'legacy'. Default value: normal)"
    Write-host
    Write-host "Example1: Create-VMNetworkAdapter.ps1  W2K8-x86-01  Internal"
    Write-host "                Create a new Network Adapter on VM named 'W2K8-x86-01'. The adapter use the Virtural Network named 'Internal' to access network."
    Write-host "Example2: Create-VMNetworkAdapter.ps1  W2K8-x86-01  Internal Legacy"
    Write-host "                Create a new legacy Network Adapter on VM named 'W2K8-x86-01'. The legacy adapter use the Virtural Network named 'Internal' to access network."
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
if ($VMName -eq $null -or $VMName -eq "")
{
    Throw "Parameter $VMName is required."
}
if ($virtualSwitchName -eq $null -or $virtualSwitchName -eq "")
{
    Throw "Parameter $virtualSwitchName is required."
}
if(!($type.ToLower().Equals("normal") -or $type.ToLower().Equals("legacy")))
{
    Throw "Parameter type should be 'normal' or 'legacy'."
}

Write-Host "Creating Network Adapter on VM: '$VMName', use Virturl Network: '$virtualSwitchName' ..."

#----------------------------------------------------------------------------
# Create a new port on Virtural Switch
#----------------------------------------------------------------------------
$query = "SELECT * FROM msvm_VirtualSwitch WHERE ElementName='" + $virtualSwitchName + "'"
$vSwitch = get-wmiobject -namespace root/virtualization -query $query
if ($vSwitch -eq $null)
{
    Throw "Specified Virtual Switch do not exist."
}

$switchManager = gwmi -namespace root/virtualization Msvm_VirtualSwitchManagementService

$portName = [System.Guid]::NewGuid().ToString()
$scope = ""
$result = $switchManager.CreateSwitchPort($vSwitch, $portName, $portName, $scope)
if ($result.ReturnValue -ne 0)
{
    $errorcode = $result.ReturnValue
    Throw "Create switch port failed! Error code: " + $errorcode
}
$newPortPath = $result.CreatedSwitchPort

#----------------------------------------------------------------------------
# Add a new EthernetPort
#----------------------------------------------------------------------------
$defaultNIC = $null
$newNIC     = $null
if ($type.ToLower().Equals("normal"))  # Create a new SyntheticEthernetPort 
{
    $defaultNIC = gwmi -namespace root/virtualization Msvm_SyntheticEthernetPortSettingData | where {$_.InstanceID -like "*Default*"}
    $newNIC = $defaultNIC.psbase.Clone()
    $syntheticNICGUID = [System.Guid]::NewGuid().ToString()
    $newNIC.VirtualSystemIdentifiers = "{" + $syntheticNICGUID + "}"
}
else  # Create a new EmulatedEthernetPort 
{
    $defaultNIC = gwmi -namespace root/virtualization Msvm_EmulatedEthernetPortSettingData | where {$_.InstanceID -like "*Default*"}
    $newNIC = $defaultNIC.psbase.Clone()
}
$newNIC.Connection = $newPortPath
$newNIC.ElementName = "$type Network Adapter"

#----------------------------------------------------------------------------
# Add the new adapter to VM
#----------------------------------------------------------------------------
$vsManager = get-wmiobject -namespace root\virtualization Msvm_VirtualSystemManagementService
$VM = get-wmiobject -namespace root\virtualization Msvm_ComputerSystem | where {$_.ElementName -like $VMName}
if ($VM -eq $null)
{
    Throw "Specified Virtual Machine do not exist."
}
$result = $vsManager.AddVirtualSystemResources($VM.__PATH, $newNIC.psbase.gettext(1)) 
if ($result.ReturnValue -ne 0)
{
    $errorcode = $result.ReturnValue
    Throw "Create Network Adapter failed! Error code: " + $errorcode
}

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Create-VMNetworkAdapter.ps1]..." -foregroundcolor yellow
if ($result.ReturnValue -ne 0)
{
    $errorcode = $result.ReturnValue
    Throw "EXECUTE [Create-VMNetworkAdapter.ps1] FAILED. Error code: " + $errorcode
}else
{
    Write-Host "EXECUTE [Create-VMNetworkAdapter.ps1] SUCCEED." -foregroundcolor green
}

