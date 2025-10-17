#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-TestFile.ps1
## Purpose:        Copy test files to test VM (Client VM and Server VM).
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[string]$srcScriptLibPathOnHost,
[string]$srcScriptPathOnHost,
[string]$srcToolPathOnHost, 
[string]$srcBinPathOnHost, 
[string]$destComputerName, 
[string]$destDir, 
[string]$usr, 
[string]$pwd,
[string]$srcMyToolPathOnHost,    # Optional
[string]$srcDataPathOnHost,      # Optional
[string]$srcSnapshotPathOnHost   # Optional
)

# Write Call Stack
if($function:EnterCallStack -ne $null)
{
	EnterCallStack "Copy-TestFile.ps1"
}

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Copy-TestFile.ps1] ..." -foregroundcolor cyan
Write-Host "`$srcScriptLibPathOnHost = $srcScriptLibPathOnHost"
Write-Host "`$srcScriptPathOnHost    = $srcScriptPathOnHost"
Write-Host "`$srcToolPathOnHost      = $srcToolPathOnHost"
Write-Host "`$srcBinPathOnHost       = $srcBinPathOnHost"
Write-Host "`$destComputerName       = $destComputerName"
Write-Host "`$destDir                = $destDir"
Write-Host "`$usr                    = $usr"
Write-Host "`$pwd                    = $pwd"
Write-Host "`$srcMyToolPathOnHost    = $srcMyToolPathOnHost"
Write-Host "`$srcDataPathOnHost      = $srcDataPathOnHost"
Write-Host "`$srcSnapshotPathOnHost  = $srcSnapshotPathOnHost"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script copies test files to remote computer. Including scripts, tools, data and test suite."
    Write-host
    Write-host "Example: Copy-TestFile.ps1 srcScriptLibPathOnHost srcScriptPathOnHost srcToolsPath srcBinPathOnHost SUT01 c$\Test username password"
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
if ($srcScriptLibPathOnHost -eq $null -or $srcScriptLibPathOnHost -eq "")
{
    Throw "Parameter srcScriptLibPathOnHost is required."
}
if ($srcScriptPathOnHost -eq $null -or $srcScriptPathOnHost -eq "")
{
    Throw "Parameter srcScriptPathOnHost is required."
}
if ($srcToolPathOnHost -eq $null -or $srcToolPathOnHost -eq "")
{
    Throw "Parameter srcToolPathOnHost is required."
}
if ($srcBinPathOnHost -eq $null -or $srcBinPathOnHost -eq "")
{
    Throw "Parameter srcBinPathOnHost is required."
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
# Copy test files
#----------------------------------------------------------------------------
Write-Host "Try to copy test suite to \\$destComputerName\$sharedRoot by $usr / $pwd ..." -foregroundcolor Yellow
for($i=0;$i -le 3;$i++)
{
    net.exe use \\$destComputerName\$sharedRoot $pwd /User:$usr 2>&1 | Write-Host
    if ($lastExitCode -eq 0)
    {
        break
    }
    else
    {
        sleep 5
    }
}

Write-Host "Copying test tools from $srcToolPathOnHost to \\$destComputerName\$destDir\Tools\ ..." -foregroundcolor Yellow
robocopy.exe /MIR /NFL /NDL /R:2 $srcToolPathOnHost \\$destComputerName\$destDir\Tools\ 2>&1 | Write-Host

Write-Host "Copying test suite from $srcBinPathOnHost  to \\$destComputerName\$destDir\Bin\ ..." -foregroundcolor Yellow
robocopy.exe /MIR /NFL /NDL /R:2 $srcBinPathOnHost \\$destComputerName\$destDir\Bin\ 2>&1 | Write-Host

Write-Host "Copying common test scripts from $srcScriptLibPathOnHost to \\$destComputerName\$destDir\Scripts\ ..." -foregroundcolor Yellow
robocopy.exe /LEV:1 /MIR /NFL /NDL /R:2 $srcScriptLibPathOnHost \\$destComputerName\$destDir\Scripts\ 2>&1 | Write-Host

Write-Host "Copying protocol's test scripts from $srcScriptPathOnHost to \\$destComputerName\$destDir\Scripts\ ..." -foregroundcolor Yellow
robocopy.exe /E /NFL /NDL /R:2 $srcScriptPathOnHost    \\$destComputerName\$destDir\Scripts\ 2>&1 | Write-Host

if (($srcMyToolPathOnHost -ne $null) -and ($srcMyToolPathOnHost -ne "") -and (Test-Path $srcMyToolPathOnHost) )
{
    Write-Host "Copying protocol's tools from $srcMyToolPathOnHost to \\$destComputerName\$destDir\MyTools\ ..." -foregroundcolor Yellow
    robocopy.exe /MIR /NFL /NDL /R:2 $srcMyToolPathOnHost \\$destComputerName\$destDir\MyTools\ 2>&1 | Write-Host
}

if (($srcDataPathOnHost -ne $null) -and ($srcDataPathOnHost -ne "") -and (Test-Path $srcDataPathOnHost) )
{
    Write-Host "Copying protocol's data (optional) from $srcDataPathOnHost to \\$destComputerName\$destDir\Data\ ..." -foregroundcolor Yellow
    robocopy.exe /MIR /NFL /NDL /R:2 $srcDataPathOnHost \\$destComputerName\$destDir\Data\ 2>&1 | Write-Host
}

if (($srcSnapshotPathOnHost -ne $null) -and ($srcSnapshotPathOnHost -ne "") -and (Test-Path $srcSnapshotPathOnHost) )
{
    Write-Host "Copying protocol's snapshot (optional) from $srcSnapshotPathOnHost to \\$destComputerName\$destDir\Snapshot\ ..." -foregroundcolor Yellow
    robocopy.exe /MIR /NFL /NDL /R:2 $srcSnapshotPathOnHost \\$destComputerName\$destDir\Snapshot\ 2>&1 | Write-Host
}

net.exe use \\$destComputerName\$sharedRoot /DELETE 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Copy-TestFile.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Copy-TestFile.ps1"
}
exit 0