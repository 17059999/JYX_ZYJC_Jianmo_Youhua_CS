#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Execute-TestCaseByContext.ps1
# Purpose:        Execute the Test Case in Windows Driver.
# Requirements:   Powershell core
# Supported OS:   Windows
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $ContextName:         The running test cases filter context name
# $CategoryName:        The running test cases filter category name
# $TestName:            The running test cases filter test name
#----------------------------------------------------------------------------

param($ContextName = "", $CategoryName = "", $TestName = "", $runTests = "true")
# For FileSharing, the context name should be any of below
# Samba_Workgroup_SMB2002
# Samba_Workgroup_SMB21
# Samba_Workgroup_SMB30

# Win2012R2_Domain_Cluster_SMB302
# Win2012R2_Domain_Cluster_SMB302_EnableLDAP
# Win2012R2_Domain_NonCluster_SMB302
# Win2012R2_Domain_NonCluster_SMB302_EnableLDAP
# Win2016_Domain_Cluster_SMB311
# Win2016_Domain_Cluster_SMB311_ForPG
# Win2016_Domain_Cluster_SMB311_BVTForPG
# Win2016_Domain_NonCluster_SMB311
# Win2016_Domain_Cluster_SMB311_RSVD
# Win2019_Domain_Cluster_SMB311
# Win2019_Domain_NonCluster_SMB311
# WinV1903_Domain_Cluster_SMB311
# WinV1903_Domain_NonCluster_SMB311
# Win2022_Domain_Cluster_SMB311_EnableQUIC
# Win2022_Domain_NonCluster_SMB311_EnableQUIC
# WinV22H2_Domain_Cluster_SMB311
# WinV22H2_Domain_NonCluster_SMB311
# Win2025_Domain_Cluster_SMB311_EnableQUIC
# Win2025_Domain_NonCluster_SMB311_EnableQUIC
# Win2025_Domain_Cluster_SMB311
# Win2025_Domain_NonCluster_SMB311
# Win2025_Workgroup_NonCluster_SMB311

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath"
$TestArray = @();

#----------------------------------------------------------------------------
# Common function
#----------------------------------------------------------------------------
function ExitCode() { 
  return $MyInvocation.ScriptLineNumber 
}

function ReadPtfConfigProperty($ptfConfigFile, $propertyName) {
  [xml]$configContent = Get-Content $ptfConfigFile
  $propertyNodes = $configContent.GetElementsByTagName("Property")
  foreach ($node in $propertyNodes) {
    if ($node.GetAttribute("name") -eq $propertyName) {
      return $node.GetAttribute("value")
    }
  }
}

#----------------------------------------------------------------------------
# Get content from protocol config file
#----------------------------------------------------------------------------
$protocolConfigFile = "$env:SystemDrive/Temp/Protocol.xml"
if (-not (Test-Path -Path $protocolConfigFile)) {
  $protocolConfigFile = "~/Temp/Protocol.xml"
}

[xml]$config = Get-Content "$protocolConfigFile"
if ($config -eq $null) {
  ."$PSScriptRoot/Write-Error.ps1" "protocolConfigFile $protocolConfigFile is not a valid XML file."
  Write-ConfigFailureSignal
  exit ExitCode
}

$sut = $config.lab.servers.vm | Where-Object { $_.role -match "NODE01" }
$sutIP = $sut.ip
$sutAlternativeIPAddress = $sut.ip
if (($sut.ip | Measure-Object).Count -gt 1) {
  $sutIP = $sut.ip[0]
  $sutAlternativeIPAddress = $sut.ip[1]
}
$adminUserName = $config.lab.core.username
$adminPassword = $config.lab.core.password
$sutName = $sut.name
$domain = $sut.domain

$driver = $config.lab.servers.vm | Where-Object { $_.role -eq "DriverComputer" }
$driverIP1 = $driver.ip[0]
if (($driver.ip | Measure-Object).Count -gt 1) {
  $driverIP2 = $driver.ip[1]
}
$endPointPath = $driver.tools.TestsuiteZip.targetFolder
$isLocalLinuxDriver = ($driver.os -eq "Linux" -or $driver.os -eq "RPMBasedLinux") -and ($protocolConfigFile -eq "$env:SystemDrive/Temp/Protocol.xml")

#----------------------------------------------------------------------------
# Define common variables
#----------------------------------------------------------------------------
$logPath = "$env:SystemDrive\Test\TestLog"
$dataPath = "$env:SystemDrive\Temp\Data\$ContextName"
$testDir = "$env:SystemDrive\Test"

if ($driver.os -eq "Linux" -or $driver.os -eq "RPMBasedLinux") {
  $testCaseRootPath = if ($isLocalLinuxDriver) {
    ""
  }
  else {
    "~"
  }

  $logPath = "$testCaseRootPath/Test/TestLog"    
  $dataPath = "$testCaseRootPath/Temp/Data/$ContextName"
  $testDir = "$testCaseRootPath/Test"
}

if (-not (Test-Path -Path $logPath)) {
  mkdir $testDir
  mkdir $logPath
}

if ($isLocalLinuxDriver) {
  & /usr/bin/env sudo pwsh -command "& dos2unix $PSScriptRoot/CopyWindowsFileToLinux.sh"
  & /usr/bin/env sudo pwsh -command "& chmod 777 $PSScriptRoot/CopyWindowsFileToLinux.sh"
}

#----------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#----------------------------------------------------------------------------
Start-Transcript -Path "$logPath\Execute-TestCaseByContext.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Check ContextName
#----------------------------------------------------------------------------
if ([System.String]::IsNullOrWhiteSpace($ContextName)) {       
  ."$PSScriptRoot/Write-Info.ps1" "ContextName cannot be null or empty."  -ForegroundColor Red
  exit ExitCode
}

#----------------------------------------------------------------------------
# Prepare for execute test suite
#----------------------------------------------------------------------------

# Create test dir
if (-not (Test-Path $testDir)) {
  mkdir $testDir
}

# Create test result directory
$testResultDir = $testDir + "/TestResults"
if (Test-Path $testResultDir) {
  Get-ChildItem -Path "$testResultDir/*" -Recurse | Remove-Item -Force -Recurse 
}
else {
  mkdir $testResultDir
}

Set-Location $testDir

# Clean up test.finished.signal
$finishSignalFile = "$testDir/test.finished.signal"
if (Test-Path $finishSignalFile) {
  Remove-Item $finishSignalFile
}

# Write test.started.signal
$startSignalFile = "$testDir/test.started.signal"
Write-Output "test.started.signal" > $startSignalFile

# Get test suite path
$binDir = "$endPointPath/Bin"

# Copy ptf files
if (Test-Path -Path $dataPath) {
  Copy-Item -Path "$dataPath/*" -Destination $binDir -Force -Confirm:$false
}

# Clear read only for PTF config files
$commonTestSuitePtfConfig = "$binDir/CommonTestSuite.deployment.ptfconfig"
$smb2TestSuitePtfConfig = "$binDir/MS-SMB2_ServerTestSuite.deployment.ptfconfig"
$smb2ModelPtfConfig = "$binDir/MS-SMB2Model_ServerTestSuite.deployment.ptfconfig"
$dfscPtfConfig = "$binDir/MS-DFSC_ServerTestSuite.deployment.ptfconfig"
$serverFailoverPtfConfig = "$binDir/ServerFailoverTestSuite.deployment.ptfconfig"
$fsaPtfConfig = "$binDir/MS-FSA_ServerTestSuite.deployment.ptfconfig"
$sqosPtfConfig = "$binDir/MS-SQOS_ServerTestSuite.deployment.ptfconfig"
$rsvdPtfConfig = "$binDir/MS-RSVD_ServerTestSuite.deployment.ptfconfig"
$authPtfConfig = "$binDir/Auth_ServerTestSuite.deployment.ptfconfig"

if ($driver.os -ne "Linux" -and $driver.os -ne "RPMBasedLinux") {
  Push-Location $binDir
  cmd /c attrib -r $commonTestSuitePtfConfig
  cmd /c attrib -r $smb2TestSuitePtfConfig
  cmd /c attrib -r $smb2ModelPtfConfig
  cmd /c attrib -r $dfscPtfConfig
  cmd /c attrib -r $serverFailoverPtfConfig
  cmd /c attrib -r $fsaPtfConfig
  cmd /c attrib -r $sqosPtfConfig
  cmd /c attrib -r $rsvdPtfConfig
  cmd /c attrib -r $authPtfConfig
}
# Cluster configuration
$sut2 = $config.lab.servers.vm | Where-Object { $_.role -match "NODE02" }
if ($config.lab.ha -ne $null) {
  $scaleOutFsName = $config.lab.ha.scaleoutfs.name
  $clusterName = $config.lab.ha.cluster.name
  $generalFsName = $config.lab.ha.generalfs.name
}

$isAzureCluster = ($config.lab.core.regressiontype -match "Azure") -and ($config.lab.ha.cluster -ne $null)

$sutComputerName = "$sutName.$domain"
if ($driver.os -eq "Linux" -or $driver.os -eq "RPMBasedLinux") {
  $sutComputerName = $sutIP
}

Write-Host "commonTestSuitePtfConfig: $commonTestSuitePtfConfig"
Write-Host "sutIP: $sutIP"
Write-Host "sutName: $sutName"
Write-Host "domain: $domain"
Write-Host "sutAlternativeIPAddress: $sutAlternativeIPAddress"
Write-Host "driverIP1: $driverIP1"
Write-Host "driverIP2: $driverIP2"
Write-Host "ContextName: $ContextName"
Write-Host "isAzureCluster: $isAzureCluster"
Write-Host "isLocalLinuxDriver: $isLocalLinuxDriver"

#----------------------------------------------------------------------------
# Update CommonTestSuite.ptfconfig
#----------------------------------------------------------------------------
if (($ContextName -match "EnableLDAP")) {
  $commonTestSuitePtfConfigForAdapters = "$binDir/CommonTestSuite.ptfconfig"
  # Update ISutCommonControlAdapter Adapters for 12R2
  ."$PSScriptRoot/Write-Info.ps1" "Updating CommonTestSuite.ptfconfig to use managed Adapter for ISutCommonControlAdapter on 12R2 platform."
  $ptfConfigXml = [xml](Get-Content $commonTestSuitePtfConfigForAdapters)
  $updateTag = $ptfConfigXml.TestSite.Adapters.ChildNodes | Where-Object { $_.name -eq 'ISutCommonControlAdapter' }
  $updateTag.ParentNode.RemoveChild($updateTag)
  $ptfConfigXml.Save($commonTestSuitePtfConfigForAdapters)
  $oldString = '<!--<Adapter xsi:type="managed" name="ISutCommonControlAdapter" adaptertype="Microsoft.Protocols.TestSuites.FileSharing.Common.Adapter.SutCommonControlAdapter"/>-->'
  $newString = '<Adapter xsi:type="managed" name="ISutCommonControlAdapter" adaptertype="Microsoft.Protocols.TestSuites.FileSharing.Common.Adapter.SutCommonControlAdapter"/>'
    ((Get-Content -path $commonTestSuitePtfConfigForAdapters -Raw) -replace $oldString, $newString) | Set-Content -Path $commonTestSuitePtfConfigForAdapters
}

#----------------------------------------------------------------------------
# Update CommonTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------
if ($ContextName -match "Samba") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SutComputerName" $sutIP
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DomainName" $sutIP
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DCServerComputerName" ""
}
elseif ($ContextName -match "2025" -and $ContextName -match "Workgroup") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SutComputerName" "$sutIP"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DomainName" $sutIP
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DCServerComputerName" ""
}
elseif ($ContextName -match "Workgroup") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SutComputerName" "$sutName"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DomainName" $sutName
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DCServerComputerName" ""
}
else {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SutComputerName" "$sutName.$domain"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DomainName" $domain

  $dc = $config.lab.servers.vm | Where-Object { $_.role -eq "DC" }

  if (($dc -eq $null ) -or ($dc -eq "workgroup")) {
    if ($ContextName -match 'domain') {
      $dcName = [System.Net.Dns]::GetHostByAddress($driver.dns).HostName.Split(".")[0]
      ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DCServerComputerName" "$dcName.$domain"
    }
    else {
      $dcName = [System.DirectoryServices.ActiveDirectory.Domain]::GetCurrentDomain().DomainControllers[0].Name.Split(".")[0]
      ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DCServerComputerName" "$dcName"
    }
  }
  else {
    $dcName = $dc.name
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "DCServerComputerName" "$dcName.$domain"
  }
}

."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "AdminUserName" $adminUserName
."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "PasswordForAllUsers" $adminPassword
."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SutIPAddress" $sutIP

if ($ContextName -match "Samba") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "NonAdminUserName" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "GuestUserName" ""
}

# Update dialect 
$smbDialect = $ContextName.Split("_") | Where-Object { $_ -match "SMB" }
if (-not [System.String]::IsNullOrEmpty($smbDialect)) {
  $smbDialect = $smbDialect.Substring(0, 1).ToUpper() + $smbDialect.Substring(1).ToLower()
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "MaxSmbVersionSupported" "$smbDialect"
}

# Server Capabilities
# Samba has special capabilities
if ($ContextName -match "Samba") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsLeasingSupported" "false"	
  if ($ContextName -match "SMB2002") {        
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsMultiCreditSupported" "false"
  }
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsMultiChannelCapable" "false"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsDirectoryLeasingSupported" "false"
  if ($ContextName -match "SMB2002" -or $ContextName -match "SMB21") {
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsEncryptionSupported" "false"
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "EncryptedFileShare" ""
  }
}
# For Windows environment, set the capabilities according to [MS-SMB2]
if ($ContextName -match "SMB2002") {
  # [MS-SMB2] SMB2_GLOBAL_CAP_LEASING: When set, indicates that the server supports leasing. This flag is not valid for the SMB 2.002 dialect.
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsLeasingSupported" "false"
	
  # [MS-SMB2] SMB2_GLOBAL_CAP_LARGE_MTU: When set, indicates that the server supports multi-credit operations. This flag is not valid for the SMB 2.002 dialect.
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsMultiCreditSupported" "false"	
}
if ($ContextName -match "SMB2002" -or $ContextName -match "SMB21") {
  # [MS-SMB2] SMB2_GLOBAL_CAP_MULTI_CHANNEL: When set, indicates that the server supports establishing multiple channels for a single session. This flag is not valid for the SMB 2.002 and SMB 2.1 dialects. 
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsMultiChannelCapable" "false"
	
  # [MS-SMB2] SMB2_GLOBAL_CAP_DIRECTORY_LEASING: When set, indicates that the server supports directory leasing. This flag is not valid for the SMB 2.002 and SMB 2.1 dialects.
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsDirectoryLeasingSupported" "false"	
	
  # [MS-SMB2] SMB2_GLOBAL_CAP_ENCRYPTION: When set, indicates that the server supports encryption. This flag is not valid for the SMB 2.002 and SMB 2.1 dialects.
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsEncryptionSupported" "false"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "EncryptedFileShare" ""
}

if ($ContextName -match "Win2022" -and $ContextName -match "SMB311") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsRDMATransformSupported" "true"	
}

if ($ContextName -match "Win2025" -and $ContextName -match "SMB311") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsRDMATransformSupported" "true"	
}

if ($ContextName -match "Domain_Cluster") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsPersistentHandlesSupported" "true"	
}
else {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsPersistentHandlesSupported" "false"	
}

# UnsupportedIoCtlCodes
if ($ContextName -match "Samba_Workgroup_SMB311") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedIoCtlCodes" "FSCTL_FILE_LEVEL_TRIM;FSCTL_OFFLOAD_READ;FSCTL_OFFLOAD_WRITE;FSCTL_GET_INTEGRITY_INFORMATION;FSCTL_SET_INTEGRITY_INFORMATION;FSCTL_LMR_REQUEST_RESILIENCY"
}

if ($ContextName -match "Samba_Workgroup_SMB30") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedIoCtlCodes" "FSCTL_FILE_LEVEL_TRIM;FSCTL_OFFLOAD_READ;FSCTL_OFFLOAD_WRITE;FSCTL_GET_INTEGRITY_INFORMATION;FSCTL_SET_INTEGRITY_INFORMATION;FSCTL_LMR_REQUEST_RESILIENCY"
}

if ($ContextName -match "Samba_Workgroup_SMB21" -or $ContextName -match "Samba_Workgroup_SMB2002") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedIoCtlCodes" "FSCTL_FILE_LEVEL_TRIM;FSCTL_VALIDATE_NEGOTIATE_INFO;FSCTL_OFFLOAD_READ;FSCTL_OFFLOAD_WRITE;FSCTL_GET_INTEGRITY_INFORMATION;FSCTL_SET_INTEGRITY_INFORMATION;FSCTL_LMR_REQUEST_RESILIENCY"
}

if ($ContextName -match "Win2008") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedIoCtlCodes" "FSCTL_FILE_LEVEL_TRIM;FSCTL_VALIDATE_NEGOTIATE_INFO;FSCTL_OFFLOAD_READ;FSCTL_OFFLOAD_WRITE;FSCTL_GET_INTEGRITY_INFORMATION;FSCTL_SET_INTEGRITY_INFORMATION;FSCTL_LMR_REQUEST_RESILIENCY"
}
if ($ContextName -match "Win2008R2") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedIoCtlCodes" "FSCTL_FILE_LEVEL_TRIM;FSCTL_VALIDATE_NEGOTIATE_INFO;FSCTL_OFFLOAD_READ;FSCTL_OFFLOAD_WRITE;FSCTL_GET_INTEGRITY_INFORMATION;FSCTL_SET_INTEGRITY_INFORMATION"
}
# UnsupportedCreateContexts
if ($ContextName -match "Samba_Workgroup_SMB2002") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedCreateContexts" "SMB2_CREATE_DURABLE_HANDLE_REQUEST;SMB2_CREATE_DURABLE_HANDLE_RECONNECT;SMB2_CREATE_REQUEST_LEASE;SMB2_CREATE_REQUEST_LEASE_V2;SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2;SMB2_CREATE_DURABLE_HANDLE_RECONNECT_V2;SMB2_CREATE_APP_INSTANCE_ID"
}
elseif ($ContextName -match "Samba_Workgroup_SMB21" -or $ContextName -match "Samba_Workgroup_SMB311") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedCreateContexts" "SMB2_CREATE_REQUEST_LEASE;SMB2_CREATE_REQUEST_LEASE_V2;SMB2_CREATE_APP_INSTANCE_ID"
}
elseif ($ContextName -match "Samba_Workgroup_SMB21" -or $ContextName -match "Samba_Workgroup_SMB30") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedCreateContexts" "SMB2_CREATE_REQUEST_LEASE;SMB2_CREATE_REQUEST_LEASE_V2;SMB2_CREATE_APP_INSTANCE_ID"
}
elseif ($ContextName -match "SMB2002") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedCreateContexts" "SMB2_CREATE_DURABLE_HANDLE_REQUEST;SMB2_CREATE_DURABLE_HANDLE_RECONNECT;SMB2_CREATE_REQUEST_LEASE;SMB2_CREATE_REQUEST_LEASE_V2;SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2;SMB2_CREATE_DURABLE_HANDLE_RECONNECT_V2;SMB2_CREATE_APP_INSTANCE_ID"
}
elseif ($ContextName -match "SMB21") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedCreateContexts" "SMB2_CREATE_REQUEST_LEASE_V2;SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2;SMB2_CREATE_DURABLE_HANDLE_RECONNECT_V2;SMB2_CREATE_APP_INSTANCE_ID"
}
else {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnsupportedCreateContexts" ""
}

."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "ClientNic1IPAddress" $driverIP1
."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "ClientNic2IPAddress" $driverIP2

if ($ContextName -match "Domain_Cluster") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "CAShareServerName" "$generalFsName.$domain"
}
else {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "CAShareName" ""    
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "CAShareServerName" ""
}
# Update Platform
$platform = $ContextName.Split("_") | Select-Object -First 1
if ($platform -eq "Samba") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "Platform" "NonWindows"
}
else {
  # The TestCaseContext uses short name, need to replace with full name "WindowsServer"
  $platform = $platform.Replace("Win", "WindowsServer")
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "Platform" "$platform"
}

# Update compression configurations
$contextNotSupportCompression = @("Win2012R2", "Win2016", "Win2019")
$contextSupportCompressionWithoutChained = @("WinV1903")

if ($contextNotSupportCompression.Where({ $ContextName -match $_ }).Count -gt 0) {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SupportedCompressionAlgorithms" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsChainedCompressionSupported" "false"
}
elseif ($contextSupportCompressionWithoutChained.Where({ $ContextName -match $_ }).Count -gt 0) {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SupportedCompressionAlgorithms" "LZ77;LZ77Huffman;LZNT1"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsChainedCompressionSupported" "false"
}
else {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SupportedCompressionAlgorithms" "Pattern_V1;LZ77;LZ77Huffman;LZNT1"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "IsChainedCompressionSupported" "true"
}

# Update configurations specified for QUIC env
if ($ContextName -match "EnableQUIC") {
  # Update transport properties
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "UnderlyingTransport" "Quic"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "TransportPort" "443"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "AllowNamedPipeAccessOverQUIC" "true"

  # Update both SutName and SutIPAddress to use computer name
  $targetName = if ($ContextName -match "Workgroup") { $sutName } else { "$sutName.$domain" }
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SutComputerName" $targetName
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $commonTestSuitePtfConfig "SutIPAddress" $targetName
}

#----------------------------------------------------------------------------
# Update MS-SMB2_ServerTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------
if ($ContextName -match "Samba") {
  # Samba SUT has single IP 192.168.1.11
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2TestSuitePtfConfig "SutAlternativeIPAddress" $sutIP
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2TestSuitePtfConfig "SharePath" "\\$sutIP\SMBBasic"
}
elseif ($ContextName -match "2025" -and $ContextName -match "Workgroup") {
  # Samba SUT has single IP 192.168.1.11
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2TestSuitePtfConfig "SutAlternativeIPAddress" $sutIP
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2TestSuitePtfConfig "SharePath" "\\$sutName\SMBBasic"
}
else {
  # The second IP for workgroup and domain non-cluster SUT
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2TestSuitePtfConfig "SutAlternativeIPAddress" $sutAlternativeIPAddress
  if($ContextName -match "Workgroup") {
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2TestSuitePtfConfig "SharePath" "\\$sutName\SMBBasic"
  }
  else {
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2TestSuitePtfConfig "SharePath" "\\$sutName.$domain\SMBBasic"
  }
}

."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2TestSuitePtfConfig "ClusteredInfrastructureFileServerName" ""

#----------------------------------------------------------------------------
# Update MS-SMB2Model_ServerTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------
if ($ContextName -match "Samba") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "SpecialShare" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "PathSeparator" "/"	
}
if ($ContextName -match "Samba" -or $ContextName -match "Win2008") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ShareWithForceLevel2WithoutSOFS" ""
}
if ($ContextName -notmatch "Domain_Cluster") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ShareWithoutForceLevel2WithSOFS" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ShareWithForceLevel2AndSOFS" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ScaleOutFileServerName" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ScaleOutFileServerIP1" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ScaleOutFileServerIP2" ""
}
else {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ScaleOutFileServerName" "$scaleOutFsName.$domain" 
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ScaleOutFileServerIP1" $sutIP
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $smb2ModelPtfConfig "ScaleOutFileServerIP2" $sutAlternativeIPAddress
}

#----------------------------------------------------------------------------
# Update MS-DFSC_ServerTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------
if ($ContextName -match "Workgroup") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "DomainNetBIOSName" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "DomainFQDNName" ""
  #Modify-ConfigFileNode.ps1 $DfscPtfConfig "SiteName" ""
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "DomainNamespace" ""
}
else {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "DomainNetBIOSName" ($domain.Split("."))[0]
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "DomainFQDNName" $domain

  if ($driver.os -eq "Linux" -or $driver.os -eq "RPMBasedLinux") {
    if ($isLocalLinuxDriver) {
      $targetFileName = "DomainBased.txt"
      bash "$PSScriptRoot/CopyWindowsFileToLinux.sh" $sutIp "C$" $targetFileName $domain $config.lab.core.username $config.lab.core.password 
      $localFilePath = "/" + $targetFileName
      if (Test-Path $localFilePath) {
        $domainBasedNsName = Get-Content $localFilePath
        ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "DomainNamespace" $domainBasedNsName
      }
    }
    else {
      $domainBasedNsName = Invoke-Command -HostName $sutIp -ScriptBlock { Get-Content C:\DomainBased.txt }
      ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "DomainNamespace" $domainBasedNsName
    }
  }
  else {
    $domainBasedNsNamePath = "\\$sutIp\C$\DomainBased.txt"
    if (Test-Path $domainBasedNsNamePath) {
      $domainBasedNsName = Get-Content $domainBasedNsNamePath
      ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "DomainNamespace" $domainBasedNsName
    }
  }
}

if ($ContextName -match "Domain_Cluster") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "StorageServerName" $sut2.name
}
else {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "StorageServerName" $sutName
}

if ($ContextName -notmatch "Win2012") {
  # The OS later than 2012/2012R2 will use FQDN (since all the contexts are domain ENV) as RootTargetType.
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $dfscPtfConfig "RootTargetType" "FQDN"
}

#----------------------------------------------------------------------------
# Update ServerFailoverTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------
if ($ContextName -match "Domain_Cluster") {

  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $serverFailoverPtfConfig "ClusterName" $clusterName
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $serverFailoverPtfConfig "ClusterNode01" "$sutName.$domain"
  $node02Name = $sut2.name
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $serverFailoverPtfConfig "ClusterNode02" "$node02Name.$domain"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $serverFailoverPtfConfig "ClusteredFileServerName" "$generalFsName.$domain"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $serverFailoverPtfConfig "ClusteredScaleOutFileServerName" "$scaleOutFsName.$domain"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $serverFailoverPtfConfig "OptimumNodeOfAsymmetricShare" "$scaleOutFsName.$domain"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $serverFailoverPtfConfig "NonOptimumNodeOfAsymmetricShare" "$scaleOutFsName.$domain"
  $driverName = $driver.name
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $serverFailoverPtfConfig "WitnessClientName" "$driverName.$domain"
}

#----------------------------------------------------------------------------
# Update MS-RSVD_ServerTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------
if ($ContextName -match "Domain_Cluster") {
  $scaleOutFsName = $config.lab.ha.scaleoutfs.name
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $rsvdPtfConfig "ShareContainingSharedVHD" "\\$scaleOutFsName.$domain\SMBClustered"
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $rsvdPtfConfig "FileServerIPContainingSharedVHD" $sutIP
  if ($ContextName -match "Win2012R2_Domain_Cluster") {
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $rsvdPtfConfig "ServerServiceVersion" "0x00000001"
  }

  # RSVD version 2 is supported in Windows Server 2016 or later ( Domain Cluster )
  if (($ContextName -notmatch "Win2012") -and ($ContextName -match "Domain_Cluster")) {
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $rsvdPtfConfig "ServerServiceVersion" "0x00000002"
  }
}

#----------------------------------------------------------------------------
# Update MS-FSA_ServerTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------
if ($ContextName -notmatch "Win2012") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $fsaPtfConfig "ReFSVersion" "2"
}

# Before Windows Server 2016 RS1, encryption is only supported by NTFS.
if ($ContextName -match "Win2008" -or $ContextName -match "Win2012") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $fsaPtfConfig "WhichFileSystemSupport_Encryption" "NTFS"
}

#----------------------------------------------------------------------------
# Update MS-SQOS_ServerTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------
# SQOS only valid in Windows Server 2016 or later ( Domain Cluster )
if (($ContextName -notmatch "Win2012") -and ($ContextName -match "Domain_Cluster")) {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $sqosPtfConfig "ShareContainingSharedVHD" "\\$scaleOutFsName.$domain\SMBClustered"

  $minIops = ReadPtfConfigProperty $sqosPtfConfig "SqosMinimumIoRate"
  $maxIops = ReadPtfConfigProperty $sqosPtfConfig "SqosMaximumIoRate"
  $maxBandwidthInKB = ReadPtfConfigProperty $sqosPtfConfig "SqosMaximumBandwidth"
  $maxBandwidth = 1024 * $maxBandwidthInKB

  if ($driver.os -eq "Linux" -or $driver.os -eq "RPMBasedLinux") {
    if ($isLocalLinuxDriver) {
      $targetFileName = "SqosPolicyId.txt"
      bash "$PSScriptRoot/CopyWindowsFileToLinux.sh" $sutIp "C$" $targetFileName $domain $config.lab.core.username $config.lab.core.password 
      $localFilePath = "/" + $targetFileName
      if (Test-Path $localFilePath) {
        $policyId = Get-Content $localFilePath
        ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $sqosPtfConfig "SqosPolicyId" $policyId
      }
    }
    else {
      $policy = Invoke-Command -HostName $sutComputerName -ScriptBlock { param($minIops, $maxIops, $maxBandwidth) New-StorageQosPolicy -Name Desktop -PolicyType Dedicated -MinimumIops $minIops -MaximumIops $maxIops -MaximumIOBandwidth $maxBandwidth } -ArgumentList $minIops, $maxIops, $maxBandwidth
    }
  }
  else {
    $policy = Invoke-Command -ComputerName $sutName -ScriptBlock { param($minIops, $maxIops, $maxBandwidth) New-StorageQosPolicy -Name Desktop -PolicyType Dedicated -MinimumIops $minIops -MaximumIops $maxIops -MaximumIOBandwidth $maxBandwidth } -ArgumentList $minIops, $maxIops, $maxBandwidth
  }    
  if ($policy -ne $null) {
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $sqosPtfConfig "SqosPolicyId" $policy.PolicyId
  }
}

#----------------------------------------------------------------------------
# Update Auth_ServerTestSuite.deployment.ptfconfig
#----------------------------------------------------------------------------

if ($ContextName -match "Domain") {
  $modifiedSalt = $domain.ToUpper() + "host" + $sutName.ToLower() + "." + $domain.ToLower()
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $authPtfConfig "ServiceSaltString" $modifiedSalt 
}


#----------------------------------------------------------------------------
#Validate environment and exclude cases by update ptfconfig
#----------------------------------------------------------------------------
if ($driver.os -ne "Linux" -and $driver.os -ne "RPMBasedLinux") {
  Validate-Environment.ps1 -ContextName $ContextName
}

Pop-Location

#----------------------------------------------------------------------------
# Functions for running test cases
#----------------------------------------------------------------------------
function RenameTrxResultFile($newName) {
  $trxResultFile = Get-ChildItem (Resolve-Path "$testDir/TestResults/*.trx") | Sort-Object LastWriteTime -Descending | Select-Object -First 1
  if ($trxResultFile -ne $null) {
    Rename-Item -Path $trxResultFile -NewName $newName
  }
}

function ExecuteTestCases($testContainer, $trxResultFileName, $testCaseFilter = "") {
  ."$PSScriptRoot/Write-Info.ps1" "Execute test cases in test container: $testContainer"
  if ([System.String]::IsNullOrEmpty($testCaseFilter)) {
    dotnet vstest (Resolve-Path "$testContainer") /Logger:trx 2>&1 | ."$PSScriptRoot/Write-Info.ps1"
  }
  else {
    ."$PSScriptRoot/Write-Info.ps1" "Filtered by: $testCaseFilter"   
    dotnet vstest (Resolve-Path "$testContainer") /TestCaseFilter:"$testCaseFilter" /Logger:trx 2>&1 | ."$PSScriptRoot/Write-Info.ps1"
  }
  Start-Sleep 10
  RenameTrxResultFile $trxResultFileName
}

function ExecuteTestCasesArray($ArrayCommands) {
   
  $ArrayCommands | ForEach-Object {
      $command = $_
      $root = $PSScriptRoot
      
      # Access properties directly
      $testContainer = $command.TestContainer
      $testCaseFilter = $command.TestCaseFilter
      $trxResultFileName = $command.TrxResultFileName
      
      ."$root/Write-Info.ps1" "Execute test cases in test filter: $($command.TestCaseFilter)"
      ."$root/Write-Info.ps1" "Execute test cases in test container: $($command.TestContainer)"
      
      if ([System.String]::IsNullOrEmpty($testCaseFilter)) {
        dotnet vstest (Resolve-Path $testContainer) /Logger:trx 2>&1 | ."$root/Write-Info.ps1"
      }
      else { 
        ."$root/Write-Info.ps1" "Filtered by: $testCaseFilter"   
        dotnet vstest (Resolve-Path $testContainer) /TestCaseFilter:"$testCaseFilter" /Logger:trx 2>&1 | ."$root/Write-Info.ps1"
      }
      Start-Sleep 10
      $trxResultFile = Get-ChildItem (Resolve-Path "$testDir/TestResults/*.trx") | Sort-Object LastWriteTime -Descending | Select-Object -First 1
      if ($trxResultFile -ne $null) {
        Rename-Item -Path $trxResultFile -NewName $trxResultFileName
      }
  }   
}

function ExecuteFsaTestCases($fileSystem, $transports, $testCaseFilter = "") {
  ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $fsaPtfConfig "FileSystem" $fileSystem.ToUpper()
  $transportArray = $transports.Split("|")
  foreach ($transport in $transportArray) {
     $FSATestArray = @();

    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $fsaPtfConfig "Transport" "$transport"

    $trxFileName = "MS-FSA_ServerTestSuite_" + $fileSystem.ToUpper() + "_" + "$transport" + ".trx"
    $test = [TestVariables]::new("$binDir/MS-FSA_ServerTestSuite.dll", "$trxFileName", $testCaseFilter)
    $FSATestArray += $test

    $trxFileName = "MS-FSAModel_ServerTestSuite_" + $fileSystem.ToUpper() + "_" + "$transport" + ".trx"
    $test = [TestVariables]::new("$binDir/MS-FSAModel_ServerTestSuite.dll", "$trxFileName", $testCaseFilter)
    $FSATestArray += $test

    ExecuteTestCasesArray($FSATestArray)
  }
}

function CheckIfHasFailure($trxResultFileName) {  
  $resultFile = "$testDir/TestResults/$trxResultFileName"
  [xml]$resultContent = Get-Content $resultFile
  $resultSummary = $resultContent.TestRun.ResultSummary.Counters
  [int]$totalNum = $resultSummary.total
  [int]$passedNum = $resultSummary.passed

  # (1) When test cases are inconclusive, they will not be showed in TRX result, and not calculated in result summary.
  # (2) There are many items in result summary, use "total == passed" is an easy way to check if all test cases are passed.
  # Result summary example:
  # <Counters total="74" executed="74" passed="74" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" 
  # passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
  if ($totalNum -gt 0 -and $passedNum -gt 0 -and $totalNum -eq $passedNum) {
    $hasFailure = $false;
    ."$PSScriptRoot/Write-Info.ps1" "All test cases passed in $trxResultFileName."
  }
  else {
    $hasFailure = $true;        
    ."$PSScriptRoot/Write-Info.ps1" "Test result $trxResultFileName has failures."
    ."$PSScriptRoot/Write-Info.ps1" "Below is the result summary:"
    ."$PSScriptRoot/Write-Info.ps1" ($resultSummary | Format-List | Out-String)
  }

  return $hasFailure
}

function ExecuteAllSMB2TestCases([string]$AdditionalTestCaseFilter = "", [string]$RunTestFilter = "") {
   
  $SMBTestArray = @();

  # Exclude AppInstanceId test cases which will be executed separately.
  $testFilter = "TestCategory!=AppInstanceId"
  $testFilter += "&Name!=AppInstanceId_DirectoryLeasing_NoLeaseInReOpen"
  $testFilter += "&Name!=AppInstanceId_Encryption"
  $testFilter += "&Name!=AppInstanceId_FileLeasing_NoLeaseInReOpen"
  $testFilter += "&Name!=AppInstanceId_Negative_EncryptionInInitialOpen_NoEncryptionInReOpen"
  $testFilter += "&Name!=AppInstanceId_Negative_NoEncryptionInInitialOpen_EncryptionInReOpen"
  if (-not [System.String]::IsNullOrEmpty($AdditionalTestCaseFilter)) {
    $testFilter += "&" + $AdditionalTestCaseFilter
  }

  if (-not [System.String]::IsNullOrEmpty($RunTestFilter)) {
    $testFilter = $RunTestFilter
    $SMBTestArray += [TestVariables]::new("$binDir/MS-SMB2Model_ServerTestSuite.dll",  "MS-SMB2Model_ServerTestSuite.trx", "$testFilter")
  }
  else {
    $SMBTestArray += [TestVariables]::new("$binDir/MS-SMB2Model_ServerTestSuite.dll",  "MS-SMB2Model_ServerTestSuite.trx", "TestCategory!=AppInstanceId")
  }

  $SMBTestArray += [TestVariables]::new("$binDir/MS-SMB2_ServerTestSuite.dll", "MS-SMB2_ServerTestSuite.trx", "$testFilter")

  # Execute AppInstanceId test cases
  $testFilter = "TestCategory=AppInstanceId"
  $testFilter += "|Name=AppInstanceId_DirectoryLeasing_NoLeaseInReOpen"
  $testFilter += "|Name=AppInstanceId_Encryption"
  $testFilter += "|Name=AppInstanceId_FileLeasing_NoLeaseInReOpen"
  $testFilter += "|Name=AppInstanceId_Negative_EncryptionInInitialOpen_NoEncryptionInReOpen"
  $testFilter += "|Name=AppInstanceId_Negative_NoEncryptionInInitialOpen_EncryptionInReOpen"
  if (-not [System.String]::IsNullOrEmpty($RunTestFilter)) {
    $testFilter = $RunTestFilter
  }
  $SMBTestArray += [TestVariables]::new("$binDir/MS-SMB2Model_ServerTestSuite.dll", "MS-SMB2_AppInstanceIdTestCases.trx", "$testFilter")

  ExecuteTestCasesArray($SMBTestArray)
}

if ($driver.os -ne "Linux" -and $driver.os -ne "RPMBasedLinux") {
  #----------------------------------------------------------------------------
  # Make sure RemoteAccess related services are running in domain controller
  #----------------------------------------------------------------------------
  $currentComputer = Get-WmiObject Win32_ComputerSystem
  $domain = $currentComputer.Domain

  function StartService($serviceName) {
    $service = Get-Service -Name $serviceName -ComputerName $domain
    $retryTimes = 0
    while ($service.Status -ne "Running" -and $retryTimes -lt 6) {
      ."$PSScriptRoot/Write-Info.ps1" "Start $serviceName service."
      Start-Service -InputObj $service -ErrorAction Continue
      Start-Sleep 10
      $retryTimes++ 
      $service = Get-Service -Name $serviceName
    }

    if ($retryTimes -ge 6) {
      ."$PSScriptRoot/Write-Error.ps1" "Service $serviceName cannot be started within 1 minute."
    }
    else {
      ."$PSScriptRoot/Write-Info.ps1" "Service $serviceName is Running."
    }
  }

  if ($currentComputer.PartOfDomain) {
    StartService "sstpsvc"
    StartService "rasman"
    StartService "RemoteAccess"
  }
}

#----------------------------------------------------------------------------
# Run test cases
#----------------------------------------------------------------------------
if ($runTests -eq "true") {
  Push-Location $testDir

  ."$PSScriptRoot/Write-Info.ps1" "Current context is $ContextName"

  # For the Windows server 2016 or later (Domain Cluster)
  if (($ContextName.trim() -notmatch "Win2012") -and ($ContextName.trim() -match "Domain_Cluster")) {
    # Execute SMB2 test cases
    ExecuteAllSMB2TestCases
    # Execute DFSC/FSRVP/RSVD test cases
    $test = [TestVariables]::new( "$binDir/MS-DFSC_ServerTestSuite.dll",  "MS-DFSC_ServerTestSuite.trx", "")
    $TestArray += $test

    $test = [TestVariables]::new( "$binDir/MS-FSRVP_ServerTestSuite.dll",  "MS-FSRVP_ServerTestSuite.trx", "")
    $TestArray += $test
    
    $test = [TestVariables]::new( "$binDir/MS-RSVD_ServerTestSuite.dll",  "MS-RSVD_ServerTestSuite.trx", "TestCategory=RsvdVersion1|TestCategory=RsvdVersion2")
    $TestArray += $test

    # SQOS is a new feature of Threshold
    # Test both versions of SQOS: 1.0 and 1.1
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $sqosPtfConfig "SqosClientDialect" Sqos10
    $test = [TestVariables]::new( "$binDir/MS-SQOS_ServerTestSuite.dll",  "MS-SQOS_ServerTestSuite_V10.trx", "")
    $TestArray += $test

    ExecuteTestCasesArray($TestArray)
    $TestArray = @()

    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" $sqosPtfConfig "SqosClientDialect" Sqos11
    $test = [TestVariables]::new( "$binDir/MS-SQOS_ServerTestSuite.dll",  "MS-SQOS_ServerTestSuite_V11.trx", "")
    $TestArray += $test

    # Run all FSA test cases against NTFS file system with both SMB3 transport. (SMB2 transport behavior is same as SMB3)
    ExecuteFsaTestCases -FileSystem "NTFS" -Transports "SMB3"
    # Run all FSA test cases against REFS file system with SMB3 transport.
    ExecuteFsaTestCases -FileSystem "REFS" -Transports "SMB3"
    # Run all FSA test cases against FAT32 file system with SMB3 transport.
    ExecuteFsaTestCases -FileSystem "FAT32" -Transports "SMB3"

    # Execute Auth test suite
    $test = [TestVariables]::new("$binDir/Auth_ServerTestSuite.dll", "Auth_ServerTestSuite.trx", "")
    $TestArray += $test

    # Execute failover test cases
    if (-not $isAzureCluster) {
      # Exclude following SWN test cases related to failover
      $testFilter = "Name!=SWNFileServerFailover_FileServer"
      $testFilter += "&Name!=SWNFileServerFailover_ScaleOutFileServer"
      $testFilter += "&Name!=BVT_SWNGetInterfaceList_ClusterSingleNode"
      $testFilter += "&Name!=BVT_SWNGetInterfaceList_ScaleOutSingleNode"
      $testFilter += "&Name!=BVT_WitnessrRegister_SWNAsyncNotification_ClientMove"
      $testFilter += "&Name!=BVT_WitnessrRegisterEx_SWNAsyncNotification_ClientMove"
      $testFilter += "&Name!=BVT_WitnessrRegisterEx_SWNAsyncNotification_IPChange" 	

      $test = [TestVariables]::new("$binDir/ServerFailoverTestSuite.dll", "ServerFailoverTestSuite.trx", "$testFilter")
      $TestArray += $test
    }

    ExecuteTestCasesArray($TestArray)
    $TestArray = @()
  }
  elseif ($ContextName.trim() -match "Win2012R2_Domain_Cluster_SMB302") {
    # Execute SMB2 test cases
    ExecuteAllSMB2TestCases
    # Execute DFSC/FSRVP/RSVD test cases
    $test = [TestVariables]::new("$binDir/MS-DFSC_ServerTestSuite.dll", "MS-DFSC_ServerTestSuite.trx", "")
    $TestArray += $test

    $test = [TestVariables]::new("$binDir/MS-FSRVP_ServerTestSuite.dll", "MS-FSRVP_ServerTestSuite.trx", "")
    $TestArray += $test

    $test = [TestVariables]::new("$binDir/MS-RSVD_ServerTestSuite.dll", "MS-RSVD_ServerTestSuite.trx", "TestCategory=RsvdVersion1")
    $TestArray += $test

    # Run all FSA test cases against NTFS file system with both SMB3 transport. (SMB2 transport behavior is same as SMB3)
    ExecuteFsaTestCases -FileSystem "NTFS" -Transports "SMB3"
    # Run all FSA test cases against REFS file system with SMB3 transport.
    ExecuteFsaTestCases -FileSystem "REFS" -Transports "SMB3"
    # Run all FSA test cases against FAT32 file system with SMB3 transport.
    ExecuteFsaTestCases -FileSystem "FAT32" -Transports "SMB3"

    # Execute Auth test suite
    $test = [TestVariables]::new("$binDir/Auth_ServerTestSuite.dll", "Auth_ServerTestSuite.trx", "")
    $TestArray += $test

    # Execute failover test cases
    if (-not $isAzureCluster) {
      # Exclude following SWN test cases related to failover
      $testFilter = "Name!=SWNFileServerFailover_FileServer"
      $testFilter += "&Name!=SWNFileServerFailover_ScaleOutFileServer"
      $testFilter += "&Name!=BVT_SWNGetInterfaceList_ClusterSingleNode"
      $testFilter += "&Name!=BVT_SWNGetInterfaceList_ScaleOutSingleNode"
      $testFilter += "&Name!=BVT_WitnessrRegister_SWNAsyncNotification_ClientMove"
      $testFilter += "&Name!=BVT_WitnessrRegisterEx_SWNAsyncNotification_ClientMove"
      $testFilter += "&Name!=BVT_WitnessrRegisterEx_SWNAsyncNotification_IPChange"    	
      $test = [TestVariables]::new("$binDir/ServerFailoverTestSuite.dll", "ServerFailoverTestSuite.trx", "$testFilter")
      $TestArray += $test
    }  
    
    ExecuteTestCasesArray($TestArray)
    $TestArray = @()
  }
  elseif ($ContextName.trim() -match "Win2012_Domain_Cluster_SMB30") {
    # Execute SMB2 test cases
    ExecuteAllSMB2TestCases

    # Execute DFSC/FSRVP test cases
    $test = [TestVariables]::new("$binDir/MS-DFSC_ServerTestSuite.dll", "MS-DFSC_ServerTestSuite.trx", "")
    $TestArray += $test

    $test = [TestVariables]::new("$binDir/MS-FSRVP_ServerTestSuite.dll", "MS-FSRVP_ServerTestSuite.trx", "")
    $TestArray += $test

    # Run all FSA test cases against NTFS file system with both SMB3 transport. (SMB2 transport behavior is same as SMB3)
    ExecuteFsaTestCases -FileSystem "NTFS" -Transports "SMB3"
    # Run all FSA test cases against REFS file system with SMB3 transport.
    ExecuteFsaTestCases -FileSystem "REFS" -Transports "SMB3"
    # Run all FSA test cases against FAT32 file system with SMB3 transport.
    ExecuteFsaTestCases -FileSystem "FAT32" -Transports "SMB3"

    # Execute Auth test suite
    $test = [TestVariables]::new("$binDir/Auth_ServerTestSuite.dll", "Auth_ServerTestSuite.trx", "")
    $TestArray += $test

    # Execute failover test cases
    if (-not $isAzureCluster) {
      # Exclude SMB302, and SWN version 2 test cases which are not supported by Win8 OS
      $testFilter = "TestCategory!=Smb302"

      # Exclude following SWN test cases related to failover
      $testFilter += "&Name!=SWNFileServerFailover_FileServer"
      $testFilter += "&Name!=SWNFileServerFailover_ScaleOutFileServer"
      $testFilter += "&Name!=BVT_SWNGetInterfaceList_ClusterSingleNode"
      $testFilter += "&Name!=BVT_SWNGetInterfaceList_ScaleOutSingleNode"
      $testFilter += "&Name!=BVT_WitnessrRegister_SWNAsyncNotification_ClientMove"
      $testFilter += "&Name!=BVT_WitnessrRegisterEx_SWNAsyncNotification_ClientMove"
      $testFilter += "&Name!=BVT_WitnessrRegisterEx_SWNAsyncNotification_IPChange"

      # Exclude following SWN test cases which are not supported by Win8 OS
      $testFilter += "&Name!=WitnessrRegisterEx_SWNAsyncNotification_Timeout"
      $testFilter += "&Name!=SWNRegistrationEx_InvalidIpAddress"
      $testFilter += "&Name!=SWNRegistrationEx_InvalidNetName"
      $testFilter += "&Name!=SWNRegistrationEx_InvalidShareName"
      $testFilter += "&Name!=SWNRegistrationEx_InvalidUnRegister"
      $testFilter += "&Name!=SWNRegistrationEx_InvalidVersion"

      $test = [TestVariables]::new("$binDir/ServerFailoverTestSuite.dll", "ServerFailoverTestSuite.trx", "$testFilter")
      $TestArray += $test
    } 
    
    ExecuteTestCasesArray($TestArray)
    $TestArray = @()
  }
  elseif ($ContextName -match "Samba") {
    # Run only MS-SMB2 test cases in Samba environment
    # Base filter
    $testCategory = "TestCategory!=Smb302"
    $testCategory += "&TestCategory!=PersistentHandle"	
    
    if ($ContextName -match "SMB311") {
      # Append from base filter	
      $testCategory += "&TestCategory!=AppInstanceId"
      # Support DurableHandleV1BatchOplock,DurableHandleV2BatchOplock		
      $testCategory += "&TestCategory!=LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV1LeaseV1"
      $testCategory += "&TestCategory!=LeaseV2"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV2"	
      $testCategory += "&TestCategory!=Replay"	
      $testCategory += "&TestCategory!=DirectoryLeasing"		
      $testCategory += "&TestCategory!=FsctlLmrRequestResiliency"
      $testCategory += "&TestCategory!=MultipleChannel"
    }
    elseif ($ContextName -match "SMB30") {
      # Append from base filter	
      $testCategory += "&TestCategory!=AppInstanceId"
      # Support DurableHandleV1BatchOplock,DurableHandleV2BatchOplock		
      $testCategory += "&TestCategory!=LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV1LeaseV1"
      $testCategory += "&TestCategory!=LeaseV2"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV2"	
      $testCategory += "&TestCategory!=Replay"	
      $testCategory += "&TestCategory!=DirectoryLeasing"		
      $testCategory += "&TestCategory!=FsctlLmrRequestResiliency"
      $testCategory += "&TestCategory!=MultipleChannel"
    }
    elseif ($ContextName -match "SMB21") {
      # Append from base filter
      $testCategory += "&TestCategory!=Smb30"
      $testCategory += "&TestCategory!=AppInstanceId"
      $testCategory += "&TestCategory!=LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV1BatchOplock"
      $testCategory += "&TestCategory!=DurableHandleV1LeaseV1"
      $testCategory += "&TestCategory!=LeaseV2"
      $testCategory += "&TestCategory!=DurableHandleV2BatchOplock"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV2"
      $testCategory += "&TestCategory!=Replay"
      $testCategory += "&TestCategory!=Encryption"
      $testCategory += "&TestCategory!=FsctlValidateNegotiateInfo"
      $testCategory += "&TestCategory!=DirectoryLeasing"
      $testCategory += "&TestCategory!=FsctlLmrRequestResiliency"
      $testCategory += "&TestCategory!=FsctlValidateNegotiateInfo"
      $testCategory += "&TestCategory!=MultipleChannel"
    }
    else {
      # SMB2002
      # Append from base filter
      $testCategory += "&TestCategory!=Smb30"
      $testCategory += "&TestCategory!=Smb21"
      $testCategory += "&TestCategory!=AppInstanceId"
      $testCategory += "&TestCategory!=LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV1BatchOplock"
      $testCategory += "&TestCategory!=DurableHandleV1LeaseV1"
      $testCategory += "&TestCategory!=LeaseV2"
      $testCategory += "&TestCategory!=DurableHandleV2BatchOplock"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV2"
      $testCategory += "&TestCategory!=Replay"
      $testCategory += "&TestCategory!=Encryption"
      $testCategory += "&TestCategory!=FsctlValidateNegotiateInfo"
      $testCategory += "&TestCategory!=DirectoryLeasing"
      $testCategory += "&TestCategory!=FsctlLmrRequestResiliency"
      $testCategory += "&TestCategory!=FsctlValidateNegotiateInfo"
      $testCategory += "&TestCategory!=MultipleChannel"	
    }		
      
    $test = [TestVariables]::new("$binDir/MS-SMB2Model_ServerTestSuite.dll", "MS-SMB2Model_ServerTestSuite.trx", "$testCategory")
    $TestArray += $test

    $test = [TestVariables]::new("$binDir/MS-SMB2_ServerTestSuite.dll", "MS-SMB2_ServerTestSuite.trx", "$testCategory")
    $TestArray += $test
    
    ExecuteTestCasesArray($TestArray)
    $TestArray = @()
  }
  else {
    # Run in workgroup or domain non-cluster environment	
    # Base filter
    $testCategory = "TestCategory!=Cluster"
    $testCategory += "&TestCategory!=PersistentHandle"
    $testCategory += "&TestCategory!=OperateOneFileFromTwoNodes"
    
    if ($ContextName -match "SMB311") {
      # Use base filter
      if ($ContextName -match "Workgroup"){
          $testCategory += "&TestCategory!=DomainRequired"
          $testCategory += "&TestCategory!=Dfsc"
          $testCategory += "&TestCategory!=Auth"
      }
    }
    elseif ($ContextName -match "SMB302") {
      $testCategory += "&TestCategory!=Smb311" 
    }
    elseif ($ContextName -match "SMB30") {
      # Append from base filter
      $testCategory += "&TestCategory!=Smb311"
      $testCategory += "&TestCategory!=Smb302"
    }
    elseif ($ContextName -match "SMB21") {
      # Append from base filter
      $testCategory += "&TestCategory!=Smb311"
      $testCategory += "&TestCategory!=Smb302"
      $testCategory += "&TestCategory!=Smb30"
      $testCategory += "&TestCategory!=AppInstanceId"
      $testCategory += "&TestCategory!=LeaseV2"
      $testCategory += "&TestCategory!=DurableHandleV2BatchOplock"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV2"
      $testCategory += "&TestCategory!=Replay"
      $testCategory += "&TestCategory!=Encryption"
      $testCategory += "&TestCategory!=DirectoryLeasing"
      $testCategory += "&TestCategory!=FsctlValidateNegotiateInfo"			
    }
    else {
      # SMB2002
      # Append from base filter
      $testCategory += "&TestCategory!=Smb311"
      $testCategory += "&TestCategory!=Smb302"	
      $testCategory += "&TestCategory!=Smb30"
      $testCategory += "&TestCategory!=Smb21"
      $testCategory += "&TestCategory!=AppInstanceId"
      $testCategory += "&TestCategory!=LeaseV1"		
      $testCategory += "&TestCategory!=DurableHandleV1BatchOplock"
      $testCategory += "&TestCategory!=DurableHandleV1LeaseV1"
      $testCategory += "&TestCategory!=LeaseV2"
      $testCategory += "&TestCategory!=DurableHandleV2BatchOplock"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV1"
      $testCategory += "&TestCategory!=DurableHandleV2LeaseV2"
      $testCategory += "&TestCategory!=Replay"
      $testCategory += "&TestCategory!=Encryption"
      $testCategory += "&TestCategory!=DirectoryLeasing"
      $testCategory += "&TestCategory!=FsctlValidateNegotiateInfo"
    }

    $testFilter = ""

    if($CategoryName -ne "") {
      # Append from base filter
      $testFilter = "TestCategory=$CategoryName"
    }

    if($TestName -ne "" -and $CategoryName -eq "") {
      # Append from base filter
      $testFilter = "Name=$TestName"
    }
    elseif ($TestName -ne "" -and $CategoryName -ne "") {
      # Append from base filter
      $testFilter += "&Name=$TestName"
    }

    ."$PSScriptRoot/Write-Info.ps1" "Current test case filter is $testCategory"

    # Execute SMB2 test cases
    ExecuteAllSMB2TestCases -AdditionalTestCaseFilter $testCategory -RunTestFilter $testFilter

      if($CategoryName -eq "" -and $TestName -eq ""){
        # Run all FSA test cases against NTFS file system with both SMB3 transport. (SMB2 transport behavior is same as SMB3)
        ExecuteFsaTestCases -FileSystem "NTFS" -Transports "SMB3"
        # Run all FSA test cases against REFS file system with SMB3 transport.
        ExecuteFsaTestCases -FileSystem "REFS" -Transports "SMB3"
        # Run all FSA test cases against FAT32 file system with SMB3 transport.
        ExecuteFsaTestCases -FileSystem "FAT32" -Transports "SMB3"

        # Execute Auth test cases (Exclude Workgroup)
        if ($ContextName -notmatch "Workgroup"){
          $test = [TestVariables]::new("$binDir/Auth_ServerTestSuite.dll", "Auth_ServerTestSuite.trx", "")
          $TestArray += $test
        }

        # Execute DFSC test cases (Exclude Workgroup)
        if ($ContextName -notmatch "Workgroup"){
          $test = [TestVariables]::new("$binDir/MS-DFSC_ServerTestSuite.dll", "MS-DFSC_ServerTestSuite.trx", "")
          $TestArray += $test
        }

        ExecuteTestCasesArray($TestArray)
        $TestArray = @()
      }
      else
      {   
          $testFilter = ""

          if($CategoryName -ne "") {
              # Append from base filter
              $testFilter = "TestCategory=$CategoryName"
          }

          if($TestName -ne "" -and $testFilter -eq "") {
              # Append from base filter
              $testFilter = "Name=$TestName"
          }
          elseif($TestName -ne "" -and $testFilter -ne "") {
              # Append from base filter
              $testFilter += "&Name=$TestName"
          }

          # Run all FSA test cases against NTFS file system with both SMB3 transport. (SMB2 transport behavior is same as SMB3)
          ExecuteFsaTestCases -FileSystem "NTFS" -Transports "SMB3" -TestCaseFilter "$testFilter"
          # Run all FSA test cases against REFS file system with SMB3 transport.
          ExecuteFsaTestCases -FileSystem "REFS" -Transports "SMB3" -TestCaseFilter "$testFilter"
          # Run all FSA test cases against FAT32 file system with SMB3 transport.
          ExecuteFsaTestCases -FileSystem "FAT32" -Transports "SMB3" -TestCaseFilter "$testFilter"

        # Execute Auth test cases (Exclude Workgroup)
        if ($ContextName -notmatch "Workgroup"){
            $test = [TestVariables]::new("$binDir/Auth_ServerTestSuite.dll", "Auth_ServerTestSuite.trx", "$testFilter")
            $TestArray += $test
        }

        # Execute DFSC test cases (Exclude Workgroup)
        if ($ContextName -notmatch "Workgroup"){
            $test = [TestVariables]::new("$binDir/MS-DFSC_ServerTestSuite.dll", "MS-DFSC_ServerTestSuite.trx", "$testFilter")
            $TestArray += $test
        }

        ExecuteTestCasesArray($TestArray)
        $TestArray = @()
      }
    }
    Pop-Location
  }
#----------------------------------------------------------------------------
# Write finish signal, stop logging and exit
#----------------------------------------------------------------------------
Write-Output "test.finished.signal" > $finishSignalFile

try {
  Stop-Transcript
}
catch {
  
}


class TestVariables {
    # Properties
    [string]$TestContainer
    [string]$TrxResultFileName
    [string]$TestCaseFilter

    # Constructor
    TestVariables([string]$testContainer, [string]$trxResultFileName, [string]$testCaseFilter) {
        $this.TestContainer = $testContainer
        $this.TrxResultFileName = $trxResultFileName
        $this.TestCaseFilter = $testCaseFilter
    }

}