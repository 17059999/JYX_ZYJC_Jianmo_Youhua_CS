# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
[string]$protocolName          = "RDP",
[string]$WorkingDirOnHost      = "D:\WinBlueRegressionTest", 
[string]$TestResultDirOnHost   = "D:\WinBlueRegressionTest\TestResults\RDP",
[string]$EnvironmentName       = "RDP.xml",
[string]$SupportCompression    = "No",
[string]$BatchToRun            = "RunAllTestCases.ps1",
[string]$SelectedAdapter       = "",
[string]$FilterToRun           = "",
[string]$SupportRDPFile        = "No"
)

Import-Module .\Common\LocalLinuxFunctionLib.psm1

# Assign argument to variable
Write-Host "WorkingDirOnHost: $WorkingDirOnHost"
$WorkingDir             = $WorkingDirOnHost
$testResultDir          = $TestResultDirOnHost

Set-StrictMode -v 2
Write-Host "Selected Adapter: $SelectedAdapter..." 
Write-Host "SupportRDPFile in Execute-ProtocolTestToLinuxDriver.ps1: $SupportRDPFile..." 

#----------------------------------------------------------------------------
# Function: UpdatePTFConfigFileNode
# Usage   : Update PTF deployment config file node
#----------------------------------------------------------------------------
function UpdatePTFConfigFileNode
{
    Param(
    [string]$name,
    [string]$value)
    
    $signFileName = "Update-$name."+(Get-Date).ToString("MMddHHmmss")+".Finished.signal"
    $updatePTFConfigFileNodeShCommand = "pwsh /Temp/Modify-PTFConfigFileNode.ps1 RDP_ServerTestSuite.deployment.ptfconfig $name $value $signFileName $testSuiteFolder"
    Execute-PlinkShCommand -VmIP $driverComputerIP -ShCommand $updatePTFConfigFileNodeShCommand -ShCommandKey "UpdatePTFConfigFileNode"
	Start-Sleep 20
}

#----------------------------------------------------------------------------
# Function: UpdateCommonPTFConfigFileNode
# Usage   : Update PTF config file node
#----------------------------------------------------------------------------
function UpdateCommonPTFConfigFileNode
{
    Param(
    [string]$name,
    [string]$value)
	
	$signFileName = "Update-$name."+(Get-Date).ToString("MMddHHmmss")+".Finished.signal"
    $updateCommonPTFConfigFileNodeShCommand = "pwsh /Temp/Modify-PTFConfigFileNode.ps1 RDP_ServerTestSuite.ptfconfig $name $value $signFileName $testSuiteFolder"
    Execute-PlinkShCommand -VmIP $driverComputerIP -ShCommand $updateCommonPTFConfigFileNodeShCommand -ShCommandKey "UpdatePTFConfigFileNode"
    Start-Sleep 20
}

#----------------------------------------------------------------------------
# Function: SwitchSecurityProtocol
# Usage   : Specify security protocol for the following test run
#----------------------------------------------------------------------------
function SwitchSecurityProtocol
{
    Param(
    [string]$SecurityProtocol,
    [string]$negotiationBasedApproach)
    
	Write-Host "Switch SecurityProtocol to $SecurityProtocol, $negotiationBasedApproach" -foregroundcolor Yellow
	UpdatePTFConfigFileNode Protocol $SecurityProtocol
	UpdatePTFConfigFileNode Negotiation $negotiationBasedApproach
    
	if($SecurityProtocol -eq "RDP")
	{
	    UpdatePTFConfigFileNode Level Low
		UpdatePTFConfigFileNode Method 128bit
	}
	else
	{
		UpdatePTFConfigFileNode Level None
		UpdatePTFConfigFileNode Method None
	}
}
#----------------------------------------------------------------------------
# Function: RunTestCasesAndWait
# Usage   : Run test cases and wait
#----------------------------------------------------------------------------
function RunTestCasesAndWait
{
    Param(
    [string]$TestRunName)
	
	Write-Host (Get-Date).ToString() + "Start running Test Suite: $TestRunName" -foregroundcolor Yellow

    $executeTestSuiteCaseLogFile = "$testDirInDriverComputerVM/Run-MIPTestCases-LinuxDriver-$TestRunName.ps1.log"
    $runTestCaseInBackground = "nohup pwsh $testDirInDriverComputerVM/Run-MIPTestCases.ps1 $protocolName $ServerCPUArchitecture $RSPathInServerVM $RSinScope $RSoutOfScope $BatchToRun"
    $runTestCaseInBackground += " '$FilterToRun'"
    $runTestCaseInBackground += " Linux > $executeTestSuiteCaseLogFile"

    Write-Host "Wait for testing done ..." -foregroundcolor Yellow
    Execute-PlinkShCommand -VmIP $currentLinuxVMIP -ShCommand $runTestCaseInBackground -ShCommandKey "RunTestCaseOf$TestSuiteName"
    
    Write-Host "Rename the trx file..." -ForegroundColor Yellow
    $renameCommand = "\`$resultsPath = (Get-Content \`$env:SystemDrive/MSIInstalled.signal) + '/../TestResults'; Rename-Item (Get-ChildItem -Path \`$resultsPath -Include '*.trx' -Recurse | Sort-Object CreationTime -Descending | Select-Object -First 1).FullName '$TestRunName.trx'"
    Execute-PlinkShCommand -VmIP $currentLinuxVMIP -ShCommand "pwsh -c `"`"$renameCommand`"`"" -ShCommandKey "RenameTestResultsOf$TestSuiteName"
}

#----------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
Start-Transcript -Path "$testResultDir\Execute-ProtocolTestToLinuxDriver.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Define source folders in VM Host
#----------------------------------------------------------------------------
$srcScriptLibPathOnHost = $workingDir + "\ScriptLib\"
$srcToolPathOnHost      = $workingDir + "\Tools\"
$xmlPathOnHost          = $workingDir + "\ProtocolTestSuite\$protocolName\VSTORMLITEFiles\XML\"
$srcScriptPathOnHost    = $workingDir + "\ProtocolTestSuite\$protocolName\Scripts\"
$srcTestSuitePathOnHost = $workingDir + "\ProtocolTestSuite\$protocolName\Bin\"
$srcMyToolPathOnHost    = $workingDir + "\ProtocolTestSuite\$protocolName\MyTools\"
$srcDataPathOnHost      = $workingDir + "\ProtocolTestSuite\$protocolName\Data\"
$srcSnapshotPathOnHost  = $workingDir + "\ProtocolTestSuite\$protocolName\Snapshot\"
$srcMSIInstallOnHost    = $workingDir + "\ProtocolTestSuite\$protocolName\Deploy\"
Push-Location $srcScriptLibPathOnHost

#----------------------------------------------------------------------------
# Read VM settings from protocol.xml
#----------------------------------------------------------------------------
Write-Host "Read VM settings from $EnvironmentName"  -foregroundcolor Yellow
[XML]$vmConfig = Get-Content "$xmlPathOnHost\$EnvironmentName"
$currentCore = $vmConfig.lab.core
if(![string]::IsNullOrEmpty($currentCore.regressiontype) -and ($currentCore.regressiontype -eq "Azure")){
    $srcToolPathOnHost = $workingDir + "\ProtocolTestSuite\$protocolName\Tools\"
}

$sutSettings = $vmConfig.lab.servers.vm | where {$_.role -eq "SUT"}
$driverComputerSettings = $vmConfig.lab.servers.vm | where {$_.role -eq "DriverComputer"}

$driverComputerOS      = "Windows" #Dirver Computer
$sutOS             	   = "Windows" #SUT
$vmtype = $sutSettings.get_ChildNodes() | where {$_.Name -eq "vmtype"}
if ($vmtype -ne $null ) {
    if($sutSettings.vmtype -eq "Linux") {
	    $sutOS             = "NonWindows"  		 
	}
	elseif ($sutSettings.vmtype -eq "Windows") {
	    $sutOS             	   = "Windows"
	}
}
$WorkgroupDomain       = $driverComputerSettings.domain
$IPVersion             = "IPv4"
$ServerCPUArchitecture = "x64"
$ServerCPUArchitecture = "x64"

$UserNameInVM          = $vmConfig.lab.core.username
$userPwdInVM           = $vmConfig.lab.core.password
$DomainInVM            = "contoso.com"
$TestDirInLinuxVM      = "/Test"
$TestDirInWindowsVM    = "SYSTEMDRIVE\Test"

# Check if configure file have hypervname, For azure it does not have hypervname, so when hypervname exists its for On-Premier run otherwise it's for Azure regression run
if($sutSettings.SelectSingleNode("hypervname")){
    $sutVMName              = $sutSettings.hypervname
}else{
    $sutVMName              = $sutSettings.name
}
$sutComputerIP              = $sutSettings.ip
if(($sutSettings.ip | Measure-Object ).Count -gt 1){
    $sutComputerIP = $sutSettings.ip[0]
}
$sutComputerName            = $sutSettings.name

if($driverComputerSettings.SelectSingleNode("hypervname")){
    $driverComputerVMName   = $driverComputerSettings.hypervname
}else{
    $driverComputerVMName   = $driverComputerSettings.name
}
$driverComputerIP      = $driverComputerSettings.ip
if(($driverComputerSettings.ip | Measure-Object ).Count -gt 1){
    $driverComputerIP = $driverComputerSettings.ip[0]
}
$driverComputerName    = $driverComputerSettings.name
$testSuiteFolder = $driverComputerSettings.tools.TestsuiteZip.targetFolder

Write-Host "$protocolName is executing on $sutVMName and $driverComputerVMName..." -foregroundcolor Yellow

$agentPort = "4488"
if($sutSettings.SelectSingleNode("agentPort")) {
    $agentPort = $sutSettings.agentPort
}
$agentRemoteServer = "mstsc"
if($sutSettings.SelectSingleNode("agentRemoteServer")) {
    $agentRemoteServer = $sutSettings.agentRemoteServer
}

$RDPListeningPort = "3389"
if($driverComputerSettings.SelectSingleNode("RDPListeningPort")) {
    $RDPListeningPort = $driverComputerSettings.RDPListeningPort
}
#----------------------------------------------------------------------------
# Define test suite endpoint, should be "Server" or "Server"
#----------------------------------------------------------------------------
$endPoint             = "Server"
$global:testResultDir = $testResultDir

#----------------------------------------------------------------------------
# Get remote computer's system drive share when they are starting up here
#----------------------------------------------------------------------------
if ($sutOS -ne "NonWindows")
{
	$sutSystemDrive            = .\Get-RemoteSystemDrive.ps1 $sutComputerIP "$sutComputerIP\$userNameInVM" "$userPwdInVM"
	$testDirInSutVM            = $testDirInWindowsVM.Replace("SYSTEMDRIVE", $sutSystemDrive )
	Write-Host "Test dir on $sutComputerIP VM is $testDirInSutVM"
	$ServerRemoteSystemDrive = $sutSystemDrive.Replace(":", "$")
}

$testDirInDriverComputerVM = $testDirInLinuxVM

#----------------------------------------------------------------------------
# Ensure VMs have install tools and test suite
#----------------------------------------------------------------------------
Write-Host "Waiting for MSIInstalled.signal (ensure VMs have install tools and test suite) ..." 
if ($sutOS -ne "NonWindows")
{
    .\WaitFor-ComputerReady.ps1 $sutComputerIP "$sutComputerIP\$userNameInVM" "$userPwdInVM" $sutSystemDrive MSIInstalled.signal 600
}

#----------------------------------------------------------------------------
# Get Timestamp of start configuration
#----------------------------------------------------------------------------
.\Get-Timestamp.ps1 $protocolName initial $testResultDir
.\Get-Timestamp.ps1 $protocolName StartconfigRDP $testResultDir

#----------------------------------------------------------------------------
# Copy test files.
#----------------------------------------------------------------------------
if ($sutOS -ne "NonWindows")
{
	Write-Host "Copy test contents to $sutComputerIP ..." 
	.\Copy-TestFile.ps1 $srcScriptLibPathOnHost $srcScriptPathOnHost $srcToolPathOnHost $srcTestSuitePathOnHost $sutComputerIP $testDirInSutVM $sutComputerIP\$userNameInVM $userPwdInVM $srcMyToolPathOnHost $srcDataPathOnHost $srcSnapshotPathOnHost
}

Write-Host "Copy test contents to $driverComputerIP ..." 

# Create "/Test" folder
Write-Host "Create '/Test' folder"
Execute-PlinkShCommand -VmIP $driverComputerIP -ShCommand "mkdir -p $testDirInDriverComputerVM" -ShCommandKey "mkdir_createTestFolder"
Start-Sleep 10

Write-Host "Copy $srcScriptLibPathOnHost to Linux / "
Execute-PscpCopyWindowsFolderToLinuxCommand -VmIP $driverComputerIP -SourceFilePath $srcScriptLibPathOnHost -DestinationFilePath $testDirInDriverComputerVM
Start-Sleep 300

#----------------------------------------------------------------------------
# Set user name after domain joined
#----------------------------------------------------------------------------
$ServerUserFullName = "$sutComputerIP\$userNameInVM"
$serverUserFullName = "$driverComputerIP\$userNameInVM"
if ($workgroupDomain -eq "Domain")
{
    $ServerUserFullName = "$DomainInVM\$userNameInVM"
    $serverUserFullName = "$DomainInVM\$userNameInVM"
}
#----------------------------------------------------------------------------
# Replace RDP_ServerTestSuite.ptfconfig.
#----------------------------------------------------------------------------
if($SelectedAdapter -ne "") {
    Write-Host "Replace RDP_ServerTestSuite.ptfconfig on $driverComputerIP ..."     
    $TargetFolderOnVM = $driverComputerSettings.tools.TestsuiteZip.targetFolder
    $testBinInServer = "$TargetFolderOnVM/Bin"
    $PTFConfigFilePathOnVM = "$testBinInServer/RDP_ServerTestSuite.ptfconfig"

    Write-Host "Enable managed Adapter in $PTFConfigFilePathOnVM ..." 
    $changeFileContentCommandPS = "/Temp/Modify-PTFConfigFileAdapter.ps1"
    $changeFileContentShCommand = "pwsh $changeFileContentCommandPS -PTFconfigFilePathOnVM $PTFConfigFilePathOnVM"
    Execute-PlinkShCommand -VmIP $driverComputerIP -ShCommand $changeFileContentShCommand -ShCommandKey "ChangePTFFileContent"

    Start-Sleep 20

    Write-Host "ServerSupportRDPFile false" -foregroundcolor Yellow
    UpdateCommonPTFConfigFileNode ServerSupportRDPFile false
}

#----------------------------------------------------------------------------
# Set configuration for ParamConfig.xml before copying test contents to VMs for test environments
#----------------------------------------------------------------------------
Write-Host "update ParamConfig.xml on VMs ..."

# Detect the RDP version of SUT
$RDPVersion = .\Get-RDPVersionFromWindowsSUT.ps1 -ComputerName $sutComputerIP -UserName $ServerUserFullName -UserPassword $userPwdInVM

# SUT cannot ping Linux driver machine name, so we use IP address.
$driverComputerNameInXML = $driverComputerIP

$paramList = @{workgroupDomain = "$workgroupDomain"; userNameInTC = "$userNameInVM"; 
userPwdInTC = "$userPwdInVM"; domainName = "$DomainInVM"; tcComputerName = "$sutComputerIP"; 
driverComputerName = "$driverComputerNameInXML"; ipVersion = "$IPVersion"; osVersion = "$sutOS"; 
CredSSPUser = "$userNameInVM"; CredSSPPwd = "$userPwdInVM"; RDPVersion = "$RDPVersion"; RDPListeningPort = "$RDPListeningPort";
compressionInTC = "$SupportCompression"; agentPort = "$agentPort"; agentRemoteServer=$agentRemoteServer }

foreach ($param in $paramList.Keys)
{
    $value = $paramList.Item($param)
    $configSignal = "ConfigParam"+"$param"+"Finished.signal"
	if ($sutOS -ne "NonWindows")
    {
        .\RemoteExecute-Command.ps1 $sutComputerIP "cmd /c PowerShell $testDirInSutVM\Scripts\Config-MIPParamConfigFile.ps1 $param $value" $ServerUserFullName $userPwdInVM
        .\WaitFor-ComputerReady.ps1 $sutComputerIP "$ServerUserFullName" "$userPwdInVM" $sutSystemDrive $configSignal 600
    }   

    $configureParameterCommand = "pwsh $testDirInDriverComputerVM/Config-MIPParamConfigFile.ps1 $param $value"
    Execute-PlinkShCommand -VmIP $driverComputerIP -ShCommand $configureParameterCommand -ShCommandKey "ConfigureParameterCommand"

    Start-Sleep 20
}

#----------------------------------------------------------------------------
# Kickoff test case configurations on VM Server(s)
#----------------------------------------------------------------------------
if ($sutOS -ne "NonWindows")
{
    $configTCPS = "Config-TerminalServer.ps1"
    if($SelectedAdapter -ne "") 
    {
        $configTCPS = "Config-TerminalServerForRemoteAdapt.ps1"
    }
    $runConfigCmd = "powershell $testDirInSutVM\Scripts\Start-MIPConfiguration.ps1 $testDirInSutVM $configTCPS -scriptsPath $testDirInSutVM\Scripts"
    $createConfigTaskCmd = "cmd /c schtasks /Create /RU Administrators /SC Weekly /TN ConfigTC /TR `"$runConfigCmd`" /IT /F" 

    Write-Host "Kickoff test case configurations on $sutComputerIP with $configTCPS..." 
    .\RemoteExecute-Command.ps1 $sutComputerIP "$createConfigTaskCmd" $ServerUserFullName  $userPwdInVM
    Start-Sleep 60
    Write-Host "cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN ConfigTC"
    cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN ConfigTC

    Write-Host "Waiting for $sutComputerIP configuration done..." -foregroundcolor Yellow
    .\WaitFor-ComputerReady.ps1 $sutComputerIP "$ServerUserFullName" "$userPwdInVM" $sutSystemDrive "configTC.finished.signal" 3600 
}

Write-Host "Kickoff test case configurations on $driverComputerIP ..." 

$kickoffConfigurationsCommand = "pwsh $testDirInDriverComputerVM/Start-MIPConfiguration.ps1 $testDirInDriverComputerVM Config-LinuxDriverComputer.ps1 Linux"
Execute-PlinkShCommand -VmIP $driverComputerIP -ShCommand $kickoffConfigurationsCommand -ShCommandKey "KickoffConfigurationsCommand"

Write-Host "Waiting for all Server configuration done..." -foregroundcolor Yellow
Start-Sleep 300

#----------------------------------------------------------------------------
# Get Timestamp of start run test suite
#----------------------------------------------------------------------------
.\Get-Timestamp.ps1 $protocolName startruntestRDP $testResultDir

#----------------------------------------------------------------------------
# Prepare to run test suite on VM Server
#----------------------------------------------------------------------------
$RSPathInServerVM = "$testSuiteFolder/Bin"
$RSinScope        = "Server+Both"
$RSoutOfScope     = "Server"
$taskName = "RunTestCases"

# Wait for driver computer and sut to be ready for testing
Write-Host "Wait for SUT $sutComputerIP to be ready for testing..." -foregroundcolor Yellow
$triedCount = 0
$sutStatus = $null
Write-Host ""
while($triedCount -lt 300 -and $sutStatus -eq $null)
{
    $triedCount++
    if( ($triedCount % 10) -eq 0)
    {
        Write-Host "." -NoNewline
    }
    $sutStatus = Test-WSMan $sutComputerIP -ErrorAction Ignore
}

if($SelectedAdapter -ne "") {
    Write-Host "Start SUT Control Adapter..." -foregroundcolor Yellow
    $oneAgent = $SelectedAdapter;
    if($SelectedAdapter.Contains(" ")) { $oneAgent = $SelectedAdapter.Split(" ")[0]; }
    switch($oneAgent.ToLower()) {
        "csharp" {cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN CSharpAgent}
        "java" {cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN JavaAgent}
        "python" {cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN PythonAgent}
        "c" {cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN CAgent}    
        default {Write-Host "Unknown SUT Control Adapter! Cannot Start!" -foregroundcolor Red}
    }
}

#----------------------------------------------------------------------------
# Start test run
#----------------------------------------------------------------------------

# SUT Control Adapter $FilterToRun.
if($SelectedAdapter -ne "") {
    # Use mstsc command instead of rdp file.
    if($agentRemoteServer -eq "mstsc" -and ($SupportRDPFile -eq "No"))
    {
        if($FilterToRun -ne "") 
        {
            $FilterToRun +="&"
        }

        # The mstsc command without rdp file does not support RDSTLS with redirection setting.
        $FilterToRun +="Name!=BVT_RDSTLSAuthentication_PositiveTest_ServerRedirectionWithPasswordCredentials"
        $FilterToRun +="&Name!=S11_RDSTLSAuthentication_PositiveTest_ServerRedirectionAndAutoReconnectWithCookie"
    }
}

if($SelectedAdapter -eq "") {
    SwitchSecurityProtocol RDP True
    RunTestCasesAndWait RDP

    SwitchSecurityProtocol TLS True
    RunTestCasesAndWait TLS

    if($SupportRDPFile -eq "Yes")
    {
        SwitchSecurityProtocol CredSSP True
        RunTestCasesAndWait CredSSP

        SwitchSecurityProtocol CredSSP False
        RunTestCasesAndWait DirectCredSSP
    }
} else {
    if($SelectedAdapter.Contains(" ")) {
        $Adapts = $SelectedAdapter.Split(" ")
    }
    else {
        $Adapts = @($SelectedAdapter)
    }
    $Adapts | ForEach-Object {
        $currAdapt = $_.ToLower()
        cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN DisconnectAllAgents

        if($currAdapt -eq "csharp") {
            cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN CSharpAgent
        }    

        if($currAdapt -eq "java") {
            cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN JavaAgent
        }
        
        if($currAdapt -eq "python") {
            cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN PythonAgent
        }

        if($currAdapt -eq "c") {   
            cmd /c schtasks /S $sutComputerIP /U $ServerUserFullName  /P $userPwdInVM /Run /TN CAgent
        }

        Write-Host "Run Cases with the SUT Control Adapter($_)..." -foregroundcolor Yellow
        SwitchSecurityProtocol RDP True
        RunTestCasesAndWait $_-RDP
        
        SwitchSecurityProtocol TLS True
        RunTestCasesAndWait $_-TLS

        if($SupportRDPFile -eq "Yes")
        {
            SwitchSecurityProtocol CredSSP True
            RunTestCasesAndWait $_-CredSSP

            SwitchSecurityProtocol CredSSP False
            RunTestCasesAndWait $_-DirectCredSSP
        }
    }
}

#----------------------------------------------------------------------------
# Get Timestamp of test done
#----------------------------------------------------------------------------
.\Get-Timestamp.ps1 $protocolName testdoneRDP $testResultDir

#----------------------------------------------------------------------------
# Copy result to host from VM 
#----------------------------------------------------------------------------
Write-Host "Copy test results and logs from SUT VM to host machine ..." -foregroundcolor Yellow
.\Copy-TestResult $sutComputerIP "$testDirInSutVM\TestResults" "$testResultDir\ServerLog" $ServerUserFullName $userPwdInVM
.\Copy-TestResult $sutComputerIP "$sutSystemDrive\*.signal" "$testResultDir\ServerLog" $ServerUserFullName $userPwdInVM
.\Copy-TestResult $sutComputerIP "$sutSystemDrive\*.log" "$testResultDir\ServerLog" $ServerUserFullName $userPwdInVM

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Stop-Transcript
# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Execute-ProtocolTestToLinuxDriver.ps1"
}

Write-Host "$protocolName execute completed." -foregroundcolor Green
exit 0
