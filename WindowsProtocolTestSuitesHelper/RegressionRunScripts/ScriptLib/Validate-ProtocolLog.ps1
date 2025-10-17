#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Validate-ProtocolLog.ps1
## Purpose:        Validate a test log and fix it if has not-colsed issue
## Version:        1.0 (12 Feb, 2009)
##
##############################################################################

param(
[string]$protocolLogFile
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Validate-ProtocolLog.ps1] ..." -foregroundcolor cyan
Write-Host "`$protocolLogFile = $protocolLogFile"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: validate a test log and fix it if has not-colsed issue."
    Write-host "Parm1: the test log file for validating, muse be absolutely path."
    Write-host
    Write-host "Example: .\Validate-ProtocolLog.ps1  C:\TestResults\MS-XXXX_Log.xml"
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
if ($protocolLogFile -eq $null -or $protocolLogFile -eq "")
{
    Throw "Parameter `$protocolLogFile is required."
}

#----------------------------------------------------------------------------
# Validate test log
#----------------------------------------------------------------------------

if((Test-Path "$protocolLogFile") -eq $false)
{
    Throw "$protocolLogFile is not exist."
}

$isBad = $false
$isNotClosed = $false
$errorMessage = ""

$logContent = Get-Content "$protocolLogFile"
try
{
    [XML]$xmlLogContent = $logContent
}
catch
{
    $isBad = $true
    $errorMessage  = $_.ToString()
    if($errorMessage.ToLower().Contains("the following elements are not closed:"))
    {
        $isNotClosed = $true
    }
}

#----------------------------------------------------------------------------
# Try to fix if bad
#----------------------------------------------------------------------------

if($isBad)
{
    if($isNotClosed)
    {
        "</LogEntries>"  | Out-File $protocolLogFile -Append -Encoding utf8
        "</TestLog>"  | Out-File $protocolLogFile -Append -Encoding utf8
    }
    else
    {
        Write-Host "The test log is in bad format and not be fixed, error: $errorMessage" -foregroundcolor Red
    }
}
else
{
    Write-Host "The test log is in good format" -foregroundcolor Green
}

#----------------------------------------------------------------------------
#  Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Validate-ProtocolLog.xml] ..." -foregroundcolor Yellow
if($isNotClosed)
{
    $logContent = Get-Content $protocolLogFile
    try
    {
        [XML]$xmlLogContent = $logContent
        Write-Host "The test log has been fixed" -foregroundcolor Green
    }
    catch
    {
        $errorMessage  = $_.ToString()
        Write-Host "The test log is in bad format and not be fixed, error: $errorMessage" -foregroundcolor Red
    }
}


