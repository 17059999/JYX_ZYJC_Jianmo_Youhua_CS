#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Config-VMTimeSync.ps1
## Purpose:        Config VM Memory capacity.
## Version:        1.0 (2 June, 2009)
##
##############################################################################
param(
[string]$VMName,
[bool]$isEnable
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-VMTimeSync.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName   = $VMName" 
Write-Host "`$isEnable = $isEnable" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Turn on or off VM time synchronization integration service."
    Write-host
    Write-host "Scenario: By default, VM will synchronize system time with the host machine in every period of time.
    Write-host "          If test suite has behavior to modify system time for test purpose, this scripts can be used to turn
    Write-host "          off time synchronization in specified VM "
    Write-Host
    Write-host "Param1: Virtual Machine name. (Required)"    
    Write-host "Param2: A bool param stands for whether to enable time synchronization integration service"
    Write-Host "        on target VM. (Required)"
    Write-host
    Write-host "Example1: Config-VMTimeSync.ps1  Win7-x64-01 $false"
    Write-host "        Turn off time synchronization on VM `"W2K8-x86-01`"."
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
if ($isEnable -eq $null)
{
    Throw "Parameter `$isEnable is required."
}

#----------------------------------------------------------------------------
# Get VM WMI Object
#----------------------------------------------------------------------------
if ($isEnable)
{
    $switch       = "on"
    $enabledState = 2
}
else
{
    $switch       = "off"
    $enabledState = 3
}

Write-Host "Turn $switch time synchronization on VM `"$VMName`"..."
$vmService = get-wmiobject -namespace root\virtualization Msvm_VirtualSystemManagementService
if ($vmService -eq $null)
{
    Throw "Error: Cannot get WMI object 'Msvm_VirtualSystemManagementService'."
}
$vm = get-wmiobject -namespace root\virtualization Msvm_ComputerSystem | where {$_.ElementName -like "$VMName"} 
if ($vm -eq $null)
{
    Throw "Error: Specified VM `"$VMName`" do not exist."
}
$vmID = $vm.Name

#----------------------------------------------------------------------------
# 
#----------------------------------------------------------------------------
$timeSyncObj = get-wmiobject -namespace root\virtualization Msvm_TimeSyncComponentSettingData | where {$_.InstanceID -like "*$vmID*"}
if ($timeSyncObj -eq $null)
{
    Throw "Error: Cannot get VM time synchronization object."
}
$timeSyncObj.EnabledState = [uint16]$enabledState

$result = $vmService.ModifyVirtualSystemResources($vm.__PATH, $timeSyncObj.psbase.getText(1))

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Config-VMTimeSync.ps1]..." -foregroundcolor yellow
if ($result.ReturnValue -ne 0)
{
    $errorcode = $result.ReturnValue
    Throw "EXECUTE [Config-VMTimeSync.ps1] FAILED. Error code: " + $errorcode
}
else
{
    Write-Host "EXECUTE [Config-VMTimeSync.ps1] SUCCEED." -foregroundcolor green
}
