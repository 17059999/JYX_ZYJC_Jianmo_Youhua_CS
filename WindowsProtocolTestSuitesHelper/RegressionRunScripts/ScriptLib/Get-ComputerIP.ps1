#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-ComputerIP.ps1
## Purpose:        Retrive the IP address (IPv4 or IPv6) according to the VM role and index.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[String]$IPVersion, 
[String]$role, 
[int]$index,
[bool]$LocalComputer = $false,
[int]$Increment      = 0
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-ComputerIP.ps1] ..." -foregroundcolor cyan
Write-Host "`$IPVersion      = $IPVersion"
Write-Host "`$role           = $role"
Write-Host "`$index          = $index"
Write-Host "`$LocalComputer  = $LocalComputer"
Write-Host "`$Increment      = $Increment"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script will retrive the IP address according to the VM name"
    Write-host
    Write-host "Example: Get-ComputerIP.ps1 IPv4 SUT 1"
    Write-host "Example: Get-ComputerIP.ps1 IPv6 SUT 1 $true 4"
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
if ($role -eq $null -or $role -eq "")
{
    Throw "Parameter role is required."
}
if ($index -eq $null -or $index -eq "")
{
    Throw "Parameter index is required."
}

#----------------------------------------------------------------------------
# Get last sector of IPv4 address
#----------------------------------------------------------------------------
$tempIPv4 = "192.168.0."
if($LocalComputer)
{
    $tempIPv6 = "2008::"
}
else
{
    $tempIPv6 = "2008--"
}
[int]$lastNumberInIPv4 = 0

if ($role -eq "DC")
{
    $lastNumberInIPv4 = 200 + $index + $Increment
}
elseif ($role -eq "SUT")
{
    $lastNumberInIPv4 = 0 + $index + $Increment
}
else  # for endpoint

{
    $lastNumberInIPv4 = 100 + $index + $Increment
}

#----------------------------------------------------------------------------
# Get full IP address
#----------------------------------------------------------------------------
$retVal = ""
if ($IPVersion.Contains("IPv4"))
{
    $retVal = $tempIPv4 + $lastNumberInIPv4 
}

# for IPv6. See Microsoft KB: http://support.microsoft.com/kb/944007
else
{
    $lastNumberInIPv6 = [System.String]::Format("{0:X}", $lastNumberInIPv4)
    if($LocalComputer)
    {
        $retVal = $tempIPv6 + $lastNumberInIPv6
    }
    else
    {
        
        $retVal = $tempIPv6 + $lastNumberInIPv6 + ".ipv6-literal.net"
    }
    
}

Write-Host "The $IPVersion Address for $role (index $index) is $retVal."

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Get-ComputerIP.ps1] FINISHED."

return $retVal
