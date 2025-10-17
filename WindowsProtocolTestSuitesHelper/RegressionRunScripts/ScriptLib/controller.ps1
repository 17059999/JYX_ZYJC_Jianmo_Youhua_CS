##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Controller.ps1
## Purpose:        Setup environment within a virtual machine, promote domain controller, install scripts.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2012, Windows Server 2012 R2, Windows Server 2016, and later.
##
## randon@microsoft.com
##
###########################################################################################

#------------------------------------------------------------------------------------------
# Parameters:
# Eject: Whether to eject and exit the script
# Phase: The phase that this script is entering
Param (
    [switch]$Eject,
    [int]$Phase = 1
)

#==========================================================================================
# Global Definitions
#==========================================================================================
#------------------------------------------------------------------------------------------
# Global Variables:
# [Script Information]
#   WorkFolderPath:     Full Path of the work folder where all scripts are located
#   InitialInvocation:  Initial Invocation of the script
#   InvocationFullPath: Full Path of this script file
#   InvocationName:     File Name of this script file
#   LogFileName:        File Name of the log file
#   XmlFileName:        File Name of the XML configuration file
#   DvdDrive:           The Drive letter of the DVD drive
#------------------------------------------------------------------------------------------
$WorkFolderPath          = "C:\Temp"
$InitialInvocation       = $MyInvocation
$InvocationFullPath      = $InitialInvocation.MyCommand.Definition
$InvocationName          = [System.IO.Path]::GetFileName($InvocationFullPath)
$LogFileName             = "$InvocationName.log"
$XmlFileName             = "setup.xml"
$HostNameFileName        = "name.txt"
$MajorVersion            = [system.environment]::OSversion.Version.Major
$DvdDrive                = Get-PSDrive -PSProvider FileSystem | foreach { New-Object System.IO.DriveInfo($_.name) | where { $_.drivetype -eq "CDRom" }} | Select-Object -First 1

#==========================================================================================
# Function Definition
#==========================================================================================

#------------------------------------------------------------------------------------------
# Write a piece of information to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteInfo {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    [string]$Message,
    [string]$ForegroundColor = "White",
    [string]$BackgroundColor = "DarkBlue")

    # WinBlue issue: Start-Transcript cannot write the log printed out by Write-Host, as a workaround, use Write-output instead
    # Write-Output does not support color
    if ([Double]$Script:HostOsBuildNumber -eq [Double]"6.3") {
        ((Get-Date).ToString() + ": $Message") | Out-Host
    }
    else {
        Write-Host ((Get-Date).ToString() + ": $Message") -ForegroundColor $ForegroundColor -BackgroundColor $BackgroundColor
    }
}

#------------------------------------------------------------------------------------------
# Write a piece of warning message to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteWarning {
    Param (
    [Parameter(ValueFromPipeline=$True)]
    [string]$Message,
    [switch]$Exit)

    Write-TestSuiteInfo -Message "[WARNING]: $Message" -ForegroundColor Yellow -BackgroundColor Black
    if ($Exit) {exit 1}
}

#------------------------------------------------------------------------------------------
# Write a piece of error message to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteError {
    Param (
    [Parameter(ValueFromPipeline=$True)]
    [string]$Message,
    [switch]$Exit)

    Write-TestSuiteInfo -Message "[ERROR]: $Message" -ForegroundColor Red -BackgroundColor Black
    if ($Exit) {exit 1}
}

#------------------------------------------------------------------------------------------
# Write a piece of success message to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteSuccess {
    Param (
    [Parameter(ValueFromPipeline=$True)]
    [string]$Message)

    Write-TestSuiteInfo -Message "[SUCCESS]: $Message" -ForegroundColor Green -BackgroundColor DarkBlue
}

#------------------------------------------------------------------------------------------
# Write a piece of step message to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteStep {
    Param (
    [Parameter(ValueFromPipeline=$True)]
    [string]$Message)

    Write-TestSuiteInfo -Message "[STEP]: $Message" -ForegroundColor Yellow -BackgroundColor DarkBlue
}

#------------------------------------------------------------------------------------------
# Sleeping for a particular amount of time to wait for an activity to be completed
#------------------------------------------------------------------------------------------
Function Wait-TestSuiteActivityComplete {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    [string]$ActivityName,
    [int]$TimeoutInSeconds = 0)

    for ([int]$Tick = 0; $Tick -le $TimeoutInSeconds; $Tick++) {
        Write-Progress -Activity "Wait for $ActivityName ..." -SecondsRemaining ($TimeoutInSeconds - $Tick) -PercentComplete (($Tick / $TimeoutInSeconds) * 100)
        if ($Tick -lt $TimeoutInSeconds) { Sleep 1 }
    }
    Write-Progress -Activity "Wait for $ActivityName ..." -Completed
}

#------------------------------------------------------------------------------------------
# Set server ISO state
# "insert" - CDROM is inserted
# "eject" - CDROM is ejected
# "completed" - the script controller.ps1 is completed
#------------------------------------------------------------------------------------------
Function Set-ServerISOState {
    Param(
    [string]$State
    )

    Write-TestSuiteInfo "Set guest exchange data under path HKLM:\SOFTWARE\Microsoft\Virtual Machine\Guest."
    Write-TestSuiteInfo "ISOState_$State = $State."

    $GuestExchangeDataPath = "HKLM:\SOFTWARE\Microsoft\Virtual Machine\Guest"
    Set-ItemProperty -Path $GuestExchangeDataPath -Name "ISOState_$State" -Value "$State" -Type String
}

#------------------------------------------------------------------------------------------
# Wait for ISO to be inserted to the DVD Drive
# Set the ISOState to "insert" after confirming that ISO is inserted
#------------------------------------------------------------------------------------------
Function Wait-DVDDriveInsertion {
    Param(
    [int]$TimeoutInSeconds = 180,
    [int]$SleepTimePerIteration = 5
    )

    Write-TestSuiteInfo "Wait for setup ISO to be inserted to the DVD Drive of the server."

    $IsInserted = $false
    $TimeLeft = $TimeoutInSeconds
    do {
        Write-TestSuiteStep "Get the DVD Drive status."
        if ($Script:DvdDrive.IsReady) {
            $IsInserted = $true
            Write-TestSuiteStep "Set the ISO state to - insert."
            Set-ServerISOState "insert"
            break
        }
        else {
            Write-TestSuiteInfo "DVD Drive not inserted yet."
        }
        Wait-TestSuiteActivityComplete -ActivityName "Insert DVD Drive" -TimeoutInSeconds $SleepTimePerIteration
    } while ($TimeLeft -= $SleepTimePerIteration)

    if (!$TimeLeft -and !$IsInserted) {
       Write-TestSuiteError "Unable to insert ISO within $TimeoutInSeconds seconds." -Exit
    }
}

#------------------------------------------------------------------------------------------
# Eject for ISO from the server itself when outside script can not eject ISO
#------------------------------------------------------------------------------------------
Function Eject-ServerDVDDrive {

    Write-TestSuiteInfo "Eject the DVD Drive from the server itself."

    $ShellApplication = New-Object -Com Shell.Application
    if ($Script:MajorVersion -eq 5) {
        # Eject Command for Windows 2003
        $ShellApplication.Namespace(17).ParseName($($Script:DvdDrive).Name).InvokeVerb('E&ject')
    }
    else {  
        # Eject Command for Windows 2008 and above
        $ShellApplication.Namespace(17).ParseName($($Script:DvdDrive).Name.Replace("\", "")).InvokeVerb("Eject")
    }
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($ShellApplication)
    Remove-Variable ShellApplication
}

#------------------------------------------------------------------------------------------
# Wait for ISO to be ejected from the DVD Drive
#------------------------------------------------------------------------------------------
Function Wait-DVDDriveEjection {
    Param(
    [int]$TimeoutInSeconds = 18000,
    [int]$SleepTimePerIteration = 5
    )
    
    Eject-ServerDVDDrive

    Write-TestSuiteInfo "Wait for ISO to be ejected from the DVD Drive of the server."

    $IsEjected = $false
    $TimeLeft = $TimeoutInSeconds
    do {
        Write-TestSuiteStep "Get the DVD Drive status."
        if (!($Script:DvdDrive.IsReady)) {
            $IsEjected = $true
            Write-TestSuiteStep "Set the ISO state to - eject."
            Set-ServerISOState "eject"
            break
        }
        else {
            Write-TestSuiteInfo "DVD Drive not ejected yet."
        }
        Wait-TestSuiteActivityComplete -ActivityName "Eject DVD Drive" -TimeoutInSeconds $SleepTimePerIteration
    } while ($TimeLeft -= $SleepTimePerIteration)

    if (!$TimeLeft -and !$IsEjected) {        
        Write-TestSuiteWarning "Unable to eject ISO within $TimeoutInSeconds seconds." -Exit
    }
}

#------------------------------------------------------------------------------------------
# Wait for Post Signal File to be generated on hard drive indicating Post Script has completed
#------------------------------------------------------------------------------------------
Function Wait-PostSignalFile {
    Param(
    [int]$TimeoutInSeconds = 18000,
    [int]$SleepTimePerIteration = 5
    )

    Write-TestSuiteInfo "Wait for Post Signal File to be generated on hard drive indicating Post Script has completed."

    $IsCompleted = $false
    $TimeLeft = $TimeoutInSeconds
    do {
        Write-TestSuiteStep "Check Post Signal File."
        if (Test-Path "$Script:WorkFolderPath\post.finished.signal") {
            $IsCompleted = $true
            Write-TestSuiteInfo "Post scripts have completed."
            break
        }
        else {
            Write-TestSuiteInfo "Post Scripts have not completed yet."
        }
        Wait-TestSuiteActivityComplete -ActivityName "Post scripts complete" -TimeoutInSeconds $SleepTimePerIteration
    } while ($TimeLeft -= $SleepTimePerIteration)

    if (!$TimeLeft -and !$IsCompleted) {
        Write-TestSuiteWarning "Post scripts unable to complete within $TimeoutInSeconds seconds." -Exit
    }
}

#------------------------------------------------------------------------------------------
# Copy files from DVD drive
#------------------------------------------------------------------------------------------
Function Copy-ServerSetupFiles {

    Write-TestSuiteInfo "Copy setup files from DVD drive to local."

    Write-TestSuiteStep "Copy files from DVD drive to the work folder."
    robocopy $Script:DvdDrive $Script:WorkFolderPath /E

    # Remove All files' ReadOnly attribute
    $targetPathAllFiles = "$Script:WorkFolderPath\*.*"
    attrib $targetPathAllFiles -s -r /s /d

    Write-TestSuiteStep "Start checking the controller.ps1 status by running Check-ControllerStatus.ps1."
    Invoke-Expression "$Script:WorkFolderPath\Check-ControllerStatus.ps1"
}

#------------------------------------------------------------------------------------------
# Format the input xml file and display it to the screen
#------------------------------------------------------------------------------------------
Function Format-TestSuiteXml {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    [xml]$Xml,
    [int]$Indent = 2)

    Process {
        $StringWriter = New-Object System.IO.StringWriter
        $XmlWriter = New-Object System.Xml.XmlTextWriter $StringWriter
        $XmlWriter.Formatting = “indented”
        $XmlWriter.Indentation = $Indent
        [xml]$Xml.WriteContentTo($XmlWriter)
        $XmlWriter.Flush()
        $StringWriter.Flush()

        # Output the result
        Write-Output $("`n" + $StringWriter.ToString())
    }
}

#------------------------------------------------------------------------------------------
# Get all configuration data for setup test suite server
# $Script:Setup from setup.xml
# $Script:Name from name.txt
# $Script:Server from setup.xml using name from name.txt
#------------------------------------------------------------------------------------------
Function Get-ServerSetupConfigurations {

    Write-TestSuiteInfo "Get test suite setup configurations."

    Write-TestSuiteStep "Get contents from $Script:XmlFileName."
    [xml]$Script:Setup = Get-Content "$Script:WorkFolderPath\$Script:XmlFileName"
    $Script:Setup | Format-TestSuiteXml -Indent 4

    Write-TestSuiteStep "Get server name from $Script:HostNameFileName."
    $Script:Name = Get-Content "$Script:WorkFolderPath\$Script:HostNameFileName"
    Write-TestSuiteInfo $Script:Name

    Write-TestSuiteStep "Get the configurations for $Script:Name from $Script:XmlFileName."
    $Script:Server = ($Script:Setup).lab.servers.vm | where {$_.name -eq $Script:Name} | Select-Object -First 1
}

#------------------------------------------------------------------------------------------
# Setup the IP address on the NICs
#------------------------------------------------------------------------------------------
Function Set-ServerIPAddresses {

    Write-TestSuiteInfo "Set server IP Addresses."

    # Get the Subnet and Gateway for our nic
    $DefaultGateway = ($Script:Setup).lab.network.vnet | where {$_.hostisgateway -eq "true"}

    # Windows 2003, we do not set ipv6
    if ($Script:MajorVersion -eq 5){
        # Set the IP of the NIC 
        Write-TestSuiteStep $("Setting IP address of the NIC to : " + ($Script:Server).ip )
        netsh interface ip set address local static ($Script:Server).ip $DefaultGateway.subnet $DefaultGateway.ip 1
        
        # Configure DNS server IP
        Write-TestSuiteStep $("Setting DNS Server: " + ($Script:Server).dns)
        netsh interface ip Set dns local static ($Script:Server).dns
    } 
    else {
        # This will set the network card settings for Windows 2008 and above, multinic scenario
        [array]$ServerVnet    = ($Script:Server).vnet
        [array]$ServerIp      = ($Script:Server).ip
        [array]$ServerGateway = ($Script:Server).gateway
        [array]$ServerDns     = ($Script:Server).dns 
        [array]$ServerIpv6    = ($Script:Server).ipv6
        $NicNumber = 0;
        $currVnet= $ServerVnet[0];
        $currIp = $ServerIp[0]

        # Win32_PnPSignedDriver class: http://msdn.microsoft.com/en-us/library/aa394354(v=vs.85).aspx
        # Physical device object (PDO). PDOs represent individual devices on a bus. Other drivers for the device attach on top of the PDO. 
        # It is always at the bottom of the device stack. Example: "\Device\00000002"
        #
        # Sort the device by PDO so it map to correct network adapter in Hyper-V manager
        $PnpSignedDrivers = Get-WMIObject Win32_PNPSignedDriver | where { $_.DeviceClass -eq "NET" -and $_.DeviceID -like "VMBUS*" } | Sort-Object PDO
        foreach($PnpDriver in $PnpSignedDrivers) {
            $Adapter = Get-WMIObject Win32_NetworkAdapter | Where-Object { $_.PNPDeviceID -eq $PnpDriver.DeviceID }
            $Nic = Get-WmiObject Win32_NetworkAdapterConfiguration | Where-Object { $_.Index -eq $Adapter.Index }

            $NicNumber++;
            if($ServerVnet.Count -gt 1){
                $currVnet= $ServerVnet[$NicNumber-1];
                $currIp = $ServerIp[$NicNumber-1];
            }   
            $Network = ($Script:Setup).lab.network.vnet | where {$_.name -eq $currVnet}
            $DefaultSubnet = $Network.subnet
                
            # Rename NIC 1, add suffix to avoid name duplicate
            Write-TestSuiteStep ("(NIC $NicNumber) Setting Adapter Name : " +  $currVnet + "-$NicNumber" )
            $Adapter.NetConnectionID =  $currVnet + "-$NicNumber"
            $Adapter.put() # To make the property update of NetConnectionID take effect immediately
                
            # Set IPv4 on NIC
            if ($currIp -eq $null) {
                Write-TestSuiteStep "Disable IPv4 address"
                cmd /c netsh interface ipv4 uninstall 2>&1 | Write-TestSuiteInfo
            }
            elseif ($currIp -eq "0.0.0.0") {
                Write-TestSuiteStep "(NIC $NicNumber)Setting IPv4 address:DHCP"
                netsh interface ipv4 set address $nic.interfaceindex dhcp
                Write-TestSuiteStep "(NIC $NicNumber)Skip Setting DNS IPv4 Server, assume this is DHCP."
            }
            else {
                Write-TestSuiteStep ("(NIC $NicNumber)Setting IPv4 address : " + $ServerIp[$NicNumber-1])
                if (!$ServerGateway -or ($ServerGateway[$NicNumber-1] -eq "Empty")) {
                    netsh interface ipv4 set address $Nic.interfaceindex static $ServerIp[$NicNumber-1] $DefaultSubnet
                }
                else {
                    netsh interface ipv4 set address $Nic.interfaceindex static $ServerIp[$NicNumber-1] $DefaultSubnet $ServerGateway[$NicNumber-1]
                }
                if (!$ServerDns -or ($ServerDns[$NicNumber-1] -ne "Empty")) {
                    $DnsServers = $ServerDns[$NicNumber-1] -split ";"
                    Set-DnsClientServerAddress -InterfaceIndex $Nic.interfaceindex -ServerAddresses $DnsServers
                }
            }
            
            # Set IPv6 on NIC
            if ($ServerIpv6 -ne $null) {
                Write-TestSuiteStep ("(NIC 1)Setting Public IPv6 address :" + $ServerIpv6[$NicNumber-1])
                netsh interface ipv6 set address $Nic.interfaceindex $ServerIpv6[$NicNumber-1]
                netsh interface ipv6 set dnsservers $Nic.interfaceindex static $ServerDns[$NicNumber-1] primary
            }                       
        }
    }
}

#------------------------------------------------------------------------------------------
# Set the administrator password and the server name
#------------------------------------------------------------------------------------------
Function Set-ServerName {
    
    Write-TestSuiteInfo ("Setting the servername: " + ($Script:Server).name)

    # Update administrator password
    $CmdLine = "net user administrator " + ($Script:Setup).lab.core.password
    Write-TestSuiteInfo $CmdLine
    Invoke-Expression $CmdLine

    Write-TestSuiteInfo ("Rename computer " + $env:COMPUTERNAME + " to new name " + $(($Script:Server).name))
    $ComputerName = Get-WmiObject -Class Win32_ComputerSystem  
    $ComputerName.rename(($Script:Server).name)
}

#------------------------------------------------------------------------------------------
# Set the SourcePath and ServicePackSourcePath
# Windows 2003
#------------------------------------------------------------------------------------------
Function Set-SourceAndServicePackSourcePath {
    if ($Script:MajorVersion -eq 5) {
        Set-ItemProperty -Path HKLM:\Software\Microsoft\Windows\CurrentVersion\Setup -Name SourcePath -Value "C:\CDImage\"
        Set-ItemProperty -Path HKLM:\Software\Microsoft\Windows\CurrentVersion\Setup -Name ServicePackSourcePath -Value "C:\CDImage\"
    }
}

#------------------------------------------------------------------------------------------
# Install UI for Threshold build
# Windows 10
#------------------------------------------------------------------------------------------
Function Install-ServerUserInterface {
    if ($Script:MajorVersion -eq 10) {
        Write-TestSuiteInfo "Install User-Interfaces-Infra for Threshold build"
        # Install-WindowsFeature -Name User-Interfaces-Infra -IncludeAllSubFeature -IncludeManagementTools
    }
}

#------------------------------------------------------------------------------------------
# Set next phase to enter after rebooting the server
#------------------------------------------------------------------------------------------
Function Set-NextPhase {
    Param(
    [int]$PhaseNumber
    )

    # If PhaseNumber == 0, remove registry key for next run
    if ($PhaseNumber -eq 0) {
        Remove-ItemProperty -Path HKLM:\Software\Microsoft\Windows\CurrentVersion\Run -Name Install
    }
    # Else, set registry key for next run phase
    else {
        $Command = "$PSHOME\PowerShell.exe -NoExit -Command `"$Script:WorkFolderPath\$Script:InvocationName -Phase $PhaseNumber`""
        Set-ItemProperty -Path HKLM:\Software\Microsoft\Windows\CurrentVersion\Run -Name Install -Value $Command
    }
}

#------------------------------------------------------------------------------------------
# Create Registry keys for automatic logon
#------------------------------------------------------------------------------------------
Function Set-AutoLogon {
    Param (
    $Username,
    $Domain,
    $Password,
    $Count)
    
    Write-TestSuiteInfo "Set AutoLogin for $Domain\$Username, with AutologonCount $Count"

    # Setup Autologon on
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name AutoAdminLogon -Value 1
    
    # Set Domain name
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name DefaultDomainName -Value $Domain
    
    # Set User Name
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name DefaultUserName -Value $Username
    
    # Set Password
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name DefaultPassword -Value $Password
    
    # Set Logon Count
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name AutologonCount -Value $Count
}

#------------------------------------------------------------------------------------------
# Resize to the partition to the maximum size
#------------------------------------------------------------------------------------------
Function Maximize-ServerPartition {
    try {
        Write-TestSuiteInfo "Resize to the partition to the maximum size."
        $Size = (Get-PartitionSupportedSize –DiskNumber 0 –PartitionNumber 1)
        Resize-Partition -DiskNumber 0 –PartitionNumber 1 -Size $Size.SizeMax -ErrorAction SilentlyContinue
    }
    catch {
        Write-TestSuiteError "Failed to resize."
    }   
}

#------------------------------------------------------------------------------------------
# Reboot the computer
#------------------------------------------------------------------------------------------
Function Reboot-Computer {
    Param ($Time)
    
    Write-TestSuiteInfo "`n"
    Write-TestSuiteInfo "Reboot started"
    Write-TestSuiteInfo "To abort reboot type `"Shutdown /a`" "

    Write-TestSuiteInfo "Call shutdown /r /f /t $Time /d P:2:4"   
    shutdown /r /f /t $Time /d P:2:4

    # By using delayed shutdown, below script should be executed as well.
    Write-TestSuiteInfo "Call Stop-Transcript to finish logging."
    Stop-Transcript

    Exit
}

#------------------------------------------------------------------------------------------
# Prepare the server environment for setup
# 1. Create a work folder if it does not exist
# 2. Start logging
# 3. Check hardware
#------------------------------------------------------------------------------------------
Function Prepare-ServerSetup {
    
    Write-TestSuiteInfo "Prepare the server for setup."

    Write-TestSuiteStep "Create work folder if it does not exist."
    if (!(Test-Path $Script:WorkFolderPath)) {
        CMD /C MKDIR $Script:WorkFolderPath
    }

    Write-TestSuiteStep "Start logging."
    Start-Transcript -Path "$Script:WorkFolderPath\$Script:LogFileName" -Append -Force

    Write-TestSuiteStep "Check hardware."
    if ([System.String]::IsNullOrEmpty($Script:DvdDrive)) {
        Write-TestSuiteError "No DVD Drive found in this server." -Exit
    }
}

#------------------------------------------------------------------------------------------
# Cleanup the server environment for setup
# 1. Eject CD/DVD Drive and letting the external script know that we are done setup.
# 2. Remove registry key for next phase after rebooting.
# 3. Stop logging and exit script.
#------------------------------------------------------------------------------------------
Function Cleanup-ServerSetup {

    Write-TestSuiteInfo "Cleanup the server environment for setup."

    Write-TestSuiteStep "Eject the CD/DVD Drive if not ejected."
    Eject-ServerDVDDrive
    
    Write-TestSuiteInfo "Remove the Registry key so we don't launch again."
    Set-NextPhase -PhaseNumber 0
    
    Write-TestSuiteStep "Stop logging."
    Stop-Transcript

    Write-TestSuiteStep "Exit the script."
    Exit
}

#------------------------------------------------------------------------------------------
# Phase 1
# 1. Copy files from DVD.
# 2. Get setup configurations from the setup.xml file.
# 3. Setup IP Addresses and Server Name, and auto logon information.
# 4. Set Source Path and ServicePackSourcePath or install UI features regarding to the OS Version.
# 5. Maximize partition.
# 6. Set next phase after rebooting.
# 7. Reboot the computer.
#------------------------------------------------------------------------------------------
Function Enter-Phase1 {

    Write-TestSuiteInfo "Entering setup phase 1."

    Write-TestSuiteStep "Wait for Test Suite ISO to be inserted."
    Wait-DVDDriveInsertion

    Write-TestSuiteStep "Copy files from DVD."
    Copy-ServerSetupFiles

    Write-TestSuiteStep "Get setup configurations from the setup.xml file."
    Get-ServerSetupConfigurations
    Add-GenericCredentials

    Write-TestSuiteStep "Setup IP Addresses and Server Name, and Auto Logon information."
    Set-ServerIPAddresses
    if ($env:COMPUTERNAME -ne ($Script:Server).name) {
        Set-ServerName
        Set-AutoLogon -Username ($Script:Setup).lab.core.username -Domain ($Script:Server).name -Password ($Script:Setup).lab.core.password -Count 999
    }

    Write-TestSuiteStep "Set Source Path and ServicePackSourcePath or install UI features regarding to the OS Version."
    Set-SourceAndServicePackSourcePath
    Install-ServerUserInterface

    Write-TestSuiteStep "Maximize partition."
    Maximize-ServerPartition

    Write-TestSuiteStep "Set next phase the script will enter after rebooting."
    Set-NextPhase -PhaseNumber 2

    Write-TestSuiteInfo "Wait for Test Suite ISO to be ejected."
    Wait-DVDDriveEjection

    Write-TestSuiteStep "Reboot the computer."
    Reboot-Computer 10
}

#------------------------------------------------------------------------------------------
# Phase 2
# 1. Get setup configurations from the setup.xml file.
# 2. Check if setup Server Name succeeded or not, if not, reset Server Name and Auto Logon. And then reboot to enter phase 2 again.
# 3. Install Visual Studio and Application
# 4. Set next phase after rebooting.
# 5. Reboot the computer.
#------------------------------------------------------------------------------------------
Function Enter-Phase2 {
    
    Write-TestSuiteInfo "Entering setup phase 2."
    
    Write-TestSuiteStep "Get setup configurations from the setup.xml file."
    Get-ServerSetupConfigurations
    
    Write-TestSuiteStep "Check if setup Server Name succeeded or not, if not, reset Server Name and Auto Logon. And then reboot to enter phase 2 again."
    if ($env:COMPUTERNAME -ne $server.name){
        Set-ServerName
        Set-AutoLogon -Username ($Script:Setup).lab.core.username -Domain ($Script:Server).name -Password ($Script:Setup).lab.core.password -Count 3
        Set-NextPhase -PhaseNumber 2
        Reboot-Computer 10
        exit
    }

    Write-TestSuiteStep "Install Visual Studio and Application if VM is not BaseVM."
    if (($Script:Server).name -notmatch "BaseVM") {

        if (![string]::IsNullOrEmpty(($Script:Server).postiso)) {
            Write-TestSuiteStep "Insert Visual Studio install ISO."
            Write-TestSuiteInfo "Wait for Visual Studio ISO to be inserted."
            Wait-DVDDriveInsertion

            if ((($Script:Server).postiso -eq "VS2010.iso") -and (Test-Path $($Script:DvdDrive.Name + "VS_SETUP.MSI"))) {
                Write-TestSuiteInfo "Install VS2010 ahead of time to improve the speed of VM configuration."
                Invoke-Expression "$Script:WorkFolderPath\Install_vs2010.ps1"
            }
            if ((($Script:Server).postiso -eq "VS2012.iso") -and (Test-Path $($Script:DvdDrive.Name + "vs_ultimate.exe"))) {
                Write-TestSuiteInfo "Install VS2012 ahead of time to improve the speed of VM configuration."
                Invoke-Expression "$Script:WorkFolderPath\Install_vs2012.ps1"
            }

            Write-TestSuiteInfo "Wait for Visual Studio ISO to be ejected."
            Wait-DVDDriveEjection
        }

        if (![string]::IsNullOrEmpty(($Script:Server).installiso)) {
            Write-TestSuiteStep "Install Application install ISO."
            Write-TestSuiteInfo "Wait for Application ISO to be inserted."
            Wait-DVDDriveInsertion

            # Install some application

            Write-TestSuiteInfo "Wait for Application ISO to be ejected."
            Wait-DVDDriveEjection
        }

        if (![string]::IsNullOrEmpty(($Script:Server).installscript)) {
            Write-TestSuiteInfo "Install Application ahead of time to improve the speed of VM configuration."
            Invoke-Expression "$Script:WorkFolderPath\Install.ps1"
        }
    }

    Write-TestSuiteStep "Set next phase after rebooting."
    Set-NextPhase -PhaseNumber 3

    Write-TestSuiteStep "Reboot the computer."
    Reboot-Computer 10
}

#------------------------------------------------------------------------------------------
# Phase 3
# 1. Get setup configurations from the setup.xml file.
# 2. Install Features. And then reboot to enter phase 3 again to install post scripts.
# 3. If Visual Studio or Application were installed in Phase 2, switch to Test Suite ISO.
# 4. If outside script does not need to wait for post script to complete, eject the Test Suite ISO directly.
# 5. Install Post scripts.
# 6. Set next phase after rebooting.
# 7. Reboot the computer.
#------------------------------------------------------------------------------------------
Function Enter-Phase3 {

    Write-TestSuiteInfo "Entering setup phase 3."
    
    Write-TestSuiteStep "Get setup configurations from the setup.xml file."
    Get-ServerSetupConfigurations
    
    Write-TestSuiteStep "Install Features. And then reboot to enter phase 3 again to install post scripts."
    if (![string]::IsNullOrEmpty(($Script:Server).installfeaturescript) -and !(Test-Path "$Script:WorkFolderPath\installfeaturescript.signal")) {
    
        Write-TestSuiteInfo "Write information to signal file."
        ECHO "installfeaturescript started" > $Script:WorkFolderPath\installfeaturescript.signal

        Write-TestSuiteInfo "Install Features by running InstallFeatureScript.ps1."
        Invoke-Expression "$Script:WorkFolderPath\InstallFeatureScript.ps1"

        Write-TestSuiteStep "Set next phase after rebooting."
        Set-NextPhase -PhaseNumber 3

        Write-TestSuiteStep "Reboot the computer."
        Reboot-Computer 10
    }

    # move this before executing the post scripts, because post script can cause rebooting.
    Write-TestSuiteStep "Set next phase after rebooting."
    Set-NextPhase -PhaseNumber 4
    
    Write-TestSuiteStep "Install Post script if exists."
    if (![string]::IsNullOrEmpty(($Script:Server).postscript)) {
        Write-TestSuiteStep "Install Post script."
        Set-ServerISOState -State "installpostscriptready"

        # Write-TestSuiteInfo "Wait for Test Suite ISO to be inserted again to trigger post script installation."
        # Wait-DVDDriveInsertion
        Invoke-Expression "$Script:WorkFolderPath\Post.ps1"
    }

    Write-TestSuiteStep "Reboot the computer."
    Reboot-Computer 10
}

#------------------------------------------------------------------------------------------
# Phase 4
# 1. Get setup configurations from the setup.xml file.
# 2. Decide whether to eject Test Suite ISO immediately or wait for outside script to control eject by sequence.
# 3. Clean up the server.
#------------------------------------------------------------------------------------------
Function Enter-Phase4 {

    Write-TestSuiteInfo "Entering setup phase 4."
    
    Write-TestSuiteStep "Get setup configurations from the setup.xml file."
    Get-ServerSetupConfigurations

    if ([string]::IsNullOrEmpty(($Script:Server).skipwaitingforpostscript) -or (($Script:Server).skipwaitingforpostscript -eq "true")) {
        Write-TestSuiteStep "Outside script does not want to wait for post script to complete, set ISOState to completed immediately."
    }
    else {        
        Wait-PostSignalFile
    }
    Set-ServerISOState -State "completed"

    Write-TestSuiteInfo "Install Finished"
    
    Cleanup-ServerSetup
}

#==========================================================================================
# Main Script Body
#==========================================================================================

#==========================================================================================
# Create work folder if it does not exist and start logging
#==========================================================================================
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "           Create Work Folder and Start Logging             "
Write-TestSuiteInfo "============================================================"
Prepare-ServerSetup

#==========================================================================================
# If $Eject is true, clean up the server environment and exit script
#==========================================================================================
if ($Eject -eq $true) { Cleanup-ServerSetup }

#==========================================================================================
# Switch to control the phases of all the installation procedures
#==========================================================================================
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                    Entering Phase $Phase                   "
Write-TestSuiteInfo "============================================================"
switch ($Phase) {
    1 { Enter-Phase1 } # Setup ENV
    2 { Enter-Phase2 } # Install Visual Studio and Applicatioin
    3 { Enter-Phase3 } # Execute Post Script
    4 { Enter-Phase4 } # Clean up ENV
}
