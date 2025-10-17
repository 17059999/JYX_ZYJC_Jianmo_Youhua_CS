#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Mount-VMISO.ps1
## Purpose:        Mount a ISO image to the specified vitual matchine.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

Param(
$VMName, 
$ISOPath,
$ControllerID=1, 
$LUN=0, 
$server="."
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Mount-VMISO.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
Write-Host "`$ISOPath = $ISOPath" 
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
    Write-host "Usage: Mount a iso image to  the specified vitual matchine"
    Write-host
    Write-host "Example: Mount-VMISO.ps1 VMName 'C:\update.iso' 1 0 ."
    Write-host "Mount a DVD image C:\update.iso, to disk 0, contoller 1 on the VM whose info is in VMName"
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
if ($ISOPath -eq $null -or $ISOPath -eq "")
{
    Throw "Parameter: Path of the ISO Image is required."
}
if((Test-Path -Path $ISOPath) -eq $False)
{
    Throw "the ISO Image was not found."
}

#----------------------------------------------------------------------------
# Functions for managing VM information / status
#----------------------------------------------------------------------------
Function Get-VM 
{Param ($machineName="%", $Server=".") 
     Get-WmiObject -computername $Server -NameSpace "root\virtualization" -Query "Select * From MsVM_ComputerSystem Where ElementName Like '$machineName' AND Caption Like 'Virtual%' "
}

#----------------------------------------------------------------------------
# Functions for working disk objects , SCSI Controller, Driver, Disk
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

Function Add-VMDISK
{Param ($VM , $ControllerID=0 , $LUN=0, $VHDPath, $server=".", [switch]$DVD , [switch]$SCSI)
 if ($VM -is [String]) {$VM=(Get-VM -machineName $VM -Server $Server) }
 # Similar to Adding the drive, but we request a different resource type, and the parent is the 'Microsoft Synthetic Disk Drive', instead of disk controller
 # Mount an ISO in a DVD drive or A VHD in a Disk drive 
     if ($VM -is [System.Management.ManagementObject]) 
     { 
        if ($DVD)  {$diskRASD=Get-VMRASD -resType 21 -resSubType 'Microsoft Virtual CD/DVD Disk' -server $vm.__Server } 
        else       {$diskRASD=Get-VMRASD -resType 21 -resSubType 'Microsoft Virtual Hard Disk'   -server $vm.__Server }
        if ($SCSI) {$diskRASD.parent=(Get-VMDrive -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -SCSI) -Lun $Lun ).__Path }
        else       {$diskRASD.parent=(Get-VMDrive -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -IDE)  -Lun $lun ).__Path }
        $diskRASD.Connection=$VHDPath
        $arguments = @($VM.__Path, @( $diskRASD.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null, $null )
        $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
        $result=$VSMgtSvc.psbase.invokeMethod("AddVirtualSystemResources", $arguments)   
        return $result
    } 
     
}
#----------------------------------------------------------------------------
# Mount the ISO Image to vitual matchine
#----------------------------------------------------------------------------
$result = (Add-VMDISK $VMName  $ControllerID $LUN $ISOPath $server -DVD)

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Mount-VMISO.ps1]..." -foregroundcolor yellow
if ($result -ne 0)
{
    Throw "EXECUTE [Mount-VMISO.ps1] FAILED."
}
else
{
    Write-Host "EXECUTE [Mount-VMISO.ps1] SUCCEED." -foregroundcolor green
}

exit
