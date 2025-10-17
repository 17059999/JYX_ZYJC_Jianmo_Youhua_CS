#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Verify-NetworkEnvironment.ps1
## Purpose:        Verify the network environment, such as IP Address, disable Firewall...
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[String]$IPVersion,
[String]$workgroupDomain
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Verify-NetworkEnvironment.ps1] ..." -foregroundcolor cyan
Write-Host "`$IPVersion       = $IPVersion"
Write-Host "`$workgroupDomain = $workgroupDomain"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script will verify the network environment, such as IP Address, Firewall..."
    Write-host
    Write-host "Example: Verify-NetworkEnvironment.ps1 IPv4 Domain"
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
if ($IPVersion -eq $null -or $IPVersion -eq "")
{
    Throw "Parameter IPVersion is required."
}
if ($workgroupDomain -eq $null -or $workgroupDomain -eq "")
{
    Throw "Parameter role is required."
}

#----------------------------------------------------------------------------
# Print current IP Config information
#----------------------------------------------------------------------------
Write-Host  "IPConfig information:"
ipconfig.exe 2>&1 |Write-Host

#----------------------------------------------------------------------------
# Verify domain
#----------------------------------------------------------------------------
Write-Host "Run WMI Query to verify the domain/workgroup:"
get-WmiObject win32_computersystem

if ($workgroupDomain -eq "Domain")
{
    # after joining domain and after computer restarting, the firewall will be enabled. 
    # disable firewall here.

    Write-Host "Turn off firewall after domain joined..."  
    netsh.exe firewall set opmode disable 2>&1 |Write-Host   
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Verify-NetworkEnvironment.ps1] FINISHED."
