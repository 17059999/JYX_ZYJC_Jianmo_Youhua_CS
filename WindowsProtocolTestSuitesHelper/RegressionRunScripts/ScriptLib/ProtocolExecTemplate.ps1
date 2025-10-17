#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Execute-ProtocolTest.ps1
## Purpose:        This is the entry powershell script for protocol [PROTOCOL]
## Version:        2.0 ([DATE ex. 21 June, 2011])
## Requirements:   Windows Powershell 2.0 CTP2
## Supported OS:   Windows 2008 Server x64
##
##############################################################################

param(
[string]$protocolName = "MS-SMB2", 
[string]$cluster      = "C6", 

[string]$srcVMPath, 
[string]$srcScriptLibPath,
[string]$srcToolPath, 
[string]$srcTestSuitePath, 
[string]$ISOPath, 

[string]$clientOS,
[string]$serverOS,
[string]$workgroupDomain,  
[string]$IPVersion, 
[string]$ClientCPUArchitecture,
[string]$ServerCPUArchitecture,

[string]$workingDir, 
[string]$VMDir,
[string]$testResultDir,

[string]$userNameInVM,
[string]$userPwdInVM,
[string]$domainInVM, 
[string]$testDirInVM
)

Set-StrictMode -v 2

#----------------------------------------------------------------------------
# Define source folders in VM Host
#----------------------------------------------------------------------------
$srcScriptLibPathOnHost = $workingDir + "\ScriptLib\"
$srcToolPathOnHost      = $workingDir + "\Tools\"
$srcScriptPathOnHost    = $workingDir + "\$protocolName\Scripts\"
$srcTestSuitePathOnHost = $workingDir + "\$protocolName\Bin\"
$srcMyToolPathOnHost    = $workingDir + "\$protocolName\MyTools\"
$srcDataPathOnHost      = $workingDir + "\$protocolName\Data\"
$srcSnapshotPathOnHost  = $workingDir + "\$protocolName\Snapshot\"

#----------------------------------------------------------------------------
# Vefity parameters
#----------------------------------------------------------------------------
if ($protocolName -ne "[PROTOCOL NAME]")
{
    Throw "Protocol name doesn't match. [PROTOCOL NAME] is required, actual $protocolName."
}

[CHECK ANY OTHER PARAMETERS, ALWAYS USING -ne.]

#----------------------------------------------------------------------------
# Allocate resource lock
#----------------------------------------------------------------------------
.\Allocate-Lock.ps1

#----------------------------------------------------------------------------
# Get VM folder name and computer name
#----------------------------------------------------------------------------
if ($workgroupDomain -eq "Domain")
{
    $DCVMName = "W2K8-x86-DC"        ### .\GetVMName.ps1       $serverOS $CPUArchitecture "DC" "01"
    $DCComputerName = "DC01"         ### .\GetComputerName.ps1 $serverOS $CPUArchitecture "DC" "01"
}

[ALWAYS USE -eq BELOW]

$serverVMName       = .\Get-VMName.ps1 $serverOS $serverCPUArchitecture "SUT" 01
$serverComputerName = .\Get-ComputerIP.ps1 $IPVersion "SUT" 1      ### "SUT01"

$clientVMName       = .\Get-VMName.ps1 $clientOS $clientCPUArchitecture "ENDPOINT" 01
$clientComputerName = .\Get-ComputerIP.ps1 $IPVersion "ENDPOINT" 1 ### "ENDPOINT01"

#----------------------------------------------------------------------------
# Create Internal Network
#----------------------------------------------------------------------------
$hostIP = .\Allocate-IP.ps1
if ($hostIP -ne $null)
{
    $hostIPv6 = .\Convert-IPAddress.ps1 $hostIP
    .\Create-InternalNetwork.ps1 $protocolName $hostIP "255.255.255.0" "192.168.0.201" $hostIPv6 "2008::c9"
    $internalNetwork = $protocolName
}
else
{
    $internalNetwork = "Internal"
}

#----------------------------------------------------------------------------
# Define test entry files which will be run in VM
#----------------------------------------------------------------------------
[CHANGE FOLLOWING WHILE IT REQUIRES MORE THAN TWO MACHINES]
Write-Host "[PROTOCOL NAME] is executing on $serverVMName and $clientVMName ..." -foregroundcolor Yellow
$serverConfigScript = $testDirInVM + "\Scripts\serverConfigEntry.bat"
$clientConfigScript = $testDirInVM + "\Scripts\clientConfigEntry.bat"
$runTestSuiteScript = $testDirInVM + "\Scripts\runTestSuiteEntry.bat"

#----------------------------------------------------------------------------
# Create child VHD and VM
#----------------------------------------------------------------------------
Write-Host "Create child VHD and VM..."  -foregroundcolor Yellow
if ($workgroupDomain -eq "Domain")
{
    Write-Host "Create child VHD and VM: $DCVMName ..." 
    .\Create-VHD.ps1 "$workingDir\VM\Virtual Hard Disks\$protocolName-$DCVMName\$DCVMName.vhd" Different 127GB "$workingDir\VMLib\$DCVMName\Virtual Hard Disks\$DCVMName.vhd"
    .\Create-VM.ps1 $protocolName-$DCVMName "$workingDir\VM\Virtual Hard Disks\$protocolName-$DCVMName\$DCVMName.vhd" 900MB $internalNetwork
    $DCVMName = "$protocolName-$DCVMName"
}
.\Create-VHD.ps1 "$workingDir\VM\Virtual Hard Disks\$protocolName-$serverVMName\$serverVMName.vhd" Different 127GB "$workingDir\VMLib\$serverVMName\Virtual Hard Disks\$serverVMName.vhd"
.\Create-VM.ps1 $protocolName-$serverVMName "$workingDir\VM\Virtual Hard Disks\$protocolName-$serverVMName\$serverVMName.vhd" 900MB $internalNetwork

.\Create-VHD.ps1 "$workingDir\VM\Virtual Hard Disks\$protocolName-$clientVMName\$clientVMName.vhd" Different 127GB "$workingDir\VMLib\$clientVMName\Virtual Hard Disks\$clientVMName.vhd"
.\Create-VM.ps1 $protocolName-$clientVMName "$workingDir\VM\Virtual Hard Disks\$protocolName-$clientVMName\$clientVMName.vhd" 900MB $internalNetwork

$serverVMName = "$protocolName-$serverVMName"
$clientVMName = "$protocolName-$clientVMName"

#----------------------------------------------------------------------------
# Run VMs
#----------------------------------------------------------------------------
[START RUN VM AND WAIT FOR IT IS READY TO USE]
Write-Host "Start to run VMs..."  -foregroundcolor Yellow
if ($workgroupDomain -eq "Domain")
{
    Write-Host "Start to run VM: $DCVMName ..." 
    .\Run-VM.ps1 $DCVMName 
}
Write-Host "Start to run VM: $serverVMName ..." 
.\Run-VM.ps1 $serverVMName 
Write-Host "Start to run VM: $clientVMName ..." 
.\Run-VM.ps1 $clientVMName 

Write-Host "Waiting for all machines starting up ..." 
if ($workgroupDomain -eq "Domain")
{
    .\WaitFor-ComputerReady.ps1 $DCComputerName 
}
.\WaitFor-ComputerReady.ps1 $serverComputerName 
.\WaitFor-ComputerReady.ps1 $clientComputerName 

#----------------------------------------------------------------------------
# Set VM computer name
#----------------------------------------------------------------------------
if ($workgroupDomain -eq "Domain")
{
	$DCComputerName = .\Set-ComputerName.ps1 $protocolName-D1 $DCComputerName $userNameInVM $userPwdInVM
}
$serverComputerName = .\Set-ComputerName.ps1 $protocolName-S1 $serverComputerName $userNameInVM $userPwdInVM
$clientComputerName = .\Set-ComputerName.ps1 $protocolName-E1 $clientComputerName $userNameInVM $userPwdInVM

#----------------------------------------------------------------------------
# Set VM computer IP
#----------------------------------------------------------------------------
$ServerIP=.\Allocate-IP.ps1
$ClientIP=.\Allocate-IP.ps1
if($ServerIP -ne $null -and $ClientIP -ne $null)
{
    Write-Host "allocated IP: $ServerIP $ClientIP" -ForegroundColor Green
    .\Set-RemoteIP.ps1 $ServerIP $serverComputerName $userNameInVM $userPwdInVM "Local Area Connection"
    .\Set-RemoteIP.ps1 $ClientIP $clientComputerName $userNameInVM $userPwdInVM "Local Area Connection"
    if ($workgroupDomain -eq "Domain")
    {
        $DCIP=.\Allocate-IP.ps1
        if ($DCIP -ne $null)
        {
            Write-Host "allocated IP: $DCIP" -ForegroundColor Green
            .\Set-RemoteIP.ps1 $DCIP $DCComputerName $userNameInVM $userPwdInVM "Local Area Connection"
            .\Set-RemoteDNS.ps1 $DCIP $serverComputerName $userNameInVM $userPwdInVM "Local Area Connection"
            .\Set-RemoteDNS.ps1 $DCIP $clientComputerName $userNameInVM $userPwdInVM "Local Area Connection"
        }
    }
}


#----------------------------------------------------------------------------
# Release resource lock
#----------------------------------------------------------------------------
.\Release-Lock.ps1

#----------------------------------------------------------------------------
# Get remote computer's system drive share when they are starting up here
#----------------------------------------------------------------------------
$serverSystemDrive = .\Get-RemoteSystemDrive.ps1 $serverComputerName
$clientSystemDrive = .\Get-RemoteSystemDrive.ps1 $clientComputerName 

#----------------------------------------------------------------------------
# Define test folders in VM
#----------------------------------------------------------------------------
$testDirInServerVM = $testDirInVM.Replace("SYSTEMDRIVE", $serverSystemDrive )
$testDirInClientVM = $testDirInVM.Replace("SYSTEMDRIVE", $clientSystemDrive )

Write-Host "Test dir on Server VM is $testDirInServerVM"
Write-Host "Test dir on Client VM is $testDirInClientVM"


#----------------------------------------------------------------------------
# Config AutoLogon For VMs
#----------------------------------------------------------------------------
[CONFIG AUTOLOGON FOR VM]
Write-Host "Start to set autologon for VMs..."  -foregroundcolor Yellow
Write-Host "Start to set autologon for $serverComputerName ..." 
.\Config-AutoLogon.ps1 $serverComputerName
Write-Host "Start to set autologon for $clientComputerName ..." 
.\Config-AutoLogon.ps1 $clientComputerName

Write-Host "Waiting for all machines starting up ..." 
.\WaitFor-ComputerReady.ps1 $serverComputerName 
.\WaitFor-ComputerReady.ps1 $clientComputerName 

#----------------------------------------------------------------------------
# Copy test contents to VMs
#----------------------------------------------------------------------------
[COPY TEST FILES TO VM]
Write-Host "Start to copy test contents to VMs..."  -foregroundcolor Yellow
Write-Host "Copy test contents to $serverComputerName ..." 
.\Copy-TestFile.ps1 $srcScriptLibPathOnHost $srcScriptPathOnHost $srcToolPathOnHost $srcTestSuitePathOnHost $serverComputerName $testDirInVM $null $null $srcMyToolPathOnHost $srcDataPathOnHost $srcSnapshotPathOnHost
Write-Host "Copy test contents to $clientComputerName ..." 
.\Copy-TestFile.ps1 $srcScriptLibPathOnHost $srcScriptPathOnHost $srcToolPathOnHost $srcTestSuitePathOnHost $clientComputerName $testDirInVM $null $null $srcMyToolPathOnHost $srcDataPathOnHost $srcSnapshotPathOnHost

#----------------------------------------------------------------------------
# Kickoff test case configurations on VM client(s)
#----------------------------------------------------------------------------
[INVOKE THE ENTRY SCRIPT ON VM AND START TO CONFIG VM]
Write-Host "Start to kickoff test case configurations on VM client..."  -foregroundcolor Yellow
Write-Host "Kickoff test case configurations on $serverComputerName ..." 
.\RemoteExecute-Command.ps1 $serverComputerName "$serverConfigScript $testDirInVM $clientOS $serverCPUArchitecture $IPVersion $workgroupDomain $cluster $userNameInVM $userPwdInVM $domainInVM"
Write-Host "Kickoff test case configurations on $clientComputerName ..." 
.\RemoteExecute-Command.ps1 $clientComputerName "$clientConfigScript $testDirInVM $serverOS $clientCPUArchitecture $IPVersion $workgroupDomain $cluster $userNameInVM $userPwdInVM $domainInVM"

#----------------------------------------------------------------------------
# Wait for configuration done
#----------------------------------------------------------------------------
[WAIT VM CONFIGURING DONE]
Write-Host "Waiting for all machines configuration done..." -foregroundcolor Yellow
.\WaitFor-ComputerReady.ps1 $serverComputerName $null $null "C$" "config.finished.signal" 3600
.\WaitFor-ComputerReady.ps1 $clientComputerName $null $null "C$" "config.finished.signal" 3600

#----------------------------------------------------------------------------
# Start to run test suite on VM client
#----------------------------------------------------------------------------
[START TO RUN TEST SUITE ON CLIENT VM]
[RUN TEST SUITE]
Write-Host "Start running Test Suite ..." -foregroundcolor Yellow
$filterForNetMonCap = [PROTOCOL NAME]
.\RemoteExecute-Command.ps1 $clientComputerName "$runTestSuiteScript $testDirInClientVM $clientCPUArchitecture $testResultDir $protocolName $filterForNetMonCap `"Server+Both`" `"Proxyserver+Client`""

#----------------------------------------------------------------------------
# Wait for test done
#----------------------------------------------------------------------------
[WAIT FOR TEST DONE]
Write-Host "Wait for testing done ..." -foregroundcolor Yellow
[CHANGE THE TIMEOUT TO A REASONALBE VALUE]
.\WaitForReady.ps1 $clientComputerName $null $null "C$" "test.finished.signal" 3600

#----------------------------------------------------------------------------
# Copy result to host from client VM 
#----------------------------------------------------------------------------
[COPY TEST RESULT TO LOCAL]
Write-Host "Copy test result from test VM to host machine ..." -foregroundcolor Yellow
.\Copy-TestResult $serverComputerName "$testDirInServerVM\TestResults" $testResultDir\ServerLog
.\Copy-TestResult $clientComputerName "$testDirInClientVM\TestResults" $testResultDir

Write-Host "[PROTOCOL NAME] execute completed (not verified)." -foregroundcolor Green

#----------------------------------------------------------------------------
# Remove VM
#----------------------------------------------------------------------------
Write-Host "Remove VMs..."  -foregroundcolor Yellow
if ($workgroupDomain -eq "Domain")
{
    Write-Host "Remove VM: $DCVMName ..." 
    .\Remove-VM.ps1 $DCVMName
}
Write-Host "Remove VM: $serverVMName ..."
.\Remove-VM.ps1 $serverVMName
Write-Host "Remove VM: $clientVMName ..." 
.\Remove-VM.ps1 $clientVMName

#----------------------------------------------------------------------------
# Delete VHD
#----------------------------------------------------------------------------
if ($workgroupDomain -eq "Domain")
{
    remove-item "$workingDir\VM\Virtual Hard Disks\$protocolName-$DCVMName" -recurse
}
remove-item "$workingDir\VM\Virtual Hard Disks\$protocolName-$serverVMName" -recurse
remove-item "$workingDir\VM\Virtual Hard Disks\$protocolName-$clientVMName" -recurse

#----------------------------------------------------------------------------
# Delete Internal Network
#----------------------------------------------------------------------------
if ($hostIP -ne $null)
{
    .\Delete-InternalNetwork.ps1 $protocolName
}

#----------------------------------------------------------------------------
# Release IP resource
#----------------------------------------------------------------------------
if ($hostIP -ne $null)
{
    if ($workgroupDomain -eq "Domain")
    {
        .\Release-IP.ps1 $DCIP
    }
    .\Release-IP.ps1 $ClientIP
    .\Release-IP.ps1 $ServerIP
    .\Release-IP.ps1 $hostIP
}

Write-Host "[PROTOCOL NAME] execute completed." -foregroundcolor Green

exit
