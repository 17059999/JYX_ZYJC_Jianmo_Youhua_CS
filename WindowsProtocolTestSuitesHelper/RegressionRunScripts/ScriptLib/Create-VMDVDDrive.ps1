#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-VMDVDDrive.ps1
## Purpose:        Create a new DVD Drive and add to Virtural Machine.
## Version:        1.0 (8 July, 2008)
##
##############################################################################
param(
[string]$VMName,
[string]$controller,
[string]$slot
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-VMDVDDrive.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
Write-Host "`$controller = $controller" 
Write-Host "`$slot = $slot" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Create a new DVD Drive and add to Virtural Machine."
    Write-host "Param1: Virtual Machine name on which install the new DVD Drive. (Required)"    
    Write-host "Param2: IDE Controller that used to mount the DVD drive. (Required). Note that each VM only has two IDE Controller(0,1), no other value is allowed."
    Write-host "Param3: Slot of the IDE Controller. (Required). Note that each IDE Controller only has two slots(0,1), no other value is allowed."
    Write-host
    Write-host "Example1: Create-VMDVDDrive.ps1  W2K8-x86-01  0 1"
    Write-host "                Create a DVD Drive on VM named 'W2K8-x86-01', IDE Controller 0, Slot 1."
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
if ($controller -eq $null -or $controller -eq "")
{
    Throw "Parameter `$controller is required."
}
if ($slot -eq $null -or $slot -eq "")
{
    Throw "Parameter `$slot is required."
}
if(!($controller -eq 0 -or $controller -eq 1))
{
    Throw "Invalid parameter `$controller. Only 0 and 1 are allowed."
}
if(!($slot -eq 0 -or $slot -eq 1))
{
    Throw "Invalid parameter `$slot. Only 0 and 1 are allowed."
}

#----------------------------------------------------------------------------
# Get VM WMI Object
#----------------------------------------------------------------------------
Write-Host "Creating DVD Drive on VM: $VMName, IDE: Controller $controller, Slot: $slot ..."
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
Write-Host "VM ID is $vmID"

#----------------------------------------------------------------------------
# Get the IDE Controller
#----------------------------------------------------------------------------
$controllerList = get-wmiobject -namespace root\virtualization Msvm_ResourceAllocationSettingData | where {$_.ResourceSubType -like "*Emulated*IDE*"} 
$targetController = $null
foreach ($controllerObj in $controllerList)
{ 
    if (($controllerObj.Address -eq $controller) -and ($controllerObj.InstanceID.ToString().Contains("$vmID"))) 
    {
        $targetController = $controllerObj
    }
} 
if ($targetController -eq $null)
{
    Throw "Error: Cannot get the specified IDE Controller: `"$controller`"."
}

#----------------------------------------------------------------------------
# Create the DVD Drive
#----------------------------------------------------------------------------
$defaultDVDDrive = get-wmiobject -namespace root\virtualization Msvm_ResourceAllocationSettingData | where {($_.ResourceSubType -like "Microsoft Synthetic DVD Drive") -and ($_.InstanceID -like "*Default*" )}   
$newDVDDrive = $defaultDVDDrive.psbase.Clone()
$newDVDDrive.Parent = $targetController.__Path
$newDVDDrive.Address = $slot
$result = $vmService.AddVirtualSystemResources($vm.__PATH, $newDVDDrive.psbase.Gettext(1)) 

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Create-VMDVDDrive.ps1]..." -foregroundcolor yellow
if ($result.ReturnValue -ne 0)
{
    $errorcode = $result.ReturnValue
    Throw "EXECUTE [Create-VMDVDDrive.ps1] FAILED. Error code: " + $errorcode
}
else
{
    Write-Host "EXECUTE [Create-VMDVDDrive.ps1] SUCCEED." -foregroundcolor green
}
