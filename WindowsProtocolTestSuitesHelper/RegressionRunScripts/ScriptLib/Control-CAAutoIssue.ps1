#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Control-CAAutoIssue.ps1
## Purpose:        Enable or Disable the Certificate Authority (CA) to issue certificate automatically. 
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$machineDnsName,
[string]$CAName,
[string]$autoIssue = "Enable"
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Control-CAAutoIssue.ps1] ..." -foregroundcolor cyan
Write-Host "`$machineDnsName = $machineDnsName"
Write-Host "`$CAName = $CAName"
Write-Host "`$autoIssue = $autoIssue"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Enable or Disable the Certificate Authority (CA) to issue certificate automatically. "
    Write-host "Parm1: CA server's DNS name. (Required)"
    Write-host "Parm2: CA name. (Required)"
    Write-host "Parm3: Enable or Disable auto issue. (Optional, Default value: `"Enable`", Legal value: `"Enable`"|`"Disable`")"
    Write-host
    Write-host "Example: Control-CAAutoIssue.ps1 SUT01.contoso.com contoso-SUT01-CA Enable"
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
if ($machineDnsName -eq $null -or $machineDnsName -eq "")
{
    Throw "Parameter machineDnsName is required."
}
if ($CAName -eq $null -or $CAName -eq "")
{
    Throw "Parameter CAName is required."
}
if(!($autoIssue.ToLower().Equals("enable") -or $autoIssue.ToLower().Equals("disable")))
{
    Throw "Parameter autoIssue should be `"Enable`"|`"Disable`"."
}

#----------------------------------------------------------------------------
# Enable/Disable auto issue
#----------------------------------------------------------------------------
$massage = "Try to enable CA: $machineDnsName\$CAName auto issue certificate ..."
$autoIssueSwitch = 1
if ($autoIssue.ToLower().Equals("disable"))
{
    $massage = "Try to disable CA: $machineDnsName\$CAName auto issue certificate ..."
    $autoIssueSwitch = 0
}
Write-Host "$massage"
$result = certutil.exe -config $machineDnsName\$CAName -setreg policy\RequestDisposition $autoIssueSwitch

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
Write-Host "Verifying [Control-CAAutoIssue.ps1] ..." -foregroundcolor Yellow
if (($result -ne $null) -and ($result.getType().isArray) -and ($result.Length -gt 1) -and ($result[$result.Length-2].Contains("successfully")))
{
    Write-Host "Execution successfully. The CertSvc service may need to be restarted for changes to take effact." -foregroundcolor Green
}
else
{
    Throw "EXECUTE [Control-CAAutoIssue.ps1] FAILED. Error message: $result"
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Control-CAAutoIssue.ps1] SUCCEED." -foregroundcolor Green

