#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-VirtualDirectory.ps1
## Purpose:        Create a virtual directory on the default website.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

Param(
[string]$VDName = $null, 
[string]$VDRoot = $null
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-VirtualDirectory.ps1] ..." -foregroundcolor cyan
Write-Host "`$VDName = $VDName"
Write-Host "`$VDRoot = $VDRoot"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script used to creat a new virtual directory on the default website"
    Write-host
    Write-host "Example: Create-VirtualDirectory.ps1 Test C:\Test"
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
if ($VDName -eq $null -or $VDName -eq "")
{
    Throw "the name of virtual directory is required."
}

if ($VDRoot -eq $null -or $VDRoot -eq "")
{
    Throw "the root path of virtual directory is required."
}

$RootPathExist = Test-Path -Path $VDRoot

if($RootPathExist -eq $False)
{
    Throw "Root path is invalid."
}

#----------------------------------------------------------------------------
# Config the Virtual Directory with the config file
#----------------------------------------------------------------------------
$osVersion = .\Get-OsVersion.ps1

if($osVersion -eq "XP")
{
    $sysDrive = $env:HOMEDRIVE
    pushd "$sysDrive\Inetpub\AdminScripts"
    cscript adsutil.vbs create W3SVC/1/root/$VDName "IIsWebVirtualDir"
    cscript adsutil.vbs set W3SVC/1/root/$VDName/Path "$VDRoot"
    cscript adsutil.vbs set W3SVC/1/root/$VDName/AppRoot "/LM/W3SVC/1/Root/$VDName"
}
else
{
    cmd.exe /c "CScript //H:CScript" 2>&1 | Write-Host
    cmd.exe /c "iisvdir /create w3svc/1/ROOT  $VDName $VDRoot" 2>&1 | Write-Host
}

#----------------------------------------------------------------------------
# Verfiy if create success
#----------------------------------------------------------------------------
Write-Host "Verifying [Create-VirtualDirectory.ps1] ..." -foregroundcolor yellow
$IsSuccess = $false
if($osVersion -eq "XP")
{
    $s = cscript adsutil.vbs Get W3Svc/1/root/$VDName/AppRoot

    if($s[3] -eq "AppRoot                         : (STRING) `"/LM/W3SVC/1/Root/$VDName`"")
    {
        $IsSuccess = $true
    }
    popd
}
else
{
    $IISWebVDObjArr = Get-WmiObject -Class IISWebVirtualDir -Namespace "root/MicrosoftIISV2"
    

    if($IISWebVDObjArr)
    {
        foreach($IISWebVDObj in $IISWebVDObjArr)
        {
            if($IISWebVDObj.Name -eq "W3SVC/1/ROOT/$VDName")
            {
                $IsSuccess = $true
                break
            }
        }
    }
}
if($IsSuccess)
{
    Write-Host "$VDName virtual directory create successfully." -ForegroundColor green
}
else
{
    Throw "EXECUTE [Create-VirtualDirectory.ps1] FAILED."
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Create-VirtualDirectory.ps1] SUCCEED." -foregroundcolor green

exit





