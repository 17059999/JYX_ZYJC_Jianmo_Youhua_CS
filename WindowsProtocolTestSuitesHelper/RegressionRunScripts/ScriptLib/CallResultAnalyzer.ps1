Param (
[string]$vhdName = "10074.0.amd64fre.fbl_impressive.150424-1350_server_serverdatacenter_en-us.vhd",
[string]$protocolName = "RDP",
[string]$testResultFileFolder = "C:\Users\jingzl\Desktop\test\result",
[string]$reportFolder = "C:\Users\jingzl\Desktop\test\result",
[string]$exePath = "C:\Users\jingzl\Desktop\test\result\TestResultAnalyzerCmd.exe"
)

$vhdVersion = $vhdName.split('.')[0]

if($vhdVersion -eq $null -or $vhdVersion.Trim() -eq "")
{
	return 1;
}

if($testResultFileFolder -eq $null -or $testResultFileFolder.Trim() -eq "")
{
	return 1;
}

if($exePath -eq $null -or $exePath.Trim() -eq "")
{
	return 1;
}

if($reportFolder -eq $null -or $reportFolder.Trim() -eq "")
{
	$reportFolder = $testResultFileFolder
}

# runName
$runName = $protocolName + "_" + $vhdVersion + "_" + (Get-Date -UFormat "%Y-%m-%d").ToString()

CMD /C $exePath $testResultFileFolder $runName $reportFolder
