# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param($workingDir = "$env:SystemDrive\Temp", $protocolConfigFile = "$workingDir\Protocol.xml")

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath;$scriptPath\Scripts"

#----------------------------------------------------------------------------
# if working dir is not exists. it will use scripts path as working path
#----------------------------------------------------------------------------
if(!(Test-Path "$workingDir"))
{
    $workingDir = $scriptPath
}

if(!(Test-Path "$protocolConfigFile"))
{
    $protocolConfigFile = "$workingDir\Protocol.xml"
    if(!(Test-Path "$protocolConfigFile"))
    {
        Write-Error.ps1 "No protocol.xml found."
        exit ExitCode
    }
}

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
[string]$logFile = $MyInvocation.MyCommand.Path + ".log"
Start-Transcript -Path "$logFile" -Append -Force

#----------------------------------------------------------------------------
# Define common functions
#----------------------------------------------------------------------------
function ExitCode()
{
    return $MyInvocation.ScriptLineNumber
}

#----------------------------------------------------------------------------
# Get content from protocol config file
#----------------------------------------------------------------------------
[xml]$config = Get-Content "$protocolConfigFile"
if($config -eq $null)
{
    Write-Error.ps1 "protocolConfigFile $protocolConfigFile is not a valid XML file."
    exit ExitCode
}

#----------------------------------------------------------------------------
# Define common variables
#----------------------------------------------------------------------------

$systemDrive = $ENV:SystemDrive
$OSVersion = [System.Environment]::OSVersion.Version

$sut = $config.lab.servers.vm | Where-Object { $_.role -match "NODE01" }
$sutIP = $sut.ip
$sutName = $sut.name
$domain = $sut.domain
$sutComputerName = $sutName
if ((-not [string]::IsNullOrEmpty($domain)) -and ($domain.ToLower() -ne "workgroup")) {
    $sutComputerName = "$sutName.$domain".ToLower()
}

if ($driver.os -eq "Linux") {
    Write-Error.ps1 "QUIC is not supported on Linux"
    exit ExitCode
}

$osName = (Get-WMIObject Win32_OperatingSystem).Name
Write-Info.ps1 "OS Name: $osName"

#----------------------------------------------------------------------------
# Create SMB certificate mapping
#----------------------------------------------------------------------------
if ($osName -match "Azure Edition") {
    Write-Info.ps1 "Create SelfSigned Certificate: $sutName"
    $currCert = New-SelfSignedCertificate -Subject $sutName -FriendlyName "SMB over QUIC for File Servers" -KeyUsageProperty Sign -KeyUsage DigitalSignature -CertStoreLocation Cert:\LocalMachine\My -HashAlgorithm SHA256 -Provider "Microsoft Software Key Storage Provider" -KeyAlgorithm ECDSA_P256 -KeyLength 256 -DnsName @($sutComputerName, $sutName)

    $certThumbprint = $currCert.Thumbprint
    $subject = $currCert.Subject
    Write-Info.ps1 "Mapping SmbServer:$sutComputerName with Certificate: $certThumbprint Subject:$subject"
    New-SmbServerCertificateMapping -Name $sutComputerName -Thumbprint $certThumbprint -StoreName my -Subject $subject

    Write-Info.ps1 "Import the certificate to root"
    $pfxPwd = ConvertTo-SecureString -String "Password01!" -Force -AsPlainText
    Export-PfxCertificate -Cert $currCert -FilePath "QUICCert.pfx" -Password $pfxPwd
    Import-PfxCertificate -FilePath "QUICCert.pfx" -CertStoreLocation Cert:\LocalMachine\Root -Password $pfxPwd

    Write-Info.ps1 "Enable SMB encryption on QUIC connection."
    Set-SmbServerConfiguration -DisableSmbEncryptionOnSecureConnection $false -Confirm:$false

    Write-Info.ps1 "Enable NamedPipe access on QUIC connection."
    Set-SmbServerConfiguration -RestrictNamedpipeAccessViaQuic $false -Confirm:$false
}
else {
    Write-Info.ps1 "QUIC is only supported on Windows Server 2022 Azure Edition and later versions."
}


#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
Write-Info.ps1 "Completed setup QUIC ENV."
Stop-Transcript
exit 0