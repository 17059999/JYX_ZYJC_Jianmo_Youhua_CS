#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Remove-VMs.ps1
## Purpose:        Destroy and Remove VM folders
## Requirements:   Windows Powershell 2.0, 3.0
## Supported OS:   Windows Server 8, Windows Server 2012
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
# Check parameters
#----------------------------------------------------------------------------
if($workingDir -eq $null -or $workingDir.Trim() -eq "")
{
    Write-Info.ps1 "workingDir Could not be null or empty." -ForegroundColor Red
    return 1;
}
if($protocolName -eq $null -or $protocolName.Trim() -eq "")
{
    Write-Info.ps1 "protocolName Could not be null or empty." -ForegroundColor Red
    return 1;
}
if($testLogDir -eq $null -or $testLogDir.Trim() -eq "")
{
    Write-Info.ps1 "testLogDir Could not be null or empty." -ForegroundColor Red
    return 1;
}
if($VMConfigFileName -eq $null -or $VMConfigFileName.Trim() -eq "")
{
    Write-Info.ps1 "VMConfigFileName Could not be null or empty." -ForegroundColor Red
    return 1;
}

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
if(!(Test-Path $testLogDir))
{
    md $testLogDir
}
Start-Transcript -Path "$testLogDir\Remove-VMs.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Determine current OS
#----------------------------------------------------------------------------
$osbuildnum= "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
if ([double]$osbuildnum -lt [double]"6.1") 
{
   Write-Info.ps1 "Unsupported OS which older than Windows Server 2008, will exit immediately" -ForegroundColor Red
   exit 1
} 


#----------------------------------------------------------------------------
# Import HyperV module
#----------------------------------------------------------------------------
if ([double]$osbuildnum -eq [double]"6.1")  
{
    # Verify we have the hyperv module to import
    if (Get-Module -ListAvailable hyperv)
    {
	    Write-Info.ps1 "Found HyperV v1.0 Powershell module"
    } 
    else 
    {
	    Robocopy $WorkingDir\hyperv C:\Windows\System32\WindowsPowerShell\v1.0\Modules\hyperv /mir 
    }

    # Import the Hyperv Management Module
    if ( !(Get-Module hyperv) ) 
    {
	    Import-Module Hyperv
	    Write-Info.ps1 "Importing HyperV v1.0 Powershell module"
    }
}

if ([double]$osbuildnum -ge [double]"6.2") 
{
    # Install Hyperv Module if not exist
    Import-Module ServerManager
    $featureName = "RSAT-Hyper-V-Tools"
    $feature = Get-WindowsFeature | where {$_.Name -eq "$featureName"}
    if($feature.Installed -eq $false)
    {
        Add-WindowsFeature -Name $featureName -IncludeAllSubFeature -IncludeManagementTools
        sleep 5
    }
    
    # Import Hyper-V module
    if ( (Get-Module -ListAvailable Hyper-V) )
    {
	    Import-Module Hyper-V
	    Write-Info.ps1 "Importing HyperV v3.0 Powershell module"
    }
    else 
    {
	    Write-Info.ps1 "HyperV v3.0 Powershell module is missing, please check OS Installation"
    }
}

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

$vnets = $VMConfig.hyperv.virtualSwitch.vnet
if($vnets -eq $null)
{
    $vnets = $vmconfig.lab.network.vnet
}

#----------------------------------------------------------------------------
# Remove VMs according to VM config
#----------------------------------------------------------------------------
Write-Info.ps1 "Remove VMs according to VM config ..." -foregroundcolor Yellow
foreach ($vm in $vms)
{
    $vmName = $vm.hypervname # hypervname is a new item in latest xml file
	if($vmName -eq $null) {$vmName = $vm.name}
	$vmobj = get-vm | where {$_.Name -eq $vmName}
	if($vmobj -eq $null) 
    {
        Write-Info.ps1 "VM $vmName does not exist."
        continue
    }
		
    if ([double]$osbuildnum -eq [double]"6.1") 
    {
	    get-vm $vmName -Running | stop-vm -force -Wait
        sleep 10
	    get-vm $vmName | Remove-VM -Force
        sleep 5
    }
    
     if ([double]$osbuildnum -ge [double]"6.2") 
     {        
        $retryTimes = 0
        while($vmobj -ne $null -and $vmobj.State -ne "Off" -and $retryTimes -lt 5)
        {
            Write-Info.ps1 ("VM $vmName is " + $vmobj.State + ", try to stop it...")
            stop-vm $vmName -force -ErrorAction SilentlyContinue -Confirm:$false
            Sleep 10
            $retryTimes++ 
            $vmobj = get-vm | where {$_.Name -eq $vmName}
        }

        $retryTimes = 0
        while($vmobj -ne $null -and $retryTimes -lt 5)
        {
            Write-Info.ps1 "Try to remove VM $vmName"
            Remove-VM $vmName -Force -ErrorAction SilentlyContinue -Confirm:$false
            Sleep 10
            $retryTimes++ 
            $vmobj = get-vm | where {$_.Name -eq $vmName}
            if($vmobj -ne $null)
            {
                Write-Info.ps1 "VM $vmName still exist."
            }
        }
     }
}

#----------------------------------------------------------------------------
# Remove network switches according to VM config
#----------------------------------------------------------------------------
Write-Info.ps1 "Remove network switches according to VM config ..." -foregroundcolor Yellow
foreach ($vnet in $vnets)
{
    $switchName = $vnet.name
    Write-Info.ps1 "Remove VM switch $switchName"
    if ([double]$osbuildnum -eq [double]"6.1") 
    {
	    Get-VMSwitch | where{$_.ElementName -eq $switchName -and $_.ElementName -notmatch "external"} | Remove-VmSwitch -Force
    }

    if ([double]$osbuildnum -ge [double]"6.2") 
    {        
        Get-VMSwitch | where {$_.Name -eq "$switchName" -and $_.SwitchType -ne "External"} | Remove-VmSwitch -Force -ErrorAction SilentlyContinue
    }
}

#----------------------------------------------------------------------------
# Remove internal network from hyperv host if exist
#----------------------------------------------------------------------------
if ([double]$osbuildnum -eq [double]"6.1") 
{
    Write-Info.ps1 "Remove internal network from hyperv host if exist"
    $virtualSwitchMgmtSvc = Get-WMIObject Msvm_VirtualSwitchManagementService -namespace "root\virtualization"

    foreach ($vnet in $vnets)
    {
        $switchName = $vnet.name
        $items = Get-WMIObject Msvm_InternalEthernetPort -namespace "root\virtualization" | where {$_.ElementName -eq $switchName}
        foreach ($item in $items)
        {
            Write-Info.ps1 "Remove InternalEthernetPort for Vm switch $switchName"
            $virtualSwitchMgmtSvc.DeleteInternalEthernetPort($item) | Out-Null
        }
    }
}

#----------------------------------------------------------------------------
# Set internal network to DHCP exist
#----------------------------------------------------------------------------
Function SetNetworkToDHCP($nic,$switchName)
{
    if ($nic -ne $null)
    {
        if($nic.dhcpEnabled -ne $true)
        {
            Write-Info.ps1 ("$switchName" + ": Set IPv4 address to DHCP and delete DNS servers")
            CMD /C netsh interface ipv4 set address $nic.interfaceindex dhcp 2>&1 | Write-Info.ps1
            CMD /C netsh interface ipv4 delete dnsservers $nic.interfaceindex all 2>&1 | Write-Info.ps1
        }
        
        $ips = $nic.IPAddress
        foreach($ip in $ips)
        {   
            $objIP = [IPAddress]$ip
            if($objIP.AddressFamily -eq "InternetworkV6" -and $ip.Substring(0,6) -ne "fe80::")
            {
                Write-Info.ps1 "$switchName : delete IPv6 address and DNS servers"
                CMD /C netsh interface ipv6 delete address $nic.interfaceindex $ip 2>&1 | Write-Info.ps1
                CMD /C netsh interface ipv6 delete dnsservers $nic.interfaceindex all 2>&1 | Write-Info.ps1
            }
        }        
    }
}

if ([double]$osbuildnum -eq [double]"6.1") 
{
    Write-Info.ps1 "Set all other internal network adapters to DHCP"
    $switches = Get-VMSwitch | where {$_.ElementName -notmatch "External"}
    Foreach ($sw in $switches)
    {	
        $switchName = $sw.name
        $items = Get-WMIObject Msvm_InternalEthernetPort -namespace "root\virtualization" | where {$_.ElementName -eq $switchName}
        foreach($item in $items)
        {
            $networkAdapter = Get-WmiObject Win32_NetworkAdapter | where {$_.GUID -eq $item.DeviceID}
			$nic = Get-WmiObject -Class Win32_NetworkAdapterConfiguration | where {$_.Index -eq $networkAdapter.DeviceID}
			SetNetworkToDHCP $nic $switchName
        }
    }
}

if ([double]$osbuildnum -ge [double]"6.2") 
{
    Write-Info.ps1 "Set all other internal network adapters to DHCP"
    $switches = Get-VMSwitch -SwitchType Internal
    foreach ($sw in $switches)
    {	
        $switchName = $sw.Name
		$netAdapters = Get-NetAdapter | where {$_.Name -match $switchName}
		foreach ($netAdapter in $netAdapters)
		{
			$nic = Get-WmiObject -Class Win32_NetworkAdapterConfiguration | where {$_.SettingId -eq $netAdapter.DeviceID}
			SetNetworkToDHCP $nic $switchName
		}
	}
}

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Stop-Transcript
exit 0
