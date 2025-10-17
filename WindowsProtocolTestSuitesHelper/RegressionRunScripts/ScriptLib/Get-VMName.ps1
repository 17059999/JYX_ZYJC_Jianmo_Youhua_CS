#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-VMName.ps1
## Purpose:        Retrive the VM name according to VM's parameters.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[String]$OS, 
[String]$CPUArchitecture, 
[String]$Role, 
[String]$index
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-VMName.ps1] ..." -foregroundcolor cyan
Write-Host "`$OS              = $OS"
Write-Host "`$CPUArchitecture = $CPUArchitecture"
Write-Host "`$Role            = $Role"
Write-Host "`$index           = $index"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script will retrive the VM name"
    Write-host
    Write-host "Example: Get-VMName.ps1 Win7 x64 SUT 1"
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
if ($OS -eq $null -or $OS -eq "")
{
    Throw "Parameter OS is required."
}
if ($CPUArchitecture -eq $null -or $CPUArchitecture -eq "")
{
    Throw "Parameter CPUArchitecture is required."
}
if ($Role -eq $null -or $Role -eq "")
{
    Throw "Parameter Role is required."
}
if ($index -eq $null -or $index -eq "")
{
    Throw "Parameter index is required."
}

#----------------------------------------------------------------------------
# Get VM name
#----------------------------------------------------------------------------
$temp = $OS + "-" + $CPUArchitecture + "-"
if ($Role -eq "DC")
{
    $retVal = $temp + $Role
}
else
{
    $retVal = $temp + $index
}
Write-Host "Retrived VM name is $retVal"

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Get-VMName.ps1] FINISHED."

return $retVal