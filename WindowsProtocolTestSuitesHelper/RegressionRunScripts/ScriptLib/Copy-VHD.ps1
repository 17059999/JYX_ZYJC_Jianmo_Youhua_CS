#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-VHD.ps1
## Purpose:        Copy Virtual Machine images from file server.
## Version:        1.0 (9 Apr, 2010)
##
##############################################################################

param(
[string]$srcVMPath, 
[string]$VMDir,
[string]$VMName,
[string]$ProtocolName
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Copy-VHD.ps1]..." -foregroundcolor cyan
Write-Host "`$srcVMPath    = $srcVMPath" 
Write-Host "`$VMDir        = $VMDir" 
Write-Host "`$VMName       = $VMName" 
Write-Host "`$ProtocolName = $ProtocolName" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script copies Virtual Machine images from file server."
    Write-host
    Write-host "Example: Copy-VHD.ps1 \\pt3controller\VM_Base d:\VM W2K8-x86-01 MS-SMB2"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($srcVMPath -eq $null -or $srcVMPath -eq "")
{
    Throw "Parameter srcVMPath is required."
}
if ($VMDir -eq $null -or $VMDir -eq "")
{
    Throw "Parameter VMDir is required."
}
if ($VMName -eq $null -or $VMName -eq "")
{
    Throw "Parameter VMName is required."
}
if ($ProtocolName -eq $null -or $ProtocolName -eq "")
{
    Throw "Parameter $ProtocolName is required."
}

$VHD = "$VMName.vhd"

Write-Host "Copying $VHD from $srcVMPath to $VMDir ..." 
#----------------------------------------------------------------------------
# Copying is skipped if a file is existing in target directory, and keeps the same version with source file.
#----------------------------------------------------------------------------
robocopy /NFL /NDL "$srcVMPath\$VMName\Virtual Hard Disks" "$VMDir\BaseVHD" $VHD 2>&1 |Write-Host

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Copy-VHD.ps1]..." 
$vhdFile = "$VMDir\BaseVHD\$VMName.vhd"
if ([System.IO.File]::Exists($vhdFile) -ne $true)
{
    Throw "EXECUTE [Copy-VHD.ps1] FAILED. The VHD file cannot be found in the destination folder."
}
else
{
    # Mark vhd file as readonly
    attrib +R "$vhdFile"
    Write-Host "EXECUTE [Copy-VHD.ps1] SUCCEED." -foregroundcolor green
}

exit
