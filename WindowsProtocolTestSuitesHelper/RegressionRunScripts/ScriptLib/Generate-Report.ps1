#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Generate-Report.ps1
## Purpose:        Generate Html Report
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################
param(
[String]$protocolName,
[String]$binPath,
[String]$logPath,
[String]$inScope    = "Server+Both",
[String]$outOfScope = "Client",
[String]$scriptPath = $binPath +"\..\Scripts",
[String]$toolPath   = $binPath +"\..\Tools"
)

# Write Call Stack
if($function:EnterCallStack -ne $null)
{
	EnterCallStack "Generate-Report.ps1"
}

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
$logFile    = $logPath+"\Generate-Report.ps1.log"
Stop-Transcript
Start-Transcript $logFile -force

Write-Host "EXECUTING [Generate-Report.ps1] ..." -foregroundcolor cyan
Write-Host "`$protocolName   = $protocolName"
Write-Host "`$binPath        = $binPath"
Write-Host "`$scriptPath     = $scriptPath"
Write-Host "`$toolPath       = $toolPath"
Write-Host "`$logFile        = $logFile"
Write-Host "`$inScope        = $inScope"
Write-Host "`$outOfScope     = $outOfScope"

Write-Host "Begin to analyze requirement coverage..."
#----------------------------------------------------------------------------
# Starting running script
#----------------------------------------------------------------------------
Write-Host "Get log file list ..."
$logFileName     = [System.IO.Directory]::GetFiles($logPath, "*.xml", [System.IO.SearchOption]::AllDirectories)
$listlogFileName = ""
for ($index = 0; $index -lt $logFileName.Count; $index++)
{    
    Write-Host "`$logFileName   = "$logFileName[$index]""    
    $listlogFileName += "`""+$logFileName[$index]+"`" "
}
# find the reporting tool path
# the below commented because we only use reportingtool.exe from the "Tools" folder, not from the PTF installation folder
#$clientMachineCPUArch = & "$scriptPath\Get-OSArchitechture.ps1"
#$programFolder  = $env:ProgramFiles
#if ($clientMachineCPUArch -eq "x64")
#{
#    $programFolder = $programFolder + " (x86)"
#}

#----------------------------------------------------------------------------
# Get table file list under Bin folder
#----------------------------------------------------------------------------
# Change RS file pattern from "*_RequirementSpec.xml" to "*_*RequirementSpec.xml" to support using RS name like "MS-XXXX_ServerRequirementSpec.xml"
$tableFileNames = [System.IO.Directory]::GetFiles($binPath, "*_*RequirementSpec.xml", [System.IO.SearchOption]::AllDirectories)
if($tableFileNames.Count -lt 1)
{
	Write-Host "Fail to get Requirement Spec table(MS-Protocolname_*RequirementSpec.xml)"
	throw "Fail to get Requirement Spec table(MS-Protocolname_*RequirementSpec.xml)"
}

$listTableFileName = ""
for ($index = 0; $index -lt $tableFileNames.Count; $index++)
{    
    Write-Host "`$tableFileNames   = "$tableFileNames[$index]""    
    $listTableFileName += "`""+$tableFileNames[$index]+"`" "
}

$reportingTool  = "`""+ "$toolPath\ResultGatherer\ReportingTool.exe" + "`""
$outFileName    = "`""+ $logPath + "\" + $protocolName+"_TestResult.html" + "`""
Write-Host "`$reportingTool       = $reportingTool"
Write-Host "`$listlogFileName     = $listlogFileName"
Write-Host "`$listTableFileName   = $listTableFileName"
Write-Host "`$outFileName         = $outFileName"

$reportingToolPro     = Get-ChildItem "$toolPath\ResultGatherer\ReportingTool.exe"
$reportingToolVersion = $reportingToolPro.VersionInfo.ProductBuildPart

# Use RS file to set the value of $hasScopeColumn -- $tableFileNames is an array, so use the first element.
$content = (Get-Content $tableFileNames[0].Replace("`"", "") -TotalCount 50)
$hasScopeColumn = $false
foreach($line in $content)
{
    $hasScopeColumn = $line.ToLower().Contains("<ns1:scope>")   
    if($hasScopeColumn)
    {
         break;
    }
}

if($reportingToolVersion -gt 1777)
{
    #Only ReportingTools.exe after 1777 supports parameter "/verbose"
    if($hasScopeColumn)
    {
        #Only in case the RS has the Scope column, it supports ReportingTool.exe's /inScope and /outOfScope parameters
        Write-Host "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /verbose /P:$protocolName`_ /r /ins:$inScope /oos:$outOfScope"
        cmd /c "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /verbose /P:$protocolName`_ /r /ins:$inScope /oos:$outOfScope" 2>&1 | Write-Host
    }
    else
    {
        Write-Host "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /verbose /P:$protocolName`_ /r"
        cmd /c "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /verbose /P:$protocolName`_ /r" 2>&1 | Write-Host
    }
}
if($reportingToolVersion -gt 1026)
{
    #Only ReportingTools.exe after C8 supports parameter "/p"
    if($hasScopeColumn)
    {
        #Only in case the RS has the Scope column, it supports ReportingTool.exe's /inScope and /outOfScope parameters
        Write-Host "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /P:$protocolName`_ /r /ins:$inScope /oos:$outOfScope"
        cmd /c "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /P:$protocolName`_ /r /ins:$inScope /oos:$outOfScope" 2>&1 | Write-Host
    }
    else
    {
        Write-Host "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /P:$protocolName`_ /r"
        cmd /c "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /P:$protocolName`_ /r" 2>&1 | Write-Host
    }
}
else
{
    if($hasScopeColumn)
    {
        #Only in case the RS has the Scope column, it supports ReportingTool.exe's /inScope and /outOfScope parameters
        Write-Host "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /r /ins:$inScope /oos:$outOfScope"
        cmd /c "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /r /ins:$inScope /oos:$outOfScope" 2>&1 | Write-Host
    }
    else
    {
        Write-Host "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /r"
        cmd /c "`"$reportingTool`"" "/l:$listlogFileName /t:$listTableFileName /o:$outFileName /r" 2>&1 | Write-Host
    }
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Generate-Report.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow
Stop-Transcript

# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Generate-Report.ps1"
}
exit 0