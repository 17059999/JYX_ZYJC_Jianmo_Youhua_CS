#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Compress-Cab.ps1
## Purpose:        Compress folder to a cab
## Version:        1.0 (21 Dec ,2009)
##
##############################################################################
param(
[string]$rootpath,            #root directory (need full path)
[boolean]$remove    = $true   #whether to delete original folder
)

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script compress the folder containing multiple files to a cab"
    Write-host
    Write-host "First Parameter             `t:Root path  : The full path of the folder."
    Write-host "Second parameter            `t:Remove     : Remove or not remove the orginal folder." 
    Write-host
    Write-host 'Example: Compress-Cab.ps1 c:\folder $true'
    Write-host
}

#---------------------------------------------------------------------
# Show help if required
#---------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#---------------------------------------------------------------------------
# Function Relocation: move all files in subfolders to the root folder,
#                      and record their orignal location
#---------------------------------------------------------------------------
function Relocation($dir)
{
	$subdirarray=$dir.GetDirectories()
	if($subdirarray.Length -ne 0)
	{
		foreach($subdir in $subdirarray)
		{
			Relocation($subdir)
		}	
	}
	
	$files=$dir.GetFiles()
	if($files.Length -ne 0)
	{
		foreach($file in $files)
		{
			Add-Content $file.FullName -path $log -Encoding Ascii
			$file.MoveTo("$rootpath\$file")		
		}		
	}
	if($dir.GetFiles().Length -eq 0)
	{
		$dir.Delete()
	}
}

#-----------------------------------
# process of compressing
#-----------------------------------
$log="$rootpath\log.txt"
$rootdir=New-Object System.IO.DirectoryInfo($rootpath)
if($rootdir.Exists -eq $false)
{
	throw "Root path is invalid!"
}
Write-Host "begin to compress..." -ForegroundColor Yellow

#traverse the subfolders and relocation files to the root folder
$subdirarray=$rootdir.GetDirectories()
if($subdirarray.Length -ne 0)
{
	foreach($subdir in $subdirarray)
	{
		Relocation($subdir)
	}
}

#make a list of all the files for makecab.exe
$parentroot=$rootdir.Parent.FullName
pushd $parentroot

$filelist="$parentroot\list.txt"
$newfiles=$rootdir.GetFiles()
foreach($file in $newfiles)
{
	Add-Content $file.FullName -path $filelist -Encoding Ascii
}

#compress via makecab.exe and clean the temp files
cmd /c makecab /F $filelist              2>&1 | Write-Host
cmd /c copy disk1\1.cab .\1.cab          2>&1 | Write-Host
cmd /c rd /S /Q .\disk1                  2>&1 | Write-Host
cmd /c del setup.inf                     2>&1 | Write-Host
cmd /c del setup.rpt                     2>&1 | Write-Host

$newcabname=$rootdir.Name+".cab"

cmd /c move 1.cab $newcabname            2>&1 | Write-Host
cmd /c del $filelist                     2>&1 | Write-Host

#remove the original folder
if($remove -eq $true)
{
	cmd /c rd /S /Q $rootpath           2>&1 | Write-Host
}

Write-Host "$newcabname is successfully compressed." -ForegroundColor Green	
popd
