#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Modify-ParamConfigFileNode.ps1
## Purpose:        Modify ParamConfigFile Node.
## Version:        1.0 (8 Feb, 2021)
##
##############################################################################


Param(
[string]$sourceFileName, 
[string]$attrName, 
[string]$newContent
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Modify-ParamConfigFileNode.ps1] ..." -foregroundcolor cyan
Write-Host "`$sourceFileName = $sourceFileName"
Write-Host "`$attrName       = $attrName"
Write-Host "`$newContent     = $newContent"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Set an attribute of node in XML file."
    Write-host
    Write-host "Example: Modify-ParamConfigFileNode.ps1 C:\ParamConfig.xml ClientOS Win7"
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
    Throw "Parameter sourceFileName is required."
}
if ($attrName -eq $null -or $attrName -eq "")
{
    Throw "Parameter attribute Name is required."
}
if ($newContent -eq $null -or $newContent -eq "")
{
    Throw "Parameter attribute newContent is required."
}

#----------------------------------------------------------------------------
# Get parameter
#----------------------------------------------------------------------------
$fileExist =  Test-Path $sourceFileName
if($fileExist -eq $false)
{
    Write-Host "No $sourceFileName exist"    
}
else
{
    [xml]$content = Get-Content $sourceFileName
    if($content -eq $NULL -or $content.Parameters -eq $NULL -or $content.Parameters -eq "")
    {
        Write-Host "No Parameters found"
        return
    }
    else
    {
        $newPara = $TRUE
        $propertyNodes = $content.GetElementsByTagName("Parameter")
        foreach($node in $propertyNodes)
        {
            if($node.Name -eq $attrName)
            {
                $newPara = $FALSE
                $node.SetAttribute("Value", "$newContent")
                break    
            }
        }

        if($newPara)
        {
            Write-Host "Parameter $attrName not found"
            return
        }
        else
        {
            $content.Save((Resolve-Path $sourceFileName))
        }
    }
}
#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Modify-ParamConfigFileNode.ps1] SUCCEED." -foregroundcolor green

exit