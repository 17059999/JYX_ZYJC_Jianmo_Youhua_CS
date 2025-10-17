#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           CleanupMachine.ps1
## Purpose:        Clean up the test machine.
## Version:        1.1 (21 April, 2010)
##
##############################################################################

$FilePath = "E:\LabDailyRun\PrepareMachines"
$Date = Get-Date
$Excutelog = "CleanTestMachine" + $Date.year + "-" + $Date.Month + "-" + $Date.Day + ".log"
Stop-Transcript
Start-Transcript $FilePath\Log\$Excutelog -force
push-location $FilePath
Get-Location | Write-Host

#----------------------------------------------------------------------------
#stage 1: cancel the job stilling running or scheduled on daily run machine 
#----------------------------------------------------------------------------

$config = "LabDailyRunMachineList.xml"
[xml]$content = Get-Content $config
$Nodes = $content.GetElementsByTagName("DailyRunMachineList").Item(0).ChildNodes
Write-Host "ProductGroup is $Nodes"

$WTTPath = $env:ProgramFiles + "\WTT 2.2\Client"
push-location $WTTPath 
Get-Location | Write-Host

function CancelJob([string] $list)
{
    $temp = $list.Split(",")
    for ($t = 0; $t -lt $temp.Count; $t ++)
    {
        $targetmachine = $temp[$t]
        Write-Host `$targetmachine is $targetmachine
        Write-Host "cmd /c WttCmd /CancelJob /TargetMachine:$targetmachine"
        cmd /c WttCmd /CancelJob /TargetMachine:$targetmachine
    } 
}

#----------------------------------------------------------------------------
#loop for cancel jobs 
#----------------------------------------------------------------------------
for ($LoopTime = 0; $LoopTime -lt 10; $LoopTime ++) 
{
    $now = Get-Date
    Write-Host "-------canceling jobs---Loop: $LoopTime--$now------------"
    foreach($Node in $Nodes )
    {
        $MachineList = $Node.GetAttribute("List")
        Write-Host "MachineList is $MachineList"
        if ($MachineList -ne "")
        {
            CancelJob $MachineList
        }
    }       
    start-sleep -s 10         
}
  
#----------------------------------------------------------------------------
#stage 2: clean up test machine 
#----------------------------------------------------------------------------

if ($env:PROCESSOR_ARCHITECTURE -eq "x86")
{
    #$env:Path=$env:Path + ";C:\Program Files\WTT 2.2\Studio"
    $WTTPath = $env:ProgramFiles + "\WTT 2.2\Studio"
}
else
{
    #$env:Path=$env:Path + ";C:\Program Files (x86)\WTT 2.2\Studio"
    $WTTPath = ${env:ProgramFiles(x86)} + "\WTT 2.2\Studio"
}
push-location $WTTPath 
Get-Location | Write-Host

#----------------------------------------------------------------------------------
# ScheduleJob 947 to clean up the test machine under $\PrivateBJ\LabDailyRun 
#----------------------------------------------------------------------------------

cmd /c "WttCl.exe ScheduleJob /DataStore:ProtocolTest /JobId:947 /MachinePool:$\PrivateBJ\LabDailyRun /Runall" 2>&1 | Write-Host
Write-Host "The End"
