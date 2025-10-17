#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

#############################################################################
#
# Microsoft Windows Powershell Scripting
# File        : Execute-ProtocolTest.ps1
# Purpose     : Protocol Test Suite Entry Point for ADFamily
# Requirements: Windows Powershell 2.0
# Supported OS: Windows Server 2012, Windows Server 2012 R2
#
#############################################################################

Param
(
    # WTT will pass in the first three arguments.
    [string]$ProtocolName         = "ADFamily",
    [string]$WorkingDirOnHost     = "D:\WinteropProtocolTesting",
    [string]$TestResultDirOnHost  = "$WorkingDirOnHost\TestResults\$ProtocolName",
    [string]$EnvironmentName      = "ADFamily.xml",
    [string]$SelectedProtocols    = "lsat,schema,drsr,publishdc,frs2,lsad,security,nrpc,ldap,apds,samr",
    [string]$TestDriverName       = "AD_ENDPOINT",
    [string]$BatchToRun           = "RunAllTestCases.ps1",
    [string]$EnableWindowsDebug   = "false"
)

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$global:configFileFullPath = "$WorkingDirOnHost\ProtocolTestSuite\$ProtocolName\VSTORMLITEFiles\XML\$EnvironmentName"
$global:testResultDir = $TestResultDirOnHost
$CurrentScriptName = $MyInvocation.MyCommand.Name
$global:testResult = "succeeded"

#----------------------------------------------------------------------------
# Utility Function Declaration
#----------------------------------------------------------------------------
# Define call stack functions, which used to print log file.
[int]$global:indentControl = 0
$global:callStackLogFile = "$TestResultDirOnHost\Execute-ProtocolTest.ps1.CallStack.log"
function global:EnterCallStack($scriptName)
{
	if($scriptName -ne "Execute-ProtocolTest.ps1")
	{
		$global:indentControl++
	}
	$tab = ""
	for ($i=1; $i -le $global:indentControl; $i++)
	{
		$tab += "    "
	}
	$date = get-date -Format "M/d/yyyy hh:mm:ss"
	("") | Out-File -FilePath $callStackLogFile -Append -Force
	($tab + "Start $scriptName    " + $date) | Out-File -FilePath $callStackLogFile -Append -Force
}

function global:ExitCallStack($scriptName)
{

	$tab = ""
	for ($i=1; $i -le $global:indentControl; $i++)
	{
		$tab += "    "
	}
	if($scriptName -ne "Execute-ProtocolTest.ps1")
	{
		$global:indentControl--
	}
	$date = get-date -Format "M/d/yyyy hh:mm:ss"
	($tab + "Exit $scriptName    " + $date) | Out-File -FilePath $callStackLogFile -Append -Force
}

Function Write-Log
{
    Param ([Parameter(ValueFromPipeline=$true)] $text)
    $date = Get-Date
    Write-Output "`r`n$date $text"
}

#----------------------------------------------------------------------------
# Utility ScriptBlock Declaration
#----------------------------------------------------------------------------
# The below functions are declared as scriptblocks because they are going to be 
# executed on remote computers.

# Find specified file or directory from the MicrosoftProtocolTests folder on 
# the computer. The folder will be created after the test suite MSI is installed.
[ScriptBlock]$GetItemInTestSuite = {
    Param([string]$Name)

    # Try if the name specified is a directory
    [string]$Path = [System.IO.Directory]::GetDirectories("$env:HOMEDRIVE\ADFamily-TestSuite-ServerEP",`
                    $Name,[System.IO.SearchOption]::AllDirectories)
    
    if(($Path -eq $null) -or ($Path -eq ""))
    {
        # Try if the name specified is a file
        [string]$Path = [System.IO.Directory]::GetFiles("$env:HOMEDRIVE\ADFamily-TestSuite-ServerEP",`
                        $Name,[System.IO.SearchOption]::AllDirectories)
    }

    return $Path
}

# Remove all the PAUSEs in the script or batch in order to make the script
# or batch can finish automatically.
[ScriptBlock]$RemovePauses = {
    Param([string]$FilePath)

    $Content = Get-Content $FilePath
    $NewContent = ""

    foreach ($Line in $Content)
    {
        if($Line.ToLower().Contains(" pause") -or ($Line.ToLower().Trim() -eq "pause"))
        {
            $Line = ""
        }
        $NewContent = $NewContent + $Line + "`r`n"
    }

    Set-Content $FilePath $NewContent
}

# Create a windows task and run it.
# To do this is because if the commands executed remotely do not have
# full permissions to access all system resources. But a program run by
# the task scheduler has local previleges.
# Notice that the task creater must be the administrator, or the task
# will not be started.
[ScriptBlock]$CreateTaskAndRun = {
    Param([string]$FilePath,[string]$TaskName,[string]$TaskUser)

    # Push to the parent folder folder first, then run
    $ParentDir = [System.IO.Directory]::GetParent($FilePath)
    $Command = "{Push-Location $ParentDir; Invoke-Expression $FilePath}"
    # Guarantee commands run in powershell environment
    $Task = "Powershell Powershell -Command $Command"
    # Create task
    cmd /c schtasks /Create /RU Administrators /SC ONCE /ST 00:00 /TN $TaskName /TR $Task /IT /F
    Sleep 5
    # Run task
    cmd /c schtasks /Run /TN $TaskName  
}

#----------------------------------------------------------------------------
# Procedure Functions
#----------------------------------------------------------------------------
Function Prepare
{
    # Check test result directory
    if(!(Test-Path -Path $TestResultDirOnHost))
    {
        md $TestResultDirOnHost
    }

    # Enter call stack
    if($function:EnterCallStack -ne $null)
    {
	    EnterCallStack "Execute-ProtocolTest.ps1"
    }

    # Start logging
    Stop-Transcript -ErrorAction SilentlyContinue
    Start-Transcript -Path "$TestResultDirOnHost\Execute-ProtocolTest.ps1.log" -Append -Force -ErrorAction SilentlyContinue
    Write-Log "Start Executing [$CurrentScriptName] ... "
    # Push location to the ScriptLib folder
    # Because scripts in lib will be called
    $ScriptLibPathOnHost = "$WorkingDirOnHost\ScriptLib"
    Push-Location $ScriptLibPathOnHost
}
# Run test cases on driver and copy test results back to the host.
Function RunTestCaseOnDriver([string]$TrxName="ADFamily.trx")
{
    # Get Driver Infomation
    [xml]$Content = Get-Content $configFileFullPath
    $DriverParamArray = @{}

    $currentCore = $Content.lab.core
    if($null -ne $currentCore) {
        foreach($paramNode in $currentCore.ChildNodes)
        {
            $DriverParamArray[$paramNode.Name] = $paramNode.InnerText
        }
    }

    $DriverNode = $Content.SelectSingleNode("//vm[name=`'$TestDriverName`']")
    foreach ($Node in $DriverNode.ChildNodes)
    {
        $DriverParamArray[$Node.Name] = $Node.InnerText
    }

    [string]$DriverIPAddress      = $DriverParamArray["ip"]
    [string]$DriverComputerName   = $DriverParamArray["name"]
    [string]$DriverUserName       = $DriverParamArray["username"]
    [string]$DriverPassword       = $DriverParamArray["password"]
    [string]$DriverDomain         = $DriverParamArray["domain"]
    [string]$DriverFullUserName   = $DriverIPAddress + "\" + $DriverUserName
	
    # Build session. Prepare to execute script on driver computer.
    Write-Log "Trying to connect to computer $DriverIPAddress"
    
    $DriverSession = .\Get-RemoteSession.ps1 -FullUserName $DriverFullUserName -UserPassword $DriverPassword -RemoteIP $DriverIPAddress
    
	# Failed to start pssession
    if($DriverSession -eq $null)
    {
        Write-Error "Failed to connect to driver computer"
        return
    }
    
    # Get batch file path
    Write-Log "Trying to get the batch folder"
    $BatchFoldersOnVM = Invoke-Command -Session $DriverSession -ScriptBlock $GetItemInTestSuite -ArgumentList "Batch"
    $BatchFolderOnVM = $BatchFoldersOnVM.ToString().Split(' ')[0] # Get the first Batch folder found in driver VM
    $BatchFilePathOnVM = "$BatchFolderOnVM\$BatchToRun"
    Write-Log "Batch file on driver: $BatchFilePathOnVM"

    # Modify batch file. Remove PAUSEs.
    Write-Log "Removing PAUSEs in the batch file"
    Invoke-Command -Session $DriverSession -ScriptBlock $RemovePauses -ArgumentList $BatchFilePathOnVM

    if($EnableWindowsDebug.ToLower() -eq "true")
    {
        # Modify batch file. Remove PAUSEs.
        Write-Log "Enable Attach TTT"
        Invoke-Command -Session $DriverSession -ScriptBlock {& "$env:HOMEDRIVE\Temp\scripts\enableattachttt.ps1"}
    }
    # Run test cases
    Write-Log "Start to run test cases. "
    Invoke-Command -Session $DriverSession -ScriptBlock $CreateTaskAndRun `
        -ArgumentList $BatchFilePathOnVM,"RunTestCases",$DriverFullUserName
 
    # ScriptBlock to run on driver computer
    # Wait for test cases done
    Write-Log "Running test cases. Please wait..."
    
    $WaitForTestDone = [ScriptBlock]{        
        # Wait MSTest start
		$ProcList = [string](Get-Process)
		$Times = 120 # Try 120 times, i.e. two minutes
        $IsStarted = $false
		for ($count = 0; ($count -lt $Times) -and !($IsStarted); $count ++)
		{
			Sleep 1
			$ProcList = [string](Get-Process)
            $IsStarted = $ProcList.ToLower().Contains("testhost")
		}	
        Write-Output "IsStarted: $IsStarted"
		Get-Process testhost | Wait-Process
    }
    Invoke-Command -Session $DriverSession -ScriptBlock $WaitForTestDone
    
    # Test Done
    Write-Log "Run Test Suite Done"
    Remove-PSSession $DriverSession
	
	Write-Log "Waiting for test result"
    Sleep 10
	
    # Get test result network path
	if ($BatchFolderOnVM.IndexOf('\') -ne 0)
	{
		# If the batch folder path contains system drive, like "C:\MicrosotProtocolTest\..."
		# Remove the system drive, to make it like "\MicrosoftProtocolTest\..."
		# so that it is able to concatenate with the network location
		$BatchFolderOnVM = $BatchFolderOnVM.Remove(0, $BatchFolderOnVM.IndexOf('\'))
	}
    $TestResultDirOnVM = "\\$DriverIPAddress\C$" + "$BatchFolderOnVM\..\TestResults"
	
	# Copy test result to host
    Write-Log "Copying test result from test VM to host machine ..."
    net use "\\$DriverIPAddress\C$" /del /yes
	net use "\\$DriverIPAddress\C$" $DriverPassword /User:$DriverFullUserName
    Get-Item  -Path "$TestResultDirOnVM\*" # List all files under test result folder on VM
    $LatestTrx = Get-ChildItem -Path "$TestResultDirOnVM" -Include "*.trx" -Recurse | Sort-Object CreationTime -Descending | Select-Object -First 1
    # Clear trx
    if(Test-Path -Path "$TestResultDirOnVM\$TrxName")
    {
        Remove-Item -Path "$TestResultDirOnVM\$TrxName" -Force
    }
    Rename-Item $LatestTrx.FullName $TrxName
    Copy-Item -Path "$TestResultDirOnVM\*" -Destination $TestResultDirOnHost -Recurse -Force
}

# Run test cases on driver and copy test results back to the host.
Function ModifyPtfConfigOnDriver([string]$Property, [string]$Value)
{
    # Get Driver Infomation
    [xml]$Content = Get-Content $configFileFullPath
    $DriverParamArray = @{}

    $currentCore = $Content.lab.core
    if(-not $null -eq $currentCore) {
        foreach($paramNode in $currentCore.ChildNodes)
        {
            $DriverParamArray[$paramNode.Name] = $paramNode.InnerText
        }
    }

    $DriverNode = $Content.SelectSingleNode("//vm[name=`'$TestDriverName`']")
    foreach ($Node in $DriverNode.ChildNodes)
    {
        $DriverParamArray[$Node.Name] = $Node.InnerText
    }

    [string]$DriverIPAddress      = $DriverParamArray["ip"]
    [string]$DriverComputerName   = $DriverParamArray["name"]
    [string]$DriverUserName       = $DriverParamArray["username"]
    [string]$DriverPassword       = $DriverParamArray["password"]
    [string]$DriverDomain         = $DriverParamArray["domain"]
    [string]$DriverFullUserName   = $DriverIPAddress + "\" + $DriverUserName

    # Build session. Prepare to execute script on driver computer.
    Write-Log "Trying to connect to computer $DriverIPAddress"
    
    $DriverSession = .\Get-RemoteSession.ps1 -FullUserName $DriverFullUserName -UserPassword $DriverPassword -RemoteIP $DriverIPAddress

	# Failed to start pssession
    if($DriverSession -eq $null)
    {
        Write-Error "Failed to connect to driver computer"
        return
    }

    $ModifyPtfConfig = [ScriptBlock]{
        Param([string]$Property, [string]$Value)

        # Get test suite path
        $Testsuite = "ADFamily-TestSuite-ServerEP"
        $EndPointPath = "$env:SystemDrive\" + $Testsuite 
        $BinDir = "$EndPointPath\bin"
        $PtfConfig = "$BinDir\AD_ServerTestSuite.deployment.ptfconfig"
        $file=Get-Item $PtfConfig
        $file.IsReadOnly = $false
        & "$env:HOMEDRIVE\Temp\Modify-ConfigFileNode.ps1" $PtfConfig $Property $Value
    }
    Invoke-Command -Session $DriverSession -ScriptBlock $ModifyPtfConfig -ArgumentList $Property, $Value

    # Test Done
    Write-Log "Modify $Property to $Value"
    Remove-PSSession $DriverSession
}

Function AnalyzeTrxResults
{
    Write-Log "Analyze the test results"
    $TrxFiles = dir "$testResultDir" -Recurse | where{$_.Name.EndsWith(".trx")} 
    foreach($TrxFile in $TrxFiles)
    {
        [xml]$TrxResults =  Get-Content $TrxFile.FullName
        $ResultSummary = $TrxResults.GetElementsByTagName("ResultSummary")
        $Counters = $TrxResults.GetElementsByTagName("Counters")
        $Total = $Counters.total
        $Passed = $Counters.passed
        if($ResultSummary.outcome -eq "Completed")
        {
            Write-Log "All $Total cases passed in $TrxFile"
        }
        else
        {
            $global:testResult = "failed"
            Write-Log "$Passed/$Total cases passed in $TrxFile"
        }
    }
    Write-Log "The test result is: $global:testResult"
}

Function ExportUniversalLogOnDC([string]$DCRole="PDC")
{
    Write-Log "start copy $DCRole events"
    $DCParamArray = @{}
    [xml]$Content = Get-Content $configFileFullPath

    $currentCore = $Content.lab.core
    if(-not $null -eq $currentCore) {
        foreach($paramNode in $currentCore.ChildNodes)
        {
            $DCParamArray[$paramNode.Name] = $paramNode.InnerText
        }
    }

    $DCNode = $Content.SelectSingleNode("//vm[role=`'$DCRole`']")
    foreach ($Node in $DCNode.ChildNodes)
    {
        $DCParamArray[$Node.Name] = $Node.InnerText
    }

    [string]$DCIPAddress      = $DCParamArray["ip"]
    [string]$DCUserName       = $DCParamArray["username"]
    [string]$DCPassword       = $DCParamArray["password"]
    [string]$DCDomain         = $DCParamArray["domain"]
    [string]$DCFullUserName   = $DCDomain + "\" + $DCUserName
    Write-Log "start output events on DC"
    .\RemoteExecute-Command.ps1 -computerName $DCIPAddress -cmdLine "powershell.exe $env:HOMEDRIVE\temp\scripts\exportdcevents.ps1" -usr $DCFullUserName -pwd $DCPassword
    sleep 120
    $eventPath = "\\$DCIPAddress\C$\temp"
    $dumpPath = $eventPath+"\dump"
    net use $eventPath /delete /y
    net use $eventPath $DCPassword /User:$DCFullUserName
    Get-Item  -Path "$eventPath\*.evtx" # List all files under test result folder on VM
    New-Item -ItemType Directory -Force -Path "$testResultDir\UniversalLogs\$DCRole\"
    Copy-Item -Path "$eventPath\*.evtx" -Destination "$testResultDir\UniversalLogs\$DCRole\" -Recurse -Force
	$debugLogs = "\\$DCIPAddress\C$\Windows\Debug"
	Get-Item  -Path "$debugLogs\*.*"
	New-Item -ItemType Directory -Force -Path "$testResultDir\UniversalLogs\$DCRole\WindowsDebugLog"
	Copy-Item -Path "$debugLogs\*.*" -Destination "$testResultDir\UniversalLogs\$DCRole\WindowsDebugLog\" -Recurse -Force
    if(Test-Path -Path $dumpPath -PathType Container)
    {
        Get-Item  -Path "$dumpPath\*.*"
        New-Item -ItemType Directory -Force -Path "$testResultDir\UniversalLogs\$DCRole\Dump"
        Copy-Item -Path "$dumpPath\*.*" -Destination "$testResultDir\UniversalLogs\$DCRole\Dump\" -Recurse -Force
    }
    Write-Log "finished copy $DCRole events"
}

Function Finish
{
    AnalyzeTrxResults
    if($global:testResult -eq "failed")
    {
        ExportUniversalLogOnDC -DCRole "PrimaryDomainController"
        ExportUniversalLogOnDC -DCRole "SecondaryDomainController"
        ExportUniversalLogOnDC -DCRole "ReadOnlyDomainController"
        ExportUniversalLogOnDC -DCRole "ChildDomainController"
        ExportUniversalLogOnDC -DCRole "TrustedDomainController"
    }
    # Write Call Stack
    if($function:ExitCallStack -ne $null)
    {
	    ExitCallStack "Execute-ProtocolTest.ps1"
    }
    Write-Log "Protocol Test Execute Completed."
    # Finish script
    Pop-Location
    Stop-Transcript -ErrorAction SilentlyContinue
}

Function SetTrustedHosts{
    $originalValue = (get-item WSMan:\localhost\Client\TrustedHosts).value
	$originalValue = $originalValue.Replace("*","");
    
	[xml]$Content = Get-Content $configFileFullPath
    $ips = $Content.SelectNodes("//vm/ip").InnerText  

    $ipstr = ""
    foreach ($ip in $ips){
        if($ipstr){
            $ipstr = $ipstr + "," + $ip 
        }else{
            $ipstr = $ip
        }
    }

    $curValue=""
    if($originalValue){
        $curValue = $originalValue + "," + $ipstr
    }else{
        $curValue = $ipstr
    }
    Write-Host $curValue
    set-item WSMan:\localhost\Client\TrustedHosts -Value $curValue -force

    return $originalValue
}

#----------------------------------------------------------------------------
# Main Function
#----------------------------------------------------------------------------

Function Main
{
	# Enter call stack and start logging
    Prepare
	
	SetTrustedHosts
	
	# Run test cases on driver and copy test result to the host.
	# This procedure can be removed if you do not need it 
    $SelectedProtocols = $SelectedProtocols.ToLower()
    if($SelectedProtocols.Contains("adfamily"))
    {
        ModifyPtfConfigOnDriver "UseNativeRpcLib" "false"
        Write-Log "Run all test cases"
        $BatchToRun = "RunAllTestCases.ps1"
        RunTestCaseOnDriver

        ModifyPtfConfigOnDriver "UseNativeRpcLib" "true"
        Write-Log "Run MS-DRSR-NativeRPC test cases"
        $BatchToRun = "RunDRSRCases.ps1"
        RunTestCaseOnDriver -TrxName "MS-DRSR-NativeRPC.trx"
        try
        {
            [xml]$TrxResults =  Get-Content "$testResultDir\MS-DRSR-NativeRPC.trx"
            $ResultSummary = $TrxResults.GetElementsByTagName("ResultSummary")
            if($ResultSummary.outcome -eq "failed")
            {
                Write-Log "Test suite MS-DRSR-NativeRPC failed, need to rerun"
                Rename-Item -Path "$testResultDir\MS-DRSR-NativeRPC.trx" -NewName "$testResultDir\MS-DRSR-NativeRPC.trx.failed" -Force -ErrorAction SilentlyContinue
                RunTestCaseOnDriver -TrxName "MS-DRSR-NativeRPC.trx"
            }
        }
        catch
        {}
    }
    else
    {
        if($SelectedProtocols.Contains("security"))
        {
            Write-Log "Run MS-ADTS-Security test cases"
            $BatchToRun = "RunADTSSecurityCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-ADTS-Security.trx"
        }
        if($SelectedProtocols.Contains("drsr"))
        {
            ModifyPtfConfigOnDriver "UseNativeRpcLib" "false"
            Write-Log "Run MS-DRSR test cases"
            $BatchToRun = "RunDRSRCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-DRSR.trx"
            try
            {
                [xml]$TrxResults =  Get-Content "$testResultDir\MS-DRSR.trx"
                $ResultSummary = $TrxResults.GetElementsByTagName("ResultSummary")
                if($ResultSummary.outcome -eq "failed")
                {
                    Write-Log "Test suite MS-DRSR failed, need to rerun"
                    Rename-Item -Path "$testResultDir\MS-DRSR.trx" -NewName "$testResultDir\MS-DRSR.trx.failed" -Force -ErrorAction SilentlyContinue
                    RunTestCaseOnDriver -TrxName "MS-DRSR.trx"
                }
            }
            catch
            {}

            ModifyPtfConfigOnDriver "UseNativeRpcLib" "true"
            Write-Log "Run MS-DRSR-NativeRPC test cases"
            $BatchToRun = "RunDRSRCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-DRSR-NativeRPC.trx"
            try
            {
                [xml]$TrxResults =  Get-Content "$testResultDir\MS-DRSR-NativeRPC.trx"
                $ResultSummary = $TrxResults.GetElementsByTagName("ResultSummary")
                if($ResultSummary.outcome -eq "failed")
                {
                    Write-Log "Test suite MS-DRSR-NativeRPC failed, need to rerun"
                    Rename-Item -Path "$testResultDir\MS-DRSR-NativeRPC.trx" -NewName "$testResultDir\MS-DRSR-NativeRPC.trx.failed" -Force -ErrorAction SilentlyContinue
                    RunTestCaseOnDriver -TrxName "MS-DRSR-NativeRPC.trx"
                }
            }
            catch
            {}
        }
        if($SelectedProtocols.Contains("lsat"))
        {
            Write-Log "Run MS-LSAT test cases" 
            $BatchToRun = "RunLSATCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-LSAT.trx"
        }
        if($SelectedProtocols.Contains("schema"))
        {
            Write-Log "Run MS-ADTS-Schema test cases" 
            $BatchToRun = "RunADTSSchemaCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-ADTS-Schema.trx"
        }
        if($SelectedProtocols.Contains("publishdc"))
        {
            Write-Log "Run MS-ADTS-PublishDC test cases" 
            $BatchToRun = "RunADTSPublishDCCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-ADTS-PublishDC.trx"
        }
        if($SelectedProtocols.Contains("nrpc"))
        {
            Write-Log "Run MS-NRPC test cases" 
            $BatchToRun = "RunNRPCCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-NRPC.trx"
        }
        if($SelectedProtocols.Contains("lsad"))
        {
            Write-Log "Run MS-LSAD test cases" 
            $BatchToRun = "RunLSADCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-LSAD.trx"
        }
        if($SelectedProtocols.Contains("frs2"))
        {
            Write-Log "Run MS-FRS2 test cases" 
            $BatchToRun = "RunFRS2Cases.ps1"
            RunTestCaseOnDriver -TrxName "MS-FRS2.trx"
        }
        if($SelectedProtocols.Contains("ldap"))
        {
            Write-Log "Run MS-ADTS-LDAP test cases" 
            $BatchToRun = "RunADTSLDAPCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-ADTS-LDAP.trx"
        }
        if($SelectedProtocols.Contains("apds"))
        {
            Write-Log "Run MS-APDS test cases" 
            $BatchToRun = "RunAPDSCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-APDS.trx"
        }
        if($SelectedProtocols.Contains("samr"))
        {
            Write-Log "Run MS-SAMR test cases" 
            $BatchToRun = "RunSAMRCases.ps1"
            RunTestCaseOnDriver -TrxName "MS-SAMR.trx"
        }
    }
      
    
	# Exit call stack and stop logging
    Finish
    Exit 0
}

Main
