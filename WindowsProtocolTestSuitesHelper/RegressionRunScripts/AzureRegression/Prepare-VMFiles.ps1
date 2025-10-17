###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

Param(
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string] $TestSuiteName,
    # The name of the XML file, indicating which environment you want to configure
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$configFile,
    # Azure Subscriptoion Id
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$subscriptionId,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$storageShareName,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$toolLibPath,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$fileShareResourceGroup,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$storageAccount,
    [Parameter(ValueFromPipeline = $True, Mandatory = $False)]
    [string]$linuxPublickey,
    [Parameter(ValueFromPipeline = $True, Mandatory = $False)]
    [boolean]$usePublicKey = $true
)

$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$RegressionRootPath = "$scriptPath\..\"
$RandomNum

Push-Location $scriptPath

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

Function Write-TestSuiteStep {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message)

    Write-TestSuiteInfo -Message "[STEP]: $Message" -ForegroundColor Yellow -BackgroundColor DarkBlue
}

Function Write-TestSuiteWarning {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [switch]$Exit)

    Write-TestSuiteInfo -Message "[WARNING]: $Message" -ForegroundColor Yellow -BackgroundColor Black
    if ($Exit) { exit 1 }
}

Function Read-TestSuiteXml {

    Write-TestSuiteInfo "Read and parse the XML configuration file."

    Write-TestSuiteStep "Check if the XML configuration file exist or not."
    # If $XmlFileFullPath is not found, prompt a list of choices for user to choose
    if (!(Test-Path -Path $configFile)) {
        Write-TestSuiteError "$configFile file not found."
    }
    else {
        Write-TestSuiteInfo "$configFile file found."
    }

    # Read contents from the XML file
    Write-TestSuiteStep "Read contents from the XML configuration file."
    [Xml]$Script:Setup = Get-Content $configFile
    if ($null -eq $Script:Setup) {
        Write-TestSuiteError "$configFile file is not a valid xml configuration file." -Exit
    }
}

Function GenerateParameterJsonFile {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string] $vmName
    )

    Write-TestSuiteInfo "Start create Parameter.json for VM $vmName"

    $deployUser = $Script:Setup.lab.core.username;
    $deployPassword = $Script:Setup.lab.core.password;
    $deployPublicKey = $linuxPublickey + " " + $deployUser + "@" + $vmName;

    $location = $Script:Setup.lab.vmsetting.location;
    $networkId = $Script:Setup.lab.vmsetting.networkId;
    $subnetName = $Script:Setup.lab.vmsetting.subnetName;
    $diskType = $Script:Setup.lab.vmsetting.diskType;
    $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;

    $interfaceName = $vmName + "Nic"

    $vmSettings = $Script:Setup.lab.servers.vm | Where-Object { $_.name -eq $vmName }

    $ips = $vmSettings.ip
    $vmSize = $vmSettings.vmSize
    if ($null -eq $vmSize) {
        $vmSize = $Script:Setup.lab.vmsetting.vmSize;
    }

    ## create parameter.json
    Write-TestSuiteInfo "Location=   $location"
    Write-TestSuiteInfo "SubnetName=   $subnetName"
    Write-TestSuiteInfo "DiskTyp=   $diskType"
    Write-TestSuiteInfo "VMSize=   $vmSize"
    Write-TestSuiteInfo "InterfaceName=   $interfaceName"

    $ips | ForEach-Object -Process {
        Write-TestSuiteInfo "IP = $($_)"
    }

    Write-TestSuiteInfo "VMName=   $vmName"

    $paramBuilder = [System.Text.StringBuilder]::new();
    [void]$paramBuilder.AppendLine('{');
    [void]$paramBuilder.AppendLine('"$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json#",');
    [void]$paramBuilder.AppendLine('"contentVersion": "1.0.0.0",');
    [void]$paramBuilder.AppendLine('"parameters": {');
    [void]$paramBuilder.AppendLine('"location": { "value": "' + $location + '"},');

    if (($ips | Measure-Object).Count -ge 1) {
        $index = 1;
        $ips | ForEach-Object -Process {
            Write-TestSuiteInfo "Index = $index"
            if ($vmSettings.os -eq "Linux" -or $vmSettings.os -eq "RPMBasedLinux") {
                if($index -ne 1) {
                    Write-TestSuiteInfo "index -ne 1"
                    return;
                }
            }
            Write-TestSuiteInfo "AppendLine$index"
            [void]$paramBuilder.AppendLine('"networkInterfaceName' + $index + '": { "value": "' + $interfaceName + $index + '"},');
            $index++;
        }
    }

    [void]$paramBuilder.AppendLine('"networkSecurityGroupName": { "value": "' + $vmName + '_nsg"},');
    [void]$paramBuilder.AppendLine('"networkSecurityGroupRules": { "value": []},');
    [void]$paramBuilder.AppendLine('"subnetName": { "value": "' + $subnetName + '"},');
    $networkId = "/subscriptions/" + $subscriptionId + "/resourceGroups/" + $resourceGroup + "/providers/Microsoft.Network/virtualNetworks/" + $networkId;
    [void]$paramBuilder.AppendLine('"virtualNetworkId": { "value": "' + $networkId + '"},');

    if (($ips | Measure-Object).Count -ge 1) {
        $index = 1;
        $ips | ForEach-Object -Process {
            [void]$paramBuilder.AppendLine('"privateIPAddress' + $index + '": { "value": "' + $_ + '"},');
            $index++;
        }
    }

    [void]$paramBuilder.AppendLine('"virtualMachineName": { "value": "' + $vmName + '"},');

    $imageRefId = $vmSettings.imageReference.imageRefId
    $imageRefRG = $vmSettings.imageReference.imageRefRG
    $galleryName = $vmSettings.imageReference.galleryName;

    if ($null -ne $galleryName) {
        $galleryResourceGroup = $vmSettings.imageReference.galleryResourceGroup;
        $galleryImageName = $vmSettings.imageReference.galleryImageName;
        $galleryImageVersion = $vmSettings.imageReference.galleryImageVersion;
        $imageRefId = "/subscriptions/" + $subscriptionId + "/resourceGroups/" + $galleryResourceGroup + "/providers/Microsoft.Compute/galleries/" + $galleryName + "/images/" + $galleryImageName + "/versions/" + $galleryImageVersion;
        
        [void]$paramBuilder.AppendLine('"imageRefId": { "value": "' + $imageRefId + '"},');
    }
    elseif ($null -eq $imageRefId) {
        $publisher = $vmSettings.imageReference.publisher
        $offer = $vmSettings.imageReference.offer
        $sku = $vmSettings.imageReference.sku
        $version = $vmSettings.imageReference.version

        [void]$paramBuilder.AppendLine('"publisher": { "value": "' + $publisher + '"},');
        [void]$paramBuilder.AppendLine('"offer": { "value": "' + $offer + '"},');
        [void]$paramBuilder.AppendLine('"sku": { "value": "' + $sku + '"},');
        [void]$paramBuilder.AppendLine('"version": { "value": "' + $version + '"},');
    }
    else {
        if ($null -eq $imageRefRG) {
            $imageRefRG = $fileShareResourceGroup
        }

        $galleryResourceGroup = $imageRefId.Split(";")[1]
        $galleryName = $imageRefId.Split(";")[2]
        $galleryImageName = $imageRefId.Split(";")[3]
        $galleryImageVersion = $imageRefId.Split(";")[4]
        $imageRefId = "/subscriptions/" + $subscriptionId + "/resourceGroups/" + $galleryResourceGroup + "/providers/Microsoft.Compute/galleries/" + $galleryName + "/images/" + $galleryImageName + "/versions/" + $galleryImageVersion;
        [void]$paramBuilder.AppendLine('"imageRefId": { "value": "' + $imageRefId + '"},');
    }
    [void]$paramBuilder.AppendLine('"virtualMachineRG": { "value": "' + $resourceGroup + '"},');
    [void]$paramBuilder.AppendLine('"osDiskType": { "value": "' + $diskType + '"},');
    [void]$paramBuilder.AppendLine('"virtualMachineSize": { "value": "' + $vmSize + '"},');

    [void]$paramBuilder.AppendLine('"adminUsername": { "value": "' + $deployUser + '"},');
    [void]$paramBuilder.AppendLine('"adminPassword": { "value": "' + $deployPassword + '"},');
    [void]$paramBuilder.AppendLine('"adminPublicKey": { "value": "' + $deployPublicKey + '"}');
    [void]$paramBuilder.AppendLine("}");
    [void]$paramBuilder.AppendLine("}");

    $deployPath = "$scriptPath\$TestSuiteName\$vmName"
    if (Test-Path -Path $deployPath) {
        Write-TestSuiteWarning "Remove existing Folder: $deployPath"
        Remove-Item $deployPath -Force -Recurse
    }

    New-Item -ItemType Directory -Path $deployPath

    $paramPath = "$deployPath\parameters.json"

    $streamWriter = [System.IO.StreamWriter] $paramPath
    $streamWriter.Write($paramBuilder.ToString());
    $streamWriter.Close();

    $ctx = New-AzStorageContext -StorageAccountName "wintestsuiteresults" -EnableFileBackupRequestIntent
    $Blob = @{
        File             = $paramPath
        Container        = "exportedfiles"
        Blob             = "$RandomNum-parameters.json"
        Context          = $ctx
        StandardBlobTier = 'Hot'
        }
    Set-AzStorageBlobContent @Blob -Force

    $Blob2 = @{
        File             = $configFile
        Container        = "exportedfiles"
        Blob             = "$RandomNum-configFile.xml"
        Context          = $ctx
        StandardBlobTier = 'Hot'
        }
    Set-AzStorageBlobContent @Blob2 -Force
}

Function GenerateTemplateJsonFile {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string] $vmName
    )

    Write-TestSuiteInfo "Start create Template.json for VM $vmName"
    $location = $Script:Setup.lab.vmsetting.location;
    $subnetName = $Script:Setup.lab.vmsetting.subnetName;
    $diskType = $Script:Setup.lab.vmsetting.diskType;
    $interfaceName = $vmName + "Nic"
    $vmSettings = $Script:Setup.lab.servers.vm | Where-Object { $_.name -eq $vmName }
    
    $vmos = $vmSettings.os
    Write-TestSuiteInfo "OS = $vmos"

    $ips = $vmSettings.ip
    $vmSize = $vmSettings.vmSize
    if ($null -eq $vmSize) {
        $vmSize = $Script:Setup.lab.vmsetting.vmSize;
    }
    $dnsServer = $vmSettings.dns

    ## create parameter.json
    Write-TestSuiteInfo "Location=   $location"
    Write-TestSuiteInfo "SubnetName=   $subnetName"
    Write-TestSuiteInfo "DiskTyp=   $diskType"
    Write-TestSuiteInfo "VMSize=   $vmSize"
    Write-TestSuiteInfo "InterfaceName=   $interfaceName"

    $ips | ForEach-Object -Process {
        Write-TestSuiteInfo "IP = $($_)"
    }

    Write-TestSuiteInfo "VMName=   $vmName"

    $paramBuilder = [System.Text.StringBuilder]::new()
    [void]$paramBuilder.AppendLine('{');
    [void]$paramBuilder.AppendLine('"$schema": "http://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",');
    [void]$paramBuilder.AppendLine('"contentVersion": "1.0.0.0",');
    [void]$paramBuilder.AppendLine('"parameters": {');
    [void]$paramBuilder.AppendLine('"location": { "type": "string"},');
    if (($ips | Measure-Object).Count -ge 1) {
        $index = 1;
        $ips | ForEach-Object -Process {
                        Write-TestSuiteInfo "Index = $index"
            if ($vmSettings.os -eq "Linux" -or $vmSettings.os -eq "RPMBasedLinux") {
                if($index -ne 1) {
                    Write-TestSuiteInfo "index -ne 1"
                    return;
                }
            }
            Write-TestSuiteInfo "AppendLine$index"
            [void]$paramBuilder.AppendLine('"networkInterfaceName' + $index + '": { "type": "string"},');
            $index++;
        }
    }

    [void]$paramBuilder.AppendLine('"networkSecurityGroupName": { "type": "string"},');
    [void]$paramBuilder.AppendLine('"networkSecurityGroupRules": { "type": "array"},');
    [void]$paramBuilder.AppendLine('"virtualNetworkId": { "type": "string"},');

    $imageRefId = $vmSettings.imageReference.imageRefId
    $galleryName = $vmSettings.imageReference.galleryName;
    if ($null -eq $imageRefId -and $null -eq $galleryName) {
        [void]$paramBuilder.AppendLine('"publisher": { "type": "string"},');
        [void]$paramBuilder.AppendLine('"offer": { "type": "string"},');
        [void]$paramBuilder.AppendLine('"sku": { "type": "string"},');
        [void]$paramBuilder.AppendLine('"version": { "type": "string"},');
    }
    else {
        [void]$paramBuilder.AppendLine('"imageRefId": { "type": "string"},');
    }

    [void]$paramBuilder.AppendLine('"subnetName": { "type": "string"},');

    if (($ips | Measure-Object).Count -ge 1) {
        $index = 1;
        $ips | ForEach-Object -Process {
            [void]$paramBuilder.AppendLine('"privateIPAddress' + $index + '": { "type": "string"},');
            $index++;
        }
    }

    [void]$paramBuilder.AppendLine('"virtualMachineName": { "type": "string"},');
    [void]$paramBuilder.AppendLine('"virtualMachineRG": { "type": "string"},');
    [void]$paramBuilder.AppendLine('"osDiskType": { "type": "string"},');
    [void]$paramBuilder.AppendLine('"virtualMachineSize": { "type": "string"},');
    [void]$paramBuilder.AppendLine('"adminUsername": { "type": "string"},');
    [void]$paramBuilder.AppendLine('"adminPassword": { "type": "secureString"},');
    [void]$paramBuilder.AppendLine('"adminPublicKey": { "type": "string"}');
    [void]$paramBuilder.AppendLine("},");

    [void]$paramBuilder.AppendLine('"variables": { ');
    [void]$paramBuilder.AppendLine("`"nsgId`": `"[resourceId(resourceGroup().name, 'Microsoft.Network/networkSecurityGroups', parameters('networkSecurityGroupName'))]`",");
    [void]$paramBuilder.AppendLine("`"vnetId`": `"[parameters('virtualNetworkId')]`",");
    [void]$paramBuilder.AppendLine("`"subnetRef`": `"[concat(variables('vnetId'), '/subnets/', parameters('subnetName'))]`"");
    [void]$paramBuilder.AppendLine("},");

    [void]$paramBuilder.AppendLine('"resources": [ ');
    # Append Availability Set
    if ($Script:isAzureCluster -and ($vmName -match "NODE")) {
        [void]$paramBuilder.AppendLine("{");
        [void]$paramBuilder.AppendLine("`"type`": `"Microsoft.Compute/availabilitySets`",");
        [void]$paramBuilder.AppendLine("`"name`": `"$Script:nodePrefix-AS`",");
        [void]$paramBuilder.AppendLine("`"apiVersion`": `"2016-04-30-preview`",");
        [void]$paramBuilder.AppendLine("`"location`": `"[parameters('location')]`",");
        [void]$paramBuilder.AppendLine("`"properties`": {");
        [void]$paramBuilder.AppendLine("`"platformFaultDomainCount`": 2,");
        [void]$paramBuilder.AppendLine("`"platformUpdateDomainCount`": 2,");
        [void]$paramBuilder.AppendLine("`"managed`": true");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("},");
    }

    # Append load balancer
    if ($Script:isAzureCluster -and ($vmName -match "Node")) {
        [void]$paramBuilder.AppendLine("{");
        [void]$paramBuilder.AppendLine("`"type`": `"Microsoft.Network/loadBalancers`",");
        [void]$paramBuilder.AppendLine("`"name`": `"$Script:nodePrefix-LB`",");
        [void]$paramBuilder.AppendLine("`"apiVersion`": `"2017-08-01`",");
        [void]$paramBuilder.AppendLine("`"location`": `"[parameters('location')]`",");
        [void]$paramBuilder.AppendLine("`"sku`": {");
        [void]$paramBuilder.AppendLine("`"name`": `"Standard`",");
        [void]$paramBuilder.AppendLine("`"tier`": `"Regional`"");
        [void]$paramBuilder.AppendLine("},");
        [void]$paramBuilder.AppendLine("`"properties`": {");
        [void]$paramBuilder.AppendLine("`"frontendIPConfigurations`": [");
        [void]$paramBuilder.AppendLine("{");
        [void]$paramBuilder.AppendLine("`"name`": `"$Script:nodePrefix-FE`",");
        [void]$paramBuilder.AppendLine("`"properties`": {");
        [void]$paramBuilder.AppendLine("`"privateIPAddress`": `"$Script:lbIp`",");
        [void]$paramBuilder.AppendLine("`"privateIPAllocationMethod`": `"Static`",");
        [void]$paramBuilder.AppendLine("`"subnet`": {");
        [void]$paramBuilder.AppendLine("`"id`": `"[variables('subnetRef')]`"");
        [void]$paramBuilder.AppendLine("},");
        [void]$paramBuilder.AppendLine("`"privateIPAddressVersion`": `"IPv4`"");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("],");
        [void]$paramBuilder.AppendLine("`"backendAddressPools`": [");
        [void]$paramBuilder.AppendLine("{");
        [void]$paramBuilder.AppendLine("`"name`": `"$Script:nodePrefix-BEPOOL`"");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("],");
        [void]$paramBuilder.AppendLine("`"loadBalancingRules`": [");
        [void]$paramBuilder.AppendLine("{");
        [void]$paramBuilder.AppendLine("`"name`": `"$Script:nodePrefix-RULE`",");
        [void]$paramBuilder.AppendLine("`"properties`": {");
        [void]$paramBuilder.AppendLine("`"frontendPort`": 0,");
        [void]$paramBuilder.AppendLine("`"backendPort`": 0,");
        [void]$paramBuilder.AppendLine("`"enableFloatingIP`": false,");
        [void]$paramBuilder.AppendLine("`"idleTimeoutInMinutes`": 4,");
        [void]$paramBuilder.AppendLine("`"protocol`": `"All`",");
        [void]$paramBuilder.AppendLine("`"enableTcpReset`": false,");
        [void]$paramBuilder.AppendLine("`"loadDistribution`": `"Default`",");
        [void]$paramBuilder.AppendLine("`"disableOutboundSnat`": false,");
        [void]$paramBuilder.AppendLine("`"frontendIPConfiguration`": {");
        [void]$paramBuilder.AppendLine("`"id`": `"[concat(resourceId('Microsoft.Network/loadBalancers', '$Script:nodePrefix-LB'), '/frontendIpConfigurations/$Script:nodePrefix-FE')]`"");
        [void]$paramBuilder.AppendLine("},");
        [void]$paramBuilder.AppendLine("`"backendAddressPool`": {");
        [void]$paramBuilder.AppendLine("`"id`": `"[concat(resourceId('Microsoft.Network/loadBalancers', '$Script:nodePrefix-LB'), '/backendAddressPools/$Script:nodePrefix-BEPOOL')]`"");
        [void]$paramBuilder.AppendLine("},");
        [void]$paramBuilder.AppendLine("`"probe`": {");
        [void]$paramBuilder.AppendLine("`"id`": `"[concat(resourceId('Microsoft.Network/loadBalancers', '$Script:nodePrefix-LB'), '/probes/$Script:nodePrefix-PROBE')]`"");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("],");
        [void]$paramBuilder.AppendLine("`"probes`": [");
        [void]$paramBuilder.AppendLine("{");
        [void]$paramBuilder.AppendLine("`"name`": `"$Script:nodePrefix-PROBE`",");
        [void]$paramBuilder.AppendLine("`"properties`": {");
        [void]$paramBuilder.AppendLine("`"protocol`": `"Tcp`",");
        [void]$paramBuilder.AppendLine("`"port`": 59999,");
        [void]$paramBuilder.AppendLine("`"intervalInSeconds`": 10,");
        [void]$paramBuilder.AppendLine("`"numberOfProbes`": 5");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("]");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("},");
    }

    if ($vmSettings.os -ne "Linux" -and $vmSettings.os -ne "RPMBasedLinux") {
        # Append Antimalware Extension
        [void]$paramBuilder.AppendLine('{');
        [void]$paramBuilder.AppendLine("`"type`": `"Microsoft.Compute/virtualMachines/extensions`",");
        [void]$paramBuilder.AppendLine("`"name`": `"[concat(parameters('virtualMachineName'),'/IaaSAntimalware')]`",");
        [void]$paramBuilder.AppendLine("`"apiVersion`": `"2015-06-15`",");
        [void]$paramBuilder.AppendLine("`"location`": `"[parameters('location')]`",");
        [void]$paramBuilder.AppendLine("`"dependsOn`": [");
        [void]$paramBuilder.AppendLine("`"[concat('Microsoft.Compute/virtualMachines/', parameters('virtualMachineName'))]`"");
        [void]$paramBuilder.AppendLine("],");
        [void]$paramBuilder.AppendLine("`"properties`": { ");
        [void]$paramBuilder.AppendLine("`"publisher`": `"Microsoft.Azure.Security`",");
        [void]$paramBuilder.AppendLine("`"type`": `"IaaSAntimalware`",");
        [void]$paramBuilder.AppendLine("`"typeHandlerVersion`": `"1.3`",");
        [void]$paramBuilder.AppendLine("`"autoUpgradeMinorVersion`": `"true`",");
        [void]$paramBuilder.AppendLine("`"settings`": { ");
        [void]$paramBuilder.AppendLine("`"AntimalwareEnabled`": `"true`",");
        [void]$paramBuilder.AppendLine("`"Exclusions`": { ");
        [void]$paramBuilder.AppendLine("`"Extensions`": `"`",");
        [void]$paramBuilder.AppendLine("`"Paths`": `"`",");
        [void]$paramBuilder.AppendLine("`"Processes`": `"`"");
        [void]$paramBuilder.AppendLine("},");
        [void]$paramBuilder.AppendLine("`"RealtimeProtectionEnabled`": `"true`",");
        [void]$paramBuilder.AppendLine("`"ScheduledScanSettings`": { ");
        [void]$paramBuilder.AppendLine("`"isEnabled`": `"true`",");
        [void]$paramBuilder.AppendLine("`"scanType`": `"Quick`",");
        [void]$paramBuilder.AppendLine("`"day`": `"7`",");
        [void]$paramBuilder.AppendLine("`"time`": `"120`"");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("},");

        # Append Geneva Monitor Extension
        [void]$paramBuilder.AppendLine("{");
        [void]$paramBuilder.AppendLine("`"type`": `"Microsoft.Compute/virtualMachines/extensions`",");
        [void]$paramBuilder.AppendLine("`"name`": `"[concat(parameters('virtualMachineName'), '/Microsoft.Azure.Geneva.GenevaMonitoring')]`",");
        [void]$paramBuilder.AppendLine("`"apiVersion`": `"2018-10-01`",");
        [void]$paramBuilder.AppendLine("`"location`": `"[parameters('location')]`",");
        [void]$paramBuilder.AppendLine("`"dependsOn`": [");
        [void]$paramBuilder.AppendLine("`"[concat('Microsoft.Compute/virtualMachines/', parameters('virtualMachineName'))]`"");
        [void]$paramBuilder.AppendLine("],");
        [void]$paramBuilder.AppendLine("`"properties`": {");
        [void]$paramBuilder.AppendLine("`"publisher`": `"Microsoft.Azure.Geneva`",");
        [void]$paramBuilder.AppendLine("`"type`": `"GenevaMonitoring`",");
        [void]$paramBuilder.AppendLine("`"typeHandlerVersion`": `"2.0`",");
        [void]$paramBuilder.AppendLine("`"autoUpgradeMinorVersion`": true,");
        [void]$paramBuilder.AppendLine("`"enableAutomaticUpgrade`": true,");
        [void]$paramBuilder.AppendLine("`"settings`": {},");
        [void]$paramBuilder.AppendLine("`"protectedSettings`": {}");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("},");
    }

    # Append Network interface
    if ($vmSettings.os -eq "Linux" -or $vmSettings.os -eq "RPMBasedLinux") {
        # For Linux, create one network interface
        [void]$paramBuilder.AppendLine('{');
        [void]$paramBuilder.AppendLine("`"name`": `"[parameters('networkInterfaceName1')]`",");
        [void]$paramBuilder.AppendLine("`"type`": `"Microsoft.Network/networkInterfaces`",");
        [void]$paramBuilder.AppendLine("`"apiVersion`": `"2018-04-01`",");
        [void]$paramBuilder.AppendLine("`"location`": `"[parameters('location')]`",");
        [void]$paramBuilder.AppendLine("`"dependsOn`": [");
        [void]$paramBuilder.AppendLine("`"[concat('Microsoft.Network/networkSecurityGroups/', parameters('networkSecurityGroupName'))]`",");
        [void]$paramBuilder.AppendLine("],");

        [void]$paramBuilder.AppendLine("`"properties`": { ");
        [void]$paramBuilder.AppendLine("`"ipConfigurations`": [ ");
        if (($ips | Measure-Object).Count -ge 1) {
            $index = 1;
            $ips | ForEach-Object -Process {
                [void]$paramBuilder.AppendLine('{');
                [void]$paramBuilder.AppendLine("`"name`": `"ipconfig$index`",");
                [void]$paramBuilder.AppendLine("`"properties`": {");
                if ($index -eq 1) {
                    [void]$paramBuilder.AppendLine("`"primary`": true,");
                }
                [void]$paramBuilder.AppendLine("`"subnet`": {");
                [void]$paramBuilder.AppendLine("`"id`": `"[variables('subnetRef')]`"");
                [void]$paramBuilder.AppendLine("},");
                if ($Script:isAzureCluster -and ($vmName -match "NODE")) {
                    [void]$paramBuilder.AppendLine("`"loadBalancerBackendAddressPools`": [");
                    [void]$paramBuilder.AppendLine("{");
                    [void]$paramBuilder.AppendLine("`"id`": `"[concat(resourceId('Microsoft.Network/loadBalancers', '$Script:nodePrefix-LB'),'/backendAddressPools/$Script:nodePrefix-BEPOOL')]`"");
                    [void]$paramBuilder.AppendLine("}");
                    [void]$paramBuilder.AppendLine("],");
                }
                [void]$paramBuilder.AppendLine("`"privateIPAllocationMethod`": `"Static`",");
                [void]$paramBuilder.AppendLine("`"privateIPAddress`": `"[parameters('privateIPAddress$index')]`",");
                [void]$paramBuilder.AppendLine("}");
                [void]$paramBuilder.AppendLine("},");
                $index++;
            }
        }
        [void]$paramBuilder.AppendLine("],");
        [void]$paramBuilder.AppendLine("`"networkSecurityGroup`": { ");
        [void]$paramBuilder.AppendLine("`"id`": `"[variables('nsgId')]`"");
        [void]$paramBuilder.AppendLine("},");
        if ($dnsServer -ne $null -and $dnsServer -ne '') {
            [void]$paramBuilder.AppendLine("`"dnsSettings`": { ");
            [void]$paramBuilder.AppendLine("`"dnsServers`": ['$dnsServer','168.63.129.16']");
            [void]$paramBuilder.AppendLine("}");
        }
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("},");
    }
    else {
        # For Windows, Create multiple network interfaces
        if (($ips | Measure-Object).Count -ge 1) {
            $index = 1;
            $ips | ForEach-Object -Process {
                [void]$paramBuilder.AppendLine('{');
                [void]$paramBuilder.AppendLine("`"name`": `"[parameters('networkInterfaceName$index')]`",");
                [void]$paramBuilder.AppendLine("`"type`": `"Microsoft.Network/networkInterfaces`",");
                [void]$paramBuilder.AppendLine("`"apiVersion`": `"2018-04-01`",");
                [void]$paramBuilder.AppendLine("`"location`": `"[parameters('location')]`",");
                [void]$paramBuilder.AppendLine("`"dependsOn`": [");
                [void]$paramBuilder.AppendLine("`"[concat('Microsoft.Network/networkSecurityGroups/', parameters('networkSecurityGroupName'))]`",");
                if ($Script:isAzureCluster -and ($vmName -match "NODE")) {
                    [void]$paramBuilder.AppendLine("`"Microsoft.Network/loadBalancers/$Script:nodePrefix-LB`"");
                }
                [void]$paramBuilder.AppendLine("],");

                [void]$paramBuilder.AppendLine("`"properties`": { ");
                [void]$paramBuilder.AppendLine("`"ipConfigurations`": [ ");
                [void]$paramBuilder.AppendLine('{');
                [void]$paramBuilder.AppendLine("`"name`": `"ipconfig$index`",");
                [void]$paramBuilder.AppendLine("`"properties`": {");
                if ($index -eq 1) {
                    [void]$paramBuilder.AppendLine("`"primary`": true,");
                }
                [void]$paramBuilder.AppendLine("`"subnet`": {");
                [void]$paramBuilder.AppendLine("`"id`": `"[variables('subnetRef')]`"");
                [void]$paramBuilder.AppendLine("},");
                if ($Script:isAzureCluster -and ($vmName -match "NODE")) {
                    [void]$paramBuilder.AppendLine("`"loadBalancerBackendAddressPools`": [");
                    [void]$paramBuilder.AppendLine("{");
                    [void]$paramBuilder.AppendLine("`"id`": `"[concat(resourceId('Microsoft.Network/loadBalancers', '$Script:nodePrefix-LB'),'/backendAddressPools/$Script:nodePrefix-BEPOOL')]`"");
                    [void]$paramBuilder.AppendLine("}");
                    [void]$paramBuilder.AppendLine("],");
                }
                [void]$paramBuilder.AppendLine("`"privateIPAllocationMethod`": `"Static`",");
                [void]$paramBuilder.AppendLine("`"privateIPAddress`": `"[parameters('privateIPAddress$index')]`",");
                [void]$paramBuilder.AppendLine("}");
                [void]$paramBuilder.AppendLine("}");
                [void]$paramBuilder.AppendLine("],");
                [void]$paramBuilder.AppendLine("`"networkSecurityGroup`": { ");
                [void]$paramBuilder.AppendLine("`"id`": `"[variables('nsgId')]`"");
                [void]$paramBuilder.AppendLine("}");
                [void]$paramBuilder.AppendLine("},");
                [void]$paramBuilder.AppendLine("},");
                $index++;
            }
        }
    }
    ## Append network security group
    [void]$paramBuilder.AppendLine('{');
    [void]$paramBuilder.AppendLine("`"name`": `"[parameters('networkSecurityGroupName')]`",");
    [void]$paramBuilder.AppendLine("`"type`": `"Microsoft.Network/networkSecurityGroups`",");
    [void]$paramBuilder.AppendLine("`"apiVersion`": `"2021-03-01`",");
    [void]$paramBuilder.AppendLine("`"location`": `"[parameters('location')]`",");
    [void]$paramBuilder.AppendLine("`"properties`": {");
    [void]$paramBuilder.AppendLine("`"securityRules`": `"[parameters('networkSecurityGroupRules')]`"");
    [void]$paramBuilder.AppendLine("},");
    [void]$paramBuilder.AppendLine("`"tags`": {} ");
    [void]$paramBuilder.AppendLine("},");

    # Append OS, VM size, image...
    [void]$paramBuilder.AppendLine('{');
    [void]$paramBuilder.AppendLine("`"name`": `"[parameters('virtualMachineName')]`",");
    [void]$paramBuilder.AppendLine("`"type`": `"Microsoft.Compute/virtualMachines`",");
    [void]$paramBuilder.AppendLine("`"apiVersion`": `"2021-07-01`",");
    [void]$paramBuilder.AppendLine("`"location`": `"[parameters('location')]`",");
    [void]$paramBuilder.AppendLine("`"dependsOn`": [");
    if (($ips | Measure-Object).Count -ge 1) {
        $index = 1;
        $ips | ForEach-Object -Process {
            Write-TestSuiteInfo "Index = $index"
            if ($vmSettings.os -eq "Linux" -or $vmSettings.os -eq "RPMBasedLinux") {
                if($index -ne 1) {
                    return;
                }
            }
            [void]$paramBuilder.AppendLine("`"[concat('Microsoft.Network/networkInterfaces/', parameters('networkInterfaceName$index'))]`",");
            $index++;
        }
    }
    if ($Script:isAzureCluster -and ($vmName -match "NODE")) {
        [void]$paramBuilder.AppendLine("`"Microsoft.Compute/availabilitySets/$Script:nodePrefix-AS`"");
    }
    [void]$paramBuilder.AppendLine("],");
    [void]$paramBuilder.AppendLine("`"properties`": { ");
    # append availability set
    if ($Script:isAzureCluster -and ($vmName -match "NODE")) {
        [void]$paramBuilder.AppendLine("`"availabilitySet`": {");
        [void]$paramBuilder.AppendLine("`"id`": `"[resourceId('Microsoft.Compute/availabilitySets', '$Script:nodePrefix-AS')]`"");
        [void]$paramBuilder.AppendLine("},");
    }
    # append vm size
    [void]$paramBuilder.AppendLine("`"hardwareProfile`": { ");
    [void]$paramBuilder.AppendLine("`"vmSize`": `"[parameters('virtualMachineSize')]`"");
    [void]$paramBuilder.AppendLine("},");
    # append storage image
    [void]$paramBuilder.AppendLine("`"storageProfile`": { ");
    [void]$paramBuilder.AppendLine("`"osDisk`": {");
    [void]$paramBuilder.AppendLine("`"createOption`": `"fromImage`",");
    [void]$paramBuilder.AppendLine("`"managedDisk`": { ");
    [void]$paramBuilder.AppendLine("`"storageAccountType`": `"[parameters('osDiskType')]`"");
    [void]$paramBuilder.AppendLine("}");
    [void]$paramBuilder.AppendLine("},");
    [void]$paramBuilder.AppendLine("`"imageReference`": { ");
    if ($null -eq $imageRefId -and $null -eq $galleryName) {
        [void]$paramBuilder.AppendLine("`"publisher`": `"[parameters('publisher')]`",");
        [void]$paramBuilder.AppendLine("`"offer`": `"[parameters('offer')]`",");
        [void]$paramBuilder.AppendLine("`"sku`": `"[parameters('sku')]`",");
        [void]$paramBuilder.AppendLine("`"version`": `"[parameters('version')]`"");
    }
    else {
        [void]$paramBuilder.AppendLine("`"id`": `"[parameters('imageRefId')]`"");
    }
    [void]$paramBuilder.AppendLine("}");
    [void]$paramBuilder.AppendLine("},");

    # append network profile
    [void]$paramBuilder.AppendLine("`"networkProfile`": { ");
    [void]$paramBuilder.AppendLine("`"networkInterfaces`": [");
    if (($ips | Measure-Object).Count -ge 1) {
        $index = 1;
        $ips | ForEach-Object -Process {
            Write-TestSuiteInfo "Index = $index"
            if ($vmSettings.os -eq "Linux" -or $vmSettings.os -eq "RPMBasedLinux") {
                if($index -ne 1) {
                    return;
                }
            }
            [void]$paramBuilder.AppendLine('{');
            [void]$paramBuilder.AppendLine("`"properties`": { ");
            if ($index -eq 1) {
                [void]$paramBuilder.AppendLine("`"primary`": true,");
            }
            else {
                [void]$paramBuilder.AppendLine("`"primary`": false,");
            }
            [void]$paramBuilder.AppendLine("},");
            [void]$paramBuilder.AppendLine("`"id`": `"[resourceId('Microsoft.Network/networkInterfaces', parameters('networkInterfaceName$index'))]`"");
            [void]$paramBuilder.AppendLine("},");
            $index++;
        }
    }
    [void]$paramBuilder.AppendLine("]");
    [void]$paramBuilder.AppendLine("},");

    #append os profile
    [void]$paramBuilder.AppendLine("`"osProfile`": { ");
    [void]$paramBuilder.AppendLine("`"computerName`": `"[parameters('virtualMachineName')]`",");
    [void]$paramBuilder.AppendLine("`"adminUsername`": `"[parameters('adminUsername')]`",");
    [void]$paramBuilder.AppendLine("`"adminPassword`": `"[parameters('adminPassword')]`",");
    if ($vmSettings.os -ne "Linux" -and $vmSettings.os -ne "RPMBasedLinux" ) {
        [void]$paramBuilder.AppendLine("`"windowsConfiguration`": { ");
        [void]$paramBuilder.AppendLine("`"enableAutomaticUpdates`": true,");
        [void]$paramBuilder.AppendLine("`"provisionVmAgent`": true,");
        [void]$paramBuilder.AppendLine("`"additionalUnattendContent`": [");
        [void]$paramBuilder.AppendLine('{');
        [void]$paramBuilder.AppendLine("`"passName`": `"OobeSystem`",");
        [void]$paramBuilder.AppendLine("`"componentName`": `"Microsoft-Windows-Shell-Setup`",");
        [void]$paramBuilder.AppendLine("`"settingName`": `"AutoLogon`",");
        [void]$paramBuilder.AppendLine("`"content`": `"[concat('<AutoLogon><Domain>', parameters('virtualMachineName'), '</Domain><Username>', parameters('adminUsername'), '</Username><Password><Value>', parameters('adminPassword'), '</Value></Password><LogonCount>999</LogonCount><Enabled>true</Enabled></AutoLogon>')]`"");
        [void]$paramBuilder.AppendLine("},");
        # Disable OOBE setup, Disable the system firewall, Enable PowerShell remoting, Enable WinRM by quickconfig, Set PowerShell script execution policy
        [void]$paramBuilder.AppendLine('{');
        [void]$paramBuilder.AppendLine("`"passName`": `"OobeSystem`",");
        [void]$paramBuilder.AppendLine("`"componentName`": `"Microsoft-Windows-Shell-Setup`",");
        [void]$paramBuilder.AppendLine("`"settingName`": `"FirstLogonCommands`",");
        [void]$paramBuilder.AppendLine("`"content`": `"<FirstLogonCommands><SynchronousCommand><RequiresUserInput>false</RequiresUserInput><CommandLine>powershell.exe Set-ExecutionPolicy -ExecutionPolicy ByPass -Force</CommandLine><Description>set powershell ExecutionPolicy</Description><Order>1</Order></SynchronousCommand><SynchronousCommand><RequiresUserInput>false</RequiresUserInput><CommandLine>powershell.exe netsh advfirewall set allprofiles state off</CommandLine><Description>Disable Firewall</Description><Order>2</Order></SynchronousCommand><SynchronousCommand><RequiresUserInput>false</RequiresUserInput><CommandLine>powershell.exe Enable-PSRemoting -Force</CommandLine><Description>Enable PSRemoting</Description><Order>3</Order></SynchronousCommand><SynchronousCommand><RequiresUserInput>false</RequiresUserInput><CommandLine>cmd.exe /C call winrm qc -q</CommandLine><Description>Configure WinRm</Description><Order>4</Order></SynchronousCommand><SynchronousCommand><RequiresUserInput>false</RequiresUserInput><CommandLine>cmd /C reg add HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\OOBE /v DisablePrivacyExperience /t REG_DWORD /d 1 /f</CommandLine><Description>Disable OOBESetup</Description><Order>5</Order></SynchronousCommand></FirstLogonCommands>`"");
        [void]$paramBuilder.AppendLine("}");
        [void]$paramBuilder.AppendLine("]");
        [void]$paramBuilder.AppendLine("}");
    }
    else {
        [void]$paramBuilder.AppendLine("`"linuxConfiguration`": { ");
        if ($usePublicKey) {
            [void]$paramBuilder.AppendLine("`"provisionVmAgent`": true,");
            [void]$paramBuilder.AppendLine("`"ssh`": {");
            [void]$paramBuilder.AppendLine("`"publicKeys`": [");
            [void]$paramBuilder.AppendLine('{');
            [void]$paramBuilder.AppendLine("`"path`": `"[concat('/home/', parameters('adminUsername'), '/.ssh/authorized_keys')]`",");
            [void]$paramBuilder.AppendLine("`"keyData`": `"[parameters('adminPublicKey')]`"");
            [void]$paramBuilder.AppendLine("}");
            [void]$paramBuilder.AppendLine("]");
            [void]$paramBuilder.AppendLine("}");
        }
        else {
            [void]$paramBuilder.AppendLine("`"patchSettings`": {");
            [void]$paramBuilder.AppendLine("`"patchMode`": `"ImageDefault`"");
            [void]$paramBuilder.AppendLine("}");
        }
        [void]$paramBuilder.AppendLine("}");
    }
    [void]$paramBuilder.AppendLine("}");
    [void]$paramBuilder.AppendLine("},");
    if (($vmSettings.imageReference.sku -match "-preview") -or ($vmSettings.imageReference.offer -match "-preview")) {
        $publisher = $vmSettings.imageReference.publisher
        $offer = $vmSettings.imageReference.offer
        $sku = $vmSettings.imageReference.sku
        [void]$paramBuilder.AppendLine("`"plan`": {");
        [void]$paramBuilder.AppendLine("`"name`": `"$sku`",");
        [void]$paramBuilder.AppendLine("`"publisher`": `"$publisher`",");
        [void]$paramBuilder.AppendLine("`"product`": `"$offer`"");
        [void]$paramBuilder.AppendLine("}, ") ;
    }
    [void]$paramBuilder.AppendLine("`"tags`": {} ") ;
    [void]$paramBuilder.AppendLine("}");
    [void]$paramBuilder.AppendLine("],");
    [void]$paramBuilder.AppendLine("`"outputs`": { ");
    [void]$paramBuilder.AppendLine("`"adminUsername`": { ");
    [void]$paramBuilder.AppendLine("`"type`": `"string`",");
    [void]$paramBuilder.AppendLine("`"value`": `"[parameters('adminUsername')]`"");
    [void]$paramBuilder.AppendLine("}");
    [void]$paramBuilder.AppendLine("}");
    [void]$paramBuilder.AppendLine("}");

    $paramPath = "$deployPath\template.json"

    $streamWriter = [System.IO.StreamWriter] $paramPath
    $streamWriter.Write($paramBuilder.ToString());
    $streamWriter.Close();

    $ctx = New-AzStorageContext -StorageAccountName "wintestsuiteresults" -EnableFileBackupRequestIntent
    $Blob = @{
        File             = $paramPath
        Container        = "exportedfiles"
        Blob             = "$RandomNum-template.json"
        Context          = $ctx
        StandardBlobTier = 'Hot'
        }
    Set-AzStorageBlobContent @Blob -Force
}

Function GenerateTemplateFiles {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string] $vmName
    )

    $deployPath = "$scriptPath\$TestSuiteName\$vmName"
    if (Test-Path -Path $deployPath) {
        Write-TestSuiteWarning "Remove existing Folder: $deployPath"
        Remove-Item $deployPath -Force -Recurse
    }

    New-Item -ItemType Directory -Path $deployPath

    GenerateParameterJsonFile -vmName $vmName

    GenerateTemplateJsonFile -vmName $vmName
}

Function FindAndCopyConfiguredScripts {
    Param(
        [string] $destinationFolder,
        [string] $scripts,
        [string] $wrapScriptName,
        [string] $workingFolder
    )

    if (![string]::IsNullOrEmpty($scripts)) {
        if (Test-Path -Path "$destinationFolder\$wrapScriptName.ps1") {
            Remove-Item -Path "$destinationFolder\$wrapScriptName.ps1" -Force
        }

        foreach ($scriptItem in [Array]$scripts.Split(";")) {
            $configScriptItemPath = "";
            if (Test-Path -Path $($destinationFolder + "\" + $scriptItem)) {
                Write-TestSuiteInfo "$scriptItem found in Temp folder. "
                $configScriptItemPath = $scriptItem
            }
            elseif (Test-Path -Path $("$destinationFolder\Scripts\$PostScript")) {
                Write-TestSuiteInfo "$ConfigScript found in Scripts folder."
                $configScriptItemPath = "Scripts\$scriptItem";
            }
            else {
                Write-TestSuiteWarning "$scriptItem script not found."
                continue
            }
            Write-TestSuiteInfo "Add the script to $wrapScriptName.ps1"
            $Vm = $Script:Setup.lab.servers.vm | Where-Object { $_.name -like $vmName }
            [string]$CmdLine = "Write-Host `"Running $scriptItem`""
            Add-Content -Path $("$destinationFolder\$wrapScriptName.ps1") -Value $CmdLine -Force
            if ($Vm.os -eq "Linux" -or $Vm.os -eq "RPMBasedLinux") {
                Add-Content -Path $("$destinationFolder\$wrapScriptName.ps1") -Value ("pwsh " + $workingFolder + $configScriptItemPath) -Force
            } else {
                Add-Content -Path $("$destinationFolder\$wrapScriptName.ps1") -Value ("powershell " + $workingFolder + $configScriptItemPath) -Force
            }
        }
    }
}

Function PrepareTools {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string] $vmName
    )

    $Vm = $Script:Setup.lab.servers.vm | Where-Object { $_.name -like $vmName }
    $destinationFolder = "$scriptPath\$TestSuiteName\$vmName" + "_Temp"

    if (![string]::IsNullOrEmpty($Vm.tools)) {
        if (![string]::IsNullOrEmpty($Vm.tools.tool)) {
            if ($Vm.tools.tool.Count -gt 0) {
                Write-TestSuiteInfo "There are $($Vm.tools.tool.Count) tools to be installed."
            }
            else {
                Write-TestSuiteInfo "There is only 1 tool to be installed."
            }
            foreach ($Tool in $Vm.tools.tool) {
                [string]$toolFullPath = ""
                [string]$toolRelatedPath = ""
                [string]$toolName = ""

                if ($Tool.HasAttribute("MSIName")) {
                    $toolName = $Tool.MSIName
                }
                elseif ($Tool.HasAttribute("ZipName")) {
                    $toolName = $Tool.ZipName
                }
                else {
                    $toolName = $Tool.EXEName
                }

                if ($Tool.version -ne $null) {
                    $toolRelatedPath = "/" + $Tool.name + "/" + $Tool.version + "/" + $Tool.CPUArchitecture + "/" + $toolName
                    $localToolFolder = $Script:toolLibPath + "/" + $Tool.name + "/" + $Tool.version + "/" + $Tool.CPUArchitecture
                }
                else {
                    $toolRelatedPath = "/" + $Tool.name + "/" + $toolName
                    $localToolFolder = $Script:toolLibPath + "/" + $Tool.name
                }
                $toolFullPath = $Script:toolLibPath.replace("\","/") + $toolRelatedPath

                if (!(Test-Path $localToolFolder)) {
                    New-Item -ItemType Directory $localToolFolder
                }

                if (!(Test-Path -Path $toolFullPath)) {
                    # get file from deploy
                    $toolSourceFullPath = "$RegressionRootPath/ProtocolTestSuite/$TestSuiteName/deploy/$toolName"
                    if (Test-Path -Path $toolSourceFullPath) {
                        Write-TestSuiteInfo "$toolName found in $($RegressionRootPath) folder. Copy it to temp directory..."
                        Copy-Item -Path $toolSourceFullPath -Destination $toolFullPath -force
                    }
                    else {
                        #download from Azure file
                        Write-TestSuiteWarning "Download file from /ToolShare$toolRelatedPath to $toolFullPath"
                        Write-Host 'Create storage account context.'
                        $ctx = New-AzStorageContext -StorageAccountName $storageAccount -EnableFileBackupRequestIntent
                        Write-Host 'Download File Using Storage Context.'
                        Get-AzStorageFileContent -ShareName $storageShareName -Path "/ToolShare$toolRelatedPath" -Destination "$toolFullPath" -Context $ctx
                    }
                }

                if (Test-Path -Path $toolFullPath) {
                    Write-TestSuiteInfo "$toolName found in $Script:toolLibPath folder. "
                    Write-TestSuiteInfo "Source: $toolFullPath, Destination: $destinationFolder\Deploy\"
                    Copy-Item $toolFullPath -Destination "$destinationFolder\Deploy\"
                }
                else {
                    Write-TestSuiteWarning "$toolName file not found. File path: $toolFullPath"
                }
            }
        }

        if (![string]::IsNullOrEmpty($Vm.tools.TestsuiteMSI)) {
            $msiFullPath = "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\deploy\$($Vm.tools.TestsuiteMSI.MSIName)"
            if (Test-Path -Path $msiFullPath) {
                Write-TestSuiteInfo "$($Vm.tools.TestsuiteMSI.Name) found in $($RegressionRootPath) folder. Copy it to temp directory..."
                Copy-Item -Path $msiFullPath -Destination "$destinationFolder\Deploy\" -force
            }
            else {
                Write-TestSuiteWarning "$($Vm.tools.TestsuiteMSI.Name) file not found, File path: $msiFullPath."
            }
        }

        if (![string]::IsNullOrEmpty($Vm.tools.TestsuiteZip)) {
            $zipFullPath = "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\deploy\$($Vm.tools.TestsuiteZip.ZipName)"
            if (Test-Path -Path $zipFullPath) {
                Write-TestSuiteInfo "$($Vm.tools.TestsuiteZip.ZipName) found in $($RegressionRootPath) folder. Copy it to temp directory..."
                Copy-Item -Path $zipFullPath -Destination "$destinationFolder\Deploy\" -force
            }
            else {
                Write-TestSuiteWarning "$($Vm.tools.TestsuiteZip.ZipName) file not found, File path: $zipFullPath."
            }
        }
    }
}
Function Prepare-TestSuiteFiles {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string] $vmName
    )
    $deployUser = $Script:Setup.lab.core.username;
    $Vm = $Script:Setup.lab.servers.vm | Where-Object { $_.name -like $vmName }
    $workingFolder = "C:\Temp\"
    if ($Vm.os -eq "Linux" -or $Vm.os -eq "RPMBasedLinux") {
        $workingFolder = "/home/$deployUser/Temp/"
    }
    $destinationFolder = "$scriptPath\$TestSuiteName\$vmName" + "_Temp"
    if (-not (Test-Path -Path $destinationFolder)) {
        New-Item -ItemType Directory $destinationFolder
    }
    else {
        Remove-Item "$destinationFolder\*.*" -Force -Recurse
    }
    Write-TestSuiteInfo "Temp Folder: $destinationFolder"

    $scriptsFolder = "$destinationFolder\Scripts"
    Write-TestSuiteInfo "Temp Script Folder: $scriptsFolder"

    if (-Not (Test-Path -Path $scriptsFolder)) {
        New-Item -ItemType Directory $scriptsFolder
    }

    if (Test-Path -Path "$RegressionRootPath\ScriptLib") {
        Copy-Item "$RegressionRootPath\ScriptLib\*.*" -Destination $destinationFolder -Force -Recurse
    }
    if (Test-Path -Path "$RegressionRootPath\VSTORMLITE\Install") {
        Copy-Item "$RegressionRootPath\VSTORMLITE\Install\*.*" -Destination $destinationFolder -Force -Recurse
    }
    if (Test-Path -Path "$RegressionRootPath\VSTORMLITE\PostScript") {
        Copy-Item "$RegressionRootPath\VSTORMLITE\PostScript\*.*" -Destination $destinationFolder -Force -Recurse
    }
    if (Test-Path -Path "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\PostScript") {
        Copy-Item "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\PostScript\*.*" -Destination $destinationFolder -Force -Recurse
    }
    if (Test-Path -Path "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\Scripts\") {
        Copy-Item "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\Scripts\*.*" -Destination $scriptsFolder -Force -Recurse
    }
    if (Test-Path -Path "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\Tools") {
        Copy-Item "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\Tools\" -Destination "$destinationFolder" -Force -Recurse
    }
    if (Test-Path -Path "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\Data") {
        Copy-Item "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\Data\" -Destination "$destinationFolder" -Force -Recurse
    }

    Copy-Item $configFile -Destination "$destinationFolder\Protocol.xml" -Force -Recurse

    # prepare install script

    Write-TestSuiteStep "Find the install VS scripts from the ConfigScript or Install folders."
    FindAndCopyConfiguredScripts -destinationFolder $destinationFolder -scripts $Vm.installvisualstudio -wrapScriptName "InstallVisualStudio" -workingFolder $workingFolder

    Write-TestSuiteStep "Find the Install Scripts from the PostScript or Install folders."
    FindAndCopyConfiguredScripts -destinationFolder $destinationFolder -scripts $Vm.installscript -wrapScriptName "Install" -workingFolder $workingFolder

    Write-TestSuiteStep "Find the Install Feature Scripts from the PostScript or Install folders."
    FindAndCopyConfiguredScripts -destinationFolder $destinationFolder -scripts $Vm.installfeaturescript -wrapScriptName "InstallFeatureScript" -workingFolder $workingFolder

    Write-TestSuiteStep "Find the Post Scripts from the PostScript or Install folders."
    FindAndCopyConfiguredScripts -destinationFolder $destinationFolder -scripts $Vm.postscript -wrapScriptName "Post" -workingFolder $workingFolder

    Write-TestSuiteStep "Record host name $vmName in file Name.txt to the temp directory."
    $Vm.name > $destinationFolder\Name.txt

    Write-TestSuiteStep "Record logon information to the Temp directory."
    $VMDomainName = "WORKGROUP"

    if ([string]::IsNullOrEmpty($Vm.domain)) {
        $VmNetbios = "WORKGROUP"
    }
    else {
        $VMDomainName = $Vm.domain
        $VmNetbios = ($VMDomainName.Split("."))[0]
    }

    $VmUsername = $Script:Setup.lab.core.username;
    $VmPassword = $Script:Setup.lab.core.password;
    $VMDomainName > $destinationFolder\DomainDnsName.txt
    $VmNetbios > $destinationFolder\DomainNetbiosName.txt
    $VmUsername > $destinationFolder\DomainAdminName.txt
    $VmPassword > $destinationFolder\DomainAdminPwd.txt

    $deployFolder = "$destinationFolder\Deploy"
    if (-not (Test-Path -Path $deployFolder)) {
        New-Item -ItemType Directory $deployFolder
    }
}

Function Main {

    Read-TestSuiteXml

    if (!(Test-Path -Path $Script:toolLibPath)) {
        New-Item -ItemType Directory $Script:toolLibPath
    }

    $Script:isAzureCluster = ($TestSuiteName -match "FileServer") -and ($Script:Setup.lab.ha.cluster -ne $null)
    if ($Script:isAzureCluster) {
        $Script:nodePrefix = ($Script:Setup.lab.servers.vm)[0].name.Split('-')[0]
        $Script:lbIp = $Script:Setup.lab.ha.generalfs.ip
    }

    Write-TestSuiteInfo "Generate deploy template and temp folder for VMs"
    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort-Object -Property installorder)) {
        $RandomNum = Get-Random -Maximum 10000
        GenerateTemplateFiles -vmName $Vm.name
        Prepare-TestSuiteFiles -vmName $Vm.name
        PrepareTools -vmName $Vm.name
    }
}

Write-TestSuiteInfo "TestSuiteRootPath  : $RegressionRootPath"
Write-TestSuiteInfo "scriptPath         : $scriptPath"
Write-TestSuiteInfo "ResourceGroupName  : $fileShareResourceGroup"
Write-TestSuiteInfo "StorageAccount     : $storageAccount"
Set-AzCurrentStorageAccount -ResourceGroupName $fileShareResourceGroup -Name $storageAccount

Main

Pop-Location