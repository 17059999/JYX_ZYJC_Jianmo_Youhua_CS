###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           InstallGenevaMonitorToLinux.ps1
## Purpose:        Install Geneva Monitoring Tools To A Linux Machine.
## Version:        1.0 (21 April, 2021)
##
##############################################################################
Param(
    # The remote Linux machine IP Address
    [string]$vmIpAddress,
    # The username on the remote Linux Machine
    [string] $username = "IOLab",
    # The Geneva Account Region
    [string]$gcsRegion = "westus2",
    # Azure Key vault
    [string]$vaultName = "TestSuiteKeyVault",
    [bool]$isRemote = $True
)

#------------------------------------------------------------------------------------------
# Write Output
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
# Retrieve .pem files    
#------------------------------------------------------------------------------------------
Function Get-PemFiles {
    Param
    (
        [string]$certPemPath, 
        [string]$keyPemPath
    )
    if ($isRemote) {
        $gcsCertPem = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsCertPem").SecretValueText
        $gcsKeyPem = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsKeyPem").SecretValueText
    }
    else {
        if ((Get-Command Get-AzKeyVaultSecret).ParameterSets.Parameters.Name -contains "AsPlainText") {
            # Newer Get-AzKeyVaultSecret version requires -AsPlainText parameter 
            $gcsCertPem = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsCertPem" -AsPlainText
            $gcsKeyPem = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsKeyPem" -AsPlainText
        }
        else {
            $gcsCertPem = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsCertPem").SecretValueText
            $gcsKeyPem = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsKeyPem").SecretValueText
        }
    }

    $certPemBytes = [System.Convert]::FromBase64String($gcsCertPem)
    [System.IO.File]::WriteAllBytes($certPemPath, $certPemBytes)
    
    $keyPemBytes = [System.Convert]::FromBase64String($gcsKeyPem)
    [System.IO.File]::WriteAllBytes($keyPemPath, $keyPemBytes)
}

#------------------------------------------------------------------------------------------
# Retrieve and Copy Default MDSD file    
#------------------------------------------------------------------------------------------
Function New-DefaultMdsdFile {
    Param
    (
        [string]$mdsdPath, 
        [string]$certPemFileName, 
        [string]$keyPemFileName
    )
    if ($isRemote) {
        $gcsAccount = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsAccount").SecretValueText
        $gcsNamespaceLinux = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsNamespaceLinux").SecretValueText
    }
    else {
        if ((Get-Command Get-AzKeyVaultSecret).ParameterSets.Parameters.Name -contains "AsPlainText") {
            # Newer Get-AzKeyVaultSecret version requires -AsPlainText parameter 
            $gcsAccount = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsAccount" -AsPlainText
            $gcsNamespaceLinux = Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsNamespaceLinux" -AsPlainText
        }
        else {
            $gcsAccount = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsAccount").SecretValueText
            $gcsNamespaceLinux = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsNamespaceLinux").SecretValueText
        }
    }
    

    $batch = @()
    $batch += "MDSD_ROLE_PREFIX=/var/run/mdsd/default"
    $batch += "MDSD_OPTIONS=`"-d -A -r `${MDSD_ROLE_PREFIX}`""
    $batch += "MDSD_LOG=/var/log"
    $batch += "MDSD_SPOOL_DIRECTORY=/var/opt/microsoft/linuxmonagent"
    $batch += "MDSD_OPTIONS=`"-A -c /etc/mdsd.d/mdsd.xml -d -r `$MDSD_ROLE_PREFIX -S `$MDSD_SPOOL_DIRECTORY/eh -e `$MDSD_LOG/mdsd.err -w `$MDSD_LOG/mdsd.warn -o `$MDSD_LOG/mdsd.info`""
    $batch += "export MDSD_TCMALLOC_RELEASE_FREQ_SEC=1"
    $batch += "export SSL_CERT_DIR=/etc/ssl/certs"
    if ($isRemote) {
        $batch += "export MONITORING_TENANT=$($vm.name)"
    }
    else {
        $batch += "export MONITORING_TENANT=$vmname"
    }
    $batch += "export MONITORING_ROLE=$gcsNamespaceLinux"
    $batch += "export MONITORING_ROLE_INSTANCE=$username"
    $batch += "export MONITORING_GCS_ENVIRONMENT=DiagnosticsPROD"
    $batch += "export MONITORING_GCS_ACCOUNT=$gcsAccount"
    $batch += "export MONITORING_GCS_REGION=$gcsRegion"
    $batch += "export MONITORING_GCS_CERT_CERTFILE=/etc/mdsd.d/$certPemFileName"
    $batch += "export MONITORING_GCS_CERT_KEYFILE=/etc/mdsd.d/$keyPemFileName"
    $batch += "export MONITORING_GCS_NAMESPACE=$gcsNamespaceLinux"
    $batch += "export MONITORING_CONFIG_VERSION=1.1"
    $batch += "export MONITORING_USE_GENEVA_CONFIG_SERVICE=true"

    $batch | Out-File $mdsdPath
}

#------------------------------------------------------------------------------------------
# Create remote script
#------------------------------------------------------------------------------------------
Function New-RemoteScript {
    Param
    (
        [string]$remoteScriptPath, 
        [string]$certPemFileName, 
        [string]$keyPemFileName,
        [string]$mdsdFileName, 
        [string]$remoteAzSecPackPath,
        [string]$fullUserName
    )
    
    $ubuntuCodeName = if ($isRemote) { ssh $fullUserName "lsb_release -cs" } else { lsb_release -cs }
    Write-TestSuiteInfo "Processing New-RemoteScript on Ubuntu Code Name == $ubuntuCodeName"

    $remoteScript = @()
    $remoteScript += "bash -c `"echo 'deb [arch=amd64] http://packages.microsoft.com/repos/azurecore/ $ubuntuCodeName main' | sudo tee -a /etc/apt/sources.list.d/azure.list`""
    $remoteScript += "bash -c `"wget -qO - https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -`""
    $remoteScript += "bash -c `"wget -qO - https://packages.microsoft.com/keys/msopentech.asc | sudo apt-key add -`""
    
    $remoteScript += "bash -c `"sudo apt-get update`""
    $remoteScript += "bash -c `"sudo apt-get install -y azure-mdsd`""
    $remoteScript += "bash -c `"sudo apt-get install -y azure-security azsec-monitor azsec-clamav`""
    $remoteScript += "bash -c `"sudo apt-get install dos2unix`""
    
    $remoteScript += "bash -c `"sudo cp $remoteAzSecPackPath/$certPemFileName /etc/mdsd.d/`""
    $remoteScript += "bash -c `"sudo cp $remoteAzSecPackPath/$keyPemFileName /etc/mdsd.d/`""
    
    $remoteScript += "bash -c `"sudo chown syslog:syslog /etc/mdsd.d/$keyPemFileName`""
    $remoteScript += "bash -c `"sudo chmod 400 /etc/mdsd.d/$keyPemFileName`""
    $remoteScript += "bash -c `"sudo chown syslog:syslog /etc/mdsd.d/$certPemFileName`""
    $remoteScript += "bash -c `"sudo chmod 400 /etc/mdsd.d/$certPemFileName`""
    
    $remoteScript += "bash -c `"dos2unix $remoteAzSecPackPath/$mdsdFileName`""
    $remoteScript += "bash -c `"sudo cp -f $remoteAzSecPackPath/$mdsdFileName /etc/default/mdsd`""
    $remoteScript += "bash -c `"sudo azsecd config -s baseline -d P1D`""
    $remoteScript += "bash -c `"sudo azsecd config -s software -d P1D`""
    $remoteScript += "bash -c `"sudo azsecd config -s clamav -d P1D`""
    
    $remoteScript += "bash -c `"sudo service mdsd restart`""
    $remoteScript += "bash -c `"sudo service azsecd restart`""
    
    $remoteScript += "bash -c `"logger -p local1.info 'New Onboarded event'`""
    $remoteScript += "bash -c `"rm -rf $remoteAzSecPackPath`""
    
    $remoteScript | Out-File $remoteScriptPath
}

#------------------------------------------------------------------------------------------
# Copy all files to VM and execute remote script    
#------------------------------------------------------------------------------------------
Function Install-RemoteScript {    
    Param
    (
        [string]$remoteAzSecPackPath, 
        [string]$fullUserName, 
        [string]$azSecPackLinuxPath,
        [string]$remoteScriptFileName
    )

    if ($isRemote) {
        Write-TestSuiteInfo "ssh $fullUserName mkdir $remoteAzSecPackPath"
        ssh $fullUserName mkdir $remoteAzSecPackPath
        Write-TestSuiteInfo "cmd /c scp -r $($azSecPackLinuxPath)\* $($fullUserName):$remoteAzSecPackPath"
        cmd /c scp -r "$($azSecPackLinuxPath)\*" "$($fullUserName):$remoteAzSecPackPath"
        Write-TestSuiteInfo "ssh $fullUserName pwsh $remoteAzSecPackPath/$remoteScriptFileName"
        ssh $fullUserName pwsh $remoteAzSecPackPath/$remoteScriptFileName
    }
    else {
        Write-TestSuiteInfo "mkdir $remoteAzSecPackPath"
        mkdir -p $remoteAzSecPackPath
        Write-TestSuiteInfo "cp -r $($azSecPackLinuxPath)/* $($remoteAzSecPackPath)"
        cp -r $azSecPackLinuxPath/* $remoteAzSecPackPath
        Write-TestSuiteInfo "pwsh $remoteAzSecPackPath/$remoteScriptFileName"
        pwsh $remoteAzSecPackPath/$remoteScriptFileName
    }
}

Function Main {
    $fullUserName = $username + "@" + $vmIpAddress

    $certPemFileName = "gcscert.pem"
    $keyPemFileName = "gcskey.pem"
    $mdsdFileName = "DefaultMdsd"
    $remoteScriptFileName = "InstallGeneva.ps1"
    if ($isRemote) {
        $azSecPackLinuxPath = "$env:HOMEDRIVE\AzSecPackLinux"
        $certPemPath = "$azSecPackLinuxPath\$certPemFileName"
        $keyPemPath = "$azSecPackLinuxPath\$keyPemFileName"
        $mdsdPath = "$azSecPackLinuxPath\$mdsdFileName"
        $remoteScriptPath = "$azSecPackLinuxPath\$remoteScriptFileName"
        mkdir -P $azSecPackLinuxPath -Force
    }
    else {
        Install-Module -Name Az -AllowClobber -force

        $username = [System.Environment]::UserName
        $vmname = [System.Environment]::MachineName
        $azSecPackLinuxPath = "/var/AzSecPackLinux"
        $certPemPath = "$azSecPackLinuxPath/$certPemFileName"
        $keyPemPath = "$azSecPackLinuxPath/$keyPemFileName"
        $mdsdPath = "$azSecPackLinuxPath/$mdsdFileName"
        $remoteScriptPath = "$azSecPackLinuxPath/$remoteScriptFileName"
        mkdir -p $azSecPackLinuxPath
    }
    
    Write-TestSuiteInfo "$azSecPackLinuxPath"    
    $remoteAzSecPackPath = "/home/$username/azsecpack"
    Write-TestSuiteInfo "$remoteAzSecPackPath"
    
    

    Write-TestSuiteInfo ""
    Write-TestSuiteInfo "============================================================"
    Write-TestSuiteInfo "            Retrieve .pem files                             "
    Write-TestSuiteInfo "============================================================"
    Get-PemFiles -certPemPath $certPemPath -keyPemPath $keyPemPath
    
    if ($isRemote) {
        $osName = ssh $fullUserName "cat /etc/*-release | grep 'NAME='"
    }
    else {
        $osName = cat /etc/*-release | grep 'NAME='
    }
    if ($osName -match "Ubuntu") {
        Write-TestSuiteInfo ""
        Write-TestSuiteInfo "============================================================"
        Write-TestSuiteInfo "            Generate default MDSD file                      "
        Write-TestSuiteInfo "============================================================"
        New-DefaultMdsdFile -mdsdPath $mdsdPath -certPemFileName $certPemFileName -keyPemFileName $keyPemFileName

        Write-TestSuiteInfo ""
        Write-TestSuiteInfo "============================================================"
        Write-TestSuiteInfo "            Generate Remote Script                          "
        Write-TestSuiteInfo "============================================================"
        New-RemoteScript -remoteScriptPath $remoteScriptPath -certPemFileName $certPemFileName -keyPemFileName $keyPemFileName -mdsdFileName $mdsdFileName -remoteAzSecPackPath $remoteAzSecPackPath -fullUserName $fullUserName

        Write-TestSuiteInfo ""
        Write-TestSuiteInfo "============================================================"
        Write-TestSuiteInfo "            Copy and Install Remote Script                  "
        Write-TestSuiteInfo "============================================================"
        Install-RemoteScript -remoteAzSecPackPath $remoteAzSecPackPath -fullUserName $fullUserName -azSecPackLinuxPath $azSecPackLinuxPath -remoteScriptFileName $remoteScriptFileName
    }
    else {
        Write-TestSuiteInfo "============================================================"
        Write-TestSuiteInfo "            Not Ubuntu OS will not install AzSecPack        "
        Write-TestSuiteInfo "============================================================"
    }
}

Main