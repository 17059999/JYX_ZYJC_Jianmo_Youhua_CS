#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Generate-VSmdiFromTrx.ps1
## Purpose:        generate vsmdi file after parsing specific types
## Version:        1.1 (30 Apr, 2009)
##
##############################################################################

param(
[string]$rerunType,
[string]$rerunFilter,
[string]$rerunVSmdi,
[string]$sourceTrx,
[string]$refVSmdi,
[string]$refList,
[string]$parentList
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Generate-VSmdiFromTrx.ps1]." -foregroundcolor cyan
Write-Host "`$rerunType   = $rerunType"
Write-Host "`$rerunFilter = $rerunFilter"
Write-Host "`$rerunVSmdi  = $rerunVSmdi"

Write-Host "`$sourceTrx   = $sourceTrx"
Write-Host "`$refVSmdi    = $refVSmdi"
Write-Host "`$refList     = $refList"
Write-Host "`$parentList  = $parentList"

$xmlFileHead = ""
#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script configures a computer to logon automatically by specified credential."
    Write-host
    Write-host "Example: .\Generate-VSmdiFromTrx.ps1 `"Passed`" `"reruntestcases`" `"MS-IMSA-rerun.vsmdi`" `"MS-IMSA.trx`" `"MS-IMSA.vsmdi`" `"underTest`" `"Lists of Tests`""
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
# parse cases needed to rerun
#----------------------------------------------------------------------------
[xml]$list  = get-content $sourceTrx
$TestLinks  = $list.GetElementsByTagName("UnitTestResult")
[string]$rerunIds   =""
foreach($TestLink in $TestLinks)
{
    if( $rerunType.Contains($TestLink.outcome) ){
        $rerunIds = $rerunIds + $TestLink.testId+","
    }
}
if($rerunIds -eq "" )
{
    Write-Host "No $rerunType test cases found!" -foregroundcolor green
    Write-Host "EXECUTE [Generate-VSmdiFromTrx.ps1] SUCCEED." -foregroundcolor green
    return 0
}
$IdArayy  = $rerunIds.split(",")

#----------------------------------------------------------------------------
# make nodes for cases needed tor rerun
#----------------------------------------------------------------------------
[xml]$list  = get-content $refVSmdi
$TestLinks  = $list.GetElementsByTagName("TestLink")
$rerunIds   = ""
[int]$nodesAccount=0
foreach($TestLink in $TestLinks)
{
    $idName = $TestLink.id
    for([int]$tempid =0; $tempid -lt $IdArayy.Length; $tempid++ ){
        if($idName -eq $IdArayy[$tempid]){
            $nodesAccount++;
            $rerunIds = $rerunIds + "<TestLink id=`"" + $TestLink.id +"`" name=`""+$TestLink.name +"`" storage=`""+$TestLink.storage +"`" type=`""+$TestLink.type+"`" />"
            break;
        }
    }
    if($nodesAccount -eq ($IdArayy.Length-1) ){
        break;
    }
}
#----------------------------------------------------------------------------
# make nodes for xml
#----------------------------------------------------------------------------
[string]$parentNode =""
[string]$rerunNode  =""

$TestLists = $list.GetElementsByTagName("TestList")
foreach($TestList in $TestLists)
{
    if($TestList.name -eq $parentList)
    {
        $parentNode = "<TestList name=`""+$TestList.name+"`" id=`""+$TestList.id+"`">"
        $configNode = $TestList.ChildNodes.Item(0)
        $parentNode = $parentNode + "<RunConfiguration id=`""+$configNode.id+"`" name=`""+$configNode.name+"`" storage=`""+$configNode.storage+"`" type=`""+$configNode.type+"`" />"
        $parentNode = $parentNode + "</TestList>"
    }
    if($TestList.name -eq $refList)
    {
        $rerunNode = "<TestList name=`""+$rerunFilter+"`" id=`""+$TestList.id+"`" parentListId=`""+$TestList.parentListId+"`">"
    }
}
$rerunNode = $rerunNode +"<TestLinks>"+$rerunIds+"</TestLinks> </TestList>"
#----------------------------------------------------------------------------
# make xml
#----------------------------------------------------------------------------
[string]$xmlfile = $xmlFileHead
$TestListsNode   = $list.GetElementsByTagName("TestLists").Item(0)
$xmlfile = $xmlfile + "<TestLists xmlns=`""+$TestListsNode.xmlns+"`">"+ $parentNode + $rerunNode +"</TestLists>"
$xmlfile > $rerunVSmdi

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host ($nodesAccount)"/"($IdArayy.Length-1) "$rerunType test cases found!" -foregroundcolor green
Write-Host "EXECUTE [Generate-VSmdiFromTrx.ps1] SUCCEED." -foregroundcolor green
return ($IdArayy.Length-1)
exit
