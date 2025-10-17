#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           InstallDotNetFx3.ps1
## Purpose:        Destroy and Remove VM folders
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 8
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
[string]$VhdPath="",
[string]$DotNetSource="",
[string]$testLogDir= ""
)

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction SilentlyContinue | Out-Null
if(!(Test-Path $testLogDir))
{
    md $testLogDir
}
Start-Transcript -Path "$testLogDir\InstallDotNetFx3.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Check Parameters
#----------------------------------------------------------------------------
echo "`nCheck Parameters"
if($VhdPath -eq $null -or $VhdPath.Trim() -eq "")
{
    Write-Host "VhdPath could not be null or empty." -ForegroundColor Red
    return 1;
}
if($DotNetSource -eq $null -or $DotNetSource.Trim() -eq "")
{
    Write-Host "DotNetSource could not be null or empty." -ForegroundColor Red
    return 1;
}
if($testLogDir -eq $null -or $testLogDir.Trim() -eq "")
{
    Write-Host "testLogDir could not be null or empty." -ForegroundColor Red
    return 1;
}

#----------------------------------------------------------------------------
# Start Script
#----------------------------------------------------------------------------
echo "Start InstallDotNetFx3.ps1"
echo "Input parameters:"
echo "VhdPath:$VhdPath"
echo "DotNetSource:$DotNetSource"
echo "----"

#----------------------------------------------------------------------------
# Get Available Drive Letter
#----------------------------------------------------------------------------
$driveLetter = ""
$drivePath = ""
foreach ($letter in [char[]]([char]'O'..[char]'Z')) 
{ 
    $driveLetter = $letter
    $drivePath = $letter + ":"
    $l = get-wmiobject win32_logicaldisk | where {$_.DeviceID -eq $drivePath}
    if ($l -eq $null -and (Test-Path -path $drivePath) -eq $false)
    { 
        break 
    } 
}
echo "`nGet available driver letter: $driveLetter"
	
#----------------------------------------------------------------------------
# Attach VHD to disk manager
#----------------------------------------------------------------------------
echo "`nAttach VHD to disk manager"
$diskpartScript = @()
$diskpartScript += "select vdisk file=$VhdPath"
$diskpartScript += "attach vdisk noerr"
$diskpartScript += "attributes disk clear readonly"
$diskpartScript += "select partition 1"
$diskpartScript += "list partition"
$diskpartScript += "active"
$diskpartScript += "rescan"
$diskpartScript += "select partition 1"
$diskpartScript += "select volume"
$diskpartScript += "list volume"
$diskpartScript += "assign letter=$driveLetter"
$diskpartScript += "rescan"
$diskpartScript += "exit"

echo "Execute diskpart script: "
$diskpartScript | % {echo $_}

$diskpartScript | diskpart 
sleep 10

#----------------------------------------------------------------------------
# Install .net 3.5 framework
#----------------------------------------------------------------------------
if((Test-Path $drivePath) -eq $true)
{
	echo "`nInstall .net 3.5 framework"    
	CMD /C dism /image:$drivePath /enable-feature /featurename:NetFx3 /All /Source:$DotNetSource /LogLevel:3 /LogPath:"$testLogDir\InstallDotNetFx3.dism.log"
}
else
{
	echo "`nCannot find VHD image path: $drivePath"
}
sleep 10

#----------------------------------------------------------------------------
# Detach VHD
#----------------------------------------------------------------------------
echo "`nDetach VHD"
$diskpartScript = $null
$diskpartScript = @()
$diskpartScript += "select vdisk file=$VhdPath"
$diskpartScript += "select partition 1"
$diskpartScript += "remove letter=$driveLetter"
$diskpartScript += "detach vdisk"
$diskpartScript += "rescan"
$diskpartScript += "exit"

echo "Execute diskpart script: "
$diskpartScript | % {echo $_}

$diskpartScript | diskpart
sleep 5

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Stop-Transcript
exit 0