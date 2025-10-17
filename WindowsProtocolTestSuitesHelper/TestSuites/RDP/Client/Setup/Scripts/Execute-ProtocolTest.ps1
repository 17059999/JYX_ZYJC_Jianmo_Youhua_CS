# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
[string]$ProtocolName          = "RDPClient",
[string]$WorkingDirOnHost      = "D:\WinBlueRegressionTest", 
[string]$TestResultDirOnHost   = "D:\WinBlueRegressionTest\TestResults\RDP",
[string]$EnvironmentName       = "RDP.xml",
[string]$RunCaseScript         = "Execute-ProtocolTestOnDriver.ps1",
[string]$TestCaseTimeout       = "28800",
[string]$SupportCompression    = "No",
[string]$BatchToRun            = "RunAllTestCases.ps1",
[string]$SelectedAdapter      = "",
[string]$FilterToRun           = "",
[string]$SupportRDPFile        = "No",
[string]$runTests              = "true"
)

# Assign argument to variable
$WorkingDir             = $WorkingDirOnHost
$testResultDir          = $TestResultDirOnHost

Set-StrictMode -v 2
Write-Host "Selected Adapter: $SelectedAdapter..." 
Write-Host "SupportRDPFile in Execute-ProtocolTest.ps1: $SupportRDPFile..." 

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$global:testResultDir 	= $TestResultDirOnHost
$CurrentScriptName 		= $MyInvocation.MyCommand.Name
$xmlPathOnHost          = $WorkingDirOnHost + "\ProtocolTestSuite\$protocolName\VSTORMLITEFiles\XML\"
[string]$ConfigFile		= "$xmlPathOnHost\$EnvironmentName"

[xml]$vmConfig = Get-Content $ConfigFile
$driverSettings = $vmConfig.lab.servers.vm | where {$_.role -eq "DriverComputer"}
$sutSettings = $vmConfig.lab.servers.vm | where {$_.role -eq "SUT"}

$driverOS               = "Windows" #Dirver Computer	
$driverOStype = $driverSettings.get_ChildNodes() | where {$_.Name -eq "os"}	
if ($driverOStype -ne $null ) {	
    if($driverSettings.os -eq "Linux") {	
	    $driverOS             = "NonWindows"  		 	
	}	
	elseif ($driverSettings.os -eq "Windows") {	
	    $driverOS             	   = "Windows"	
	}	
}
$sutOS             	   = "Windows" #SUT
$sutOStype = $sutSettings.get_ChildNodes() | where {$_.Name -eq "os"}	
if ($sutOStype -ne $null ) {	
    if($sutSettings.os -eq "Linux") {	
	    $sutOS             = "NonWindows"  		 	
	}	
	elseif ($sutSettings.os -eq "Windows") {	
	    $sutOS             	   = "Windows"	
	}	
}

[string]$driverComputerIP      = $driverSettings.ip
[string]$driverComputerName    = $driverSettings.name
[string]$sutComputerIP         = $sutSettings.ip
[string]$sutComputerName       = $sutSettings.name
[string]$userNameInVM          = $vmConfig.lab.core.username
[string]$userPwdInVM           = $vmConfig.lab.core.password
if($vmConfig.lab.core.timeoutinminutes -ne $null)
{
    [string]$TestCaseTimeout	   = [int]$vmConfig.lab.core.timeoutinminutes * 60
}


#----------------------------------------------------------------------------
# Utility ScriptBlock Declaration
#----------------------------------------------------------------------------
# A quick utility to call Get-Timestamp.ps1
Function SetTimestamp
{
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateSet("initial","startconfig","startruntest","testdone")]
        [string]$State
    )

    if (($State -eq "startconfig") -or ($State -eq "startruntest"))
    {
        $ExecState = $State + $ProtocolName
    }
    else
    {
        $ExecState = $State
    }

    .\Get-Timestamp.ps1 $ProtocolName $ExecState $TestResultDirOnHost
}
# Execute SSH Command
Function ExecuteSSHCommand {
    Param([string]$fullUserName,[string]$sshCommand)

    Write-TestSuiteInfo "ssh $fullUserName $sshCommand"
    ssh $fullUserName $sshCommand > $null
}
# Find specified file or directory from the MicrosoftProtocolTests folder on 
# the computer. The folder will be created after the test suite MSI is installed.
[ScriptBlock]$GetItemInTestSuite = {
    Param([string]$Name,[string]$targetFolderOnVM)

    # Try if the name specified is a directory
    [string]$Path = [System.IO.Directory]::GetDirectories("$targetFolderOnVM",`
                    $Name,[System.IO.SearchOption]::AllDirectories)
    
    if(($Path -eq $null) -or ($Path -eq ""))
    {
        # Try if the name specified is a file
        [string]$Path = [System.IO.Directory]::GetFiles("$targetFolderOnVM",`
                        $Name,[System.IO.SearchOption]::AllDirectories)
    }

    return $Path
}

[ScriptBlock]$GetItemInTemp = {
    Param([string]$Name)

    # Try if the name specified is a directory
    [string]$Path = [System.IO.Directory]::GetDirectories("$env:HOMEDRIVE\Temp",`
                   $Name,[System.IO.SearchOption]::AllDirectories)
 
    if(($Path -eq $null) -or ($Path -eq ""))
    {
        # Try if the name specified is a file
        [string]$Path = [System.IO.Directory]::GetFiles("$env:HOMEDRIVE\Temp",`
                        $Name,[System.IO.SearchOption]::AllDirectories)
    }

    return $Path
}

# Create a windows task and run it.
# To do this is because if the commands executed remotely do not have
# full permissions to access all system resources. But a program run by
# the task scheduler has local previleges.
# Notice that the task creater must be the administrator, or the task
# will not be started.
[ScriptBlock]$CreateTaskAndRun = {
    Param([string]$FilePath,[string]$TaskName, [string]$Params)

    # Push to the parent folder folder first, then run
    $ParentDir = [System.IO.Directory]::GetParent($FilePath)
    $Command = "{Push-Location $ParentDir; Invoke-Expression $FilePath}"
    # Guarantee commands run in powershell environment
    $Task = "PowerShell PowerShell -Command $Command"
    if($Params -ne $null) {
        $Task = "PowerShell $FilePath $Params"
    }
    # Create task
    cmd /c schtasks /Create /RU Administrators /SC ONCE /ST 00:00 /TN $TaskName /TR $Task /IT /F
    Sleep 10
    # Run task
    cmd /c schtasks /Run /TN $TaskName  
}

function CheckSignalAndRetryRunTask
{
    Param(
    [string]$computerIP,
    [string]$userNameInVM,
    [string]$userPwdInVM,
    [string]$taskName,
    [string]$signalFolder,
    [string]$signalFileName)

    [string]$fullUserName   = $computerIP + "\" + $userNameInVM   
    Write-Host "Waiting for $computerIP $taskName done..." -foregroundcolor Yellow
    # Execute the task 
    $exeTask = "cmd /c schtasks /Run /TN $taskName"
    # Execute task with limited retry times
    for($i=0;$i -lt 5;$i++) 
    {
        try
        {
            .\WaitFor-ComputerReady.ps1 $computerIP $fullUserName $userPwdInVM $signalFolder $signalFileName 20
        }
        catch
        {
            Write-Host "$exeTask"
            .\RemoteExecute-Command.ps1 $computerIP "$exeTask" $fullUserName "$userPwdInVM"    
            continue
        }
        if($lastexitcode -eq 0)
        {
            break
        }            
    }
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
	.\RemoteExecute-Command.ps1 $driverComputerIP "cmd /c PowerShell c:\Temp\Scripts\Modify-PTFConfigFileNode.ps1 RDP_ClientTestSuite.ptfconfig $name $value $signFileName $targetFolderOnVM" $driverFullUserName $userPwdInVM	
	.\WaitFor-ComputerReady.ps1 $driverComputerIP "$driverFullUserName" "$userPwdInVM" $driverComputerSystemDrive $signFileName 600
}
function UpdatePtfconfig
{
    #----------------------------------------------------------------------------
    # Replace RDP_ClientTestSuite.ptfconfig.
    #----------------------------------------------------------------------------
    Write-Host "Replace RDP_ClientTestSuite.ptfconfig on $driverComputerIP ..."     
    $testBinInServer = "$targetFolderOnVM/Bin"
    $PTFConfigFilePathOnVM = "$testBinInServer/RDP_ClientTestSuite.ptfconfig"
    .\RemoteExecute-Command.ps1 $driverComputerIP "cmd /c PowerShell c:\Temp\Scripts\Modify-PTFConfigFileAdapter.ps1 -PTFconfigFilePathOnVM $PTFConfigFilePathOnVM" $driverFullUserName $userPwdInVM	
    
    if($SupportRDPFile -eq "Yes"){
        Write-Host "ClientSupportRDPFile true" -foregroundcolor Yellow
        UpdateCommonPTFConfigFileNode ClientSupportRDPFile true
    }else{
        Write-Host "ClientSupportRDPFile false" -foregroundcolor Yellow
        UpdateCommonPTFConfigFileNode ClientSupportRDPFile false
    }
}
#----------------------------------------------------------------------------
# Procedure Functions
#----------------------------------------------------------------------------
Function Prepare
{
    Write-Host "Start Executing [$CurrentScriptName] ... " -ForegroundColor Cyan

    # Enter call stack
    if($function:EnterCallStack -ne $null)
    {
	    EnterCallStack "Execute-ProtocolTest.ps1"
    }

    # Check test result directory
    if(!(Test-Path -Path $TestResultDirOnHost))
    {
        md $TestResultDirOnHost
    }

    # Start logging
    Start-Transcript -Path "$TestResultDirOnHost\Execute-ProtocolTest.ps1.log" -Append -Force -ErrorAction SilentlyContinue

    # Push location to the ScriptLib folder
    # Because scripts in lib will be called
    $ScriptLibPathOnHost = "$WorkingDirOnHost\ScriptLib"
    Push-Location $ScriptLibPathOnHost
}
Function ConfigSUT{
    $targetFolderOnVM = $sutSettings.tools.TestsuiteZip.targetFolder
    # Config SUT
    if($sutOS -eq "NonWindows") {
        [string]$sutFullUserName   = $userNameInVM + "@" + $sutComputerIP
        
        Write-Host "Running config script. Please wait..." -ForegroundColor Yellow
        ExecuteSSHCommand $sutFullUserName "pwsh /home/$userNameInVM/Temp/Scripts/UpdateParamConfig.ps1 $SupportCompression $sutComputerName"
        Write-Host "Config SUT Done" -foregroundcolor Yellow
    } else {
        [string]$sutFullUserName   = $sutComputerIP + "\" + $userNameInVM     
       
        # Build session. Prepare to execute script on driver computer.
        Write-Host "Trying to connect to computer $sutComputerIP" -ForegroundColor Yellow
        $sutSession = .\Get-RemoteSession.ps1 -FullUserName $sutFullUserName -UserPassword $userPwdInVM -RemoteIP $sutComputerIP

        $sutSystemDrive = .\Get-RemoteSystemDrive.ps1 $sutComputerIP "$sutFullUserName" "$userPwdInVM"
        Write-Host "Trying to get script path on SUT VM" -ForegroundColor Yellow
        $UpdateParamConfigScriptPathOnSUTVM = Invoke-Command -Session $sutSession -ScriptBlock $GetItemInTemp -ArgumentList "UpdateParamConfig.ps1"
        Write-Host $UpdateParamConfigScriptPathOnSUTVM
        
        Write-Host "Running config script. Please wait..." -ForegroundColor Yellow
        Invoke-Command -Session $sutSession -ScriptBlock $CreateTaskAndRun `
            -ArgumentList $UpdateParamConfigScriptPathOnSUTVM, "UpdateParamConfig", "-SupportCompression $SupportCompression -ComputerName $sutComputerName"

        CheckSignalAndRetryRunTask $sutComputerIP $userNameInVM $userPwdInVM "UpdateParamConfig" $sutSystemDrive "updateParamConfig.finished.signal"
        Remove-PSSession $sutSession

        # Build session. Prepare to execute script on driver computer.
        Write-Host "Trying to connect to computer $sutComputerIP" -ForegroundColor Yellow
        $sutSession = .\Get-RemoteSession.ps1 -FullUserName $sutFullUserName -UserPassword $userPwdInVM -RemoteIP $sutComputerIP

        $configTCPS = "Config-TerminalClient.ps1"
        if($SelectedAdapter -ne "") 
        {
            $configTCPS = "Config-TerminalClientForRemoteAdapt.ps1"
        }

        Write-Host "Trying to get script path on SUT VM" -ForegroundColor Yellow
        $ConfigScriptPathOnDriverVM = Invoke-Command -Session $sutSession -ScriptBlock $GetItemInTestSuite -ArgumentList $configTCPS,"$targetFolderOnVM\Scripts"
        Write-Host $ConfigScriptPathOnDriverVM
        
        Write-Host "Running configurations script on $sutComputerIP with $configTCPS, Please wait..." -ForegroundColor Yellow
        Invoke-Command -Session $sutSession -ScriptBlock $CreateTaskAndRun `
            -ArgumentList $ConfigScriptPathOnDriverVM, "RunConfig"  

        CheckSignalAndRetryRunTask $sutComputerIP $userNameInVM $userPwdInVM "RunConfig" $sutSystemDrive "configTC.finished.signal"
        Remove-PSSession $sutSession

       
        if($SelectedAdapter -ne "") { 
            cmd /c schtasks /S $sutComputerIP /U $userNameInVM  /P $userPwdInVM /Run /TN DisconnectAllAgents

            if($SelectedAdapter -eq "csharp") {
                cmd /c schtasks /S $sutComputerIP /U $userNameInVM  /P $userPwdInVM /Run /TN CSharpAgent
            }    

            if($SelectedAdapter -eq "java") {
                cmd /c schtasks /S $sutComputerIP /U $userNameInVM  /P $userPwdInVM /Run /TN JavaAgent
            }
            
            if($SelectedAdapter -eq "python") {
                cmd /c schtasks /S $sutComputerIP /U $userNameInVM  /P $userPwdInVM /Run /TN PythonAgent
            }

            if($SelectedAdapter -eq "c") {   
                cmd /c schtasks /S $sutComputerIP /U $userNameInVM  /P $userPwdInVM /Run /TN CAgent
            }
        }
        Write-Host "Config SUT Done" -foregroundcolor Yellow
    }
}
Function ConfigDriver 
{
    $targetFolderOnVM = $driverSettings.tools.TestsuiteZip.targetFolder
    if($driverOS -eq "NonWindows"){
        [string]$driverFullUserName   = $userNameInVM + "@" + $driverComputerIP
        Write-Host "Running update Param script. Please wait..." -ForegroundColor Yellow
        ExecuteSSHCommand $driverFullUserName "pwsh /home/$userNameInVM/Temp/Scripts/UpdateParamConfig.ps1 $SupportCompression $driverComputerName"

        Write-Host "Running config script. Please wait..." -ForegroundColor Yellow
        ExecuteSSHCommand $driverFullUserName "pwsh $targetFolderOnVM/Scripts/Config-LinuxDriverComputer.ps1"
        
        Write-Host "Waiting for SUT Control Adapter ready ..." -foregroundcolor Yellow
        if($SelectedAdapter -ne "") {
            Write-Host "Replace RDP_ClientTestSuite.ptfconfig on $driverComputerIP ..."     
            $testBinInServer = "$targetFolderOnVM/Bin"
            $PTFConfigFilePathOnVM = "$testBinInServer/RDP_ClientTestSuite.ptfconfig"
            ExecuteSSHCommand $driverFullUserName "pwsh /home/$userNameInVM/Temp/Modify-PTFConfigFileAdapter.ps1 -PTFconfigFilePathOnVM $PTFConfigFilePathOnVM"
            if($SupportRDPFile -eq "Yes"){
                Write-Host "ClientSupportRDPFile true" -foregroundcolor Yellow
                $signFileName = "Update-ClientSupportRDPFile."+(Get-Date).ToString("MMddHHmmss")+".Finished.signal"
                ExecuteSSHCommand $driverFullUserName "pwsh /home/$userNameInVM/Temp/Scripts/Modify-PTFConfigFileNode.ps1.ps1 ClientSupportRDPFile true $signFileName $targetFolderOnVM"
            }else{
                Write-Host "ClientSupportRDPFile false" -foregroundcolor Yellow
                $signFileName = "Update-ClientSupportRDPFile."+(Get-Date).ToString("MMddHHmmss")+".Finished.signal"
                ExecuteSSHCommand $driverFullUserName "pwsh /home/$userNameInVM/Temp/Scripts/Modify-PTFConfigFileNode.ps1.ps1 ClientSupportRDPFile false $signFileName $targetFolderOnVM"
            }
            ExecuteSSHCommand $driverFullUserName "pwsh /home/$userNameInVM/Temp/Modify-PTFConfigFileAdapter.ps1 -PTFconfigFilePathOnVM $PTFConfigFilePathOnVM"
            ExecuteSSHCommand $driverFullUserName "pwsh /home/$userNameInVM/Temp/Modify-PTFConfigFileAdapter.ps1 -PTFconfigFilePathOnVM $PTFConfigFilePathOnVM"
            Write-Host "Config Driver Done" -foregroundcolor Yellow
        }
    } else {
        
        [string]$driverFullUserName   = $driverComputerIP + "\" + $userNameInVM
        # Build session. Prepare to execute script on driver computer.
        Write-Host "Trying to connect to computer $driverComputerIP" -ForegroundColor Yellow

        $driverSession = .\Get-RemoteSession.ps1 -FullUserName $driverFullUserName -UserPassword $userPwdInVM -RemoteIP $driverComputerIP
        
        $driverComputerSystemDrive = .\Get-RemoteSystemDrive.ps1 $driverComputerIP $driverFullUserName "$userPwdInVM"
        Write-Host "Trying to get script path on Driver VM" -ForegroundColor Yellow
        $UpdateParamConfigScriptPathOnDriverVM = Invoke-Command -Session $driverSession -ScriptBlock $GetItemInTemp -ArgumentList "UpdateParamConfig.ps1"
        Write-Host $UpdateParamConfigScriptPathOnDriverVM
        
        Write-Host "Running UpdateParamConfig script. Please wait..." -ForegroundColor Yellow
        Invoke-Command -Session $driverSession -ScriptBlock $CreateTaskAndRun `
            -ArgumentList $UpdateParamConfigScriptPathOnDriverVM, "UpdateParamConfig", "-SupportCompression $SupportCompression -ComputerName $driverComputerName"
        
        CheckSignalAndRetryRunTask $driverComputerIP $userNameInVM $userPwdInVM "UpdateParamConfig" $driverComputerSystemDrive "updateParamConfig.finished.signal"
        Remove-PSSession $driverSession

        Write-Host "Trying to connect to computer $driverComputerIP" -ForegroundColor Yellow
        $driverSession = .\Get-RemoteSession.ps1 -FullUserName $driverFullUserName -UserPassword $userPwdInVM -RemoteIP $driverComputerIP
     
        Write-Host "Trying to get script path on Driver VM" -ForegroundColor Yellow
        $ConfigScriptPathOnDriverVM = Invoke-Command -Session $driverSession -ScriptBlock $GetItemInTestSuite -ArgumentList "Config-DriverComputer.ps1","$targetFolderOnVM\Scripts"
        Write-Host $ConfigScriptPathOnDriverVM
        
        Write-Host "Running config script. Please wait..." -ForegroundColor Yellow
        $driverSession = .\Get-RemoteSession.ps1 -FullUserName $driverFullUserName -UserPassword $userPwdInVM -RemoteIP $driverComputerIP
        Invoke-Command -Session $driverSession -ScriptBlock $CreateTaskAndRun `
            -ArgumentList $ConfigScriptPathOnDriverVM, "RunConfig"  
        CheckSignalAndRetryRunTask $driverComputerIP $userNameInVM $userPwdInVM "RunConfig" $driverComputerSystemDrive "config.finished.signal"             
        if($SelectedAdapter -ne "") {
            UpdatePtfconfig
        }
        Write-Host "Waiting for SUT Control Adapter ready ..." -foregroundcolor Yellow
        cmd /c schtasks /S $driverComputerIP /U $driverFullUserName  /P $userPwdInVM /Run /TN WaitForSUTControlAdapterReady
        .\WaitFor-ComputerReady.ps1 $driverComputerIP "$driverFullUserName" "$userPwdInVM" $driverComputerSystemDrive "SUTAdapter.finished.signal" 3600

        Write-Host "Config Driver Done" -foregroundcolor Yellow
        Remove-PSSession $driverSession
    }    
}
# Run test cases on driver and copy test results back to the host.
Function RunTestCaseOnDriver
{
    Write-Host "Trying to get the batch folder" -ForegroundColor Yellow  
    [string]$targetFolderOnVM = $driverSettings.tools.TestsuiteZip.targetFolder

    # Config Driver and Run Cases
    if($driverOS -eq "NonWindows"){
        [string]$driverFullUserName   = $userNameInVM + "@" + $driverComputerIP
        
        # Run test cases
        Write-Host "Start to run test cases. " -ForegroundColor Yellow
        ExecuteSSHCommand $driverFullUserName "pwsh /home/$userNameInVM/Temp/Scripts/$RunCaseScript $BatchToRun $SupportRDPFile $FilterToRun"
        # Test Done
        Write-Host "Run Test Suite Done" -foregroundcolor Yellow
        # Copy test result to host
        Write-Host "Copying test result from test VM to host machine ..." -foregroundcolor Yellow
        Write-TestSuiteInfo "cmd /c scp $($driverFullUserName):$targetFolderOnVM/TestResults/* $TestResultDirOnHost"
        cmd /c scp "$($driverFullUserName):$targetFolderOnVM/TestResults/*" $TestResultDirOnHost
    } else {
        [string]$driverFullUserName   = $driverComputerIP + "\" + $userNameInVM
        [string]$sutFullUserName   = $sutComputerIP + "\" + $userNameInVM     
        # Build session. Prepare to execute script on driver computer.
        $driverSession = .\Get-RemoteSession.ps1 -FullUserName $driverFullUserName -UserPassword $userPwdInVM -RemoteIP $driverComputerIP

        # Failed to start pssession
        if($driverSession -eq $null)
        {
            Write-Error "Failed to connect to driver computer"
            return
        }
        Write-Host "Start to run test cases. " -ForegroundColor Yellow                
        $RunCaseScriptPathOnVM = "c:\temp\scripts\$RunCaseScript" 
        Invoke-Command -Session $driverSession -ScriptBlock $CreateTaskAndRun `
            -ArgumentList $RunCaseScriptPathOnVM,"RunTestCases","$BatchToRun $SupportRDPFile $FilterToRun"
    
        # ScriptBlock to run on driver computer
        # Wait for test cases done
        Write-Host "Running test cases. Please wait..." -ForegroundColor Yellow
        
        & .\WaitFor-ComputerReady.ps1 $driverComputerIP "$driverFullUserName" "$userPwdInVM" $targetFolderOnVM\TestResults "test.finished.signal" $TestCaseTimeout

        # Test Done
        Write-Host "Run Test Suite Done" -foregroundcolor Yellow
        Remove-PSSession $driverSession
        
        Write-Host "Waiting for test result" -ForegroundColor Yellow
        Sleep 5
        
        #----------------------------------------------------------------------------
        # Copy result to host from VM 
        #----------------------------------------------------------------------------
        Write-Host "Copy test results and logs from Driver VM to host machine ..." -foregroundcolor Yellow
        net use "\\$driverComputerIP\C$" $userPwdInVM /User:$driverFullUserName
        $driverComputerSystemDrive = .\Get-RemoteSystemDrive.ps1 $driverComputerIP $driverFullUserName "$userPwdInVM"
        .\Copy-TestResult $driverComputerIP "$driverComputerSystemDrive\*.signal" "$TestResultDirOnHost\ServerLog" $driverFullUserName "$userPwdInVM"
        .\Copy-TestResult $driverComputerIP "$driverComputerSystemDrive\*.log" "$TestResultDirOnHost\ServerLog" $driverFullUserName "$userPwdInVM"
        .\Copy-TestResult $driverComputerIP "$targetFolderOnVM\TestResults" "$TestResultDirOnHost" $driverFullUserName "$userPwdInVM"
        
        Write-Host "Copy test results and logs from SUT VM to host machine ..." -foregroundcolor Yellow    
        $sutSystemDrive = .\Get-RemoteSystemDrive.ps1 $sutComputerIP "$sutFullUserName" "$userPwdInVM"    
        .\Copy-TestResult $sutComputerIP "$sutSystemDrive\*.signal" "$TestResultDirOnHost\ClientLog" $sutFullUserName $userPwdInVM
        .\Copy-TestResult $sutComputerIP "$sutSystemDrive\*.log" "$TestResultDirOnHost\ClientLog" $sutFullUserName $userPwdInVM      
    }    
}

Function Finish
{
    # Finish script
    Pop-Location
    Stop-Transcript -ErrorAction SilentlyContinue
    # Write Call Stack
    if($function:ExitCallStack -ne $null)
    {
	    ExitCallStack "Execute-ProtocolTest.ps1"
    }
    Write-Host "Protocol Test Execute Completed." -ForegroundColor Green
}

#----------------------------------------------------------------------------
# Main Function
#----------------------------------------------------------------------------

Function Main
{
	# Enter call stack and start logging
    Prepare
    SetTimestamp -State initial

    SetTimestamp -State startconfig
    ConfigSUT
    ConfigDriver

    SetTimestamp -State startruntest   
	# Run test cases on driver and copy test result to the host.
	# This procedure can be removed if you do not need it 
    if($runTests -eq "true"){
        RunTestCaseOnDriver
    }

    SetTimestamp -State testdone
	# Exit call stack and stop logging
    Finish
    Exit 0
}

Main