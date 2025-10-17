#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-VMLog.ps1
## Purpose:        Copy logs from test VM
## Requirements:   Windows Powershell 2.0, 3.0
## Supported OS:   Windows Server 2012, Windows Server 2012R2
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
[Parameter(Mandatory=$true)]
[ValidateNotNullOrEmpty()]
[string]$drivePath,
[Parameter(Mandatory=$true)]
[ValidateNotNullOrEmpty()]
[string]$testLogDir,
[string]$postscript)

Function CopyVMLog($drivePath,$testLogDir,$postscript)
{
    #----------------------------------------------------------------------------
    # Copy test logs
    #----------------------------------------------------------------------------
    Write-Host "Copy Test logs"
    if(!(Test-Path $testLogDir))
    {
        CMD /C MKDIR $testLogDir 2>&1 | Write-Host 
    }
    if(Test-Path $drivePath)
    {
        # Copy test logs and important scripts
        Copy-Item $drivePath\*.log $testLogDir	-ErrorAction SilentlyContinue
        Copy-Item $drivePath\temp\*.log $testLogDir	-ErrorAction SilentlyContinue
        Copy-Item $drivePath\temp\*.txt $testLogDir	-ErrorAction SilentlyContinue
        Copy-Item $drivePath\temp\controller.ps1 $testLogDir	-ErrorAction SilentlyContinue
        Copy-Item $drivePath\temp\install.ps1 $testLogDir	-ErrorAction SilentlyContinue
        if($postscript -ne $null)
        {
            $postscript.Split(";") | foreach{Copy-Item $drivePath\temp\$_  $testLogDir -ErrorAction SilentlyContinue}
        }
        Copy-Item $drivePath\temp\post.ps1 $testLogDir	-ErrorAction SilentlyContinue
        Copy-Item $drivePath\temp\setup.xml $testLogDir	-ErrorAction SilentlyContinue
        Copy-Item $drivePath\temp\Protocol.xml $testLogDir	-ErrorAction SilentlyContinue
        Copy-Item $drivePath\temp\unattend.xml $testLogDir	-ErrorAction SilentlyContinue

        Write-Host "Finish copy log files" -ForegroundColor Green
        sleep 5
    }
    else
    {
	    Write-Host "Cannot find VHD image path: $drivePath"
	    sleep 10
    }
}


CopyVMLog $drivePath $testLogDir $postscript
