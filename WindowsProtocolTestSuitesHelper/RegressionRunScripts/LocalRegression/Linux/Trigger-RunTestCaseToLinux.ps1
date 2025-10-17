# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Trigger-RunTestCaseToLinux.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 7
##
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $VM:                     The driver VM configured xml data
# $Setup:                  The Setup configured xml data
# $TestSuiteName:          The test suite name
# $TestCaseTimeout:        The time out of running test case
#----------------------------------------------------------------------------

Param(
    [Parameter(ValueFromPipeline=$True, Mandatory = $True)]
    $VM,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [Xml]$Setup,    
    [Parameter(ValueFromPipeline=$True, Mandatory = $True)]
    $TestSuiteName,
    [Parameter(ValueFromPipeline=$True, Mandatory = $True)]
    $EnvironmentName,
    [string]$TestCaseTimeout       = "3600"
)

Import-Module .\Common\LocalLinuxFunctionLib.psm1

$InitialInvocation       = $MyInvocation
$InvocationFullPath      = $InitialInvocation.MyCommand.Definition
$InvocationName          = [System.IO.Path]::GetFileName($InvocationFullPath)
$InvocationPath          = Split-Path -Parent $InvocationFullPath
$LogFileName             = "$InvocationName.log"
$LogFilePath             = "$InvocationPath\..\TestResults\$TestSuiteName"

Function Configure-RDPClientTestSuiteEnvironmentAndRunTestCase{
    $workingDirOnHost ="$Script:InvocationPath\..\.."
    $CmdLine = "$PSScriptRoot\..\..\ProtocolTestSuite\RDPClient\Scripts\Execute-ProtocolTestToLinuxDriver.ps1 -ProtocolName $Script:TestSuiteName -WorkingDirOnHost $workingDirOnHost -TestResultDirOnHost $Script:LogFilePath -EnvironmentName $Script:EnvironmentName"
	
	$scriptinfo = Get-Command "$PSScriptRoot\..\..\ProtocolTestSuite\RDPClient\Scripts\Execute-ProtocolTestToLinuxDriver.ps1"

    # Read custom parameters from XML file
    $parameters = @{}

    $ParametersNode = $Setup.lab.SelectSingleNode("Parameters")
    if(($ParametersNode -ne $null) -and ($ParametersNode.HasChildNodes))
    {
        foreach($arg in $ParametersNode.ChildNodes)
        {
            # Node type is Element and it has no child
            if(($arg.NodeType -eq [Xml.XmlNodeType]::Element) -and (-not $arg.HasChildNodes))
            {
                $parameters.Add($arg.Name, $arg.Value)
            }
        }
    }

    if($parameters -ne $null)
    {
        foreach($key in $parameters.Keys)
        {
            if($parameters[$key] -ne $null)
            {
                $value = $parameters[$key]
                $CmdLine += " -$key $value"
            }
            else
            {
                Write-TestSuiteInfo "$key is not a valid parameter name for Execute-ProtocolTestToLinuxDriver.ps1, please check it in lab.Parameters of XML configuration!"
            }
        }
    }

    Write-TestSuiteInfo "Running CmdLine:$CmdLine in Trigger-RunTestCaseToLinux.ps1"
    Invoke-Expression $CmdLine
    Pop-Location
}

Function Main{
    $testDir = $env:SystemDrive + "\Temp"

    Start-Transcript -Path "$testDir\Trigger-RunTestCaseToLinux.ps1.log" -Append -Force

    $vmName = $VM.hypervname

	Write-Host "Get VM $vmName IP"

    $currentLinuxVMIP = Get-LinuxVMPublicIP -VM $VM

    $vnetType = "External"
    if($TestSuiteName -eq "RDPClient"){
        #Disable Linux public ip address
        Disable-PublicIPNetwork -VM $VM

        $currentLinuxVMIP = Get-LinuxVMPrivateIP -VM $VM
        # Create trust connection with Linux OS
        Create-TrustConnection -VmIP $currentLinuxVMIP

        Configure-RDPClientTestSuiteEnvironmentAndRunTestCase
        $vnetType = "Internal"
    }else{    
        if($TestSuiteName -eq "RDPServer"){
            #Disable Linux public ip address
            Disable-PublicIPNetwork -VM $VM

            $currentLinuxVMIP = Get-LinuxVMPrivateIP -VM $VM

            # Create trust connection with Linux OS
            Create-TrustConnection -VmIP $currentLinuxVMIP

            $vnetType = "Internal"
        }

        $executeTestSuiteCasePS1 = $Setup.lab.Parameters.Parameter | Where-Object {$_.name -eq "TestCaseScript"}
        $executeTestSuiteCasePS1Name = $executeTestSuiteCasePS1.Value
        $remotePowershellFile  = "/Temp/$executeTestSuiteCasePS1Name"
   
        $changeExecuteTestSuiteCaseFilePermission = "chmod 777 $remotePowershellFile"
        Execute-PlinkShCommand -VmIP $currentLinuxVMIP -ShCommand $changeExecuteTestSuiteCaseFilePermission -ShCommandKey "change_permission"
        Write-TestSuiteInfo "Sleep 20 seconds to wait the file $shFileName permission change..."
        Start-Sleep -Seconds 20

        $executeTestSuiteCaseLogFile = $remotePowershellFile + ".log"

        Write-TestSuiteInfo "Wait to make sure the test cases are tested over..."

        $testSuiteContextNameXml = $Setup.lab.Parameters.Parameter | Where-Object {$_.name -eq "ContextName"}
        $testSuiteContextName = $testSuiteContextNameXml.Value

        if($TestSuiteName -eq 'RDPServer')
        {
            $testSuiteFilterToRunXml = $Setup.lab.Parameters.Parameter | Where-Object {$_.name -eq "FilterToRun"}
            $testSuiteFilterToRun = $testSuiteFilterToRunXml.Value
            $runTestCaseInbackground = "nohup pwsh $remotePowershellFile RunTestCasesByFilter.ps1 $testSuiteFilterToRun > $executeTestSuiteCaseLogFile"
            Execute-PlinkShCommand -VmIP $currentLinuxVMIP -ShCommand $runTestCaseInbackground -ShCommandKey "RunTestCaseOf$TestSuiteName"
        }
        else
        {
            $runTestCaseInbackground = "nohup pwsh $remotePowershellFile -ContextName $testSuiteContextName > $executeTestSuiteCaseLogFile"
            Execute-PlinkShCommand -VmIP $currentLinuxVMIP -ShCommand $runTestCaseInbackground -ShCommandKey "RunTestCaseOf$TestSuiteName"
        }
    }

    Start-Sleep -Seconds 600

    $ScriptLibPath = "$PSScriptRoot\..\..\ScriptLib"
    
    # Clean up test.finished.signal
    $finishSignalFile = "C:\test.finished.signal"
    if(Test-Path $finishSignalFile)
    {
        Remove-Item $finishSignalFile
    }

    $TargetFolderOnVM = $VM.tools.TestsuiteZip.targetFolder
    $testResultFolderPath = ""
    $testResultCompletedSignalFileFolder = ""
    if($TestSuiteName -eq 'FileServer')
    {
        $testResultFolderPath = "/Test"
        $testResultCompletedSignalFileFolder = "/Test"
    }
    elseif($TestSuiteName -eq 'RDPClient')
    {
        $testResultFolderPath = "$TargetFolderOnVM/TestResults"
        $testResultCompletedSignalFileFolder = "/"
    }
    else
    {
        $testResultFolderPath = "$TargetFolderOnVM/TestResults"
        $testResultCompletedSignalFileFolder = "$TargetFolderOnVM/TestResults"
    }

    . "$ScriptLibPath\WaitFor-LinuxComputerReady.ps1" $VM $testResultCompletedSignalFileFolder "test.finished.signal" $TestCaseTimeout $vnetType

    Write-Host "Wait for download test result"

    . "$ScriptLibPath\Copy-LinuxTestResult.ps1" $VM $testResultFolderPath $TestSuiteName $vnetType
}

Main