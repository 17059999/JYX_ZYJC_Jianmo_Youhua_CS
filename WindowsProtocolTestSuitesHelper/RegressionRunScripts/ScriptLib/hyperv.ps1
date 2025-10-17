####################################################################################################
#                                                                                                  #
# This script contains the common functions of operating HyperV with PowerShell.                   #
# The hyperv.format.ps1xml file will be used while loading these function to configure hyperv.ps1. #
# You can use ". .\hyperv.ps1" to call the functions listed in this script file.                   #
# Please see PShyperv.pdf for more information about the function list.                            #
#                                                                                                  #
####################################################################################################

Param ([switch]$quiet) 

##########################################
#                                        #  
#Global variables:                       # 
#                                        # 
##########################################
$VMState=       @{"Running"=2 ; "Stopped"=3 ; "Paused"=32768 ; "Suspended"=32769 ; "Starting"=32770 ; "Snapshotting"=32771 ; "Saving"=32773  ; "Stopping"=32774 }
$ReturnCode=    @{"0"="OK" ; "4096"="Job Started" ; "32768"="Failed"; "32769"="Access Denied" ; "32770"="Not Supported"; "32771"="Unknown" ; "32772"="Timeout" ; "32773"="Invalid parameter" ;
                  "32774"="System is in use" ; "32775"="Invalid state for this operation" ; "32776"="Incorrect data type" ; "32777"="System is not available" ; "32778"="Out of memory" }
$BootMedia=     @{"Floppy"=0 ; "CD"=1 ; "IDE"=2 ; "NET"=3 }
$StartupAction =@{"None"=0 ; "RestartOnly"=1 ; "AlwaysStartup"=2}
$ShutDownAction=@{"TurnOff"=0 ; "SaveState"=1 ; "ShutDown"=2}
$Recoveryaction=@{"None"=0 ; "Restart"=1 ; "RevertToSnapShot"=2}
$DiskType=      @{"Fixed"=2; "Dynamic"=3; "Differencing"=4; "PhysicalDrive"=5}

###################################################################
#                                                                 #
# Helper Functions - not related to any specific Hyper-V objects  #
#                                                                 #
###################################################################
Function new-zip
{Param ($zipFile)
 if (-not $ZipFile.EndsWith('.zip')) {$ZipFile += '.zip'} 
 set-content $ZipFile ("PK" + [char]5 + [char]6 + ("$([char]0)" * 18)) 
}


Filter Add-ZIPContent 
{Param ($zipFile=$(throw "You must specify a Zip File"), $files)
 if ($files -eq $null) {$files = $_}
 if (-not $ZipFile.EndsWith('.zip')) {$ZipFile += '.zip'} 
 if (-not (test-path $Zipfile)) {new-zip $ZipFile}
 $ZipObj = (new-object -com shell.application).NameSpace(((resolve-path $ZipFile).path))

 $files | foreach { if ($_ -is [String]) {$zipObj.CopyHere((resolve-path $_).path )}
		        elseif (($_ -is [System.IO.FileInfo]) -or ($_ -is [System.IO.DirectoryInfo]) ) {$zipObj.CopyHere($_.fullname) }
		    start-sleep -seconds 2} 
 $files = $null
}


Function Copy-ZipContent
{Param ($zipFile, $Path)
    if(test-path($zipFile)) {$shell = new-object -com Shell.Application
                             $destFolder = $shell.NameSpace($Path)
                             $destFolder.CopyHere(($shell.NameSpace($zipFile).Items()))}        
}


Function Get-ZIPContent 
{Param ($zipFile=$(throw "You must specify a Zip File"), [Switch]$Raw, $indent=0)
 if ($indent -eq 0) {
    if (-not $ZipFile.EndsWith('.zip')) {$ZipFile += '.zip'} 
    $ZipFile = (resolve-path $ZipFile).path
    }
        $shell = new-object -com Shell.Application
        $NS=$shell.NameSpace($zipFile)
        $Files = $(foreach ($item in $NS.items() ) {0..8 | foreach -begin   {$ZipObj = New-Object -TypeName System.Object  } `
                                                                   -process {Add-Member -inputObject $ZipObj -MemberType NoteProperty -Name ($NS.GetDetailsOf($null,$_)) -Value ($NS.GetDetailsOf($Item,$_).replace([char]8206,"")) }  `
                                                                   -end     {if ($item.isfolder) {Add-Member -inputObject $ZipObj -MemberType NoteProperty -Name Formatting -value (("| " * $indent) + "+-")
                                                                                                  $ZipObj 
                                                                                                  Get-ZipContent -zipfile $item.path -raw -indent ($indent +1 ) }
                                                                             else                {Add-Member -inputObject $ZipObj -MemberType NoteProperty -Name Formatting -value (("| " * ($indent)) + "|-")
                                                                                                  if ($indent -eq 0) {$ZipObj.formatting = "|-"}
                                                                                                  $zipObj  } }}) 
        if ($raw) {$files} else {$files | format-Table  -property @{label="File"; expression={$_.Formatting + $_.Name}},Type,"Date Modified",Size,"Compressed Size",Ratio,Method -autosize}                                          
    
}


Function Get-ScriptPath
{ split-path $myInvocation.scriptName }


Function Choose-List
{Param ($InputObject, $Property, [Switch]$multiple)
 $Global:counter=-1
 $Property=@(@{Label="ID"; Expression={ ($global:Counter++) }}) + $Property
 if ($inputObject -is [Array]) {
     $InputObject | format-table -autosize -property $Property | out-host
     if ($multiple) { $InputObject[ [int[]](Read-Host "Which one(s) ?").Split(",")] }
     else           { $InputObject[        (Read-Host "Which one ?")              ] }}
 else {$inputObject}
}


Function Out-Tree
{Param ($items, $startAt, $path=("Path"), $parent=("Parent"), $label=("Label"), $indent=0)
 $children = $items | where-object {$_.$parent -eq $startAt.$path.ToString()} 
 if ($children -ne $null) {("| " * $indent) + "+$($startAt.$label)" 
                            $children | ForEach-Object {Out-Tree $items $_ $path $parent $label ($indent+1)} }
 else                     {("| " * ($indent-1)) + "|--$($startAt.$label)" }
}


Function Choose-Tree
{Param ($items, $startAt, $path=("Path"), $parent=("Parent"), 
        $label=("Label"), $indent=0, [Switch]$multiple)
 if ($Indent -eq 0)  {$Global:treeCounter = -1 ;  $Global:treeList=@() ; $Leader="" }
 $Global:treeCounter++
 $Global:treeList=$global:treeList + @($startAt)
 $children = $items | where-object {$_.$parent -eq $startat.$path.ToString()} 
 if   ($children -ne $null) 
      {  $leader = "| " * ($indent) 
         "{0,-4} {1}+{2} "-f  $Global:treeCounter, $leader , $startAt.$label | Out-Host
        $children | sort-object $label | 
           ForEach-Object {Choose-Tree -Items $items -StartAt $_ -Path $path `
                           -parent $parent -label $label -indent ($indent+1)}
      }
 else {  $leader = "| " * ($indent-1) 
        "{0,-4} {1}|--{2} "-f  $global:Treecounter, $leader , $startAt.$Label  | out-Host }
 if ($Indent -eq 0) {if ($multiple) { $Global:treeList[ [int[]](Read-Host "Which one(s) ?").Split(",")] }
                     else           {($Global:treeList[ (Read-Host "Which one ?")]) }  
                    } 
}


Filter Convert-DiskIDtoDrive
{Param ($diskIndex)
 if ($diskIndex -eq $null) {$diskIndex = $_}
 Get-WmiObject -Query "Select * From Win32_logicaldisktoPartition Where __PATH Like '%disk #$diskIndex%' " | % {$_.dependent.split("=")[1].replace('"','')}  
}
#Example: Convert-DiskIDtoDrive 2 
#          Returns the Drive letters of the partions on Disk 2 (in the form D:, E: - with the colon, and no backslash)


Filter Test-WMIJob 
{  param  ($JobID, [Switch]$Wait, $Description="Job")   
   if ($jobID -eq $null) {$jobID = $_}  
   $Job = [WMI]$JobID
   if ($job -ne $null) {
	while (($job.jobstate -eq 4) -and $wait) { 
            Write-Progress -activity ("$Description $($Job.Caption)") -Status "% complete" -PercentComplete $Job.PercentComplete 
            Start-Sleep -seconds 1 
            $Job.PSBase.Get() }  
	$Description +": " + @{2="New"; 3="Starting"; 4="Running $($job.PercentComplete)%"; 5="Suspended"; 6="Shutting Down"; 7="Completed"; 8="Terminated"; 9="Killed"; 10="Exception: $($job.ErrorDescription)"; 11="Service"}[[int]$Job.JobState] | out-host
        $Job.Status
}}
#Example $job=Expand-VHD 'C:\users\Public\Documents\Microsoft Hyper-V\Virtual Hard Disks\foo1.vhd' 22gb
#        Test-WmiJob $job
#        Stores the Job ID and then checks its status later. 


Function test-Admin
{Param ([Switch] $verbose)
  dir Microsoft.PowerShell.Core\Registry::HKEY_USERS\s-1-5-20 -ErrorAction silentlycontinue –errorVariable MyErr | out-null
   if ($verbose) {if ($myErr) { write-host -ForegroundColor Red   "This session does not have elevated priviledges."}
                 else         { write-host -ForegroundColor Green "This session has elevated priviledges" }     }
 if ($myErr) {$false} else {$true}
}
#Example: if (-not (test-admin)) {write-host -foregroundColor Red "This Powershell session does not have elevated priviledges."}


#########################################################
#                                                       #
# Functions for Managing Virtual Hard disk (VHD) files  #
#                                                       #
#########################################################
Function Get-VhdDefaultPath
{ param ($server=".") 
 (Get-WmiObject -computerName $server -NameSpace "root\virtualization" -Class "MsVM_VirtualSystemManagementServiceSettingData").DefaultVirtualHardDiskPath
}


Function New-VFD
{Param ([String]$vfdPath=$(Throw("You must specify a Path for the disk")), $server="." , [Switch]$wait)
#Note that the path needs to be either full qualified, or the current folder ".\foo.vhd", or not specified "foo.vhd" - when a default will be assumed
#It was a choice between checking the path or allowing the command to run remotely. I picked the latter. 
   # we're going to call the CreateVirtualFloppyDisk method of the Image Management service
   # According to MSDN it's parameters are as follows 
   #     uint32 CreateVirtualFloppyDisk(
   #     [in]   string Path,
   #     [out]  CIM_ConcreteJob REF Job
 $ImgMgtSvc=Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"
 if ($vfdpath.StartsWith(".\")) {$VFDpath= join-path $PWD $vfdPath.Substring(2)  }
 else                           { if ((split-path $VFDPath)  -eq "" ) {$vfdPath  = join-path (Get-VhdDefaultPath ) $vfdPath } }
 if (-not $vfdpath.toUpper().endswith("VFD")) {$vfdPath = $vfdPath + ".vfd"}
 $arguments = @($vfdPath,$null)       
 $result=$ImgMgtSvc.PSbase.InvokeMethod("CreateVirtualFloppyDisk",$arguments ) 
 if ($wait) {test-wmijob $arguments[1]  -wait -Description "VFD Creation of $vfdPath" } 
 else       {$ReturnCode[[string]$result] | Out-host 
             $arguments[1] }
}
#Example New-VFD "Floppy.VFD"
#        Creates a new floppy disk in the default folder. 

         
Function New-VHD
{Param ([String]$vhdPath=$(Throw("You must specify a Path for the disk")) , [int64]$size, $parentDisk, $server="." ,[Switch]$Fixed ,[Switch]$wait)
#Note that the path needs to be either fully qualified, or the current folder ".\foo.vhd", or not specified "foo.vhd" - when a default will be assumed
#It was a choice between checking the path or allowing the command to run remotely. I picked the latter. 
   # we're going to call the Create xxx Virtual Hard disk method of the Image Management service
   # According to MSDN it's parameters are as follows 
   #   uint32 CreateDynamicVirtualHardDisk(
   #     [in]   string Path,
   #     [in]   uint64 MaxInternalSize (for fixed and dynamic) or String ParentPath for differencing
   #     [out]  CIM_ConcreteJob Ref Job);  
 $ImgMgtSvc=Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"
 if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
 else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath $Server) $vhdPath } }
 if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
 if ($parentDisk -ne $null) {$arguments = @($vhdPath,$parentDisk,$null)
                             $result= $ImgMgtSvc.PSbase.InvokeMethod("CreateDifferencingVirtualHardDisk",$arguments) }
 else { if ($size -lt 1gb) {Throw("You must Specify a disk Size")}
        $arguments = @($vhdPath,$size,$null)
        if ($fixed)  
           { $result=$ImgMgtSvc.PSbase.InvokeMethod("CreateFixedVirtualHardDisk",$arguments ) }
        else 
           { $result=$ImgMgtSvc.PSbase.InvokeMethod("CreateDynamicVirtualHardDisk",$arguments ) }
      }
  if ($wait) {test-wmijob $arguments[2]  -wait -Description "VHD Creation of $VhdPath "} 
 else       {$arguments[2] 
             $ReturnCode[[string]$result] | Out-host }
}
#Example 1: New-VHD "$(Get-VhdDefaultPath)\tenby.vhd" 20GB
#           Creates a 20GB Dynamic VHD named "tenby.vhd" in the default VHD folder
#Example 2: New-VHD "$(Get-VhdDefaultPath)\tenby.vhd" 20GB -fixed -wait
#           Creates a 20GB Fixed VHD named "tenby.vhd" in the default VHD folder, waiting until it has completed before continuing
#Example 3: $diskJob=New-VHD "$(Get-VhdDefaultPath)\tenby.vhd" 20GB
#           Stores the ID of the Job which creates the VHD, this can be then passed to Test-WMIJob to check the status.


Filter Mount-VHD
{Param ($vhdPath , $Partition, $Letter, [Switch]$offline)
# Does not make sense to remote mount/unmount a disk
 if ($VHDPath -eq $null) {$VHDPath = $_}
 if ($VHDPath -is [Array]) {if ($offline) {$VHDPath | ForEach-Object {Mount-vhd -VHDPath $_ offline} }
                                     else {$VHDPath | ForEach-Object {Mount-vhd -VHDPath $_  } } }
 if ($vhdpath -is [System.IO.FileInfo]) {$VHDPath = $vhdPath.fullName}
 if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
 else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath ) $vhdPath } }
 if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
 If (test-path $VHDPath) {
     $ImgMgtSvc = Get-WmiObject -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"
     $result=$ImgMgtSvc.mount($vhdPath)
     if (($result.returnValue -eq 4096) -and ((test-wmijob $result.job -wait -Description "Disk mount") -eq "OK")) 
          {Write-Progress -activity "Disk Mount" -Status "Checking for mounted disk" 
           Start-sleep 2
           $MountedDiskImage= Get-WmiObject -NameSpace "root\virtualization" -query ("Select * from Msvm_MountedStorageImage where name ='"+  $Vhdpath.replace("\","\\")  +"'")
           $diskIndex=(Get-WmiObject -Query "Select * From win32_diskdrive Where Model='Msft Virtual Disk SCSI Disk Device' and ScsiTargetID=$($MountedDiskImage.TargetId) and ScsiLogicalUnit=$($MountedDiskImage.Lun) and scsiPort=$($MountedDiskImage.PortNumber)").index
           if ($diskIndex -eq $null) {"Disk not found (yet)." | out-Host }
           elseif (-not $offline)    {Write-Progress -activity "Disk Mount" -Status "Bringing Disk on-line" 
                                      @("select disk $diskIndex", "online disk" , "attributes disk clear readonly", "exit")  | Diskpart | Out-Null 
                                      Start-Sleep -Seconds 5
				      if ($partition -and $letter) { @("select disk $diskIndex", "Select Partition $Partition", "Assign Letter=$($Letter.substring(0,1))",  "exit")  | Diskpart | out-null} 
                                      Start-Sleep -Seconds 5
                                      Convert-DiskIDtoDrive -diskIndex $diskIndex | out-host }
          write-host -noNewLine "Disk ID... "  
          $diskIndex
          }
     else {"Mount Failed: $($ReturnCode[[String]$result.returnValue])" }}
 $vhdpath=$null
}
#Example 1: dir "$(Get-VhdDefaultPath)\*.vhd" | Mount-Vhd -offline
#            Mounts all  VHDs in the default folder in an offline State and returns the IDs of the disks
#Example 2: Mount-Vhd  (get-VHDdefaultPath) +"\tenby.vhd"              
#            Mounts the VHD, brings it online and echos the Drive letters (if any), and returns the ID of the disk 


Filter UnMount-VHD
{Param ($vhdPath )
# Does not make sense to remote mount/unmount a disk
 if ($VHDPath -eq $null) {$VHDPath = $_}
 if ($VHDPath -is [Array]) {$VHDPath | ForEach-Object {UnMount-vhd -VHDPath $_  } } 
 if ($vhdpath -is [System.IO.FileInfo]) {$VHDPath = $vhdPath.fullName}
 if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
 else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath ) $vhdPath } }
 if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
 If (test-path $VHDPath) {
     $ImgMgtSvc = Get-WmiObject -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"
     $result=$ImgMgtSvc.Unmount($vhdPath)
     If ($result.ReturnValue -eq 0) {"Unmounted $vhdPath"} else {"Failed to Unmount $vhdPath, return code: $($result.ReturnValue)" }}
 $vhdpath=$null
}
#Example 1: UnMount-Vhd  (get-VHDdefaultPath) +"\tenby.vhd"
#            UnMounts the VHD 
#Example 2: dir "$(Get-VhdDefaultPath)\*.vhd"  | UnMount-Vhd  
#            Attempts to unmount all the disks in the folder - will fail gracefully if they are not mounted


Filter Compact-VHD
{Param ($vhdPath,  $Server=".", [switch]$wait)
 if ($VHDPath -eq $null) {$VHDPath = $_}
 if ($vhdpath -is [System.IO.FileInfo]) {$VHDPath = $vhdPath.fullName}
 if ($vhdPath -is [Array]) {If ($wait) {$vhdPath | foreach-Object { Compact-VHD -VHDPath $_ -Server $Server -wait} }
                            else       {$vhdPath | foreach-Object { Compact-VHD -VHDPath $_ -Server $Server } } }
 if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
 else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath ) $vhdPath } } 
 if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
 $ImgMgtSvc=Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"
 $arguments=@($vhdPath,$null)
 $result=$ImgMgtSvc.psbase.InvokeMethod("CompactVirtualHardDisk",$arguments)
 if ($wait) {test-wmijob $arguments[1]  -wait -description "Compacting $VHDPath" } 
 else       { $ReturnCode[[string]$result] | Out-host 
              $arguments[1] }

}
#Example: Compact-Vhd   (get-VHDdefaultPath) +"\tenby.vhd"
#          Compacts the VHD. you can check status with Get-WmiObject -NameSpace root\virtualization msVM_storagejob | ft jobStatus, description, percentcomplete -auto
#          Be aware percent complete doesn't update smoothly


Filter Get-VHDInfo 
{param ($vhdPath, $server=".")
   if ($vhdPath -eq $null) {$vhdPath = $_}
   if ($vhdPath -is [Array]) {$vhdPath | foreach-Object { Get-VHDInfo  -VHDPath $_ -Server $Server} }
   if ($vhdPath -is [System.IO.FileInfo]) {$vhdPath = $vhdpath.fullname}   
   if ($VhdPath -is [String]) { 
         if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
         else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath ) $vhdPath } }
         if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
	 $ImgMgtSvc=Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"
         $ARGUMENTS =@($VHDPath,$NULL)
         $Result=$ImgMgtSvc.Psbase.InvokeMethod("GetVirtualHardDiskInfo",$arguments)
         ([xml]$ARGUMENTS[1]).SelectNodes("/INSTANCE/PROPERTY")  | foreach -begin {$KVPObj = New-Object -TypeName System.Object 
									           Add-Member -inputObject $KvpObj -MemberType NoteProperty -Name "Path"  -Value $VHDPath} `
                                                                         -process {Add-Member -inputObject $KvpObj -MemberType NoteProperty -Name $_.Name -Value $_.value} `
                                                                             -end {$KvpObj} }
   $vhdPath=$null
}	
#Example 1  cd (Get-VhdDefaultPath) ; dir *.vhd | get-vhdinfo
#           Changes Location to the Default folder for VHD files, and then gets information about all the VHD files. 
#Example 2  (Get-VHDInfo 'C:\Users\Public\Documents\Microsoft Hyper-V\Virtual Hard Disks\Core.vhd').parentPath
#           Returns the parent path of core.vhd e.g. C:\Users\Public\Documents\Microsoft Hyper-V\Virtual Hard Disks\Brand-new-core.vhd
#Example 3  Get-VMDisk core% | forEach {Get-VHDInfo $_.Diskpath} | measure-object -Sum filesize
#           Gets the disks on VMs with names begining Core , gets the their details and sums the file size. 


Filter Test-VHD
{param($vhdPath , $server="." )  
 $ImgMgtSvc=Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"
 if ($VHDPath -eq $Null) {$VHDPath = $_}
 if ($VHDPath -is [Array]) {$VHDPath | ForEach-Object {Test-vhd -VHDPath $_ -server $Server} }                                     
 if ($vhdpath -is [System.IO.FileInfo]) {$VHDPath = $vhdPath.fullName}
 if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
 else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath ) $vhdPath } }
 if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
 $arguments = @($vhdPath,$null)       
 $result=$ImgMgtSvc.PSbase.InvokeMethod("ValidateVirtualHardDisk",$arguments ) 
 test-wmijob $arguments[1] -wait -description  "Validation of $vhdpath" 
 $vhdPath = $null
}
#Exmaple 1 dir "$(Get-VhdDefaultPath)\*.vhd" | Test-VHD
#          Gets all the VHD files in the default folder and checks them
#Example 2 Get-VMDisk | %{$_.DiskPath} | where {$_.endswith(".vhd")} | Test-VHD
#          Gets all disk on the VMs and validates them 


Filter Expand-VHD
{Param ($vhdPath, [int64]$size, $server=".", [Switch]$wait)
 if ($VHDPath -eq $null) {$VHDPath = $_}
 if ($VHDPath -is [Array]) {$VHDPath | ForEach-Object {Expand-vhd -VHDPath $_ -Size $size -server $Server} }                                     
 if ($vhdpath -is [System.IO.FileInfo]) {$VHDPath = $vhdPath.fullName}
 if ($vhdPath -is [String]) {
     if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
     else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath ) $vhdPath } }
     if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
     $ImgMgtSvc=Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"    
     if ($size -lt (Get-VHDInfo $VhdPath $server).MaxInternalSize) {Throw("The new Size must be BIGGER")}
     Else {$arguments = @($vhdPath,$size,$null)
           $result=$ImgMgtSvc.PSbase.InvokeMethod("ExpandVirtualHardDisk",$arguments ) 
           if ($wait) {test-wmijob $arguments[2]  -wait -description "VHD Expansion of $vhdpath"} 
           else       {$ReturnCode[[string]$result] | Out-host 
		       $arguments[2]} } }
}
#Example  Expand-VHD 'C:\users\Public\Documents\Microsoft Hyper-V\Virtual Hard Disks\foo1.vhd' 22gb
#         Increases the size of the VHD to 22GB. NB. This will not expand the partition(s) on the disk, that needs to be done seperately. 


#ToDo: Merge-VHD is untested: use at your own risk
# this function is geared to merging a child disk into its parent provided for convenience, to merge to a new disk use convert. 
Function Merge-VHD
{Param ($vhdPath=$(throw "You must specify a VHD") , [String]$DestPath=$(throw "You must specify a Destination") , $server="." ,  [Switch]$wait)
   # we're going to call the MergeVirtualHardDisk method of the Image Management service
   # According to MSDN it's parameters are as follows 
   #    uint32 MergeVirtualHardDisk(
   #     [in]   string SourcePath,       <the location of the merging file.>
   #     [in]   string DestinationPath,  <the location of the parent disk image file into which data is to be merged>
   #     [out]  CIM_ConcreteJob REF Job
 $ImgMgtSvc=Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"             
 if ($vhdpath -is [System.IO.FileInfo]) {$VHDPath = $vhdPath.fullName}
 if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
 else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath ) $vhdPath } }
 if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
 if ($DestPath.StartsWith(".\")) {$DestPath= join-path $PWD $DestPath.Substring(2)  }
 else                           { if ((split-path $DestPath)  -eq "" ) {$DestPath  = join-path (Get-VhdDefaultPath ) $DestPath } }
 if (-not $DestPath.toUpper().endswith("VHD")) {$DestPath = $DestPath + ".vhd"}
        
 $arguments = @($vhdPath,$destPath, $null)       
 $result=$ImgMgtSvc.PSbase.InvokeMethod("MergeVirtualHardDisk",$arguments ) 
 if ($wait) {test-wmijob $arguments[2]  -wait -description  "Disk Merge into $DestPath"} 
 else       {$ReturnCode[[string]$result] | Out-host 
             $arguments[2]} 
}


Function Convert-VHD
{Param ($vhdPath=$(throw "You must specify a VHD") , [String]$DestPath=$(throw "You must specify a Destination") , [int]$type=$(throw "You must specify a type"), $server=".",  [Switch]$wait )
   # we're going to call the ConvertVirtualHardDisk method of the Image Management service
   # According to MSDN it's parameters are as follows 
   # uint32 ConvertVirtualHardDisk(
   #  [in]   string SourcePath,       location of the VHD. This file will not be modified as a result of this operation.
   #  [in]   string DestinationPath,  the location of the destination virtual hard disk file.
   #  [in]   uint16 Type,             The type of the new virtual hard disk file. "Fixed"=2; "Dynamic"=3; "Differencing"=4; "PhysicalDrive"=5
   #  [out]  CIM_ConcreteJob REF Job  
 $ImgMgtSvc=Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -Class "MsVM_ImageManagementService"             
 if ($vhdpath -is [System.IO.FileInfo]) {$VHDPath = $vhdPath.fullName}
 if ($vhdpath.StartsWith(".\")) {$VhDpath= join-path $PWD $vhdPath.Substring(2)  }
 else                           { if ((split-path $VHDPath)  -eq "" ) {$vhdPath  = join-path (Get-VhdDefaultPath ) $vhdPath } }
 if (-not $vhdpath.toUpper().endswith("VHD")) {$vhdPath = $vhdPath + ".vhd"}
 if ($DestPath.StartsWith(".\")) {$DestPath= join-path $PWD $DestPath.Substring(2)  }
 else                           { if ((split-path $DestPath)  -eq "" ) {$DestPath  = join-path (Get-VhdDefaultPath ) $DestPath } }
 if (-not $DestPath.toUpper().endswith("VHD")) {$DestPath = $DestPath + ".vhd"}
        
 $arguments = @($vhdPath,$destPath, $type, $null)       
 $result=$ImgMgtSvc.PSbase.InvokeMethod("ConvertVirtualHardDisk",$arguments ) 
 if ($wait) {test-wmijob $arguments[3]  -wait -Description "Disk Conversion into $DestPath) "} 
 else       { $ReturnCode[[string]$result] | Out-host 
              $arguments[3]} 
}
#example 1 : convert-Vhd core temp -type $disktype.dynamic 
#            Will merge a differencing disk "CORE.vhd" in the default folder (and its parent(s) ) into a new disk called temp.vhd also in the default folder
#example 2 : Convert-vhd "$( Get-VhdDefaultPath )\Temp.vhd" F:\backups\MyDisk.VHD -type $Disktype.fixed 
#            Will convert a disk to a fixed one on a different drive.        
#Example 3 : pushd (Get-VhdDefaultPath) ; dir *.vhd | get-vhdinfo | where-object {$_.type -eq 3} | foreach {"convert-vhd $($_.path) '.\temp.vhd' -type 2 -wait" ; "del  $($_.path)" ; "ren temp.vhd $($_.path)"} ; popd 
#            Move to the default VHD folder, get the VHD files, isolate the dynamic ones, convert them to a fixed size one named temp , delete the original , rename temp to the orginal name

####################################################
#                                                  #
# Functions for managing VM information / status   #
#                                                  #
####################################################

Function Get-VMHost 
{param ($domain=([adsi]('GC://'+([adsi]'LDAP://RootDse').RootDomainNamingContext)))
 $searcher= New-Object directoryServices.DirectorySearcher($domain)
 $searcher.filter="(&(cn=Microsoft hyper-v)(objectCategory=serviceConnectionPoint))"
 $searcher.Findall()   | foreach {$_.path.split(",")[1].replace("CN=","")}
} 
#Example get-vmhost   "DC=ite,DC=contoso,DC=com" | foreach {$_; list-vmstate -server $_}
#                      Queries the domain ITE.Contoso.com for Hyper-V servers,#                      prints the name of each and dumps the state of VMs on each


Function Get-VM 
{Param ([String]$Name="%", $Server=".", [Switch]$suspended, [switch]$running, [Switch]$stopped) 
 $Name=$Name.replace("*","%")
 $WQL="Select * From MsVM_ComputerSystem Where ElementName Like '$Name' AND Caption Like 'Virtual%' "
 if ($running -or $stopped -or $suspended) {
    [String]$state = ""
    if ($running)  {$State +="or enabledState=" +  $VMState["running"]  }
    if ($Stopped)  {$State +="or enabledState=" +  $VMState["Stopped"]  }
    if ($suspended){$State +="or enabledState=" +  $VMState["suspended"]}
    $WQL += "AND (" + $state.substring(3) +")" }
 Get-WmiObject -computername $Server -NameSpace "root\virtualization" -Query $WQL
}
#Example 1: Get-VM
#           Returns WMI MsVM_ComputerSystem objects for all Virtual Machines on the local server(n.b. Parent Partition is filtered out)
#Example 2: Get-VM "Windows 2008 Ent Full TS"   
#	    Returns a single WMI MsVM_ComputerSystem object for the VM named "Server 2008 ENT Full TS"
#Example 3: Get-VM "%2008%"  -Server James-2088
#       or: Get-VM "*2008*" 
#	    Returns WMI MsVM_ComputerSystem objects for VMs containing 2008 in their name on the server James-2008 (n.b. WQL Wild card is %, function converts * to %) 


Filter Test-VMHeartBeat
{param($vm, $timeOut=0, $Server=".") 
 $Endtime=(get-date).addSeconds($TimeOut)
 if ($VM -eq $null) {$VM=$_}
 if (($VM -eq $null) -and ($timeOut -eq $null)) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | foreach-Object {Test-VmHeartbeat -VM $_ -timeout $timeout -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) {
    $Status="No Heartbeat component found" 
    Do {
         $hb=(Get-WmiObject -Namespace "root\virtualization" -query "associators of {$vm} where resultclass=Msvm_HeartbeatComponent")
         if ($hb -is [System.Management.ManagementObject]) {$status=@{2="OK";6="Error";12="No Contact";13="Lost Communication"}.[int]($hb.OperationalStatus[0])}
         $pending = ((get-date) -lt $endTime) -and ($status -ne "OK") 
         if ($pending) {write-progress -activity "waiting for heartbeat" -Status $vm.elementname -Current $status; start-sleep 5} 
    } while ($Pending)         
    $vm | select-object elementName, @{Name="Status"; expression={$status}}  }
 $vm=$null
}
#Example start-vm "London DC" ; Test-vmheartBeat "London DC" -Timeout 300; start-vm "London SQL"
#        Starts the VM named London DC and waits up to 5 minutes for its heartbeat, then starts VM "London SQL"



Function Convert-VMState
{Param ($ID) 
 ($vmState.GetEnumerator() | where-object {$_.value -eq $ID}).name 
}
#Example Convert-VMState 2 
#        Returns "Running"


Filter Add-VMKVP
{Param($vm,$key,$Value, $Server=".") 
 if ($VM -eq $null)    {$VM = $_}
 if ($vm -eq $null)    {$vm = "%"}
 if ($VM -is [String]) {$VM = (Get-VM -Name $VM -server $Server)}
 if ($VM -is [Array])  {$VM | ForEach-Object {Add-KVP -VM $_ -Server $Server -key $Key -Value $value } }
 if ($VM -is [System.Management.ManagementObject]) {
    $KvpItem = ([wmiclass]"\\$Server\root\virtualization:Msvm_KvpExchangeDataItem").createinstance()
    $null=$KvpItem.psobject.properties #Without this the command will fail on Powershell V1 
    $KvpItem.Name = $key 
    $KvpItem.Data = $Value 
    $KvpItem.Source = 0 
    $arguments=@($VM, @($KvpItem.psbase.GetText([System.Management.TextFormat]::WmiDtd20)) , $null)
    $VSMgtSvc = (Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService") 
    $result=$VSMgtSvc.psbase.invokeMethod("AddKvpItems", $arguments)   
    if     ($result -eq 4096){test-wmijob $Arguments[2] -wait -Description "Waiting for KVP Exchange Service to finish"} 
    elseif ($result -eq 0)   {"Set KVP $key=$Value on '$($vm.elementName)"} else {"Failed to set KVP $key=$Value on '$($vm.elementName)', result code: $result."}}
 $vm=$null
}

Function Remove-VMKVP
{Param($vm,$key, $Server = "." )
 if ($VM -eq $null)    {$VM = $_}
 if ($vm -eq $null)    {$vm = "%"}
 if ($VM -is [String]) {$VM = (Get-VM -Name $VM -server $Server)}
 if ($VM -is [Array])  {$VM | ForEach-Object {Remove-VMKVP -VM $_ -Server $Server -key $Key -Value $value } }
 if ($VM -is [System.Management.ManagementObject]) {
    $KvpItem = ([wmiclass]"\\$Server\root\virtualization:Msvm_KvpExchangeDataItem").createinstance()
    $null=$KvpItem.psobject.properties #Without this the command will fail on Powershell V1 
    $KvpItem.Name = $key 
    $KvpItem.Data = ""
    $KvpItem.Source = 0
    $arguments=@($VM, @($KvpItem.psbase.GetText([System.Management.TextFormat]::WmiDtd20)) , $null)
    $VSMgtSvc = (Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService") 
    $result=$VSMgtSvc.psbase.invokeMethod("RemoveKvpItems", $arguments)   
    if     ($result -eq 4096){test-wmijob $Arguments[2] -wait -Description "Waiting for KVP Exchange Service to finish"} 
    elseif ($result -eq 0)   {"Remove KVP $key from '$($vm.elementName)"} else {"Failed to Remove KVP $key From '$($vm.elementName)', result code: $result."}}
 $Vm = $null
}


Filter get-vmkvp
{param($vm, $Server=".") 
 if ($VM -eq $null) {$VM=$_}
 if ($vm -eq $null) {$vm = "%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | foreach-Object {get-VMKVP -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) { 
     $KVPComponent=(Get-WmiObject -computername $VM.__Server -Namespace root\virtualization -query "select * from Msvm_KvpExchangeComponent where systemName = '$($vm.name)'")
     if ($KVPComponent.GuestIntrinsicExchangeItems  ) {
         ($KVPComponent.GuestIntrinsicExchangeItems + $KVPComponent.GuestExchangeItems )| 
         forEach -begin {$KVPObj = New-Object -TypeName System.Object 
                        Add-Member -inputObject $KvpObj -MemberType NoteProperty -Name "VMElementName" -Value $vm.elementName} `
                 -process {([xml]$_).SelectNodes("/INSTANCE/PROPERTY") | forEach -process {if ($_.name -eq "Name") {$propName=$_.value}; if  ($_.name -eq "Data") {$Propdata=$_.value} } -end {Add-Member -inputObject $KvpObj -MemberType NoteProperty -Name $PropName -Value $PropData}}  `
                 -end {[string[]]$Descriptions=@()
                       if ($KvpObj.ProcessorArchitecture -eq 0)  {$descriptions += "x86" }
                       if ($KvpObj.ProcessorArchitecture -eq 9)  {$descriptions += "x64" }
                       if ($KvpObj.ProductType -eq 1 )  {$descriptions += "Workstation" }
                       if ($KvpObj.ProductType -eq 2 )  {$descriptions += "Domain Controller" }
                       if ($KvpObj.ProductType -eq 3 )  {$descriptions += "Server" } 
                       $suites=@{1="Small Business";2="Enterprise";4="BackOffice";8="Communications";16="Terminal";32="Small Business Restricted";64="Embedded NT";128="Data Center";256="Single User";512="Personal";1024="Blade"}
                       foreach  ($Key in $suites.keys) {if ($KvpObj.suiteMask -band $key)  {$descriptions += $suites.$key} }
                       Add-Member -inputObject $KvpObj -MemberType NoteProperty -Name "Descriptions" -Value $descriptions
 
                       $KvpObj} 
      }}
 $vm=$null
}
#Example 1: (Get-VMKVP  "Windows 2008 Ent Full TS").OSName 
#            Returns "Windows Server (R) 2008 Enterprise" - the OS that server is running
#Example 2: Get-vmkvp % -server james-2008
#            Returns the Key Value pairs sent back by all the VMs on the Server James-2009
#Example 3: Get-Vm -running | get-VMKVP
#            Returns the Key Value pairs for running VMs on the local Server 
# Note. The values sent Automatically to the the child VM can be found in HKLM:\SOFTWARE\Microsoft\Virtual Machine\guest\Parameters 
#       The values sent Programaticaly to the the child VM can be found in HKLM:\SOFTWARE\Microsoft\Virtual Machine\External    
#       Those sent by the Child VM are in HKLM:\SOFTWARE\Microsoft\Virtual Machine\auto
#       If the VM isn't running its Key/Value Pair Exchange Service does NOT persist the values. So stopped VMs won't return anything !


Filter Ping-VM
{Param ($vm, $Server=".")
 $PingStatusCode=@{0="Success" ; 11001="Buffer Too Small" ; 11002="Destination Net Unreachable" ; 11003="Destination Host Unreachable" ; 11004="Destination Protocol Unreachable"
                   11005="Destination Port Unreachable";11006="No Resources";11007="Bad Option";11008="Hardware Error";11009="Packet Too Big"; 11010="Request Timed Out";
                   11011="Bad Request"; 11012="Bad Route"; 11013="TimeToLive Expired Transit"; 11014="TimeToLive Expired Reassembly"; 11015="Parameter Problem";
                   11016="Source Quench"; 11017="Option Too Big"; 11018="Bad Destination"; 11032="Negotiating IPSEC"; 11050="General Failure" }
 if ($VM -eq $null)    {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array])  {$VM | foreach-Object {Ping-VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) { 
     if ($VM.EnabledState -ne $vmstate["running"]) {
            $vm | Select-object -property @{Name="VMName"; expression={$_.ElementName}}, 
                                          @{Name="FullyQualifiedDomainName"; expression={$null}} , 
                                          @{name="NetworkAddress"; expression={$null}} ,
                                          @{Name="Status"; expression={"VM $(Convert-VmState -ID $_.EnabledState)"}} }            
     else {
            $vmFQDN=(Get-VMKVP $VM).fullyQualifiedDomainName
            if ($vmFQDN -eq $null) {
            $vm | Select-object -property @{Name="VMName"; expression={$vm.ElementName}},
                                          @{Name="FullyQualifiedDomainName"; expression={$null}} , 
                                          @{name="NetworkAddress"; expression={$null}} ,
                                          @{Name="Status"; expression={"Could not discover VM's FQDN"}} }           
            else {
                   Get-WmiObject -query "Select * from  Win32_PingStatus where Address='$VmFQDN' and ResolveAddressNames = True and recordRoute=1" |
                   Select-object -property @{Name="VMName"; expression={$vm.ElementName}},
                                           @{Name="FullyQualifiedDomainName"; expression={$vmFqdn}} , 
                                           @{name="NetworkAddress"; expression={$_.ProtocolAddressResolved}} , ResponseTime , ResponseTimeToLive , StatusCode , 
                                           @{Name="Status"; expression={if ($_.PrimaryAddressResolutionStatus -eq 0) {$PingStatusCode[[int]$_.statusCode]} else {"Address not resolved"}}}
                 }
           }
    }
 $vm=$null
}
#Example 1: Ping-VM "Tenby" -server james2008
#           Attempts to ping from the local machine to the VM named "Tenby" on the server James-2008. T
#           This relies on the integration components being present and the FQDN they return being resolvable on the local machine. 
#Example 2: get-vm -r | foreach-object {if ((Ping-VM $_).statusCode -ne 0) {"$($_.elementname) is inaccessible"} }
#           Gets-the running VMs, and pings them and outputs a message for any which are running but can't be pinged. 
 

Filter Get-VMSummary
{param ($vm , $Server=".")
 if ($vm -eq $null) {$VM=$_}
 if ($vm -eq $null) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | foreach-Object {Get-VMSummary -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) { 
           $vssd=Get-VMSettingData $vm 
           $settingPath=@($Vssd.__Path)
           $arguments=@($SettingPath, @(0,1,2,3,4,100,101,102,103,104,105,106,107,108), $null)
           $VSMgtSvc=Get-WmiObject -computerName $Vssd.__Server -NameSpace "root\virtualization" -Class "MsVM_virtualSystemManagementService" 
           $result=$VSMgtSvc.psbase.InvokeMethod("GetSummaryInformation",$arguments)
           if ($Result -eq 0) {$arguments[2] | foreach-object {
                 $SiObj = New-Object -TypeName System.Object
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "Host"             -Value $_.__server
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "VMElementName"    -Value $_.elementname
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "Name"             -Value $_.name
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "CreationTime"     -Value $_.CreationTime
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "EnabledState"     -Value $_.EnabledState
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "EnabledStateText" -Value (convert-vmState($_.EnabledState))
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "Notes"            -Value $_.Notes
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "CPUCount"         -Value $_.NumberOfProcessors
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "CPULoad"          -Value $_.ProcessorLoad
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "CPULoadHistory"   -Value $_.ProcessorLoadHistory
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "MemoryUsage"      -Value $_.MemoryUsage
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "Heartbeat"        -Value $_.Heartbeat
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "HeartbeatText"    -Value @{2="OK"; 6="Error"; 12="No Contact";13="Lost Communication"}[[int]$_.Heartbeat]
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "Uptime"           -Value $_.UpTime
                 Add-Member -inputObject $siObj -MemberType ScriptProperty -Name "UptimeFormatted"  -Value {if ($This.uptime -gt 0) {([datetime]0).addmilliseconds($This.UpTime).tostring("hh:mm:ss")} else {0} }
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "GuestOS"          -Value $_.GuestOperatingSystem;
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "Snapshots"        -Value $_.Snapshots.count
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "Jobs"             -Value $_.AsynchronousTasks
                 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "FQDN"             -value ((get-vmkvp -vm $vm).FullyQualifiedDomainName)
        	 Add-Member -inputObject $siObj -MemberType NoteProperty -Name   "IpAddress"        -Value ((Ping-VM $vm).NetworkAddress)
                 $siObj } }
         else  {"Get Summary info :" + $ReturnCode[[string]$result] | Out-host }
         }
}
#Example 1: Get-VMSummary -server james-2008,jackie-2008 | ft -a 
#           Outputs formatted status for all VMs on the servers named "James-2008" and "Jackie-2008"
#Example 2: Get-VMSmmary "Windows 2008 Ent Full TS"  
#   Outputs status for all for the VM named "Server 2008 ENT Full TS" on the local server

set-alias -Name list-vmState -value get-vmSummary
set-alias -Name Get-vmState -value get-vmSummary
#This function has been called by other names


Function Choose-VM
{param ($Server=".", [Switch]$multiple)
 if ($multiple) {choose-list -InputObject (Get-VM -Server $Server) -Property @(@{Label="VM Name"; Expression={$_.ElementName}}, @{Label="State"; Expression={Convert-VmState -ID $_.EnabledState}}) -multiple }
           else {choose-list -InputObject (Get-VM -Server $Server) -Property @(@{Label="VM Name"; Expression={$_.ElementName}}, @{Label="State"; Expression={Convert-VmState -ID $_.EnabledState}})  }
}
#Example 1: Choose-vm -multiple
#            Lets the user select one or more VMs from a list of those on the local machine
#Example 2: Choose-vm -Server James-2008,Jackie-2008
#            Lets the user select a single VM from a list of those on the servers named "James-2008" and "Jackie-2008"


Filter Set-VMState 
{Param ($VM , $state, $Server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Set-VMState -VM $_ -State $state -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) {$result = $VM.RequestStateChange($State)
                                                    "Changing state of {0}: {1}." -f $vm.elementName , $ReturnCode[[string]$result.ReturnValue] | Out-host 
						    $Result.Job } 
 $VM=$null
} 
#Example 1. get-vm Core-% |  Set-VMState -state $vmstate.running
#           Starts all VMs whose names start with core-
#Example 2. set-vmstate "core","Tenby" $vmstate.running -Server "James-2008"
#           Starts the VMs named Core and Tenby on the server named James-2008 (note quotes round the parameter are optional)
 

Filter Start-VM
{Param ($VM , $Server=".", [Switch]$wait , $heartbeat) 
 if ($VM -eq $null) { $JobID=(Set-VMState -VM $_  -State $vmState.running) }
 else               { $jobID=(Set-VMState -VM $VM -State $vmState.running -Server $Server) }
 if ($wait) {test-wmijob $jobID -wait -Description "Starting VM"
	     if ($heartbeat) {Test-vmheartBeat -vm $vm -timeOut $heartbeat -Server $Server }	} 
 else       {$jobid}
}
#Example 1: Start-VM (choose-VM -server James-2008 -multiple) 
#           prompts the user to select one or more of the VMs on the server James-2008,  and starts them 
#Example 2: get-vm | where-object {$_.EnabledState -eq $vmState.Suspended} | start-vm
#           Gets VMs in the suspended (saved) state on the local server and Starts them 



Filter Suspend-VM
{Param ($VM,$Server="."  , [Switch]$wait)
 if ($VM -eq $null) {$jobID=(Set-VMState -VM $_  -State $vmState.Suspended)}
 else               {$jobID=(Set-VMState -VM $VM -State $vmState.Suspended  -Server $Server )}
 if ($wait) {test-wmijob $jobID -wait -Description "Suspending VM"}
 else       {$jobid}
}
#Example get-vm -running | suspend-vm -wait ; shutdown -s -t 0
#Gets running vms and puts them into a saved state -waiting until they are saved, then shuts down the host 
# See Start-VM for similar examples.


Filter Stop-VM
{Param ($VM  , $Server=".", [Switch]$wait)
 if ($VM -eq $null) { $jobID=(Set-VMState -VM $_  -State $vmState.Stopped)}
 else               { $jobID=(Set-VMState -VM $VM -State $vmState.Stopped -Server $Server)}
 if ($wait) {test-wmijob $jobID -wait -Description "Stopping VM"}
 else       {$jobid}
}
# See Start-VM for examples.


Filter Shutdown-VM
{Param ($VM , $Reason="Scripted", $Server=".")
 if ($VM -eq $null)    {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array])  {$VM | ForEach-Object {Shutdown-VM -VM $_ -Reason $Reason -Server $server} }
 if ($VM -is [System.Management.ManagementObject]) {
     $ShutDownComponent=get-wmiobject -computername $vm.__server -namespace "root\virtualization" -query  "SELECT * FROM Msvm_ShutdownComponent WHERE SystemName='$($vm.name)' "
     If ($ShutDownComponent -ne $null) {$result=$ShutDownComponent.InitiateShutdown($true,$reason) 
                                        If ($result.returnValue -eq 0) {"Shutdown of '$($vm.elementName) ' started."} else {"Attempt to shutdown '$($vm.elementName)' failed with code $($result.returnValue)."} }
     else  {"Could not get shutdown component for '$($vm.elementName)'."}  }
 $vm=$null
}
#Example 1:  shutdown-vm "Tenby" -server James-2008
#            Invokes an OS shutdown on the VM named Tenby
#Example 2:  get-vm | where-object {$_.EnabledState -eq $vmState.running} | shutdown-vm -Reason "Server Upgrade"
#            Gets VMs in the Running state on the local machine and invokes an OS shutdown on them and logs a reason for shutting them down
#Note this depends on the installation components being installed in the VM


Filter New-VMConnectSession
{Param ($VM,$server=$Env:computerName) 
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {& "$Env:ProgramFiles\Hyper-V\VMconnect.exe" $server $VM }
 if ($VM -is [Array]) {$VM | ForEach-Object {New-VMConnectSession -VM $_ -Server $server } }
 if ($VM -is [System.Management.ManagementObject]) {& "$Env:ProgramFiles\Hyper-V\VMconnect.exe" $Vm.__Server "-G" $VM.Name }
 $vm=$null
}
#Example 1: New-VMConnectSession $tenby
     # #           Launches a Terminal connection to the VM pointed to by $tenby on the local server.
#Example 2: New-VMConnectSession "tenby" -server James-2008
#           Launches a Terminal connection to the VM Named "tenby" on the server named "James-2008"


#############################################################################################
#                                                                                           #
# Functions for working with VMs and their motherboard settings (inc CPU and Memory)        #
#                                                                                           #
#############################################################################################
#Will make a lot of use of ModifyVirtualSystem(
#  [in]   CIM_ComputerSystem Ref ComputerSystem, // Reference to the virtual computer system #to be modified.
#  [in]   string SystemSettingData, // Embedded instance of the CIM_VirtualSystemSettingData class that describes the modified setting values for the virtual computer system.
#                                   // MSCM_VirtualSystemSettingData
#                                   //  BIOSNumLock            : False
#				    //  BootOrder              : {1, 2, 3, 0}
#				    // MsVM_VirtualSystemGlobalSettingData
#				    //  AutomaticRecoveryAction  
#				    //  AutomaticShutdownAction 
#				    //  AutomaticStartupAction 
#                                   //  SnapshotDataRoot 
#  [out]  CIM_VirtualSystemSettingData Ref ModifiedSettingData, // Reference to the CIM_VirtualSystemSettingData instance that represents the object that was modified.
#  [out]  CIM_ConcreteJob Ref Job);
#If this method is executed synchronously, it returns 0 if it succeeds. 
#If this method is executed asynchronously, it returns 4096 and the Job output parameter can be used to track the progress of the asynchronous operation. 
#Any other return value indicates an error.

Filter Get-VMSettingData
{Param ($VM, $Server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMSettingData -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) {
  if ($vm.__Class -eq "Msvm_VirtualSystemSettingData") {$VM}
  if ($vm.__Class -eq "Msvm_ComputerSystem") {Get-WmiObject -ComputerName $vm.__Server -NameSpace "root\virtualization" -Query "ASSOCIATORS OF {$VM} Where ResultClass = MsVM_VirtualSystemSettingData"  | where-object {$_.instanceID -eq "Microsoft:$($vm.name)"}} }
 $vm=$Null
}


Function New-VM
{Param([String]$Name=$(Throw("You must give a VM Name")) , $path, $Server="." )
  # We're going to call Define virtual System. You can use  $Result=$VSMgtSvc.defineVirtualSystem() which doesn't seem to be on MSDN ... 

  $vmGsd=([wmiclass]"\\$Server\root\virtualization:Msvm_VirtualSystemGlobalSettingData").createInstance() 
  $null=$VmGsd.psobject.properties #Without this the command will fail on Powershell V1 
  $vmGsd.ElementName=$name                                                                                            
  if ($path -is [string]) {$vmGsd.ExternalDataRoot=$Path}                                                                                         
  $arguments = @($vmGsd.psbase.GetText([System.Management.TextFormat]::WmiDtd20),$null,$null,$null,$null)                    
  $VSMgtSvc=Get-WmiObject -computerName $Server -NameSpace "root\virtualization" -Class "MsVM_virtualSystemManagementService"      
  $result=$VSMgtSvc.psbase.InvokeMethod("defineVirtualSystem",$arguments)                                                              
  if ($result -eq 0)  {Write-Host "Created VM '$name'"
                 [wmi]$arguments[3] }                         
  else {Write-host "VM Creation failed: error code was "
        $result}
}
#Example 1: $tenby = new-VM "Tenby"
#	    Creates an New-VM on the local machine using default settings, and setting the name to "Tenby", storing its  MsVM_ComputerSystem object in the variable $tenby
#Example 2: $tenby = new-VM "Tenby" -server "James-2008"
#	    Ditto but on a server named "James-2008"


Filter Set-VM
{Param($VM, [string]$Name, [int[]]$bootOrder, $notes, $AutoRecovery, $AutoShutDown, $autoStartup, $Server="." )
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [System.Management.ManagementObject]) {
     $VSMgtSvc=Get-WmiObject -computerName $server -NameSpace "root\virtualization" -Class "MsVM_virtualSystemManagementService"
 
     if (($Name -ne "") -or ($notes -ne $null) -or ($BootOrder -ne $null)) { 
         $VSSD=(Get-VMSettingData $VM)
	 if ($vm -eq $vssd) {$vm = gwmi -namespace root\virtualization -computername $vssd.__SERVER -Query "select * from MSVM_Computersystem where name ='$($vssd.systemName)'"}
         if ($Name      -ne "")        {$VSSD.ElementName=$Name}
         if ($notes     -is [String])  {$VSSD.notes=$Notes}
         if ($BootOrder -is [array])   {if (($bootOrder | group | where-object {$_.count -gt 1}) -ne $null) {"An item appeared twice in the boot order"}
                                        else {$VSSD.BootOrder=$BootOrder} }
         # Build the arguments array - the path VSSD object in XML form and a bunch of nulls
         $arguments = @($VM , $VSSD.psbase.GetText([System.Management.TextFormat]::WmiDtd20),$null,$null)
         # Invoke the define Virtual System method, passing the arguments.  
         if   ($VSMgtSvc.psbase.InvokeMethod("ModifyVirtualSystem", $arguments) -eq 0) { "Modified VM Settings object for $($vm.elementname)" } 
         else                                                                          {write-error "Could not Modify VM Settings Object for $($vm.elementname)"}}
     If (($AutoRecovery -ne $null) -or ($AutoShutDown -ne $null) -or ($AutoStartup -ne $null)) {
        $VSGSD=(get-wmiobject  -namespace root\virtualization  -Query "associators of {$vm} where resultclass=Msvm_VirtualSystemGlobalSettingData")
        if ($AutoRecovery -is [int]) {$VSGSD.AutomaticRecoveryAction         = $AutoRecovery }
        if ($AutoShutDown -is [int]) {$VSGSD.AutomaticShutdownAction         = $AutoShutdown }
        if ($AutoStartup  -is [int]) {$VSGSD.AutomaticStartupAction = $autoStartup  }
         $arguments = @($VM , $VSGSD.psbase.GetText([System.Management.TextFormat]::WmiDtd20),$null,$null)
         # Invoke the define Virtual System method, passing the arguments.  
         if   ($VSMgtSvc.psbase.InvokeMethod("ModifyVirtualSystem", $arguments) -eq 0) { "Modified VM Global Settings object  for $($vm.elementname)" } 
         else                                                                          {write-error "Could not Modify VM Global Settings Object for $($vm.elementname)"}}}
}
#Example 1:  Set-vm $vm -bootorder $bootmedia["CD"],$bootmedia["IDE"],$bootmedia["net"],$bootmedia["Floppy"]
#             Sets the boot order for the machine whose config is $vm to CD, IDE, Network, Floppy. 
#Example 2:  set-vm -vm core -bootOrder @(3,2,0,1) 
#             Sets the boot order for the VM named Core to Network, IDE, Floppy, CD
#Example 3:  set-vm -vm core -autoStart $StartupAction["AlwaysStartup"]
#             Sets the VM named Core to always Startup 
#Example 4:  Set-vm $vm -bootorder $bootmedia["CD"],$bootmedia["IDE"],$bootmedia["net"],$bootmedia["Floppy"] -autoStart 0 -AutoShutdown 2
#             Sets the boot order for the machine whose config is $vm AND sets it to never autostart and to close down the OS on shutdown 
#Example 5: Set-vm "CORE-%" -bootorder $bootmedia["CD"],$bootmedia["IDE"],$bootmedia["net"],$bootmedia["Floppy"] -autoStart $StartupAction["AlwaysStartup"]
#             Should take all the machines whose names begin CORE- and set their boot order and start-up action


Filter Remove-VM
{Param ($VM ,  $server=".", [Switch]$wait)
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Remove-VM -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) {
     $VSMgtSvc=Get-WmiObject -computerName $VM.__server -NameSpace "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     $arguments = @($vm.__path,$null)
     $vmName=$vm.elementname
     $result = $VSMgtSvc.psbase.InvokeMethod("DestroyVirtualSystem", $arguments)
     if ($result -eq 0) {"Deleted VM '$vmName'." | out- host} 
     else  {if ($result -eq 4096) {"Job Started to delete VM '$vmName'." | out-host
				   $arguments[1]}
            else                  {Write-error "Failed to delete VM '$vmname', return code: $result." } }
     $vm=$null}
}
#Example 1: Remove-VM "Tenby" -server James 2008
#	    Removes the VM named Tenby from the server named "James-2008"
#Example 2: Get-vm | remove-vm 
#	    Removes all the VMs on the local machine. No prompts. No warnings and -WhatIf is ignored. 


Filter Set-VMMemory
{Param ($VM , [int64]$memory, $server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Set-VMMemory -VM $_ -Memory $memory -Server $Server} }
   # We're going to call Modify Virtual System Resources, and we need set up the parameter array - following info is copied from MSDN
   #   uint32 ModifyVirtualSystemResources(
   #     [in]   CIM_ComputerSystem Ref ComputerSystem,  // Reference to the virtual computer system whose resources are to be modified.
   #     [in]   string ResourceSettingData[],           // Array of embedded instances of the CIM_ResourceAllocationSettingData class 
   #                                                    //  that describe the resources to be modified 
   #     [out]  CIM_ConcreteJob Ref Job,                //  Optional reference that is returned if the operation is executed asynchronously. );
 if ($VM -is [System.Management.ManagementObject]) {
     # Step 1 Get a MsVM_MemorySettingData object - a subclass of Resource-Allocation-Setting-Data
     if ($memory -gt 1mb) {$memory = $memory /1mb}
     $memSettingData=Get-vmMemory $vm
     $memsettingData.Limit           =$Memory 
     $memsettingData.Reservation     =$Memory 
     $memsettingData.VirtualQuantity =$Memory  
     # Step 2 build the arguments array - the WMI path for the server, the Memory Setting Data object in XML form and a null for the job ID
     $arguments=@($VM.__Path, @($memsettingData.psbase.GetText([System.Management.TextFormat]::WmiDtd20)) , $null)
     $VSMgtSvc = (Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService") 
     #          invoke its define Modify Virtual System Resources method, passing the arguments.
     # If this method is executed synchronously, it returns 0 if it succeeds. 
     # If this method is executed asynchronously, it returns 4096 and the Job output parameter can be used to track progress  
     # Any other return value indicates an error
     # So return "Success" or "Failure" 
     $result=$VSMgtSvc.psbase.invokeMethod("ModifyVirtualSystemResources",$arguments)  
     if ($result -eq 0) {"Set memory for '$($vm.elementName)' to $memory MB."} else {"Failed to set memory for '$($vm.elementName)', result code: $result."} }
 $vm=$null
}
#Example 1: set-VMmemory "Tenby" 1.5gb -server James-2008
#           sets the VM named "Tenby" on server James-2008 to have 1.5GB of RAM
#Example 2: Get-vm Core-% | set-VMmemory -memory 1073741824
#           Gives 1GB of RAM to all the VMs on the local server whose names begin CORE-


Filter Get-VMMemory
{Param ($VM , $server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -eq $null) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMMemory -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) {$vssd = Get-vmSettingData $vm
             Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Query "associators of {$vssd} where resultclass=Msvm_MemorySettingData" }
 $vm=$null
}
#Example Get-VMMemory core
#        Returns the memory settings for the VM named core on the local server. 

Filter Set-VMCPUCount
{Param ($VM , [int]$CPUCount, $server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -server $server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Set-VMCPUCount -VM $_ -CPUCount $CPUCount} }
 if ($VM -is [System.Management.ManagementObject]) {
     # See comments in Set-VMMemory for how this works. This time we're going to get a MsVM_ProcessorSettingData object - a subclass of Resource Allocation Setting Data
     $procsSettingData=Get-VMCpuCount $VM 
     $procsSettingData.VirtualQuantity=$CPUCount
     $arguments=@($VM.__Path, @($procsSettingData.psbase.GetText([System.Management.TextFormat]::WmiDtd20)) , $null)
     $VSMgtSvc = (Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService") 
     $result=$VSMgtSvc.psbase.invokeMethod("ModifyVirtualSystemResources", $arguments)   
     if ($result -eq 0) {"Set CPU Count for '$($vm.elementName)' to $CPUCount."} else {"Failed to set CPUCount for '$($vm.elementName)', result code: $result."}}
 $vm=$null
}
#Example 1: Set-VMCPUCount "tenby" 2 -Server "James-2008"
#           Assigns 2 CPUs to the VM named Tenby on Server James-2008 
#Example 2: Get-vm Core-% | Set-VMCPUCount - CPUCount 2
#           Gives 2 CPUs to all VMs on the local machine whose names begin CORE-


Filter Get-VMCPUCount
{Param ($VM , $server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -eq $null) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMCpuCount -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) {$vssd = Get-vmSettingData $vm
     Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -query "associators of {$vssd} where resultclass=MsVM_ProcessorSettingData"}
 $vm=$null
}
#Example Get-VMCPUCount core
#        Returns the CPU settings for the VM named core on the local server. 


Filter Get-VMProcessor {
Param ($vm, $server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($vm -eq $null) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMProcessor -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject]) {
        Get-WmiObject -namespace root\virtualization -computerName $server -query "associators of {$vm} where ResultClass= MSVM_Processor" 
    }
 }

#Example Get-VMProccessor core
#        Returns the CPU settings for the VM named core on the local server. 

Function New-VMRasd
{Param ($ResType, $ResSubType , $server=".")
 #Get a Resource Allocation Setting Data object  
 $allocCapsPath= ((Get-WmiObject -ComputerName $server -NameSpace "root\virtualization" -Query "Select * From MsVM_AllocationCapabilities Where ResourceType = $ResType AND ResourceSubType = '$ResSubType'").__Path).replace('\','\\')
 New-Object System.Management.Managementobject((Get-WmiObject -ComputerName $server -NameSpace "root\virtualization" -Query "Select * From MsVM_SettingsDefineCapabilities Where  valuerange=0 and Groupcomponent = '$AllocCapsPath'").partcomponent)
}

############################################################################
#                                                                          #
# Functions for working with disk objects , SCSI Controller, Driver, Disk  #
#                                                                          #
############################################################################

Filter Get-VMDiskController
{Param ($VM , $ControllerID, $server=".", [Switch]$SCSI, [Switch]$IDE )
 if ($VM -eq $null) {$VM=$_}
 if ($VM -eq $null) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -server $Server) }
 if ($VM -is [Array]) {if ($SCSI) {$VM | ForEach-Object {Get-VMDiskController -VM $_ -ControllerID  $ControllerID -SCSI} }
                       if ($IDE)  {$VM | ForEach-Object {Get-VMDiskController -VM $_ -ControllerID  $ControllerID -IDE } } 
                       if ((-not $scsi) -and (-not $IDE) -and ($contollerID -eq $null)) {$VM | ForEach-Object {Get-VMDiskController -VM $_ } } }
 if ($VM -is [System.Management.ManagementObject]) {
     if ((-not $scsi) -and (-not $IDE) -and ($contollerID -eq $null)) {
         if ($vm.__class -eq "Msvm_ComputerSystem") {$vm=(get-vmsettingData $vm)} 
   #notice this uses the Associators of , and select where instanceID syntaxes
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
#Example 1: Get-VM -server James-2008| Get-VMDiskController -IDE -SCSI
#           Returns all the DiskControllers for all the VMs on Server James-2008      
#Example 2: Get-VMDiskController $Tenby -SCSI -ContollerID 0
#           Returns SCSI controller 0 in the VM pointed to by $Tenby


Filter Get-VMDriveByController
{Param ($Controller, $LUN="%" )
 if ($Controller -eq $null) {$Controller=$_}
 if ($Controller -is [Array]) {$Controller | ForEach-Object {Get-VMDriveByController -Controller  $Controller -LUN $Lun} }
 if ($Controller -is [System.Management.ManagementObject]) {
    $CtrlPath=$Controller.__Path.replace("\","\\")
    Get-WmiObject -computerName $controller.__server -Query "Select * From MsVM_ResourceAllocationSettingData Where PARENT='$ctrlPath' and Address Like '$Lun' " -NameSpace "root\virtualization" }
 $Controller = $null
}
#Example 1: Get-VMDiskController "Tenby" -server "James-2008" -IDE -ContollerID 0 | Get-VMDriveByController
#           Gets the drives attached to IDE controller 0 in the VM named Tenby on Server James-2008
#Example 2: Get-VMDriveByController $controller 0
#           Gets the first drive attached tothe controller pointed to by $controller. 


Filter Get-VMDiskByDrive
{Param ($Drive)
 if ($Drive -eq $null) {$Drive=$_}
 if ($Drive -is [Array]) {$Controller | ForEach-Object {get-vmdiskByDrive -Drive $Drive} }
 if ($Drive -is [System.Management.ManagementObject]) {
    $DrivePath=$Drive.__Path.replace("\","\\")
    Get-WmiObject -computerName $drive.__server -Query "Select * From MsVM_ResourceAllocationSettingData Where PARENT='$DrivePath' " -NameSpace "root\virtualization" }
 $Drive = $null
}
#Example 1: Get-VMDiskController "Tenby" -server "James-2008" -IDE -ContollerID 0 | Get-VMDriveByController | get-vmdiskByDrive
#           Gets the disks in the drives attached to IDE controller 0 in the VM named Tenby on Server James-2008
#Example 2: get-vmdiskByDrive $drive
#           Gets the disk in the drive pointed to by $drive


Filter Get-VMDisk
{Param ($vm, [switch]$snapshot)
 if ($vm -eq $null) {$vm = $_}
 if ($vm -eq $null) {$vm = "%"} 
 if ($vm -is [String]) {$vm = get-vm -Name $vm}
 if ($vm -isnot [array]) {$vm=@($vm)}
 if ($snapshot) {$VM= ($VM + (get-vmsnapshot $vm)  | sort elementname) }
 foreach ($v in $vm) {
         if ($v -is [String]) {$v = get-vm -Name $v}
         foreach ($dc in (get-vmdiskcontroller -vm $v)) {
                 foreach ($drive in (Get-VMDriveByController -controller $dc)) {
                         get-vmdiskByDrive -drive $drive | select-object -property `
                                                       @{name="VMElementName"; expression={$v.elementName}},
                                                       @{name="VMGUID"; expression={$v.Name}},
                                                       @{name="ControllerName"; expression={$dc.elementName}},
                                                       @{name="ControllerInstanceID"; expression={$dc.InstanceId}},
                                                       @{name="ControllerID"; expression={$dc.instanceID.split("\")[-1]}},
                                                       @{name="DriveName"; expression={$drive.caption}} ,
                                                       @{name="DriveInstanceID"; expression={$drive.instanceID}},
                                                       @{name="DriveLUN"; expression={$drive.address}},
                                                       @{name="DiskPath"; expression={$_.Connection}},
                                                       @{name="DiskName"; expression={$_.ElementName}},
                                                       @{name="DiskInstanceID"; expression={$_.InstanceID}} }}}
 $vm=$null
}
#Example 1: Get-VMDisk (choose-vm -server "James-2008" -multi) | format-table -autosize -property VMname, DriveName,  @{Label="Conected to"; expression={"{0,5} {1}:{2}" -f $_.Controllername.split(" ")[0], $_.ControllerID, $_.DriveLun }} , DiskPath
#           Displays the disks connected to the chosen VMs, giving the VM Name, Hard drive/DVD Drive , Controller:LUN and VHD/ISO file
#Example 2: Get-VMDisk * | foreach {$_.diskpath}
#           Returns a list of all the disk paths used on all the VMs on the Local server 
#Example 3: Get-VMDisk * | where {$_.diskpath -match "^IDE"} 
#           Finds which VMs are connected to the Physical CD/DVD drive.  


Filter Add-VMSCSIController
{Param ($VM, $server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Add-VMSCSIController -VM $_ -server $server } }
 if ($VM -is [System.Management.ManagementObject]) {
   # We're going to call ADD Virtual System Resources, and we need set up the parameter array - following info is copied from MSDN
   # uint32 AddVirtualSystemResources(
   #  [in]   CIM_ComputerSystem REF TargetSystem,    // Reference to the virtual computer system whose resources are to be modified. 
   #  [in]   string ResourceSettingData[],           // Array of embedded instances of the CIM_ResourceAllocationSettingData class 
   #                                                 // that describe the resources 
   #  [out]  CIM_ResourceAllocationSettingData REF NewResources[],
   #  [out]  CIM_ConcreteJob REF Job);
   # so build the arguments array - the WMI path for the server, the Resource allocation Setting Data object in XML form and a null for the job ID
     $SCSIRASD=NEW-VMRasd -ResType 6 -ResSubtype 'Microsoft Synthetic SCSI Controller' -Server $vm.__Server
     $SCSIRASD.elementName="VMBus SCSI Controller"
     $arguments = @($VM.__Path, @( $SCSIRASD.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null, $null )
     $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
   #  invoke its ADD Virtual System Resources method, passing the arguments. A return code of 0 indicates success
     $result=$VSMgtSvc.psbase.InvokeMethod("AddVirtualSystemResources", $arguments)   
     if ($result -eq 0) {"Added VMBus SCSI Controller to '$($vm.elementName)'."} else {"Failed to add SCSI Controller to '$($vm.elementName)', result code: $result."} }	
 $vm=$null
}
#Example 1: Add-VMSCSIController $tenby 
#           Adds a VMBus SCSI Controller to VM whose info is in $tenby
#Example 2: Get-vm Core-% -server james-2008 | Add-VMSCSIController 
#           Adds a SCSI Controller to all VMs whose names begin CORE- on the server named "James-2008"


Function Remove-VMSCSIcontroller
{Param( $VM, $controllerID, $server="." )
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 $controller= (Get-VMDiskController -vm $vm -ControllerID $ControllerID -SCSI)
   # We're going to call Remove Virtual System Resources, and we need set up t he parameter array - following info is copied from MSDN
   # uint32 RemoveVirtualSystemResources(
   #  [in]   CIM_ComputerSystem REF TargetSystem,
   #  [in]   CIM_ResourceAllocationSettingData REF ResourceSettingData[],
   #  [out]  CIM_ConcreteJob REF Job);
 # to Avoid leaving orphaned objects behind we're going to 
 #   (1) Get the drives bound to the controller, (2) remove the disks inserted into them, (3) Then Remove the drives
 # and finally (4) Remove  the controller
 # So we have to set up the arguments array and call the Remove method more than once. 
 if ($Controller -is [System.Management.ManagementObject]) {
        $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
	foreach ($drive in (Get-VMDriveByController $controller)) {  
	    $disk = ($drive | get-vmdiskByDrive) 
            if ($disk -is [System.Management.ManagementObject]) {
                $arguments = @($VM.__Path, @( $disk.__Path ), $null ) 
                $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
                if ($result -eq 0) {"Removed disk from VM '$($vm.elementname)'." }  else {"Failed to remove disk from VM '$($vm.elementname)', result code: $Result." }}	
            $arguments = @($VM.__Path, @( $drive.__Path ), $null ) 
            $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
            if ($result -eq 0) {"Removed drive from VM '$($vm.elementname)'." }  else {"Failed to remove disk from VM'$($vm.elementname)', result code: $Result." }}	
        $arguments = @($VM.__Path, @( $controller.__Path ), $null ) 
        $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
        if ($result -eq 0) {"Removed Controller from VM '$($vm.elementname)'." }  else {"Failed to remove controller from VM '$($vm.elementname)', result code: $Result." }}	
 $Controller = $null
}
#Example 1: Remove-VMSCSIController $tenby 0
#           Remove the first VMBus SCSI Controller to VM whose info is in $tenby


Filter Add-VMDRIVE
{Param ($VM , $ControllerID=0 , $LUN, $Server="." ,   [switch]$DVD , [switch]$SCSI)
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $server) }
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
#Example 1: Add-VMDRIVE "tenby" 1 1 -server james-2008 
#           Adds a virtal DVD to IDE contoller 1, disk slot 1 on the VM named Tenby on Server James-2008
#Example 2: Add-VMDRIVE $tenby 0 3 -SCSI 
#           Adds a virtal hard disk drive to SCSI controller 0, LUN 3 on the VM whose info is in $tenby
#Example 3: Get-vm Core-% | Add-VMDRIVE -controllerID 0 -lun 1 -DVD
#           Adds a DVD drive to  IDE contoller 0, disk slot 1 on all the VMs on the local server whose name begins with CORE-


Function Add-VMDISK
{Param ($VM , $ControllerID=0 , $LUN=0, $VHDPath, $server=".", [switch]$DVD , [switch]$SCSI)
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 # Similar to Adding the drive, but we request a different resource type, and the parent is the 'Microsoft Synthetic Disk Drive', instead of disk controller
 # Mount an ISO in a DVD drive or A VHD in a Disk drive 
 if ($VM -is [System.Management.ManagementObject]) { 
     if ($DVD)  {$diskRASD=NEW-VMRasd -resType 21 -resSubType 'Microsoft Virtual CD/DVD Disk' -server $vm.__Server } 
     else       {$diskRASD=NEW-VMRasd -resType 21 -resSubType 'Microsoft Virtual Hard Disk'   -server $vm.__Server }
     if ($SCSI) {$diskRASD.parent=(Get-VMDriveByController -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -SCSI) -Lun $Lun ).__Path }
     else       {$diskRASD.parent=(Get-VMDriveByController -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -IDE)  -Lun $lun ).__Path }
     $diskRASD.Connection=$VHDPath
     $arguments = @($VM.__Path, @( $diskRASD.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null, $null )
     $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     $result=$VSMgtSvc.psbase.invokeMethod("AddVirtualSystemResources", $arguments)   
     if ($result -eq 0) {"Added disk to '$($vm.elementName)'."} else {"Failed to add disk to '$($vm.elementName)', result code: $result."} } 
}
#Example 1: Add-VMDisk $tenby 0 1 "C:\update.iso" -DVD
#           Adds a DVD image C:\update.iso, to disk 1, contoller 0 on the VM whose info is in $tenby
#Example 2: Add-VMDisk $tenby 0 0 ((get-VHDdefaultPath) +"\tenby.vhd") 
#           Adds a virtal hard disk named tenby.vhd in the Default folder , to disk 0, contoller 0 on the VM whose info is in $tenby


Function Add-VMNewHardDisk 
{Param ($VM , [int]$ControllerID=0, [int]$LUN=0, $VHDPath, $size=127GB, $parentDisk, $Server=".", [switch]$fixed, [switch]$SCSI)
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [System.Management.ManagementObject]) { 
       if ($Vhdpath -eq $null) {$vhdPath = $vm.elementName }	
       if ($parentDisk -ne $null){New-VHD -vhdPath $vhdPath -parentDisk $parentDisk -server $vm.__server }
       else       { if ($fixed)  {New-VHD -vhdPath $vhdPath -size $size -fixed -server $vm.__server }
                           else  {New-VHD -vhdPath $vhdPath -size $size -server $vm.__server }  }
 if ($SCSI) { Add-VMDRIVE -VM $VM -ControllerID  $ControllerID -LUN $LUN -scsi
              Add-VMDISK  -VM $VM -ControllerID  $ControllerID -LUN $LUN -VHDPath $VHDPath -scsi}
 else       { Add-VMDRIVE -VM $VM -ControllerID  $ControllerID -LUN $LUN
              Add-VMDISK  -VM $VM -ControllerID  $ControllerID -LUN $LUN -VHDPath $VHDPath}}
}
#Example:  Add-VMNewHardDisk -vm $vm -controllerID 0 -lun 3 -vhdpath "$(get-VHDdefaultPath)\foo31.vhd" -size 20gb -scsi 
#          Adds a 20GB dynamic disk, named foo31.vhd in the default folder,  to the VM defined in $VM, on SCSI controller 0, Lun 3. 


Function Set-VMDisk
{Param( $VM, [int]$controllerID, [int]$LUN, $path, $server="." , [switch]$scsi)
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -server $Server) }
 if ($VM -is [System.Management.ManagementObject]) { 
     if ($SCSI) {  $RASD=(Get-VMDriveByController -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -SCSI) -Lun $Lun ) | get-vmdiskByDrive }
     else       {  $RASD=(Get-VMDriveByController -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -IDE)  -Lun $lun ) | get-vmdiskByDrive }
     if ($RASD -is [System.Management.ManagementObject]) {
         $RASD.connection=$path
         $arguments = @($VM.__Path, @( $RASD.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null )                                                                                                                                                                                             
         $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
         $result=$VSMgtSvc.psbase.invokeMethod("ModifyVirtualSystemResources", $arguments) 
         if ($result -eq 0) {"Changed disk on VM '$($vm.elementname)'." }	else {"Failed to change disk on VM '$($vm.elementname)', result code: $Result." }}	
     else {"Could not find the specified disk on VM '$($vm.elementname)'"} }
}
#Example 1: Set-VMDisk Tenby 0 1 (Get-WmiObject -Query "Select * From win32_cdromdrive Where ID='D:' " ).deviceID
#           Sets the DVD on controller 0, device 1 for the VM named "Tenby" on the local Server to point to physical drive D: on the host. 
#Example 2: Set-VMDisk $Core 0 0 "\\?\Volume{d1f72a03-d43a-11dc-8bf1-806e6f6e6963}\Virtual Hard Disks\Core.vhd"
#           Sets the Disk on controller 0, device 0 of the VM pointed to by $core to Core.VHD using GUID (not drive letter) notation


Function Remove-VMdrive
{Param( $VM, [int]$controllerID, [int]$LUN, $server="." , [switch]$scsi, [switch]$Diskonly )
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -server $Server) }
 if ($SCSI) {$drive=(Get-VMDriveByController -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -SCSI) -Lun $Lun )}
 else       {$drive=(Get-VMDriveByController -controller (Get-VMDiskController -vm $vm -ControllerID $ControllerID -IDE)  -Lun $lun )}
 if ($drive -is [System.Management.ManagementObject]) {
     $disk=$drive | get-vmdiskByDrive
     $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     if ($disk -is [System.Management.ManagementObject]) {
         $arguments = @($VM.__Path, @( $disk.__Path ), $null ) 
         $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
         if ($result -eq 0) {"Removed disk from VM '$($vm.elementname)'." }  else {"Failed to remove disk from VM '$($vm.elementname)', result code: $Result." }}	
     if (-not $diskOnly) {  
        $arguments = @($VM.__Path, @( $drive.__Path ), $null ) 
        $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
        if ($result -eq 0) {"Removed drive from VM '$($vm.elementname)'." }  else {"Failed to remove disk from VM'$($vm.elementname)', result code: $Result." }}}	
}
#Example 1: Remove-VMdrive "Tenby" 0 1 -SCSI -DiskOnly -Server "James-2008" 
#           Remove the Disk from the drive at device 1 of SCSI controller 0 of the VM named "Tenby" on the Server "James-2008" 
#Example 2: Remove-VMdrive $Core 1 1 -IDE 
#           Remove the Disk and drive at device 1 of IDE controller 1 in the VM pointed to by $core


Function Add-VMFloppyDisk
{Param ($VM , $VFDPath , $server="."  )
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -server $Server) }
 if ($VM -is [System.Management.ManagementObject]) { 
     if ($vfdpath.StartsWith(".\")) {$VfDpath= join-path $PWD $vfdPath.Substring(2)  }
     else                           { if ((split-path $VfDPath)  -eq "" ) {$vfdPath  = join-path (Get-VhdDefaultPath $Server) $vfdPath } }
     if (-not $vfdpath.toUpper().endswith("VFD")) {$vfdPath = $vfdPath + ".vfd"}
     $diskRASD=NEW-VMRasd -resType 21 -resSubType 'Microsoft Virtual Floppy Disk' -server $vm.__Server 
     $diskRASD.parent=(Get-WmiObject -computerName $vm.__server -NameSpace "root\virtualization" -Query "Select * From MsVM_ResourceAllocationSettingData Where instanceId Like 'Microsoft:$($vm.name)%' and resourceSubtype = 'Microsoft Synthetic Diskette Drive'").__Path
     $diskRASD.connection=$VFDPath
     $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     $arguments = @($VM.__Path, @( $diskRASD.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null, $null )
     $Result=$VSMgtSvc.psbase.invokeMethod("AddVirtualSystemResources", $arguments)
     if ($result -eq 0) {"Added floppy to VM '$($vm.elementname)'." }  else {"Failed to add floppy to VM'$($vm.elementname)', result code: $Result." }}
}
#Example: add-VMFloppyDisk $core "C:\Users\Public\Documents\Microsoft Hyper-V\Blank Floppy Disk\blank.VFD"  
#         Adds a floppy disk to the machine Pointed to by $Core , the VFD being Blank.vfd.      



Filter Remove-VMFloppyDisk
{Param ($VM , $server="."  )
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMFloppyDisk -VM $_  }}
 if ($VM -is [System.Management.ManagementObject]) {
	 $floppy = Get-VmFloppyDisk -VM $vm -Server $Server
         if ($floppy)   {  $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
                            $arguments = @($VM.__Path, @( $Floppy.__Path ), $null ) 
                            $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
                            if ($result -eq 0) {"Removed Floppy from VM '$($vm.elementname)'." }  else {"Failed to remove Floppy from VM'$($vm.elementname)', result code: $Result." }}	}
	
}



Filter Get-VMFloppyDisk
{Param ($VM , $server="."  )
 if ($VM -eq $null) {$VM=$_}
 if ($VM -eq $null) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMFloppyDisk -VM $_  }}
 if ($VM -is [System.Management.ManagementObject]) {
 Get-WmiObject -computerName $vm.__server -NameSpace "root\virtualization" -Query "Select * From MsVM_ResourceAllocationSettingData Where instanceId Like 'Microsoft:$($vm.name)%' and resourceSubtype = 'Microsoft Virtual Floppy Disk'"}
 $vm = $null
}
#Example: Get-VMFloppyDisk (get-vm -server james-2008) | foreach {$_.connection} 
#         Dumps a list of all the VFD files in the floppy drives of VMs on the server "James-2008"


########################This function is properly tested yet and support status is unknown :-) Use at your own risk
Function Get-VmBackupScript
{param($vm="%", $BackupDevice="e:\" , $SERVER=".", [switch]$dataFiles ) 
 Write-Warning "This method of backup is not supported by Microsoft product support services "
 
 # We have to make shadow copies of all volumes touched by hyper-v, and the system drive. 
 # So get a list disk volumes, for all VMs, 
 # Add their parent disks
 # Add the VMs Snapshot and Data folders 
 # Add the OS System Directory 
 # then check which volumes are in the list above. 
 
 $diskPaths  = Get-VMDisk -vm % -Server $Server -snapshot | foreach {$_.diskpath} | select-object -unique          
 $diskpaths += $Diskpaths | where {$_.toupper().endswith("VHD")} | Get-VHDInfo | forEach {$_.parentPath} | select-object -unique
 $diskpaths += Get-WmiObject -NameSpace "root\virtualization" -class "Msvm_VirtualSystemGlobalSettingData" | foreach-object {$_.SnapshotDataRoot ; $_.ExternalDataRoot } | select-object -unique 
 $diskpaths += (get-wmiobject -computerName $server -class win32_operatingSystem).systemdirectory
 $vols=&{foreach ($volume in (get-wmiobject -computerName $server -class Win32_volume)) {
                 if ( ($null -ne ($diskpaths | where {$_ -match $volume.name.replace("\","\\")})     ) -or
                      ($null -ne ($diskpaths | where {$_ -match $volume.DeviceID.replace("\","\\")}) )     ) {$volume} }
         }
   "Rem this method of backup may work, but is not supported by Microsoft."
   "Rem CMD file for Backing up vhds on Server $server - Stage 1 build the script for diskshadow"
   "echo Delete Shadows all                                    > Script.dsh "
   "echo Set Context Persistent                               >> Script.dsh "
   "echo Begin Backup                                         >> Script.dsh "
   "echo Writer Verify {66841cd4-6ded-4f4b-8f17-fd23f8ddc3de} >> Script.dsh "
   
   #The script is going to have one line for each volume          e.g. "Add Volumne C:\ Alias MyShadow1 
   #And then later it will expose the shadow copy using the alias e.g. "Expose %MyShadow1%" 
   #And then it runs a script which                               e.g. "Exec copyVM1.cmd"
   #If the VM(s) being backed up don't use that volume then CopyVMn.cmd won't do much !
   
   $Vols | foreach -begin {$i=0} -process {$i++ ; 
   "echo Add Volume $($_.name) ALIAS MyShadow$i                       >> Script.dsh "}
   "echo Create                                               >> Script.dsh "
   "echo End Backup                                           >> Script.dsh "
   $Vols | foreach -begin {$i=0} -process {$i++ ; 
   "echo Expose %MyShadow$i% X:                                >> Script.dsh "
   "echo Exec CopyVM$i.CMD                                     >> Script.dsh "}
   "echo Unexpose X:                                          >> Script.dsh "
   "echo Exit                                                 >> Script.dsh "   
   "  "
 
 # Now get the disk paths and data paths for the VM(s) being backed up 
 $diskPaths =  Get-VMDisk -vm $vm -Server $Server -snapshot | foreach {$_.diskpath} | select-object -unique          
 $diskpaths += $Diskpaths | where {$_.toupper().endswith("VHD")} | Get-VHDInfo | forEach {$_.parentPath} | select-object -unique
 If ($Datafiles) {$Datapaths = (Get-WmiObject -NameSpace "root\virtualization" -Query "Select * from  Msvm_VirtualSystemGlobalSettingData where Elementname like '$VM'" | 
                                foreach-object {$_.SnapshotDataRoot ; $_.ExternalDataRoot } )| sort |  select-object -unique }                              
 else {$Datapaths = @()}      
 
 $Vols | foreach -begin {$i=0} -process {$i++ ; 
 $vn          = $_.name
 $vid         = $_.DeviceID
 $vnRegEx     = $_.name.replace("\","\\").replace("?","\?")
 $vidRegEx    = $_.DeviceID.replace("\","\\").replace("?","\?")
 $destination = $Backupdevice + ($_.__Server) + $_.DeviceID.SPLIT("?")[1] 
   
   "Rem Build the CMD file to be run by disk shadow for $vn, as copyvm$i.cmd"
   "echo MD ""$destination"" > CopyVM$i.CMD" 
   $Datapaths | where {$_ -match "^$vnRegEx"}  | foreach {$CopyFrom=$_.replace($vn,'X:\')
                              $CopyTo  =$_.replace($vn,$destination)
                              "echo MD """+ $CopyTo.Substring(0,$copyto.lastIndexof("\")) + """   >> copyVM$i.CMD"
                              "echo RoboCOPY ""$CopyFrom""  ""$Copyto"" /E >> CopyVM$i.Cmd " }
   $Datapaths | where {$_ -match "^$vidRegEx"} | foreach {$CopyFrom=$_.replace($vn,'X:\')
                              $CopyTo  =$_.replace($vn,$destination)
                              "echo MD """+ $CopyTo.Substring(0,$copyto.lastIndexof("\")) + """   >> copyVM$i.CMD"
                              "echo RoboCOPY ""$CopyFrom""  ""$Copyto"" /E >> CopyVM$i.Cmd " }
   
   $diskpaths | where {$_ -match "^$vnRegEx"}  | foreach {$CopyFrom=$_.replace($vn,'X:\')
                              $CopyTo  =$_.replace($vn,$destination)
                              "echo MD """+ $CopyTo.Substring(0,$copyto.lastIndexof("\")) + """   >> copyVM$i.CMD"
                              "echo COPY ""$CopyFrom""  ""$Copyto""  >> CopyVM$i.Cmd " }
   $diskpaths | where {$_ -match "^$vidRegEx"} | foreach {$CopyFrom=$_.replace($vid,'X:\')
                              $CopyTo  =$_.replace($vid,$destination)
                              "echo MD """+ $CopyTo.Substring(0,$copyto.lastIndexof("\")) + """   >> copyVM$i.CMD"
                              "echo COPY ""$CopyFrom""  ""$Copyto""  >> CopyVM$i.Cmd " }
   }
   ""
   "Rem Now Invoke disk shadow, and get out of here "
   "DiskShadow /s Script.dsh "   
   " "   
   "Exit"
}
#Example 1: Get-VmBackupScript -Server Hyper-Core -BackupDevice R: |  winrs -r:hyperCore cmd.exe
#           Gets a backup script to copy the VHDs and ISOs for all vms on the server "hyper-core" to its drive R:, and runs it by piping it into a WINRS session. 
#Example 2: Get-VmBackupScript -datafiles |  CMD
#           Gets a backup script to copy the VHDs, ISOs and Datafiles for all vms on the local server and runs it by piping it into CMD.exe 


######################################################################################################
#                                                                                                    #
# Functions for working with Networking, (NICS, switches and ports on switches that nics connect to) #
#                                                                                                    #
######################################################################################################

Function Get-VMSwitch
{Param ($name="%",$server=".")
 $Name=$Name.replace("*","%")
  Get-WmiObject -computerName $server -NameSpace  "root\virtualization" -query "Select * From MsVM_VirtualSwitch Where elementname like '$name' "
}


Function Choose-VMSwitch
{Param ($server="." )
 choose-list  (Get-Vmswitch -server $server) @(@{Label="Switch Name"; Expression={$_.ElementName}} )
}
#Example: Choose-VmSwitch -Server James-2008
#         Prompts the user to choose from the available switches on the server James-2008


Filter Get-VMNic
{Param ($VM, $server="." ,  [switch]$Legacy, [switch]$VMBus)
 if ((-not ($legacy)) -and (-not ($VmBus)) ) {$vmbus = $legacy=$True}
 if ($VM -eq $null) {$VM=$_}
 if ($VM -eq $null) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {if ($legacy) {$VM | ForEach-Object {Get-VmNic -VM $_ -legacy} }
                       if ($vmbus)  {$VM | ForEach-Object {Get-VmNic -VM $_ -VMbus } } }
 if ($VM -is [System.Management.ManagementObject]) {$vssd = Get-VMSettingData $vm 
    if ($legacy) {Get-WmiObject -computerName $vm.__server -NameSpace "root\virtualization" -Query "Associators of {$vssd} where resultClass=MsVM_EmulatedEthernetPortSettingData" }
    if ($vmbus)  {Get-WmiObject -computerName $vm.__server -NameSpace "root\virtualization" -Query "Associators of {$vssd} where resultClass=MsVM_SyntheticEthernetPortSettingData"}}
 $vm = $null
}
#Example: Get-VMNic $core -legacy -vmbus
#         Returns both Legacy and VMbus NICs found on the VM pointed to by $core


Filter Get-VMNicport
{Param ($nic) 
 if ($nic -eq $null) {$nic=$_}
 if ($nic -is [System.Management.ManagementObject]) {
   Get-WmiObject -computerName $nic.__server -NameSpace "root\virtualization" -Query "Select * From Msvm_SwitchPort  where __Path='$( $nic.connection[0].replace('\','\\') )'" }
 $nic=$null
}
#Example: Get-VMNic $core -legacy -vmbus | get-vmNicPort
#         Returns the SwitchPorts on the NICs of the VM pointed to by $core 


Filter Get-VMnicSwitch
{Param ($nic)
 if ($nic -eq $null) {$nic=$_}
 if ($nic -is [System.Management.ManagementObject]) {
     $NicPort=Get-VMNicPort $nic
	if ($nicPort) {Get-WmiObject -computerName $nic.__server  -NameSpace "root\virtualization" -Query "ASSOCIATORS OF {$nicPort} where resultclass = Msvm_VirtualSwitch" }
        else {"Not Connected"}}
 $nic = $null
}
#Example:  (Get-VMNic $vm -legacy -vmbus | get-vmNicSwitch) | foreach-object {$_.elementName}
#         Returns the Switches used by the VM pointed to by $core 


Filter Choose-VMNIC
{param ($vm, $server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [System.Management.ManagementObject]) {
     choose-list (get-vmnic $vm -legacy -vmbus) @("ResourceSubType", "address", @{label="Network"; expression={(get-vmnicSwitch $_).elementname}}) }
}
#Example: choose-vmnic $Core
#          Allows the user to choose from the NICs on the server pointed to by $core


Filter Add-VMNIC
{Param ($VM , $Virtualswitch, $mac, $GUID=("{"+[System.GUID]::NewGUID().ToString()+"}") , $server=".", [switch]$legacy )
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {if ($legacy) {$VM | ForEach-Object {add-VmNic -VM $_ -Virtualswitch $Virtualswitch -legacy} }
                              else  {$VM | ForEach-Object {add-VmNic -VM $_ -Virtualswitch $Virtualswitch} } }
 if ($VM -is [System.Management.ManagementObject]) {
     # As before We're going to call ADD Virtual System Resources, and we need set up the parameter array...
     # Create the correct Resource Allocation Setting Data object    
     if ($Legacy) {$NicRASD = NEW-VMRasd -resType 10 -resSubType 'Microsoft Emulated Ethernet Port' -server $vm.__Server 
                   $NicRASD.ElementName= "Legacy Network Adapter"}  
     else         {$NicRASD = NEW-VMRasd -resType 10 -resSubType 'Microsoft Synthetic Ethernet Port' -server $vm.__Server 
                   $NicRASD.VirtualSystemIdentifiers=@($GUID)
                   $NicRASD.ElementName= "VMBus Network Adapter"}     
     if ($virtualSwitch -ne $null) {$Newport = new-VmSwitchport $virtualSwitch -server $vm.__Server 
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
#Example 1: Add-VMNIC $tenby (choose-VMswitch)
#         adds a VMbus nic to the server  choosing the connection from a list of switches
#Example 2: Add-VMNIC $tenby (choose-VMswitch) -legacy
#         adds a Legacy nic to the server  choosing the connection from a list of switches
#Example 3: get-vm core-% -Server James-2008 | add-vmnic -virtualSwitch "Internal Virtual Network" -legacy
#         Adds a legacy nic to those VMs on Server James-2008 which have names beginning Core- and binds them to "Internal virtual network"


Function Set-VMNICSwitch
{Param ($VM , $nic, $Virtualswitch, $Server=".") 
  # Same process as in Add-VMNIC
  if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
  if ($virtualSwitch -ne $null) {$Newport = new-VmSwitchPort $virtualSwitch}
  if ($Newport -eq $null) {$Newport= ""}
  $Oldport=Get-WmiObject -computerName $vm.__server -NameSpace "root\virtualization" -Query "Select * From Msvm_SwitchPort  where __Path='$( $nic.connection[0].replace('\','\\') )' "
  if ($oldPort -ne $null) {
       $SwitchMgtSvc=(Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Query "Select * From MsVM_VirtualSwitchManagementService")
       $arguments=@($oldport)
       $result = $SwitchMgtSvc.psbase.invokeMethod("DeleteSwitchPort",$arguments)  
       if ($Result -eq 0 ) {"Removed Switch port."} else {"Failed to remove switch port."}}  
  $nic.connection = $newPort
  $arguments = @($VM.__Path, @( $nic.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null )                                                                                                                                                                                             
  $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
  $result=$VSMgtSvc.psbase.invokeMethod("ModifyVirtualSystemResources", $arguments) 
  if ($Result -eq 0 ) {"Changed network connection on VM '$($vm.elementName)'."} else {"Failed to change Network connection on VM '$($vm.elementName)'."}
}
#Example: Set-VMNICSwitch $core (choose-vmNic $core) (choose-VMswitch $core.__server)
#         Re-connects a NIC on the VM pointed to by $core, if there are multiple NICs the user will prompted to select one, and they will be prompted to select a switch


Function Set-VMNICAddress
{Param($vm, $nic, $mac, $Server=".")
         if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
	 if ($mac -match "^[0-9|a-f]{12}$") {
	         $nic.address=$mac 
		 $nic.staticMacAddress=$true
	         $arguments = @($VM.__Path, @( $nic.psbase.GetText([System.Management.TextFormat]::WmiDtd20) ), $null )
        	 $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
	         $result=$VSMgtSvc.psbase.invokeMethod("ModifyVirtualSystemResources", $arguments) 
        	 if ($Result -eq 0 ) {"Set Mac Address on VM '$($vm.elementName)'."} else {"Failed to Set Mac Address on VM '$($vm.elementName)'."}}
}
#Example: Set-VMNICAddress $core (choose-vmNic $core) "00155D010101"
#         Sets the MAC address of a NIC on the pointed to by $core, if there are multiple NICs the user will prompted to select one


Function New-VMSwitchPort
{Param ($virtualSwitch , $Server=".") 
 if ($Virtualswitch -is [String]) {$Virtualswitch=Get-vmSwitch -name $virtualSwitch -server $server}
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
#Not intended to be called directly, used by Add-VMNIC and SetVMNICConnection


Function Remove-VMNIC
{Param ($VM , $nic, $Server=".") 
 if ($VM -is  [String]) {$VM=(Get-VM -Name $VM -Server $Server) } 
 if ($NIC -is [Array]) {$NIC | ForEach-Object {Remove-VMNIC -VM $VM -NIC $_ } }
 if (($VM -is [System.Management.ManagementObject]) -and ($nic -is [System.Management.ManagementObject]) ) {
	 Set-VMNICSwitch -VM $vm -Nic $nic $null 
 	 $arguments = @($VM.__Path, @( $nic.__Path ), $null )                                                                                                                                                                                             
         $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
         $result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemResources", $arguments) 
         if ($Result -eq 0 ) {"Removed NIC from VM '$($vm.elementName)'."} else {"Failed to remove NIC from VM '$($vm.elementName)'."}  }
}
#Example: Remove-VMNIC $core (choose-vmNic $core) 
#         Removes a NIC on server pointed to by $core, if there are multiple NICs the user will prompted to select one


Filter Get-VMByMACaddress
{Param($mac, $Server=".")
 if ($mac -is [Array]) {$mac | ForEach-Object {Get-VMByMACaddress -mac $_ -server $server } }
 if ($mac -eq $Null) {$mac = $_}
 Get-VMNic -server $Server | where {$_.address -match "$Mac"} | foreach {get-wmiobject -computername $_.__Server -namespace root\virtualization -query "Select * from msvm_computersystem where name='$($_.InstanceID.split(""\"")[0].split("":"")[-1])'"}
 $mac=$null
}
#Example 1: Get-VMbymacAddress "00155D000101"
#           Returns detils of the VM with the NIC given the address 00155D000101
#Example 2: get-vm (get-vmbyMacAddress  "00155DD0BEEF").vm
#            Returns the WMI object representing that VM. 


Function New-VMPrivateSwitch 
{ param ($virtualSwitchName=$(Throw("You must specify a name for the switch")),$ports=1024, $server=".") 
  $SwitchMgtSvc=(Get-WmiObject -ComputerName $Server  -NameSpace  "root\virtualization" -Query "Select * From MsVM_VirtualSwitchManagementService")
  $arguments=@($virtualSwitchName,$virtualSwitchName,$Ports,$null,$null)
  $result=$SwitchMgtSvc.psbase.invokeMethod("CreateSwitch",$arguments)
  if ($result -eq 0) {Write-host "Created Virtual Switch $virtualSwitchName"
                  $arguments[4]}
  else {Write-host "Failed to create Virtual Switch, Result code $result"}
}
#Example: New-VMPrivateSwitch "VM network" -server "HVCore"
#         Creates a Switch on the server named core. The network will not be accessible in the host OS, and will be named  "VM Network" in Hyper-V
# Note, I have not provided a delete function for this, (Use the GUI) if you want to wrtite one look at the DeleteSwitch method VirtualSwitchManagementService


Function New-VMInternalSwitch
{ param ($virtualSwitchName=$(Throw("You must specify a name for the switch")),$ports=1024, $server=".") 
  $SwitchWMIPath = New-VMPrivateSwitch $virtualSwitchName $ports $server
  $SwitchMgtSvc=(Get-WmiObject -ComputerName $Server  -NameSpace  "root\virtualization" -Query "Select * From MsVM_VirtualSwitchManagementService")
  $arguments=@($SwitchWMIPath, ($virtualSwitchName+ "_InternalPort"),($virtualSwitchName + "_InternalPort"), $null, $null )
  $Result=$SwitchMgtSvc.psbase.invokeMethod("CreateSwitchPort",$arguments)
  if ($Result -eq 0) { $SP=$arguments[4]
                   $arguments=@($virtualSwitchName,$virtualSwitchName,$null )
                   $result=$SwitchMgtSvc.psbase.invokeMethod("CreateInternalEthernetPortDynamicMac",$arguments)
                   $IntEthPort=$arguments[2]
                   $endPoint=(Get-WmiObject  -NameSpace "root\virtualization" -Query "ASSOCIATORS OF {$IntEthPort} where resultClass = Msvm_SwitchLANEndpoint")
			 $arguments=@($SP,$endpoint,$null)
			 $result=$SwitchMgtSvc.psbase.invokeMethod("ConnectSwitchPort",$arguments)
			 if ($Result -eq 0) {write-host "Bound Internal Ethernet Port to Switch"
                                   $arguments[2]}
                   else {write-host "Failed to Bind Internal Ethernet to Switch port, result code $Result"}
                  }
  else {Write-host "Failed to Create Switch port, result Code $Result"}   
}
#Example: New-VMInternalSwitch "Host and VM network"
#         Creates a Switch and virtual NIC in the host. The device name for the NIC in the host and the Network name in Hyper-V will be "Host and VM Network"
# Note, I have not provided a delete function for this, (Use the GUI) if you want to wrtite one look at the DisconnectSwitchPort, DeleteSwitchPort and DeleteInternalEthernetPort and DeleteSwitch methods of the VirtualSwitchManagementService   

 
Function choose-VMExternalEthernet
{param ($server=".") 
 choose-list (Get-WmiObject -ComputerName $Server -NameSpace "root\virtualization" -query "Select * from Msvm_ExternalEthernetPort where isbound=false") -property Name }


Filter New-VMExternalSwitch
{ param ($virtualSwitchName=$(Throw("You must specify a name for the switch")),$ExternalEthernet,$ports=1024, $server=".") 
  if ($ExternalEthernet -is [String]) {$ExternalEthernet=(Get-WmiObject -NameSpace  "root\virtualization" -q "Select * from Msvm_ExternalEthernetPort where Name like '$ExternalEthernet%' ")}
  if ($ExternalEthernet -eq $Null)    {$ExternalEthernet=$_} 
  if ($ExternalEthernet -is [system.management.managementObject]) {
     If  ($virtualSwitchName -eq $null) {$virtualSwitchName=$ExternalEthernet.name + " Virtual NIC"}
     $SwitchWMIPath = New-VMPrivateSwitch $virtualSwitchName $ports $server
     $SwitchMgtSvc=(Get-WmiObject -ComputerName $Server  -NameSpace  "root\virtualization" -Query "Select * From MsVM_VirtualSwitchManagementService")
     $arguments=@($SwitchWMIPath, ($virtualSwitchName + "_InternalPort"),($virtualSwitchName + "_InternalPort"), $null, $null )
     $Result=$SwitchMgtSvc.psbase.invokeMethod("CreateSwitchPort",$arguments)
     if ($Result -eq 0) { $IntSP=$arguments[4] }
     $arguments=@($SwitchWMIPath,($virtualSwitchName + "_ExternalPort"),($virtualSwitchName + "_ExternalPort"), $null, $null )
     $Result=$SwitchMgtSvc.psbase.invokeMethod("CreateSwitchPort",$arguments)
     if ($Result -eq 0) { $ExtSP=$arguments[4] }
     $arguments=@($ExtSp, $intSp, $ExternalEtherNet.__path, $virtualSwitchName, $virtualSwitchName, $Null)
     $Result=$SwitchMgtSvc.psbase.invokeMethod("SetupSwitch",$arguments)
     if ($Result = 4096) {Test-WMIJob $arguments[5] -wait -Description "Configuring NIC"}
     else {"Network Setup" + $ReturnCode[[string]$result] }  }
   else {"No valid network card was provided"}
}  
#Example 1: choose-VMExternalEthernet |  New-VMExternalSwitch -virtualSwitchName "Wired virtual Network" 	
#           Allows the user to choose if there are multiple available NICs and binds the selected one to a new switch. 
#           The device name for the NIC in created in the host and the Network name in Hyper-V will be "Wired virtual Network" 	
#Example 2: New-VMExternalSwitch -virtualSwitchName "Wired virtual Network" -ext "Broadcom" -Server Core
#           Finds a Nic with a name begining "Broadcom" on the server named "core", and binds it to a new switch. 
#           The device name for the NIC in created in the host and the Network name in Hyper-V will be "Wired virtual Network" 	
# Note, I have not provided a delete function for this, (Use the GUI) if you want to write one look at the TearDownSwitch, DeleteSwitchPort and DeleteSwitch methods of the VirtualSwitchManagementService



############################################################# 
#                                                           # 
# Functions for managing Serial port connections            #
#                                                           #
#############################################################


Filter Get-VMSerialPort
{Param ($VM, $portNo, $server=".")
    if ($VM -eq $null)    {$VM = $_}
    if ($vm -eq $null)    {$vm = "%"}
    if ($VM -is [String]) {$VM = (Get-VM -Name $VM -server $Server)}
    if ($VM -is [Array])  {$VM | ForEach-Object {Get-VMSerialPort -VM $_ -Server $Server -portNo $PortNo} }
    if ($VM -is [System.Management.ManagementObject]) {
         $VSSD = (get-vmsettingData $VM)
         $comPort = Get-WmiObject -namespace "root\virtualization" -computerName $VSSD.__Server -Query "ASSOCIATORS OF {$VSSD} where ResultClass=Msvm_ResourceAllocationSettingData" | where {$_.ResourceSubType -eq 'Microsoft Serial Port'}         
         if ($PortNo) {$ComPort | Where {$_.Caption -like "*$PortNo"} }
         Else {$comPort}}
}
#Example Get-VMSerialPort "core"
 
Filter Set-VMSerialPort
{Param($VM, $PortNo=1, [String]$Connection=$(throw "You must specify a Connection string"), $Server="." )
 if ($VM -eq $null)    {$VM = $_}
 if ($VM -is [String]) {$VM = (Get-VM -Name $VM -server $Server)}
 if ($VM -is [Array])  {$VM | ForEach-Object {Set-VMSerialPortConnection -VM $_ -Server $Server -PortNo $PortNo -Connection $Connection}}
 if ($VM -is [System.Management.ManagementObject]) 
    {$VMMS = (Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Class "Msvm_VirtualSystemManagementService") 
     $comPort = Get-VMSerialPort -vm $vm -Portno $PortNo      
     $comPort.Connection = $Connection
     $arguments = @($VM.__Path, [String[]]@($comPort.psbase.GetText([System.Management.TextFormat]::WmiDtd20)), $null)
     $result = $VMMS.PSBase.InvokeMethod("ModifyVirtualSystemResources", $arguments)
     if ($result -eq 0) { Write-Host "Serial port connection string has been updated." }
     elseif ($result -eq 4096)  {test-wmiJob $arguments[2] -wait -Description "Waiting for serial port connection..." }
     else   {Write-Error "Error.  Result: $result"}
    }
 $VM = $null
}
#Example Set-VMSerialPort "CORE" 2 "\\.\PIPE\WIBBLE"


############################################################# 
#                                                           # 
# Functions for managing VM State (Snapshots and VM Export) #
#                                                           #
#############################################################

Filter Export-VM
{Param ($VM , $path, $Server=".", [switch]$copyState,  [switch]$Wait, [Switch]$Preserve)
 if ($VM -eq $null) {$VM=$_ }
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $server) }
 #if ($VM -is [Array]) {$VM | ForEach-Object {export -VM $_ -Server $server -Path $path} }
 if ($VM -is [System.Management.ManagementObject]) {  
   $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
   $arguments=@($VM.__path,($CopyState.Ispresent),$path,$null)   
   $Result=$VSMgtSvc.psbase.invokeMethod("ExportVirtualSystem", $arguments)   
   if ($wait) {test-wmijob $arguments[3]  -wait -Description "Export of $($vm.elementName)"
               if ($Preserve) {Add-ZIPContent "$path\$($vm.elementname)\importFiles.zip" "$path\$($vm.elementname)\config.xml","$path\$($vm.elementname)\virtual machines"
                               if (test-path "$path\$($vm.elementname)\snapshots") {Add-ZIPContent "$path\$($vm.elementname)\importFiles.zip" "$path\$($vm.elementname)\snapshots"} } } 
   else       {$ReturnCode[[string]$result] | Out-host 
               $arguments[3]} }
$vm=$null
}


Filter Import-VM
{Param ($path, $Server=".", [switch]$reimportVM, [switch]$ReuseIDs,  [switch]$Wait, [Switch]$Preserve)
 if ($path -eq $null) {$Path=$_ }
 if (($Path -is [String]) -and ($server -eq ".")) {$path = (Resolve-Path $path).path}
 if ($Path -is [System.IO.DirectoryInfo]) {$path = $path.fullName} 
 if ($Preserve) {Add-ZIPContent "$path\importFiles.zip" "$Path\config.xml","$path\virtual machines"
                 if (test-path "$path\$($vm.elementname)\snapshots") {Add-ZIPContent "$path\importFiles.zip" "$path\snapshots"}}  
 elseif ($reImportVM -and (test-path "$path\importFiles.zip"))
         {Copy-ZipContent -Zipfile "$path\importFiles.zip" -path $path
         remove-VM -VM $ReimportVM -Server $server -wait}
 $VSMgtSvc=Get-WmiObject -ComputerName $server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
 $arguments=@($path,(-not $ReuseIDs.Ispresent),$null)   
 $Result=$VSMgtSvc.psbase.invokeMethod("importVirtualSystem", $arguments)   
 If ($wait) {test-wmijob $arguments[2]  -wait -Description "Import from $path"} 
 else       {$ReturnCode[[string]$result] | Out-host 
             $arguments[2]} 
$path=$null
}


Filter Get-VMSnapshot
{Param ($VM, $Name="%", $Server=".", [Switch]$newest)
 if ($VM -eq $null) {$VM=$_ }
 if ($VM -eq $null) {$VM="%"}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMSnapshot -VM $_ -Server $server} }
 if ($VM -is [System.Management.ManagementObject]) {
    $Snaps=Get-WmiObject -computerName $vm.__server -NameSpace root\virtualization -Query "Select * From MsVM_VirtualSystemSettingData Where systemName='$($VM.name)' and instanceID <> 'Microsoft:$($VM.name)' and elementName like '$name' " }
    if ($newest) {$Snaps | sort creationTime | select -last 1 } else {$snaps}
 $vm=$null
}
#Example: Get-Vmsnapshot $Core
#         Returns the snapshots on the VM pointed to by $core


Function Get-VMSnapshotTree
{Param ($VM , $Server=".")
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $server) }
 if ($VM -is [System.Management.ManagementObject]) {
   $snapshots=(Get-VMSnapshot -VM $VM -Server $Server) 
   #need to check for 0 or 1 snapshots
   if ($snapshots -is [array]) {out-tree -items $snapshots -startAt ($snapshots | where{$_.parent -eq $null}) -path "__Path" -Parent "Parent" -label "elementname"} }
}
#Example: Get-VmsnapshotTree $Core
#         Returns the snapshots on the VM pointed to by $core and displays them as a tree


Filter Choose-VMSnapshot
{Param ($VM , $Server=".")
 if ($VM -eq $null) {$VM=$_ }
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $server) }
 if ($VM -is [System.Management.ManagementObject]) {
 	$snapshots=(Get-VMSnapshot $VM)
	if ($snapshots -is [array]) {Choose-Tree -items $snapshots -startAt ($snapshots | where{$_.parent -eq $null}) -path "__Path" -Parent "Parent" -label "elementname"}
        else {$snapshots}  }
}
#Example: Choose-Vmsnapshot $Core
#         Gets the Snapshots of the machine pointed to by $core and if there are multiple snap shots prompst the user to select one from a tree 


Filter New-VMSnapshot
{Param( $VM , $note, $Server=".", [switch]$wait)
 if ($VM -eq $null) {$VM=$_ }
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {New-VMSnapshot -VM  $_ -Server $server -note $note -wait:$wait } }
 if ($VM -is [System.Management.ManagementObject]) {
      #We are going to invoke CreateVirtualSystemSnapshot. Following from MSDN
      #CreateVirtualSystemSnapshot. (
      #  [in]   CIM_ComputerSystem Ref SourceSystem,  // Reference to the virtual computer system to be snapshotted.
      #  [out]  CIM_VirtualSystemSettingData Ref SnapshotSettingData,  //The CIM_VirtualSystemSettingData instance that was created to represent the snapshot.
      #  [out]  CIM_ConcreteJob Ref Job );
      #If this method is executed synchronously, it returns 0 if it succeeds. 
      #If this method is executed asynchronously, it returns 4096 and the Job output parameter can be used to track the progress of the asynchronous operation.
      # Any other return value indicates an error.
      # Virtual system Setting data never seems to be set. 
     $arguments=($VM,$Null,$null) 
     $VSMgtSvc=Get-WmiObject -ComputerName $VM.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     $result=$VSMgtSvc.psbase.invokeMethod("CreateVirtualSystemSnapshot",$arguments)
     "Snapshoting VM '$($vm.elementName)': "+ $ReturnCode[[string]$result] | Out-host
     if ($wait) {test-wmiJob $arguments[2] -wait} else {$arguments[2]}
		 if ($note) {Get-VMSnapshot $vm -newest | set-vm -note $note} }
 $vm=$null
} 
#Example 1: new-vmsnapshot $Core 
#           Takes a snapshot of the VM pointed to by $core
#Example 2: get-vm "core%" -server "James-2008" | new-VmSnapshot -wait
#           Gets the VMs with names beginning "Core" on the server "James-2008" and snapshots them one by one 

Filter Remove-VMSnapshot
{Param( $snapshot , [Switch]$Tree , [Switch]$wait )
 if ($snapshot -eq $null) {$snapshot=$_ }
 if ($snapshot -is [Array]) {$snapshot | ForEach-Object {Remove-VMSnapshot -snapshot  $snapshot} }
 if ($snapshot -is [System.Management.ManagementObject]) {
     #We are going to invoke RemoveVirtualSystemSnapshot. Following from MSDN
     #RemoveVirtualSystemSnapshot(
     #  [in]   CIM_VirtualSystemSettingData Ref SnapshotSettingData, // Reference to the CIM_VirtualSystemSettingData instance which represents the snapshot to be removed.
     #  [out]  CIM_ConcreteJob Ref Job );
     #If this method is executed synchronously, it returns 0 if it succeeds. 
     #If this method is executed asynchronously, it returns 4096 and the Job output parameter can be used to track the progress of the asynchronous operation.  
     #Any other return value indicates an error.
     $arguments=($snapshot,$Null) 
     $VSMgtSvc=Get-WmiObject -ComputerName $snapshot.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     if ($tree) {$result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemSnapshotTree",$arguments) }
     else       {$result=$VSMgtSvc.psbase.invokeMethod("RemoveVirtualSystemSnapshot",$arguments) }
     if ($wait) {test-wmijob $arguments[1]  -wait -Description "Removing snapshot $($snapshot.elementName)  "} 
     else                    { "Removing Snapshot(s): " + $ReturnCode[[string]$result] | out-host
                                $arguments[1] }}
 $snapShot=$null
}
#Example: choose-vmsnapshot $Core | remove-vmsnapshot -tree
#         Lets the user select a snapshot on the VM pointed to by $core and removes it and any children it may have


#ToDo have not tested all the switch combinations here. 
Filter Apply-VMSnapshot
{Param($SnapShot, [Switch]$force , [Switch]$Restart, [Switch]$wait)
 if ($snapshot -eq $null) {$snapshot=$_ }
 if ($snapshot -is [System.Management.ManagementObject]) {
     $VM = Get-WmiObject -computername $snapshot.__server -NameSpace "root\virtualization" -Query ("Select * From MsVM_ComputerSystem Where Name='$($Snapshot.systemName)' " )
  if ($force -and ($vm.enabledState -ne $vmState.Stopped) ) {Write-host "Force switch specified. Stopping VM"; Stop-vm $vm -wait}
  #We are going to call ApplyVirtualSystemSnapshot - the following is from MSDN
  #  [in]   CIM_ComputerSystem Ref ComputerSystem, // Reference to the CIM_ComputerSystem instance to which the snapshot should be applied.
  #  [in]   CIM_VirtualSystemSettingData Ref SnapshotSettingData, // Reference to the CIM_VirtualSystemSettingData instance that represents the snapshot to be applied.
  #  [out]  CIM_ConcreteJob Ref Job );
  #If this method is executed synchronously, it returns 0 if it succeeds. 
  #If this method is executed asynchronously, it returns 4096 and the Job output parameter can be used to track the progress of the asynchronous operation. 
  #Any other return value indicates an error.
     $arguments=@($VM,$snapshot,$null)
     $VSMgtSvc=Get-WmiObject -ComputerName $snapshot.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
     $result=$VSMgtSvc.psbase.invokeMethod("ApplyVirtualSystemSnapshot", $arguments)
     if ($wait -or $restart) {test-wmijob $arguments[2]  -wait -Description "Applying snapshot"} 
     else                    {$ReturnCode[[string]$result] | out-host
			      $arguments[2] }
     if ($Restart) {Write-host "Restart switch specified. Starting VM"; Start-vm $vm}
  } 
}
#Example: choose-vmsnapshot $Core | Apply-vmsnapshot 
#         Lets the user select a snapshot on the VM pointed to by $core and applies it.


Filter Update-VMSnapshot
{Param ($vm , $SnapName, $note, $server=".")
 if ($VM -eq $null) {$VM=$_ }
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Update-VMSnapshot -VM $_ -Server $server -snapName $SnapName -note $note} }
 if ($VM -is [System.Management.ManagementObject]) {
   if ($snapName -eq $null) {$snapName=(Get-VMSnapshot $vm | sort creationTime | select -last 1 ).elementname } 
   If ($snapName) {rename-VMsnapshot  -vm $vm -SnapName $snapName -newName "Delete-me"}
   new-vmSnapshot $vm -wait -note $note
   $TempsnapName=(Get-VMSnapshot $vm | sort creationTime | select -last 1 ).elementname 
   rename-VMsnapshot -vm $vm -snapName $TempSnapName -Newname $snapName
   Get-VmSnapShot $vm -name "Delete-me" | remove-vmSnapShot -wait }
 $vm=$null
}


Filter Rename-VMsnapshot 
{param ($vm, $snapname, $newName, $server=".")
 if ($VM -eq $null) {$VM=$_ }
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $server -snapname $snapName -newName $newName) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Rename-VMSnapshot -VM $_ -Server $server} }
 if (($VM -is [System.Management.ManagementObject]) -and ($snapName -is [String]) -and ($newName -is [string])) {
     $snap=Get-VmSnapshot -vm $vm -name $snapName 
     if ($snap -is [System.Management.ManagementObject]) {
       Write-host "Renaming snapshot '$SnapName' to '$newName'."
       $snap.ElementName=$newName 
       $arguments = @($vm , $Snap.psbase.GetText([System.Management.TextFormat]::WmiDtd20),$null,$null)
       $VSMgtSvc=Get-WmiObject -ComputerName $snap.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService"
       $result=$VSMgtSvc.psbase.InvokeMethod("ModifyVirtualSystem", $arguments)
       $ReturnCode[[string]$result] | out-host }
     }
}


Filter Get-VMJPEG
{Param ($VM,  [int]$Width=800, [int]$Height=600, $Path , $Server=".")
 if ($vm -eq $null) {$vm = $_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMJPeg -VM $_ -Server $Server -Width $Width, -Height $Height, -Path $Path} }
 $VMSettings = (Get-VMSettingData -vm $VM -Server $Server)
 if ($VMSettings -is [System.Management.ManagementObject]) {
       [System.Reflection.Assembly]::LoadWithPartialName("System.Drawing") | out-null
	$VSMgtSvc=Get-WmiObject -computerName $VMSettings.__server -NameSpace "root\virtualization" -Class "MsVM_virtualSystemManagementService"
	$ImgFormat=[System.Drawing.Imaging.PixelFormat]::Format16bppRgb565
	$wo= [System.Drawing.Imaging.ImageLockMode]::WriteOnly	
	# we're going to call the GetVirtualSystemThumbnailImage method of the Virtual System Management Service
	# According to MSDN it's parameters are as follows 
	#   uint32 GetVirtualSystemThumbnailImage(
	#   [in]   CIM_VirtualSystemSettingData REF TargetSystem,
	#   [in]   uint16 WidthPixels,
	#   [in]   uint16 HeightPixels,
	#   [out]  uint8 ImageData[]
	$arguments = @($VMSettings , $Width, $Height , $null)
        $result =$VSMgtSvc.psbase.InvokeMethod("GetVirtualSystemThumbnailImage", $arguments)
	if  ($result  -eq 0) {
	    if ($arguments[3] -ne $null) {
		#Create a bitmap of the requested size in 16BPP format
		$VMThumbnail = new-object System.Drawing.Bitmap( $Width, $Height,  $ImgFormat )
		#Lock the System.Drawing.Bitmap into system memory (the rectangle is a structure specifying the portion to lock.)
		$rectangle = new-object System.Drawing.Rectangle(0,0,$Width,$Height)
		$VMThumbnailBitmapData = $VMThumbnail.LockBits($rectangle, $wo, $ImgFormat)
		#This is a nasty fudge: copy from $RawImageData, starting at byte 0, into memory starting at the first byte of the BitmapData 
	    	[System.Runtime.InteropServices.marshal]::Copy($arguments[3],  0, $VMThumbnailBitmapData.Scan0, ($Width * $Height * 2))
		#Now unlock it 
		$VMThumbnail.UnlockBits($VMThumbnailBitmapData);
		#Cope with the path being blank or pointing to a folder, not a file,  or not being fully qualified
		$JpegPath = $path	
		if ($JpegPath -eq $null) {$JpegPath = $pwd}
		if (test-path $JpegPath -pathtype container) {$JpegPath = join-Path $JpegPath ($VMSettings.elementName + ".JPG") }
 		if ($JpegPath.StartsWith(".\")) {JpegPath= join-path $PWD JpegPath.Substring(2)  }
		$Folder = split-path $JpegPath
		if ($folder -eq "" ) {$JpegPath  = join-Path $pwd $JpegPath }
		else  {$jpegpath=$jpegpath.Replace($Folder , (resolve-path $folder)) }
                if (-not $Jpegpath.toUpper().endswith("JPG")) {$JpegPath = $JpegPath + ".jpg"}
		# ...and save
		$VMThumbnail.Save($JPegPath)
		write-host "Wrote picture of $($vmSettings.ElementName)  ... "
		$jpegPath }
	    else {write-host "No image was returned. Try requesting a smaller size."}  }
        else {write-host "The attempt to get the image failed with code : $result"}    }
 $VM = $null
}
#Example 1 : Get-VMJPEG core
#            Gets a 800x600 jpeg for the machine named core, and writes it as core.jpg in the current folder    
#Example 2:  Get-vm -Running -server "James-2008" | Get-VMJPEG -w 320 -h 240 -path images 
#            Discovers running VMs on Server "James-2008", for each writes a 320x240 size image to the folder named images. 
#Example 3:  While ($true) { Get-VMJPEG -vm "core" -w 640 -h 480 -path ((get-date).toLongTimeString().replace(":","-") + ".JPG") ;  Sleep -Seconds 10}
#            Creates a loop which continues until interupted; in the loop creates an image of the VM Core with a name based on the current time, then waits 10 seconds and repeats

##############################################################
#                                                            #
# List functions added - they all have nouns begin VM or VHD #
#                                                            #  
############################################################## 


Update-FormatData  (Join-Path (get-scriptPath) "hyperv.format.ps1xml")

if ($myinvocation.line -match "^\.\s") {
   "VM Functions loaded"
    if (-not $quiet) {dir "Function:*-VM*","Function:*-VHD*" | select name | sort name } }
Else {write-host -ForegroundColor red "No functions were loaded - you need to invoke with . scriptname "}
#[that gives a list by verb, this will give it sorted by noun] dir Function: | Where {($_.name -match "-VM") -or ($_.name -match "-VHD")} | select name | sort @{expression={$_.name.split("-")[1]}}, name
if (-not (test-admin)) {write-host -foregroundColor Red "This Powershell session does not have elevated priviledges. You may not be able to access Hyper-V features"}

