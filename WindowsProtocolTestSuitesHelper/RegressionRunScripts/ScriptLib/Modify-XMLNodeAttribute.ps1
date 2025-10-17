#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Modify-XMLNodeAttribute.ps1
## Purpose:        Modify an attribute of node in XML file.
## Version:        1.1 (25 July, 2008)
##
##############################################################################

Param(
[string]$sourceFileName,
[String]$nodeTagName,
[string]$attributeName, 
[string]$newContent,
[string]$addNew = "Add" # NoAdd or Add
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Modify-XMLNodeAttribute.ps1] ..." -foregroundcolor cyan
Write-Host "`$sourceFileName = $sourceFileName"
Write-Host "`$nodeTagName    = $nodeTagName"
Write-Host "`$attributeName  = $attributeName"
Write-Host "`$newContent     = $newContent"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Modify an attribute of node in XML file."
    Write-host
    Write-host "The Path of the XML type file must be a absoluted path!"
    Write-host
    Write-host "Example: Modify-XMLNodeAttribute.ps1 C:\config.xml Property Name abc Add"
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
if ($nodeTagName -eq $null -or $nodeTagName -eq "")
{
    Throw "Parameter nodeTagName is required."
}
if ($attributeName -eq $null -or $attributeName -eq "")
{
    Throw "Parameter attributeName is required."
}
if ($newContent -eq $null -or $newContent -eq "")
{
    Throw "Parameter newContent is required."
}

#----------------------------------------------------------------------------
# Modify the content of the node
#----------------------------------------------------------------------------
$isFileExist  = $false
$isNodeFound  = $false

$isFileExist = Test-Path $sourceFileName
if($isFileExist -eq $true)
{
    attrib.exe $sourceFileName -R   2>&1 |Write-Host
    [xml]$configContent = Get-Content $sourceFileName
    $PropertyNodes = $configContent.GetElementsByTagName($nodeTagName)
    foreach($node in $PropertyNodes)
    {
       if($addNew -eq "Add" -or $node.HasAttribute("$attributeName"))
       {
           $isNodeFound  = $true
           $node.SetAttribute($attributeName,$newContent)                  
       }
    }
    
    $configContent.save($sourceFileName)
    
}
else
{
    Throw "Config failed: The config file $sourceFileName does not existed!" 
}

#----------------------------------------------------------------------------
# Verify the result
#----------------------------------------------------------------------------
$checked = $false
if($isFileExist -eq $true -and $isNodeFound )
{
    [xml]$configContent = Get-Content $sourceFileName
    $propertyNodes = $configContent.GetElementsByTagName($nodeTagName)
    foreach($node in $propertyNodes)
    {
        if($node.GetAttribute($attributeName) -eq $newContent)
        {
            $checked = $true
            break    
        }
    }
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
if($checked)
{
    Write-Host "Config success: Set $attributeName to $newContent in $nodeTagName" -ForegroundColor green
    Write-Host "EXECUTE [Modify-XMLNodeAttribute.ps1] SUCCEED." -foregroundcolor green
}
else
{
    Write-Error "Config failed: Please check if the $sourceFileName is readonly or if $nodeTagName has the attribute:$attributeName." 
    Throw "EXECUTE [Modify-XMLNodeAttribute.ps1] FAILED."
}

exit
