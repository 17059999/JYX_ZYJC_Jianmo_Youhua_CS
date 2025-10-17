##############################################################################
## 
## Microsoft Windows Powershell Scripting
## File:            Config-UAC.PS1
## Purpose:         Enable or Disable User access control(UAC) (will take effect after restart).
## Version:         1.1 (26 Jun 2008)
##
##############################################################################

param(
[string]$enable
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-UAC.PS1]..." -foregroundcolor cyan
Write-Host "`$enable = $enable" 

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host
    Write-host "This PS1 script will enable or disable User access control(UAC) (will take effect after restart). Curently UAC is supported in Vista & 2K8 OS"
    Write-host
    Write-host "Example: Config-UAC.PS1 Enable"
    Write-host
}

#----------------------------------------------------------------------------
#Verify Required parameters
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    write-host 
    write-host
    show-scriptusage 
    return
}

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($enable -eq $null -or $enable -eq "")
{
    Throw "Parameter `$enable is required."
}

$expectedEnableLUA = 0
if($enable -eq "Enable")
{
    $expectedEnableLUA = 1
}
elseif($enable -eq "Disable")
{
    $expectedEnableLUA = 0
}
else
{
    Throw "Parameter `$enable SHOULD be only Enable or Disable."
}

#--------------------------------------------------------------------------
#Check and modify user access control value in registry
#--------------------------------------------------------------------------
$orgEnableLUA = get-ItemProperty -Path registry::HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\policies\system -Name enableLUA
if ($orgEnableLUA.EnableLUA -eq $expectedEnableLUA)
{ 
    Write-host "User Access Control is already $enable." -foregroundcolor green
}
else
{
    Set-ItemProperty -Path registry::HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\policies\system -Name EnableLUA -Value $expectedEnableLUA
}

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Config-UAC.ps1]..." -foregroundcolor yellow
$newEnableLUA = get-ItemProperty -Path registry::HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\policies\system -Name enableLUA
if ($newEnableLUA.EnableLUA -eq $expectedEnableLUA) 
{
    write-host "EXECUTE [Config-UAC.ps1] SUCCEED." -foregroundcolor green
}
Else
{
    throw "EXECUTE [Config-UAC.ps1] FAILED."
}

exit