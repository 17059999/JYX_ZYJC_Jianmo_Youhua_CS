###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

Param(
    [string]$vmName = "defaultVm",
    [string]$vmAccount = "iolab",
    [string]$vmPassword = "password!",
    [boolean]$usePipelineParams = $true,
    [string]$resourceGroup = "TestSuiteOnAzure",
    # Azure Storage Account Name
    [string]$storageAccount = "testsuitesshare",
    [string]$storageAccountKey,
    [string]$containerName = "azureregressionshare",
    [string]$filepath,
    [string]$vaultName = "TestSuiteKeyVault",
    # Azure ApplicationId
    [string]$applicationId = "ded7315a-2d0b-47b0-8b4f-9d34eb0a6288",
    # Connect to azure certificate thumbprint
    [string]$thumbPrint = "c13a03e41fe0220fab4b688951fa986e8441385b",
    # Azure TenantId
    [string]$tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    [string]$certPassword = "123",
    [string]$token
)

$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent;
$RegressionRootPath = "$scriptPath\..\"
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

Function Write-TestSuiteWarning {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [switch]$Exit)

    Write-TestSuiteInfo -Message "[WARNING]: $Message" -ForegroundColor Yellow -BackgroundColor Black
    if ($Exit) { exit 1 }
}

#------------------------------------------------------------------------------------------
# Connect to AzureRm
#------------------------------------------------------------------------------------------
Function ConnectToAz {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$thumbPrint,
        [string]$applicationId,
        [string]$tenantId)

    Process {
        #Import-Module AzureRM -ErrorAction Stop
        Write-Host "Connecting to Az"
        Connect-AzAccount -CertificateThumbprint $thumbPrint -ApplicationId $applicationId -TenantId $tenantId -ErrorAction Stop
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
# Read and parse XML configuration file
# $Setup will be used as a global variable to store the configuration information
#------------------------------------------------------------------------------------------
Function Read-TestSuiteXml {

    Write-TestSuiteInfo "Read and parse the XML configuration file."

    Write-TestSuiteInfo "Check if the XML configuration file exist or not."
    # If $XmlFileFullPath is not found, prompt a list of choices for user to choose
    if (!(Test-Path -Path $Script:XmlFileFullPath)) {
        Write-TestSuiteInfo "$Script:XmlFileFullPath file not found."
    }
    else {
        Write-TestSuiteInfo "$Script:XmlFileFullPath file found."
    }

    # Read contents from the XML file
    Write-TestSuiteInfo "Read contents from the XML configuration file."
    [Xml]$Script:Setup = Get-Content $Script:XmlFileFullPath
    if ($null -eq $Script:Setup) {
        Write-TestSuiteInfo "$Script:XmlFileFullPath file is not a valid xml configuration file." -Exit
    }
    else {
        $Script:Setup | Format-TestSuiteXml -Indent 4
    }
}

$ExecuteS360Setting = {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$vmName,
        [string]$resourceGroup,
        [string]$vmAccount,
        [string]$vmPassword,
        [string]$storageAccount,
        [string]$storageAccountKey,
        [string]$containerName,
        [string]$filepath,
        [string]$vaultName,
        [string]$applicationId,
        [string]$thumbPrint,
        [string]$tenantId,
        [string]$certPassword,
        [string]$scriptPath,
        [string]$token
    )

    #------------------------------------------------------------------------------------------
    # Enable Encryption...
    #------------------------------------------------------------------------------------------
    Function EnableEncryption-TestSuiteVM {
        Write-TestSuiteInfo "Gets information about the key vaults $vaultName"
    
        $KeyVault = Get-AzKeyVault -VaultName $vaultName
        $DiskEncryptionKeyVaultUrl = $KeyVault.VaultUri
        $KeyVaultResourceId = $KeyVault.ResourceId

        Write-TestSuiteInfo "Enables encryption on $vmName"
        if ($osType -ne "Linux") {
            Set-AzVMDiskEncryptionExtension -ResourceGroupName $vmResourceGroup -VMName $vmName -DiskEncryptionKeyVaultUrl $DiskEncryptionKeyVaultUrl -DiskEncryptionKeyVaultId $KeyVaultResourceId -Force
        }
        else {
            # for Linux OS, execute the below Encryption method with special parameters
            #When encrypting Linux OS volumes, the VM should be considered unavailable. We strongly recommend to avoid SSH logins while the encryption is in progress to avoid issues blocking any open files that will need to be accessed during the encryption process.
            #Set-AzureRmVMDiskEncryptionExtension -ResourceGroupName $resourceGroup -VMName $vmName -DiskEncryptionKeyVaultUrl $DiskEncryptionKeyVaultUrl -DiskEncryptionKeyVaultId $KeyVaultResourceId -SkipVmBackup -VolumeType All -Force
        }
    }

    #------------------------------------------------------------------------------------------
    # enable Antimalware ,data from Prepare-VMFiles.ps1
    #------------------------------------------------------------------------------------------
    Function EnableAntimalware-TestSuiteVM {

        Write-TestSuiteInfo "enable Antimalware on $vmName"
        if ($osType -ne "Linux") {
            Set-AzVMExtension -ResourceGroupName $vmResourceGroup -Location $location -VMName $vmName -Name "IaaSAntimalware" -Publisher "Microsoft.Azure.Security" -ExtensionType "IaaSAntimalware" -TypeHandlerVersion "1.3"
        }
    }
    
    Function RunExtensionScriptOnRemoteVM {
        param (
            [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
            [string]$ExtenstionName,
            [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
            [string]$ResourceGroup,
            [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
            [string]$Location,
            [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
            [string]$SettingsString,
            [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
            [string]$VMName
        )

        $jobs = @()

        Write-TestSuiteInfo "Start execute extension job $ExtenstionName on $VMName"

        $ProtectedSettingsString = '{"storageAccountName":"' + $storageAccount + '","storageAccountKey":"' + $storageAccountKey + '"}';

        $jobName = $VMName + "_" + $ExtenstionName

        if ($osType -ne "Linux") {
            $job = Set-AzVMExtension -Name $jobName `
                -ResourceGroupName $ResourceGroup `
                -VMName $VMName `
                -Publisher "Microsoft.Compute" `
                -ExtensionType "CustomScriptExtension" `
                -TypeHandlerVersion "1.0" `
                -SettingString $SettingsString `
                -Location $Location `
                -ProtectedSettingString $ProtectedSettingsString `
                -AsJob
        }
        else {
            $job = Set-AzVMExtension -Name $jobName `
                -ResourceGroupName $ResourceGroup `
                -VMName $VMName `
                -Publisher "Microsoft.Azure.Extensions" `
                -type "CustomScript" `
                -TypeHandlerVersion "2.1" `
                -SettingString $SettingsString `
                -Location $Location `
                -ProtectedSettingString $ProtectedSettingsString `
                -AsJob
        }

        $jobs += $job;

        Write-TestSuiteInfo "waiting extension job $ExtenstionName complete"

        foreach ($job in $jobs) {
            Wait-Job -id $job.Id -Timeout 3600 | Receive-Job -ErrorAction SilentlyContinue
        }

        Write-TestSuiteInfo "Remove extension job $ExtenstionName"

        $jobName = $VMName + "_" + $ExtenstionName

        Remove-AzVMExtension -ResourceGroupName $ResourceGroup -VMName $VMName -Name $jobName -Force

        Write-TestSuiteInfo "Remove extension job $ExtenstionName complete"
    }
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

    $vm = Get-AzVM -name $vmName -ResourceGroupName $resourceGroup
    if ($vm -eq $null) {
        Write-Host "$vmName is not exist in $resourceGroup"
        exit
    }
    $vmResourceGroup = $resourceGroup
    $location = $vm.Location
    $osType = $vm.StorageProfile.OsDisk.OsType

    Write-Host "osType:$osType"
    Write-Host "vmResourceGroup:$vmResourceGroup"
    Write-Host "location:$location"

    EnableEncryption-TestSuiteVM

    EnableAntimalware-TestSuiteVM
    
    $storageAccountKeys = Get-AzStorageAccountKey -ResourceGroupName $vmResourceGroup -AccountName $storageAccount
    
    if ($null -eq $storageAccountKeys -or $storageAccountKeys.Count -eq 0) {
    Write-TestSuiteInfo "Cannot find valid StorageAccountKey!"
    return
    }
    if ($storageAccountKeys.Count -gt 0) {        
        if ([string]::IsNullOrEmpty($token))
        {
            Write-TestSuiteInfo "Token is not provided. Exiting." -ForegroundColor Red
            exit 1
        }

        $context = New-AzStorageContext -StorageAccountName "$storageAccount" -SasToken "$token"
        write-host "$context is ready"
        $container = Get-AzStorageContainer -Name $containerName -Context $context -ErrorAction SilentlyContinue
        write-host "`$container is ready:$container"
        
        if ($container -eq $null) {
            #New-AzStorageContainer -Name $containerName -Context $context
        }

        write-host "set blobcontent:$context "
        Set-AzStorageBlobContent -Container $containerName -File "$ScriptPath\ApplyS360.ps1" -Blob "ApplyS360.ps1" -Context $context -Force
        Set-AzStorageBlobContent -Container $containerName -File $filepath -Blob "AzureAccountCert.pfx" -Context $context -Force
        Set-AzStorageBlobContent -Container $containerName -File "$ScriptPath\Linux\InstallGenevaMonitorToLinux.ps1" -Blob "InstallGenevaMonitorToLinux.ps1" -Context $context -Force
        Set-AzStorageBlobContent -Container $containerName -File "$ScriptPath\TLSSettings.ps1" -Blob "TLSSettings.ps1" -Context $context -Force
        Set-AzStorageBlobContent -Container $containerName -File "$ScriptPath\Linux\Install-PSCore.sh" -Blob "Install-PSCore.sh" -Context $context -Force
    }
    else {
        Write-TestSuiteInfo "Cannot find StorageAccountKey!" 
    }

    if ($osType -eq "Linux") {
        $ExtenstionName = "InstallPSCore"
        $settings = '{"fileUris":["https://' + $storageAccount + '.blob.core.windows.net/' + $containerName + '/Install-PSCore.sh"],"commandToExecute": "sh Install-PSCore.sh"}'
        
        RunExtensionScriptOnRemoteVM -ExtenstionName $ExtenstionName `
            -ResourceGroup $vmResourceGroup `
            -Location $location `
            -SettingsString $settings `
            -VMName $vmName
    }

    $ExtenstionName = "ApplyS360"

    if ($osType -eq "Linux") {
        $settings = '{"fileUris":["https://' + $storageAccount + '.blob.core.windows.net/' + $containerName + '/ApplyS360.ps1","https://' + $storageAccount + '.blob.core.windows.net/' + $containerName + '/AzureAccountCert.pfx","https://' + $storageAccount + '.blob.core.windows.net/' + $containerName + '/InstallGenevaMonitorToLinux.ps1"],"commandToExecute": "pwsh -File \"./ApplyS360.ps1\" ' + $vmName + ' ' + $vmAccount + ' ' + $vmPassword + ' ' + $osType + ' ' + $applicationId + ' ' + $thumbPrint + ' ' + $tenantId + ' ' + $vaultName + ' ' + $vmResourceGroup + ' ' + $location + ' ' + $certPassword + '"}';
    }
    else {
        $settings = '{"fileUris":["https://' + $storageAccount + '.blob.core.windows.net/' + $containerName + '/ApplyS360.ps1","https://' + $storageAccount + '.blob.core.windows.net/' + $containerName + '/AzureAccountCert.pfx","https://' + $storageAccount + '.blob.core.windows.net/' + $containerName + '/TLSSettings.ps1"],"commandToExecute": "powershell.exe -File \"./ApplyS360.ps1\" ' + $vmName + ' ' + $vmAccount + ' ' + $vmPassword + ' ' + $osType + ' ' + $applicationId + ' ' + $thumbPrint + ' ' + $tenantId + ' ' + $vaultName + ' ' + $vmResourceGroup + ' ' + $location + ' ' + $certPassword + '"}';
    }

    write-host "RunExtensionScriptOnRemoteVM -ExtenstionName $ExtenstionName"
    RunExtensionScriptOnRemoteVM -ExtenstionName $ExtenstionName `
        -ResourceGroup $vmResourceGroup `
        -Location $location `
        -SettingsString $settings `
        -VMName $vmName
}

Function Main {
    if (-not(Get-Module -name Az)) {
        Install-Module -Name Az -AllowClobber -force
        Import-Module -name Az
    }

    if (!$usePipelineParams) {
        $Script:XmlFileFullPath = "$RegressionRootPath..\TestSuites\AzureVm.xml"

        Read-TestSuiteXml

        $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup
        $vmAccount = $Script:Setup.lab.core.username
        $vmPassword = $Script:Setup.lab.core.password

        foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort-Object -Property installorder)) {
            $vmName = $Vm.name;
            Start-Job -Scriptblock $ExecuteS360Setting -ArgumentList $vmName, $resourceGroup, $vmAccount, $vmPassword, $storageAccount, $storageAccountKey, $containerName, $filepath, $vaultName, $applicationId, $thumbPrint, $tenantId, $certPassword, $scriptPath, $token
        }
    }
    else {
        Start-Job -Scriptblock $ExecuteS360Setting -ArgumentList $vmName, $resourceGroup, $vmAccount, $vmPassword, $storageAccount, $storageAccountKey, $containerName, $filepath, $vaultName, $applicationId, $thumbPrint, $tenantId, $certPassword, $scriptPath, $token
    }

    While ($(Get-Job -State Running).count -gt 0) {
        start-sleep 120
    }
    #Get information from each job.
    foreach ($job in Get-Job) {
        $info = Receive-Job -Id ($job.Id)
        Write-Host $info
    }
    #Remove all jobs created.
    Get-Job | Remove-Job
}

Main

Pop-Location