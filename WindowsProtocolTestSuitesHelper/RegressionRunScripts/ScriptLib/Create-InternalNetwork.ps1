#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-InternalNetwork.ps1
## Purpose:        Create an internal network with name of "Internal" in Hyper-V host.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$networkName = "Internal",
[string]$IPv4Address = "192.168.0.254",
[string]$IPv4Mask    = "255.255.255.0",
[string]$IPv4Gateway = "192.168.0.201",
[string]$IPv6Address = "2008::fe",
[string]$IPv6Gateway = "2008::c9"
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-InternalNetwork.ps1]..." -foregroundcolor cyan
Write-Host "`$networkName = $networkName" 
Write-Host "`$IPv4Address = $IPv4Address" 
Write-Host "`$IPv4Mask    = $IPv4Mask"
Write-Host "`$IPv4Gateway = $IPv4Gateway"
Write-Host "`$IPv6Address = $IPv6Address"
Write-Host "`$IPv6Gateway = $IPv6Gateway"

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{
    Write-host 
    Write-host "Usage: This script creates an internal network named Internal in Hyper-V."
    Write-host
    Write-host "Example: CreateInternalNetwork.ps1"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------

if ($args[0] -match '-(\?|(h|(help)))')
{
    write-host 
    Show-ScriptUsage 
    return
}

#--------------------------------------------------------------------------
# networkName in Hyper-V
# networkAdapterName in Network Connection Management of 2K8
#--------------------------------------------------------------------------
$networkAdapterName = "HyperV_" + $networkName + "_Network"
Write-host "Creating an internal network named $networkName in Hyper-V." -foregroundcolor Yellow

#-----------------------------------------------------------------------
# Get the WMI object of the Msvm_VirtualSwitchManagementService class
#-----------------------------------------------------------------------
$switchManager = gwmi -namespace root/virtualization Msvm_VirtualSwitchManagementService
if ($switchManager -eq $null)
{
    throw "Cannot get the SwitchManager."
}

#-----------------------------------------------------------------------
# Delete the Virtual Network with same name in Hyper-V
#-----------------------------------------------------------------------
$query = "SELECT * FROM msvm_VirtualSwitch WHERE Name='" + $networkName + "'"
$virtualSwitch = get-wmiobject -namespace root/virtualization -query $query
if ($virtualSwitch -ne $null)
{
    Write-host "Deleting existing Virtual Network Switch."
    $result = $switchManager.DeleteSwitch($virtualSwitch)
    write-host "Return value:" $result.ReturnValue
}

#-----------------------------------------------------------------------
# Create a Virtual Network in Hyper-V
#-----------------------------------------------------------------------
$result = $switchManager.CreateSwitch($networkName, $networkName, 1024, $null)
$query = "SELECT * FROM msvm_VirtualSwitch WHERE Name='" + $networkName + "'"
$virtualSwitch = get-wmiobject -namespace root/virtualization -query $query

if ($virtualSwitch -eq $null)
{
    $errorcode = $result.ReturnValue
    Throw "CreateSwitch Failed! Error code: " + $errorcode
}

#-----------------------------------------------------------------------
# Create a port in the newly created Virtual Network Switch
#-----------------------------------------------------------------------
$guid = [System.Guid]::NewGuid().ToString()
$result = $switchManager.CreateSwitchPort($virtualSwitch, $guid, $networkName + "_Port", $null)
$query = "SELECT * FROM Msvm_SwitchPort WHERE Name='" + $guid + "'"
$switchPort = get-wmiobject -namespace root/virtualization -query $query

if ($switchPort -eq $null)
{
    $errorcode = $result.ReturnValue
    Throw "CreateSwitchPort Failed! Error code: " + $errorcode
}

#-----------------------------------------------------------------------
# Create a NIC for the internal network if not exist
#-----------------------------------------------------------------------
$query = "SELECT * FROM Msvm_InternalEthernetPort WHERE ElementName='" + $networkAdapterName + "'"
$internalPort = get-wmiobject -namespace "root\virtualization" -query $query

if ($internalPort -eq $null)
{
    $result = $switchManager.CreateInternalEthernetPortDynamicMac($networkAdapterName, $networkAdapterName)
    $internalPort = get-wmiobject -namespace root/virtualization -query $query
}

if ($internalPort -eq $null)
{
    $errorcode = $result.ReturnValue
    Throw "CreateInternalEthernetPortDynamicMac Failed! Error code: " + $errorcode
}

#-----------------------------------------------------------------------
# Get the port of the internal NIC
#-----------------------------------------------------------------------
$query = "ASSOCIATORS OF {" + $internalPort.Path.Path + "} WHERE ResultClass = CIM_LANEndpoint"
$lan = get-wmiobject -namespace root/virtualization -query $query

if ($lan -eq $null)
{
    Throw "Get the port of the internal NIC Failed!"
}

#-----------------------------------------------------------------------
# Connect the Virtual Network Switch and the NIC
#-----------------------------------------------------------------------
$result = $switchManager.ConnectSwitchPort($switchPort, $lan)
if ($result.ReturnValue -ne 0)
{
    $errorcode = $result.ReturnValue
    Throw "ConnectSwitchPort Failed! Error code: " + $errorcode
}

#-----------------------------------------------------------------------
# Get the NIC interface Name (NICID)
#-----------------------------------------------------------------------
$query = "SELECT * FROM Win32_NetworkAdapter WHERE GUID='" + $internalPort.DeviceID + "'"
$NIC = gwmi -namespace root/cimv2 -query $query
$NICID = $NIC.NetConnectionID

if ($NICID -eq $null)
{
    Throw "Get the NIC interface Name (NICID) Failed!"
}

#-----------------------------------------------------------------------
# Set the static IP
#-----------------------------------------------------------------------
write-host "Virtual NIC created successfully."
write-host "Config the IP address..."

netsh interface ipv4 set address   "$NICID" static $IPv4Address $IPv4Mask $IPv4Gateway
#netsh interface ipv4 set dnsserver "$NICID" static 192.168.0.201 primary

netsh interface ipv6 set address   "$NICID" $IPv6Address
netsh interface ipv6 add route     ::/0 "$NICID" $IPv6Gateway
#netsh interface ipv6 set dnsserver "$NICID" static 2008::c9 primary

netsh firewall set service FILEANDPRINT ENABLE

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Create-InternalNetwork.ps1]..." -foregroundcolor yellow
Write-Host "EXECUTE [Create-InternalNetwork.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit
