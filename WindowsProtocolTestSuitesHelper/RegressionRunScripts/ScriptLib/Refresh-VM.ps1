#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Refresh-VM.ps1
## Purpose:        Copy Virtual Machine images from file server.
## Version:        1.1 (30 Oct, 2008)
##
##############################################################################

param(
[string]$srcVMPath = "\\wseatc-protocol\PETLabStore\VMLib", 
[string]$VMDir     = "D:\VM",
[string]$VMName    = "W2K8-x64-01"
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Refresh-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$srcVMPath = $srcVMPath" 
Write-Host "`$VMDir     = $VMDir" 
Write-Host "`$VMName    = $VMName" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script copies Virtual Machine images from file server."
    Write-host
    Write-host "Example: CopyAndImportVM.ps1 \\pt3controller\VM_Base d:\VM W2K8-x86-01"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($Args[0] -match '-(\?|(h|(help)))')
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

Write-Host "Copying $VMName: "
Write-Host "From: $srcVMPath "
Write-Host "To:   $VMDir" 

#----------------------------------------------------------------------------
# Copying by robocopy /MIR
# Copying is skipped if a file is existing in target directory, and keeps the same version with source file.
#----------------------------------------------------------------------------
$vmPathInServer = "$srcVMPath\$VMName"
$vmPathInHost   = "$VMDir\$VMName"

Write-Host "The path for the VM in file server:   $vmPathInServer " 
Write-Host "The path for the VM in local machine: $vmPathInHost " 

Write-host "Try to get all the snapshots (avhd) ..."
$avhds = [System.IO.Directory]::GetFiles($vmPathInServer, "*.avhd", [System.IO.SearchOption]::AllDirectories) 
foreach ($avhd in $avhds)
{
   Write-Host "Snapshot found: $avhd " -foregroundcolor yellow
   
   $arrAvhd           = $avhd.Split("\")
   $avhdName          = $arrAvhd[$arrAvhd.count-1]   
   $avhdDirectoryName = $arrAvhd[$arrAvhd.count-2]
   $avhdDirectoryFullPath = "$VMDir\$VMName\Snapshots\$avhdDirectoryName"
   
   Write-Host "The snapshot name is: $avhdName"
   Write-Host "The folder name for this snapshot is: $avhdDirectoryName" 
   Write-Host "The full path for the snapshot should be: $avhdDirectoryFullPath"
   
   if ([System.IO.Directory]::Exists($avhdDirectoryFullPath) -eq $FALSE)
   {
        Write-Host "The folder does not exist: $avhdDirectoryFullPath "
        Write-Host "Create folder..."
        md $avhdDirectoryFullPath 
   }
   
   $avhdOldPaths =[System.IO.Directory]::GetFiles($vmPathInHost, $avhdName, [System.IO.SearchOption]::AllDirectories) 
   foreach ($avhdOldPath in $avhdOldPaths)
   {
       if ($avhdOldPath -ne $null -and $avhdOldPath -ne "")
       {
           Write-Host "The local path for this snapshot: $avhdOldPath"

           $arrAvhdOldPath           = $avhdOldPath.Split("\")
           $avhdOldDirectoryName     = $arrAvhdOldPath[$arrAvhdOldPath.count-2]
           Write-Host "The folder name for this AVHD is: $avhdOldDirectoryName"
       
          if ($avhdOldDirectoryName -ne $avhdDirectoryName)
          {
              Write-Host "Moving snapshot..."
              Move-Item -Path "$avhdOldPath" -Destination "$avhdDirectoryFullPath\$avhdName"
              Write-Host "Moving snapshot done."
          }
          else
          {
              Write-Host "DO not need to move snapshot because they are in the same folder."
          }
       }
   }   
}
robocopy /MIR "$srcVMPath\$VMName" "$VMDir\$VMName " /XA:RO 2>&1 |Write-Host   #/NFL /NDL 

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Refresh-VM.ps1]..." 
$vhds = [System.IO.Directory]::GetFiles("$VMDir\$VMName\Virtual Hard Disks", "*.vhd", [System.IO.SearchOption]::AllDirectories) 
foreach($vhd in $vhds)
{
    # Mark vhd file as readonly
    attrib +R "$vhd"
}
Write-Host "EXECUTE [Refresh-VM.ps1] SUCCEED." -foregroundcolor green

exit
