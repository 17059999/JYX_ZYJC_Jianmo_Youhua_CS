###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Setup-TestSuiteEnvironment.ps1
## Purpose:        Setup environment for a specified Test Suite.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2012, Windows Server 2012 R2, Windows Server 2016, and later.
##
###########################################################################################

<#

.SYNOPSIS

Setup the test suite environment on a VM host by creating and configuring all the virtual machines for this test suite.

.DESCRIPTION

The Setup-TestSuiteEnvironment function reads configuration information from the XML file specified by the EnvironmentName parameter, creates VMs using the VHDs specified by the Server/ClientDiskName parameters, and orchestrate the configurations of all the VMs.

.EXAMPLE

Setup test suite environment.

.\Setup-TestSuiteEnvironment.ps1 -TestSuiteName "FileServer_VSO" -EnvironmentName "FileServer.xml" -ServerDiskName "14300.1000.amd64fre.rs1_release_svc.160324-1723_server_serverdatacenter_en-us.vhd" -ServerAnswerFile "14300_ServerDataCenter.xml" -ClientDiskName "14393.0.amd64fre.rs1_release.160715-1616_client_enterprise_en-us_vl.vhd" -ClientAnswerFile "14393_Enterprise.xml" -DynamicDisk $true

.NOTES

You need to execute this script by administrative privilege, the administrative privilege is to ensure you could properly use the Hyper-V Manager.

#>

Param
(
    # The name of the Test Suite, only used to fetch XML configuration file and specify log and vm folder
    [string]$TestSuiteName = "MS-AZOD",
    # The name of the XML file, indicating which environment you want to configure
    # If multiple environments of the same test suite are to be deployed, specify <$EnvironmentName> as "ADFamily1.xml", "ADFamily2.xml", ...
    [string]$EnvironmentName = "MS-AZOD.xml",
    # Use dynamic disk or not
    [switch]$DynamicDisk = $true
)

Import-Module .\Common\LocalLinuxFunctionLib.psm1

#==========================================================================================
# Global Definitions
#==========================================================================================

#------------------------------------------------------------------------------------------
# Global Variables:
# [Script Information]
#   InitialInvocation:  Initial Invocation of the script
#   InvocationFullPath: Full Path of this script file
#   InvocationName:     File Name of this script file 
#   InvocationPath:     File Path of this script file
#   LogFileName:        File Name of the log file
#   LogFilePath:        File Path of the log file
# [Host Information]
#   HostOsBuildNumber:  Build Number for the operating system of vm host
# [Lab Information]
#   XmlLibPath:         File Path of the prepared XML configuration file library
#   VhdLibPath:         File Path of the virtual hard disks, the vhd names are presented as input parameters: ServerDiskName and ClientDiskName
#   AnswerFileLibPath   File Path of the answer files, the file names are presented as input parameters: ServerAnswerFile and ClientAnswerFile
#   InstallLibPath      File Path of all the install scripts
#   PostScriptLibPath   File Path of the post scripts
#   MediaScriptLibPath  File Path of the media files
#   ToolLibPath         File Path of all the tools
#   VsLicensePath       File Path of visual studio license file
# [Test Suite Information]
#   XmlFileFullPath:    Full Path of the XML configuration file
#   XmlFileName:        File Name of the XML configuration file
#   XmlFilePath:        File Path of the XML configuration file
#   PostScriptFilePath: File Path of the Post Scripts file
#   DeployFilePath:     File Path of the Deploy file
#   DataFilePath:       File Path of the Data file
#   ScriptsFilePath:    File Path of the Scripts file
#   VmDirPath:          The directory to store all the virtual machines for this test suite
#   IsoDirPath:         The directory to store all the iso files for this test suite
#------------------------------------------------------------------------------------------
$InitialInvocation = $MyInvocation
$InvocationFullPath = $InitialInvocation.MyCommand.Definition
$InvocationName = [System.IO.Path]::GetFileName($InvocationFullPath)
$InvocationPath = Split-Path -Parent $InvocationFullPath
$LogFileName = "$InvocationName.log"
$LogFilePath = "$InvocationPath\..\TestResults\$TestSuiteName"
$HostOsBuildNumber = "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
$XmlLibPath = "$InvocationPath\XML"
$VhdLibPath = "$InvocationPath\..\VHD"
$AnswerFileLibPath = "$InvocationPath\..\VSTORMLITE\AnswerFile"
$InstallLibPath = "$InvocationPath\..\VSTORMLITE\Install"
$PostScriptLibPath = "$InvocationPath\..\VSTORMLITE\PostScript"
$MediaLibPath = "$InvocationPath\..\Media"
$ScriptLibPath = "$InvocationPath\..\ScriptLib"
$ToolLibPath = "$InvocationPath\..\Tools"
$VsLicensePath = "$InvocationPath\..\VsLicense"
$XmlFileFullPath = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML\$EnvironmentName"
$XmlFileName = $EnvironmentName
$XmlFilePath = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML"
$PostScriptFilePath = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\PostScript"
$DeployFilePath = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\Deploy"
$DataFilePath = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\Data"
$ScriptsFilePath = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\Scripts"
$ToolsFilePath = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\Tools"
$VmDirPath = "$InvocationPath\..\VM\$TestSuiteName\Default"
$IsoDirPath = "$InvocationPath\..\VM\$TestSuiteName\Default\ISO"

#==========================================================================================
# Function Definition
#==========================================================================================

#------------------------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#------------------------------------------------------------------------------------------
Function Start-TestSuiteLog {

    # Stop the previous transcript
    try {
        Stop-Transcript -ErrorAction SilentlyContinue
    }
    catch [System.InvalidOperationException] {}

    # Create log directory if not exist
    if (!(Test-Path -Path $Script:LogFilePath)) {
        mkdir $Script:LogFilePath
    }

    # Start new transcript
    Start-Transcript -Path "$Script:LogFilePath\$Script:InvocationName.log" -Append -Force
}

#------------------------------------------------------------------------------------------
# Write a piece of information to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteInfo {
    Param(
        [Parameter(ValueFromPipeline = $True)]
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
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [switch]$Exit)

    Write-TestSuiteInfo -Message "[WARNING]: $Message" -ForegroundColor Yellow -BackgroundColor Black
    if ($Exit) { exit 1 }
}

#------------------------------------------------------------------------------------------
# Write a piece of error message to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteError {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [switch]$Exit)

    Write-TestSuiteInfo -Message "[ERROR]: $Message" -ForegroundColor Red -BackgroundColor Black
    if ($Exit) { exit 1 }
}

#------------------------------------------------------------------------------------------
# Write a piece of success message to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteSuccess {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message)

    Write-TestSuiteInfo -Message "[SUCCESS]: $Message" -ForegroundColor Green -BackgroundColor DarkBlue
}

#------------------------------------------------------------------------------------------
# Write a piece of step message to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteStep {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message)

    Write-TestSuiteInfo -Message "[STEP]: $Message" -ForegroundColor Yellow -BackgroundColor DarkBlue
}

#------------------------------------------------------------------------------------------
# Sleeping for a particular amount of time to wait for an activity to be completed
#------------------------------------------------------------------------------------------
Function Wait-TestSuiteActivityComplete {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$ActivityName,
        [int]$TimeoutInSeconds = 0)

    for ([int]$Tick = 0; $Tick -le $TimeoutInSeconds; $Tick++) {
        Write-Progress -Activity "Wait for $ActivityName ..." -SecondsRemaining ($TimeoutInSeconds - $Tick) -PercentComplete (($Tick / $TimeoutInSeconds) * 100)
        if ($Tick -lt $TimeoutInSeconds) { Start-Sleep 1 }
    }
    Write-Progress -Activity "Wait for $ActivityName ..." -Completed
}

#------------------------------------------------------------------------------------------
# Sync the source file to destination file
#------------------------------------------------------------------------------------------
Function Sync-File {
    Param (
        [string]$Source,
        [string]$Destination,
        [switch]$ExitOnError)

    if (!(Test-Path $Source)) {
        Write-TestSuiteError "Folder $Source does not exist."
        if ($ExitOnError) { Exit 1 } else { return }
    }

    Robocopy.exe $Source $Destination /MIR /NFL /NDL /NC /NS /NP
}

#------------------------------------------------------------------------------------------
# Test if we are logged in as administrator
# If not force an elevated copy of the shell
#------------------------------------------------------------------------------------------
Function Elevate-Shell {

    Write-TestSuiteInfo "Check to see if we are currently running as `"Administrator`":"
    Write-TestSuiteInfo "If we are currently running as `"Administrator`", change the title and background color to indicate;"
    Write-TestSuiteInfo "Else, elevate the shell to run as `"Administrator`"."

    # Get identity of the current logon user and the administrator
    Write-TestSuiteStep "Get the identity of the current logon user:"
    $MyWindowsId = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $MyWindowsPrincipal = New-Object System.Security.Principal.WindowsPrincipal($MyWindowsId)
    Write-TestSuiteInfo $MyWindowsPrincipal.Identity.Name
    Write-TestSuiteStep "Get the identity of the Administrator role:"
    $AdminRole = [System.Security.Principal.WindowsBuiltInRole]::Administrator
    Write-TestSuiteInfo $AdminRole

    # Check if the current logon user is in the Administrator role
    Write-TestSuiteStep "Check if the current logon user is in the Administrator role:"
    if ($MyWindowsPrincipal.IsInRole($AdminRole)) {
        # Inform the user that we have detected we are running as "Administrator"
        Write-TestSuiteInfo "Detected we are running as `"Administrator`"."

        # Change the title and background color to indicate this
        Write-TestSuiteStep "Change the title and background color to indicate this."
        $Host.UI.RawUI.WindowTitle = $Script:InitialInvocation.MyCommand.Definition + " (Elevated as $AdminRole)"
        $Host.UI.RawUI.BackgroundColor = "DarkBlue"
        Clear-Host
    }
    else {
        # Inform the user that we have detected we are not running as "Administrator"
        Write-TestSuiteWarning "Detected we are not running as the `"Administrator`". Attempt to Elevate the prompt."

        Write-TestSuiteStep "Relaunch as administrator."
        # Create a new process object that starts PowerShell
        $NewProcess = New-Object System.Diagnostics.ProcessStartInfo "$PSHOME\PowerShell.exe";
        # Specify the current script path and name as a parameter, and build the argument list        
        $ArgumentList = "-NoExit -Command " + $InvocationFullPath
        $NewProcess.Arguments = $ArgumentList
        # Indicate that the process should be elevated
        $NewProcess.Verb = "runas"
        # Start the new process
        [System.Diagnostics.Process]::Start($NewProcess)
        # Exit from the current, unelevated process
        exit
    }
}

#------------------------------------------------------------------------------------------
# Check the prerequisites of the host machine before setup test suite environment
#------------------------------------------------------------------------------------------
Function Check-HostPrerequisites {

    Write-TestSuiteInfo "Check prerequisites of the host for test suite environment setup:"

    Write-TestSuiteStep "Check if the host operating system version is supported or not."
    if ([Double]$Script:HostOsBuildNumber -le [Double]"6.1") {
        Write-TestSuiteError "Unsupported operating system version $Script:HostOsBuildNumber. Must be larger than 6.1." -Exit
    }
    else {
        Write-TestSuiteSuccess "Supported operating system version $Script:HostOsBuildNumber."
    }

    Write-TestSuiteStep "Check if the host has enabled router by registry key."
    # http://technet.microsoft.com/en-us/library/cc962461.aspx
    If ((Get-ItemProperty -path HKLM:\system\CurrentControlSet\services\Tcpip\Parameters -name IpEnableRouter -ErrorAction Silentlycontinue).ipenablerouter -ne 1) {
        Write-TestSuiteWarning "Router is disabled. Registry key IpEnableRouter under path HKLM:\system\CurrentControlSet\services\Tcpip\Parameters is not set to 1. Set it now..."
        Set-ItemProperty -Path HKLM:\system\CurrentControlSet\services\Tcpip\Parameters -Name IpEnableRouter -Value 1
    }
    else {
        Write-TestSuiteSuccess "Router is enabled."
    }

    Write-TestSuiteStep "Check if `"RSAT-Hyper-V-Tools`" feature is installed or not."
    Write-TestSuiteInfo "Import ServerManager module if not imported."
    Import-Module ServerManager
    $FeatureName = "RSAT-Hyper-V-Tools"
    $Feature = Get-WindowsFeature | Where { $_.Name -eq "$FeatureName" }
    if ($Feature.Installed -eq $false) {
        Write-TestSuiteWarning "Feature not installed. Install it now..."
        Add-WindowsFeature -Name $FeatureName -IncludeAllSubFeature -IncludeManagementTools
        Wait-TestSuiteActivityComplete -ActivityName "Install $FeatureName" -TimeoutInSeconds 5
    }
    else {
        Write-TestSuiteSuccess "Feature already installed."
    }
    
    Write-TestSuiteStep "Check if `"Hyper-V v3.0 PowerShell Module`" is imported:"
    if (!(Get-Module -ListAvailable Hyper-V)) {
        Write-TestSuiteWarning "Module not imported. Import it now..."
        Import-Module Hyper-V
    } 
    else {
        Write-TestSuiteSuccess "Module already imported."
    }
}

#------------------------------------------------------------------------------------------
# Format the input xml file and display it to the screen
#------------------------------------------------------------------------------------------
Function Format-TestSuiteXml {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [xml]$Xml,
        [int]$Indent = 2)

    Process {
        $StringWriter = New-Object System.IO.StringWriter
        $XmlWriter = New-Object System.Xml.XmlTextWriter $StringWriter
        $XmlWriter.Formatting = "indented"
        $XmlWriter.Indentation = $Indent
        [xml]$Xml.WriteContentTo($XmlWriter)
        $XmlWriter.Flush()
        $StringWriter.Flush()

        # Output the result
        Write-Output $("`n" + $StringWriter.ToString())
    }
}

#------------------------------------------------------------------------------------------
# Get the Xml file from the pipeline
# Extract useful contents from the xml file, and form an object
# Write the object to the output
#------------------------------------------------------------------------------------------
Function Get-XmlFileContents {
    Process {
        # Create a new object
        $Obj = New-Object PSObject

        # Read data from the xml file
        [Xml]$Xml = Get-Content $_.FullName

        # Build object by adding properties
        $Obj | Add-Member NoteProperty LabId($XmlFileId)
        $Obj | Add-Member NoteProperty Option($XmlFileId)
        $Obj | Add-Member NoteProperty FileName($_.name)
        $Obj | Add-Member NoteProperty VMs($Xml.lab.servers.vm.count)
        $Obj | Add-Member NoteProperty Memory((($Xml.lab.servers.vm | Measure-Object -Property memory -Sum).sum))
        $Obj | Add-Member NoteProperty Domain(($Xml.lab.servers.vm)[0].domain)
        $Obj | Add-Member NoteProperty Description($Xml.lab.core.description)
        $XmlFileId++

        # Output the object
        Write-Output $Obj
    }
}

#------------------------------------------------------------------------------------------
# Provide list for the end user to select a sample XML configuration file, used only when $XmlFile is empty
#------------------------------------------------------------------------------------------
Function Choose-XmlFromLib {

    Write-TestSuiteInfo "Choose an XML configuration file from the prepared XML library."   

    # Get a list of the xml files in the xml file directory
    Write-TestSuiteStep "Go to the XML library $Script:XmlLibPath and list all the XML configuration files."
    $Files = dir $Script:XmlLibPath

    # Print out the files to the screen using the xmltable function
    Write-TestSuiteInfo "============================================================"
    Write-TestSuiteInfo "             Availible XML Configuration Files              "
    Write-TestSuiteInfo "============================================================"
    $XmlFileId = 0
    $Files | Get-XmlFileContents | Format-Table -Property LabId, FileName, VMs, Memory, Description -AutoSize

    # Ask which file to use
    $Choice = Read-Host "Select an XML configuration file by <LabId>"
    # Set the XmlFileFullPath variable to the full path of the chosen file
    $Script:XmlFileFullPath = $Files[$Choice].FullName

    # Fix the full path for the xml file
    Write-TestSuiteStep "Fix the full path of the chosen file."
    $Script:XmlFileFullPath = ([IO.Path]::Combine($Script:InvocationPath, $Script:XmlFileFullPath))
    Write-TestSuiteInfo $Script:XmlFileFullPath
}

#------------------------------------------------------------------------------------------
# Read and parse XML configuration file
# $Setup will be used as a global variable to storage the configuration information
#------------------------------------------------------------------------------------------
Function Read-TestSuiteXml {

    Write-TestSuiteInfo "Read and parse the XML configuration file."

    Write-TestSuiteStep "Check if the XML configuration file exist or not."
    # If $XmlFileFullPath is not found, prompt a list of choices for user to choose
    if (!(Test-Path -Path $Script:XmlFileFullPath)) {
        Write-TestSuiteWarning "$Script:XmlFileFullPath file not found. Choose one from the prepared XML library."
        Choose-XmlFromLib
    }
    else {
        Write-TestSuiteSuccess "$Script:XmlFileFullPath file found."
    }

    # Read contents from the XML file
    Write-TestSuiteStep "Read contents from the XML configuration file."
    [Xml]$Script:Setup = Get-Content $Script:XmlFileFullPath
    if ($Script:Setup -eq $null) {
        Write-TestSuiteError "$Script:XmlFileFullPath file is not a valid xml configuration file." -Exit
    }
    else {
        $Script:Setup | Format-TestSuiteXml -Indent 4
    }
}

#------------------------------------------------------------------------------------------
# Fix the contents read from XML configuration file according to real situation of the host capacity and file existance
#------------------------------------------------------------------------------------------
Function Fix-TestSuiteXml {

    Write-TestSuiteInfo "Fix the contents read from XML configuration file according to the real situation of the host."

    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort -Property installorder)) {
        # Fix CPU
        Write-TestSuiteInfo "Fix the virtual processor numbers for this VM if it exceeds the physical limitation."
        Write-TestSuiteStep "Get the maximum CPU numbers supported in this physical host."
        $HostCpu = (Get-WmiObject Win32_ComputerSystem).NumberofLogicalProcessors
        Write-TestSuiteInfo "$HostCpu cores"

        Write-TestSuiteStep "Get the demanding CPU numbers from the XML configuration file."
        Write-TestSuiteInfo "- Priority List (If the previous is not specified, go to the next one):"
        Write-TestSuiteInfo " 1. Read from `"lab.servers.vm.cpu`" node in the XML configuration file."
        Write-TestSuiteInfo " 2. Read from `"lab.core.basecpu`" node in the XML configuration file."
        if ($Vm.cpu) {
            Write-TestSuiteInfo "The `"lab.servers.vm.cpu`" node is specified in the XML configuration file."
            $DemandingCpu = $Vm.cpu
        }
        elseif ($Script:Setup.lab.core.basecpu) {
            Write-TestSuiteInfo "The `"lab.core.basecpu`" node is specified in the XML configuration file."
            $DemandingCpu = $Script:Setup.lab.core.basecpu
            $Vm.AppendChild($Script:Setup.CreateElement("cpu"))
            $Vm.cpu = [string]$DemandingCpu
        }
        Write-TestSuiteInfo $DemandingCpu

        Write-TestSuiteStep "Check if the demanding CPU number exceeds the physical limitation."
        if ([int]$DemandingCpu -gt [int]$HostCpu) {
            Write-TestSuiteWarning "Exceeded. Will use the maximum supported..."
            if (!$Vm.SelectSingleNode("./cpu")) {
                $Vm.AppendChild($Script:Setup.CreateElement("cpu"))
            }
            $Vm.cpu = [string]$HostCpu
        }
        else {
            Write-TestSuiteSuccess "Not exceeded."
        }

        # Fix Memory
        Write-TestSuiteInfo "Fix the memory if it is not an even number."
        if ($Vm.memory % 2) {
            Write-TestSuiteInfo "The memory is not an even number, correct it by deducting 1."
            $Vm.memory = [string]($Vm.memory - 1)
        }

        $imageRef = $Vm.imageReference
        if ($null -eq $imageRef) {
            Write-TestSuiteError "Image information is specified in nowhere." -Exit
        }
        # Fix Disk
        Write-TestSuiteStep "Determine the virtual hard disks to be attached to this VM."
        $diskName = $imageRef.diskName
        if ([string]::IsNullOrEmpty($diskName)) {
            Write-TestSuiteError "Disk information is specified in nowhere." -Exit
        } 

        Write-TestSuiteSuccess "`$diskName parameter is specified."
        Write-TestSuiteInfo $diskName
        Write-TestSuiteStep "Check if the VHD file exists in the VHD library."
        $VmSourceDiskFullPath = $Script:VhdLibPath + "\" + $diskName
        if (!(Test-Path $VmSourceDiskFullPath)) {
            Write-TestSuiteError "VHD file $VmSourceDiskFullPath not found." -Exit
        }
        else {
            Write-TestSuiteSuccess "VHD file $VmSourceDiskFullPath found."
        }

        if (!$Vm.SelectSingleNode("./disk")) {
            $Vm.AppendChild($Script:Setup.CreateElement("disk"))
        }
        $Vm.disk = [string]$VmSourceDiskFullPath
            
        if ($Vm.os -ne "Linux") {
            # Fix Answer File
            Write-TestSuiteStep "Determine the answer file to be used for this VM."
            $answerFile = $imageRef.answerFile
            if ([string]::IsNullOrEmpty($answerFile)) {
                Write-TestSuiteError "Answer File information is specified in nowhere." -Exit
            } 

            Write-TestSuiteSuccess "The `$answerFile parameter is specified."
            Write-TestSuiteInfo $answerFile
            Write-TestSuiteStep "Check if the Answer File exists in the Answer File library."
            $VmAnswerFileFullPath = $Script:AnswerFileLibPath + "\" + $answerFile
            if (!(Test-Path $VmAnswerFileFullPath)) {
                Write-TestSuiteError "Answer File $VmAnswerFileFullPath not found." -Exit
            }
            else {
                Write-TestSuiteSuccess "Answer File $VmAnswerFileFullPath found."
            }

            if (!$Vm.SelectSingleNode("./answerfile")) {
                $Vm.AppendChild($Script:Setup.CreateElement("answerfile"))
            }
            $Vm.answerfile = [string]$VmAnswerFileFullPath
        }
    }

    Write-TestSuiteStep "Read contents after fixing the XML configuration file."
    $Script:Setup | Format-TestSuiteXml -Indent 4
}

#------------------------------------------------------------------------------------------
# Take snapshots for all VMs
#------------------------------------------------------------------------------------------
Function Checkpoint-AllVMs {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$CheckpointName)

    Process {
        Write-TestSuiteStep "Take a snapshot of the VM: $CheckpointName."
        foreach ($Vm in ($Script:Setup.lab.servers.vm | sort -Property installorder)) {
            Checkpoint-VM -Name $Vm.hypervname -SnapshotName $CheckpointName
        }
    }
}

#------------------------------------------------------------------------------------------
# Check if the host machine has duplicate IP addresses that will be used as the gateways for this test suite's virtual networks
#------------------------------------------------------------------------------------------
Function Check-DuplicateIpAddresses {

    Write-TestSuiteInfo "Check if the host machine has duplicate IP addresses that will be used as the gateways for this test suite's virtual networks."

    $isDuplicated = $false

    Write-TestSuiteStep "Get all the IP addresses for the physical network adapters in the host."
    $HostNetworkAdapterConfigurations = Get-WmiObject -Class Win32_NetworkAdapterConfiguration -Filter IPEnabled=TRUE -ComputerName .
    $HostNetworkAdapterConfigurations | Format-Table -Property Description, IPAddress -AutoSize

    Write-TestSuiteStep "Get all the Public IP addresses from XML configuration file."
    $ConfigureVirtualNetworks = $Script:Setup.lab.network.vnet
    $ConfigureVirtualNetworks | Format-Table -Property name, ip -AutoSize

    Write-TestSuiteStep "Check if the host has duplicate IP addresses that will be used by this test suite already configured on the physical network adapters in the host."
    foreach ($HostNetworkAdapterConfiguration in $HostNetworkAdapterConfigurations) {
        foreach ($HostNetworkAdapterIp in $HostNetworkAdapterConfiguration.IPAddress) {
            foreach ($ConfigureVirtualNetwork in $ConfigureVirtualNetworks) {
                if ($ConfigureVirtualNetwork.ip -eq $HostNetworkAdapterIp) {
                    Write-TestSuiteWarning $("The IP address - " + $ConfigureVirtualNetwork.ip + " for virtual network - " + $ConfigureVirtualNetwork.name + " has already been configured on network adapter - " + $HostNetworkAdapterConfiguration.Description)
                    $isDuplicated = $true
                } 
            }
        }
    }

    if ($isDuplicated) {
        Write-TestSuiteError "There are IP addresses already configured on the host that will be used as the gateways for this test suite's virtual networks." -Exit
    }
}

#------------------------------------------------------------------------------------------
# Check if the host machine has duplicate vm names already configured on Hyper-V Manager that will be used for this test suite
#------------------------------------------------------------------------------------------
Function Check-DuplicateVmNames {

    Write-TestSuiteInfo "Check if the host machine has duplicate vm names already configured on Hyper-V Manager that will be used by this test suite."

    $isDuplicated = $false

    Write-TestSuiteStep "Get all the VM names on the host:"
    $HostVms = (Get-VM  | Select-Object -Property Name)
    $HostVms | Format-Table -Property Name -AutoSize

    Write-TestSuiteStep "Get all the VM names in the XML configuration file:"
    $ConfigureVms = $Script:Setup.lab.servers.vm
    $ConfigureVms | Format-Table -Property hypervname -AutoSize

    Write-TestSuiteStep "Check if the host has duplicate VMs configured in Hyper-V Manager:"
    foreach ($ConfigureVm in $ConfigureVms) {
        foreach ($HostVm in $HostVms) {
            if ($ConfigureVm.hypervname -eq $HostVm.Name) { 
                Write-TestSuiteInfo $("VM name - " + $ConfigureVm.hypervname + " has already been used in the host.")
                $isDuplicated = $true
            }
        }
    }
    if ($isDuplicated) {
        Write-TestSuiteError "There are VM names already configured on the host that will be used by this test suite." -Exit     
    }
}

#------------------------------------------------------------------------------------------
# Check if the host machine has enough memory to create virtual machines
#------------------------------------------------------------------------------------------
Function Check-HostMemory {

    Write-TestSuiteInfo "Check if the host machine has duplicate vm names already configured on Hyper-V Manager that will be used by this test suite."

    Write-TestSuiteStep "Get the maximum available physical memory in the host."
    $MaxHypervFreeMemory = [int](((Get-WmiObject win32_OperatingSystem).freephysicalmemory) / 1kb)
    Write-TestSuiteInfo "$MaxHypervFreeMemory MB"
    
    Write-TestSuiteStep "Get the total memory needed in the XML configuration file."
    $TotalLabMemory = ($Script:Setup.lab.servers.vm | Measure-Object -Property memory -Sum).sum
    Write-TestSuiteInfo "$TotalLabMemory MB"

    Write-TestSuiteStep "Check if there is enough free memory left for installing the test suite environment."
    if ($TotalLabMemory -gt $MaxHypervFreeMemory) {
        Write-TestSuiteError "Configuration requires $TotalLabMemory MB memory, but host has only $MaxHypervFreeMemory MB free physical memory.`nPlease prepare sufficent memory before start." -Exit
    }
    else {
        Write-TestSuiteSuccess "Enough free physical memory on host for deployment."
    }
}

#------------------------------------------------------------------------------------------
# Deploy all the virtual network switches for this test suite
#------------------------------------------------------------------------------------------
Function Deploy-TestSuiteVirtualNetworkSwitches {

    Write-TestSuiteInfo "Deploy virtual network switches for this test suite."

    foreach ($Vnet in $Script:Setup.lab.network.vnet) {
    
        Write-TestSuiteStep "Get the virtual switch's name."
        Write-TestSuiteInfo $Vnet.name

        # Get the "NATSwitch" virtual network switch if it exists
        Write-TestSuiteStep "Get the existing NATSwitch virtual switch."
        $NATVirtualSwitch = Get-VMSwitch -SwitchName "NATSwitch" -SwitchType Internal -ErrorAction SilentlyContinue
        if ($NATVirtualSwitch -ne $null) {
            Write-TestSuiteStep "The NATSwitch exists,let us updating the NetIPAddress properties."
            $NATVirtualSwitch
            Write-TestSuiteStep "The old NATSwitch Net IP address properties."
            Get-NetIPAddress -IPAddress 192.168.0.1
            Remove-NetIPAddress -IPAddress 192.168.0.1 -Confirm:$false
            $ifIndex = (Get-NetAdapter "vEthernet (NATSwitch)").ifIndex
            New-NetIPAddress -IPAddress 192.168.0.1 -PrefixLength 24 -InterfaceIndex $ifIndex
            Write-TestSuiteStep "The new NATSwitch Net IP address properties."
            Get-NetIPAddress -IPAddress 192.168.0.1
        }
        else {
            Write-TestSuiteStep "Create NATSwitch."
            New-VMSwitch -SwitchName "NATSwitch" -SwitchType Internal
            $ifIndex = (Get-NetAdapter "vEthernet (NATSwitch)").ifIndex
            New-NetIPAddress -IPAddress 192.168.0.1 -PrefixLength 24 -InterfaceIndex $ifIndex
            New-NetNat -Name MyNATnetwork -InternalIPInterfaceAddressPrefix 192.168.0.0/24
        }

        Write-TestSuiteStep "Get the virtual switch's type."
        # Virtual switch type, currently only supports internal and external
        # If not specified in the XML configuration file, set as default - "internal"
        $NetworkType = $Vnet.Get_ChildNodes() | Where { $_.Name -eq "networktype" }
        if ($NetworkType -eq $null) {
            $NetworkType = "Internal"
        }
        else {
            $NetworkType = $Vnet.networktype
        }
        Write-TestSuiteInfo $NetworkType

        switch ($NetworkType) {
            "Internal" {
                # If switch is not exists then create switch, otherwise update its static ip address and subnet information
                if ($Vnet.hostisgateway -eq "true") {

                    $VmNetworkAdapter = Get-VMNetworkAdapter -All | Where { $_.Name -eq $Vnet.name }
                    if ($VmNetworkAdapter -eq $null) {
                        Write-TestSuiteStep "Create a new internal virtual switch."
                        New-VMSwitch -Name $Vnet.name -SwitchType Internal
                        # Wait for the operating system to refresh the newly created network adapter
                        Wait-TestSuiteActivityComplete -ActivityName "virtual switch $($Vnet.name)" -TimeoutInSeconds 5
                    }

                    # Retry the switch creation after restart the VMMS service if the previous creation failed.
                    $VmNetworkAdapter = Get-VMNetworkAdapter -ManagementOS -SwitchName $Vnet.name
                    if ($VmNetworkAdapter -eq $null) {
                        Write-TestSuiteStep "Original switch $($Vnet.name) creation failed, try to restart Hyper-V VMM service."
                        Restart-Service vmms
                        Start-Sleep -Seconds 20
                        Write-TestSuiteStep "Retry to create a new internal virtual switch."
                        New-VMSwitch -Name $Vnet.name -SwitchType Internal
                        # Wait for the operating system to refresh the newly created network adapter
                        Wait-TestSuiteActivityComplete -ActivityName "virtual switch $($Vnet.name)" -TimeoutInSeconds 5
                    }

                    # Find the network adapter on the host by name
                    Write-TestSuiteStep "Get virtual network adapter by the exists or newly created virtual switch's name."
                    $VmNetworkAdapter = Get-VMNetworkAdapter -ManagementOS -SwitchName $Vnet.name
                    if ($VmNetworkAdapter -eq $null) {
                        Write-TestSuiteError $("No virtual network adapter found by the newly created virtual switch's name - " + $Vnet.name + '. Please manually create and remove same switch name on the agent host, it will be fixed.') -Exit
                    }
                    else {
                        $VmNetworkAdapter | Format-Table -Property Name, SwitchName, DeviceID, MacAddress, Status -AutoSize

                        Write-TestSuiteStep "Get physical network adapter by the virtual network adapter's GUID:"
                        $NetworkAdapter = Get-WmiObject -Class win32_NetworkAdapter | Where { $_.GUID -eq $VmNetworkAdapter.DeviceID }
                        if ($NetworkAdapter -eq $null) {
                            Write-TestSuiteError $("No physical network adapter found by virtual network adapter's GUID - " + $VmNetworkAdapter.DeviceID) -Exit
                        }
                        else {
                            $NetworkAdapter | Format-Table -Property ServiceName, Name, GUID, MACAddress -AutoSize

                            Write-TestSuiteStep ("Set the statistic IP address and subnet to this physical network adapter.")
                            if ($([IPAddress]$Vnet.ip).AddressFamily -eq "InterNetwork") {
                                netsh interface ipv4 set address $NetworkAdapter.InterfaceIndex static $Vnet.ip $Vnet.subnet
                                Write-TestSuiteSuccess $("IP address - " + $Vnet.ip + " and subnet - " + $Vnet.subnet + " have been updated.")
                            }
                            else {
                                netsh interface ipv6 set address $NetworkAdapter.interfaceindex $Vnet.ip
                                Write-TestSuiteSuccess $("IP address - " + $Vnet.ip + " has been updated.")
                            }
                        }
                    }
                }
            }
            "External" {
                # Get the virtual network switch for "external" type if it exists
                Write-TestSuiteStep "Get the existing external virtual switch."
                $ExternalVirtualSwitch = Get-VMSwitch -Name $Vnet.name -SwitchType External -ErrorAction SilentlyContinue
                if ($ExternalVirtualSwitch -ne $null) {
                    $ExternalVirtualSwitch
                }
                else {
                    #------------------------------------------------------------------------------------------
                    # Check if the host machine has duplicate ip addresses or virtual machines
                    #------------------------------------------------------------------------------------------
                    Write-TestSuiteInfo ""
                    Write-TestSuiteInfo "============================================================"
                    Write-TestSuiteInfo "                  Check Host IP Duplicities                    "
                    Write-TestSuiteInfo "============================================================"
                    
                    Check-DuplicateIpAddresses

                    # If unable to get the external virtual network switch, then select a network adapter interface to create one
                    Write-TestSuiteWarning $("No external virtual switch found by name - " + $Vnet.name + ". Create a new one...")
                    Write-TestSuiteStep "Get an existing physical network adapter."
                    $NetworkAdapter = Get-NetAdapter -ErrorAction SilentlyContinue | Where-Object { ($_.InterfaceDescription -notmatch "Hyper-V Virtual") -and ($_.Status -eq "Up") } | Select-Object -First 1 -ErrorAction SilentlyContinue
                    if ($NetworkAdapter -eq $null) {
                        Write-TestSuiteError "No physical network adapter found. Please add hardware to this host machine." -Exit
                    }
                    else {
                        $NetworkAdapter
                        Write-TestSuiteStep "Create a new external virtual switch on this physical network adapter."                      
                        New-VMSwitch -Name $Vnet.name -AllowManagementOS $true -NetAdapterInterfaceDescription $NetworkAdapter.InterfaceDescription
                    }
                }
            }
            default {
                Write-TestSuiteError $("Network type - $NetworkType not supported.") -Exit
            }
        }
    }
}

#------------------------------------------------------------------------------------------
# Create virtual hard disk
#------------------------------------------------------------------------------------------
Function Create-TestSuiteVHD {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        $Vm)

    Process {
        
        Write-TestSuiteInfo "Start creating VHD for $($Vm.hypervname)."

        Write-TestSuiteStep "Get the source VHD file."
        $VmSourceVhd = Get-Item $Vm.disk
        $VmSourceVhdName = $VmSourceVhd.Name
        $VmSourceVhdPath = $VmSourceVhd.DirectoryName
        Write-TestSuiteInfo $Vm.disk

        Write-TestSuiteStep "Indicate the target VHD file that this virtual machine will use."
        $VmVhdPath = "$Script:VmDirPath\$($Vm.hypervname)\Virtual Hard Disks"

        $osName = $Vm.os
        Write-TestSuiteStep "The VM OS is $osName"
        if ($Vm.os -eq "Linux") {
            $VmVhdName = "$($Vm.hypervname).vhdx"
        }
        else {
            $VmVhdName = "$($Vm.hypervname).vhd"
        }
        $VmVhdFullPath = "$VmVhdPath\$VmVhdName"
        Write-TestSuiteInfo $VmVhdFullPath

        Write-TestSuiteStep "Check if dynamic disk is enabled or not."
        if ($Script:DynamicDisk) {
            Write-TestSuiteSuccess "Dynamic disk is enabled."
            
            Write-TestSuiteStep "Remove existing VHDs in the VM folder to avoid conflict."

            if (Test-path -path $VmVhdPath) {
                Get-ChildItem $VmVhdPath *.vhd | remove-item -Force
            } 

            Write-TestSuiteStep "Copy the source VHD $VmSourceVhdFullPath to the target VHD path $VmVhdPath, and rename it with $VmVhdName..."
            Start-Job -Name "Create VHD for $($Vm.hypervname)" -ScriptBlock { `
                    xcopy.exe /i /y /j $args[0] $($args[1] + "\"); `
                    Rename-Item -Path $($args[1] + "\" + $args[2]) -NewName $args[3]; `
            } -ArgumentList $Vm.disk, $VmVhdPath, $VmSourceVhdName, $VmVhdName            
        }
        else {
            Write-TestSuiteWarning "Dynamic disk is disabled."
            Write-TestSuiteStep "Create disk from the source VHD $VmSourceVhdFullPath and place it in the target VHD path $VmVhdFullPath..."
                        
            Write-TestSuiteStep "Remove existing VHDs in the VM folder to avoid conflict."
            Get-ChildItem $VmVhdPath *.vhd | remove-item -Force

            Start-Job -Name "Create VHD for $($Vm.hypervname)" -ScriptBlock { `
                    New-VHD -ParentPath $args[0] -Path $args[1]; `
            } -ArgumentList $Vm.disk, $VmVhdFullPath
        }

        Write-TestSuiteStep "Change the VM's disk node in XML configuration file."
        $Vm.disk = [string]$VmVhdFullPath
    }
}

#------------------------------------------------------------------------------------------
# Get an available NAT IP
#------------------------------------------------------------------------------------------
$NATIPList = @()
Function Get-AvailableNATIP {
    $index = 2
    $requestIP = "192.168.0." + $index
    $requestPass = $false
    Write-TestSuiteInfo "VMs on host:"
    while ($index -le 254 -and !$requestPass) {
        $requestPass = $true
        foreach ($vm in $(Get-VM)) {
            Write-TestSuiteInfo "$($vm.VMName),"
            $netAds = $vm | Get-VMNetworkAdapter
            if ($netAds -ne $null) {
                foreach ($ip in $($netAds.IPAddresses)) {
                    Write-TestSuiteInfo "Checking $ip."
                    if ("$ip" -eq $requestIP -or $NATIPList.Contains($requestIP)) {
                        Write-TestSuiteInfo "$ip is reserved, try next ip."
                        $NATIPList += $requestIP
                        $requestPass = $false
                        $index = $index + 1
                        $requestIP = "192.168.0." + $index
                        break;
                    }
                }
                if (!$requestPass) { break; }
            }
        }
    }
    Write-TestSuiteInfo "Requested NAT IP:$requestIP"
    return $requestIP;
}

#------------------------------------------------------------------------------------------
# Assemble all the required files and create the ISO file for a particular virtual machine
#------------------------------------------------------------------------------------------
Function Create-TestSuiteISO {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        $Vm)
    
    Process {

        Write-TestSuiteInfo "Start creating ISO for $($Vm.hypervname)."

        $VmIsoPath = $Script:IsoDirPath + "\" + $Vm.hypervname
        Write-TestSuiteStep "Create a new ISO directory $VmIsoPath."
        if (Test-Path -Path $VmIsoPath) {
            Write-TestSuiteWarning "The directory has already been created. Remove it recursively."
            Remove-Item -Path $VmIsoPath -Recurse -Force
        }
        mkdir $VmIsoPath
        if ($Vm.os -eq "Linux") {
            Write-TestSuiteStep "Prepare ISO files for Linux."
            $setIPPath = $InvocationPath + "\Linux\setIP.sh"
            Write-TestSuiteStep "Copy setIP.sh from the $setIPPath folder to the ISO directory."
            Copy-Item -Path $setIPPath -Destination $VmIsoPath -force
            $ipCount = $Vm.ip.Count
            if ($ipCount -ne 1) {
                $destSetIPfile = $VmIsoPath + "\setIP.sh"
                Write-TestSuiteStep "Modify $destSetIPfile from eth1 to eth$ipCount"
                (Get-Content -path $destSetIPfile -Raw).Replace("        eth1:", "        eth$($ipCount):") | Set-Content -Path $destSetIPfile
                (Get-Content -path $destSetIPfile -Raw).Replace("01-network-manager-all", "0$ipCount-network-manager-all") | Set-Content -Path $destSetIPfile
            }
            $requestIP = Get-AvailableNATIP
            Write-TestSuiteStep "Write ip.txt for Linux."
            $ipPath = $VmIsoPath + "\ip.txt"
            "$requestIP" | Out-File -FilePath $ipPath
            "255.255.255.0" | Out-File -FilePath $ipPath -Append
            "192.168.0.1" | Out-File -FilePath $ipPath -Append
            "192.168.0.1,10.50.50.50" | Out-File -FilePath $ipPath -Append
            Write-TestSuiteStep "Done setIP.sh and ip.txt for Linux."
        }
        else {
            Write-TestSuiteStep "Prepare ISO files for Windows."
            mkdir $VmIsoPath\Deploy
            mkdir $VmIsoPath\VsLicense

            Write-TestSuiteStep "Copy the XML configuration file to the ISO directory, and rename it to setup.xml and Protocol.xml."
            Copy-Item -Path $Script:XmlFileFullPath -Destination "$VmIsoPath\setup.xml" -force
            Copy-Item -Path $Script:XmlFileFullPath -Destination "$VmIsoPath\Protocol.xml" -force

            # This file will be used by SysPrep tool, it will call "run.cmd", and "run.cmd" will launch "Controller.ps1" eventually
            Write-TestSuiteStep "Copy the Answer file to the ISO directory, and rename it to unattend.xml:"
            Copy-Item -Path $Vm.answerfile -Destination "$VmIsoPath\unattend.xml" -force

            Write-TestSuiteStep "Copy all the files from the ScriptLib folder to the ISO directory recursively."
            Copy-Item -Path $($Script:ScriptLibPath + "\*") -Destination $VmIsoPath -Recurse -force

            Write-TestSuiteStep "Copy all the files from the VsLicense folder to the ISO directory recursively."
            Copy-Item -Path $($Script:VsLicensePath + "\*") -Destination "$VmIsoPath\VsLicense" -Recurse -force

            Write-TestSuiteStep "Copy all the files from the LabPacks folder to the ISO directory if it exists."
            if (Test-Path -Path $($Script:InvocationPath + "\LabPacks")) {
                Sync-File ($Script:InvocationPath + "\LabPacks") ($VmIsoPath + "\LabPacks")
            }

            Write-TestSuiteStep "Copy all the files from the Install\Hotfix folder to the ISO directory if it exists."
            if (Test-Path -Path $($Script:InvocationPath + "\Install\Hotfix")) {
                Sync-File ($Script:Invocationpath + "\Install\Hotfix") ($VmIsoPath + "\Hotfix")
            }

            # These tools will be installed in Controller.ps1 phase 2
            if (![string]::IsNullOrEmpty($Vm.tools)) {
                if (![string]::IsNullOrEmpty($Vm.tools.tool)) {
                    if ($Vm.tools.tool.Count -gt 0) {
                        Write-TestSuiteInfo "There are $($Vm.tools.tool.Count) tools to be installed."
                    }
                    else {
                        Write-TestSuiteInfo "There is only 1 tool to be installed."
                    }
                    foreach ($Tool in $Vm.tools.tool) {
                        if ($Tool.HasAttribute("MSIName")) {
                            Write-TestSuiteStep "Find the $($Tool.MSIName) from the ..\Tools folder."
                            if (Test-Path -Path $($Script:ToolLibPath + "\" + $Tool.name + "\" + $Tool.version + "\" + $Tool.CPUArchitecture + "\" + $Tool.MSIName)) {
                                Write-TestSuiteSuccess "$($Tool.MSIName) found in $($Script:ToolLibPath) folder. Copy it to ISO directory..."
                                Copy-Item -Path $($Script:ToolLibPath + "\" + $Tool.name + "\" + $Tool.version + "\" + $Tool.CPUArchitecture + "\" + $Tool.MSIName) -Destination $($VmIsoPath + "\Deploy") 
                            }
                            else {
                                Write-TestSuiteWarning "$($Tool.MSIName) file not found."
                            }
                        }
                        elseif ($Tool.HasAttribute("EXEName")) {
                            Write-TestSuiteStep "Find the $($Tool.EXEName) from the ..\Tools folder."
                            if (Test-Path -Path $($Script:ToolLibPath + "\" + $Tool.name + "\" + $Tool.version + "\" + $Tool.CPUArchitecture + "\" + $Tool.EXEName)) {
                                Write-TestSuiteSuccess "$($Tool.EXEName) found in $($Script:ToolLibPath) folder. Copy it to ISO directory..."
                                Copy-Item -Path $($Script:ToolLibPath + "\" + $Tool.name + "\" + $Tool.version + "\" + $Tool.CPUArchitecture + "\" + $Tool.EXEName) -Destination $($VmIsoPath + "\Deploy")
                            }
                            else {
                                Write-TestSuiteWarning "$($Tool.EXEName) file not found."
                            }
                        }
                        elseif ($Tool.HasAttribute("ZipName")) {
                            Write-TestSuiteStep "Find the $($Tool.ZipName) from the ..\Tools folder."
                            if ($Tool.version -ne $null) {
                                if (Test-Path -Path $($Script:ToolLibPath + "\" + $Tool.name + "\" + $Tool.version + "\" + $Tool.CPUArchitecture + "\" + $Tool.ZipName)) {
                                    Write-TestSuiteSuccess "$($Tool.ZipName) found in $($Script:ToolLibPath) folder. Copy it to ISO directory..."
                                    Copy-Item -Path $($Script:ToolLibPath + "\" + $Tool.name + "\" + $Tool.version + "\" + $Tool.CPUArchitecture + "\" + $Tool.ZipName) -Destination $($VmIsoPath + "\Deploy")
                                }
                                else {
                                    Write-TestSuiteWarning "$($Tool.ZipName) file not found."
                                }
                            }
                            else {
                                if (Test-Path -Path $($Script:ToolLibPath + "\" + $Tool.name + "\" + $Tool.ZipName)) {
                                    Write-TestSuiteSuccess "$($Tool.ZipName) found in $($Script:ToolLibPath) folder. Copy it to ISO directory..."
                                    Copy-Item -Path $($Script:ToolLibPath + "\" + $Tool.name + "\" + $Tool.ZipName) -Destination $($VmIsoPath + "\Deploy")
                                }
                                else {
                                    Write-TestSuiteWarning "$($Tool.ZipName) file not found."
                                }
                            }
                        }
                    }
                }

                if (![string]::IsNullOrEmpty($Vm.tools.TestsuiteMSI)) {
                    Write-TestSuiteStep "Find the Test Suite from the ..\ProtocolTestSuite\<TestSuiteName>\Deploy folder."
                    if (Test-Path -Path $($Script:DeployFilePath + "\" + $($Vm.tools.TestsuiteMSI.MSIName))) {
                        Write-TestSuiteSuccess "$($Vm.tools.TestsuiteMSI.Name) found in $($Script:DeployFilePath) folder. Copy it to ISO directory..."
                        Copy-Item -Path $($Script:DeployFilePath + "\" + $($Vm.tools.TestsuiteMSI.MSIName)) -Destination $($VmIsoPath + "\Deploy") -force
                    }
                    else {
                        Write-TestSuiteWarning "$($Vm.tools.TestsuiteMSI.Name) file not found."
                    }
                }

                if (![string]::IsNullOrEmpty($Vm.tools.TestsuiteZip)) {
                    Write-TestSuiteStep "Find the Test Suite from the ..\ProtocolTestSuite\<TestSuiteName>\Deploy folder."
                    if (Test-Path -Path $($Script:DeployFilePath + "\" + $($Vm.tools.TestsuiteZip.ZipName))) {
                        Write-TestSuiteSuccess "$($Vm.tools.TestsuiteZip.ZipName) found in $($Script:DeployFilePath) folder. Copy it to ISO directory..."
                        Copy-Item -Path $($Script:DeployFilePath + "\" + $($Vm.tools.TestsuiteZip.ZipName)) -Destination $($VmIsoPath + "\Deploy") -force
                    }
                    else {
                        Write-TestSuiteWarning "$($Vm.tools.TestsuiteZip.ZipName) file not found."
                    }
                }
            }
        
            # These scripts will be executed in Controller.ps1 phase 2
            if (![string]::IsNullOrEmpty($Vm.installscript)) {
                Write-TestSuiteStep "Find the Install Scripts from the ScriptLib folders."
                foreach ($InstallScript in [Array]$Vm.installscript.Split(";")) {
                    if (Test-Path -Path $($Script:ScriptLibPath + "\" + $InstallScript)) {
                        Write-TestSuiteSuccess "$InstallScript found in ScriptLibPath folder. Copy it to ISO directory..."
                        Copy-Item -Path $($Script:ScriptLibPath + "\" + $InstallScript) -Destination $($VmIsoPath + "\" + $InstallScript) -force
                    }
                    else {
                        Write-TestSuiteWarning "$InstallScript script not found."
                        continue
                    }
                    Write-TestSuiteStep "Add the script to Install.ps1, which will be called by controller.ps1 in phase 2 later."
                    [string]$CmdLine = "Write-Host `"Running $InstallScript`""
                    Add-Content -Path $VmIsoPath\Install.ps1 -Value $CmdLine
                    Add-Content -Path $VmIsoPath\Install.ps1 -Value ("C:\Temp\" + $InstallScript)
                }
            }

            # These scripts will be executed in Controller.ps1 phase 3, before PostScript
            if (![string]::IsNullOrEmpty($Vm.installfeaturescript)) {
                Write-TestSuiteStep "Find the Install Feature Scripts from the ScriptLib folders."
                foreach ($InstallFeatureScript in [Array]$Vm.installfeaturescript.Split(";")) {
                    if (Test-Path -Path $($Script:ScriptLibPath + "\" + $InstallFeatureScript)) {
                        Write-TestSuiteSuccess "$InstallFeatureScript found in PostScript folder. Copy it to ISO directory..."
                        Copy-Item -Path $($Script:ScriptLibPath + "\" + $InstallFeatureScript) -Destination $($VmIsoPath + "\" + $InstallFeatureScript) -force
                    }
                    elseif (Test-Path -Path $($Script:ScriptsFilePath + "\" + $InstallFeatureScript)) {
                        Write-TestSuiteSuccess "$InstallFeatureScript found in Test Suite scripts folder."
                        Copy-Item -Path $($Script:ScriptsFilePath + "\" + $InstallFeatureScript) -Destination $($VmIsoPath + "\" + $InstallFeatureScript) -force
                    }
                    else {
                        Write-TestSuiteWarning "$InstallFeatureScript script not found."
                        continue
                    }
                    Write-TestSuiteInfo "Add the script to InstallFeatureScript.ps1, which will be called by controller.ps1 in phase 3 later."
                    [string]$CmdLine = "Write-Host `"Running $InstallFeatureScript`""
                    Add-Content -Path $VmIsoPath\InstallFeatureScript.ps1 -Value $CmdLine
                    Add-Content -Path $VmIsoPath\InstallFeatureScript.ps1 -Value ("C:\Temp\" + $InstallFeatureScript)
                }
            }

            # These scripts will be executed in Controller.ps1 phase 3
            if (![string]::IsNullOrEmpty($Vm.postscript)) {
                Write-TestSuiteStep "Find the Post Scripts from the PostScript or Install folders."
                foreach ($PostScript in [Array]$Vm.postscript.Split(";")) {
                    if (Test-Path -Path $($Script:PostScriptFilePath + "\" + $PostScript)) {
                        Write-TestSuiteInfo "$PostScript found in Test Suite PostScript folder."
                        Copy-Item -Path $($Script:PostScriptFilePath + "\" + $PostScript) -Destination $($VmIsoPath + "\" + $PostScript) -force
                    }
                    elseif (Test-Path -Path $($Script:ScriptsFilePath + "\" + $PostScript)) {
                        Write-TestSuiteInfo "$PostScript found in Test Suite scripts folder."
                        Copy-Item -Path $($Script:ScriptsFilePath + "\" + $PostScript) -Destination $($VmIsoPath + "\" + $PostScript) -force
                    }
                    elseif (Test-Path -Path $($Script:ScriptLibPath + "\" + $PostScript)) {
                        Write-TestSuiteInfo "$PostScript found in ScriptLib folder."
                        Copy-Item -Path $($Script:ScriptLibPath + "\" + $PostScript) -Destination $($VmIsoPath + "\" + $PostScript) -force
                    }
                    else {
                        Write-TestSuiteInfo "$PostScript script not found."
                        continue
                    }
                    Write-TestSuiteInfo "Add the script call to Post.ps1, which will be called by controller.ps1 in phase 3 later."
                    [string]$CmdLine = "Write-Host `"Running $PostScript`""
                    Add-Content -Path $VmIsoPath\Post.ps1 -Value $CmdLine
                    Add-Content -Path $VmIsoPath\Post.ps1 -Value ("C:\Temp\" + $PostScript)
                }
                # By default, no retart needed during post script
                if (($Vm.SelectSingleNode("./restartinpostscript") -eq $null) -or (!$Vm.restartinpostscript)) {
                    # This signal file will be generated if no restart throughout the post scripts
                    # If restart is occurred in any one of the post scripts, make sure the final script writes this signal file
                    Add-Content -Path $VmIsoPath\Post.ps1 -Value ("Write signal file`: post.finished.signal to hard drive.")
                    Add-Content -Path $VmIsoPath\Post.ps1 -Value ("cmd /C ECHO CONFIG FINISHED > `"C:\Temp\post.finished.signal`"")
                }
            }

            # These scripts will be needed for running the post scripts in Controller.ps1 phase 3
            Write-TestSuiteStep "Copy all the files from <TestSuite>\Scripts to the ISO directory if it exists."
            if (Test-Path -Path $Script:ScriptsFilePath) {
                Sync-File ($Script:ScriptsFilePath) ($VmIsoPath + "\Scripts")
            }
            Write-TestSuiteStep "Copy all the files from <TestSuite>\Data to the ISO directory if it exists."
            if (Test-Path -Path $Script:DataFilePath) {
                Sync-File ($Script:DataFilePath) ($VmIsoPath + "\Data")
            }
            Write-TestSuiteStep "Copy all the files from <TestSuite>\Tools to the ISO directory if it exists."
            if (Test-Path -Path $Script:ToolsFilePath) {
                Sync-File ($Script:ToolsFilePath) ($VmIsoPath + "\Tools")
            }

            Write-TestSuiteStep "Record host name $($Vm.name) in file Name.txt to the ISO directory."
            $Vm.name > $VmIsoPath\Name.txt

            Write-TestSuiteStep "Record logon information to the ISO directory."
            $VMDomainName = "WORKGROUP"

            if ([string]::IsNullOrEmpty($Vm.domain)) {                        
                $VmNetbios = "WORKGROUP"
            }
            else {
                $VMDomainName = $Vm.domain
                $VmNetbios = ($VMDomainName.Split("."))[0]
            }
            $VmUsername = $Script:Setup.lab.core.username
            $VmPassword = $Script:Setup.lab.core.password
            $VMDomainName > $VmIsoPath\DomainDnsName.txt
            $VmNetbios > $VmIsoPath\DomainNetbiosName.txt
            $VmUsername > $VmIsoPath\DomainAdminName.txt
            $VmPassword > $VmIsoPath\DomainAdminPwd.txt
        
            if (($Vm.SelectSingleNode("./isdc") -ne $null) -and $Vm.isdc) {
                Write-TestSuiteStep "Record domain controller promotion information using the template under Install folder to the ISO diretory."
                [Array]$VmDomainControllers = $Script:Setup.lab.servers.vm | Where { ($_.domain -eq $VMDomainName) -and ($_.isdc -eq "true") } | Sort-Object -Property Installorder
                if ($Vm.name -eq $VmDomainControllers[0].name) {
                    $DcPromoContents = Get-Content $Script:InstallLibPath\dcpromo_create.txt
                    $DcPromoContents[8] = "NewDomainDNSName=$($VMDomainName)"
                    $DcPromoContents[10] = "DomainNetbiosName=$VmNetbios"
                    $DcPromoContents[19] = "SafeModeAdminPassword=$VmPassword"
                }
                else {
                    $DcPromoContents = Get-Content $Script:InstallLibPath\dcpromo_join.txt
                    $DcPromoContents[11] = "ReplicaDomainDNSName=$($VMDomainName)"
                    $DcPromoContents[16] = "UserDomain=$($VMDomainName)"
                    $DcPromoContents[17] = "Username=$VmUsername"
                    $DcPromoContents[18] = "Password=$VmPassword"
                    $DcPromoContents[23] = "SafeModeAdminPassword=$VmPassword"
                }
                $DcPromoContents > $VmIsoPath\dcpromo.txt
            }
        }
        Write-TestSuiteStep "Create the ISO file using the ISO directory."
        $VmIsoFullPath = $VmIsoPath + ".iso"
        Start-Job -Name "Create ISO for $($Vm.hypervname)" -ScriptBlock { cmd /c "$($args[0])\oscdimg.exe -j2 -o -m `"$($args[1])`" `"$($args[2])`"" 2>&1 } -ArgumentList "$Script:InstallLibPath", $VmIsoPath, $VmIsoFullPath
    }
}

#------------------------------------------------------------------------------------------
# Create virtual machine
#------------------------------------------------------------------------------------------
Function Create-TestSuiteVM {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        $Vm)
    
    Process {

        Write-TestSuiteInfo "Start creating VM for $($Vm.hypervname)."

        Write-TestSuiteStep "Create a new virtual machine with name - $($Vm.hypervname) under location - $($Script:VmDirPath)."

        $newVM = @{
            Name = $Vm.hypervname
            Generation = 2
            Path = $Script:VmDirPath
        }

        New-VM @newVM
        # New-VM -Name $Vm.hypervname -Path $Script:VmDirPath

        Write-TestSuiteStep "Configure the CPU for this virtual machine to - $($Vm.cpu)."
        Set-VM -Name $Vm.hypervname -ProcessorCount $Vm.cpu
        
        Add-VMScsiController -VMName $Vm.hypervname
        $owner = Get-HgsGuardian UntrustedGuardian
        $kp = New-HgsKeyProtector -Owner $owner -AllowUntrustedRoot
        Set-VMKeyProtector -VMName $Vm.hypervname -KeyProtector $kp.RawData
        Enable-VMTPM -VMName $Vm.hypervname
        
        $VmMem = [int]$Vm.memory * 1024 * 1024
        Write-TestSuiteStep "Configure the memory for this virtual machine to - $($Vm.memory) MB ($VmMem Bytes)."
        if (($Vm.minimumram -ne $null) -and ($Vm.maximumram -ne $null)) {
            $MinMem = [int]$Vm.minimumram * 1024 * 1024
            $MaxMem = [int]$Vm.maximumram * 1024 * 1024
            Write-TestSuiteStep "Minimum memory - $($Vm.minimumram) MB ($MinMem Bytes) and Maximum memory - $($Vm.maximumram) MB ($MaxMem Bytes)."
            Set-VM -Name $Vm.hypervname -DynamicMemory -MemoryStartupBytes $VmMem -MemoryMinimumBytes $MinMem -MemoryMaximumBytes $MaxMem
        }
        else {
            Set-VM -Name $Vm.hypervname -StaticMemory -MemoryStartupBytes $VmMem
        }

        Write-TestSuiteStep "Remove the existing virtual network adapters of this virtual machine."
        Remove-VMNetworkAdapter -VMName $Vm.hypervname
        
        Write-TestSuiteStep "Add a new virtual network adapter to this virtual machine, and connect it to the following virtual switches."
        $NicNumber = 0;
        [array]$ServerVnet = $Vm.vnet
        $VirtualSwitch = $ServerVnet[0]
        foreach ($ip in $Vm.ip) {
            if ($ServerVnet.Count -gt 1) {
                $VirtualSwitch = $ServerVnet[$NicNumber]
            }
            
            Write-TestSuiteInfo "set virtual network adapter for ip:$ip - $VirtualSwitch"
            Add-VMNetworkAdapter -VMName $Vm.hypervname -SwitchName $VirtualSwitch
            $NicNumber++;
        }

        # For Linux, add an NATSwitch adapter
        if ($Vm.os -eq "Linux") {
            if ( $null -ne $Vm.vnetExternal) {
                try {
                    if ($Vm.vnetExternal -eq "NATSwitch") {
                        Add-VMNetworkAdapter -VMName $Vm.hypervname -SwitchName "NATSwitch"
                        Write-TestSuiteStep "$($Vm.hypervname) added switch: NATSwitch."
                    }
                    else {
                        $linuxExternalVirtualSwitchList = Get-VMSwitch -SwitchType External
                        Add-VMNetworkAdapter -VMName $Vm.hypervname -SwitchName $linuxExternalVirtualSwitchList[0].name
                        Write-TestSuiteStep "$($Vm.hypervname) added switch: $($linuxExternalVirtualSwitchList[0].name)."
                    }
                }
                catch {
                    Write-TestSuiteError "Add external adapter $($Vm.vnetExternal) failed for Linux VM $($Vm.hypervname)" -Exit
                }
            }
        }
        
        Write-TestSuiteStep "Wait for create VHD job to be ready within 3600 seconds."
        $Job = Get-Job -Name "Create VHD for $($Vm.hypervname)"
        while ($Job.State -eq "running") {
            Wait-TestSuiteActivityComplete -ActivityName "Create VHD for $($Vm.hypervname)" -TimeoutInSeconds 5
        }
        $Job | Wait-Job -Timeout 3600
        Write-TestSuiteStep "Check whether VHD file exists or not."
        if (!(Test-Path $Vm.disk)) {
            Write-TestSuiteError "$($Vm.disk) file not found." -Exit
        }

        Write-TestSuiteStep "Attach VHD to this virtual machine."
        Add-VMHardDiskDrive -VMName $Vm.hypervname -ControllerType SCSI -ControllerNumber 0 -ControllerLocation 0 -Path $Vm.disk

        Write-TestSuiteStep "Set the VM note with the Current User, Computer Name and IP Addresses (The note will be shown in VStorm Portal as VM Description)."
        $VmNote = $env:USERNAME + ": " + $Vm.name + ": " + $Vm.ip
        Set-VM -VMName $Vm.hypervname -Notes $VmNote
    }
}

#------------------------------------------------------------------------------------------
# Mount the ISO to a particular virtual machine
#------------------------------------------------------------------------------------------
Function Mount-TestSuiteISO {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        $Vm,
        [string]$VmIsoFullPath,
        [int]$TimeoutInSeconds = 3600,
        [int]$SleepTimePerIteration = 5
    )

    Process {

        Write-TestSuiteInfo "Start mounting ISO for $($Vm.hypervname)."

        Write-TestSuiteStep "Get the DVD drive information of this virtual machine."
        $VmDvdDrive = Get-VMDvdDrive -VMName $Vm.hypervname | Select-Object -First 1 
        $VmDvdDrive | Format-List VMName, ControllerType, ControllerNumber, ControllerLocation, DvdMediaType, Path, IsDeleted

        # If $VmIsoFullPath parameter is not specified, assume it means the test suite ISO, otherwise, it will specify application ISO, or Visual Studio ISO
        if ([string]::IsNullOrEmpty($VmIsoFullPath)) {
            Write-TestSuiteStep "Wait for create ISO job to be ready within 3600 seconds."
            $Job = Get-Job -Name "Create ISO for $($Vm.hypervname)"
            while ($Job.State -eq "running") {
                Wait-TestSuiteActivityComplete -ActivityName "Create VHD for $($Vm.hypervname)" -TimeoutInSeconds 5
            }
            $Job | Wait-Job -Timeout 3600
            $VmIsoFullPath = $Script:IsoDirPath + "\" + $Vm.hypervname + ".iso"
        }
        Write-TestSuiteStep "Check whether ISO file exists or not."
        if (!(Test-Path $VmIsoFullPath)) {
            Write-TestSuiteError "$VmIsoFullPath file not found." -Exit
        }

        Write-TestSuiteStep "Mount $VmIsoFullPath to the DVD drive."
        $IsMounted = $false
        $TimeLeft = $TimeoutInSeconds

        do {
            if ([System.String]::IsNullOrEmpty($VmDvdDrivePath)) {
                Add-VMDvdDrive -VMName $Vm.hypervname -Path $VmIsoFullPath
            }
            else {
                Set-VMDvdDrive -VMName $Vm.hypervname -Path $VmIsoFullPath -ControllerNumber $VmDvdDrive.ControllerNumber -ControllerLocation $VmDvdDrive.ControllerLocation
            }
            
            Wait-TestSuiteActivityComplete -ActivityName "Mount ISO $VmIsoFullPath" -TimeoutInSeconds 15
            
            $VmDvdDrivePath = (Get-VMDvdDrive -VMName $Vm.hypervname).Path
            if (![System.String]::IsNullOrEmpty($VmDvdDrivePath)) {
                $IsMounted = $true 
                break
            }

            Write-TestSuiteStep "Mount $VmIsoFullPath to the DVD drive failed, will retry in 2 minutes."
            $SleepTimePerIteration = 120
            Wait-TestSuiteActivityComplete -ActivityName "Mount ISO $VmIsoFullPath" -TimeoutInSeconds $SleepTimePerIteration
        } while ($TimeLeft -= $SleepTimePerIteration)
        if ($IsMounted) {
            Write-TestSuiteSuccess "Successfully mounted."
        }
        else {
            Write-TestSuiteError "Unable to mount ISO within $TimeoutInSeconds seconds." -Exit
        }
    }
}

#------------------------------------------------------------------------------------------
# Used by Wait-TestSuiteISOState function to get exchange data from the virtual machines 
# of this test suite, which is the ISOState. Controller.ps1 within the virtual machine 
# will modify this ISOState in HKLM:\SOFTWARE\Microsoft\Virtual Machine\Guest everytime the
# state is changed
#------------------------------------------------------------------------------------------
Function Get-TestSuiteVmExchangeData {
    Param(
        [string]$VmName,
        [string]$ExchangeItemName)

    Write-TestSuiteInfo "Get exchange data from VM."
    $ExchangeItemDataValue = "not found"

    Write-TestSuiteStep "Get VM by name $VmName."
    $Vm = Get-WmiObject -Namespace "root\virtualization\v2" -Query "Select * From Msvm_ComputerSystem Where ElementName='$VmName'"

    $GuestExchangeItems = $Vm.GetRelated("Msvm_KvpExchangeComponent").GuestExchangeItems
    if ($GuestExchangeItems.count -gt 1) {
        foreach ($GuestExchangeItem in $GuestExchangeItems) {
            $GuestExchangeItemNode = ([xml]$GuestExchangeItem).SelectSingleNode("/INSTANCE/PROPERTY[@NAME='Name']/VALUE[child::text() = '$ExchangeItemName']")
            if ($GuestExchangeItemNode -ne $null) { 
                $ExchangeItemDataValue = $GuestExchangeItemNode.SelectSingleNode("/INSTANCE/PROPERTY[@NAME='Data']/VALUE/child::text()").Value
                break
            }
        }
    }
    elseif ($GuestExchangeItems.count -eq 1) {
        $GuestExchangeItem = $GuestExchangeItems
        $GuestExchangeItemNode = ([xml]$GuestExchangeItem).SelectSingleNode("/INSTANCE/PROPERTY[@NAME='Name']/VALUE[child::text() = '$ExchangeItemName']")
        if ($GuestExchangeItemNode -ne $null) { 
            $ExchangeItemDataValue = $GuestExchangeItemNode.SelectSingleNode("/INSTANCE/PROPERTY[@NAME='Data']/VALUE/child::text()").Value
        }
    }

    Write-TestSuiteInfo "Exchange data value is $ExchangeItemDataValue."
    return $ExchangeItemDataValue
}

#------------------------------------------------------------------------------------------
# Wait for a particular ISOState for a virtual machine
# The Controller.ps1 script in the virtual machine will control the ISOState change:
# "insert" - CDROM is inserted
# "eject" - CDROM is ejected
# "installpostscriptready" - ready to install post script
# "completed" - the script controller.ps1 is completed
# "failed" - internal error occurred within VM
#------------------------------------------------------------------------------------------
Function Wait-TestSuiteISOState {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        $Vm,
        [string]$State = "eject",
        [int]$TimeoutInSeconds = 3600,
        [int]$SleepTimePerIteration = 5
    )

    Process {

        Write-TestSuiteInfo "Wait for ISOState to be changed to $State in $($Vm.hypervname)."
        $isReset = $false
        $VmISOState = $null
        $TimeLeft = $TimeoutInSeconds
        do {
            $VmISOState = Get-TestSuiteVmExchangeData -VmName $Vm.hypervname -ExchangeItemName "ISOState_$State"
            if ($VmISOState -eq "failed") {
                Write-TestSuiteError "Internal error occurred while configuring $($Vm.hypervname)." -Exit
            }
            #before timeout, reset the vm and wait the state 
            if ($isReset -eq $false -and $TimeLeft -le $TimeoutInSeconds / 3) {
                $isReset = $true
                Write-TestSuiteStep "Restart VM:$($Vm.hypervname)."
                Restart-VM $Vm.hypervname -Force
            }
            Wait-TestSuiteActivityComplete -ActivityName "ISOState to be changed to $State in $($Vm.hypervname)" -TimeoutInSeconds $SleepTimePerIteration
        } while (($VmISOState -ne $State) -and ($TimeLeft -= $SleepTimePerIteration))
        if (!$TimeLeft -and ($VmISOState -ne $State)) {
            if ($VmISOState -eq "insert") {
                Write-TestSuiteStep "Eject the ISO from DVD drive."
                $VmDvdDrive = Get-VMDvdDrive -VMName $Vm.hypervname | Set-VMDvdDrive -Path $null
            }
            Write-TestSuiteError "Timeout reached before ISOState can be changed to $State within $TimeoutInSeconds seconds. ISO State is $VmISOState." -Exit            
        }
    }
}

#------------------------------------------------------------------------------------------
# Deploy all the virtual machines of this test suite
# It creates and starts all the VMs
# It creates and mounts all the ISOs
# It calls Mount-TestSuiteISO for the 1st time, and enters the Controller.ps1 phase 1
#------------------------------------------------------------------------------------------
Function Deploy-TestSuiteVirtualMachines {

    Write-TestSuiteInfo "Deploy virtual machines for this test suite."
    Write-TestSuiteInfo "It creates all the VMs, creates all the ISOs and mounts them, and then starts all the VMs."

    # Replace $Script:VmDirPath and $Script:IsoDirPath if EnvironmentName element exists
    if ($Script:Setup.lab.core.SelectSingleNode("./EnvironmentName") -ne $null) {
        $EnvironmentName = $Script:Setup.lab.core.EnvironmentName
        $Script:VmDirPath = "$InvocationPath\..\VM\$TestSuiteName\$EnvironmentName"
        $Script:IsoDirPath = "$InvocationPath\..\VM\$TestSuiteName\$EnvironmentName\ISO"
    }

    Write-TestSuiteInfo "All VM related files are stored in directory: $Script:VmDirPath."

    Write-TestSuiteStep "Create the VM directory to store all the VMs of this test suite."
    if (!(Test-Path -Path $Script:VmDirPath)) {
        Write-TestSuiteWarning "Create $Script:VmDirPath."
        md $Script:VmDirPath
    }
    else {
        Write-TestSuiteWarning "Create $Script:VmDirPath has already been created."
    }    

    Write-TestSuiteStep "Copy the XML configuration file $Script:XmlFileFullPath to the VM directory:"
    Copy-Item $Script:XmlFileFullPath $Script:VmDirPath -force

    Write-TestSuiteStep "Create the ISO directory to store all the ISO files of this test suite."

    if (!(Test-Path -Path $Script:IsoDirPath)) {
        Write-TestSuiteWarning "Create $Script:IsoDirPath has already been created. Remove it recursively."
        md $Script:IsoDirPath
    }
    else {
        Write-TestSuiteWarning "Create $Script:IsoDirPath has already been created."
    }
    
    # These actions will take some time, so start them early
    Write-TestSuiteStep "Start creating VHDs and ISOs for all the VMs of this test suite."
    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort -Property installorder)) {
        $Vm | Create-TestSuiteISO
        $Vm | Create-TestSuiteVHD
    }

    Write-TestSuiteInfo "Start creating VMs for all the VMs of this test suite."
    $Script:Setup.lab.servers.vm | Sort -Property installorder | Create-TestSuiteVM

    # Mount the ISO, this will enter the controller.ps1, phase 1
    Write-TestSuiteInfo "Start mounting ISOs for all the VMs of this test suite."
    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort -Property installorder)) {
        $Vm | Mount-TestSuiteISO
    }

    # Start the VM
    Write-TestSuiteStep "Start all the VMs."
    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort -Property installorder)) {
        Start-VM -Name $Vm.hypervname
    }

    # Configure Linux VM
    ConfigureLinuxVM

    # Configure NATSwitch Windows VM IP Address
    ConfigureWindowsVMIPForNATSwitch

    $windowsVmList = Get-VMListByOSType -osType "Windows"
    $waitSeconds = 900
    if ($null -eq $windowsVmList) {
        $waitSeconds = 300
    }    
    
    # Wait for VM to complete Sysprep before it goes on to the next step
    Write-TestSuiteStep "Wait for VM to complete Sysprep. It will take about $waitSeconds seconds... "
    Wait-TestSuiteActivityComplete -ActivityName "Sysprep" -TimeoutInSeconds $waitSeconds
}

Function ConfigureLinuxVM {

    $linuxVmList = Get-VMListByOSType -osType "Linux"
    if ($null -eq $linuxVmList) {
        return
    }
 
    Write-TestSuiteInfo "Set the IP address and update the host name of the Linux VM."

    # Sleep 20 seconds to wait the VM get the NAT IP address
    Write-TestSuiteInfo "Sleep 20 seconds to wait the Linux VM get the NAT IP address..."
    Start-Sleep -Seconds 20

    $rediectStandardConfirmInput = "C:\auto_generate_confirm_text.txt"
    Write-Output "Y" > $rediectStandardConfirmInput

    foreach ($Vm in $linuxVmList) {
        $vmName = $Vm.hypervname

        Write-TestSuiteInfo "Get Linux VM NAT IP address"
        $currentLinuxVMIP = Get-LinuxVMPublicIP -VM $Vm
        Write-TestSuiteInfo "Wait Linux VM NAT IP address to be stable."
        Start-Sleep -Seconds 10
        # Create trust connection with Linux OS
        Create-TrustConnection -VmIP $currentLinuxVMIP

        $versionFileName = "os-release"
        $versionFilePath = "/etc/$versionFileName"
        $destinationFilePath = "$PSScriptRoot\$vmName$versionFileName"

        # Get current Linux version
        Execute-PscpCopyLinuxFileToWindowsCommand -VmIP $currentLinuxVMIP `
            -SourceFilePath $versionFilePath `
            -DestinationFilePath $destinationFilePath `
            -ShCommandKey "Get_OS_Version"
   
        # Wait the Linux version file copy to local over
        Start-Sleep -Seconds 10

        $linuxVersion = GetLinuxVersion -IPAddress $currentLinuxVMIP -VMName $vmName -destinationFilePath $destinationFilePath

        Write-TestSuiteInfo "Current Linux version = $linuxVersion"
        if ($linuxVersion -ne "ubuntu18.04" -and $linuxVersion -ne "ubuntu20.04") {
            Write-TestSuiteError "Current script just support Ubuntu 18.04 and Ubuntu 20.04" -Exit
        }

        Write-TestSuiteInfo "Wait 20 seconds to make sure the /etc/netplan/01-network-manager-all.yaml is generated over"
        Start-Sleep -Seconds 20 

        # Update IP Address
        $ipIndex = 0        
        [array]$ipList = $VM.IP
        [array]$subnetList = $VM.subnet
        [array]$gatewayList = $VM.gateway
        [array]$dnsList = $VM.dns

        foreach ($ip in $ipList) {
            $ipAddress = $ipList[$ipIndex]
            $vmIPMask = Get-IPCount -subnet $subnetList[$ipIndex]
            $vmIPAddress = "$ipAddress/$vmIPMask"

            $vmGateWay = $gatewayList[$ipIndex]
            $vmDNS = $dnsList[$ipIndex]

            $localVMIPAddressYamlFile = "C:\0$ipIndex-network-manager-all.yaml"
            $remoteIPAddressConfiguredYamlFile = "/etc/netplan/0$ipIndex-network-manager-all.yaml"
            $networkEthIndex = "eth$ipIndex"

            UpdateIPAddress -localVMIPAddressYamlFile $localVMIPAddressYamlFile `
                -remoteIPAddressConfiguredYamlFile $remoteIPAddressConfiguredYamlFile `
                -networkEthIndex $networkEthIndex `
                -ipAddress $vmIPAddress `
                -gateWay $vmGateWay `
                -dns $vmDNS `
                -linuxExternalIPAddress $currentLinuxVMIP

            $ipIndex++
        }

        # Update host name   
        Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
            -ShCommand "hostnamectl set-hostname $vmName" `
            -ShCommandKey "set-hostname"

        # Apply the updated IP address configuration
        Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
            -ShCommand "sudo netplan apply" `
            -ShCommandKey "netplan"

        if (![string]::IsNullOrEmpty($Vm.tools)) {
            if (![string]::IsNullOrEmpty($Vm.tools.tool)) {
                if ($Vm.tools.tool.Count -gt 0) {
                    Write-TestSuiteInfo "There are $($Vm.tools.tool.Count) tools to be installed."
                }

                foreach ($Tool in $Vm.tools.tool) {
                    if ($Tool.HasAttribute("ZipName")) {
                        $myZipFile = $Tool.ZipName
                        Write-TestSuiteStep "Find the $myZipFile from the ..\Tools folder."

                        $zipTargetFolder = $Tool.targetFolder
                        if ($zipTargetFolder -ne $null) {
                            Write-TestSuiteStep "Create the $zipTargetFolder on Linux."
                            Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
                                -ShCommand "mkdir -p $zipTargetFolder" `
                                -ShCommandKey "createSSHFolder" 

                            Write-TestSuiteStep "Find the $($Tool.name) from the ..\Tools folder."
                            $myZipFilePath = $($Script:ToolLibPath + "\$($Tool.name)\" + $myZipFile)
                            $myDestZipFilePath = "$zipTargetFolder/$myZipFile"
                            if (Test-Path -Path $myZipFilePath) {
                                Write-TestSuiteSuccess "$myZipFile found in $($Script:ToolLibPath) folder. Copy it to Linux..."
                                Execute-PscpCopyWindowsFileToLinuxCommand -VmIP $currentLinuxVMIP `
                                    -SourceFilePath $myZipFilePath `
                                    -DestinationFilePath $myDestZipFilePath
                                Write-TestSuiteInfo "Complete to copy $myZipFilePath to Linux $myDestZipFilePath "
                            }
                            else {
                                Write-TestSuiteWarning "$myZipFile file not found."
                            }

                            Write-TestSuiteInfo "Sleep 35 seconds to wait the zip file upload over..."
                            Start-Sleep -Seconds 35
     
                            # Unzip the file on Linux
                            Write-TestSuiteStep "Unzip the zip file on Linux."
                            Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
                                -ShCommand "cd $zipTargetFolder;unzip -o $myZipFile" `
                                -ShCommandKey "unzipFile"

                            $zipInstallScript = $Tool.installScript
                            if ($zipInstallScript -ne $null) {
                                Write-TestSuiteStep "install script on Linux."
                                Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
                                    -ShCommand "$zipInstallScript" `
                                    -ShCommandKey "zipInstallScript"
                            }
                        }
                    }
                }
            }
        }

        Write-TestSuiteStep "Handle Linux Installscript Script..."
        foreach ($InstallScript in [Array]$Vm.installscript.Split(";")) {
            if (![string]::IsNullOrWhiteSpace($InstallScript)) {
                if (Test-Path -Path $($Script:ScriptLibPath + "\" + $InstallScript)) {
                    Write-TestSuiteSuccess "$InstallScript found in $Script:ScriptLibPath InstallScript folder. Execute it..."
                }
                else {
                    Write-TestSuiteError "$InstallScript is NOT found in $Script:ScriptLibPath InstallScript folder"
                }

                Write-TestSuiteSuccess "Start to execute $InstallScript install script... in $currentLinuxVMIP"   
                & $Script:ScriptLibPath\$InstallScript -Vm $Vm -Setup $Script:Setup -TestSuiteName $TestSuiteName -EnvironmentName $EnvironmentName
            }
        }

        # Restart VM to load configuration file
        Write-TestSuiteInfo "Restart the Linux VM $currentLinuxVMIP to apply all the configuration"

        Restart-VM -Name $vmName -Force
    }
}

function Set-WinVM-NATIP {
    param(
        $VMName,
        $IPAddress,
        $Subnets,
        $GateWays,
        [array]$DNSServers
    )

    $VMManServ = Get-WmiObject -Namespace root\virtualization\v2 -Class Msvm_VirtualSystemManagementService

    $vm = Get-WmiObject -Namespace 'root\virtualization\v2' -Class 'Msvm_ComputerSystem' | Where-Object { $_.ElementName -eq $VMName }

    $vmSettings = $vm.GetRelated('Msvm_VirtualSystemSettingData') | Where-Object { $_.VirtualSystemType -eq 'Microsoft:Hyper-V:System:Realized' } 

    $nwAdapters = $vmSettings.GetRelated('Msvm_SyntheticEthernetPortSettingData') 

    $ipstuff = $nwAdapters.getrelated('Msvm_GuestNetworkAdapterConfiguration')

    $ipstuff.DHCPEnabled = $false
    $ipstuff.DNSServers = $DNSServers
    $ipstuff.IPAddresses = $IPAddress
    $ipstuff.Subnets = $Subnets
    $ipstuff.DefaultGateways = $GateWays
    $ipstuff.ProtocolIFType = 4096

    $setIP = $VMManServ.SetGuestNetworkAdapterConfiguration($VM, $ipstuff.GetText(1))
}

Function ConfigureWindowsVMIPForNATSwitch {

    $windowsVmList = Get-VMListByOSType -osType "Windows"
    if ($null -eq $windowsVmList) {
        return
    }
 
    Write-TestSuiteInfo "Set the IP address and update the host name of the Linux VM."
    foreach ($Vm in $windowsVmList) {
        if ($Vm.vnetExternal -eq "NATSwitch") {
            $vmName = $Vm.hypervname
            Write-TestSuiteInfo "Set Windows VM $vmName NATSwitch IP address."
            $requestIP = Get-AvailableNATIP
            Set-WinVM-NATIP -VMName $vmName -IPAddress "$requestIP" -Subnets "255.255.255.0" -GateWays "192.168.0.1" -DNSServers @("192.168.0.1", "10.50.50.50")
        }
    }
}

Function UpdateIPAddress(
    $localVMIPAddressYamlFile,
    $remoteIPAddressConfiguredYamlFile, 
    $networkEthIndex,
    $ipAddress, $gateWay, $dns, 
    $linuxExternalIPAddress) {

    Write-Output "network:" > $localVMIPAddressYamlFile
    Write-Output "    ethernets:" >> $localVMIPAddressYamlFile
    Write-Output "        ${networkEthIndex}:" >> $localVMIPAddressYamlFile
    Write-Output "            addresses: [$ipAddress]" >> $localVMIPAddressYamlFile
    Write-Output "            dhcp4: no" >> $localVMIPAddressYamlFile
    Write-Output "            gateway4: $gateWay" >> $localVMIPAddressYamlFile
    Write-Output "            nameservers:" >> $localVMIPAddressYamlFile
    Write-Output "                addresses: [$dns]" >> $localVMIPAddressYamlFile
    Write-Output "                search: []" >> $localVMIPAddressYamlFile
    Write-Output "            optional: true" >> $localVMIPAddressYamlFile
    Write-Output "" >> $localVMIPAddressYamlFile
    Write-Output "    version: 2" >> $localVMIPAddressYamlFile
    Write-Output "    renderer: NetworkManager" >> $localVMIPAddressYamlFile

    # Copy File to Linux to override the IP configuration file
    $tmpRemoteIPAddressConfiguredYamlFile = "/tmp/tmp.yaml"
    Write-TestSuiteInfo "Copy $localVMIPAddressYamlFile to Linux $tmpRemoteIPAddressConfiguredYamlFile to override the IP configuration file"

    Execute-PscpCopyWindowsFileToLinuxCommand -VmIP $linuxExternalIPAddress -SourceFilePath $localVMIPAddressYamlFile -DestinationFilePath $tmpRemoteIPAddressConfiguredYamlFile
    
    Write-TestSuiteInfo "Complete to copy $localVMIPAddressYamlFile to Linux $tmpRemoteIPAddressConfiguredYamlFile "
    
    # Remove the ip configuration file after upload over
    Remove-Item $localVMIPAddressYamlFile

    Write-TestSuiteInfo "Sleep 30 seconds to wait the file upload over..."
    Start-Sleep -Seconds 30
     
    # Format the file from DOS format to Unix format
    Execute-PlinkShCommand -VmIP $linuxExternalIPAddress `
        -ShCommand "dos2unix $tmpRemoteIPAddressConfiguredYamlFile;mv $tmpRemoteIPAddressConfiguredYamlFile $remoteIPAddressConfiguredYamlFile" `
        -ShCommandKey "dos2unix" `

}

Function GetLinuxVersion ($IPAddress, $vmName, $destinationFilePath) {

    $versionContent = Get-Content $destinationFilePath   

    $resultVersionName = ""
    foreach ($versionItemLine in $versionContent) {    
        if ($versionItemLine.Contains("ID=")) {
            $resultVersionName += $versionItemLine.Split("=")[1].Replace('"', '')
        }
        elseif ($versionItemLine.Contains("VERSION_ID=")) {
            $resultVersionName += $versionItemLine.Split("=")[1].Replace('"', '')
        }
    }

    Write-Host "Linux OS VersionName = "$resultVersionName

    return $resultVersionName.Trim()
}

Function Get-IPCount {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
        [string]$subnet
    )

    $binarySubnet = -join ($subnet.Split('.') | ForEach-Object { [System.Convert]::ToString($_, 2).PadLeft(8, '0') })
    $binarySubnet = $binarySubnet.toCharArray()
    $ipCount = 0
    foreach ($currentChar in $binarySubnet)	{
        if ($currentChar -eq "1") {
            $ipCount++
        }
    }

    return $ipCount
}

#------------------------------------------------------------------------------------------
# Get the virtual machine list by the operation system type
#------------------------------------------------------------------------------------------
Function Get-VMListByOSType {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
        [string]$osType
    )
    
    if ($osType -eq "Linux") {
        # Return Linux VM
        return $Script:Setup.lab.servers.vm | Where-Object { $_.os -like $osType }
    }
    elseif ($osType -eq "All") {
        # Return all OS VM
        return $Script:Setup.lab.servers.vm
    }
    else {
        # Return Other OS VM(include Windows or any OS that does not configure the 'os' node value)
        return $Script:Setup.lab.servers.vm | Where-Object { $_.os -like $osType -or [string]::IsNullOrWhiteSpace($_.os) }
    }
}

#------------------------------------------------------------------------------------------
# Install Visual Studio for this test suite
# It waits for Controller.ps1 phase 1 to finish and eject the ISO
# It then calls Mount-TestSuiteISO for the 2nd time, and enters the Controller.ps1 phase 2
#------------------------------------------------------------------------------------------
Function Install-VisualStudioForTestSuite {

    Write-TestSuiteInfo "Install Visual Studio for the test suite."

    foreach ($Vm in ($Script:Setup.lab.servers.vm | sort -Property installorder)) {
        if ($Vm.os -ne "Linux") {
            if (($Vm.SelectSingleNode("./postiso") -ne $null) -and (($Vm.postiso -eq "VS2010.iso") -or ($Vm.postiso -eq "VS2012.iso"))) {
                Write-TestSuiteStep "Wait for $($Vm.hypervname) to eject the DVD drive, then mount Visual Studio ISO to install Visual Studio."
                $Vm | Wait-TestSuiteISOState -State "eject"
                
                Write-TestSuiteStep "Mount Visual Studio installation ISO to VM and Controller.ps1 inside the VM will install it."
                $Vm | Mount-TestSuiteISO -VmIsoFullPath $($Script:MediaLibPath + "\" + $Vm.postiso)
            }
        }
    }
}

#------------------------------------------------------------------------------------------
# Install Applications for this test suite
# It waits for Controller.ps1 phase 1, or Install Visual Studio to finish and eject the ISO
# It then calls Mount-TestSuiteISO for the 2nd time, and enters the Controller.ps1 phase 2
#------------------------------------------------------------------------------------------
Function Install-ApplicationsForTestSuite {
    
    Write-TestSuiteInfo "Install Applications for the test suite."

    foreach ($Vm in ($Script:Setup.lab.servers.vm | sort -Property installorder)) {
        if ($Vm.os -ne "Linux") {
            if ($Vm.SelectSingleNode("./installiso") -ne $null) {
                Write-TestSuiteStep "Wait for $($Vm.hypervname) to eject the DVD drive, then mount Applications ISO to install applications."
                $Vm | Wait-TestSuiteISOState -State "eject"
                
                Write-TestSuiteStep "Mount Applications installation ISO to VM and Controller.ps1 inside the VM will install it."
                $Vm | Mount-TestSuiteISO -VmIsoFullPath $($Script:MediaLibPath + "\" + $Vm.installiso)
            }
        }
    }
}

#------------------------------------------------------------------------------------------
# Install post configures for this test suite
# It waits for Controller.ps1 phase 2 to finish and eject the ISO
# It then calls Mount-TestSuiteISO for the 3rd time, and enters the Controller.ps1 phase 3
#------------------------------------------------------------------------------------------
Function Install-TestSuitePostScripts {
    
    Write-TestSuiteInfo "Install Post Scripts for the test suite."

    foreach ($Vm in ($Script:Setup.lab.servers.vm | sort -Property installorder)) {
        
        if ($VM.os -ne "Linux") {
            Write-TestSuiteStep "Wait for $($Vm.hypervname) to be ready to install post script, then mount Test Suite ISO to trigger controller.ps1 for the installation."
            $Vm | Wait-TestSuiteISOState -State "installpostscriptready" -TimeoutInSeconds 7200
            $Vm | Mount-TestSuiteISO
            
            Write-TestSuiteStep "Decide whether to wait for the post script installation to be completed or not."
            if (![string]::IsNullOrEmpty($Vm.skipwaitingforpostscript) -and ($Vm.skipwaitingforpostscript -eq "false")) {
                Write-TestSuiteInfo "Wait for $($Vm.hypervname) to complete the post script installation."
                # By default, wait 5 minutes for post script to complete
                Wait-TestSuiteActivityComplete -ActivityName "Install Post Scripts" -TimeoutInSeconds 300
                $Vm | Wait-TestSuiteISOState -State "completed"
            }
        }
        else {
            Write-TestSuiteStep "Wait for $($Vm.hypervname) to be ready to install post script"
            if ($Vm.postscript -ne $null) {
                foreach ($postscript in [Array]$Vm.postscript.Split(";")) {
                    if (![string]::IsNullOrWhiteSpace($postscript)) {
                        if (Test-Path -Path $($Script:ScriptLibPath + "\" + $postscript)) {
                            Write-TestSuiteSuccess "$postscript found in $Script:ScriptLibPath PostScript folder. Execute it..."
                        }
                        else {
                            Write-TestSuiteError "$postscript is NOT found in $Script:ScriptLibPath PostScript folder"
                        }
        
                        Write-TestSuiteSuccess "Start to execute post script... in $($Vm.hypervname)"                    
                        & $Script:ScriptLibPath\$postscript -Vm $Vm -Setup $Script:Setup -TestSuiteName $TestSuiteName -EnvironmentName $EnvironmentName
                    }
                }
            }
        }
    }
}

#------------------------------------------------------------------------------------------
# Complete the test suite setup
#------------------------------------------------------------------------------------------
Function Complete-TestSuiteSetup {

    Write-TestSuiteInfo "Complete the setup for this test suite."

    Stop-Transcript

    exit 0
}

#==========================================================================================
# Main Script Body
#==========================================================================================

#==========================================================================================
# Elevate shell user to administrator
#==========================================================================================
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                       Elevate Shell                        "
Write-TestSuiteInfo "============================================================"
Elevate-Shell

#------------------------------------------------------------------------------------------
# Start logging
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                       Start Logging                        "
Write-TestSuiteInfo "============================================================"
Start-TestSuiteLog

#------------------------------------------------------------------------------------------
# Print out input parameters and global variables
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                      Input parameters                      "
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "`$TestSuiteName:      $TestSuiteName"
Write-TestSuiteInfo "`$EnvironmentName:    $EnvironmentName"
Write-TestSuiteInfo "`$ServerDiskName:     $ServerDiskName"
Write-TestSuiteInfo "`$ServerAnswerFile:   $ServerAnswerFile"
Write-TestSuiteInfo "`$ClientDiskName:     $ClientDiskName"
Write-TestSuiteInfo "`$ClientAnswerFile:   $ClientAnswerFile"
Write-TestSuiteInfo "`$DynamicDisk:        $DynamicDisk"
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                      Global variables                      "
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "[Script Information]"
Write-TestSuiteInfo "`t`$InitialInvocation:  $InitialInvocation"
Write-TestSuiteInfo "`t`$InvocationFullPath: $InvocationFullPath"
Write-TestSuiteInfo "`t`$InvocationName:     $InvocationName"
Write-TestSuiteInfo "`t`$InvocationPath:     $InvocationPath"
Write-TestSuiteInfo "`t`$LogFileName:        $LogFileName"
Write-TestSuiteInfo "`t`$LogFilePath:        $LogFilePath"
Write-TestSuiteInfo "[Host Information]"
Write-TestSuiteInfo "`t`$HostOsBuildNumber:  $HostOsBuildNumber"
Write-TestSuiteInfo "[Lab Information]"
Write-TestSuiteInfo "`t`$XmlLibPath:         $XmlLibPath"
Write-TestSuiteInfo "`t`$VhdLibPath:         $VhdLibPath"
Write-TestSuiteInfo "`t`$AnswerFileLibPath:  $AnswerFileLibPath"
Write-TestSuiteInfo "`t`$InstallLibPath:     $InstallLibPath"
Write-TestSuiteInfo "`t`$PostScriptLibPath:  $PostScriptLibPath"
Write-TestSuiteInfo "`t`$MediaLibPath:       $MediaLibPath"
Write-TestSuiteInfo "`t`$ScriptLibPath:      $ScriptLibPath"
Write-TestSuiteInfo "`t`$ToolLibPath:        $ToolLibPath"
Write-TestSuiteInfo "`t`$VsLicensePath:      $VsLicensePath" 
Write-TestSuiteInfo "[Test Suite Information]"
Write-TestSuiteInfo "`t`$XmlFileFullPath:    $XmlFileFullPath"
Write-TestSuiteInfo "`t`$XmlFileName:        $XmlFileName"
Write-TestSuiteInfo "`t`$XmlFilePath:        $XmlFilePath"
Write-TestSuiteInfo "`t`$VmDirPath:          $VmDirPath"
Write-TestSuiteInfo "`t`$IsoDirPath:         $IsoDirPath"
Write-TestSuiteInfo "============================================================"

#------------------------------------------------------------------------------------------
# Check if the host machine has setup all the prerequisites, e.g. Hyper-V Management, etc.
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                 Check Host Prerequisites                   "
Write-TestSuiteInfo "============================================================"
Check-HostPrerequisites

#------------------------------------------------------------------------------------------
# Read, parse and fix the XML configuration file
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Read and Fix XML Configuration File             "
Write-TestSuiteInfo "============================================================"
Read-TestSuiteXml
Fix-TestSuiteXml

#------------------------------------------------------------------------------------------
# Check if the host machine has duplicate ip addresses or virtual machines
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                  Check Host Duplicities                    "
Write-TestSuiteInfo "============================================================"
Check-DuplicateVmNames

#------------------------------------------------------------------------------------------
# Check if the host machine has enough memory to create virtual machines
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                  Check Host Capacities                     "
Write-TestSuiteInfo "============================================================"
Check-HostMemory

#------------------------------------------------------------------------------------------
# Deploy virtual network switches
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "              Deploy Virtual Network Switches               "
Write-TestSuiteInfo "============================================================"
Deploy-TestSuiteVirtualNetworkSwitches

#------------------------------------------------------------------------------------------
# Deploy virtual machines
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"  
Write-TestSuiteInfo "                  Deploy Virtual Machines                   "
Write-TestSuiteInfo "============================================================"
Deploy-TestSuiteVirtualMachines

#------------------------------------------------------------------------------------------
# Install Visual Studio and applications ahead of time to save configuration time
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "           Install Visual Studio and Applications           "
Write-TestSuiteInfo "============================================================"
Install-VisualStudioForTestSuite
Install-ApplicationsForTestSuite

#------------------------------------------------------------------------------------------
# Configure virtual machines by installing the post scripts
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                   Install Post Scripts                     "
Write-TestSuiteInfo "============================================================"
Install-TestSuitePostScripts
Checkpoint-AllVMs -CheckpointName "Setup Done"

#------------------------------------------------------------------------------------------
# Complete setup virtual machines
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                      Complete Setup                        "
Write-TestSuiteInfo "============================================================"
Complete-TestSuiteSetup