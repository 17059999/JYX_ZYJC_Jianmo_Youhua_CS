#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-VM.ps1
## Purpose:        Create a VM and mount a VHD in this VM.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

Param (
[string]$VMName      ="Win7-x86-01", 
[string]$VHDPath     ="D:\Win7VHD\Enterprise\6731.1.080613-2011\WIN7-x86-01.vhd",
[string]$memoryInGB  ="1GB",
[string]$NICName     ="Internal"  
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName      = $VMName" 
Write-Host "`$VHDPath     = $VHDPath" 
Write-Host "`$memoryInGB  = $memoryInGB" 
Write-Host "`$NICName     = $NICName" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Create a VM and mount a VHD to the vitual matchine. Set the memory and NIC (Legacy)."
    Write-host
    Write-host "Example: Create-VM.ps1 WIN7-x86-01 D:\Win7VHD\Enterprise\6731.1.080613-2011\WIN7-x86-01.vhd 1GB Internal"
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
# Delete all previous VMs
#----------------------------------------------------------------------------
#.\Delete-AllVM.ps1

if ($VMName -ne "")
{
    #----------------------------------------------------------------------------
    # Remove a VM if its name already exists in HyperV manager (if not, the start vm will fail because the VM files are being used by HyperV)
    #----------------------------------------------------------------------------
    $VMSvr = Get-WmiObject -namespace "Root\Virtualization" "Msvm_VirtualSystemManagementService"
    $VM = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' and elementName= '$VMName'"
    
    if($VM -ne $null)
    {
        #----------------------------------------------------------------------------
        # Destroy VM
        #----------------------------------------------------------------------------
        Write-Host "The VM $VMName was found already exist in the HyperV manager. Wait 30 seconds to destroy it..." -foregroundcolor Red
        sleep 30
        Write-Host "Destorying $VMName ..." 
        .\Destroy-VM.ps1 "$VMName"
        Write-Host "Now the previous VM of $VMName has been destroyed. It is safe to copy the VM files now." 
    }
}

#____________________________________________________________
# The following VM operation code comes from:
# http://sharepoint/sites/HyperVCode/Code%20DropBox/HyperV.ps1
#____________________________________________________________

#----------------------------------------------------------------------------
# Get VM Drive
# Example 1: Get-VMDiskController "Tenby" -server "James-2008" -IDE -ContollerID 0 | get-VMDrive
#           Gets the drives attached to IDE controller 0 in the VM named Tenby on Server James-2008
# Example 2: get-VMDrive $controller 0
#           Gets the first drive attached tothe controller pointed to by $controller.
#
#----------------------------------------------------------------------------
Filter Get-VMDrive
{Param ($Controller, $LUN )
 if ($Controller -eq $null) {$Controller=$_}
 if ($Controller -is [Array]) {$Controller | ForEach-Object {Get-VMDrive -Controller  $Controller -LUN $Lun} }
 if ($Controller -is [System.Management.ManagementObject]) {
    $CtrlPath=$Controller.__Path.replace("\","\\")
    Get-WmiObject -computerName $controller.__server -Query "Select * From MsVM_ResourceAllocationSettingData Where PARENT='$ctrlPath' and Address Like '$Lun%' " -NameSpace "root\virtualization" }
 $Controller = $null
}
 
#----------------------------------------------------------------------------
# Get VM disk controller
# Example 1: Get-VM -server James-2008| Get-VMDiskController -IDE -SCSI
#           Returns all the DiskControllers for all the VMs on Server James-2008      
# Example 2: Get-VMDiskController $Tenby -SCSI -ContollerID 0
#           Returns SCSI controller 0 in the VM pointed to by $Tenby
#
#----------------------------------------------------------------------------
Filter Get-VMDiskController
{Param ($VM , $ControllerID, $server=".", [Switch]$SCSI, [Switch]$IDE )
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -machineName $VM -server $Server) }
 if ($VM -is [Array]) {if ($SCSI) {$VM | ForEach-Object {Get-VMDiskController -VM $_ -ControllerID  $ControllerID -SCSI} }
                       if ($IDE)  {$VM | ForEach-Object {Get-VMDiskController -VM $_ -ControllerID  $ControllerID -IDE } } 
                       if ((-not $scsi) -and (-not $IDE) -and ($contollerID -eq $null)) {$VM | ForEach-Object {Get-VMDiskController -VM $_ } } }
 if ($VM -is [System.Management.ManagementObject]) {
     if ((-not $scsi) -and (-not $IDE) -and ($contollerID -eq $null)) {
         if ($vm.__class -eq "Msvm_ComputerSystem") {$vm=(get-vmsettingData $vm)} 
         Get-WmiObject -Query "ASSOCIATORS OF {$vm} where resultclass= Msvm_ResourceAllocationSettingData " -computerName $vm.__server -NameSpace "root\virtualization"  | 
                where {($_.resourcesubtype -eq 'Microsoft Emulated IDE Controller')  -or ($_.resourceSubtype -eq 'Microsoft Synthetic SCSI Controller')}  }
     Else {
         if ($scsi) { $controllers=Get-WmiObject -Query "Select * From MsVM_ResourceAllocationSettingData
                                            Where instanceId Like 'Microsoft:$($vm.name)%'and resourceSubtype = 'Microsoft Synthetic SCSI Controller' " `
                                           -NameSpace "root\virtualization" -computerName $vm.__server
              if ($controllerID -eq $null) {$controllers}
                      else  {$controllers | select -first ($controllerID + 1)  | select -last 1  }    }
         if ($IDE)  { Get-WmiObject -Query "Select * From MsVM_ResourceAllocationSettingData 
                                            Where instanceId Like 'Microsoft:$($vm.name)%\\$ControllerID%'
                                            and resourceSubtype = 'Microsoft Emulated IDE Controller' " -NameSpace "root\virtualization" -computerName $vm.__server } } }
 $vm=$null
}

#----------------------------------------------------------------------------
# Add VM Drive
# Example 1: Add-VMDRIVE "tenby" 1 1 -server james-2008 
#           Adds a virtal DVD to IDE contoller 1, disk slot 1 on the VM named Tenby on Server James-2008
# Example 2: Add-VMDRIVE $tenby 0 3 -SCSI 
#           Adds a virtal hard disk drive to SCSI controller 0, LUN 3 on the VM whose info is in $tenby
# Example 3: Get-vm Core-% | Add-VMDRIVE -controllerID 0 -lun 1 -DVD
#           Adds a DVD drive to  IDE contoller 0, disk slot 1 on all the VMs on the local server whose name begins with CORE-
#
#----------------------------------------------------------------------------
Filter Add-VMDRIVE
{Param ($VM , $ControllerID=0 , $LUN, $Server="." ,   [switch]$DVD , [switch]$SCSI)
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -machineName $VM , -Server $server) }
 if ($VM -is [Array]) {if ($DVD) {$VM | ForEach-Object {Add-VMDRIVE -VM $_ -ControllerID $ControllerID -LUN $LUN -server $server  -DVD} }
                           else  {if ($scsi) {$VM | ForEach-Object {Add-VMDRIVE -VM $_ -ControllerID $ControllerID -LUN $LUN -server $server} } 
                        else {$VM | ForEach-Object {Add-VMDRIVE -VM $_ -ControllerID $ControllerID -LUN $LUN -server $server -SCSI } } }}
 # think of this as a container into which we mount a virtual disk - makes more sense with DVDs (DVD drive and DVD disk) than Hard disks
 if ($VM -is [System.Management.ManagementObject]) { 
     #Step 1. Get Resource Allocation Setting Data object for a disk, set it's parent to the Controller and the Address to the "Lun"
     if ($DVD)  {$diskRASD=NEW-VMRasd -ResType 16 -ResSubType 'Microsoft Synthetic DVD Drive' -Server $VM.__Server } 
     else       {$diskRASD=NEW-VMRasd -ResType 22 -ResSubType 'Microsoft Synthetic Disk Drive'  -Server $VM.__Server }
     if ($SCSI) {$diskRASD.parent=(Get-VMDiskController -vm $vm -ControllerID $ControllerID -SCSI).__Path }
     else       {$diskRASD.parent=(Get-VMDiskController -vm $vm -ControllerID $ControllerID -IDE).__Path  }
     $diskRASD.address=$Lun
     $arguments = @($VM.__Path, @( $diskRASD.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null, $null )
     $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     $result=$VSMgtSvc.psbase.invokeMethod("AddVirtualSystemResources", $arguments)   
     if ($result -eq 0) {"Added drive to '$($vm.elementName)'."} else {"Failed to add drive to '$($vm.elementName)', result code: $result."} }
 $vm=$null
}

#----------------------------------------------------------------------------
# Create a New VM
# Example 1: $tenby = new-VM "Tenby"
#        Creates an New-VM on the local machine using default settings, and setting the name to "Tenby", storing its  MsVM_ComputerSystem object in the variable $tenby
# Example 2: $tenby = new-VM "Tenby" -server "James-2008"
#        Ditto but on a server named "James-2008"
#
#----------------------------------------------------------------------------
Function New-VM
{Param([String]$Machinename=$(Throw("You must give a VM Name")) ,  $Server="." )
  # We're going to call Define virtual System This mode of calling doesn't seem to be on MSDN ... 
 $VSMgtSvc=Get-WmiObject -computerName $server -NameSpace "root\virtualization" -Class "MsVM_virtualSystemManagementService"
 $Result=$VSMgtSvc.defineVirtualSystem()
 if ($result.ReturnValue -eq 0) {
     "Created VM" | out-host
     $VSSD=get-wmiobject -computerName $server -namespace root\virtualization -query ("select * from Msvm_VirtualSystemSettingData where systemname= "+  $Result.definedSystem.split("=")[-1] )
 # Now modify the VM to match the parameters 
     $VSSD.ElementName=$machineName
 # Build the arguments array - the path VSSD object in XML form and a bunch of nulls
     $arguments = @($Result.definedSystem , $VSSD.psbase.GetText([System.Management.TextFormat]::WmiDtd20),$null,$null)
 # Invoke the define Virtual System method, passing the arguments.  
     if ($VSMgtSvc.psbase.InvokeMethod("ModifyVirtualSystem", $arguments) -eq 0) {  "Set VM Name" | out-Host
                                                                                     Get-WmiObject -computername $Server -NameSpace "root\virtualization" -Query ("Select * From MsVM_ComputerSystem Where Name="+ $Result.definedSystem.split("=")[-1] )} 
     else {write-error "Couldn't Set VM Name"}}
}

#----------------------------------------------------------------------------
# Add a VM disk
# Example 1: Add-VMDisk $tenby 0 1 "C:\update.iso" -DVD
#           Adds a DVD image C:\update.iso, to disk 1, contoller 0 on the VM whose info is in $tenby
# Example 2: Add-VMDisk $tenby 0 0 ((get-VHDdefaultPath) +"\tenby.vhd") 
# 
#----------------------------------------------------------------------------
Function Add-VMDISK
{Param ($VM , $ControllerID=0 , $LUN=0, $VHDPath, $server=".", [switch]$DVD , [switch]$SCSI)
 if ($VM -is [String]) {$VM=(Get-VM -machineName $VM -Server $Server) }
 # Similar to Adding the drive, but we request a different resource type, and the parent is the 'Microsoft Synthetic Disk Drive', instead of disk controller
 # Mount an ISO in a DVD drive or A VHD in a Disk drive 
 if ($VM -is [System.Management.ManagementObject]) { 
     if ($DVD)  {$diskRASD=NEW-VMRasd -resType 21 -resSubType 'Microsoft Virtual CD/DVD Disk' -server $vm.__Server } 
     else       {$diskRASD=NEW-VMRasd -resType 21 -resSubType 'Microsoft Virtual Hard Disk'   -server $vm.__Server }
     if ($SCSI) {$diskRASD.parent=(Get-VMDrive -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -SCSI) -Lun $Lun ).__Path }
     else       {$diskRASD.parent=(Get-VMDrive -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -IDE)  -Lun $lun ).__Path }
     $diskRASD.Connection=$VHDPath
     $arguments = @($VM.__Path, @( $diskRASD.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null, $null )
     $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     $result=$VSMgtSvc.psbase.invokeMethod("AddVirtualSystemResources", $arguments)   
     if ($result -eq 0) {"Added disk to '$($vm.elementName)'."} else {"Failed to add disk to '$($vm.elementName)', result code: $result."} } 
}
 

#----------------------------------------------------------------------------
# New a switch port on VM
# Not intended to be called directly, used by Add-VMNIC and SetVMNICConnection
# Adds a virtal hard disk named tenby.vhd in the Default folder , to disk 0, contoller 0 on the VM whose info is in $tenby 
# 
#----------------------------------------------------------------------------
Function New-VMSwitchPort
{Param ($virtualSwitch , $Server=".") 
 if ($Virtualswitch -is [String]) {$Virtualswitch=(Get-WmiObject -computerName $server -NameSpace "root\virtualization" -Query "Select * From MsVM_VirtualSwitch Where elementname = '$Virtualswitch' ")}
 if ($Virtualswitch -is [System.Management.ManagementObject])  {
    $SwitchMgtSvc=(Get-WmiObject -computerName $Virtualswitch.__server -NameSpace  "root\virtualization" -Query "Select * From MsVM_VirtualSwitchManagementService")
      # We're going to call Create Switch Port, and we need set up the parameter array - following info is copied from MSDN
      #  uint32 CreateSwitchPort(
      #    [in]   MsVM_SwitchService SwitchService,   // The name of a Switch 
      #    [in]   string Name,                        // A guid 
      #    [in]   string FriendlyName,                // Usually "network adapter" or "legacy network adapter"
      #    [in]   string ScopeOfResidence,
      #   [out]  MsVM_SwitchPort CreatedSwitchPort    // Path to the created port
      #);

     [String]$GUID=[System.GUID]::NewGUID().ToString()
     $arguments=@($Virtualswitch.__Path, $GUID, $GUID, $null, $null)
     $result = $SwitchMgtSvc.psbase.invokeMethod("CreateSwitchPort",$arguments) 
     if ($result -eq 0) {"Created VirtualSwitchPort on '$($virtualSwitch.elementName)' " | out-host
                         @($arguments[4]) }
     else               {"Failed to create VirtualSwitchPort on '$($virtualSwitch.elementName)': return code: $Result" | out-host} }
}


#----------------------------------------------------------------------------
# New Rasd on VM
# Not intended to be called directly, used by Add-VMNIC and SetVMNICConnection
# Adds a virtal hard disk named tenby.vhd in the Default folder , to disk 0, contoller 0 on the VM whose info is in $tenby 
# 
#----------------------------------------------------------------------------
Function New-VMRasd
{Param ($ResType, $ResSubType , $server=".")
 #Get a Resource Allocation Setting Data object  
 $allocCapsPath= ((Get-WmiObject -ComputerName $server -NameSpace "root\virtualization" -Query "Select * From MsVM_AllocationCapabilities Where ResourceType = $ResType AND ResourceSubType = '$ResSubType'").__Path).replace('\','\\')
 New-Object System.Management.Managementobject((Get-WmiObject -ComputerName $server -NameSpace "root\virtualization" -Query "Select * From MsVM_SettingsDefineCapabilities Where  valuerange=0 and Groupcomponent = '$AllocCapsPath'").partcomponent)
}


#----------------------------------------------------------------------------
# Add a VM NIC
# Example 1: Add-VMNIC $tenby (choose-VMswitch)
#         adds a VMbus nic to the server  choosing the connection from a list of switches
# Example 2: Add-VMNIC $tenby (choose-VMswitch) -legacy
#         adds a Legacy nic to the server  choosing the connection from a list of switches
# Example 3: get-vm core-% -Server James-2008 | add-vmnic -virtualSwitch "Internal Virtual Network" -legacy
#         Adds a legacy nic to those VMs on Server James-2008 which have names beginning Core- and binds them to "Internal virtual network"
# 
#----------------------------------------------------------------------------
Filter Add-VMNIC
{Param ($VM , $Virtualswitch, $mac, $server=".", [switch]$legacy )
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Machinename $VM -Server $Server) }
 if ($VM -is [Array]) {if ($legacy) {$VM | ForEach-Object {add-VmNic -VM $_ -Virtualswitch $Virtualswitch -legacy} }
                              else  {$VM | ForEach-Object {add-VmNic -VM $_ -Virtualswitch $Virtualswitch} } }
 if ($VM -is [System.Management.ManagementObject]) {
     # As before We're going to call ADD Virtual System Resources, and we need set up the parameter array...
     # Create the correct Resource Allocation Setting Data object    
     if ($Legacy) {$NicRASD = NEW-VMRasd -resType 10 -resSubType 'Microsoft Emulated Ethernet Port' -server $vm.__Server 
                   $NicRASD.ElementName= "Legacy Network Adapter"}  
     else         {$NicRASD = NEW-VMRasd -resType 10 -resSubType 'Microsoft Synthetic Ethernet Port' -server $vm.__Server 
                   $NicRASD.VirtualSystemIdentifiers=@("{"+[System.GUID]::NewGUID().ToString()+"}")
                   $NicRASD.ElementName= "VMBus Network Adapter"}     
     if ($virtualSwitch -ne $null) {$Newport = new-VmSwitchport $virtualSwitch
                                    if ($Newport -eq $null) {$Newport= ""}
                                    $NicRASD.Connection= $newPort}
      if ($mac -ne $null) {$nicRasD.address = $mac
               $nicRasD.StaticMacAddress = $true }
      #Now calling AddVirtualSystemResources as usual
      $arguments = @($VM.__Path, @( $nicRASD.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null, $null )
      $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
      $result = $VSMgtSvc.psbase.invokeMethod("AddVirtualSystemResources", $arguments) 
      if ($result  -eq 0) {"Added NIC to '$($VM.elementname)'."} else {"Failed to add NIC to '$($VM.elementname)', return code: $Result" }}
  $vm = $null
}


#----------------------------------------------------------------------------
# Set VM memory
# Example 1: set-VMmemory "Tenby" 1.5gb -server James-2008
#           sets the VM named "Tenby" on server James-2008 to have 1.5GB of RAM
# Example 2: Get-vm Core-% | set-VMmemory -memory 1073741824
#           Gives 1GB of RAM to all the VMs on the local server whose names begin CORE-
# 
#----------------------------------------------------------------------------
Filter Set-VMMemory
{Param ($VM , $memory, $server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Machinename $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Set-VMMemory -VM $_ -Memory $memory -Server $Server} }
   # We're going to call Modify Virtual System Resources, and we need set up the parameter array - following info is copied from MSDN
   #   uint32 ModifyVirtualSystemResources(
   #     [in]   CIM_ComputerSystem Ref ComputerSystem,  // Reference to the virtual computer system whose resources are to be modified.
   #     [in]   string ResourceSettingData[],           // Array of embedded instances of the CIM_ResourceAllocationSettingData class 
   #                                                    //  that describe the resources to be modified 
   #     [out]  CIM_ConcreteJob Ref Job,                //  Optional reference that is returned if the operation is executed asynchronously. );
 if ($VM -is [System.Management.ManagementObject]) {
     # Step 1 Get a MsVM_MemorySettingData object - a subclass of Resource-Allocation-Setting-Data
     $memSettingData=Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Query "select * from Msvm_MemorySettingData where instanceId Like 'Microsoft:$($vm.name)%' "
     $memsettingData.Limit           =$Memory / 1MB
     $memsettingData.Reservation     =$Memory / 1MB
     $memsettingData.VirtualQuantity =$Memory / 1MB
 
     # Step 2 build the arguments array - the WMI path for the server, the Memory Setting Data object in XML form and a null for the job ID
     $arguments=@($VM.__Path, @($memsettingData.psbase.GetText([System.Management.TextFormat]::WmiDtd20)) , $null)
     $VSMgtSvc = (Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService") 
     #          invoke its define Modify Virtual System Resources method, passing the arguments.
     # If this method is executed synchronously, it returns 0 if it succeeds. 
     # If this method is executed asynchronously, it returns 4096 and the Job output parameter can be used to track progress  
     # Any other return value indicates an error
     # So return "Success" or "Failure" 
     $result=$VSMgtSvc.psbase.invokeMethod("ModifyVirtualSystemResources",$arguments)  
     if ($result -eq 0) {"Set memory for '$($vm.elementName)' to $memory."} else {"Failed to set memory for '$($vm.elementName)', result code: $result."} }
 $vm=$null
}

#----------------------------------------------------------------------------
# Call a utility tool from hyper-v code drop
#----------------------------------------------------------------------------  

$myVM = (New-VM $VMName)

$message = set-vmMemory $myVM $memoryInGB
Write-Host $message -foregroundcolor Green

$message = add-vmdrive $myVM 0 0
Write-Host $message -foregroundcolor Green

$message = add-vmdisk $myVM 0 0 $VHDPath
Write-Host $message -foregroundcolor Green

$message = add-vmdrive $myVM 1 0 -DVD
Write-Host $message -foregroundcolor Green

$message = Add-VMNIC $myVM $NICName -legacy
Write-Host $message -foregroundcolor Green

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Create-VM.ps1]..." -foregroundcolor yellow
Write-Host "EXECUTE [Create-VM.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit

