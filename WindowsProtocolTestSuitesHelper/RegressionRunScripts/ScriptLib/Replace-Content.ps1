#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Replace-Content.ps1
## Purpose:        Replace the $originalValue value with $newValue in the specified file.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2008 R2, Windows Server 2008 SP2
##
##############################################################################

Param(
[String]$sourceFileName  = ".",
[String]$originalValue   = "",
[String]$newValue        = ""
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Replace-Content.ps1] ..." -foregroundcolor cyan
Write-Host "`$sourceFileName      = $sourceFileName"
Write-Host "`$originalValue       = $originalValue"
Write-Host "`$newValue            = $newValue"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Replace the $originalValue value with $newValue in the specified file."
    Write-host
    Write-host "Example: Replace-Content.ps1 c:\test\scripts\config-server.ps1 `"netcfg -c s -u MS_Server`" `"#netcfg -c s -u MS_Server`""
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
if ($sourceFileName -eq $null -or $sourceFileName -eq "")
{
    Throw "Parameter attribute Name is required."
}
if ($originalValue -eq $null -or $originalValue -eq "")
{
    Throw "Parameter attribute Name is required."
}

$fileExist =  Test-Path $sourceFileName
if($fileExist -eq $false)
{
    Write-Host "No $sourceFileName exist"    
}

$content = Get-Content $sourceFileName
$content | foreach {$_ -replace "$originalValue", "$newValue"} | Set-Content $sourceFileName
Write-Host  "Write signal file to system drive."
cmd /c ECHO "Replace content Finished" >$env:HOMEDRIVE\ReplaceContent.finish.signal
exit 0
