#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-MIPMSIandTools.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 7
##
##############################################################################

Param(
[string]$ConfigureFile,
[string]$ToolPath     = "D:\WinteropProtocolTesting\Tools",
[string]$targetPath  = "D:\WinteropProtocolTesting\VSTORMLITE\Custom\deploy"
)

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($ConfigureFile -eq $null -or $ConfigureFile -eq "")
{
    Throw "Parameter protocolName is required."
}

Write-Host "Get tools install information"
[xml]$xmlContent = gc $ConfigureFile
$Tools = $xmlContent.SelectNodes("lab/servers/vm/tools")

if($Tools -eq $null -or $Tools -eq "")
{
	exist 0
}

$IsVcredistCopied = $false
foreach($ToolNode in $Tools)
{
   foreach($node in $ToolNode.GetElementsByTagName("tool"))
    {
        $name = $node.name
		$version = $node.version
		$CPUArchitecture = $node.CPUArchitecture
		$FileName = $node.MSIName
		if($FileName -eq $null)
		{
			$FileName = $node.EXEName
		}

		if($FileName -eq "vcredist.exe")
		{
            if($IsVcredistCopied -eq $false)
            {
                $IsVcredistCopied = $true
                robocopy $ToolPath\$name\$version\$CPUArchitecture\  $targetPath $FileName
            }
            else
            {
                $FileName = $CPUArchitecture + "_vcredist.exe"
                Copy $ToolPath\$name\$version\$CPUArchitecture\vcredist.exe $targetPath\$FileName
            }
		}
        else
        {
            robocopy $ToolPath\$name\$version\$CPUArchitecture\  $targetPath $FileName
        }

    }
}
exit 0

