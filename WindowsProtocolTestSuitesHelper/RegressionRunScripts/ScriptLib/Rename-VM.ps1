#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Rename-VM.ps1
## Purpose:        Rename a VM in the Hyper-V.
## Version:        1.0 (10 Nov, 2008)
##
##############################################################################

Param(
$scrVMName, 
$newVMName,
$server="."
)
#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Rename-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$scrVMName = $scrVMName" 
Write-Host "`$newVMName = $newVMName" 
Write-Host "`$server = $server" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script rename an existing virtual machine in Hyper-V."
    Write-host
    Write-host "Example: Rename-VM.ps1 W2k3-x86-01 W2k3-x86-05"
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
if ($scrVMName -eq $null -or $scrVMName -eq "")
{
    Throw "Parameter sourceVMName is required."
}
if ($newVMName -eq $null -or $newVMName -eq "")
{
    Throw "Parameter New VMName is required."
}
#----------------------------------------------------------------------------
# Start Rename an VM
#----------------------------------------------------------------------------
Write-Host "Start Rename an VM ..." 
#Get a VMManagementService object
$VMManagementService = gwmi -class "Msvm_VirtualSystemManagementService" -namespace "root\virtualization" -computername "." 
#Get the VM object that we want to modify
$query = "SELECT * FROM Msvm_ComputerSystem WHERE ElementName='" + $scrVMName + "'"
$VM = gwmi -query $query -namespace "root\virtualization" -computername "." 
#Verify whether the source VM is exist in the Hyper-V
if($VM -eq $null)
{
    Throw "Source VM is not exist."
}
#Get the VirtualSystemSettingsData of the VM we want to modify
$query = "Associators of {$VM} WHERE AssocClass=MSVM_SettingsDefineState"
$VMSystemSettingData = gwmi -query $query -namespace "root\virtualization" -computername "." 
#Change the ElementName property
$VMSystemSettingData.ElementName = $newVMName 
#Update the VM with ModifyVirtualSystem
$Result = $VMManagementService.ModifyVirtualSystem($VM.__PATH,$VMSystemSettingData.psbase.GetText(1))

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Rename-VM.ps1]..." 
if($Result.ReturnValue -eq 0)
{
    Write-Host "EXECUTE [Rename-VM.ps1] SUCCEED." -foregroundcolor green
}
else
{
    Throw "EXECUTE [Rename-VM.ps1] Failed." 
}

exit