#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           CopyScriptLibToShare.ps1
## Purpose:        Copy ScriptLib and VSTORMLITE to share folder.
## Requirements:   Windows Powershell 2.0 CTP2
## Supported OS:   Windows 7 and later versions
##
##############################################################################

param(
		[switch]$UpdateScriptLib,
		[Parameter(Mandatory=$true)]
		[string]$ShareFolderPath
	)

$InitialInvocation       = $MyInvocation
$InvocationFullPath      = $InitialInvocation.MyCommand.Definition
$InvocationPath          = Split-Path -Parent $InvocationFullPath

if($UpdateScriptLib){
	Write-Host "Start to copy ScriptLib" -ForegroundColor Green
	robocopy $InvocationPath\..\ScriptLib $ShareFolderPath\ScriptLib /E
	if($lastexitcode -ge 0 -and $lastexitcode -le 7) {
		Write-Host "Copy ScriptLib succeeded" -ForegroundColor Green
	}
	else {
		throw "Robocopy failed with exit code:$lastexitcode"
	}

	Write-Host "Start to copy LocalRegression" -ForegroundColor Green
	robocopy $InvocationPath\..\LocalRegression $ShareFolderPath\LocalRegression /E
	if($lastexitcode -ge 0 -and $lastexitcode -le 7) {
		Write-Host "Copy LocalRegression succeeded" -ForegroundColor Green
		exit 0
	}
	else {
		throw "Robocopy failed with exit code:$lastexitcode"
	}

	Write-Host "Start to copy VSTORMLITE" -ForegroundColor Green
	robocopy $InvocationPath\..\VSTORMLITE $ShareFolderPath\VSTORMLITE /E
	if($lastexitcode -ge 0 -and $lastexitcode -le 7) {
		Write-Host "Copy VSTORMLITE succeeded" -ForegroundColor Green
		exit 0
	}
	else {
		throw "Robocopy failed with exit code:$lastexitcode"
	}
}else{
	write-host "Do not need update ScriptLib and VSTORMLITE" -ForegroundColor Green
}

