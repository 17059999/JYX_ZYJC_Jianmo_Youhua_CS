#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

Param
(
    [string]$WorkingPath = "C:\temp"
)

# Switch to the script path
Write-Host "Switching to $WorkingPath..." -ForegroundColor Yellow
Push-Location $WorkingPath
	
#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath"

#----------------------------------------------------------------------------
# Protocol variables
#----------------------------------------------------------------------------
$protocolName = "BranchCache"
$endPointPath = "$env:SystemDrive\MicrosoftProtocolTests\BranchCache"
$version = Get-ChildItem $endPointPath | Where-Object {$_.Name -match "\d+\.\d+\.\d+\.\d+"} | Sort-Object Name -descending | Select-Object -first 1
$binDir = "$endPointPath\$version\bin"

# Get mstest
$tempDir = "$env:SystemDrive\Temp"
[string]$mstest = & "$tempDir\GetTestPath.ps1" mstest

Write-Host "mstest path: $mstest" -ForegroundColor Yellow
$testDir = "$env:SystemDrive\Test"
$paramConfigFile = "$scriptPath\$protocolName.xml"

#----------------------------------------------------------------------------
# Start Transcript
#----------------------------------------------------------------------------
Start-Transcript -Path "$scriptPath\Domain-RunAllTestCases.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Set Execution Policy for 32 bit PowerShell
#----------------------------------------------------------------------------
Write-Host "Set Execution Policy for 32 bit PowerShell ..."
$cmd = "%systemroot%\syswow64\windowspowershell\v1.0\powershell.exe -Command set-executionpolicy -ExecutionPolicy unrestricted >>c:\CMDlog.txt"
cmd /c $cmd 2>&1 | .\Write-Info.ps1


#----------------------------------------------------------------------------
# Make sure RemoteAccess service is running
#----------------------------------------------------------------------------
$service = Get-Service -Name RemoteAccess -ComputerName DC -ErrorAction Stop
while($service.Status -ne "Running")
{
    Write-Host "RemoteAccess service in DC is not runing, try to start it..."
    Start-Service -InputObj $service -ErrorAction Continue
    Sleep 5
    $service = Get-Service -Name RemoteAccess -ComputerName DC -ErrorAction Stop
}

Push-Location $WorkingPath
#----------------------------------------------------------------------------
# Workaround to resolve DNS name
#----------------------------------------------------------------------------
$contentServerComputerName = .\Get-Parameter.ps1 -sourceFileName $paramConfigFile -attrName "ContentServerComputerName"
$contentServerFQDN = .\Get-Parameter.ps1 -sourceFileName $paramConfigFile -attrName "ContentServerFQDN"
$contentServerCtrlIp = .\Get-Parameter.ps1 -sourceFileName $paramConfigFile -attrName "contentServerCtrlIp"

$contentServerCtrlIp + " " + $contentServerComputerName | Out-File C:\Windows\System32\drivers\etc\hosts -Append -Encoding ascii
$contentServerCtrlIp + " " + $contentServerFQDN | Out-File C:\Windows\System32\drivers\etc\hosts -Append -Encoding ascii

#----------------------------------------------------------------------------
# Perpare for execute test suite
#----------------------------------------------------------------------------
$ptfconfig = "$binDir\BranchCache_TestSuite.deployment.ptfconfig"
CMD /C attrib -R $ptfconfig

if(!(Test-Path $testDir))
{
	md $testDir
}

$testResultDir = $testDir + "\TestResults"
if(Test-Path $testResultDir)
{
    Get-childitem -path "$testResultDir\*" -Recurse | Remove-Item -force -Recurse 
}
else
{
    md $testResultDir
}

$finishSignalFile = "$testDir\test.finished.signal"
if(Test-Path $finishSignalFile)
{
	Remove-Item $finishSignalFile
}


#----------------------------------------------------------------------------
# Start to execute test suite
#----------------------------------------------------------------------------
$startSignalFile = "$testDir\test.started.signal"
echo "test.started.signal" > $startSignalFile
cd $testDir

# Make sure the hosted cache server name used in execution case is consistent with environment
& $WorkingPath\Modify-ConfigFileNode.ps1 $ptfconfig "HostedCacheServerComputerName" "HostedServer"

# Start to run test cases based on PCCRTP transport
& $WorkingPath\Modify-ConfigFileNode.ps1 $ptfconfig "ContentTransport" "PCCRTP"
& "$mstest" /testcontainer:"$binDir\BranchCache_TestSuite.dll" /runconfig:"$binDir\LocalTestRun.testsettings" /Category:"PCCRTP" /resultsfile:"$testResultDir\BranchCache_TestSuite_PCCRTP.trx" 2>&1 

# Start to run test cases based on SMB2 transport
& $WorkingPath\Modify-ConfigFileNode.ps1 $ptfconfig "ContentTransport" "SMB2"
& "$mstest" /testcontainer:"$binDir\BranchCache_TestSuite.dll" /runconfig:"$binDir\LocalTestRun.testsettings" /Category:"SMB2" /resultsfile:"$testResultDir\BranchCache_TestSuite_SMB2.trx" 2>&1


#----------------------------------------------------------------------------
# Finished
#----------------------------------------------------------------------------
echo "test.finished.signal" > $finishSignalFile
Stop-Transcript
#$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")