#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-TimeStamp.ps1
## Purpose:        Record timestamp which is used to calculate the test config/run time.
## Version:        1.0 (19 Oct, 2009)
##
##############################################################################
param(
[string]$protocolName,
[string]$execState,
[string]$testResultDir
)

# Write Call Stack
if($function:EnterCallStack -ne $null)
{
	EnterCallStack "Get-TimeStamp.ps1"
}

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-Timestamp.ps1]." -foregroundcolor cyan
Write-Host "`$protocolName   = $protocolName"
Write-Host "`$execState      = $execState"
Write-Host "`$testResultDir  = $testResultDir"

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script will record the timestamp and calculate time periods (Seconds) according to different execution states."
    Write-host
    Write-Host "Param ([string]`$protocolName,[string]`$execState,[string]`$testResultDir)"
    Write-Host 
    Write-host "Available execState value: initial, startconfig*, startruntest*, testdone*"
    Write-host
    Write-host "Example: Get-TimeStamp.ps1 MS-NRLS testdone D:\PCTLabTest\TestResults"
    Write-host
}

#----------------------------------------------------------------------------
# Verify Required parameters
#----------------------------------------------------------------------------
if($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#----------------------------------------------------------------------------
# Validate parameter
#----------------------------------------------------------------------------
#if (!($protocolName.StartsWith("MS-") -or $protocolName.StartsWith("MC-")))
#{
#    Show-ScriptUsage
#    Throw "Parameter protocolName is required!"
#}
if($testResultDir -eq $null -or $testResultDir -eq "")
{
    Show-ScriptUsage
    Throw "Parameter testResultDir is required!"    
}
if(!(Test-Path $testResultDir))
{
    Show-ScriptUsage
    Throw "Parameter testResultDir does not exist!"
} 

$timestampFile = $testResultDir + "\"+ $protocolName + "_Timestamp.txt" 
        
        
switch($execState)
{
    #initialization: in file timestamp.txt, set configTimePeriod and runtestTimePeriod to 0 (Minutes).
    {$execState.ToLower().Equals("initial")}
    {
        if(Test-Path $timestampFile)
        {
            Write-Host "timestamp record file exists and will be deleted during initialization."
            Remove-Item $timestampFile -Force
        }
        Add-Content -Path $timestampFile "configTimePeriod (Minutes):"
        Add-Content -Path $timestampFile 0
        Add-Content -Path $timestampFile "runtestTimePeriod (Minutes):" 
        Add-Content -Path $timestampFile 0  
        Add-Content -Path $timestampFile "******************************" 
    }
    
    #record the timestamp to start configuring in VMs
    #should check if timestamp.txt existed first
    {$execState.ToLower().StartsWith("startconfig")}
    {
        $now = Get-Date
        if(Test-Path $timestampFile)
        { 
            if((Get-Content -Path $timestampFile).Count -ge 4)
            {
                Write-Host "write the $execState timestamp $now to $timestampFile..."
                Add-Content -Path $timestampFile "$execState time:"
                Add-Content -Path $timestampFile $now
            }
            else
            {
                Throw "The content in timestamp.txt is illegal! Please call ""Get-TimeStamp.ps1 initial `$testResultDir"" firstly "
            }
        }
        else
        {
            Throw "timestamp.txt does not exist! Please call ""Get-TimeStamp.ps1 initial `$testResultDir"" firstly "
        } 
    }
    
    #record the timestamp to start running testcase in VMs
    #should check whethter the last but one line is config timestamp line; or it should throw an error
    {$execState.ToLower().StartsWith("startruntest")}
    {
        $now = Get-Date
        if(Test-Path $timestampFile)
        { 
            $temp = Get-Content -Path $timestampFile
            $tempcount = $temp.count
            if($temp[$tempcount-2].ToLower().contains("startconfig"))
            {
                Write-Host "write the $execState timestamp $now to $timestampFile..."
                Add-Content -Path $timestampFile "$execState time:"
                Add-Content -Path $timestampFile $now
            }
            else
            {
               Throw "startconfig time should be recorded before startruntest time!" 
            }
        }
        else
        {
            Throw "timestamp.txt does not exist!"
        } 
    }
    
    #record the timestamp finishing running testcase in VMs
    #should check whethter the last but one line is runtest timestamp line; or it should throw an error
    {$execState.ToLower().StartsWith("testdone")}
    {
        $now = Get-Date
        if(Test-Path $timestampFile)
        { 
            $temp = Get-Content -Path $timestampFile
            $tempcount = $temp.count
            if($temp[$tempcount-2].ToLower().contains("startruntest"))
            {
                Write-Host "write the $execState timestamp $now to $timestampFile..."
                Add-Content -Path $timestampFile "$execState time:"
                Add-Content -Path $timestampFile $now
                
                #calculate time periods (Seconds) and record them to timestamp.txt
                #$temp = Get-Content -Path $timestampFile            
                $configTimePeriod  = [int](([DateTime]$temp[$tempcount-1]-[DateTime]$temp[$tempcount-3]).TotalMinutes) + [int]$temp[1]
                $runtestTimePeriod = [int](($now-[DateTime]$temp[$tempcount-1]).TotalMinutes) + [int]$temp[3]
                
                $content = Get-Content -Path $timestampFile
                $content[1] = $configTimePeriod
                $content[3] = $runtestTimePeriod
                Set-Content -Path $timestampFile $content
                Add-Content -Path $timestampFile "******************************"
            
                Write-Host "configTimePeriod (Minutes) is changed to $configTimePeriod minutes..."
                Write-Host "runtestTimePeriod (Minutes) is changed to $runtestTimePeriod minutes..."
            }
            else
            {
               Throw "startruntest time should be recorded before testdone time!" 
            }
        }
        else
        {
            Throw "timestamp.txt does not exist!"
        } 
    }
    
    #error parameter, show ScriptUsage
    default 
    {
        Show-ScriptUsage
        Throw "Parameter execState's value $execState is not valid!"         
    }
}

# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Get-TimeStamp.ps1"
}