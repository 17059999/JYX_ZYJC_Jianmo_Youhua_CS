#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Allocate-Lock.ps1
## Purpose:        Allocate the process lock for the current process
## Version:        1.0 (28 April, 2011)
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows
##
##############################################################################

Write-Host "Allocating Lock." -ForegroundColor Yellow
$SignalFileFullName = "$Env:SystemDrive\allocate.vmlock.signal"
if ([System.IO.File]::Exists( $SignalFileFullName ) -eq $true)
{
	Write-Host "There is other Test Suite applied Lock, waiting ..." -ForegroundColor Yellow
}

$retryCount = 0
while ([System.IO.File]::Exists( $SignalFileFullName ) -eq $true)
{
	$retryCount++
	if ($retryCount -eq 60)
	{
		$retryCount = 0
		$NoNewLineIndicator = $False
	}
	else
	{
		$NoNewLineIndicator = $True
	}
	Write-Host "." -NoNewLine:$NoNewLineIndicator
    Start-Sleep -Seconds 1
}
Write-Host ""
Write-Host "Finish allocating lock" -ForegroundColor Green  
cmd /C ECHO ALLOCATING FINISHED>$Env:SystemDrive\allocate.vmlock.signal

exit