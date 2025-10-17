#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-VMLogs.ps1
## Purpose:        Copy logs from test VMs
## Requirements:   Windows Powershell 2.0, 3.0
## Supported OS:   Windows Server 2012, Windows Server 2012R2
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
[string]$workingDir = "D:\WinteropProtocolTesting",
[string]$protocolName  = "FileSharing",  # e.g. FileSharing, MS-SMB2
[string]$testLogDir  = "$workingDir\TestResults\$protocolName",
[string]$VMConfigFileName  = ("$workingDir\$protocolName\VSTORMLITEFiles\XML\$protocolName" + ".xml")
)

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath"

#----------------------------------------------------------------------------
# Common functions
#----------------------------------------------------------------------------
Function CopyLogsFromVHD($vhdPath, $testLogDir,$postscript)
{
    #----------------------------------------------------------------------------
    # Start Script
    #----------------------------------------------------------------------------
    Write-Info.ps1 "======================================="
    Write-Info.ps1 "Copy Logs from VHD"
    Write-Info.ps1 "VhdPath:  $VhdPath"
    Write-Info.ps1 "testLogDir:  $testLogDir"
    Write-Info.ps1 "---------------------------------------"

    #----------------------------------------------------------------------------
    # Get Available Drive Letter
    #----------------------------------------------------------------------------
    $driveLetter = ""
    $drivePath = ""
    foreach ($letter in [char[]]([char]'O'..[char]'Z')) 
    { 
        $driveLetter = $letter
        $drivePath = $letter + ":"
        $l = get-wmiobject win32_logicaldisk | where {$_.DeviceID -eq $drivePath}
        if ($l -eq $null -and (Test-Path -path $drivePath) -eq $false)
        { 
            break 
        } 
    }
    Write-Info.ps1 "`nGet available driver letter: $driveLetter"
	
    #----------------------------------------------------------------------------
    # Attach VHD to disk manager
    #----------------------------------------------------------------------------
    Write-Info.ps1 "`nAttach VHD to disk manager"
    $diskpartScript = @()
    $diskpartScript += "select vdisk file=`"$VhdPath`""
    $diskpartScript += "attach vdisk readonly"
    $diskpartScript += "attributes disk clear readonly"
    $diskpartScript += "select partition 1"
    $diskpartScript += "list partition"
    $diskpartScript += "active"
    $diskpartScript += "rescan"
    $diskpartScript += "select partition 1"
    $diskpartScript += "select volume"
    $diskpartScript += "list volume"
    $diskpartScript += "assign letter=$driveLetter"
    $diskpartScript += "rescan"
    $diskpartScript += "exit"

    Write-Info.ps1 "Execute diskpart script: "
    $diskpartScript | % {Write-Info.ps1 $_}

    $diskpartScript | diskpart 
    sleep 10

    #----------------------------------------------------------------------------
    # Copy test logs
    #----------------------------------------------------------------------------
    Write-Info.ps1 "Copy VM log"
    powershell -file $workingDir\ScriptLib\Copy-VMLog.ps1 $drivePath $testLogDir $postscript

    #----------------------------------------------------------------------------
    # Detach VHD
    #----------------------------------------------------------------------------
    Write-Info.ps1 "`nDetach VHD"
    $diskpartScript = $null
    $diskpartScript = @()
    $diskpartScript += "select vdisk file=`"$VhdPath`""
    $diskpartScript += "select partition 1"
    $diskpartScript += "remove letter=$driveLetter"
    $diskpartScript += "detach vdisk"
    $diskpartScript += "rescan"
    $diskpartScript += "exit"

    Write-Info.ps1 "Execute diskpart script: "
    $diskpartScript | % {Write-Info.ps1 $_}

    $diskpartScript | diskpart
    sleep 5
}

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
if(!(Test-Path $testLogDir))
{
    md $testLogDir
}
Start-Transcript -Path "$testLogDir\Copy-VMLogs.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Get VM config
#----------------------------------------------------------------------------
Write-Info.ps1 "Get VM config from $VMConfigFileName" -foregroundcolor Yellow
$VMConfigFile = $VMConfigFileName
if($VMConfigFileName.Contains("\") -eq $false)
{
    # If VMConfigFileName is not a full path, find out the file
	$item = Get-ChildItem -Recurse -Path "$workingDir\$protocolName" | where {$_.Name -eq "$VMConfigFileName"}
    $VMConfigFile = $item.FullName
}

[xml]$VMConfig = get-content $VMConfigFile
$vms = $VMConfig.hyperv.server.vm
if($vms -eq $null)
{
    # To support both XML structures
	$vms = $vmconfig.lab.servers.vm
}

#----------------------------------------------------------------------------
# Take VM screenshot to show VM's latest state
#----------------------------------------------------------------------------
$vmScreenshotPath = "$testLogDir\VMScreenshots"
if(!(Test-Path $vmScreenshotPath))
{
    CMD /C MKDIR $vmScreenshotPath 2>&1 | Write-Info.ps1 
}

foreach ($vm in $vms)
{	
    $vmName = $vm.hypervname
	if($vmName -eq $null) {$vmName = $vm.name}
	$vmobj = get-vm $vmName
	if($vmobj -eq $null) {continue}
		   
    Write-Info.ps1 "Take screenshot for VM: $vmName"    
    Take-VMScreenshot.ps1 -VMName $vmName -ScreenshotPath "$vmScreenshotPath\$VMName.jpg"
}

#----------------------------------------------------------------------------
# Copy Logs from VMs
#----------------------------------------------------------------------------
Write-Info.ps1 "Copy Logs from VMs ..." -foregroundcolor Yellow
try{
foreach ($vm in $vms)
{	
    $vmName = $vm.hypervname # hypervname is a new item in latest xml file
	if($vmName -eq $null) {$vmName = $vm.name}
	$vmobj = get-vm $vmName
	if($vmobj -eq $null) {continue}
		
    $postscript = 	$vm.postscript
    
    Write-Info.ps1 "Checkpoint VM $vmName"
    Checkpoint-VM -VM $vmobj -SnapshotName "To copy logs"
    $vhdPath = (Get-VHD -VMId $vmobj.VMId).ParentPath
        
    if($vhdPath -ne $null -and (Test-Path $vhdPath) -eq $true)
    {
        CopyLogsFromVHD $vhdPath "$testLogDir\$vmName" $postscript
    }
}

}
catch{
    Write-Info.ps1 "VM Logs copy failed" -foregroundcolor Yellow
}
#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Stop-Transcript
exit 0