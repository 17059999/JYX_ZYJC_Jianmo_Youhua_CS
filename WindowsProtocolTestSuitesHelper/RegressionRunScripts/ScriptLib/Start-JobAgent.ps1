#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Start-JobAgent.ps1
## Purpose:        This is the job agent to run the real job scheduled by Schedule-TestJob.ps1
## Version:        1.0 (28 April, 2011)
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows
##
##############################################################################

param(
[string]$protocolName, 
[string]$cluster, 

[string]$srcVMPath="\\FileServer\VMLib",
[string]$srcScriptLibPath="\\FileServer\PETLabStore\ScriptLib",
[string]$srcToolPath="\\FileServer\PETLabStore\Tools",
[string]$srcTestSuitePath="\\FileServer\PETLabStore\TestSuite",
[string]$ISOPath="\\FileServer\PETLabStore\ISO",

#The following is for WTT Mix parameters
[string]$clientOS,
[string]$serverOS,
[string]$workgroupDomain,
[string]$IPVersion,
[string]$ClientCPUArchitecture,
[string]$ServerCPUArchitecture,
#End of WTT Mix parameters

[string]$workingDir,
[string]$VMDir,
[string]$testResultDir,

[string]$userNameInVM,
[string]$userPwdInVM,
[string]$domainInVM,
[string]$testDirInVM,

#For resource release
[int]$memsize,
[int]$hdsize,
[int]$ipsize,

#For SQL/Exchange productline
[string]$CustomizeScenario="",

[String]$SendTo,
[String]$SendCC
)

Write-Host "Start to Run the Job."
#----------------------------------------------------------------------------
# Start a clear new log.
#----------------------------------------------------------------------------
Write-Host "Stop previous transcript if it is not stopped in previous running."
#Trap [Exception]
#{
#    Stop-Transcript
#    continue
#}
Stop-Transcript
# Start Transcript. Please make sure the folder exists before this.
if(!(Test-Path $testResultDir))
{
    New-Item $testResultDir -ItemType directory
}
Start-Transcript $testResultDir\Start-JobAgent.log -force

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Start-JobAgent.ps1] ..." -foregroundcolor cyan
Write-Host "`$protocolName          = $protocolName"
Write-Host "`$cluster               = $cluster"

Write-Host "`$srcVMPath             = $srcVMPath"
Write-Host "`$srcScriptLibPath      = $srcScriptLibPath"
Write-Host "`$srcToolPath           = $srcToolPath"
Write-Host "`$srcTestSuitePath      = $srcTestSuitePath"
Write-Host "`$ISOPath               = $ISOPath"

Write-Host "`$clientOS              = $clientOS"
Write-Host "`$serverOS              = $serverOS"
Write-Host "`$workgroupDomain       = $workgroupDomain"
Write-Host "`$IPVersion             = $IPVersion"
Write-Host "`$ClientCPUArchitecture = $ClientCPUArchitecture"
Write-Host "`$ServerCPUArchitecture = $ServerCPUArchitecture"

Write-Host "`$workingDir            = $workingDir"
Write-Host "`$VMDir                 = $VMDir"
Write-Host "`$testResultDir         = $testResultDir"

Write-Host "`$userNameInVM          = $userNameInVM"
Write-Host "`$userPwdInVM           = $userPwdInVM"
Write-Host "`$domainInVM            = $domainInVM"
Write-Host "`$testDirInVM           = $testDirInVM"

Write-Host "`$memsize               = $memsize"
Write-Host "`$hdsize                = $hdsize"
Write-Host "`$ipsize                = $ipsize"

Write-Host "`$CustomizeScenario     = $CustomizeScenario"

Write-Host "`$SendTo                = $SendTo"
Write-Host "`$SendCC                = $SendCC"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script executes the test suite on Hyper-V host machine."
    Write-host
    Write-host "Example: Start-JobAgent.ps1 MS-SMB2 C6 \\pt3controller\VMLib \\pt3controller\ScriptLib \\pt3controller\Tools \\pt3controller\TestSuite \\pt3controller\ISO Vista W2K8 Workgroup IPv4 x86 x86 d:\PCTLabTest d:\VM d:\PCTLabTest\TestResult Administrator Password01! contoso.com C:\Test 3 30 3 `"`" alias alias"
    Write-host
    Write-host "Note: Only protocolName and cluster are required. Others are optional."
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($protocolName -eq $null -or $protocolName -eq "")
{
    Throw "Parameter protocolName is required."
}
$srcTestSuitePath = "$srcTestSuitePath\$protocolName"

#----------------------------------------------------------------------------
# Change the working directory.
#----------------------------------------------------------------------------
Write-Host "Set $workingDir\ScriptLib as current dir."
Push-Location $workingDir\ScriptLib

#----------------------------------------------------------------------------
# Check overriden value for parameters from test machine's [WTTBin] folder
#----------------------------------------------------------------------------
function Check-OverridenParam([string]$paramName, [string]$defaultValue)
{ 
    $newValue = .\Get-OverriddenLabParam.ps1 $paramName
    if ($newValue -ne "")
    {
        Write-Host "Replace $paramName with $newValue ..." 
        return $newValue
    }
    else
    {
        return $defaultValue
    }
}

$srcVMPath        = Check-OverridenParam "VMBasePath"    $srcVMPath
$srcScriptLibPath = Check-OverridenParam "ScriptLibPath" $srcScriptLibPath
$srcToolPath      = Check-OverridenParam "ToolsPath"     $srcToolPath
$srcTestSuitePath = Check-OverridenParam "TestSuitePath" $srcTestSuitePath
$ISOPath          = Check-OverridenParam "ISOPath"       $ISOPath

#----------------------------------------------------------------------------
# Copy tool used for PET Testing, based on parameters (maybe overriden)
#----------------------------------------------------------------------------
Write-Host "Copy PET Tools ..." -foregroundcolor cyan
robocopy /MIR /NFL /NDL $srcToolPath $workingDir\Tools 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Copy reportingtool.exe from \PETLabStore\Tools\ResultGatherer\Cx, according to the $cluster
#----------------------------------------------------------------------------
Write-Host "Copy $cluster ReportingTool.exe ..." -foregroundcolor cyan
xcopy $srcToolPath\ResultGatherer\$cluster\*.* $workingDir\Tools\ResultGatherer /s /y  2>&1 | Write-Host

#----------------------------------------------------------------------------
# Not used here now: Copy TestSuite binaries used for PET Testing, based on parameters (maybe overriden)
#
# WTT task "Copy Protocol's folder - Robocopy" will Copy binaries used for PET Testing based on parameters instead.
# Command used in this WTT task: robocopy [WinTestSuitePath]\[ProtocolName]\[Endpoint] [WorkingDir]\[ProtocolName] /MIR
#        parameter [Endpoint] will specify which endpoint's test will be run, "Server" or "Client"
#----------------------------------------------------------------------------
#Write-Host "Copy PET TestSuite Binaries ..." -foregroundcolor cyan
#robocopy /MIR /NFL /NDL $srcTestSuitePath\$protocolName $workingDir\$protocolName 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Cache the domain, username and password, so that other functions can use it if not provided by their caller.
#----------------------------------------------------------------------------
if ($workgroupDomain -eq "Domain")
{
    $global:domain = $domainInVM
}
else
{
    $global:domain = $null
}
$global:usr = $userNameInVM
$global:pwd = $userPwdInVM

#----------------------------------------------------------------------------
# Cache the test result dir
#----------------------------------------------------------------------------
$global:testResultDir = $testResultDir

#----------------------------------------------------------------------------
# Make a copy of etc\hosts file from VM_Base.
#----------------------------------------------------------------------------
Copy-Item $srcVMPath\hosts $env:windir\system32\drivers\etc\hosts -force

#----------------------------------------------------------------------------
# Execute protocol entry script.
#----------------------------------------------------------------------------
Write-Host "Execute protocol entry script ..." -foregroundcolor Yellow
if($CustomizeScenario -eq "")
{
    & ..\$protocolName\Scripts\Execute-ProtocolTest.ps1 `
    $protocolName $cluster `
    $srcVMPath $srcScriptLibPath $srcToolPath $srcTestSuitePath $ISOPath  `
    $clientOS $serverOS $workgroupDomain $IPVersion $ClientCPUArchitecture $ServerCPUArchitecture `
    $workingDir $VMDir $testResultDir `
    $userNameInVM $userPwdInVM $domainInVM $testDirInVM
}
else
{
    & ..\$protocolName\Scripts\Execute-ProtocolTest.ps1 `
    $protocolName $cluster `
    $srcVMPath $srcScriptLibPath $srcToolPath $srcTestSuitePath $ISOPath  `
    $clientOS $serverOS $workgroupDomain $IPVersion $ClientCPUArchitecture $ServerCPUArchitecture `
    $workingDir $VMDir $testResultDir `
    $userNameInVM $userPwdInVM $domainInVM $testDirInVM $CustomizeScenario
}

Pop-Location
Stop-Transcript

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Execute-TestSuite.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

#----------------------------------------------------------------------------
# Release the resource after the job is completed.
#----------------------------------------------------------------------------
$resPath = Get-Location
$resconfig = [String]$resPath + "\Schedule-TestJob.config"
[xml]$res = get-content $resconfig
if ($res -ne $null)
{
    [int]$ava_mem=$res.Resource.Available.Memory;
    [int]$ava_hd=$res.Resource.Available.Harddisk;
    [int]$ava_ip=$res.Resource.Available.SubIp;
    [int]$max_mem=$res.Resource.Max.Memory;
    [int]$max_hd=$res.Resource.Max.Harddisk;
    [int]$max_ip=$res.Resource.Max.SubIp;
    $ava_mem = [int]$ava_mem+$memsize;
    $ava_hd = [int]$ava_hd+$hdsize;
    $ava_ip = [int]$ava_ip+$ipsize;
    # succed to allocate the resource for the task.
    if( ($ava_mem -le $max_mem) -and ($ava_hd -le $max_hd) -and ($ava_ip -le $max_ip))
    {
        $res.Resource.Available.Memory=$ava_mem.toString()
        $res.Resource.Available.Harddisk=$ava_hd.toString()
        $res.Resource.Available.SubIp=$ava_ip.toString()
        $res.Save($resconfig);
        Write-Host "Successfully release "$memsize"G memory, "$hdsize"G harddisk and "$ipsize" ip subnet." -ForegroundColor Green
    }
    # fail to allocate the resouce for the job task.
    Write-Host "Invalid Release. " -BackgroundColor Red 
    Write-Host "Available memory: "$ava_mem"G" -ForegroundColor Yellow
    Write-Host "Available harddisk: "$ava_hd"G" -ForegroundColor Yellow
    Write-Host "Max memory: "$max_mem"G" -ForegroundColor Yellow
    Write-Host "Max harddisk: "$max_hd"G" -ForegroundColor Yellow
    Write-Host "Max ip subnet: "$max_ip"G" -ForegroundColor Yellow
    Write-Host "Job completed and resource released!"
}

#To Do : copy test result to WTT

#Analyse Test Result
$WinTestLogRepository = "$WorkingDir\TestLog"
$TestReportConfigFileName = "Win7WeeklyReport.config.xml"
$mix = "$clientOS-$serverOS-$workgroupDomain-$IPVersion-$ClientCPUArchitecture-$ServerCPUArchitecture"
powershell $WorkingDir\ScriptLib\Report-TestResult.ps1 $ProtocolName $Cluster $mix $WorkingDir $TestResultDir $WinTestLogRepository $SendTo $SendCC $TestReportConfigFileName "Weekly"

exit