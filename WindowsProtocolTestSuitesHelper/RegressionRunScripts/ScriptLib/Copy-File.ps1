#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-File.ps1
## Purpose:        Copy files to remote computer.
## Version:        1.0 (14 July, 2008)
##
##############################################################################

param(
[string]$srcPathOnHost,
[string]$destComputerName, 
[string]$destDir, 
[string]$usr, 
[string]$pwd
)

# Write Call Stack
if($function:EnterCallStack -ne $null)
{
	EnterCallStack "Copy-File.ps1"
}

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Copy-File.ps1] ..." -foregroundcolor cyan
Write-Host "`$srcPathOnHost       = $srcPathOnHost"
Write-Host "`$destComputerName    = $destComputerName"
Write-Host "`$destDir             = $destDir"
Write-Host "`$usr                 = $usr"
Write-Host "`$pwd                 = $pwd"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Copy files to remote computer."
    Write-host
    Write-host "Example: Copy-File.ps1 srcScriptLibPathOnHost  SUT01 c$\Test username password"
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
if ($srcPathOnHost -eq $null -or $srcPathOnHost -eq "")
{
    Throw "Parameter srcScriptLibPathOnHost is required."
}
if ($destComputerName -eq $null -or $destComputerName -eq "")
{
    Throw "Parameter destComputerName is required."
}
if ($destDir -eq $null -or $destDir -eq "")
{
    Throw "Parameter destDir is required."
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
#       $usr = "$computerName\$usr"
#    }
#    else
#    {
#        $usr = "$global:domain\$usr"
#   }
#}
#[v-xich]: Remove this, we don't use domain account as default.

#----------------------------------------------------------------------------
# Find root of sharing on remote machine
#----------------------------------------------------------------------------
$destDir = $destDir.Replace(":", "$")
$slashIndex = $destDir.IndexOf("\")
if ($slashIndex -gt 0)
{
    $sharedRoot = $destDir.SubString(0, $slashIndex)
}
else
{
    $sharedRoot = $destDir
}

#----------------------------------------------------------------------------
# Copy files
#----------------------------------------------------------------------------
Write-Host "Try to copy test suite to \\$destComputerName\$sharedRoot by $usr / $pwd ..." -foregroundcolor Yellow
net.exe use \\$destComputerName\$sharedRoot $pwd /User:$usr 2>&1 | Write-Host

Write-Host "Copying test tools from $srcPathOnHost to \\$destComputerName\$destDir\ ..." -foregroundcolor Yellow
robocopy.exe /MIR /NFL /NDL $srcPathOnHost \\$destComputerName\$destDir 2>&1 | Write-Host

net.exe use \\$destComputerName\$sharedRoot /DELETE 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Copy-File.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Copy-File.ps1"
}
exit 0