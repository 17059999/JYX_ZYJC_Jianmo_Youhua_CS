#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-TestResult.ps1
## Purpose:        Copy test results from VM (Client VM and Server VM) to VM host.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[string]$srcComputerName, 
[string]$srcResultDir,
[string]$destPath, 
[string]$usr, 
[string]$pwd
)

# Write Call Stack
if($function:EnterCallStack -ne $null)
{
	EnterCallStack "Copy-TestResult.ps1"
}

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Copy-TestResult.ps1] ..." -foregroundcolor cyan
Write-Host "`$srcComputerName = $srcComputerName"
Write-Host "`$srcResultDir    = $srcResultDir"
Write-Host "`$destPath        = $destPath"
Write-Host "`$usr             = $usr"
Write-Host "`$pwd             = $pwd"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script copies test results from a source computer to local."
    Write-host
    Write-host "Example: Copy-TestResult.ps1 SUT01 c$\Test\TestResult d:\TestResult username password"
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
if ($srcComputerName -eq $null -or $srcComputerName -eq "")
{
    Throw "Parameter srcComputerName is required."
}
if ($srcResultDir -eq $null -or $srcResultDir -eq "")
{
    Throw "Parameter srcResultDir is required."
}
if ($destPath -eq $null -or $destPath -eq "")
{
    Throw "Parameter destPath is required."
}

#----------------------------------------------------------------------------
# Using global username/password when caller doesnot provide.
#----------------------------------------------------------------------------
if ($usr -eq $null -or $usr -eq "")
{
    $usr = $global:usr
    $pwd = $global:pwd
}

#----------------------------------------------------------------------------
# Make username prefixed with domain/computername
#----------------------------------------------------------------------------
#if ($usr.IndexOf("\") -eq -1)
#{
#    if ($global:domain  -eq $null -or $global:domain -eq "")
#    {
#        $usr = "$srcComputerName\$usr"
#    }
#    else
#    {
#        $usr = "$global:domain\$usr"
#    }
#}
#[v-xich]: Remove this, we don't use domain account as default.

#----------------------------------------------------------------------------
# Find the root of sharing on remote computer
#----------------------------------------------------------------------------
$srcResultDir = $srcResultDir.Replace(":", "$")
$slashIndex = $srcResultDir.IndexOf("\")
if ($slashIndex -gt 0)
{
    $sharedRoot = $srcResultDir.SubString(0, $slashIndex)
}
else
{
    $sharedRoot = $srcResultDir
}

Write-Host "Try to copy test result from \\$srcComputerName\$sharedRoot by $usr / $pwd ..." -foregroundcolor Yellow

#----------------------------------------------------------------------------
# Copy file from remote computer to local
#----------------------------------------------------------------------------
net.exe use \\$srcComputerName\$sharedRoot $pwd /User:$usr 2>&1 | Write-Host

$itemPath = "\\$srcComputerName\$srcResultDir"
$itemCount = (Get-Item $itemPath | Measure-Object).Count
if ($itemCount -gt 0) {
    Write-Host "Copying from $itemPath to $destPath ..." -foregroundcolor Yellow
    xcopy.exe /R /H /E /I /Y $itemPath $destPath 2>&1 | Write-Host
}
else {
    Write-Host "File not found: $itemPath"
}

net.exe use \\$srcComputerName\$sharedRoot /DELETE 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Copy-TestResult.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Copy-TestResult.ps1"
}
exit 0