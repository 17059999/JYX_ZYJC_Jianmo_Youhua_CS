#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Update-Content.ps1
## Purpose:        Replace the $originalValue value with $newValue in the specified file.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2008 R2, Windows Server 2008 SP2
##
##############################################################################

Param(
[String]$sourceFileName  = ".",
[String]$originalValue   = "", #For replace: the value to be replace, for Add, new value will be added before the value 
[String]$newValue        = "",#If you want to comment a line ,please set the value as "****"
[String]$action          = "Replace" #should be "Replace" or "Add"
)
Start-Transcript $env:HOMEDRIVE\Update-Content.ps1.log -Append
#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Update-Content.ps1] ..." -foregroundcolor cyan
Write-Host "`$sourceFileName      = $sourceFileName"
Write-Host "`$originalValue       = $originalValue"
Write-Host "`$newValue            = $newValue"
Write-Host "`$action              = $action"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Replace the $originalValue value with $newValue in the specified file."
    Write-host
    Write-host "Example: Update-Content.ps1 c:\test\scripts\config-server.ps1 `"netcfg -c s -u MS_Server`" `"#netcfg -c s -u MS_Server`""
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

#.\TurnOff-FileReadonly.ps1 $sourceFileName

#replace the specified value with the new value
if($action -eq "Replace")
{
	(Get-Content $sourceFileName)| foreach { if ($_ -match "$originalValue"){if($newvalue -eq "****"){"`# $_"}else{$_ -replace $originalValue,$newValue}}else{$_}}| Set-Content $sourceFileName
	Write-Host  "Write signal file to system drive."
	cmd /c ECHO "Replace content Finished" >$env:HOMEDRIVE\ReplaceContent.finish.signal
}
# add the new value before the specified value
if($action -eq "Add")
{
	(Get-Content $sourceFileName)| foreach { if ($_ -match "$originalValue"){"$newValue"} $_ }| Set-Content $sourceFileName
	Write-Host  "Write signal file to system drive."
	cmd /c ECHO "Replace content Finished" >$env:HOMEDRIVE\AddContent.finish.signal
}
cmd /c ECHO "Replace content Finished" >$env:HOMEDRIVE\UpdateContent.finish.signal
exit 0
