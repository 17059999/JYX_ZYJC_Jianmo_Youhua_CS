###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Run-ShutDown-VM.ps1
## Purpose:        Shut down VMs at the end of a regression run
## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later; Linux (ubuntu 18.04).
## Input parameter is 
##      TestSuiteName       :  Test Suite name
##      EnvironmentName     :  Environment xml of current testsuite
## Process:
##  1. Connect to azure use tenantid/subs/thumbprintid
##  2. Shut down VM to avoid S360 issues.
###########################################################################################

Param
(
    # The name of the Test Suite, only used to fetch XML configuration file and specify log and vm folder
    [string]$TestSuiteName = "RDPServer",
    # The name of the XML file, indicating which environment you want to configure
    [string]$EnvironmentName = "RDPServer.xml",
    # Azure Subscriptoion Id
    [string]$subscriptionId = "43ceac48-878b-45c7-9a98-4bdca5a11f25",
    # Azure ApplicationId
    [string]$applicationId = "ded7315a-2d0b-47b0-8b4f-9d34eb0a6288",
    # Connect to azure certificate thumbprint
    [string]$thumbPrint = "c13a03e41fe0220fab4b688951fa986e8441385b",
    # Azure TenantId
    [string]$tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47"
)

$InvocationPath = Split-Path $MyInvocation.MyCommand.Definition -parent
# Local Tool Share path
[string]$ToolLibPath = ""
# environment configure file full path
[string]$XmlFileFullPath = ""

Push-Location "$InvocationPath\..\"
# Test Suite Root Folder under Jenkins workspace
$RegressionRootPath = Get-Location


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
# Write a piece of step message to the screen
#------------------------------------------------------------------------------------------
Function Write-TestSuiteStep {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message)

    Write-TestSuiteInfo -Message "[STEP]: $Message" -ForegroundColor Yellow -BackgroundColor DarkBlue
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
    if ($Exit) {exit 1}
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
    if ($Exit) {exit 1}
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
        # Inform the user that we have we are running as "Administrator"
        Write-TestSuiteInfo "we are running as `"Administrator`"."

        # Change the title and background color to indicate administrator role
        Write-TestSuiteStep "Change the title and background color to indicate administrator role."
        $Host.UI.RawUI.WindowTitle = $Script:InitialInvocation.MyCommand.Definition + " (Elevated as $AdminRole)"
        $Host.UI.RawUI.BackgroundColor = "DarkBlue"
        Clear-Host
    }
    else {
        # Inform the user that we have we are not running as "Administrator"
        Write-TestSuiteWarning "we are not running as the `"Administrator`". Attempt to Elevate the prompt."

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
# Initialization global variable value
#------------------------------------------------------------------------------------------
Function Initialization-Environment{
    $Script:XmlFileFullPath = "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML\$EnvironmentName"

    $Script:ToolLibPath = "$RegressionRootPath\..\..\TestSuitesShare\Tools"

    Write-TestSuiteInfo "RegressionRootPath:     $RegressionRootPath"
    Write-TestSuiteInfo "AzureRegression:        $RegressionRootPath\AzureRegression"
    Write-TestSuiteInfo "LogFilePath:            $RegressionRootPath\TestResults"
    Write-TestSuiteInfo "ToolLibPath:            $ToolLibPath"
    Write-TestSuiteInfo "XmlFileFullPath:        $XmlFileFullPath"
}

#------------------------------------------------------------------------------------------
# Read and parse XML configuration file
# $Setup will be used as a global variable to store the configuration information
#------------------------------------------------------------------------------------------
Function Read-TestSuiteXml {

    Write-TestSuiteInfo "Read and parse the XML configuration file."

    Write-TestSuiteStep "Check if the XML configuration file exist or not."
    # If $XmlFileFullPath is not found, prompt a list of choices for user to choose
    if (!(Test-Path -Path $Script:XmlFileFullPath)) {
        Write-TestSuiteError "$Script:XmlFileFullPath file not found."
    }
    else {
        Write-TestSuiteSuccess "$Script:XmlFileFullPath file found."
    }

    # Read contents from the XML file
    Write-TestSuiteStep "Read contents from the XML configuration file."
    [Xml]$Script:Setup = Get-Content $Script:XmlFileFullPath
    if ($null -eq $Script:Setup) {
        Write-TestSuiteError "$Script:XmlFileFullPath file is not a valid xml configuration file." -Exit
    }
    else {
        $Script:Setup | Format-TestSuiteXml -Indent 4
    }

    if($Script:Setup.lab.vmsetting.vaultName) {
        $vaultName = $Script:Setup.lab.vmsetting.vaultName
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
# Connect to AzureRm
#------------------------------------------------------------------------------------------
Function ConnectToAzureRm {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$thumbPrint,
        [string]$applicationId,
        [string]$tenantId)

    Process {
       # Import-Module AzureRM -ErrorAction Stop
        Write-Host "Connecting to AzureRm"
        Write-Host $thumbPrint
        Write-Host $applicationId
        Write-Host $tenantId
        Connect-AzAccount -CertificateThumbprint $thumbPrint -ApplicationId $applicationId -Tenant $tenantId -ErrorAction Stop
    }
}


#------------------------------------------------------------------------------------------
# Complete the test suite setup
#------------------------------------------------------------------------------------------
Function Stop-VirtualMachines {
    Select-AzSubscription -Subscription $subscriptionId > $null;
    Get-AzContext

    $jobs = @()
    Write-TestSuiteInfo "Stop Virtual Machines."

    $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;
    Write-TestSuiteInfo $resourceGroup

    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort-Object -Property installorder)) {

       $params = @($Vm.name, $resourceGroup)

       $job = start-job -scriptblock {
         param($vmName, $resourceGroupName)
         Stop-AzVM -Name $vmName -ResourceGroupName $resourceGroupName -Force
       } -argumentlist $params

       $jobs = $jobs + $job
    }

    If($jobs -ne @())
    {
        write-host "Waiting for jobs to complete..." -foregroundcolor yellow -backgroundcolor red
        wait-job -job $jobs
        get-job | receive-job
    }
    Else
    {
        write-host "There were no running VMs" -foregroundcolor yellow -backgroundcolor red
    }
}


#==========================================================================================
# Elevate shell user to administrator
#==========================================================================================
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                       Elevate Shell                        "
Write-TestSuiteInfo "============================================================"
Elevate-Shell

#------------------------------------------------------------------------------------------
# Initialization-Environment
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Initialization-Environment                      "
Write-TestSuiteInfo "============================================================"
Initialization-Environment

#------------------------------------------------------------------------------------------
# Read, parse and fix the XML configuration file
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Read and Fix XML Configuration File             "
Write-TestSuiteInfo "============================================================"
Read-TestSuiteXml

#------------------------------------------------------------------------------------------
# Connect to AzureRM    
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Connect to AzureRM                              "
Write-TestSuiteInfo "============================================================"
#ConnectToAzureRm -thumbPrint $thumbPrint -applicationId $applicationId -tenantId $tenantId

#------------------------------------------------------------------------------------------
# Complete Setup and stop azure VM
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "                      Stop Virtual Machines                        "
Write-TestSuiteInfo "============================================================"
Stop-VirtualMachines