#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Is-InDomain.ps1
## Purpose:        Checking that current computer system is joined in a domain or not.
## Version:        1.0 (25 Jun, 2008)
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Is-InDomain.ps1] ..." -foregroundcolor cyan

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Check current computer system is in a domain or not."
    Write-host "       If it is in a domain, the return value will be bool type `$TRUE. Otherwise, the return value will be bool type `$FALSE."    
    Write-host
    Write-host "Example: `$domainFlag = Is-InDomain.ps1"
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
# Check domain role
#----------------------------------------------------------------------------
$computerSysObj = Get-WmiObject -Class Win32_ComputerSystem
if ($computerSysObj -eq $null)
{
    Throw "Error: Cannot get WMI object."
}

$domainRole = $computerSysObj.DomainRole
$returnValue = $False
switch ($domainRole)
{
    0
    {
        Write-Host "Current computer is a Standalone Workstation." -ForegroundColor Green
        $returnValue =  $False
    }
    1
    {
        Write-Host "Current computer is a Domain Workstation."  -ForegroundColor Green
        $returnValue =  $True
    }
    2
    {
        Write-Host "Current computer is a Standalone Server."  -ForegroundColor Green
        $returnValue =  $False
    }
    3
    {
        Write-Host "Current computer is a Member Server."  -ForegroundColor Green
        $returnValue =  $True
    }
    4
    {
        Write-Host "Current computer is a Backup Domain Controller."  -ForegroundColor Green
        $returnValue =  $True
    }
    5
    {
        Write-Host "Current computer is a Primary Domain Controller."  -ForegroundColor Green
        $returnValue =  $True
    }
    default
    {
        Write-Host "Unknown domain role." -ForegroundColor Yellow
        $returnValue =  $False
    }
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Is-InDomain.ps1] SUCCEED." -foregroundcolor Green
return $returnValue

