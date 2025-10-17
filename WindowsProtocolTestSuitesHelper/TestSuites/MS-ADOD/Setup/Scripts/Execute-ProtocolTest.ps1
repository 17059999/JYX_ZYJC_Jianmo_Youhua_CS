#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

#############################################################################
#
# Microsoft Windows PowerShell Scripting
# File        : Execute-ProtocolTest.ps1
# Purpose     : Protocol Test Suite Entry Point for MS-ADOD
# Requirements: Windows PowerShell 2.0
# Supported OS: Windows Server 2012
# Remark      : This script is used in regression run for azure and Local.
#
# This script will remotely install MSI and run configuration scripts on each
# VM listed in the specified XML file, run test cases on the driver and copy
# test result back to the host machine.
#
##############################################################################

param(
[string]$protocolName          = "MS-ADOD",
[string]$WorkingDirOnHost      = "D:\WinBlueRegressionTest", 
[string]$TestResultDirOnHost   = "D:\WinBlueRegressionTest\TestResults\ADOD",
[string]$EnvironmentName       = "MS-ADOD.xml"
)

# Assign argument to variable
$sutOS             	   = "Windows" #SUT
$WorkingDir            = $WorkingDirOnHost
$BatchToRun            = "RunAllTestCases.cmd"  

#----------------------------------------------------------------------------
# Define source folders in VM Host
#----------------------------------------------------------------------------
$srcScriptLibPathOnHost = $workingDir + "\ScriptLib\"
$srcToolPathOnHost      = $workingDir + "\Tools\"
$xmlPathOnHost          = $workingDir + "\ProtocolTestSuite\$protocolName\VSTORMLITEFiles\XML\"
$srcScriptPathOnHost    = $workingDir + "\ProtocolTestSuite\$protocolName\Scripts\"
$srcTestSuitePathOnHost = $workingDir + "\ProtocolTestSuite\$protocolName\Bin\"
$srcDataPathOnHost      = $workingDir + "\ProtocolTestSuite\$protocolName\Data\"
$srcMSIInstallOnHost    = $workingDir + "\ProtocolTestSuite\$protocolName\Deploy\"
Push-Location $srcScriptLibPathOnHost

# Get Driver Computer
[XML]$vmConfig = Get-Content "$xmlPathOnHost\$EnvironmentName"
$driverComputerSettting = $vmConfig.lab.servers.vm | where {$_.role -eq "DriverComputer"}
$clientComputerSettting = $vmConfig.lab.servers.vm | where {$_.role -eq "Client"}

[string]$DriverIPAddress      = $driverComputerSettting.ip
[string]$DriverVMName         = $driverComputerSettting.hypervname
[string]$DriverComputerName   = $driverComputerSettting.name
[string]$DriverUserName       = $vmConfig.lab.core.username
[string]$DriverPassword       = $vmConfig.lab.core.password
[string]$DriverDomain         = $driverComputerSettting.domain
[string]$DriverFullUserName   = $DriverIPAddress + "\" + $DriverUserName
[string]$IsDC = $driverComputerSettting.domain

if($IsDC.ToLower() -eq "true")
{
	$DriverFullUserName = $DriverDomain + "\" + $DriverUserName
}
else
{
	$DriverFullUserName = $DriverIPAddress + "\" + $DriverUserName
}

Write-Host '1---'+$vmConfig -BackgroundColor Red
Write-Host '2---'+$xmlPathOnHost -BackgroundColor Red
Write-Host '3---'+$driverComputerSettting -BackgroundColor Red
Write-Host '4---'+$proxyComputerSettting -BackgroundColor Red


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


#  Make the script wait for some seconds while printing dots on the screen.
Function Wait([int]$seconds = 10, [string]$prompt)
{
    if($prompt -ne $null)
    {
        Write-Host $prompt -ForegroundColor Yellow
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
# The below functions are declared as scriptblocks because they are going to be 
# executed on remote computers.

# Install all MSI files in the specified folder
[ScriptBlock]$InstallMSIs = {
    Param([string]$FolderPath,[string]$EndPoint)
            
    $InstallMSIs = Get-ChildItem $FolderPath

    foreach ($Msi in $InstallMSIs)
    {
        cmd /c msiexec -i ($Msi.FullName) -q TARGET_ENDPOINT=$EndPoint
    }
}

# Call Config-MIPParamConfigFile.ps1
[ScriptBlock]$UpdateConfigValue = {
    Param([string]$ConfigName,[string]$ConfigValue)
            
    C:\Temp\Config-MIPParamConfigFile.ps1 -attrName $ConfigName -Value $ConfigValue
}

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
    $Task = "PowerShell PowerShell -Command $Command"
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
    Write-Host "Start Executing Execute-ProtocolTest.ps1 ... " -ForegroundColor Cyan

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
}

Function ConfigParamConfig
{
    $IPVersion             = "IPv4"
    $fullDomainName        = $driverComputerSettting.domain
    $domainAdminUserName   = $vmConfig.lab.core.username
    $domainAdminUserPwd    = $vmConfig.lab.core.password
    $pdcComputerName       = $driverComputerSettting.name
    $pdcIP                 = $driverComputerSettting.ip
    $clientComputerName    = $clientComputerSettting.name
    $clientIP              = $clientComputerSettting.ip
    $clientAdminUserName   = $vmConfig.lab.core.username
    $clientAdminUserPwd    = $vmConfig.lab.core.password

    Write-Host "Start to configure ParamConfig.xml" -ForegroundColor Yellow
    Write-Host "$IPVersion, $fullDomainName, $domainAdminUserName, $domainAdminUserPwd, $pdcComputerName"

    $paramList = @{fullDomainName = "$fullDomainName"; domainAdminUserName = "$domainAdminUserName"; 
    domainAdminUserPwd = "$domainAdminUserPwd"; pdcComputerName = "$pdcComputerName"; pdcIP = "$pdcIP"; 
    clientComputerName = "$clientComputerName"; clientIP = "$clientIP"; clientAdminUserName = "$clientAdminUserName"; 
    clientAdminUserPwd = "$clientAdminUserPwd"; ipVersion = "$IPVersion"; }

    $RemoteSession = .\Get-RemoteSession.ps1 -FullUserName $DriverFullUserName -UserPassword $DriverPassword -RemoteIP $DriverIPAddress
	# Failed to start pssession
    if($RemoteSession -eq $null)
    {
        Write-Error "Failed to connect to $DriverIPAddress" 
        return
    }


    foreach ($param in $paramList.Keys)
    {
        $value = $paramList.Item($param)
        $configSignal = "ConfigParam"+"$param"+"Finished.signal"
        
	    if ($sutOS -ne "NonWindows")
        {
            Write-Host "Key:"$param
            Write-Host "Value:"$value

            Invoke-Command -Session $RemoteSession -ScriptBlock $UpdateConfigValue -ArgumentList $param,$value
        }   
    }
}

# Configurate the specified VM: Install MSI and run config script.
Function RemoteConfigVM($VmParamArray,$InstallMSI = $true)
{
    [string]$RemoteIP = $VmParamArray["ip"]
    [string]$RemoteComputerName = $VmParamArray["name"]
    [string]$RemoteComputerDomain = $VmParamArray["domain"]
    [string]$RemotePassword = $vmConfig.lab.core.password
    [string]$RemoteUserName = ""   
    [string]$IsDC = $VmParamArray["isdc"]

    Write-Host "RemoteIP:                   $RemoteIP"
    Write-Host "RemoteComputerName:         $RemoteComputerName"
    Write-Host "RemoteComputerDomain:       $RemoteComputerDomain"
    Write-Host "RemotePassword:             $RemotePassword"
    Write-Host "IsDC:                       $IsDC"
          
    # If the computer is a DC, use the domain user to logon.
	# Because DC cannot be logged on with local user accounts.
    # If the computer is not a DC, use the local account to 
	# logon, no matter whether the computer has been joined to 
	# a domain.
    if($IsDC.ToLower() -eq "true")
    {
        $RemoteUserName = $RemoteComputerDomain + "\" + $vmConfig.lab.core.username
    }
    else
    {
        $RemoteUserName = $RemoteComputerName + "\" + $vmConfig.lab.core.username
    }
    
    Write-Host "Start configuring $RemoteComputerName" -ForegroundColor Cyan

    # Build remote session
    Write-Host "Try to connect to computer $RemoteIP" -ForegroundColor Yellow
    Write-Host "User:$RemoteUserName, Password: $RemotePassword" -BackgroundColor Green

    $RemoteSession = .\Get-RemoteSession.ps1 -FullUserName $RemoteUserName -UserPassword $RemotePassword -RemoteIP $RemoteIP
    
	# Failed to start pssession
    if($RemoteSession -eq $null)
    {
        Write-Error "Failed to connect to $RemoteIP" 
        return
    }
    
    # Install MSI on the VM
    if ($InstallMSI -eq $true)
    {     
        # Copy deploy files to the VM
        # Directory on VM: C:\Deploy
        $DeployPathOnHost = "$TestSuiteDirOnHost\Deploy"
        Write-Host "Copying MSI to VM" -ForegroundColor Yellow
        # Copy deploy files to the VM
        net use "\\$RemoteIP\C$" $RemotePassword /User:$RemoteUserName
        Copy-Item -Path $DeployPathOnHost -Destination "\\$RemoteIP\C$" -Recurse -Force

        # Determine whether the target VM is Driver or SUT
        if ($RemoteComputerName -eq $DriverComputerName){ $TargetEndPoint = "TESTSUITE" }
        else { $TargetEndPoint = "SUT" }

        # Remote Install MSI
        Write-Host "Installing MSI" -ForegroundColor Yellow
        Invoke-Command -Session $RemoteSession -ScriptBlock $InstallMSIs -ArgumentList "C:\Deploy","$TargetEndPoint"
    }

    # Run config script on VM
    [string]$ConfigScript = $VmParamArray["configscript"]
    # Get config script path
    if($ConfigScript.Length -eq 0){
        return;
    }

    Write-Host "Trying to get script path on VM" -ForegroundColor Yellow
    $ConfigScriptPathOnVM = Invoke-Command -Session $RemoteSession -ScriptBlock $GetItemInTestSuite `
                            -ArgumentList $ConfigScript
    Write-Host "Config Script Path: $ConfigScriptPathOnVM"

    # Remove PAUSEs in the script
    Write-Host "Removing PAUSEs in the script" -ForegroundColor Yellow
    Invoke-Command -Session $RemoteSession -ScriptBlock $RemovePauses -ArgumentList $ConfigScriptPathOnVM
    
    # Remove signal files if any
    $RemoveSignalFiles = [ScriptBlock]{
        Get-ChildItem "$env:HOMEDRIVE\" | Where-Object {$_.Name.ToLower().Contains("signal")} | Remove-Item
    }
    Invoke-Command -Session $RemoteSession -ScriptBlock $RemoveSignalFiles
    
    # Run config script
    Write-Host "Running config script. Please wait..." -ForegroundColor Yellow
    Invoke-Command -Session $RemoteSession -ScriptBlock $CreateTaskAndRun `
        -ArgumentList $ConfigScriptPathOnVM,"RunConfig",$RemoteUserName
    # All remote commands finished
    Remove-PSSession $RemoteSession
    
    # Wait for script done and restart
	# Because some scripts generate signal file while some do not,
	# some scripts need reboot some do not, 
	# we do not have a general method to tell whether the script finishes
	# or not.
	# Usually 200 seconds is enough for the script to finish.
    Wait 200 ## TODO: Modify according to the actual requirement

    $RemoteSession = .\Get-RemoteSession.ps1 -FullUserName $RemoteUserName -UserPassword $RemotePassword -RemoteIP $RemoteIP
    $CheckSignalFiles = [ScriptBlock]{
        Test-Path -Path "C:\ConfigScript.finished.signal"
    }
    if ($RemoteSession -ne $null)
    {
        if (Invoke-Command -Session $RemoteSession -ScriptBlock $CheckSignalFiles)
        {
            Write-Host "Configuring Computer $RemoteComputerName Done" -ForegroundColor Green
        }
        else
        {
            Write-Host "Configuring Computer $RemoteComputerName Failed" -ForegroundColor Red
        }
        Remove-PSSession $RemoteSession
    }
    else
    {
        Write-Host "Unable to contact Computer $RemoteComputerName" -ForegroundColor Red
    }

    # Finish
}

# Read all the VMs from the XML and config each VM.
Function ConfigEachVM
{
    $VMs = $vmConfig.SelectNodes("//vm")

    foreach ($VM in $VMs)
    {
        $VmParamArray = @{}

        foreach($Node in $VM.ChildNodes)
        {
            $VmParamArray[$Node.Name] = $Node.InnerText
        }

	    ## TODO: If you do not need install MSI on the computer, use
	    RemoteConfigVM $VmParamArray -InstallMSI $false
    }
}

# Run test cases on driver and copy test results back to the host.
Function RunTestCaseOnDriver
{
    # Get Driver Infomation
    
    Write-Host $DriverIPAddress
    Write-Host $DriverComputerName
    Write-Host $DriverUserName
    Write-Host $DriverPassword
    Write-Host $DriverDomain
    Write-Host $DriverFullUserName
	# Use local user to logon
	
    # Build session. Prepare to execute script on driver computer.
    Write-Host "Trying to connect to computer $DriverIPAddress" -ForegroundColor Yellow
    $DriverSession = .\Get-RemoteSession.ps1 -FullUserName $DriverFullUserName -UserPassword $DriverPassword -RemoteIP $DriverIPAddress
    
	# Failed to start pssession
    if($DriverSession -eq $null)
    {
        Write-Error "Failed to connect to driver computer"
        return
    }
    
    # Get batch file path
    Write-Host "Trying to get the batch folder" -ForegroundColor Yellow
    $BatchFoldersOnVM = Invoke-Command -Session $DriverSession -ScriptBlock $GetItemInTestSuite -ArgumentList "Batch"
    $BatchFolderOnVM = $BatchFoldersOnVM.ToString().Split(' ')[0] # Get the first Batch folder found in driver VM
    $BatchFilePathOnVM = "$BatchFolderOnVM\$BatchToRun"
    Write-Host "Batch file on driver: $BatchFilePathOnVM"

    # Modify batch file. Remove PAUSEs.
    Write-Host "Removing PAUSEs in the batch file" -ForegroundColor Yellow
    Invoke-Command -Session $DriverSession -ScriptBlock $RemovePauses -ArgumentList $BatchFilePathOnVM

    # Run test cases
    Write-Host "Start to run test cases. " -ForegroundColor Yellow
    Invoke-Command -Session $DriverSession -ScriptBlock $CreateTaskAndRun `
        -ArgumentList $BatchFilePathOnVM,"RunTestCases",$DriverFullUserName
 
    # ScriptBlock to run on driver computer
    # Wait for test cases done
    Write-Host "Running test cases. Please wait..." -ForegroundColor Yellow
    
    $WaitForTestDone = [ScriptBlock]{        
        # Wait MSTest start
		$ProcList = [string](Get-Process)
		$Times = 60 # Try 60 times, i.e. one minute
		for ($count = 0; ($count -lt $Times) -and !($ProcList.ToLower().Contains("vstest.console")); $count ++)
		{
			Sleep 1
			$ProcList = [string](Get-Process)
		}	
		# Wait until test finished
		Get-Process vstest.console | Wait-Process
    }
    Invoke-Command -Session $DriverSession -ScriptBlock $WaitForTestDone
    
    # Test Done
    Write-Host "Run Test Suite Done" -foregroundcolor Yellow
    Remove-PSSession $DriverSession
	
	Write-Host "Waiting for test result" -ForegroundColor Yellow
    Sleep 5
	
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
    Write-Host "Copying test result from test VM to host machine ..." -foregroundcolor Yellow
	net use "\\$DriverIPAddress\C$" $DriverPassword /User:$DriverFullUserName
    Get-Item  -Path "$TestResultDirOnVM\*" # List all files under test result folder on VM
    Copy-Item -Path "$TestResultDirOnVM\*" -Destination $TestResultDirOnHost -Recurse -Force

    Get-Item -Path "\\$DriverIPAddress\C$\Logs\*"
    Copy-Item -Path "\\$DriverIPAddress\C$\Logs\*" -Destination "$TestResultDirOnHost\UniversalLogs\" -Recurse -Force
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
    ConfigParamConfig
    SetTimestamp -State initial

    SetTimestamp -State startconfig
	# Install MSI and run config scripts
	# This procedure can be removed if you do not need it 
	# If you do not need to install MSI, configurate it 
	# in the function. See the function above.
    # --------------no need to config---------------------
    ConfigEachVM  
    
    SetTimestamp -State startruntest
	# Run test cases on driver and copy test result to the host.
	# This procedure can be removed if you do not need it 
    RunTestCaseOnDriver  
    SetTimestamp -State testdone
        
	# Exit call stack and stop logging
    Finish
    Exit 0
}

Main
