##################################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################################

#--------------------------------------------------------------------------------------------------
# Script to Prepare a Windows VHD to upload to Azure
# https://docs.microsoft.com/en-us/azure/virtual-machines/windows/prepare-for-upload-vhd-image
#--------------------------------------------------------------------------------------------------

Write-Host ":Preparing OS started"

try
{
    # Ensure network profile as private
    $networkProfile = Get-NetConnectionProfile
    if($networkProfile.NetworkCategory -eq "Public") {
      Set-NetConnectionProfile -InterfaceIndex $networkProfile.InterfaceIndex -NetworkCategory Private
    }

    # install visual studio
    .\vs_enterprise.exe --add Microsoft.NetCore.Component.SDK --add Microsoft.VisualStudio.Component.Roslyn.Compiler --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 --add Microsoft.VisualStudio.Component.VC.CLI.Support --add Microsoft.VisualStudio.Component.VC.Redist.14.Latest --add Microsoft.VisualStudio.Component.VC.CoreIde --add Microsoft.VisualStudio.Component.Windows10SDK.19041 --add Microsoft.VisualStudio.Component.TestTools.WebLoadTest --passive --norestart
    
    ## Run Windows System File Checker utility before generalization of OS image

    sfc.exe /scannow

    # Remove any static persistent routes in the routing table:
    route.exe print
    # route.exe delete

    # Remove the WinHTTP proxy:
    netsh.exe winhttp reset proxy

    # Open DiskPart:

    # Set the disk SAN policy to Onlineall:
    # copy and paste the following commands in the DISKPART prompt

    # san policy=onlineall
    # exit

    diskpart.exe /s diskpart_script.txt

    # Set Coordinated Universal Time (UTC) time for Windows.

    Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Control\TimeZoneInformation -Name RealTimeIsUniversal -Value 1 -Type DWord -Force
    Set-Service -Name w32time -StartupType Automatic

    # Set the power profile to high performance:

    powercfg.exe /setactive SCHEME_MIN

    # Make sure the environmental variables TEMP and TMP are set to their default values: 

    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment' -Name TEMP -Value "%SystemRoot%\TEMP" -Type ExpandString -Force
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment' -Name TMP -Value "%SystemRoot%\TEMP" -Type ExpandString -Force

    # Check the Windows services

    Get-Service -Name BFE, Dhcp, Dnscache, IKEEXT, iphlpsvc, nsi, mpssvc, RemoteRegistry |
      Where-Object StartType -ne Automatic |
        Set-Service -StartupType Automatic

    Get-Service -Name Netlogon, Netman, TermService |
      Where-Object StartType -ne Manual |
        Set-Service -StartupType Manual

    ## Update remote desktop registry settings

    # Remote Desktop Protocol (RDP) is enabled:

    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server' -Name fDenyTSConnections -Value 0 -Type DWord -Force
    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services' -Name fDenyTSConnections -Value 0 -Type DWord -Force

    # The RDP port is set up correctly using the default port of 3389:

    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\Winstations\RDP-Tcp' -Name PortNumber -Value 3389 -Type DWord -Force

    # The listener is listening on every network interface:

    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\Winstations\RDP-Tcp' -Name LanAdapter -Value 0 -Type DWord -Force

    # Configure network-level authentication (NLA) mode for the RDP connections:

    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name UserAuthentication -Value 1 -Type DWord -Force
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name SecurityLayer -Value 1 -Type DWord -Force
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name fAllowSecProtocolNegotiation -Value 1 -Type DWord -Force

    # Set the keep-alive value:

    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services' -Name KeepAliveEnable -Value 1  -Type DWord -Force
    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services' -Name KeepAliveInterval -Value 1  -Type DWord -Force
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\Winstations\RDP-Tcp' -Name KeepAliveTimeout -Value 1 -Type DWord -Force

    # Set the reconnect options:

    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services' -Name fDisableAutoReconnect -Value 0 -Type DWord -Force
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\Winstations\RDP-Tcp' -Name fInheritReconnectSame -Value 1 -Type DWord -Force
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\Winstations\RDP-Tcp' -Name fReconnectSame -Value 0 -Type DWord -Force

    # Limit the number of concurrent connections:

    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\Winstations\RDP-Tcp' -Name MaxInstanceCount -Value 4294967295 -Type DWord -Force

    # Remove any self-signed certificates tied to the RDP listener:

    if ((Get-Item -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp').Property -contains 'SSLCertificateSHA1Hash')
    {
        Remove-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name SSLCertificateSHA1Hash -Force
    }

    ## Configure Windows Firewall rules

    # Turn on Windows Firewall on the three profiles

    Set-NetFirewallProfile -Profile Domain, Public, Private -Enabled True

    # Run the following example to allow WinRM through the three firewall profiles

    Enable-PSRemoting -Force
    Set-NetFirewallRule -DisplayName 'Windows Remote Management (HTTP-In)' -Enabled True

    # Enable the following firewall rules to allow the RDP traffic:

    Set-NetFirewallRule -DisplayGroup 'Remote Desktop' -Enabled True

    # Enable the rule for file and printer sharing so the VM can respond to

    Set-NetFirewallRule -DisplayName 'File and Printer Sharing (Echo Request - ICMPv4-In)' -Enabled True

    # Create a rule for the Azure platform network

    New-NetFirewallRule -DisplayName AzurePlatform -Direction Inbound -RemoteAddress 168.63.129.16 -Profile Any -Action Allow -EdgeTraversalPolicy Allow
    New-NetFirewallRule -DisplayName AzurePlatform -Direction Outbound -RemoteAddress 168.63.129.16 -Profile Any -Action Allow

    ## Verify the VM

    Echo Y | chkdsk.exe /f

    # Set the Boot Configuration Data (BCD) settings.

    cmd /c bcd.bat

    # The dump log can be helpful in troubleshooting Windows crash issues

    # Set up the guest OS to collect a kernel dump on an OS crash event
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\CrashControl' -Name CrashDumpEnabled -Type DWord -Force -Value 2
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\CrashControl' -Name DumpFile -Type ExpandString -Force -Value "%SystemRoot%\MEMORY.DMP"
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\CrashControl' -Name NMICrashDump -Type DWord -Force -Value 1

    # Set up the guest OS to collect user mode dumps on a service crash event
    $key = 'HKLM:\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps'
    if ((Test-Path -Path $key) -eq $false) {(New-Item -Path 'HKLM:\SOFTWARE\Microsoft\Windows\Windows Error Reporting' -Name LocalDumps)}
    New-ItemProperty -Path $key -Name DumpFolder -Type ExpandString -Force -Value 'C:\CrashDumps'
    New-ItemProperty -Path $key -Name CrashCount -Type DWord -Force -Value 10
    New-ItemProperty -Path $key -Name DumpType -Type DWord -Force -Value 2
    Set-Service -Name WerSvc -StartupType Manual

    # Verify that the Windows Management Instrumentation (WMI) repository is consistent:

    winmgmt.exe /verifyrepository

    # Make sure no other application is using port 3389.

    netstat.exe -anob

    if(((Get-Partition -DriveLetter C).Size/1GB) -lt 80)
    {
      Resize-Partition -DriveLetter C -Size 80GB
    }

    ## Restricting script execution after this session
    #Set-ExecutionPolicy -ExecutionPolicy Restricted

    #Create file to indicate that the process has completed successfully
    New-Item -Path . -Name "prepare_os.signal.txt" -ItemType "File" -Value "Prepare OS Completed"
    
    #End script when VS installation complete
    $vsProcess = Get-Process -Name vs_enterprise -ErrorAction SilentlyContinue
    if($null -ne ($vsProcess))
    {
      Wait-Process -Name vs_enterprise -ErrorAction SilentlyContinue
    }

    Write-Host ":Preparing OS completed"
}
catch
{
    Write-Host ":Could not prepare OS: "$_
}