###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

Param
(
    # The path of the CSV file
    [string]$csvFile = "Windows.csv",
    # Azure Subscriptoion Id
    [string]$subscriptionId = "43ceac48-878b-45c7-9a98-4bdca5a11f25",
    # Azure ApplicationId
    [string]$applicationId = "ded7315a-2d0b-47b0-8b4f-9d34eb0a6288",
    # Connect to azure certificate thumbprint
    [string]$thumbPrint = "c13a03e41fe0220fab4b688951fa986e8441385b",
    # Azure TenantId
    [string]$tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    [string]$storageAccount = "testsuiteshare",
    [string]$storageShareName = "protocoltestsuiteshare",
    [string]$fileShareResourceGroup = "TestSuiteOnAzure"
)

$InvocationPath = Split-Path $MyInvocation.MyCommand.Definition -parent

Push-Location "$InvocationPath\..\"
# Test Suite Root Folder under Jenkins workspace
$RegressionRootPath = Get-Location

$Script:ToolLibPath = "$RegressionRootPath\..\..\TestSuitesShare\Tools"


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

Function Read-CSV {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string] $filePath,
        [string] $XmlPath
    )
    Write-TestSuiteInfo "Read the CSV configuration file."

    Write-TestSuiteStep "Check if the CSV configuration file exist or not."

    if (!(Test-Path -Path $filePath)) {
        Write-TestSuiteError "$filePath file not found."
    }
    else {
        Write-TestSuiteSuccess "$filePath file found."
    }

    if (!(Test-Path -Path $XmlPath)) {
        Write-TestSuiteError "$XmlPath file not found."
    }
    else {
        Write-TestSuiteSuccess "$XmlPath file found."
    }

    # Read contents from the XML file
    Write-TestSuiteStep "Read contents from the XML configuration file."
    [Xml]$xmlObjects = Get-Content $XmlPath
    if ($null -eq $xmlObjects) {
        Write-TestSuiteError "$XmlPath file is not a valid xml configuration file." -Exit
    }
    else {
        $xmlObjects | Format-TestSuiteXml -Indent 4
    }
    # Read contents from the CSV file
    $vms = Import-Csv -Path $filePath
    $index = 0;
    foreach ($vm in $vms) {
        $xmlObjects.lab.core.username = $vm.UserName;
        $xmlObjects.lab.core.password = $vm.Password;
        $xmlObjects.lab.vmsetting.location = $vm.Location;
        $xmlObjects.lab.vmsetting.networkId = $vm.VMSwitch;
        $xmlObjects.lab.vmsetting.subnetName = $vm.Subnet;
        $xmlObjects.lab.vmsetting.resourceGroup = $vm.ResourceGroup;
        $xmlObjects.lab.vmsetting.diskType = $vm.DiskType;
        $xmlObjects.lab.vmsetting.vmSize = $vm.VMSize;
        if ($index -eq 0) {
            $xmlObjects.lab.servers.vm.role = $vm.Role;
            $xmlObjects.lab.servers.vm.os = $vm.OS;
            $xmlObjects.lab.servers.vm.imageReference.publisher = $vm.Publisher;
            $xmlObjects.lab.servers.vm.imageReference.offer = $vm.Offer;
            $xmlObjects.lab.servers.vm.imageReference.sku = $vm.Sku;
            $xmlObjects.lab.servers.vm.imageReference.version = $vm.Version;
            $xmlObjects.lab.servers.vm.name = $vm.VMName;
            $xmlObjects.lab.servers.vm.domain = $vm.Domain;
            $xmlObjects.lab.servers.vm.ip = $vm.IP;
        }
        else {
            if ($index -eq 1) {
                $newVm = $xmlObjects.lab.servers.vm.Clone()
            }
            Else {
                $newVm = $xmlObjects.lab.servers.vm[0].Clone()
            }
            $newVm.installorder = ($index + 1).ToString()
            $newVm.role = $vm.Role
            $newVm.os = $vm.OS
            $newVm.imageReference.publisher = $vm.Publisher
            $newVm.imageReference.offer = $vm.Offer
            $newVm.imageReference.sku = $vm.Sku
            $newVm.imageReference.version = $vm.Version
            $newVm.name = $vm.VMName
            $newVm.domain = $vm.Domain
            $newVm.ip = $vm.IP

            $xmlObjects.lab.servers.AppendChild($newVm)
        }
        
        $index++;
    }
    $xmlObjects.Save($XmlPath);
}

#------------------------------------------------------------------------------------------
# Read and parse XML configuration file
# $Setup will be used as a global variable to store the configuration information
#------------------------------------------------------------------------------------------
Function Read-TestSuiteXml {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string] $filePath
    )
    Write-TestSuiteInfo "Read and parse the XML configuration file."

    Write-TestSuiteStep "Check if the XML configuration file exist or not."
    # If $filePath is not found, prompt a list of choices for user to choose
    if (!(Test-Path -Path $filePath)) {
        Write-TestSuiteError "$filePath file not found."
    }
    else {
        Write-TestSuiteSuccess "$filePath file found."
    }

    # Read contents from the XML file
    Write-TestSuiteStep "Read contents from the XML configuration file."
    [Xml]$Script:Setup = Get-Content $filePath
    if ($null -eq $Script:Setup) {
        Write-TestSuiteError "$filePath file is not a valid xml configuration file." -Exit
    }
    else {
        $Script:Setup | Format-TestSuiteXml -Indent 4
    }

    $Script:TestSuiteName = $Script:Setup.lab.core.TestSuiteName
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
        Import-Module AzureRM -ErrorAction Stop
        Write-Host "Connecting to Azure: $thumbPrint"
        $thumbPrint = "c13a03e41fe0220fab4b688951fa986e8441385b"
        Connect-AzureRmAccount -CertificateThumbprint $thumbPrint -ApplicationId $applicationId -TenantId $tenantId -ErrorAction Stop
    }
}

#------------------------------------------------------------------------------------------
# Deploy all the virtual machines of this test suite
# It creates and starts all the VMs
#------------------------------------------------------------------------------------------
Function Deploy-TestSuiteVirtualMachines {
    Select-AzSubscription -SubscriptionID $subscriptionId > $null;

    # Start to create VM jobs
    Write-TestSuiteInfo "Deploy azure virtual machines for this test suite."
    
    #$jobIds = New-Object 'System.Collections.Generic.List[Int]'
    $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;
    $deployNames = @()
    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort-Object -Property installorder)) {
        $vmName = $Vm.name
        Write-Host "Start create Azure VM:$vmName, ResourceGroup: $resourceGroup" 
        
        $DeployName = "Deploy_" + $vmName
        $deployNames += $DeployName

        Write-Host "$RegressionRootPath\AzureRegression\$TestSuiteName\$vmName\template.json"

        $templateFilePath = "$RegressionRootPath\AzureRegression\$TestSuiteName\$vmName\template.json"
        $parametersFilePath = "$RegressionRootPath\AzureRegression\$TestSuiteName\$vmName\parameters.json"

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
        $completeCount = (Get-AzResourceGroupDeployment -ResourceGroupName $resourceGroup | Select-Object -Property DeploymentName, ProvisioningState | Where-Object { ($_.DeploymentName -In $deployNames) -And ($_.ProvisioningState -eq "Succeeded") } | Measure-Object).Count
        $failedCount = (Get-AzResourceGroupDeployment -ResourceGroupName $resourceGroup | Select-Object -Property DeploymentName, ProvisioningState | Where-Object { ($_.DeploymentName -In $deployNames) -And ($_.ProvisioningState -eq "Failed") } | Measure-Object).Count

        Write-Host "Total VMs: $jobsCount, Completed VMs Count: $completeCount, Failed VMs Count: $failedCount"
        $timeoutCount--
    } 
    until((($completeCount + $failedCount) -eq $jobsCount) -Or ($timeoutCount -eq 0))

    if ($failedCount -gt 0) {
        $errorMsg = "There $failedCount VM deployed failed, please check error from azure portal (https://ms.portal.azure.com/#@microsoft.onmicrosoft.com/resource/subscriptions/43ceac48-878b-45c7-9a98-4bdca5a11f25/resourceGroups/$resourceGroup/deployments) and try again"
        throw $errorMsg
    }

    if ($timeoutCount -eq 0) {
        $errorMsg = "VM deployed timeout, please check error from azure portal (https://ms.portal.azure.com/#@microsoft.onmicrosoft.com/resource/subscriptions/43ceac48-878b-45c7-9a98-4bdca5a11f25/resourceGroups/$resourceGroup/deployments) and try again"
        throw $errorMsg
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
# Read CSV and fix the XML configuration file
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Read CSV File                                   "
Write-TestSuiteInfo "============================================================"
Read-CSV "$RegressionRootPath\..\$csvFile" "$RegressionRootPath\..\TestSuites\AzureVm.xml"

#------------------------------------------------------------------------------------------
# Read, parse and fix the XML configuration file
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Read XML File                                   "
Write-TestSuiteInfo "============================================================"
Read-TestSuiteXml "$RegressionRootPath\..\TestSuites\AzureVm.xml"

#------------------------------------------------------------------------------------------
# Prepare Temp folder for Virtual Machines 
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Prepare Temp folder for Virtual Machines        "
Write-TestSuiteInfo "============================================================"
& $RegressionRootPath\AzureRegression\Prepare-VMFiles.ps1 -TestSuiteName $TestSuiteName `
    -configFile "$RegressionRootPath\..\TestSuites\AzureVm.xml" `
    -subscriptionId $subscriptionId `
    -fileShareResourceGroup $fileShareResourceGroup `
    -storageShareName $storageShareName `
    -storageAccount $storageAccount `
    -toolLibPath $ToolLibPath `
    -usePublicKey $false

#------------------------------------------------------------------------------------------
# Deploy Virtual Machines 
#------------------------------------------------------------------------------------------
Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "            Deploy Virtual Machines                         "
Write-TestSuiteInfo "============================================================"
Deploy-TestSuiteVirtualMachines
