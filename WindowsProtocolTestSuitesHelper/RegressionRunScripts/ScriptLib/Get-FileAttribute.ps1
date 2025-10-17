#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-FileAttribute.ps1
## Purpose:        Create a new file
## Version:        1.0 (25  Mar, 2010])
##
##############################################################################

param(
[string] $targetFile
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-FileAttribute.ps1]..." -foregroundcolor cyan
Write-Host "`$targetFile = $targetFile" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This scripts will create a new file."
    Write-host
    Write-host "Example: Get-FileAttribute.ps1 c:\target.txt"
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
#This methon requires the file set in driver C or D.
if($targetFile.Length -eq [int]0)
{
    Throw "The file'name is empty."
}
else
{
    write-host (gci $targetFile -force).Attributes
}