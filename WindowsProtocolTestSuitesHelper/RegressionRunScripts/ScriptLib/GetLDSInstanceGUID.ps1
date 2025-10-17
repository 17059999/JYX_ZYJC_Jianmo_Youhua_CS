#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

#-----------------------------------------------------------------------------
# Get LDS Instance GUID
#-----------------------------------------------------------------------------

Param
(
    [parameter(Mandatory=$true)]
    [string]$InstanceName
)

# Get GUID
[string]$regPath = "HKLM:\SYSTEM\ControlSet001\services\ADAM_" + $InstanceName + "\Parameters"
[string]$ConNC = Get-ItemProperty -path $regPath -Name "Configuration NC"
$separators = @(' ', ',', ';')
[string[]]$Temp = $ConNC.Split($separators)
[string]$GUID=""

foreach($token in $Temp)
{
    if($token.Contains("{") -and $token.Contains("}"))
    {
        [string[]]$parts = $token.Split('=')
        $GUID = $parts[1]
        $GUID=$GUID.Replace("{","")
        $GUID=$GUID.Replace("}","")
        break;
    }
}

Write-Host "ConNC = $ConNC"
Write-Host "GUID = $GUID"

return $GUID