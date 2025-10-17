#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Decompress-Cab.ps1
## Purpose:        Decompress folder from a cab made by Compress-Cab.ps1
## Version:        1.0 (21 Dec ,2009)
##
##############################################################################
param(
[string]$cabfile,             #cab file name (need full path)
[boolean]$remove    = $true   #whether to delete original cab file
)

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script decompresses the folder from a cab made by Compress-Cab.ps1"
    Write-host
    Write-host "First Parameter             `t:Root path  : The full path of the folder."
    Write-host "Second parameter            `t:Remove     : Remove or not remove the orginal folder." 
    Write-host
    Write-host 'Example: Decompress-Cab.ps1 c:\folder.cab $true'
    Write-host
}

#--------------------------------------------------------
# Show help if required
#--------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#-----------------------------------
# process of decompressing
#-----------------------------------
$file=New-Object System.IO.FileInfo($cabfile)
if($file.Exists -eq $false)
{
	throw "Cab file not found. Need full path."
}
Write-Host "begin to decompress..." -ForegroundColor Yellow

#expand files in the cab to a new folder
$parentpath=$file.DirectoryName
$newrootfolder=$file.Name.substring(0,$file.Name.LastIndexOf('.'))
pushd $parentpath
cmd /c md $newrootfolder
cmd /c expand $cabfile -F:* $newrootfolder

#place the files as their original location
pushd $newrootfolder
$logfile=gc log.txt
foreach($line in $logfile)
{
	$tmpfile=$line.Substring($line.LastIndexOf('\')+1)
	$replace=$line.Substring(0,$line.IndexOf($newrootfolder)+$newrootfolder.Length)
	$tmpfolder=$line.Replace($replace,'.')
	$tmpfolder=$tmpfolder.Substring(0,$tmpfolder.LastIndexOf('\'))
	
	cmd /c md $tmpfolder                 2>&1 | Write-Host
	cmd /c move $tmpfile $tmpfolder      2>&1 | Write-Host				
}
cmd /c del log.txt                       2>&1 | Write-Host
popd

#remove the cab file
if($remove -eq $true)
{
	cmd /c del $cabfile               2>&1 | Write-Host
}

Write-Host "Decompression Done!" -ForegroundColor Green
popd


