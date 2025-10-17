###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install_VisualStudio_Online.ps1
## Purpose:        Install Visual Studio online
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2012, Windows Server 2012 R2, Windows Server 2016, and later.
##
###########################################################################################

Param(
    [string] $MajorVersion = "2017",
    [string] $DetailVersion = "15.0"
)

$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent;
Push-Location $scriptPath
# Check if VS 2017 installed
$isVSInstalled = $false
try {
    $vsRegKey = Get-ItemProperty -Path HKLM:\SOFTWARE\Microsoft\VisualStudio\SxS\VS7 -Name $DetailVersion -ErrorAction SilentlyContinue
    if ($vsRegKey) {
        $isVSInstalled = $true
    }
}
catch {
    try {
        $vsRegKey = Get-ItemProperty -Path HKLM:\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7 -Name $DetailVersion -ErrorAction SilentlyContinue
        if ($vsRegKey) {
            $isVSInstalled = $true
        }
    }
    catch {
    }
}

if ($isVSInstalled) {
    Write-Host "VS $MajorVersion already exists, skip to install"
    Write-Host "Write signal file to system drive."
    cmd /c ECHO "InstallVisualStudio.Completed.signal" >$env:HOMEDRIVE\InstallVisualStudio.Completed.signal
}
else {
    Write-Host "Start to install VS $MajorVersion, start to wait..."
    cmd /c vs_community.exe --installPath C:\vs_community --add Microsoft.VisualStudio.Workload.CoreEditor --add Microsoft.VisualStudio.Workload.ManagedDesktop --add Microsoft.VisualStudio.Workload.NativeDesktop --passive --norestart

    [int] $timeout = 240
    [int] $vsProcessCount = 0
    do { 
        Write-Host ((Get-Date).ToString() + ": Start to sleep 30 seconds, then check if vs installer complete")
        Start-Sleep -Seconds 30
        
        $vsProcessCount = (Get-Process | Where-Object {$_.ProcessName -match "vs_installer"} | Measure-Object).Count
    
        $timeout--
    }while (($vsProcessCount -gt 0) -and ($timeout -gt 0))
    
    if ($timeout -eq 0) {
        Write-Error "Install vs $MajorVersion timeout"
    }
    else {
        Write-Host "Install vs $MajorVersion completed"
        Write-Host  "Write signal file to system drive."
        cmd /c ECHO "InstallVisualStudio.Completed.signal" >$env:HOMEDRIVE\InstallVisualStudio.Completed.signal
    }
}

Pop-Location