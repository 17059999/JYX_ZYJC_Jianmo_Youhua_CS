#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Schedule-TestJob.ps1
## Purpose:        Schedule test jobs on a big server with resource allocation/release. 
##                 Alloc-Resource method reads the available resource from file "Schedule-TestJob.config" and allocate the resource for the job. 
##                 Release-Resource method releases the allocated resouce back to file "Schedule-TestJob.config".
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

#For resource allocation
[int]$memsize,
[int]$hdsize,
[int]$ipsize,

#For SQL/Exchange productline
[string]$CustomizeScenario="",

[String]$SendTo,
[String]$SendCC

)

# The resouce config file. 
$resPath = Get-Location
$resconfig = [String]$resPath + "\Schedule-TestJob.config"

#----------------------------------------------------------------------------
# Function: Alloc-Resource
# Usage   : Allocates the memory and harddisk for the job. 
#           e.g Alloc-Resource -memsize 5 -hdsize 10;
#----------------------------------------------------------------------------
function Alloc-Resource([int]$memsize, [int]$hdsize, [int]$ipsize)
{
	[xml]$res = get-content $resconfig
	if ($res -eq $null)
	{
		#If the resource config file does not exist, then continue without resource allocation.
		return $true
	}
	[int]$ava_mem=$res.Resource.Available.Memory;
	[int]$ava_hd=$res.Resource.Available.Harddisk;
	[int]$ava_ip=$res.Resource.Available.SubIp;
	# succeed to allocate the resource for the task.
	if( ($memsize -le $ava_mem) -and ($hdsize -le $ava_hd) -and($ipsize -le $ava_ip))
	{
		$ava_mem = $ava_mem-$memsize;
		$ava_hd = $ava_hd-$hdsize;
		$ava_ip = $ava_ip-$ipsize;
		$res.Resource.Available.Memory=$ava_mem.toString()
		$res.Resource.Available.Harddisk=$ava_hd.toString()
		$res.Resource.Available.SubIp=$ava_ip.toString()
		$res.Save($resconfig);
		Write-Host "Successfully allocate "$memsize"G memory, "$hdsize"G harddisk and "$ipsize" ip subnet." -ForegroundColor Green
		return $true
	}
	# fail to allocate the resouce for the job task.
	Write-Host "Not Enough resouce." -BackgroundColor Yellow 
	Write-Host "Available memory: "$ava_mem"G" -ForegroundColor Yellow
	Write-Host "Available harddisk: "$ava_hd"G" -ForegroundColor Yellow
	Write-Host "Available ip subnet: "$ava_ip"G" -ForegroundColor Yellow
	return $false
}

$alloc=Alloc-Resource -memsize $memsize -hdsize $hdsize -ipsize $ipsize
while($alloc -ne $true)
{
	[System.Threading.Thread]::Sleep(1000*60);
	$alloc=Alloc-Resource -memsize $memsize -hdsize $hdsize -ipsize $ipsize
}

#$currentdir=$myinvocation.mycommand.path.trim($myinvocation.mycommand.Name)
# use WMI to start up a new process to run the real job
$objWMI= [WmiClass]"\\localhost\root\cimv2:Win32_Process"


# $args is passed from WTT and will be passed to Start-JobAgent.ps1
$objWMI.Create("powershell.exe .\Start-JobAgent.ps1 $protocolName $cluster $srcVMPath $srcScriptLibPath $srcToolPath $srcTestSuitePath $ISOPath $clientOS $serverOS $workgroupDomain $IPVersion $ClientCPUArchitecture $ServerCPUArchitecture $workingDir $VMDir $testResultDir $userNameInVM $userPwdInVM $domainInVM $testDirInVM $memsize $hdsize $ipsize $CustomizeScenario $SendTo $SendCC", "$workingDir\ScriptLib")

Write-Host "Real job started in another process. "
Write-Host "Schedule success! Return now."

exit