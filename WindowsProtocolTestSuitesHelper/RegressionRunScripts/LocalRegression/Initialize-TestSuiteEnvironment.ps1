###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Initialize-TestSuiteEnvironment.ps1
## Purpose:        Initialize environment for setting up a specified Test Suite.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2012, Windows Server 2012 R2, Windows Server 2016, and later.
##
###########################################################################################

<#

.SYNOPSIS

Initialize the test suite environment on a VM host by copying all the necessary files from different shares to a work folder.

.DESCRIPTION

The Initialize-TestSuiteEnvironment function uses Robocopy to retrieve tools, VHDs, Media and Test Suite files from different shares.

.EXAMPLE

.\Initialize-TestSuiteEnvironment.ps1 -ToolShare "\\pet-storage-04\PrototestRegressionShare\Tools" -VHDShare "\\pet-storage-04\PrototestRegressionShare\VHDShare" -MediaShare "\\pet-storage-04\PrototestRegressionShare\VSTORMLITE\media" -TestSuitesShare "\\pet-storage-04\PrototestRegressionShare\ProtocolTestSuite" -TestSuiteNames @("ADFamily", "Kerberos")

If no or null -TestSuiteNames is shown in the parameter, all test suites under ProtocolTestSuite folder will be copied to VM host destination

.NOTES

You need to make sure the share paths are correct, otherwise, unexpected errors will occur.

#>

Param
(
    # The scriptlibs share path    
    [ValidateScript({Test-Path $_ -PathType 'Container'})]
    [string]$ScriptLibShare,
    # The share path that stores all the Tools
    # Default: \\pet-storage-04\PrototestRegressionShare\Tools
    [Parameter(Mandatory=$true)]
    [ValidateScript({Test-Path $_ -PathType 'Container'})]
    [string]$ToolShare         = "\\pet-storage-04\PrototestRegressionShare\ToolShare",
    # The share path that stores all the VHDs
    # Default: \\\\pet-storage-04\PrototestRegressionShare\VHDShare\<Plugfest or IOLab>
    [Parameter(Mandatory=$true)]
    [ValidateScript({Test-Path $_ -PathType 'Container'})]
    [string]$VHDShare          = "\\pet-storage-04\PrototestRegressionShare\VHDShare",
    # The share path that stores all the Medias, for example Visual Studio ISOs
    # Default: \\pet-storage-04\PrototestRegressionShare\VSTORMLITE\media
    [Parameter(Mandatory=$true)]
    [ValidateScript({Test-Path $_ -PathType 'Container'})]
    [string]$MediaShare        = "\\pet-storage-04\PrototestRegressionShare\MediaShare",
    # The share path that stores all the Test Suite Files
    [ValidateScript({Test-Path $_ -PathType 'Container'})]
    [string]$TestSuitesShare,
    # The names of the Test Suites that want to be tested
	[Parameter(Mandatory=$true)]
    [string[]]$TestSuiteNames,
    # The name of the XML file, indicating which environment you want to configure
    # If multiple environments of the same test suite are to be deployed, specify <$EnvironmentName> as "ADFamily1.xml", "ADFamily2.xml", ...
    [Parameter(Mandatory=$true)]
    [string]$EnvironmentName,
    # Source path of VS license file on remote server
    [ValidateNotNullOrEmpty()]
    [ValidateScript({Test-Path $_ -PathType 'Container'})]
    [string]$VsLicenseSrcPath        = "\\ziz-dfsr02\WINTEROP\tools\VSLicense",
    # Domain that connect to VS license file server
    [string]$VsLicenseServerDomain="FarEast",
    # User name that connect to VS license file server
    [string]$VsLicenseServerUserName="pettest",
    # Password that connect to VS license file server
    [string]$VsLicenseServerPassword=">vvk2PEw"
    
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
# [Work Folder Information]
#   ToolLibPath:        File Path of all the tools
#   VhdLibPath:         File Path of the virtual hard disks, the vhd names are presented as input parameters: ServerDiskName and ClientDiskName
#   MediaLibPath        File Path of the media files
#	ScriptLibPath		File Path of the common script library
#   VsLicensePath       File Path of visual studio license file
# [Test Suite Information]
#   TestSuiteLibPath:   File Path of all the protocol test suites
#------------------------------------------------------------------------------------------
$InitialInvocation       = $MyInvocation
$InvocationFullPath      = $InitialInvocation.MyCommand.Definition
$InvocationName          = [System.IO.Path]::GetFileName($InvocationFullPath)
$InvocationPath          = Split-Path -Parent $InvocationFullPath
$LogFileName             = "$InvocationName.log"
$LogFilePath             = "$InvocationPath\..\Logs"
$HostOsBuildNumber       = "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
$ScriptLibPath           = "$InvocationPath\..\ScriptLib"
$ToolLibPath             = "$InvocationPath\..\Tools"
$VhdLibPath              = "$InvocationPath\..\VHD"
$MediaLibPath            = "$InvocationPath\..\Media"
$VsLicensePath           = "$InvocationPath\..\VsLicense"
$TestSuiteLibPath        = "$InvocationPath\..\ProtocolTestSuite"
$XmlFileFullPath         = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteNames\VSTORMLITEFiles\XML\$EnvironmentName"

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
    [switch]$ExitOnError,
    [string]$file = "")

    if (!(Test-Path $Source))
    {
        Write-TestSuiteError "Folder $Source does not exist."
        if ($ExitOnError) { Exit 1 } else { return }
    }

    Robocopy.exe $Source $Destination $file /MIR /NFL /NDL /NC /NS /NP
}

#------------------------------------------------------------------------------------------
# Select one item from all the choices
#------------------------------------------------------------------------------------------
Function Select-Item {
    Param(
    [string[][]]$Choices,
    [string]$Caption = "Please make a selection",
    [string]$Message = "Choices are presented below",
    [int]$Default = 0
    )

    $ChoiceDesc = New-Object System.Collections.ObjectModel.Collection[System.Management.Automation.Host.ChoiceDescription]
    $Choices | ForEach-Object { $ChoiceDesc.Add((New-Object System.Management.Automation.Host.ChoiceDescription -ArgumentList $_[0], $_[1])) }
    $Host.UI.PromptForChoice($Caption, $Message, $ChoiceDesc, $Default)
}


#------------------------------------------------------------------------------------------
# Copy Lab Run Files to Work Folder
#------------------------------------------------------------------------------------------
Function Copy-LabRunFiles {
    if(![string]::IsNullOrEmpty($Script:ScriptLibShare)){
        Write-TestSuiteStep "Copy Scriptlib to the WorkFolder."
        Sync-File $Script:ScriptLibShare $Script:ScriptLibPath -ExitOnError
    }

    Write-TestSuiteStep "Copy Tools to the WorkFolder."
    Sync-File $Script:ToolShare $Script:ToolLibPath -ExitOnError

    Write-TestSuiteStep "Copy VHDs to the WorkFolder."
    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort -Property installorder)) {
        $imageRef = $Vm.imageReference
        if($null -eq $imageRef) {
            continue
        }
        $diskName = $imageRef.diskName
        if ([string]::IsNullOrEmpty($diskName)) {
            continue
        }
        Sync-File $Script:VHDShare $Script:VhdLibPath -ExitOnError $diskName
    }

    Write-TestSuiteStep "Copy Visual Studio ISOs to the WorkFolder."
    Sync-File $Script:MediaShare $Script:MediaLibPath -ExitOnError
}

#------------------------------------------------------------------------------------------
# Copy Visual Studio License File to Work Folder
#------------------------------------------------------------------------------------------
Function Copy-VisualStudioLicenseFile {
    $netConn=Test-NetConnection $Script:VsLicenseSrcPath
    if($netConn.PingSucceeded -eq $false){
    Write-TestSuiteWarning "Can't connect to VS license file server"
    return
    }
    $hasUsedCred=$false
    $isPathExist=Test-Path $Script:VsLicenseSrcPath 
    if($isPathExist -eq $false){
    try{
    $Fullusername="$VsLicenseServerDomain\$VsLicenseServerUserName"
    net use $Script:VsLicenseSrcPath    $VsLicenseServerPassword /user:$Fullusername 
    $hasUsedCred=$true
    }catch{
    $ErrorMessage = $_.Exception.Message
    Write-TestSuiteError $ErrorMessage
    return
    }
    }
    if(Test-Path $Script:VsLicenseSrcPath){
    Write-TestSuiteStep "Copy Visual Studio License File to the WorkFolder."
    Sync-File $Script:VsLicenseSrcPath $Script:VsLicensePath
    }
    if($hasUsedCred){
    net use $Script:VsLicenseSrcPath  /delete
    }
}

#------------------------------------------------------------------------------------------
# Copy Test Suite Files to Work Folder
#------------------------------------------------------------------------------------------
Function Copy-TestSuiteFiles {
    if([string]::IsNullOrEmpty($Script:TestSuitesShare)) { return }
    
    if([string]::IsNullOrEmpty($Script:TestSuiteNames))
    {      
        Write-TestSuiteStep "Copy all test suites from $Script:TestSuitesShare to $Script:TestSuiteLibPath."
        Sync-File $Script:TestSuitesShare $Script:TestSuiteLibPath -ExitOnError
    }
    else
    {
        foreach ($TestSuiteName in $Script:TestSuiteNames) {          
            Write-TestSuiteStep "Copy $TestSuiteName files to the WorkFolder."
            Sync-File $Script:TestSuitesShare\$TestSuiteName $Script:TestSuiteLibPath\$TestSuiteName -ExitOnError
        }
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
# Complete Initialize
#------------------------------------------------------------------------------------------
Function Complete-InitializeTestSuiteEnvironment {
    Write-TestSuiteInfo "Complete initializing the test suite environment."
    Write-TestSuiteSuccess "All jobs are done."
}

#==========================================================================================
# Main Script Body
#==========================================================================================

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
Write-TestSuiteInfo "`$ToolShare:          $ToolShare"
Write-TestSuiteInfo "`$ScriptLibShare:     $ScriptLibShare"
Write-TestSuiteInfo "`$VHDShare:           $VHDShare"
Write-TestSuiteInfo "`$MediaShare:         $MediaShare"
Write-TestSuiteInfo "`$TestSuitesShare:    $TestSuitesShare"
Write-TestSuiteInfo "`$TestSuiteNames:     $TestSuiteNames"
Write-TestSuiteInfo "`$VsLicenseSrcPath:   $VsLicenseSrcPath"
Write-TestSuiteInfo "`$EnvironmentName:   $EnvironmentName"

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
Write-TestSuiteInfo "[Work Folder Information]"
Write-TestSuiteInfo "`t`$ToolLibPath:        $ToolLibPath"
Write-TestSuiteInfo "`t`$VhdLibPath:         $VhdLibPath"
Write-TestSuiteInfo "`t`$MediaLibPath:       $MediaLibPath"
Write-TestSuiteInfo "`t`$ScriptLibPath:		 $ScriptLibPath"
Write-TestSuiteInfo "`t`$VsLicensePath:		 $VsLicensePath"
Write-TestSuiteInfo "[Test Suite Information]"
Write-TestSuiteInfo "`t`$TestSuiteLibPath:   $TestSuiteLibPath"
Write-TestSuiteInfo "============================================================"

#------------------------------------------------------------------------------------------
# Read, parse and fix the XML configuration file
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Read XML Configuration File             "
Write-TestSuiteInfo "============================================================"
Read-TestSuiteXml

#------------------------------------------------------------------------------------------
# Copy Lab Run Files to Work Folder
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Copy Lab Run Files to Work Folder               "
Write-TestSuiteInfo "============================================================"
Copy-LabRunFiles

#------------------------------------------------------------------------------------------
# Copy Visual Studio License File to Work Folder
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "       Copy Visual Studio License File to Work Folder       "
Write-TestSuiteInfo "============================================================"
Copy-VisualStudioLicenseFile

#------------------------------------------------------------------------------------------
# Copy Test Suite Files to Work Folder
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "           Copy Test Suite Files to Work Folder             "
Write-TestSuiteInfo "============================================================"
Copy-TestSuiteFiles

#------------------------------------------------------------------------------------------
# Complete Initializing Test Suite Environment
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                  Complete Initializing                     "
Write-TestSuiteInfo "============================================================"
Complete-InitializeTestSuiteEnvironment