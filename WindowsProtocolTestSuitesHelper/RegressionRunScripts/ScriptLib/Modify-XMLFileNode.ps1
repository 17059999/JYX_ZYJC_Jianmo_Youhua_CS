#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Modify-XMLFileNode.ps1
## Purpose:        Modify the node value for the "XML" file.
## Version:        1.1 (24 Mar, 2009)
##
##############################################################################

Param(
[string]$sourceFileName, 
[string]$nodeName, 
[string]$specAttributeName,
[string]$specAttributeValue,
[string]$modifyAttributeName,
[string]$modifyAttributeValue
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Modify-ConfigFileNode.ps1] ..." -foregroundcolor cyan
Write-Host "`$sourceFileName       = $sourceFileName"
Write-Host "`$nodeName             = $nodeName"
Write-Host "`$specAttributeName    = $specAttributeName"
Write-Host "`$specAttributeValue   = $specAttributeValue"
Write-Host "`$modifyAttributeName  = $modifyAttributeName"
Write-Host "`$modifyAttributeValue = $modifyAttributeValue"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script used to modify a node value of the xml file."
    Write-host
    Write-host "The Path of the config file must be a absoluted path!"
    Write-host 
    Write-host "Example: To change the value of ServerComputerName to SUT01"
    Write-host
    Write-host "Modify-XMLFileNode.ps1 c:\Test\Data\MS-WMF.deployment.ptfconfig property name ServerComputerName value SUT01"
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
# Modify the content of the node
#----------------------------------------------------------------------------
$ifFileExist = $false
$IsFindNode = $false

$ifFileExist = Test-Path $sourceFileName
if($ifFileExist -eq $true)
{
    attrib -R $sourceFileName
    
    [xml]$configContent = Get-Content $sourceFileName
    $PropertyNodes = $configContent.GetElementsByTagName($nodeName)
    foreach($node in $PropertyNodes)
    {
        if($node.GetAttribute($specAttributeName) -eq $specAttributeValue)
        {
            $node.SetAttribute($modifyAttributeName,$modifyAttributeValue)
            $IsFindNode = $true
            break
        }
    }
    
    if($IsFindNode)
    {
        $configContent.save($sourceFileName)
    }
    else
    {
        Throw "Config failed: Can't find the node whoes name attribute is $nodeName" 
    }

    attrib +R $sourceFileName
}
else
{
    Throw "Config failed: The config file $sourceFileName does not existed!" 
}

#----------------------------------------------------------------------------
# Verify the result
#----------------------------------------------------------------------------
if($ifFileExist -eq $true -and $IsFindNode)
{
    [xml]$configContent = Get-Content $sourceFileName
    $PropertyNodes = $configContent.GetElementsByTagName($nodeName)
    foreach($node in $PropertyNodes)
    {
        if($node.GetAttribute($specAttributeName) -eq $specAttributeValue)
        {
            if($node.GetAttribute($modifyAttributeName) -eq $modifyAttributeValue)
            {
                Write-Host "Config success: set ($modifyAttributeName = $modifyAttributeValue) for node $nodeName with ($specAttributeName = $specAttributeValue)" -ForegroundColor green
                return
            }    
        }
    }
    Write-Error "Config failed: Please check if the $sourceFileName is readonly." 
    Throw "EXECUTE [Modify-XMLFileNode.ps1] FAILED."
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Modify-XMLFileNode.ps1] SUCCEED." -foregroundcolor green

exit
