###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Execute-TestSuiteCases.ps1
## Purpose:        Run test cases of a specified Test Suite.
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
    [string]$TestSuiteName      = "ADFamily",
    # The name of the XML file, indicating which environment you want to configure
    [string]$EnvironmentName    = "ADFamily.xml",
    # The workspace of the jenkinsfile, for copy the "$result.json" to the target path to show it.
    [string]$WorkSpace="",
    [string]$subscriptionId = "43ceac48-878b-45c7-9a98-4bdca5a11f25",
    [string]$resourceGroup = "TestSuiteOnAzure",
    [string]$applicationId = "9aaeeda0-6001-4bf0-a801-6ac6d6463db5",
    [string]$thumbPrint = "4F4E13D7BF4E3679400BFF96951814C4AA79C251",
    [string]$tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    [string]$resultStorageAccount = "testsuiteresults",
    [string]$storageShareName = "protocoltestsuiteshare",
    [string]$fileShareResourceGroup = "TestSuiteOnAzure"
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
# [Test Suite Information]
#   XmlFileFullPath:    Full Path of the XML configuration file
#   XmlFileName:        File Name of the XML configuration file
#   XmlFilePath:        File Path of the XML configuration file
#   VmDirPath:          The directory to store all the virtual machines for this test suite
#   IsoDirPath:         The directory to store all the iso files for this test suite
#   Parameters:         The custom parameters for each test suites to run their own <TestSuite>\Scripts\Execute-ProtocolTest.ps1, these parameters can be specified in the XML configuration file
#------------------------------------------------------------------------------------------
$InitialInvocation       = $MyInvocation
$InvocationFullPath      = $InitialInvocation.MyCommand.Definition
$InvocationName          = [System.IO.Path]::GetFileName($InvocationFullPath)
$InvocationPath          = Split-Path -Parent $InvocationFullPath
$LogFileName             = "$InvocationName.log"
$LogFilePath             = "$InvocationPath\..\TestResults\$TestSuiteName"
$HostOsBuildNumber       = "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
$ScriptLibPath           = "$InvocationPath\..\ScriptLib"
$XmlLibPath              = "$InvocationPath\XML"
$XmlFileName             = $EnvironmentName
$XmlFileFullPath         = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML\$EnvironmentName"
$XmlFilePath             = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML"
$ScriptsFilePath         = "$InvocationPath\..\ProtocolTestSuite\$TestSuiteName\Scripts"
$VmDirPath               = "$InvocationPath\..\VM\$TestSuiteName"
$Parameters              = $null

#==========================================================================================
# Function Definition
#==========================================================================================

#------------------------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#------------------------------------------------------------------------------------------
Function Start-TestSuiteLog {

    try{
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
        if ($Tick -lt $TimeoutInSeconds) { Sleep 1 }
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

    # Read custom parameters from XML file
    $ParametersNode = $Setup.lab.SelectSingleNode("Parameters")
    if(($ParametersNode -ne $null) -and ($ParametersNode.HasChildNodes))
    {
        $Script:Parameters = @{}
        foreach($arg in $ParametersNode.ChildNodes)
        {
            # Node type is Element and it has no child
            if(($arg.NodeType -eq [Xml.XmlNodeType]::Element) -and (-not $arg.HasChildNodes))
            {
                $Script:Parameters.Add($arg.Name, $arg.Value)
            }
        }
    }
}

#------------------------------------------------------------------------------------------
# Run test cases
#------------------------------------------------------------------------------------------
Function Run-TestCases {

    $driverNodeOSType = GetRunTestCaseDriverOSType
    if( $driverNodeOSType -eq "Linux"){
        Run-TestCasesInLinuxDriver
    }else{
        Run-TestCasesInWindowsDriver
    }
}

Function GetRunTestCaseDriverOSType{
    [Xml]$Script:Setup = Get-Content $Script:XmlFileFullPath

    $driverNode = $Script:Setup.lab.servers.vm | Where-Object {$_.role -eq "DriverComputer"}
    if($driverNode.os -eq "Linux"){
        return "Linux"
    }

    return "Windows"
}

#------------------------------------------------------------------------------------------
# Run test cases and copy test results in Windows Driver Computer
#------------------------------------------------------------------------------------------
Function Run-TestCasesInWindowsDriver{

    Write-TestSuiteInfo "Run test cases for the test suite."

    Write-TestSuiteStep "Execute test cases"
    Push-Location $Script:ScriptsFilePath

    $CmdLine = ".\Execute-ProtocolTest.ps1 -ProtocolName $Script:TestSuiteName -WorkingDirOnHost $Script:InvocationPath\.. -TestResultDirOnHost $Script:LogFilePath -EnvironmentName $Script:EnvironmentName"

    # Append override parameters to command line
    $scriptinfo = Get-Command ".\Execute-ProtocolTest.ps1"
    if($Script:Parameters -ne $null)
    {
        foreach($key in $Script:Parameters.Keys)
        {
            if($scriptinfo.Parameters[$key] -ne $null)
            {
                $value = $Script:Parameters[$key]
                $CmdLine += " -$key $value"
            }
            else
            {
                Write-TestSuiteWarning "$key is not a valid parameter name for Execute-ProtocolTest.ps1, please check it in lab.Parameters of XML configuration!"
            }
        }
    }
    Write-TestSuiteInfo "Running CmdLine:$CmdLine in Execute-TestSuiteCases.ps1"
    Invoke-Expression $CmdLine
    Pop-Location

    CopyTestResultToAzureAndStopVM
}

#------------------------------------------------------------------------------------------
# Run test cases and copy test results in Linux Driver Computer
#------------------------------------------------------------------------------------------
Function Run-TestCasesInLinuxDriver{    
    [Xml]$Script:Setup = Get-Content $Script:XmlFileFullPath
    $driverNode = $Script:Setup.lab.servers.vm | Where-Object {$_.role -eq "DriverComputer"}
    & $PSScriptRoot\Linux\Trigger-RunTestCaseToLinux.ps1 -Vm $driverNode -Setup $Script:Setup -TestSuiteName $TestSuiteName -EnvironmentName $EnvironmentName

    CopyTestResultToAzureAndStopVM
}

Function CopyTestResultToAzureAndStopVM{
    Write-Host "Connecting to Azure Storage Account"
    Set-AzCurrentStorageAccount -ResourceGroupName $fileShareResourceGroup -Name $resultStorageAccount

    Write-TestSuiteStep "Copy test result to Azure FileShare"
    # Create TestSuite Folder
    $currentDatetime = Get-Date -format "yyyy-MM-dd_HHmmss"
    $destinationFolder = "local-$($TestSuiteName.ToLower())-$($currentDatetime.Replace("_","-"))"
    Write-Host "============================================================"
    Write-Host "DestinationFolder : $destinationFolder"
    Write-Host "storageShareName : $storageShareName"
    Write-Host "resultStorageAccount : $resultStorageAccount"
    Write-Host "============================================================"

    $directory = Get-AzStorageContainer | Where-Object {$_.Name -eq $destinationFolder}
    if(!$directory){
        Write-TestSuiteInfo "Create share container : $destinationFolder"
        New-AzStorageContainer -Name $destinationFolder -Permission Off 
    }
    Get-ChildItem -Path "$LogFilePath" -File -Recurse | Set-AzStorageBlobContent -Container $destinationFolder

    # Create regression result info to json file
    & "$ScriptLibPath\Generate-RegressionInfo.ps1" -logFilePath $LogFilePath `
        -configFile $XmlFileFullPath `
        -EnvironmentName $EnvironmentName `
        -VMHostName $env:computername `
        -blobContainerName $destinationFolder

    # Copy jsonFile to parent folder
    $jsonName = $EnvironmentName -replace ".xml", ".json"
    $jsonPath = [string]::Format("{0}\{1}", $LogFilePath, $jsonName)
    Write-Host "Copy json result from $jsonPath to $WorkSpace" 

    Push-Location "$WorkSpace"
    Remove-Item *.json
    Copy-Item $jsonPath -Destination "$WorkSpace" -Force
    
    Pop-Location

        foreach ($Vm in ($Script:Setup.lab.servers.vm | sort -Property installorder)) {
        Write-TestSuiteStep "Take a snapshot of the VM after test execution."
        Checkpoint-VM -Name $Vm.hypervname -SnapshotName 'AfterTest'        
        #Sleep 2 minutes to wait checkpoint complete
        Start-Sleep -s 120
        
        Write-TestSuiteStep "Stop the VM."
        Stop-VM -Name $Vm.hypervname -Force
    }
}

#------------------------------------------------------------------------------------------
# Complete test case execution
#------------------------------------------------------------------------------------------
Function Complete-TestCaseExecution {

    Write-TestSuiteInfo "Complete the test cases execution for this test suite."

    Wait-TestSuiteActivityComplete -ActivityName "Test Suite Cool Down" -TimeoutInSeconds 30

    try
    {
        Stop-Transcript -ErrorAction Stop
    }
    catch
    {
        Write-Host "Stop-Transcript failed:$_"
    }

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
# Run test cases and parse the results
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Run test cases and parse the results            "
Write-TestSuiteInfo "============================================================"
Run-TestCases

#------------------------------------------------------------------------------------------
# Complete test case execution
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "              Complete test case execution                  "
Write-TestSuiteInfo "============================================================"
Complete-TestCaseExecution