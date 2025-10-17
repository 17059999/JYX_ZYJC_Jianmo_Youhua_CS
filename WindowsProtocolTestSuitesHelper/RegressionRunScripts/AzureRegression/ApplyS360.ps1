Param(
    [string]$vmName,
    [string]$vmAccount,
    [string]$vmPassword,
    [string]$osType,
    # Azure ApplicationId
    [string]$applicationId = "ded7315a-2d0b-47b0-8b4f-9d34eb0a6288",
    # Connect to azure certificate thumbprint
    [string]$thumbPrint = "c13a03e41fe0220fab4b688951fa986e8441385b",
    # Azure TenantId
    [string]$tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    [string]$vaultName = "TestSuiteKeyVault",
    [string]$resourceGroup = "TestSuiteOnAzure",
    [string]$location = "westus2",
    [string]$certPassword = "123"
)

$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent;

#------------------------------------------------------------------------------------------
# Connect to Az
#------------------------------------------------------------------------------------------
Function ConnectToAz {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$thumbPrint,
        [string]$applicationId,
        [string]$tenantId)

    Process {
        #Import-Module Az -ErrorAction Stop
        WriteToLogFile "Connecting to Az"
        Connect-AzAccount -CertificateThumbprint $thumbPrint -ApplicationId $applicationId -TenantId $tenantId -ErrorAction Stop
        WriteToLogFile "connected"
    }
}


function CreateTaskScheduler($filePath, $taskName, $pwd, $timeInterval) {
    #Create Task Scheduler
    try { 
        $taskAction = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-File $filePath" 
        $taskTrigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Hours $timeInterval) 
        $taskPrincipal = New-ScheduledTaskPrincipal -UserId $vmAccount -RunLevel Highest 
        $taskSettings = New-ScheduledTaskSettingsSet -Compatibility Win8 
        $task = New-ScheduledTask -Action $taskAction -Trigger $taskTrigger -Principal $taskPrincipal -Settings $taskSettings 
        Register-ScheduledTask -TaskName $taskName -InputObject $task -User $vmAccount -Password $pwd -ErrorAction Stop
    }
    catch { 
        WriteToLogFile "Scheduled Task $taskName Already Exists"
    }
}

function UpdateDotNet {
    #Uninstall old SDKs
    choco uninstall dotnet-5.0-sdk -y
    choco uninstall dotnetcore-sdk -y

    #Update Dot Net 
    choco install dotnet-6.0-sdk -y --params="Passive"

    #Update Dot Net 
    choco install dotnet-6.0-aspnetruntime -y --params="Passive"
}

function UpdateWindowsStoreApps {
    #Update Windows Store Apps
    Get-CimInstance -Namespace "Root\cimv2\mdm\dmmap" -ClassName "MDM_EnterpriseModernAppManagement_AppManagement01" | Invoke-CimMethod -MethodName UpdateScanMethod
}

function UpdateWindowsOS {
    #Update Windows OS
    $updateCategory = @("Critical Updates", "Update Rollups", "Updates", "Security Updates", "Service Packs", "Definition Updates")
    Install-PackageProvider -Name NuGet -Force -Confirm:$False
    Install-Module PSWindowsUpdate -Force
    Add-WUServiceManager -MicrosoftUpdate -Confirm:$false
    Install-WindowsUpdate -MicrosoftUpdate -AcceptAll -IgnoreReboot -ForceDownload -ForceInstall -Category $updateCategory
    Get-WUInstall -MicrosoftUpdate -AcceptAll -IgnoreReboot -ForceDownload -ForceInstall -Category $updateCategory
}

function UpdateVisualStudio {
    Start-Process -Wait -FilePath "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vs_installer.exe" -ArgumentList "update --passive --norestart --installpath ""${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise"""
}

function RemoveOldDotNetFolder($version, $netCoreFolder) {

    $versionX = Get-ChildItem $netCoreFolder -Filter $version
    $LatestVersionXFolder = $versionX | Sort-Object -Descending -Property LastWriteTime | Select -First 1

    if ($versionX.Count -gt 1) {
        foreach ($folder in $versionX) {
            if ($LatestVersionXFolder -ne $folder) {
                Remove-Item -Path "$netCoreFolder\$folder" -Recurse -Force -Confirm:$false
                WriteToLogFile "Folder Deleted : $netCoreFolder\$folder"
            }

        }
    }
}

function DisableFeatureUpdate {
    New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate" -Name "DisableOSUpgrade" -Value 1 -PropertyType DWord -Force
    New-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate" -Name "OSUpgrade" -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\OSUpgrade" -Name "AllowOSUpgrade" -Value 0 -PropertyType DWord -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU" -Name "AllowMUUpdateService" -Value 1 -PropertyType DWord -Force
}

#------------------------------------------------------------------------------------------
# Enable TLS 1.2
#------------------------------------------------------------------------------------------
function InstallTLS12 {

    $scriptName = "TLSSettings.ps1"

    if ($osType -ne "Linux") {
        powershell.exe "$scriptPath\$scriptName"
    }
}

#------------------------------------------------------------------------------------------
# Create Software Update start script and run
#------------------------------------------------------------------------------------------
Function ConfigureAutoSoftwareUpdate {
    if ($osType -ne "Linux") {
        $password = $vmPassword
        $folder = -join ((65..90) + (97..122) | Get-Random -Count 10 | ForEach-Object { [char]$_ })
        $softwareUpdate = [System.String]::Concat("$env:HOMEDRIVE", "\", $folder)
        $dotnetFolder = [System.String]::Concat("$env:ProgramFiles", "\dotnet\shared\Microsoft.NETCore.App")


        $gitPath = "$softwareUpdate\git"
        $vsPath = "$softwareUpdate\vs"
        $osPath = "$softwareUpdate\os"


        mkdir -Path $softwareUpdate -Force
        mkdir -Path $gitPath -Force
        mkdir -Path $vsPath -Force
        mkdir -Path $osPath -Force

        $gitUpdatePath = "$gitPath\runGitUpdateAgent.ps1"
        $sdkCoreUpdatePath = "$vsPath\runSDKCoreUpdateAgent.ps1"
        $coreUpdatePath = "$vsPath\runCoreUpdateAgent.ps1"
        $aspCoreUpdatePath = "$vsPath\runASPCoreUpdateAgent.ps1"
        $vsUpdatePath = "$vsPath\runVSUpdateAgent.ps1"
        $winStoreUpdate = "$osPath\windowsStoreUpdate.ps1"
        $winOSUpdate = "$osPath\windowsOSUpdate.ps1"


        #Create GIT update Power Shell file
        $batch = @()
        $batch += 'git update-git-for-windows -y'
        $batch | Out-File $gitUpdatePath

        #Create DOTNET 6 update Power Shell file
        $batch = @()
        $batch += 'choco upgrade dotnet-6.0-sdk -y'
        $batch | Out-File $sdkCoreUpdatePath

        #Create Visual Studio update Power Shell file
        $batch = @()
        $batch += 'Start-Process -Wait -FilePath "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vs_installer.exe" -ArgumentList "update --passive --norestart --installpath ""${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise"""'
        $batch | Out-File $vsUpdatePath

        #Create Windows Store update Power Shell file
        $batch = @()
        $batch += 'Get-CimInstance -Namespace "Root\cimv2\mdm\dmmap" -ClassName "MDM_EnterpriseModernAppManagement_AppManagement01" | Invoke-CimMethod -MethodName UpdateScanMethod'
        $batch | Out-File $winStoreUpdate

        #Create Windows OS update Power Shell file
        $batch = @()
        $batch += '$updateCategory = @("Critical Updates","Update Rollups","Updates","Security Updates","Service Packs","Definition Updates")'
        $batch += 'Install-PackageProvider -Name NuGet -Force -Confirm:$False'
        $batch += 'Install-Module PSWindowsUpdate -Force'
        $batch += 'Add-WUServiceManager -MicrosoftUpdate -Confirm:$false'
        $batch += 'Install-WindowsUpdate -MicrosoftUpdate -AcceptAll -IgnoreReboot -ForceDownload -ForceInstall -Category $updateCategory'
        $batch += 'Get-WUInstall -MicrosoftUpdate -AcceptAll -IgnoreReboot -ForceDownload -ForceInstall -Category $updateCategory'
        $batch | Out-File $winOSUpdate

        #Create Task Scheduler Script Block
        #GIT Scheduled Task
        $path = $gitUpdatePath
        $taskName = "GitUpdateMonitor"
        $interval = 6
        CreateTaskScheduler $path $taskName $password $interval


        #DOTNET SDK Scheduled Task
        $path = $sdkCoreUpdatePath
        $taskName = "SDKCoreUpdateMonitor"
        $interval = 6
        CreateTaskScheduler $path $taskName $password $interval


        #Visual Studio Scheduled Task
        $path = $vsUpdatePath
        $taskName = "VSUpdateMonitor"
        $interval = 6
        CreateTaskScheduler $path $taskName $password $interval

        #Microsoft Store Scheduled Task
        $path = $winStoreUpdate
        $taskName = "MicrosoftStoreUpdateMonitor"
        $interval = 6
        CreateTaskScheduler $path $taskName $password $interval

        #Microsoft Store Scheduled Task
        $path = $winOSUpdate
        $taskName = "MicrosoftOSUpdateMonitor"
        $interval = 6
        CreateTaskScheduler $path $taskName $password $interval

        try { 
            $checkTLS = [Enum]::GetNames([Net.SecurityProtocolType]) -contains 'Tls12'
            Set-ExecutionPolicy Bypass -Scope Process -Force; 
            if ($checkTLS -eq 'True') {
                [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
            }        
            Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

            #UpdateDotNetVersions
            UpdateDotNet
            
            #Check For Existing Multiple Versions
            RemoveOldDotNetFolder "3.*" $dotnetFolder
            RemoveOldDotNetFolder "5.*" $dotnetFolder
        }
        catch { 
            WriteToLogFile "DOT NET Update Failed"
            WriteToLogFile $_.ScriptStackTrace
        }

        try { 
            #Windows Store Apps update
            UpdateWindowsStoreApps
            WriteToLogFile "UpdateWindowsStoreApps complete"
        }
        catch { 
            WriteToLogFile "Windows Store Update Failed"
            WriteToLogFile $_.ScriptStackTrace
        }

        try { 
            #Update Visual Studio
            UpdateVisualStudio
            WriteToLogFile "UpdateVisualStudio complete"
        }
        catch { 
            WriteToLogFile "Visual Studio Update Failed"
            WriteToLogFile $_.ScriptStackTrace
        }

        try { 
            #Update Script For PowerShell 7.0
            if ($PSVersionTable.PSVersion.Major -eq 5) {
                iex "& { $(irm https://aka.ms/install-powershell.ps1) } -UseMSI -Quiet"
            }
            WriteToLogFile "Powershell complete"
        }
        catch { 
            WriteToLogFile "Powershell Update Failed"
            WriteToLogFile $_.ScriptStackTrace
        }

        try { 
            #Update Windows OS
            DisableFeatureUpdate
            UpdateWindowsOS
            WriteToLogFile "UpdateWindowsOS complete"
        }
        catch { 
            WriteToLogFile "Windows OS Update Failed"
            WriteToLogFile $_.ScriptStackTrace
        }
    }
    else {
        WriteToLogFile "update software"
        sudo apt-get install unattended-upgrades
        sudo dpkg-reconfigure --priority=high unattended-upgrades
        WriteToLogFile "update software completed"
    }
}

#------------------------------------------------------------------------------------------
# Create Geneva Monitor start script and run
#------------------------------------------------------------------------------------------
function ConfigureGenevaMonitor {

    if ((Get-Command Get-AzKeyVaultSecret).ParameterSets.Parameters.Name -contains "AsPlainText") {
        # Newer Get-AzKeyVaultSecret version requires -AsPlainText parameter 
        $gcsAccount = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsAccount" -AsPlainText
        $Cert = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsCert" -AsPlainText
        $CertPassword = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsCertPassword" -AsPlainText
        $Namespace = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsNamespace" -AsPlainText
        $Thumbprint = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsThumbprint" -AsPlainText
    }
    else {
        $gcsAccount = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsAccount").SecretValueText
        $Cert = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsCert").SecretValueText
        $CertPassword = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsCertPassword").SecretValueText
        $Namespace = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsNamespace").SecretValueText
        $Thumbprint = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsThumbprint").SecretValueText
    }

    if ($osType -ne "Linux") {
        #enable GenevaMonitoring
        Set-AzVMExtension -ResourceGroupName $resourceGroup -Location $location -VMName $vmName -Name "GenevaMonitoring" -Publisher "Microsoft.Azure.Geneva" -Type "GenevaMonitoring" -TypeHandlerVersion "2.0" -SettingString '{}';
        #Starting Geneva Monitor configuration
        $azSecPackPath = "C:\AzSecPack"
        $monitoringPath = "$azSecPackPath\Monitoring"
        $dataPath = "$monitoringPath\Data"

        mkdir -Path $azSecPackPath -Force
        mkdir -Path $monitoringPath -Force
        mkdir -Path $dataPath -Force

        $certPath = "$azSecPackPath\AzSecPack.pfx"
        $certBytes = [System.Convert]::FromBase64String($Cert)
        [System.IO.File]::WriteAllBytes($certPath, $certBytes)
        Import-PfxCertificate -FilePath $certPath -Password (ConvertTo-SecureString -String $CertPassword -AsPlainText -Force) -CertStoreLocation Cert:\LocalMachine\My

        $batch = @()
        $batch += "set MONITORING_DATA_DIRECTORY=$dataPath"
        $batch += "set MONITORING_TENANT=%USERNAME%"
        $batch += "set MONITORING_ROLE=$Namespace"
        $batch += "set MONITORING_ROLE_INSTANCE=%COMPUTERNAME%"
        $batch += "set MONITORING_GCS_ENVIRONMENT=Diagnostics PROD"
        $batch += "set MONITORING_GCS_ACCOUNT=$gcsAccount"
        $batch += "set MONITORING_GCS_NAMESPACE=$Namespace"
        $batch += "set MONITORING_GCS_REGION=$gcsRegion"
        $batch += "set MONITORING_GCS_THUMBPRINT=$Thumbprint"
        $batch += "set MONITORING_GCS_CERTSTORE=LOCAL_MACHINE\My"
        $batch += "set MONITORING_CONFIG_VERSION=1.9"
        if ([System.Environment]::OSVersion.Version.Build -ge 17763) {
            $batch += "set AZSECPACK_PILOT_FEATURES=MdeServer2019Support"
        }
        else {
            $batch += "set AZSECPACK_PILOT_FEATURES=WDATP"
        }
        $batch += "%MonAgentClientLocation%/MonAgentClient.exe -useenv"

        [System.IO.File]::WriteAllLines("$monitoringPath\runAgent.cmd", $batch)

        try {
            $taskAction = New-ScheduledTaskAction -Execute "$monitoringPath\runAgent.cmd"
            $taskTrigger = New-ScheduledTaskTrigger -AtStartup
            $taskPrincipal = New-ScheduledTaskPrincipal "SYSTEM"
            $taskSettings = New-ScheduledTaskSettingsSet
            $task = New-ScheduledTask -Action $taskAction -Trigger $taskTrigger -Principal $taskPrincipal -Settings $taskSettings 
            Register-ScheduledTask "GenevaMonitoringStartup" -InputObject $task -ErrorAction Stop
        }
        catch { 
            WriteToLogFile "Scheduled Task $taskName Already Exists"
        }
        Start-Sleep -Seconds 5

        Start-ScheduledTask -TaskName "GenevaMonitoringStartup"
    }
    else {
        WriteToLogFile "linux InstallGenevaMonitorToLinux"
        pwsh -file "$scriptPath/InstallGenevaMonitorToLinux.ps1" -vaultName $vaultName -gcsRegion $gcsRegion -isRemote:False
        WriteToLogFile "linux InstallGenevaMonitorToLinux completed"
    }
}

function WriteToLogFile ($message) {
    $message + " - " + (Get-Date).ToString() >> $logfilepath
}

Function Main {

    # The Geneva Account Region
    [string]$gcsRegion = "westus2"

    if ($osType -ne "Linux") {
        $logfilepath = "$env:HOMEDRIVE\s360.txt"
        Import-PfxCertificate -FilePath "$scriptPath\AzureAccountCert.pfx" -Password (ConvertTo-SecureString -String $certPassword -AsPlainText -Force) -CertStoreLocation Cert:\LocalMachine\My
    }
    else {
        $logfilepath = "/var/s360.txt"
        $StoreName = [System.Security.Cryptography.X509Certificates.StoreName]::My 
        $StoreLocation = [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser
        $Store = [System.Security.Cryptography.X509Certificates.X509Store]::new($StoreName, $StoreLocation) 
        $Flag = [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable 
        #access "$scriptPath/AzureAccountCert.pfx"
        sudo chmod 755 "$scriptPath/AzureAccountCert.pfx"
        $Certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new("$scriptPath/AzureAccountCert.pfx", $certPassword, $Flag) 
        $Store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite) 
        $Store.Add($Certificate) 
        $Store.Close() 
    }
    if (Test-Path $logfilepath) {
        Remove-Item $logfilepath -Force
    }

    ConfigureAutoSoftwareUpdate

    Install-Module -Name Az -AllowClobber -force
    ConnectToAz -thumbPrint $thumbPrint -applicationId $applicationId -tenantId $tenantId
    
    WriteToLogFile "ConfigureGenevaMonitor"
    ConfigureGenevaMonitor

    WriteToLogFile "InstallTLS12"
    InstallTLS12

    WriteToLogFile "Finish"
}

Main

Pop-Location