###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Cleanup-TestSuiteEnvironment.ps1
## Purpose:        Prepare environment for setting up a specified Test Suite.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2012, Windows Server 2012 R2, Windows Server 2016, and later.
##
###########################################################################################

#------------------------------------------------------------------------------------------
# Parameters:
# TestSuiteName: the name of the test suite under construstion
# EnvironmentName: the name of the XML configuration file, indicating which environment you want to deploy
#------------------------------------------------------------------------------------------
Param
(
    # The name of the Test Suite, only used to fetch XML configuration file and specify log and vm folder
    # Therefore, if multiple environments are to be deployed:
    # 1. Specify <$TestSuiteName> as "ADFamily1", "ADFamily2", ...
    # 2. Change the XML configuration <file name> and the test suite <folder name> accordingly
	[Parameter(Mandatory=$true)]
    [string]$TestSuiteName,
    # The name of the XML file, indicating which environment you want to configure
	[Parameter(Mandatory=$true)]
    [string]$EnvironmentName
)

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
#   CustomLibPath       File Path of the custom files
#   InstallLibPath      File Path of all the install scripts
#   PostScriptLibPath   File Path of the post scripts
# [Test Suite Information]
#   XmlFileFullPath:    Full Path of the XML configuration file
#   XmlFileName:        File Name of the XML configuration file
#   XmlFilePath:        File Path of the XML configuration file
#   VmDirPath:          The directory to store all the virtual machines for this test suite
#   IsoDirPath:         The directory to store all the iso files for this test suite
#------------------------------------------------------------------------------------------
$InitialInvocation       = $MyInvocation
$InvocationFullPath      = $InitialInvocation.MyCommand.Definition
$InvocationName          = [System.IO.Path]::GetFileName($InvocationFullPath)
$InvocationPath          = Split-Path -Parent $InvocationFullPath
$LogFileName             = "$InvocationName.log"
$LogFilePath             = "$InvocationPath\..\TestResults\$TestSuiteName"
$HostOsBuildNumber       = "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
$XmlLibPath              = "$InvocationPath\XML"
$XmlFileFullPath         = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML\$EnvironmentName"
$XmlFileName             = $EnvironmentName
$XmlFilePath             = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML"
$VmDirPath               = "$InvocationPath\..\VM\$TestSuiteName\Default"

#==========================================================================================
# Function Definition
#==========================================================================================

#------------------------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#------------------------------------------------------------------------------------------
Function Start-TestSuiteLog {

    try {
        # Stop the previous transcript
        Stop-Transcript -ErrorAction SilentlyContinue
    }
    catch [System.InvalidOperationException] {}

    # Create log directory if not exist
    if (!(Test-Path -Path $Script:LogFilePath)) {
        md $Script:LogFilePath
    }

    # Start new transcript
    Start-Transcript -Path "$Script:LogFilePath\$Script:InvocationName.log" -Append -Force
}

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
    if([Double]$Script:HostOsBuildNumber -eq [Double]"6.3") {
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
        if ($Tick -lt $TimeoutInSeconds) { Start-Sleep 1 }
    }
    Write-Progress -Activity "Wait for $ActivityName ..." -Completed
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
        $ArgumentList = "-NoExit -Command " + $Script:InitialInvocation.Line
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
    $Files | Get-XmlFileContents | Format-Table -Property LabId,FileName,VMs,Memory,Description -AutoSize

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
    if(!(Test-Path -Path $Script:XmlFileFullPath)) {
        Write-TestSuiteWarning "$Script:XmlFileFullPath file not found. Choose one from the prepared XML library."
        Choose-XmlFromLib
    }
    else {
        Write-TestSuiteSuccess "$Script:XmlFileFullPath file found."
    }

    # Read contents from the XML file
    Write-TestSuiteStep "Read contents from the XML configuration file."
    [Xml]$Script:Setup = Get-Content $Script:XmlFileFullPath
    if($Script:Setup -eq $null) {
        Write-TestSuiteError "$Script:XmlFileFullPath file is not a valid xml configuration file." -Exit
    }
    else {
        $Script:Setup | Format-TestSuiteXml -Indent 4
    }
}

#------------------------------------------------------------------------------------------
# Remove all the virtual network switches for this test suite
#------------------------------------------------------------------------------------------
Function Remove-TestSuiteVirtualNetworkSwitches {

    Write-TestSuiteInfo "Remove virtual network switches for this test suite."

    foreach ($Vnet in $Script:Setup.lab.network.vnet) {
    
        Write-TestSuiteStep "Get the virtual switch's name."
        Write-TestSuiteInfo $Vnet.name

        Write-TestSuiteStep "Get the virtual switch's type."
        # Virtual switch type, currently only supports internal and external
        # If not specified in the XML configuration file, set as default - "internal"
        $NetworkType = $Vnet.Get_ChildNodes() | Where {$_.Name -eq "networktype"}
        if($NetworkType -eq $null) {
            $NetworkType = "Internal"
        }
        else {
            $NetworkType = $Vnet.networktype
        }
        Write-TestSuiteInfo $NetworkType

        $isClean = $Vnet.Get_ChildNodes() | Where {$_.Name -eq "iscleanup"}
        if($isClean -eq $null) {
            $isClean = 'true'
        }
        else {
            $isClean = $Vnet.iscleanup
        }

        Write-TestSuiteInfo "is clean virtual network switches: $isClean"
        if($isClean -eq 'true')
        {
            switch ($NetworkType) {
                "Internal" {
                    # Remove the "internal" virtual network switch if it exists, "external" will not be removed because it will cause network connectivity problems
                    Write-TestSuiteStep "Remove the internal virtual switch if it exists."
                    Get-VMSwitch -Name $Vnet.name -SwitchType Internal -ErrorAction SilentlyContinue | Remove-VmSwitch -Force -ErrorAction SilentlyContinue
                }
                "External" {
                    # Get the virtual network switch for "external" type if it exists
                    Write-TestSuiteStep "Do not remove the existing external virtual switch."
                }
                default {
                    Write-TestSuiteError $("Network type - $NetworkType not supported.") -Exit
                }
            }
        }
    }
}

#------------------------------------------------------------------------------------------
# Remove all the virtual machines of this test suite
#------------------------------------------------------------------------------------------
Function Remove-TestSuiteVirtualMachines {

    Write-TestSuiteInfo "Remove virtual machines for this test suite."
    Write-TestSuiteInfo "It removes all the VMs, removes all the ISOs, and all the VMs metadata."

	# Replace $Script:VmDirPath if EnvironmentName element exists
	if($Script:Setup.lab.core.SelectSingleNode("./EnvironmentName") -ne $null) {
		$EnvironmentName = $Script:Setup.lab.core.EnvironmentName
		$Script:VmDirPath = "$InvocationPath\..\VM\$TestSuiteName\$EnvironmentName"
	}
	
	Write-TestSuiteInfo "All VM related files are stored in directory: $Script:VmDirPath."

    # Stop and remove all the VMs from Hyper-V Manager
    Write-TestSuiteStep "Stop and remove all the VMs from Hyper-V Manager."
    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort -Property installorder)) {
        Write-TestSuiteStep "Get all the VM names on the host:"
        $currVM = (Get-VM  | Select-Object -Property Name) | where {$_.Name -eq $Vm.hypervname}
        if($currVM) {
            Stop-VM -Name $Vm.hypervname -TurnOff -Force
            Remove-VM -Name $Vm.hypervname -Force
        }
        
        Write-TestSuiteStep "Start to remove VHDs all the VMs of this test suite."
        if(Test-Path ([IO.Path]::Combine($Script:VmDirPath, $vm.hypervname)))
        {
            Remove-Item -Path ([IO.Path]::Combine($Script:VmDirPath, $vm.hypervname)) -Recurse -Force -Confirm:$false -ErrorAction SilentlyContinue
        }   

        Write-TestSuiteStep "Start to remove ISOs for all the VMs of this test suite." 
        if(Test-Path ([IO.Path]::Combine($Script:VmDirPath,  "ISO", $vm.hypervname)))
        {
            Remove-Item -Path ([IO.Path]::Combine($Script:VmDirPath,  "ISO", $vm.hypervname)) -Recurse -Force -Confirm:$false -ErrorAction SilentlyContinue
        }
        if(Test-Path ([IO.Path]::Combine($Script:VmDirPath,  "ISO", $vm.hypervname+ ".iso")))
        {
            Remove-Item -Path ([IO.Path]::Combine($Script:VmDirPath,  "ISO", $vm.hypervname+ ".iso")) -Force -Confirm:$false -ErrorAction SilentlyContinue
        }
    }
}

#------------------------------------------------------------------------------------------
# Remove all the virtual machines of this test suite
#------------------------------------------------------------------------------------------
Function Remove-TestResults {
    # Remove logs under TestResults directory of current test suite
    Write-TestSuiteStep "Remove logs under TestResults directory of current test suite."
    if(Test-Path -Path $Script:LogFilePath ) {
        Write-TestSuiteInfo "$Script:LogFilePath was found. Remove it recursively."
        Remove-Item -Path $Script:LogFilePath -Recurse -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-TestSuiteWarning "$Script:LogFilePath does not exist on the host."
    }
}
#------------------------------------------------------------------------------------------
# Complete the test suite cleanup
#------------------------------------------------------------------------------------------
Function Complete-TestSuiteCleanup {

    Write-TestSuiteInfo "Complete the cleanup for this test suite."

    Wait-TestSuiteActivityComplete -ActivityName "Test Suite Cool Down" -TimeoutInSeconds 30

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
# Remove testresults for current test suite
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "              Remove Test Results                           "
Write-TestSuiteInfo "============================================================"
Remove-TestResults

#------------------------------------------------------------------------------------------
# Start logging
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                       Start Logging                        "
Write-TestSuiteInfo "============================================================"
Start-TestSuiteLog

#------------------------------------------------------------------------------------------
# Print out input parameters
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                      Global variables                      "
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "`$InitialInvocation:  $InitialInvocation"
Write-TestSuiteInfo "`$InvocationFullPath: $InvocationFullPath"
Write-TestSuiteInfo "`$InvocationName:     $InvocationName"
Write-TestSuiteInfo "`$InvocationPath:     $InvocationPath"
Write-TestSuiteInfo "`$LogFileName:        $LogFileName"
Write-TestSuiteInfo "`$LogFilePath:        $LogFilePath"
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                      Input parameters                      "
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "`$TestSuiteName:      $TestSuiteName"
Write-TestSuiteInfo "`$EnvironmentName:    $EnvironmentName"
Write-TestSuiteInfo "============================================================"

#------------------------------------------------------------------------------------------
# Read, parse and fix the XML configuration file
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Read and Fix XML Configuration File             "
Write-TestSuiteInfo "============================================================"
Read-TestSuiteXml

#------------------------------------------------------------------------------------------
# Remove virtual machines
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                   Remove Virtual Machines                  "
Write-TestSuiteInfo "============================================================"
Remove-TestSuiteVirtualMachines

#------------------------------------------------------------------------------------------
# Remove virtual network switches
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "              Remove Virtual Network Switches               "
Write-TestSuiteInfo "============================================================"
Remove-TestSuiteVirtualNetworkSwitches

#------------------------------------------------------------------------------------------
# Complete cleanup
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                     Complete Cleanup                       "
Write-TestSuiteInfo "============================================================"
Complete-TestSuiteCleanup