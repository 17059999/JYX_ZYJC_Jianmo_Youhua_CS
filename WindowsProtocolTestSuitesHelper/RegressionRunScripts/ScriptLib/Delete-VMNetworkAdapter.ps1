#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Delete-VMNetworkAdapter.ps1
## Purpose:        Delete a Network Adapter from Virtural Machine.
## Version:        1.0 (28 Sep, 2008)
##
##############################################################################

param(
[string]$VMName,
[string]$virtualSwitchName,
[string]$networkType = "normal"
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Delete-VMNetworkAdapter.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
Write-Host "`$virtualSwitchName = $virtualSwitchName" 
Write-Host "`$networkType = $networkType" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Delete Network Adapter from specified Virtural Machine."
    Write-host "Param1: Virtual Machine name, from which the Network Adapter deleted. (Required)"
    Write-host "Param2: Virtual Switch name which used by the Network Adapter to access network. (Required)"
    Write-host "Param3: Network Type. (Optional. Legal value: 'normal' | 'legacy'. Default value: normal)"
    Write-host
    Write-host "Example1: Delete-VMNetworkAdapter.ps1  W2K8-x86-01  Internal"
    Write-host "                Delete Network Adapter on VM named 'W2K8-x86-01'. The adapter use the Virtural Network named 'Internal' to access network."
    Write-host "Example2: Delete-VMNetworkAdapter.ps1  W2K8-x86-01  Internal Legacy"
    Write-host "                Delete Legacy Network Adapter on VM named 'W2K8-x86-01'. The legacy adapter use the Virtural Network named 'Internal' to access network."
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
if(!($networkType.ToLower().Equals("normal") -or $networkType.ToLower().Equals("legacy")))
{
    Throw "Parameter `$networkType should be 'normal' or 'legacy'."
}

Write-Host "Deleting Network Adapter '$virtualSwitchName' on VM '$VMName' ..."

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
# Get Virtual Switch
#----------------------------------------------------------------------------
$query = "SELECT * FROM Msvm_VirtualSwitch WHERE ElementName='" + $virtualSwitchName + "'"
$vSwitch = get-wmiobject -namespace root\virtualization -query $query
if ($vSwitch -eq $null)
{
    Throw "Specified Virtual Switch do not exist."
}
$vSwitchID = $vSwitch.Name

#----------------------------------------------------------------------------
# Delete Ethernet Port from VM
#----------------------------------------------------------------------------
if ($networkType -eq "normal")
{
    $NICs = Get-WmiObject -namespace root\virtualization Msvm_SyntheticEthernetPortSettingData | where { ($_.InstanceID -like "*$VMID*") -and ($_.Connection -like "*$vSwitchID*") }
}
else
{
    $NICs  = Get-WmiObject -namespace root\virtualization Msvm_EmulatedEthernetPortSettingData | where { ($_.InstanceID -like "*$VMID*") -and ($_.Connection -like "*$vSwitchID*") }
}
if ($NICs -eq $null)
{
    Write-Host "Specified VM '$VMName' has no Network Access from Virtual Switch '$virtualSwitchName' of Network Type '$networkType'." -ForegroundColor Yellow
    return
}

$errorFlag = $false
$errorcode = 0
$switchPorts = $null
foreach ($NIC in $NICs)
{
    # Get Swith Port ID
    $tmpFullString = $NIC.Connection[0]
    $startIndex = $tmpFullString.IndexOf(",Name=") + 7
    $endIndex = $tmpFullString.IndexOf(",SystemCreationClassName=")
    $switchPortID = $tmpFullString.Substring($startIndex, $endIndex - $startIndex - 1)
    
    # Remove Ethernet Port
    $result = $vsManager.RemoveVirtualSystemResources($VM.__PATH, $NIC)
    if ($result.ReturnValue -ne 0)
    {
        $errorFlag = $true
        $errorcode = $result.ReturnValue
        continue
    }
    # Remove Switch Port
    else
    {
        $switchPort = Get-WmiObject -namespace root\virtualization Msvm_SwitchPort | where {$_.Name -eq $switchPortID}
        if ($switchPort -ne $null)
        {
            $result = $switchManager.DeleteSwitchPort($switchPort)
            if ($result.ReturnValue -ne 0)
            {
                $errorFlag = $true
                $errorcode = $result.ReturnValue
                continue
            }
        }
    }
}

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Delete-VMNetworkAdapter.ps1]..." -foregroundcolor yellow
if ($errorFlag -eq $true)
{
    Throw "EXECUTE [Delete-VMNetworkAdapter.ps1] FAILED. Error code: " + $errorcode
}
else
{
    Write-Host "EXECUTE [Delete-VMNetworkAdapter.ps1] SUCCEED." -foregroundcolor green
}

