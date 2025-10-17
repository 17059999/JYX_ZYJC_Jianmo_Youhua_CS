#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Execute-ProtocolTest.ps1
## Purpose:        Protocol Test Suite Entry Point for MS-AZOD
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 8
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################
#$CmdLine = ".\Execute-ProtocolTest.ps1 -ProtocolName $Script:TestSuiteName -WorkingDirOnHost $Script:InvocationPath\.. -TestResultDirOnHost $Script:LogFilePath -EnvironmentName $Script:EnvironmentName"

param(
[string]$protocolName         = "MS-AZOD", 
[string]$WorkingDirOnHost      = "D:\WinteropProtocolTesting", 
[string]$TestResultDirOnHost   =  "$WorkingDirOnHost\TestResults\$protocolName",
[string]$EnvironmentName       = "MS-AZOD.xml",
[string]$TestDirInVM           = "C:\Test",
[string]$BatchToRun            = "RunAllTestCases.cmd"
)

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$global:configFileFullPath = "$WorkingDirOnHost\ProtocolTestSuite\$protocolName\VSTORMLITEFiles\XML\$EnvironmentName"
$global:testResultDir = $TestResultDirOnHost
$CurrentScriptName = $MyInvocation.MyCommand.Name
$global:testResult = "succeeded"


# Run test cases on driver and copy test results back to the host.
Function RunTestCaseOnDriver([string]$TrxName=""+ $protocolName +".trx")
{
    # Get Driver Infomation
    [xml]$Content = Get-Content $configFileFullPath

    #Xpath is case sensitive, so make the name node as lower case when match the lower case computername
    $DriverNode = $content.SelectSingleNode("//vm[translate(role,'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')= `"drivercomputer`"]")   
    $DriverParamArray = @{}
    foreach ($Node in $DriverNode.ChildNodes)
    {
        $DriverParamArray[$Node.Name] = $Node.InnerText
    }

    [string]$DriverIPAddress      = $DriverParamArray["ip"]
    [string]$DriverComputerName   = $DriverParamArray["name"]
    [string]$DriverUserName       = $DriverParamArray["username"]
    [string]$DriverPassword       = $DriverParamArray["password"]
    [string]$DriverDomain         = $DriverParamArray["domain"]
    
    [string]$DriverFullUserName   = $DriverUserName
    if([string]::IsNullOrEmpty($DriverDomain))
    {
        $DriverFullUserName   = $DriverIPAddress + "\" + $DriverUserName
    }
    else
    {
        $DriverFullUserName   = $DriverDomain + "\" + $DriverUserName
    }
	
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

    # Run test cases
    Write-Log "Start to run test cases. "
    Invoke-Command -Session $DriverSession -ScriptBlock $CreateTaskAndRun `
        -ArgumentList $BatchFilePathOnVM,"RunTestCases",$DriverFullUserName
 
    # ScriptBlock to run on driver computer
    # Wait for test cases done
    Write-Log "Running test cases. Please wait..."
    
    $WaitForTestDone = [ScriptBlock]{        
        # Wait vstest.console start
		$ProcList = [string](Get-Process)
		$Times = 120 # Try 120 times, i.e. two minutes
        $IsStarted = $false
		for ($count = 0; ($count -lt $Times) -and !($IsStarted); $count ++)
		{
			Sleep 1
			$ProcList = [string](Get-Process)
            $IsStarted = $ProcList.ToLower().Contains("vstest.console")
		}	
        Write-Output "IsStarted: $IsStarted"
		Get-Process vstest.console | Wait-Process
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
    $TestResultDirOnVM = "\\$DriverIPAddress\C$" + "$BatchFolderOnVM\TestResults"
	
	# Copy test result to host
    Write-Log "Copying test result from test VM to host machine ..."
    net use "\\$DriverIPAddress\C$" /del /yes
	net use "\\$DriverIPAddress\C$" $DriverPassword /User:$DriverFullUserName
    Get-Item  -Path "$TestResultDirOnVM\*" # List all files under test result folder on VM
    $LatestTrx = Get-ChildItem -Path "$TestResultDirOnVM" -Include "*.trx" -Recurse | Sort-Object CreationTime -Descending | Select-Object -First 1
    Rename-Item $LatestTrx.FullName $TrxName
    Copy-Item -Path "$TestResultDirOnVM\*" -Destination $TestResultDirOnHost -Recurse -Force
}

Function Write-Log
{
    Param ([Parameter(ValueFromPipeline=$true)] $text)
    $date = Get-Date
    Write-Output "`r`n$date $text"
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
    [string]$Path = [System.IO.Directory]::GetDirectories("$env:HOMEDRIVE\MicrosoftProtocolTests",`
                    $Name,[System.IO.SearchOption]::AllDirectories)
    
    if(($Path -eq $null) -or ($Path -eq ""))
    {
        # Try if the name specified is a file
        [string]$Path = [System.IO.Directory]::GetFiles("$env:HOMEDRIVE\MicrosoftProtocolTests",`
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
# Define call stack functions
#----------------------------------------------------------------------------
[int]$global:indentControl = 0
$global:callStackLogFile = "$testResultDir\Execute-ProtocolTest.ps1.CallStack.log"
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

if($function:EnterCallStack -ne $null)
{
	EnterCallStack "Execute-ProtocolTest.ps1"
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


Function Main
{
	# Enter call stack and start logging
    Prepare

	# Run test cases on driver and copy test result to the host.
    RunTestCaseOnDriver
}

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
Start-Transcript -Path "$testResultDir\Execute-ProtocolTest.ps1.log" -Append -Force

main

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Stop-Transcript
# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Execute-ProtocolTest.ps1"
}
exit 0
