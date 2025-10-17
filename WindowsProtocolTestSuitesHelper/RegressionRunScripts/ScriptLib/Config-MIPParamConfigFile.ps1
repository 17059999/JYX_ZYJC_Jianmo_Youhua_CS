#############################################################################
##
## Microsoft Windows Powershell Scripting
## Purpose: Get Parameter which used in Protocol in XML file.
##
##############################################################################

Param(
[String]$attrName,
[String]$Value
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-MIPParamConfigFile.ps1] ..." -foregroundcolor cyan
Write-Host "`$attrName    = $attrName"
Write-Host "`$Value       = $Value"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Set an attribute of node in XML file."
    Write-host
    Write-host "Example: Config-MIPParamConfigFile.ps1 ClientOS Win7"
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
if ($attrName -eq $null -or $attrName -eq "")
{
    Throw "Parameter attribute Name is required."
}
if ($Value -eq $null -or $Value -eq "")
{
    Throw "Parameter attribute Value is required."
}

# remove existing signal first
$signFileName="ConfigParam"+"$attrName"+"Finished.signal"
if (Test-Path -Path "$env:HOMEDRIVE/$signFileName")
{
   Remove-Item -path "$env:HOMEDRIVE/$signFileName" -force
}

#----------------------------------------------------------------------------
# Get the full path of ParamConfig.xml
#----------------------------------------------------------------------------
Write-Host "Get the scripts path from MSIInstalled.signal file"
$signalFile  = "$env:HOMEDRIVE/MSIInstalled.signal"
if (Test-Path -Path $signalFile)
{
    $TestSuiteScriptsFullPath = Get-Content $signalFile
}
else
{
    Write-Host "MSI has not been installed. please check"
    return
}
$sourceFileName = "$TestSuiteScriptsFullPath/ParamConfig.xml"

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
                $node.SetAttribute("Value", "$Value")
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
            $content.Save($sourceFileName)
        }
    }
}
Write-Host  "Write signal file to system drive."
Write-Output "Config ParamConfig Finished" > $env:HOMEDRIVE/$signFileName
exit 0