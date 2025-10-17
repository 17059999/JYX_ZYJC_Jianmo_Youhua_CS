###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           InstallGenevaMonitorToLinuxStandalone.ps1
## Purpose:        Install Geneva Monitoring Tools To A Linux Machine.
## Version:        1.0 (21 April, 2021)
##
##############################################################################
Param(
    # The remote Linux machine IP Address
    [Parameter(Mandatory = $True)]
    [string]$vmIpAddress,
    # The remote Linux machine Name
    [Parameter(Mandatory = $True)]
    [string]$vmName,
    # The username on the remote Linux Machine
    [string] $username = "IOLab",
    # The Geneva Account Region
    [string]$gcsRegion = "westus2",
    # Azure Key vault
    [string]$vaultName = "TestSuiteKeyVault"
)

#------------------------------------------------------------------------------------------
# Write Output
#------------------------------------------------------------------------------------------
Function Write-Output {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message)
    Write-Host ((Get-Date).ToString() + ": $Message")
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

    $gcsCertPem = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsCertPem").SecretValueText
    $gcsKeyPem = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsKeyPem").SecretValueText
    
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

    $gcsAccount = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsAccount").SecretValueText
    $gcsNamespaceLinux = (Get-AzKeyVaultSecret -VaultName $vaultName -Name "GcsNamespaceLinux").SecretValueText

    $batch = @()
    $batch += "MDSD_ROLE_PREFIX=/var/run/mdsd/default"
    $batch += "MDSD_OPTIONS=`"-d -A -r `${MDSD_ROLE_PREFIX}`""
    $batch += "MDSD_LOG=/var/log"
    $batch += "MDSD_SPOOL_DIRECTORY=/var/opt/microsoft/linuxmonagent"
    $batch += "MDSD_OPTIONS=`"-A -c /etc/mdsd.d/mdsd.xml -d -r `$MDSD_ROLE_PREFIX -S `$MDSD_SPOOL_DIRECTORY/eh -e `$MDSD_LOG/mdsd.err -w `$MDSD_LOG/mdsd.warn -o `$MDSD_LOG/mdsd.info`""
    $batch += "export MDSD_TCMALLOC_RELEASE_FREQ_SEC=1"
    $batch += "export SSL_CERT_DIR=/etc/ssl/certs"
    $batch += "export MONITORING_TENANT=$($vmName)"
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
        [string]$remoteAzSecPackPath
    )

    $remoteScript = @()
    $remoteScript += "bash -c `"echo 'deb [arch=amd64] http://packages.microsoft.com/repos/azurecore/ bionic main' | sudo tee -a /etc/apt/sources.list.d/azure.list`""
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

    Write-Output "ssh $fullUserName mkdir $remoteAzSecPackPath"
    ssh $fullUserName mkdir $remoteAzSecPackPath

    Write-Output "cmd /c scp -r $($azSecPackLinuxPath)\* $($fullUserName):$remoteAzSecPackPath"
    cmd /c scp -r "$($azSecPackLinuxPath)\*" "$($fullUserName):$remoteAzSecPackPath"

    Write-Output "ssh $fullUserName pwsh $remoteAzSecPackPath/$remoteScriptFileName"
    ssh $fullUserName pwsh $remoteAzSecPackPath/$remoteScriptFileName
}

Function Main {
    $fullUserName = $username + "@"+ $vmIpAddress
        
    $azSecPackLinuxPath = "$env:HOMEDRIVE\AzSecPackLinux"
    mkdir -Path $azSecPackLinuxPath -Force
                
    $remoteAzSecPackPath = "/home/$username/azsecpack"

    $certPemFileName = "gcscert.pem"
    $certPemPath = "$azSecPackLinuxPath\$certPemFileName"

    $keyPemFileName = "gcskey.pem"
    $keyPemPath = "$azSecPackLinuxPath\$keyPemFileName"

    $mdsdFileName = "DefaultMdsd"
    $mdsdPath = "$azSecPackLinuxPath\$mdsdFileName"
    
    $remoteScriptFileName = "InstallGeneva.ps1"
    $remoteScriptPath = "$azSecPackLinuxPath\$remoteScriptFileName"

    Write-Output ""
    Write-Output "============================================================"
    Write-Output "            Connect to Azure                                "
    Write-Output "============================================================"
    Connect-AzAccount

    Write-Output ""
    Write-Output "============================================================"
    Write-Output "            Retrieve .pem files                             "
    Write-Output "============================================================"
    Get-PemFiles -certPemPath $certPemPath -keyPemPath $keyPemPath

    Write-Output ""
    Write-Output "============================================================"
    Write-Output "            Generate default MDSD file                      "
    Write-Output "============================================================"
    New-DefaultMdsdFile -mdsdPath $mdsdPath -certPemFileName $certPemFileName -keyPemFileName $keyPemFileName

    Write-Output ""
    Write-Output "============================================================"
    Write-Output "            Generate Remote Script                          "
    Write-Output "============================================================"
    New-RemoteScript -remoteScriptPath $remoteScriptPath -certPemFileName $certPemFileName -keyPemFileName $keyPemFileName -mdsdFileName $mdsdFileName -remoteAzSecPackPath $remoteAzSecPackPath

    Write-Output ""
    Write-Output "============================================================"
    Write-Output "            Copy and Install Remote Script                  "
    Write-Output "============================================================"
    Install-RemoteScript -remoteAzSecPackPath $remoteAzSecPackPath -fullUserName $fullUserName -azSecPackLinuxPath $azSecPackLinuxPath -remoteScriptFileName $remoteScriptFileName
}

Main