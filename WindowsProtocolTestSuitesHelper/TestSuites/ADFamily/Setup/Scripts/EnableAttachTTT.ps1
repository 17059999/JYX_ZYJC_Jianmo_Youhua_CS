#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

Function Write-Log
{
    Param ([Parameter(ValueFromPipeline=$true)] $text,
    $ForegroundColor = "Green"
    )

    $date = Get-Date
    Write-Host -NoNewLine -ForegroundColor $ForegroundColor "`r`n$date $text`r`n"
}

#-----------------------------------------------------
# Function: Modify-PTFConfig
# Usage: Modify PTF configure file
#-----------------------------------------------------
Function Modify-PTFConfig(
    [parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$PTFConfigPath,
    [parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$nodeName,
    [parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$value
    )
{
    Write-Log "Turn off Read-only arribute for $PTFConfigPath"
    Set-ItemProperty -Path $PTFConfigPath -Name IsReadOnly -Value $false

    Write-Log "Modify node: $nodeName to value: $value"
    [xml]$configContent = Get-Content $PTFConfigPath
    $PropertyNodes = $configContent.GetElementsByTagName("Property")
    foreach($node in $PropertyNodes)
    {
        if($node.GetAttribute("name") -eq $nodeName)
        {
            $node.SetAttribute("value", $value)
            $IsFindNode = $true
            break
        }
    }

    if($IsFindNode)
    {
        $configContent.Save($PTFConfigPath)
    }
    else
    {
        throw "Setting PTFConfig failed: Cannot find the node whose name attribute is $nodeName"
    }

    Write-Log "Turn on Read-only attribute for $PTFConfigPath"
    Set-ItemProperty -Path $PTFConfigPath -Name IsReadOnly -Value $true
}

$ptfconfigs = dir "$env:HOMEDRIVE\MicrosoftProtocolTests" -Recurse | where{$_.Name -eq "AD_ServerTestSuite.ptfconfig"}
foreach($ptfconfig in $ptfconfigs)
    {
        Modify-PTFConfig -PTFConfigPath $ptfconfig.FullName -nodeName "ATLASAttachTTT" -value "true"
        }
