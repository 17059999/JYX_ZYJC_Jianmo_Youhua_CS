#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:            Allocate-IP.ps1
## Purpose:         Allocate a IP address resource from host. This script reads 
##				    the available resource from file "Schedule-TestJob.config" 
##					and allocate IP address for the job. 
## Version:         1.0 (1 June, 2011)
## Requirements:    Windows Powershell 2.0
## Supported OS:    Windows
##
##############################################################################

$resPath = Get-Location
$resconfig = [String]$resPath + "\Schedule-TestJob.config"
[xml]$res = get-content $resconfig
if ($res -eq $null)
{
    return $null
}
$IpNodes = $res.GetElementsByTagName("IpAddress")
$IsFound = "false"
foreach ($node in $IpNodes)
{
	if($node.GetAttribute("value") -eq "true")
	{
		$AllocIp=$node.GetAttribute("name")
		$IsFound="true"
		$node.SetAttribute("value", "false")
		break
	}
	$AllocIp=""
}
if ($IsFound -eq "true")
{
	$res.Save($resconfig)
}
return $AllocIp