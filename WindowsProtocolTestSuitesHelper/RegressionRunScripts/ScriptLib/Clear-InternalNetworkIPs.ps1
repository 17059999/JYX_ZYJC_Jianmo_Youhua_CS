#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Clear-InternalNetworkIPs.ps1
## Purpose:        Stop VMs and clear internal network IPs
## Requirements:   Windows Powershell 2.0, 3.0
## Supported OS:   Windows Server 8, Windows Server 2012
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
[string]$workingDir = "D:\WinteropProtocolTesting",
[string]$protocolName  = "FileSharing", # e.g. FileSharing, MS-SMB2
[string]$testLogDir  = "$workingDir\TestResults\$protocolName",
[string]$VMConfigFileName  = ("$workingDir\$protocolName\VSTORMLITEFiles\XML\$protocolName" + ".xml")
)

#----------------------------------------------------------------------------
# Check parameters
#----------------------------------------------------------------------------
if($workingDir -eq $null -or $workingDir.Trim() -eq "")
{
    Write-Host "workingDir Could not be null or empty." -ForegroundColor Red
    return 1;
}
if($protocolName -eq $null -or $protocolName.Trim() -eq "")
{
    Write-Host "protocolName Could not be null or empty." -ForegroundColor Red
    return 1;
}
if($testLogDir -eq $null -or $testLogDir.Trim() -eq "")
{
    Write-Host "testLogDir Could not be null or empty." -ForegroundColor Red
    return 1;
}
if($VMConfigFileName -eq $null -or $VMConfigFileName.Trim() -eq "")
{
    Write-Host "VMConfigFileName Could not be null or empty." -ForegroundColor Red
    return 1;
}

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction SilentlyContinue | Out-Null
if(!(Test-Path $testLogDir))
{
    md $testLogDir
}
Start-Transcript -Path "$testLogDir\Clear-InternalNetworkIPs.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Determine current OS
#----------------------------------------------------------------------------
$osversion="Unsupported"
$osbuildnum= "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
if ($osbuildnum -eq "6.1") {$osversion="Windows Server 2008"}
if ($osbuildnum -ge "6.2") {$osversion="Windows Server 2012"}
if ($osversion -eq "Unsupported") 
{
   echo "Unsupported OS, will exit immediately"
   exit 1
} 
else 
{
    echo ("Supported Host OS: "+$osversion)
}

#----------------------------------------------------------------------------
# Import HyperV module
#----------------------------------------------------------------------------
if ($osversion -like "Windows Server 2008*")  
    {
	    # Verify we have the hyperv module to import
	    if (Get-Module -ListAvailable hyperv)
        {
		    echo "Found HyperV v1.0 Powershell module"
	    } 
        else 
        {
		    Robocopy $WorkingDir\hyperv C:\Windows\System32\WindowsPowerShell\v1.0\Modules\hyperv /mir 
	    }

	    # Import the Hyperv Management Module
	    if ( !(Get-Module hyperv) ) 
        {
		    Import-Module Hyperv
		    echo ("Importing HyperV v1.0 Powershell module")
	    }
    }

     if ($osversion -like "Windows Server 2012") 
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
		    echo "Importing HyperV v3.0 Powershell module"
	    }
	    else 
        {
		    echo "HyperV v3.0 Powershell module is missing, please check OS Installation"
	    }
    }

#----------------------------------------------------------------------------
# Get VM config
#----------------------------------------------------------------------------
Write-Host "Get VM config from $VMConfigFileName" -foregroundcolor Yellow
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
# Stop VMs according to VM config
#----------------------------------------------------------------------------
Write-Host "Remove VMs according to VM config ..." -foregroundcolor Yellow
$vms | Foreach {	
    $vmName = $_.hypervname # hypervname is a new item in latest xml file
	if($vmName -eq $null) {$vmName = $_.name}
	
    if ($osversion -like "Windows Server 2008*") 
    {
		try { get-vm $vmName -Running | Invoke-VMShutdown -force }
	    catch { get-vm $vmName -Running | stop-vm -force -Wait }
    }
    
     if ($osversion -like "Windows Server 2012") 
     {
	 	try { stop-vm $vmName -force }
        catch { stop-vm $vmName –TurnOff -force -ErrorAction SilentlyContinue}
     }
}

#----------------------------------------------------------------------------
# Clear static IPs
#----------------------------------------------------------------------------
Function SetNetworkToDHCP($nic,$switchName)
{
    if ($nic -ne $null)
    {
        if($nic.dhcpEnabled -ne $true)
        {
            Write-Host ("$switchName" + ": Set IPv4 address to DHCP and delete DNS servers")
            CMD /C netsh interface ipv4 set address $nic.interfaceindex dhcp 2>&1 | Write-Host
            CMD /C netsh interface ipv4 delete dnsservers $nic.interfaceindex all 2>&1 | Write-Host
        }
        
        $ips = $nic.IPAddress
        foreach($ip in $ips)
        {   
            $objIP = [IPAddress]$ip
            if($objIP.AddressFamily -eq "InternetworkV6" -and $ip.Substring(0,6) -ne "fe80::")
            {
                Write-Host ("$switchName" + ": delete IPv6 address and DNS servers")
                CMD /C netsh interface ipv6 delete address $nic.interfaceindex $ip 2>&1 | Write-Host
                CMD /C netsh interface ipv6 delete dnsservers $nic.interfaceindex all 2>&1 | Write-Host
            }
        }        
    }
}

Write-Host "Clear static IPs from internal network switches ..." -foregroundcolor Yellow
if ($osversion -like "Windows Server 2008*") 
{
    Write-Host "Set VM internal network adapters to DHCP."
    foreach ($vnet in $vnets) 
    {	
        $switchName = $vnet.name
        $items= Get-WMIObject Msvm_InternalEthernetPort -namespace "root\virtualization" | where {$_.ElementName -eq $switchName}
        foreach ($item in $items)
        {
            $networkAdapter = Get-WmiObject Win32_NetworkAdapter | where {$_.GUID -eq $item.DeviceID}
			$nic = Get-WmiObject -Class Win32_NetworkAdapterConfiguration | where {$_.Index -eq $networkAdapter.DeviceID}
			SetNetworkToDHCP $nic $switchName
        }
    }

    Write-Host "Set all other internal network adapters to DHCP"
    $switches = Get-VMSwitch | where {$_.ElementName -notmatch "External"}
    foreach ($sw in $switches)
    {	
        $switchName = $sw.name
        $items= Get-WMIObject Msvm_InternalEthernetPort -namespace "root\virtualization" | where {$_.ElementName -eq $switchName}
        foreach ($item in $items)
        {
            $networkAdapter = Get-WmiObject Win32_NetworkAdapter | where {$_.GUID -eq $item.DeviceID}
			$nic = Get-WmiObject -Class Win32_NetworkAdapterConfiguration | where {$_.Index -eq $networkAdapter.DeviceID}
			SetNetworkToDHCP $nic $switchName
        }
    }
}

if ($osversion -like "Windows Server 2012") 
{
    Write-Host "Set VM internal network adapters to DHCP."
    foreach ($vnet in $vnets)
    {
        $switchName = $vnet.name
		$netAdapters = Get-NetAdapter | where {$_.Name -match $switchName}
		foreach($netAdapter in $netAdapters)
		{
			$nic = Get-WmiObject -Class Win32_NetworkAdapterConfiguration | where {$_.SettingId -eq $netAdapter.DeviceID}
            SetNetworkToDHCP $nic $switchName
		}
	}

    Write-Host "Set all other internal network adapters to DHCP"
    $switches = Get-VMSwitch -SwitchType Internal
    foreach ($sw in $switches) {	
        $switchName = $sw.Name
		$netAdapters = Get-NetAdapter | where {$_.Name -match $switchName}
		foreach($netAdapter in $netAdapters)
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
