#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-MSIandTools.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 7
##
##############################################################################

Param(
[string]$protocolName,#Protocol Name
[string]$testDirInVM     = "c:\Test",
[string]$targetEndpoint  = "TESTSUITE",##"TESTSUITE" for Client,the values is specified by the test suites.
[string]$IPVersion       = "IPv4",
[string]$workgroupDomain = "Domain",
[string]$userNameInVM    = "administrator",
[string]$userPwdInVM     = "Password01!",
[string]$domainInVM      = "contoso.com",
[string]$endPoint        = "Server",
[string]$step            = ""
)

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($protocolName -eq $null -or $protocolName -eq "")
{
    Throw "Parameter protocolName is required."
}
if ($targetEndpoint -eq $null -or $targetEndpoint -eq "")
{
    Throw "Parameter targetEndpoint is required."
}
if ($endPoint -eq $null -or $endPoint -eq "")
{
    Throw "Parameter endPoint is required."
}

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
$testResultsPath="$testDirInVM\TestResults"
if (!(Test-Path -Path $testResultsPath) )
{
    New-Item -Type Directory -Path $testResultsPath -Force
}
$logFile = $testResultsPath + "\Install-MSIandTools.ps1.log"
if (!(Test-Path -Path $logFile))
{
    New-Item -Type File -Path $logFile -Force
}
Start-Transcript $logFile -Append

Write-Host "EXECUTING [Install-MSIandTools.ps1]..." -foregroundcolor cyan
Write-Host "`$protocolName    = $protocolName"
Write-Host "`$testDirInVM     = $testDirInVM"
Write-Host "`$targetEndpoint  = $targetEndpoint"
Write-Host "`$IPVersion       = $IPVersion"
Write-Host "`$workgroupDomain = $workgroupDomain"
Write-Host "`$userNameInVM    = $userNameInVM"
Write-Host "`$userPwdInVM     = $userPwdInVM"
Write-Host "`$domainInVM      = $domainInVM"
Write-Host "`$endPoint        = $endPoint"
Write-Host "`$step            = $step"


#----------------------------------------------------------------------------
#  Set value for variable
#----------------------------------------------------------------------------
$MSIInstallFullName = $protocolName + "-TestSuite-" + "$endPoint" + "EP.msi"
$DeployDir          = "$testDirInVM\Deploy"
$toolsPath          = "$testDirInVM\Tools"
$scriptsPath        = "$testDirInVM\Scripts"

write-host "Set location to $scriptsPath ."
pushd $scriptsPath

#----------------------------------------------------------------------------
#  Function: Wait till no SETUP process
#----------------------------------------------------------------------------
Function Wait
{
    Write-Host "Wait for VS2010 process ..." -foregroundcolor yellow
    $process = Get-Process | Where-Object{$_.Name -eq "setup"}
    $newline = 0
    while($process -ne $null)
    {
        if($newline%60 -eq 0)
        {
            Write-Host
        }
        Write-Host "." -foregroundcolor Green -nonewline
        sleep 1
        $newline ++
        $process = Get-Process | Where-Object{$_.Name -eq "setup"}
    }
    Write-Host "VS2010 Setup process terminated." -foregroundcolor yellow
}

#----------------------------------------------------------------------------
#  Function: Join to Domain
#----------------------------------------------------------------------------
Function JoinDomain
{
    Write-Host "Join Domain" -foregroundcolor yellow
    .\Config-IP.ps1 $IPVersion $testResultsPath
    .\Join-Domain.ps1 $workgroupDomain $domainInVM $userNameInVM $userPwdInVM $testResultsPath
    
    Write-Host "Set Auto logon ..." -foregroundcolor yellow    
    $autoUserName = $domainInVM + "\" + $userNameInVM
    set-itemproperty -path  HKLM:\SOFTWARE\Microsoft\"Windows NT"\CurrentVersion\Winlogon -name DefaultUserName -value $autoUserName
    set-itemproperty -path  HKLM:\SOFTWARE\Microsoft\"Windows NT"\CurrentVersion\Winlogon -name DefaultDomainName -value ""
    set-itemproperty -path  HKLM:\SOFTWARE\Microsoft\"Windows NT"\CurrentVersion\Winlogon -name DefaultPassword -value $userPwdInVM
    set-itemproperty -path  HKLM:\SOFTWARE\Microsoft\"Windows NT"\CurrentVersion\Winlogon -name AutoAdminLogon -value 1
    set-itemproperty -path  HKLM:\SOFTWARE\Microsoft\"Windows NT"\CurrentVersion\Winlogon -name AltDefaultUserName -value $autoUserName
    set-itemproperty -path  HKLM:\SOFTWARE\Microsoft\"Windows NT"\CurrentVersion\Winlogon -name "AltDefaultDomainName" -value ""
    
    .\restartandrun.bat "cmd /c powershell $scriptsPath\Install-MSIandTools.ps1 $protocolName $testDirInVM $targetEndpoint $IPVersion $workgroupDomain $userNameInVM $userPwdInVM $domainInVM $endPoint Finished"
}

#--------------------------------------------------------------------------------------------------------------------------------
#  Function: Write  MSI full path to the finished signal file($env:HOMEDRIVE\MSIInstalled.signal)
#--------------------------------------------------------------------------------------------------------------------------------
Function WriteFinishSignal
{
    cmd /c netsh advfirewall set allprofile state off 2>&1 | Write-Host
    Write-Host  "Write signal file to system drive."
    $MSIScriptsFile = [System.IO.Directory]::GetFiles("$env:HOMEDRIVE\MicrosoftProtocolTests", "ParamConfig.xml", [System.IO.SearchOption]::AllDirectories)
    [String]$TestSuiteScriptsFullPath = [System.IO.Directory]::GetParent($MSIScriptsFile)
    cmd /c ECHO $TestSuiteScriptsFullPath >$env:HOMEDRIVE\MSIInstalled.signal
}

#--------------------------------------------------------------------------------------------------------------------------------
#  Function: Get the tools  configuration information from  MIPToolsSettings.xml
#--------------------------------------------------------------------------------------------------------------------------------
Function Get-ConfigurationInfo($queryAttr)
{
    Write-Host "Get tools install information"
    [xml]$xmlContent = gc .\MIPToolsSettings.xml    ##$testDirInVM\Scripts\MIPToolsSettings.xml
    $protocols = $xmlContent.SelectNodes("MIPProtocols/$protocolName")
    $protocol
    foreach($protoNode in $protocols)
    {
        if($protoNode.EndPoint -eq $endPoint)
        {
            $protocol = $protoNode
        }
    }
    foreach($node in $protocol.GetElementsByTagName("tool"))
    {
        If($node.Name -eq $queryAttr)
        {        
            return $node
        }
    }
    return $null
}

#----------------------------------------------------------------------------
#  Start to install Tools and MSI.
#----------------------------------------------------------------------------
if($targetEndpoint -eq "TESTSUITE")
{
    if($step -eq "" -or $step -eq $null)
    {    
        $vsts = Get-ConfigurationInfo "VSTS"
        If($vsts -ne $null)
        {
            #----------------------------------------------------------------------------
            # Install VS2010
            #----------------------------------------------------------------------------
            Write-Host "install VS2010 ..." -foregroundcolor yellow

            # Install process
            $CDROM = (gwmi Win32_CDROMDrive).id

            Write-Host "Start installing VS2010 ..." -ForegroundColor Yellow
            cmd /c start $CDROM\setup\setup.exe /q /full /norestart
            Wait
            .\restartandrun.bat "cmd /c powershell $scriptsPath\Install-MSIandTools.ps1 $protocolName $testDirInVM $targetEndpoint $IPVersion $workgroupDomain $userNameInVM $userPwdInVM $domainInVM $endPoint continue"
        }
        else
        {
            .\restartandrun.bat "cmd /c powershell $scriptsPath\Install-MSIandTools.ps1 $protocolName $testDirInVM $targetEndpoint $IPVersion $workgroupDomain $userNameInVM $userPwdInVM $domainInVM $endPoint continue"
        }
    }
    elseif($step -eq "continue")
    {
        $vsts = Get-ConfigurationInfo "VSTS"
        If($vsts -ne $null)
        {
            Write-Host "Continue installing VS2010 ..." -ForegroundColor Yellow
            $CDROM = (gwmi Win32_CDROMDrive).id
            cmd /c start $CDROM\setup\setup.exe /q /full /norestart
            Wait
        }
        $protocolSDK = Get-ConfigurationInfo "ProtocolSDK"
        If($protocolSDK -ne $null)
        {
            $version = $protocolSDK.Get($protocolSDK.count-1).version
            $CPUArchitecture = $protocolSDK.Get($protocolSDK.count-1).CPUArchitecture
            Write-Host "Start to install Protocol SDK ..." -ForegroundColor Yellow
            Write-Host "Source file: $toolsPath\ProtocolSDK\$version\$CPUArchitecture\ProtocolTestSuitesLibrary.msi"
            cmd /c $toolsPath\ProtocolSDK\$version\$CPUArchitecture\ProtocolTestSuitesLibrary.msi -q /l $testResultsPath\Install-MIPTools.log 2>&1 | Write-Host
        }
        $specExplorer = Get-ConfigurationInfo "SpecExplorer"
        If($specExplorer -ne $null)
        {
            $version = $specExplorer.Get($specExplorer.count-1).version
            $CPUArchitecture = $specExplorer.Get($specExplorer.count-1).CPUArchitecture
            Write-Host "Start to install SpecExplorer ..." -foregroundcolor yellow
            Write-Host "Source file: $toolsPath\SE\$version\$CPUArchitecture\SpecExplorer.msi"
            cmd /c $toolsPath\SE\$version\$CPUArchitecture\SpecExplorer.msi -q /l $testResultsPath\Install-MIPTools.log 2>&1 | Write-Host            
        }

        $networkMonitor = Get-ConfigurationInfo "NetworkMonitor"
        If($networkMonitor -ne $null)
        {
            $version = $networkMonitor.Get($networkMonitor.count-1).version
            $CPUArchitecture = $networkMonitor.Get($networkMonitor.count-1).CPUArchitecture     
            Write-Host "Start to install network monitor:" -foregroundcolor yellow
            Write-Host "Source file: $toolsPath\NetworkMonitor\$version\$CPUArchitecture\Netmonpt3.msi"
            cmd /c "msiexec /quiet /i $toolsPath\NetworkMonitor\$version\$CPUArchitecture\Netmonpt3.msi /l $testResultsPath\Install-MIPTools.log" 2>&1 | Write-Host
        }

        $networkMonitorParser = Get-ConfigurationInfo "NetworkMonitorParser"
        If($networkMonitorParser -ne $null)
        {
            $version = $networkMonitorParser.Get($networkMonitorParser.count-1).version
            $CPUArchitecture = $networkMonitorParser.Get($networkMonitorParser.count-1).CPUArchitecture      
            Write-Host "Start to install network monitor parser:" -foregroundcolor yellow
            Write-Host "Source file: $toolsPath\NetworkMonitor\$version\$CPUArchitecture\Microsoft_PT3_Parsers.msi"
            cmd /c "msiexec /quiet /i $toolsPath\NetworkMonitor\$version\$CPUArchitecture\Microsoft_PT3_Parsers.msi /l $testResultsPath\Install-MIPTools.log" 2>&1 | Write-Host
        }
		
		$PTF = Get-ConfigurationInfo "PTF"	
        If($PTF -ne $null)
        {
            $version = $PTF.Get($PTF.count-1).version
            $CPUArchitecture = $PTF.Get($PTF.count-1).CPUArchitecture      
            Write-Host "Start to install PTF:" -foregroundcolor yellow
            Write-Host "Source file: $toolsPath\PTF\$version\$CPUArchitecture\ProtocolTestFramework.msi"
            cmd /c "msiexec /quiet /i $toolsPath\PTF\$version\$CPUArchitecture\ProtocolTestFramework.msi /l $testResultsPath\Install-MIPTools.log" 2>&1 | Write-Host
        }

        Write-Host "Start to install test suite:" -foregroundcolor yellow
        cmd /c msiexec -i $DeployDir\$MSIInstallFullName -q TARGET_ENDPOINT=$targetEndpoint /l $testResultsPath\Install-MIPTools.log
        
        if($workgroupDomain -eq "Domain")
        {
            JoinDomain
        }
        else
        {
            WriteFinishSignal
        }
    }
    elseif($step -eq "Finished")
    {
        WriteFinishSignal        
    }
}
elseif($targetEndpoint -eq "DOMAIN")
{
    if($step -eq "" -or $step -eq $null)
    {
        cmd /c msiexec -i $DeployDir\$MSIInstallFullName -q TARGET_ENDPOINT=$targetEndpoint  /l $testResultsPath\Install-MIPTools.log
        WriteFinishSignal
    }
}
else
{
    if($step -eq "" -or $step -eq $null)
    {
        cmd /c msiexec -i $DeployDir\$MSIInstallFullName -q TARGET_ENDPOINT=$targetEndpoint  /l $testResultsPath\Install-MIPTools.log
        if($workgroupDomain -eq "Domain")
        {
            JoinDomain
        }
         else
        {
            WriteFinishSignal
        }
    }
    elseif($step -eq "Finished")
    {
        WriteFinishSignal
    }
}
exit 0

