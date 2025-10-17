#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:            Release-IP.ps1
## Purpose:         Release a IP address resource back to host. This script recieves 
##				    a string of a IP address as an input parameter, release the available 
##					resource back to file Schedule-TestJob.config
## Version:         1.0 (1 June, 2011)
## Requirements:    Windows Powershell 2.0
## Supported OS:    Windows
##
##############################################################################
param(
    [string]$Ip
)

$resPath = Get-Location
$resconfig = [String]$resPath + "\Schedule-TestJob.config"
[xml]$res = get-content $resconfig
$IpNodes = $res.GetElementsByTagName("IpAddress")
$IsFound = "false"
foreach ($node in $IpNodes)
{
	if($node.GetAttribute("name") -eq $Ip)
	{
		$IsFound="true"
		$node.SetAttribute("value", "true")
		break
	}
}
if ($IsFound -eq "true")
{
	$res.Save($resconfig)
}
return $IsFound