#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Remove-VMISO.ps1
## Purpose:        Remove a ISO image from the specified vitual matchine.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

Param(
$VMName,
$ControllerID=1,
$LUN=0, 
$server="."
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Remove-VMISO.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
Write-Host "`$ControllerID = $ControllerID" 
Write-Host "`$LUN = $LUN" 
Write-Host "`$server = $server" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options

#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: remove a iso image from  the specified vitual matchine"
    Write-host
    Write-host "Example: Remove_ISO.ps1 VMName 1 0 ."
    Write-host "Remove the Image at disk 0, contoller 1 on the VM whose info is in VMName"
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
    Throw "Parameter: name of the virtual matchine is required."
}

#----------------------------------------------------------------------------
#
# Functions for managing VM information / status
#
#----------------------------------------------------------------------------

Function Get-VM 
{Param ($machineName="%", $Server=".") 
 Get-WmiObject -computername $Server -NameSpace "root\virtualization" -Query "Select * From MsVM_ComputerSystem Where ElementName Like '$machineName' AND Caption Like 'Virtual%' "
}
#example 1: Get-VM
#           Returns WMI MsVM_ComputerSystem objects for all Virtual Machines (n.b. Parent Partition is filtered out)
#Example 2: Get-VM "Server 2008 Ent Full TS"   
#        Returns a single WMI MsVM_ComputerSystem object for the VM named "Server 2008 ENT Full TS"
#Example 3: Get-VM "%2008%" 
#        Returns WMI MsVM_ComputerSystem objects for machines containing 2008 in their name (n.b.Wild card is % sign not *) 

#----------------------------------------------------------------------------
#
# Functions for working disk objects , SCSI Controller, Driver, Disk
#
#----------------------------------------------------------------------------

Filter Get-VMDiskController
{Param ($VM , $ControllerID, $server=".", [Switch]$SCSI, [Switch]$IDE )
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [Array]) {if ($SCSI) {$VM | ForEach-Object {Get-VMDiskController -VM $_ -ControllerID  $ControllerID -SCSI} }
                       if ($IDE)  {$VM | ForEach-Object {Get-VMDiskController -VM $_ -ControllerID  $ControllerID -IDE } } }
 if ($VM -is [String]) {$VM=(Get-VM -machineName $VM -server $Server) }
 if ($VM -is [System.Management.ManagementObject]) {
     if ($scsi) { $controllers=Get-WmiObject -Query "Select * From MsVM_ResourceAllocationSettingData
                                        Where instanceId Like 'Microsoft:$($vm.name)%'and resourceSubtype = 'Microsoft Synthetic SCSI Controller' " `
                                       -NameSpace "root\virtualization" -computerName $vm.__server
          if ($controllerID -eq $null) {$controllers}
                  else  {$controllers | select -first ($controllerID + 1)  | select -last 1  }    }
     if ($IDE)  { Get-WmiObject -Query "Select * From MsVM_ResourceAllocationSettingData 
                                        Where instanceId Like 'Microsoft:$($vm.name)%\\$ControllerID%'
                                        and resourceSubtype = 'Microsoft Emulated IDE Controller' " -NameSpace "root\virtualization" -computerName $vm.__server } }
 $vm=$null
}

Filter Get-VMDrive
{Param ($Controller, $LUN )
 if ($Controller -eq $null) {$Controller=$_}
 if ($Controller -is [Array]) {$Controller | ForEach-Object {Get-VMDrive -Controller  $Controller -LUN $Lun} }
 if ($Controller -is [System.Management.ManagementObject]) {
    $CtrlPath=$Controller.__Path.replace("\","\\")
    Get-WmiObject -computerName $controller.__server -Query "Select * From MsVM_ResourceAllocationSettingData Where PARENT='$ctrlPath' and Address Like '$Lun%' " -NameSpace "root\virtualization" }
 $Controller = $null
}

Function Get-VMRASD
{Param ($ResType, $ResSubType , $server=".")
 #Get a Resource Allocation Setting Data object  
 $allocCapsPath= ((Get-WmiObject -ComputerName $server -NameSpace "root\virtualization" -Query "Select * From MsVM_AllocationCapabilities Where ResourceType = $ResType AND ResourceSubType = '$ResSubType'").__Path).replace('\','\\')
 New-Object System.Management.Managementobject((Get-WmiObject -ComputerName $server -NameSpace "root\virtualization" -Query "Select * From MsVM_SettingsDefineCapabilities Where  valuerange=0 and Groupcomponent = '$AllocCapsPath'").partcomponent)
}

Filter Get-VMDisk
{Param ($Drive)
 if ($Drive -eq $null) {$Drive=$_}
 if ($Drive -is [Array]) {$Controller | ForEach-Object {Get-VMDrive -Controller  $Controller -LUN $Lun} }
 if ($Drive -is [System.Management.ManagementObject]) {
    $DrivePath=$Drive.__Path.replace("\","\\")
    Get-WmiObject -computerName $drive.__server -Query "Select * From MsVM_ResourceAllocationSettingData Where PARENT='$DrivePath' " -NameSpace "root\virtualization" }
 $Controller = $null
}

Function Remove-VMdrive
{Param( $VM, $controllerID, $LUN, $server="." , [switch]$scsi, [switch]$Diskonly )
    if ($VM -is [String]) {$VM=(Get-VM -machineName $VM -server $Server) }
    if ($SCSI) {$drive=(Get-VMDrive -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -SCSI) -Lun $Lun )}
    else       {$drive=(Get-VMDrive -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -IDE)  -Lun $lun )}
    if ($drive -is [System.Management.ManagementObject]) {
        $disk=$drive | get-vmdisk
        $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
        if ($disk -is [System.Management.ManagementObject]) 
        {
            $arguments = @($VM.__Path, @( $disk.__Path ), $null ) 
            $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
            return $result
        }    
        if (-not $diskOnly) 
        {  
            $arguments = @($VM.__Path, @( $drive.__Path ), $null ) 
            $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
            return $result

        }
    }    
}

#----------------------------------------------------------------------------
#Remove the Image
#----------------------------------------------------------------------------
$Result = (Remove-VMdrive $VMName  $ControllerID $LUN  $server -Diskonly)

#----------------------------------------------------------------------------
#Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Remove-VMISO.ps1]..." -foregroundcolor yellow
if ($result -eq 0) 
{
    Write-Host "EXECUTE [Remove-VMISO.ps1] SUCCEED." -foregroundcolor green
} 
else 
{
    throw "EXECUTE [Remove-VMISO.ps1] FAILED. ERROR CODE: $result" 
}