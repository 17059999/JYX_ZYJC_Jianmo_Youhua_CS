#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Set-FileAttribute.ps1
## Purpose:        Create a new file
## Version:        1.0 (25  Mar, 2010])
##
##############################################################################

param(
[string] $targetFile,
[string] $attribute,
[bool] $grantpermission = $true
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-FileAttribute.ps1]..." -foregroundcolor cyan
Write-Host "`$targetFile      = $targetFile" 
Write-Host "`$attribute       = $attribute" 
Write-Host "`$grantpermission = $grantpermission" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This scripts will create a new file."
    Write-host
    Write-host "Enable Readonly Example: Set-FileAttribute.ps1 c:\target.txt Readonly"
    Write-host "Disable Readonly Example: Set-FileAttribute.ps1 c:\target.txt Readonly `$false"
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
    $file =(gci $targetFile -force); 
    if($grantpermission -eq "Yes")
    {
        $file.Attributes = $file.Attributes -bor [System.IO.FileAttributes]::$attribute; 
    }
    else
    {
        if ($file.attributes -band [system.IO.FileAttributes]::$attribute) 
        { 
            $file.attributes = $file.attributes -bxor [system.IO.FileAttributes]::$attribute 
            if($?)
            {
                Write-Host "Set File Attribute success!"
            }
            else
            {
                Write-Host "Set File Attribute failed!"
            } 
        }
        else
        {
            Write-Host "File do NOT has Attribute $attribute"
        }
    }
}