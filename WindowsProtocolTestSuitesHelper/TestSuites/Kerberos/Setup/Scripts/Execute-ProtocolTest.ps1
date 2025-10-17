#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

#############################################################################
#
# Microsoft Windows PowerShell Scripting
# File        : Execute-ProtocolTest.ps1
# Purpose     : Protocol Test Suite Entry Point for Kerberos
# Requirements: Windows PowerShell 2.0
# Supported OS: Windows Server 2012 or later versions
#
# This script will remotely install MSI and run configuration scripts on each
# VM listed in the specified XML file, run test cases on the driver and copy
# test result back to the host machine.
#
# To make this script work, you must specify the parameters for each VM in a 
# XML file ($configFileFullPath) and the XML MUST contain the nodes below for each VM.
# XML nodes example:
#    <vm>
#      <hypervname>Kerberos-DC01</hypervname>  
#      <name>DC01</name>
#      <username>Administrator</username>   
#      <password>Password01@</password>      
#      <domain>contoso.com</domain>
#      <isdc>true</isdc>       
#      <ip>192.168.0.1</ip>
#      <configscript>Config-DC01.ps1</configscript>
#    </vm>
# Missing any node listed above will make the script execution fail.
# If <isdc> node is true, the <username> and <password> will be treated
# as domain username and password; or they will be treated as a local
# account. 
#
# Directory structure on host:
# D:\
# |-- WinBlueRegressionTest ($WorkingDirOnHost)
#     |-- TestResults
#     |-- ScriptLib
#     |-- $ProtocolName
#         |-- Scripts
# Directory structure on VM:
# C:\
# |-- Deploy (Deploy folder on host will be copied here)
#     |-- *.msi
# |-- MicrosoftProtocolTests
#     |-- ...
#         |-- Batch ($BatchFolderOnVM)
#             |-- TestResults
#             |-- RunAllTestCases.cmd
#
# The script will call some scripts in the ScriptLib. So make sure the Script-
# Lib folder is there. Besides, no extra scripts are needed. Scripts running 
# on the VM will be executed directly from this script. No files need to be 
# copied to the VM.
# 
# This script is designed to be capable with any protocol test. Just modify
# the parameters at the beginning. Other parts of the script do not need to 
# be modified. If you have special need for your protocol test, see the 
# comments in the in the script, and modify accordingly.
#
# Created by t-zewang@microsoft.com on 12/5/2012.
#
##############################################################################

Param
(
    [string]$ProtocolName         = "Kerberos",
    [string]$WorkingDirOnHost     = "D:\WinteropProtocolTesting",
    [string]$TestResultDirOnHost  = "$WorkingDirOnHost\TestResults\$ProtocolName",
    [string]$EnvironmentName      = "Kerberos.xml"
)

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$global:configFileFullPath = "$WorkingDirOnHost\ProtocolTestSuite\$ProtocolName\VSTORMLITEFiles\XML\$EnvironmentName"
$global:testResultDir = $TestResultDirOnHost
$CurrentScriptName = $MyInvocation.MyCommand.Name
$RunCaseScript = "Execute-ProtocolTestOnDriver.ps1"
$CurrentScriptPath = Split-Path $MyInvocation.MyCommand.Definition -Parent
$env:Path += ";$CurrentScriptPath"

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

#  Make the script wait for some seconds while printing dots on the screen.
Function Wait([int]$seconds = 10, [string]$prompt)
{
    if($prompt -ne $null)
    {
        Write-Host $prompt
    }

    for ($count = 1; $count -lt $seconds; $count ++)
    {
        $TimeLeft = $seconds - $count
        Write-Progress -Activity "Please wait..." `
                       -Status "$([int]($TimeLeft / 60)) minutes $($TimeLeft % 60) seconds left." `
                       -PercentComplete (($count / $seconds) * 100)
        sleep 1
    }
}

#----------------------------------------------------------------------------
# Utility ScriptBlock Declaration
#----------------------------------------------------------------------------

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
    $Task = "PowerShell PowerShell -Command $Command"
    # Create task
    cmd /c schtasks /Create /RU Administrators /SC ONCE /ST 00:00 /TN $TaskName /TR $Task /IT /F
    Sleep 5
    # Run task
    cmd /c schtasks /Run /TN $TaskName  
}

# Check the test case result
[ScriptBlock]$ScriptToCheckTrxResult = {
    Param([string]$trxPath)

    $result = Get-Item $trxPath\*.trx

    return $result
}


#----------------------------------------------------------------------------
# Procedure Functions
#----------------------------------------------------------------------------
Function Prepare
{
    Write-Host "Start Executing [$CurrentScriptName] ... "

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

# Configurate the specified VM: Install MSI and run config script.
Function RemoteConfigVM($VmParamArray)
{
    [string]$RemoteIP = $VmParamArray["ip"]
    [string]$RemoteComputerName = $VmParamArray["name"]
    [string]$RemoteComputerDomain = $VmParamArray["domain"]
    [string]$RemotePassword = $VmParamArray["password"]
    [string]$RemoteUserName = ""   
    [string]$IsDC = $VmParamArray["isdc"]

    # If the computer is a DC, use the domain user to logon.
	# Because DC cannot be logged on with local user accounts.
    # If the computer is not a DC, use the local account to 
	# logon, no matter whether the computer has been joined to 
	# a domain.
    if($IsDC.ToLower() -eq "true")
    {
        $RemoteUserName = $RemoteComputerDomain + "\" + $VmParamArray["username"]
    }
    else
    {
        $RemoteUserName = $RemoteComputerName + "\" + $VmParamArray["username"]
    }
    
    Write-Host "Start configuring $RemoteComputerName" -ForegroundColor Cyan

    # Build remote session
    Write-Host "Try to connect to computer $RemoteIP" -ForegroundColor Yellow
    $RemoteSession = .\Get-RemoteSession.ps1 -FullUserName $RemoteUserName -UserPassword $RemotePassword -RemoteIP $RemoteIP

    # Run config script on VM
    [string]$ConfigScript = $VmParamArray["configscript"]
    # Get config script path
    Write-Host "Trying to get script path on VM" -ForegroundColor Yellow
    $ConfigScriptPathOnVM = "c:\temp\scripts\$ConfigScript" 
    
    Write-Host "Config Script Path: $ConfigScriptPathOnVM"
    
    # Remove signal files if any
    $RemoveSignalFiles = [ScriptBlock]{
        Get-ChildItem "$env:HOMEDRIVE\" | Where-Object {$_.Name.ToLower().Contains("signal")} | Remove-Item
    }
    #Always check remote session status before run command remotely.
    Invoke-Command -Session $RemoteSession -ScriptBlock $RemoveSignalFiles
    
    # Run config script
    Write-Host "Running config script. Please wait..." -ForegroundColor Yellow
    #Always check remote session status before run command remotely.
    Invoke-Command -Session $RemoteSession -ScriptBlock $CreateTaskAndRun `
        -ArgumentList $ConfigScriptPathOnVM,"RunConfig",$RemoteUserName

    # All remote commands finished
    Remove-PSSession $RemoteSession

    Write-Host "Wait for script to finish by checking config*.finished.signal"
	$waitTimes = 90 # Default times of waiting
	$retryConfig = $false
	if($RemoteComputerName -match "AP0*")
	{
		# For AP01/AP02, it may hang with unknown reason
		# So we use a retry 
	    $retryConfig = $true
		$waitTimes = 45 
	}

     While ($waitTimes -ge 0)
    {
        #Always check remote session status before run command remotely.
        $RemoteSession = .\Get-RemoteSession.ps1 -FullUserName $RemoteUserName -UserPassword $RemotePassword -RemoteIP $RemoteIP
        
        $signalFile = Invoke-Command -Session $RemoteSession -ScriptBlock { Get-Item C:\Temp\Config*.finished.signal }

        if($signalFile -eq $null)
        {
            Remove-PSSession $RemoteSession
            Write-Host "Did not find signal file, will try again." -ForegroundColor Yellow
			Start-Sleep -s 20
        }
        else
        {
		    Write-Host "Found signal file: $signalFile" 
				
	        # Copy test logs to host
            Write-Host "Copying test logs from test VM to host machine ..."
	        net use "\\$RemoteIP\C$" $RemotePassword /User:$RemoteUserName
            Get-Item  -Path "\\$RemoteIP\C$\Temp\*.log" # List all logs files under test result folder on VM
            $Hypervname = $VmParamArray["hypervname"]
			
			Write-Host "Source Path \\$RemoteIP\C$\Temp\*.log  Destination: $TestResultDirOnHost\$Hypervname\" -ForegroundColor Yellow
			if(-not (Test-Path "$TestResultDirOnHost\$Hypervname\")){
				md "$TestResultDirOnHost\$Hypervname\"
			}
            Copy-Item -Path "\\$RemoteIP\C$\Temp\*.log" -Destination "$TestResultDirOnHost\$Hypervname\" -Recurse -Force

	    	# Restart computer (all VMs in Kerberos require restart after run config)
			Write-Host "Restart Computer $RemoteComputerName" -ForegroundColor Yellow
            
	        
            Invoke-Command -Session $RemoteSession -ScriptBlock {Restart-Computer -force} 

            Remove-PSSession $RemoteSession
        
			Start-Sleep -s 60
            break
        }

        $waitTimes = $waitTimes - 1
		
		if($waitTimes -le 0 -and $retryConfig -eq $true)
		{
			Write-Host "$RemoteComputerName does not finish config within 15 minutes, will retry again." -ForegroundColor Yellow
			$retryConfig = $false # only retry once, set retryConfig to false
			$waitTimes = 45 # reset wait time
            
            #Always check remote session status before run command remotely.
            $RemoteSession = .\Get-RemoteSession.ps1 -FullUserName $RemoteUserName -UserPassword $RemotePassword -RemoteIP $RemoteIP
			Invoke-Command -Session $RemoteSession -ScriptBlock $CreateTaskAndRun `
        		-ArgumentList $ConfigScriptPathOnVM,"RetryRunConfig",$RemoteUserName
            Remove-PSSession $RemoteSession
		}
    } 
	
	if($waitTimes -le 0)
	{
		Write-Host "$RemoteComputerName does not finish config within 30 minutes, quit current script." -ForegroundColor Yellow
		exit 1 #Notify wtt to let the job fail here
	}
	
	Write-Host "Wait for computer restart completely. So that dependent computers can find the computer successfully."
    
    $VmCredential = New-Object System.Management.Automation.PSCredential `
        -ArgumentList $RemoteUserName,(ConvertTo-SecureString $RemotePassword -AsPlainText -Force)

    $retryCount = 1
	While($retryCount -lt 10)
	{
		Try
		{
            Write-Host "Try To Connect to remote machine"
			$RemoteSession = New-PSSession -ComputerName $RemoteIP -Credential $VmCredential -ErrorAction Stop
            
            if($RemoteSession -eq $null)
            {
                Write-Host "Failed to connect to driver computer, Retry $retryCount" -ForegroundColor Yellow
                Start-Sleep -s 60
			    $retryCount++
            }
            else{
                Write-Host "Get Remote session"
                $retryCount = 11
            }
		}
		Catch
		{
			Write-Host "Failed to connect to driver computer, Retry $retryCount" -ForegroundColor Yellow
            Start-Sleep -s 20
			$retryCount++
		}
	}


    # Remove PSSession
	Write-Host "Remove RemoteSession"
    Remove-PSSession $RemoteSession
		
    # Finish
    Write-Host "Configuring Computer $RemoteComputerName Done" -ForegroundColor Green
}

# Read all the VMs from the XML and config each VM.
Function ConfigEachVM
{
    [xml]$Content = Get-Content $configFileFullPath
    $VMs = $Content.SelectNodes("//vm")   
    $currentCore = $Content.lab.core
    foreach ($VM in $VMs)
    {
        $VmParamArray = @{}

        if($null -ne $currentCore) {
            foreach($paramNode in $currentCore.ChildNodes)
            {
                $VmParamArray[$paramNode.Name] = $paramNode.InnerText
            }
        }

        foreach($Node in $VM.ChildNodes)
        {
            $VmParamArray[$Node.Name] = $Node.InnerText
        }

        RemoteConfigVM $VmParamArray
    }

    # Wait for computer to be stable
    Start-Sleep 300
}

# Run test cases on driver and copy test results back to the host.
Function RunTestCaseOnDriver
{
    Write-Host "-------------------------------------"
    Write-Host "Run test case on driver computer."
    
    # Get Driver Infomation
    [xml]$Content = Get-Content $configFileFullPath
    $currentCore = $Content.lab.core
	$DriverNode = $Content.lab.servers.vm | where {$_.role -eq "DriverComputer"}

    [string]$DriverIPAddress      = $DriverNode.ip
    [string]$DriverComputerName   = $DriverNode.name
    [string]$DriverUserName       = $DriverNode.username
    [string]$DriverPassword       = $DriverNode.password
    [string]$DriverDomain         = $DriverNode.domain
    [string]$DriverFullUserName   = $DriverComputerName + "\" + $DriverUserName

    if($null -ne $currentCore) {
        if(![string]::IsNullOrEmpty($currentCore.regressiontype) -and ($currentCore.regressiontype -eq "Azure")){
            $DriverUserName = $currentCore.username
            $DriverPassword = $currentCore.password
            $DriverFullUserName   = $DriverComputerName + "\" + $DriverUserName
        }
    }

    # Build session. Prepare to execute script on driver computer.
    Write-Host "Trying to connect to computer $DriverIPAddress"

    $DriverCredential = New-Object System.Management.Automation.PSCredential `
        -ArgumentList $DriverFullUserName,(ConvertTo-SecureString $DriverPassword -AsPlainText -Force)

    $DriverSession = New-PSSession -ComputerName $DriverIPAddress -Credential $DriverCredential
	
	# Failed to start pssession
    if($DriverSession -eq $null)
    {
        Write-Error "Failed to connect to driver computer"
        return
    }
	
    Write-Host "Trying to get the batch folder" -ForegroundColor Yellow
    $TargetFolderOnVM = $DriverNode.tools.TestsuiteZip.targetFolder

    # Run test cases
    Write-Host "Start to run test cases. "
    $RunCaseScriptPathOnVM = "c:\temp\scripts\$RunCaseScript" 
    Invoke-Command -Session $DriverSession -ScriptBlock $CreateTaskAndRun `
        -ArgumentList $RunCaseScriptPathOnVM,"RunTestCases",$DriverFullUserName
 
    # ScriptBlock to run on driver computer
    Write-Host "Running test cases. Please wait..."
    
    $WaitForTestDone = [ScriptBlock]{        
        # Wait VSTEST start
		$ProcList = [string](Get-Process)
		$Times = 120 # Try 120 times, i.e. two minutes
        $IsStarted = $false
		for ($count = 0; ($count -lt $Times) -and !($IsStarted); $count ++)
		{
			Sleep 5
			$ProcList = [string](Get-Process)
            $IsStarted = $ProcList.ToLower().Contains("testhost.x86")
		}	
        Write-Output "IsStarted: $IsStarted"
		Get-Process testhost.x86 | Wait-Process
    }
    Invoke-Command -Session $DriverSession -ScriptBlock $WaitForTestDone
	
	# Test Done
    Write-Host "Run Test Suite Done"
    Remove-PSSession $DriverSession
	
	Write-Host "Waiting for test result"
    Sleep 10
	 
    # Get test result network path
    if ($TargetFolderOnVM.IndexOf('\') -ne 0)
    {
        # If the batch folder path contains system drive, like "C:\MicrosotProtocolTest\..."
        # Remove the system drive, to make it like "\MicrosoftProtocolTest\..."
        # so that it is able to concatenate with the network location
        $TargetFolderOnVM = $TargetFolderOnVM.Remove(0, $TargetFolderOnVM.IndexOf('\'))
    }
    $TestResultDirOnVM = "\\$DriverIPAddress\C$" + "$TargetFolderOnVM\TestResults"
    
    # Copy test result to host
    Write-Host "Copying test result from test VM to host machine ..."
    net use "\\$DriverIPAddress\C$" $DriverPassword /User:$DriverFullUserName
    Get-Item  -Path "$TestResultDirOnVM\*" # List all files under test result folder on VM
    Copy-Item -Path "$TestResultDirOnVM\*" -Destination $TestResultDirOnHost -Recurse -Force
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
    Write-Host "Protocol Test Execute Completed."
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
	
	# Install MSI and run config scripts
	# This procedure can be removed if you do not need it 
	# If you do not need to install MSI, configurate it 
	# in the function. See the function above.
	ConfigEachVM

    # Run test cases on driver and copy test result to the host.
    # This procedure can be removed if you do not need it 
    RunTestCaseOnDriver

    # Exit call stack and stop logging
    Finish
        
    Exit 0
}

# Call Main function
Main
