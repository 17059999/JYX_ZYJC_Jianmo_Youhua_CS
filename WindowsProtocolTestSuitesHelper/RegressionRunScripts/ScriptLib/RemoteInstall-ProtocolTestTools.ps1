#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           RemoteInstall-ProtocolTestTools.ps1
## Purpose:        Protocol Test Suite Entry Point for FileSharing
## Requirements:   Windows Powershell 2.0, 3.0
## Supported OS:   Windows Server 8, Windows Server 2012
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
[string]$WorkingDir = "$env:SystemDrive\WinteropProtocolTesting",
[string]$VMConfigFile = "$env:SystemDrive\WinteropProtocolTesting\FileSharing\VSTORMLITEFiles\XML\FileSharing_Samba_SMB2002.xml", 
[string]$TestLogPath = "$env:SystemDrive\WinteropProtocolTesting\TestResults\FileSharing"
)


#----------------------------------------------------------------------------
# Define Common Funcitons
#----------------------------------------------------------------------------
function ExitCode()
{ 
    return $MyInvocation.ScriptLineNumber 
}

function CopyFiles($srcPath,$destComputerName,$usr,$pwd)
{
$sharedRoot = "C`$"
Write-Host "Try to connect to \\$destComputerName\$sharedRoot by $usr / $pwd ..." -foregroundcolor green
for($i=0;$i -le 3;$i++)
{
    net.exe use "\\$destComputerName\$sharedRoot" $pwd /User:$usr 2>&1 | Write-Host
    if ($lastExitCode -eq 0)
    {
        break
    }
    else
    {
        sleep 5
    }
}
Write-Host "Copying test files from $srcPath to \\$destComputerName\$sharedRoot\Temp\" -foregroundcolor green
robocopy.exe /MIR /NFL /NDL /R:2 $srcPath "\\$destComputerName\$sharedRoot\Temp\" 2>&1 | Write-Host

Write-Host "Disconnect share \\$destComputerName\$sharedRoot" -ForegroundColor green
net.exe use "\\$destComputerName\$sharedRoot" /DELETE 2>&1 | Write-Host
}

function GetNetuseIp($ip)
{
$objIP = [IPAddress]$ip
if($objIP.AddressFamily -eq "Internetwork")
{
	Write-Host "$ip is IPv4 address"
	$netuseIP = $ip 
}
else
{
	Write-Host "$ip is IPv6 address"
	$netuseIP = $ip.Replace(":","-")
	$netuseIP = $netuseIP + ".ipv6-literal.net"
}

return $netuseIP
}

#----------------------------------------------------------------------------
# Verify input parameters
#----------------------------------------------------------------------------
Write-Host "Verify input parameters..." -foregroundcolor green
if(!(Test-Path $WorkingDir))
{
    Write-Host "WorkingDir $WorkingDir does not existed." -ForegroundColor Red
    exit ExitCode
}
if ([System.String]::IsNullOrEmpty($VMConfigFile))
{
    Write-host "VMConfigFile could not be null or empty." -ForegroundColor Red
    exit ExitCode
}
else
{
    if(!(Test-Path $VMConfigFile)) 	
    {
        Write-host "VMConfigFile $VMConfigFile could not be found." -ForegroundColor Red
        exit ExitCode
    }
}
if(!(Test-Path $TestLogPath))
{
    Write-Host "TestLogPath $TestLogPath does not existed." -ForegroundColor Red
    exit ExitCode
}

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
Start-Transcript -Path "$TestLogPath\RemoteInstall-ProtocolTestTools.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Print input parameters
#----------------------------------------------------------------------------
Write-Host "------------------------------" -foregroundcolor Green
Write-Host "Starting..." -foregroundcolor Green
Write-Host "Input parameters" -foregroundcolor Green
Write-Host "`t`$WorkingDir = $WorkingDir" -foregroundcolor Yellow
Write-Host "`t`$VMConfigFile = $VMConfigFile" -foregroundcolor Yellow
Write-Host "`t`$TestLogPath = $TestLogPath" -foregroundcolor Yellow
Write-Host "------------------------------" -foregroundcolor Green

#----------------------------------------------------------------------------
# Check OS version and Prepare Hyper-V module
#----------------------------------------------------------------------------
$osbuildnum= "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor

Write-host "Perpare Hyper-V Module."
.\Prepare-HyperVModule.ps1
$prepareModuleExitCode = $lastexitcode
if($prepareModuleExitCode -ne 0)
{
    Write-host "Prepare-HyperVModule.ps1 has unexptected error code $prepareModuleExitCode" -ForegroundColor Red
    exit ExitCode
}
$VMInfo = get-item "HKLM:\SOFTWARE\Microsoft\Virtual Machine\Guest\Parameters"
$vmHostName = $VmInfo.GetValue("HostName")
$delegatedVMName = $VmInfo.GetValue("VirtualMachineName")
Write-Host "VM Host: $vmHostName" -ForegroundColor Green
Write-Host "Delegated VM: $delegatedVMName" -ForegroundColor Green

#----------------------------------------------------------------------------
# Copy protocol test tools to VMs and install
#----------------------------------------------------------------------------
Write-Host "Copy protocol test tools to VMs and install" -foregroundcolor green
$protocolTestFilePath = "$workingDir\VSTORMLITE\Custom"
[xml]$VMConfig = Get-Content $VMConfigFile
if($VMConfig -eq $null)
{
    Write-host "VM configure file $VMConfigFile is not valid." -ForegroundColor Red
    exit ExitCode
}

$global:userNameInVM    = $VMConfig.lab.core.username
$global:userPwdInVM     = $VMConfig.lab.core.password
$VMs = $VMConfig.lab.servers.vm | where {$_.Tools -ne $null}
foreach($vm in $VMs)
{
    # Prepare connection IP and user name
    Write-Host "Prepare connection IP and user name" -ForegroundColor Green
    $vmIP = $vm.ip | select-object -first 1
    $vmNetuseIP = GetNetuseIp $vmIP
    if($vm.domain -match "workgroup")
    {
        $netUser = $vm.name + "\$userNameInVM"
    }
    else
    {
        $netUser = $vm.domain + "\$userNameInVM"
    }

    # Create VMsnapshot before restore to CleanENV.
    Write-Host "Create VMsnapshot before restore to CleanENV." -ForegroundColor Green    
    $snapshotName = "Finish Testing (" + (get-date -Format "MM/dd/yyy - hh:mm:ss") + ")"
    if ($osbuildnum -eq "6.1") 
    {
        $vmObj = Get-Vm -Name $vm.hypervname -Server $vmHostName
        New-VMSnapshot -VM $vmobj -Server $vmHostName -Force
        Sleep 10
    }
    if ($osbuildnum -ge "6.2") 
    {
        Checkpoint-VM -Name $vm.hypervname -SnapshotName $snapshotName -ComputerName $vmHostName
        sleep 10
    }

    # Restore VMsnapshot to CleanENV
    Write-Host "Restore VMsnapshot to CleanENV" -ForegroundColor Green
    if ($osbuildnum -eq "6.1") 
    {
        $vmObj = Get-Vm -Name $vm.hypervname -Server $vmHostName
        $snapshot = Get-VMSnapshot -vm $vmobj -Server $vmHostName | where {$_.ElementName -eq "CleanENV"} | Restore-VMSnapshot -Confirm:$false -Restart
        Sleep 15
    }
    if ($osbuildnum -ge "6.2") 
    {
        Get-VMSnapshot -VMName $vm.hypervname -ComputerName $vmHostName | where {$_.Name -eq "CleanENV"} |  Restore-VMSnapshot -Confirm:$false
        Start-VM -Name $vm.hypervname -ComputerName $vmHostName
        sleep 15
    }

    # Copy test files to test VM
    Write-host "Copy test files to test VM" -ForegroundColor Green
    CopyFiles $protocolTestFilePath $vmNetuseIP $netUser $userPwdInVM
    
    # Specify VM name for install tools
    $writeVmName = "CMD /C ECHO " + $vm.name + "`> C:\Temp\name.txt"
    .\RemoteExecute-Command.ps1 $vmNetuseIP "$writeVmName" "$netUser" "$userPwdInVM"

    # Create task for install protocol tools
    $taskName = "InstallProtocolTestTools"
    $task = "PowerShell C:\Temp\InstallMSIAndTools.ps1"

    $createTask = "CMD /C schtasks /Create /RU Administrators /SC Monthly /TN $TaskName /TR `"$Task`" /IT /F"
    Write-Host "$createTask"
    .\RemoteExecute-Command.ps1 $vmNetuseIP "$createTask" "$netUser" "$userPwdInVM"

    # Execute the task to install tools.
    $exeTask = "cmd /c schtasks /Run /TN $TaskName"
    Write-Host "$exeTask"
    .\RemoteExecute-Command.ps1 $vmNetuseIP "$exeTask" "$netUser" "$userPwdInVM"
    # Check if Install tools finish
    .\WaitFor-ComputerReady.ps1 $vmNetuseIP "$netUser" "$userPwdInVM" "C:\Temp" "InstallMSIAndTools.Completed.signal" 120
    if($lastexitcode -ne 0)
    {
        # Retry
        .\RemoteExecute-Command.ps1 $vmNetuseIP "$exeTask" "$netUser" "$userPwdInVM"
        .\WaitFor-ComputerReady.ps1 $vmNetuseIP "$netUser" "$userPwdInVM" "C:\Temp" "InstallMSIAndTools.Completed.signal" 120
    }

    # Create VMsnapshot for VMs with new test suite installed
    Write-Host "Create VMsnapshot for VMs with new test suite installed." -ForegroundColor Green
    if(Test-Path "$TestLogPath\Build.txt")
    {
        $content = Get-Content "$TestLogPath\Build.txt" | Select-Object -First 1
        if($content -ne $null -and $content -ne "")
        {
            $index = $content.IndexOf("1.0")
            $buildstring = $content.Substring($index,10).Replace("\",".")
        }
    }

    if($buildstring -eq $null -or $buildstring -notmatch "1.0")
    {
        $buildstring = "latest build"
    }

    $snapshotName = "Installed $buildstring (" + (get-date -Format "MM/dd/yyy - hh:mm:ss") + ")"
    if ($osbuildnum -eq "6.1") 
    {
        $vmObj = Get-Vm -Name $vm.hypervname -Server $vmHostName
        New-VMSnapshot -VM $vmobj -Server $vmHostName -Force
        Sleep 10
    }

    if ($osbuildnum -ge "6.2") 
    {
        Checkpoint-VM -Name $vm.hypervname -SnapshotName $snapshotName -ComputerName $vmHostName
        sleep 10
    }
}

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Write-Host "Completed. Stop logging and exit" -foregroundcolor Green
Stop-Transcript
exit 0
