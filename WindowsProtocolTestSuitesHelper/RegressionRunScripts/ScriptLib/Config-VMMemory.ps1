#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Config-VMMemory.ps1
## Purpose:        Config VM Memory capacity.
## Version:        1.0 (20 Oct, 2008)
##
##############################################################################
param(
[string]$VMName,
[string]$mem
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-VMMemory.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
Write-Host "`$mem = $mem" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Config VM Memory capacity."
    Write-host "Param1: Virtual Machine name. (Required)"    
    Write-host "Param2: Memory capacity assigned to the VM. (Required)"
    Write-host
    Write-host "Example1: Config-VMMemory.ps1  W2K8-x86-01  1024"
    Write-host "                Modify VM `"W2K8-x86-01`"'s memory capacity to 1024MB."
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
if ($mem -eq $null -or $mem -eq "")
{
    Throw "Parameter `$mem is required."
}
[int]$memValue = [int]$mem
if (($memValue -lt 8) -or ($memValue -gt 3964))
{
    Throw "Parameter `$mem must between 8 and 3964"
}

#----------------------------------------------------------------------------
# Get VM WMI Object
#----------------------------------------------------------------------------
Write-Host "Configuring VM `"$VMName`", modify memory to $mem MB..."
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
$memObj = get-wmiobject -namespace root\virtualization Msvm_MemorySettingData | where {$_.InstanceID -like "*$vmID*"}
if ($memObj -eq $null)
{
    Throw "Error: Cannot get VM Memory object."
}
$memObj.VirtualQuantity = [int]$memValue
$memObj.Reservation     = [int]$memValue
$memObj.Limit           = [int]$memValue

$result = $vmService.ModifyVirtualSystemResources($vm.__PATH, $memObj.psbase.getText(1))

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Config-VMMemory.ps1]..." -foregroundcolor yellow
if ($result.ReturnValue -ne 0)
{
    $errorcode = $result.ReturnValue
    Throw "EXECUTE [Config-VMMemory.ps1] FAILED. Error code: " + $errorcode
}
else
{
    Write-Host "EXECUTE [Config-VMMemory.ps1] SUCCEED." -foregroundcolor green
}
