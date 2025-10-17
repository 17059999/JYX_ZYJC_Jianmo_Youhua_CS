#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Bind-InternalNetwork.ps1
## Purpose:        Bind a Network Adapter of specified VM from currently used Internal Network to a different Internal Newwork.
## Version:        1.0 (6 Oct, 2008)
##
##############################################################################

param(
[string]$VMName,
[string]$virtualSwitchName,
[string]$newVirtualSwitchName,
[string]$networkType = "normal"
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Bind-InternalNetwork]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
Write-Host "`$virtualSwitchName    = $virtualSwitchName" 
Write-Host "`$newVirtualSwitchName = $newVirtualSwitchName" 
Write-Host "`$networkType = $networkType" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Bind a Network Adapter of specified VM from currently used Internal Network to a different Internal Newwork."
    Write-host "Param1: Virtual Machine name. (Required)"
    Write-host "Param2: Currently used Virtual Switch name. (Required)"
    Write-host "Param3: New Virtual Switch name, which will be used to access network after this script be executed. (Required)"
    Write-host "Param4: Network Type. (Optional. Legal value: 'normal' | 'legacy'. Default value: normal)"
    Write-host
    Write-host "Example1: Bind-InternalNetwork  W2K8-x86-01  Internal  Internal_2"
    Write-host "                Bind a Network Adapter of VM 'W2K8-x86-01' to a new Internal Network 'Internal_2' from currently used Internal Network 'Internal'."
    Write-host "Example2: Bind-InternalNetwork  W2K8-x86-01  Internal  Internal_2  Legacy"
    Write-host "                Bind a Lagacy Network Adapter of VM 'W2K8-x86-01' to a new Internal Network 'Internal_2' from currently used Internal Network 'Internal'."
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
    Throw "Parameter `$VMName is required."
}
if ($virtualSwitchName -eq $null -or $virtualSwitchName -eq "")
{
    Throw "Parameter `$virtualSwitchName is required."
}
if ($newVirtualSwitchName -eq $null -or $newVirtualSwitchName -eq "")
{
    Throw "Parameter `$newVirtualSwitchName is required."
}
if ($virtualSwitchName -eq $newVirtualSwitchName)
{
    Throw "Parameters `$virtualSwitchName and `$newVirtualSwitchName cannot be equal."
}
if(!($networkType.ToLower().Equals("normal") -or $networkType.ToLower().Equals("legacy")))
{
    Throw "Parameter `$networkType should be 'normal' or 'legacy'."
}

function Get-SwitchPortID([string]$connectionString)
{
    $startIndex = $connectionString.IndexOf(",Name=") + 7
    $endIndex = $connectionString.IndexOf(",SystemCreationClassName=")
    return $connectionString.Substring($startIndex, $endIndex - $startIndex - 1)
}

function Get-DeviceID([string]$connectionString)
{
    $startIndex = $connectionString.LastIndexOf('\')
    return $connectionString.Substring($startIndex + 1, $connectionString.Length - ($startIndex + 1))
}

Write-Host "Binding Network for VM '$VMName' from '$virtualSwitchName' to '$newVirtualSwitchName' ..."
#----------------------------------------------------------------------------
# Get VM
#----------------------------------------------------------------------------
$vsManager = get-wmiobject -namespace root\virtualization Msvm_VirtualSystemManagementService
$switchManager = get-wmiobject -namespace root\virtualization Msvm_VirtualSwitchManagementService

$VM = get-wmiobject -namespace root\virtualization Msvm_ComputerSystem | where {$_.ElementName -like $VMName}
if ($VM -eq $null)
{
    Throw "Specified Virtual Machine do not exist."
}
$VMID = $VM.Name

#----------------------------------------------------------------------------
# Get the old Virtual Switch
#----------------------------------------------------------------------------
$query = "SELECT * FROM Msvm_VirtualSwitch WHERE ElementName='" + $virtualSwitchName + "'"
$oldVSwitch = get-wmiobject -namespace root\virtualization -query $query
if ($oldVSwitch -eq $null)
{
    Throw "Specified Virtual Switch `"$virtualSwitchName`" do not exist."
}
$oldVSwitchID = $oldVSwitch.Name

#----------------------------------------------------------------------------
# Get the new Virtual Switch
#----------------------------------------------------------------------------
$query = "SELECT * FROM Msvm_VirtualSwitch WHERE ElementName='" + $newVirtualSwitchName + "'"
$vSwitch = get-wmiobject -namespace root\virtualization -query $query
if ($vSwitch -eq $null)
{
    Throw "Specified Virtual Switch `"$newVirtualSwitchName`" do not exist."
}
$vSwitchID = $vSwitch.Name

#----------------------------------------------------------------------------
# Delete the old SwitchPort
#----------------------------------------------------------------------------
if ($networkType -eq "normal")
{
    $NICs = Get-WmiObject -namespace root\virtualization Msvm_SyntheticEthernetPortSettingData | where { ($_.InstanceID -like "*$VMID*") -and ($_.Connection -like "*$oldVSwitchID*") }
}
else
{
    $NICs  = Get-WmiObject -namespace root\virtualization Msvm_EmulatedEthernetPortSettingData | where { ($_.InstanceID -like "*$VMID*") -and ($_.Connection -like "*$oldVSwitchID*") }
}
if ($NICs -eq $null)
{
    Write-Host "Specified VM '$VMName' has no Network Access from Virtual Switch '$virtualSwitchName' of Network Type '$networkType'." -ForegroundColor Yellow
    return
}

$NIC = $null
if($NICs.gettype().isArray -eq $true)
{
    $NIC = $NICs[0]
}
else
{
    $NIC = $NICs
}
$deviceID = Get-DeviceID $NIC.InstanceID

$oldSwitchPortID = Get-SwitchPortID $NIC.Connection[0]
$oldSwitchPort = Get-WmiObject -namespace root\virtualization Msvm_SwitchPort | where {$_.Name -eq $oldSwitchPortID}
$result = $switchManager.DisconnectSwitchPort($oldSwitchPort)
if ($result.ReturnValue -ne 0)
{
    Throw "Cannot disconnect from old SwitchPort `"$oldSwitchPortID`""
}
$result = $switchManager.DeleteSwitchPort($oldSwitchPort)
if ($result.ReturnValue -ne 0)
{
    Throw "Cannot delete the old SwitchPort `"$oldSwitchPortID`""
}

#----------------------------------------------------------------------------
# Create a new SwitchPort
#----------------------------------------------------------------------------
$portName = [System.Guid]::NewGuid().ToString()
$scope = ""
$result = $switchManager.CreateSwitchPort($vSwitch, $portName, $portName, $scope)
if ($result.ReturnValue -ne 0)
{
    Throw "Cannot create new SwitchPort failed."
}
$newSwitchPortID = Get-SwitchPortID $result.CreatedSwitchPort
$newSwitchPort = Get-WmiObject -namespace root\virtualization Msvm_SwitchPort | where {$_.Name -eq $newSwitchPortID}

#----------------------------------------------------------------------------
# Get Endpoint
#----------------------------------------------------------------------------
if ($networkType -eq "normal")
{
    $ethernetPort = Get-WmiObject -namespace root\virtualization Msvm_SyntheticEthernetPort | where { ($_.SystemName -like "*$VMID*") -and ($_.DeviceID -like "*\$deviceID*") }
}
else
{
    $ethernetPort = Get-WmiObject -namespace root\virtualization Msvm_EmulatedEthernetPort | where { ($_.SystemName -like "*$VMID*") -and ($_.DeviceID -like "*\$deviceID*") }
}
if ($ethernetPort -eq $null)
{
    Throw "Cannot get Ethernet Port."
}
$deviceID = $ethernetPort.DeviceID
$LANEndpoint = Get-WmiObject -namespace root\virtualization Msvm_VmLANEndpoint | where { ($_.Name -like "*$deviceID*") -and ($_.SystemName -like "*$VMID*") }
if ($LANEndpoint -eq $null)
{
    Throw "Cannot get LANEndpoint."
}

#----------------------------------------------------------------------------
# Bind to new SwitchPort
#----------------------------------------------------------------------------
$result = $switchManager.ConnectSwitchPort($newSwitchPort, $LANEndpoint)

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Bind-InternalNetwork]..." -foregroundcolor yellow
if ($result.ReturnValue -ne 0)
{
    Throw "EXECUTE [Bind-InternalNetwork] FAILED. Error code: " + $result.ReturnValue
}
else
{
    Write-Host "EXECUTE [Bind-InternalNetwork] SUCCEED." -foregroundcolor green
}

