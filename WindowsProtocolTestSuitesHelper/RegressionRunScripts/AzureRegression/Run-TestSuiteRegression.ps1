###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Run-TestSuiteRegression.ps1
## Purpose:        Setup test suite environment and run test case for specified xml
## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later; Linux (ubuntu 18.04).
## Input parameter is 
##      TestSuiteName       :  Test Suite name
##      EnvironmentName     :  Environment xml of current testsuite
## Process:
##  1. Connect to azure use tenantid/subs/thumbprintid
##  2. Prepare temp fold for each VM, generate deploy configure file
##  3. Deploy azure vm use generated configure files use Prepare-VMFiles.ps1
##  4. Call Configure-AzureVMs.ps1 to configure each VM
##        a. Call Set-AzureRmVMExtension to configure each VM, include: disable firewall, enable remote ps, enable WinRM, set PS execute policy
##        b. Execute InstallScript, InstallFeatureScript, PostScript and wait signal file for each script
##  5. Call Run-TestCase.ps1 to start run Test Case, then upload result to azure file share
###########################################################################################

Param
(
    # The name of the Test Suite, only used to fetch XML configuration file and specify log and vm folder
    [string]$TestSuiteName = "RDPServer",
    # The name of the XML file, indicating which environment you want to configure
    [string]$EnvironmentName = "RDPServer.xml",
    # Azure Subscriptoion Id
    [string]$subscriptionId = "43ceac48-878b-45c7-9a98-4bdca5a11f25",
    # Azure Key vault
    [string]$vaultName = "WinProtocolVault",
    # Azure ApplicationId
    [string]$applicationId = "ded7315a-2d0b-47b0-8b4f-9d34eb0a6288",
    # Connect to azure certificate thumbprint
    [string]$thumbPrint = "c13a03e41fe0220fab4b688951fa986e8441385b",
    # Azure TenantId
    [string]$tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    # File Share's resource group name
    [string]$fileShareResourceGroup = "TestSuiteOnAzure",
    # Azure results Blob Account Name
    [string]$ResultStorageAccount = "testsuiteresults",
    # Azure Storage Account Name
    [string]$storageAccount = "testsuiteshare",
    # Azure Storage Share Name
    [string]$storageShareName = "protocoltestsuiteshare",
    # SSH Public Key For Linux
    [string]$linuxPublickey = "",
    # Stop VMs After Regression
    [boolean]$StopVMsAfterRegression = $true,
    # Regression reuslt and log path
    [string]$LogFilePath,
    # Run tests or not
    [string]$runTests = "true",
    # Create Restore Points or not
    [string]$enableRestorePoint = "false"
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
# Initialization global variable value
#------------------------------------------------------------------------------------------
Function Initialization-Environment{
    $Script:XmlFileFullPath = "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML\$EnvironmentName"

    $Script:ToolLibPath = "$RegressionRootPath\..\..\TestSuitesShare\Tools"

    Write-TestSuiteInfo "RegressionRootPath:     $RegressionRootPath"
    Write-TestSuiteInfo "AzureRegression:           $RegressionRootPath\AzureRegression"
    Write-TestSuiteInfo "LogFilePath:            $RegressionRootPath\TestResults"
    Write-TestSuiteInfo "ToolLibPath:            $ToolLibPath"
    Write-TestSuiteInfo "XmlFileFullPath:        $XmlFileFullPath"
}

#------------------------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#------------------------------------------------------------------------------------------
Function Start-TestSuiteLog {
    $currentDatetime = Get-Date -format "yyyy-MM-dd_HHmmss"
    $EnvironmentFolder = $EnvironmentName.Substring(0,$EnvironmentName.IndexOf(".xml"))
    # Log folder structure
    # Testsuite name - time/date
    # Environment name
    #     Initialize environment .log
    #     Setup environment .log
    #     Execute environment.log
    #     VM
    #         configscript.log (new task for copy log out of VM, execute-test suite.ps1)
    #     .trx
    if(!$Script:LogFilePath){
        $Script:LogFilePath = "$RegressionRootPath\TestResults\$TestSuiteName" + "_$currentDatetime\$EnvironmentFolder"
    }
    
    if(!(Test-Path $Script:LogFilePath)){
        New-Item -ItemType Directory $Script:LogFilePath
    }else{
        # clear preview result
        remove-item -Path "$Script:LogFilePath\*.*" -Recurse -Force
    }

    # Stop the previous transcript
    try {
        Stop-Transcript -ErrorAction SilentlyContinue
    }
    catch [System.InvalidOperationException] {}

    # Start new transcript
    Start-Transcript -Path "$Script:LogFilePath\Setup_TestSuiteEnvrionment_$TestSuiteName.log" -Append -Force
    Write-TestSuiteWarning "TestSuite Root Folder $RegressionRootPath"
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
# Deploy all the virtual machines of this test suite
# It creates and starts all the VMs
#------------------------------------------------------------------------------------------
Function Deploy-TestSuiteVirtualMachines {
    Select-AzSubscription -Subscription $subscriptionId > $null;

    # Start to create VM jobs
    Write-TestSuiteInfo "Deploy azure virtual machines for this test suite. $subscriptionId"
    
    #$jobIds = New-Object 'System.Collections.Generic.List[Int]'
    $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;
    Write-TestSuiteInfo "Deploy azure virtual machines for this test suite resource group. $resourceGroup"
    $deployNames = @()
    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort-Object -Property installorder)) {
        $vmName = $Vm.name
        Write-Host "Start create Azure VM:$vmName, ResourceGroup: $resourceGroup" 
        
        $DeployName = "Deploy_" + $vmName
        $deployNames += $DeployName
        $templateFilePath = "$RegressionRootPath\$TSHelperFolder\AzureRegression\$TestSuiteName\$vmName\template.json"
        $parametersFilePath = "$RegressionRootPath\$TSHelperFolder\AzureRegression\$TestSuiteName\$vmName\parameters.json"

         Write-Host "Template File Path VM:$templateFilePath" 
         Write-Host "Parameters File Path VM:$parametersFilePath" 

        & $InvocationPath\Deploy-AzureVms.ps1 -VMName $vmName `
            -DeployName $DeployName `
            -resourceGroup $resourceGroup `
            -templateFilePath $templateFilePath `
            -parametersFilePath $parametersFilePath

    }

    $jobsCount = ($deployNames | Measure-Object).Count
    
    $completeCount = 0
    $failedCount = 0
    $timeoutCount = 120;
    do {
        Start-Sleep -Seconds 60
        $completeCount = 0
        $failedCount = 0
        $completeCount = (Get-AzResourceGroupDeployment -ResourceGroupName $resourceGroup | Select-Object -Property DeploymentName, ProvisioningState | Where-Object { ($_.DeploymentName -In $deployNames) -And ($_.ProvisioningState -eq "Succeeded")} | Measure-Object).Count
        $failedCount = (Get-AzResourceGroupDeployment -ResourceGroupName $resourceGroup | Select-Object -Property DeploymentName, ProvisioningState | Where-Object { ($_.DeploymentName -In $deployNames) -And ($_.ProvisioningState -eq "Failed")} | Measure-Object).Count

        Write-Host "Total VMs: $jobsCount, Completed VMs Count: $completeCount, Failed VMs Count: $failedCount"
        $timeoutCount--
    } 
    until((($completeCount + $failedCount) -eq $jobsCount) -Or ($timeoutCount -eq 0))

    if($failedCount -gt 0){
        $errorMsg = "There $failedCount VM deployed failed, please check error from azure portal (https://ms.portal.azure.com/#@microsoft.onmicrosoft.com/resource/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/deployments) and try again"
        throw $errorMsg
    }

    if($timeoutCount -eq 0){
        $errorMsg = "VM deployed timeout, please check error from azure portal (https://ms.portal.azure.com/#@microsoft.onmicrosoft.com/resource/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/deployments) and try again"
        throw $errorMsg
    }
}


#------------------------------------------------------------------------------------------
# Start logging
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Start Logging                                   "
Write-TestSuiteInfo "============================================================"
Start-TestSuiteLog

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
# Prepare Temp folder for Virtual Machines 
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Prepare Temp folder for Virtual Machines        "
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "Log Path: $Script:LogFilePath "
& $RegressionRootPath\AzureRegression\Prepare-VMFiles.ps1 -TestSuiteName $TestSuiteName `
        -configFile $XmlFileFullPath `
        -subscriptionId $subscriptionId `
        -fileShareResourceGroup $fileShareResourceGroup `
        -storageShareName $storageShareName `
        -storageAccount $storageAccount `
        -toolLibPath $ToolLibPath `
        -linuxPublickey $linuxPublickey

#------------------------------------------------------------------------------------------
# Deploy Virtual Machines 
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Deploy Virtual Machines                         "
Write-TestSuiteInfo "============================================================"
Deploy-TestSuiteVirtualMachines

#------------------------------------------------------------------------------------------
# Configure virtual machines
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Configure Virtual Machines                      "
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo $TestSuiteName
Write-TestSuiteInfo $XmlFileFullPath
Write-TestSuiteInfo $subscriptionId
Write-TestSuiteInfo $storageAccount
Write-TestSuiteInfo $vaultName
& $RegressionRootPath\AzureRegression\Configure-AzureVms.ps1 -TestSuiteName $TestSuiteName `
        -configFile $XmlFileFullPath `
        -subscriptionId $subscriptionId `
        -storageAccount $storageAccount `
        -vaultName $vaultName `
        -enableRestorePoint $enableRestorePoint

#------------------------------------------------------------------------------------------
# Start Run TestCase on virtual machines
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Start Run TestCase on Virtual Machines          "
Write-TestSuiteInfo "============================================================"
& $RegressionRootPath\AzureRegression\Run-TestCase.ps1 -TestSuiteName $TestSuiteName `
    -configFile $XmlFileFullPath `
    -EnvironmentName $EnvironmentName `
    -subscriptionId $subscriptionId `
    -storageShareName $storageShareName `
    -fileShareResourceGroup $fileShareResourceGroup `
    -storageAccount $storageAccount `
    -resultStorageAccount $resultStorageAccount `
    -toolLibPath $ToolLibPath `
    -logFilePath $Script:LogFilePath `
    -runTests $runTests