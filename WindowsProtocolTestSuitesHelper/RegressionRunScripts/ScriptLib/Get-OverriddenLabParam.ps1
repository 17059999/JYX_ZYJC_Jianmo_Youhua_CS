#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-OverriddenLabParam.ps1
## Purpose:        Provide a solution to let different team to override WTT parameters, according to their own hosted fileserver, log server.
##                 This script will read a OverriddenLabParams.xml (which is under [WTTBin] - typically it is C:\ProgramFile\WTT 2.2\Client).
## Version:        0.1 (3 March, 2009)
##
##############################################################################

param(
[String]$paramName,     # the parameter name you want to override  
[string]$xmlFilePath  = "$env:WTTBIN\OverriddenLabParams.xml"  # xml file location in the test host                
)

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host
    Write-host "Example: Get-OverriddenLabParam.ps1 ISOPath"
    Write-host
    Write-host "Exception: if input `$paramName does't exist in the xml file, it will return an empty string."
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
if ($paramName -eq $null -or $paramName -eq "")
{
    Show-ScriptUsage
    Throw "Parameter `$paramName is required."
}

if((Test-Path($xmlFilePath)) -eq $false)
{
    return ""
}
else
{
    Write-Host "Overriden lab run config file found from $xmlFilePath."
}

#----------------------------------------------------------------------------
# Get value from xml config file
#----------------------------------------------------------------------------
$xmlDoc      = NEW-OBJECT System.Xml.XmlDocument
$xmlDoc.Load($xmlFilePath)
$xmlHeadNode = $xmlDoc.SelectSingleNode("LabConfig")
$xmlNodeList = $xmlHeadNode.ChildNodes
$newValueFound = $false
$newValue    = ""
foreach( $xn in $xmlNodeList)
{
    if($xn.Name -eq $paramName)
    {   
        $newValue = $xn.InnerText.Trim()
        $newValueFound = $true
    }
}
if($newValueFound -eq $false)
{
    Write-Host "Cannot find the overriden value for $paramName" -foregroundcolor Yellow
    Write-Host "Only the following params are redefined in the config file:"
    foreach( $xn in $xmlNodeList)
    {
        $name=$xn.Name
        Write-Host "$name"   
    }
}

return $newValue 