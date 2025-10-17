#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-WindowsUpdateLogsFromVHD.ps1
## Purpose:        Copy Windows Update Logs from VHD
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 8
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

param(
[string]$VhdPath="",
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
Start-Transcript -Path "$testLogDir\Copy-WindowsUpdateLogsFromVHD.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Check Parameters
#----------------------------------------------------------------------------
echo "`nCheck Parameters"
if($VhdPath -eq $null -or $VhdPath.Trim() -eq "")
{
    Write-Host "VhdPath could not be null or empty." -ForegroundColor Red
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
echo "======================================="
echo "Start Copy-WindowsUpdateLogsFromVHD.ps1"
echo "Input parameters:"
echo "VhdPath:  $VhdPath"
echo "testLogDir:  $testLogDir"
echo "======================================="

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
# Copy test logs
#----------------------------------------------------------------------------
if((Test-Path $drivePath) -eq $true)
{
    # Copy Configure files

    Copy-Item $drivePath\temp\InstallBranch.txt $testLogDir	-ErrorAction SilentlyContinue
    Copy-Item $drivePath\temp\InstallMethod.txt $testLogDir	-ErrorAction SilentlyContinue
    Copy-Item $drivePath\temp\WindowsUpdateConfig.xml $testLogDir	-ErrorAction SilentlyContinue

    # Copy CritFix Log Files
    copy-item $drivePath\temp\CritFix\*.log $testLogDir	-ErrorAction SilentlyContinue
    copy-item $drivePath\temp\CritFix\*.xml $testLogDir	-ErrorAction SilentlyContinue
	
	# Copy MTP Package Installation Verification Directory
	Copy-Item $driverPath\TestBin -Destination $testLogDir\TestBin -Recurse -ErrorAction SilentlyContinue

    # Copy common log files
    copy-item $drivePath\temp\*.log $testLogDir	-ErrorAction SilentlyContinue

    # Adding logs for all cached updates
    cmd /c tree /f $drivePath\temp\Cacheupdates > $testLogDir\Cacheupdates_TreeView.log    

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