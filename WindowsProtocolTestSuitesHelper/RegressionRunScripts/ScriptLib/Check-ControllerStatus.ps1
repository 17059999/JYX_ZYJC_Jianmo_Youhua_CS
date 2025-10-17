#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

param(
    [ValidateSet("CreateCheckerTask", "StartChecker")]
    [string]$action="CreateCheckerTask"
)

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$scriptName = $MyInvocation.MyCommand.Path
$env:Path += ";$scriptPath;$scriptPath\Scripts"
$TaskName = "CheckControllerStatus"

#----------------------------------------------------------------------------
# Common functions
#----------------------------------------------------------------------------
# Function to Control Writing Information to the screen
Function WriteInfo {
    [Parameter(ValueFromPipeline=$True)]
	Param([string]$msg,
    $ForegroundColor = "Green",
    $BackgroundColor = "Black"
    )

    $osbuildnum= "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
    if([double]$osbuildnum -eq [double]"6.3")
    {
        # WinBlue issue: Start-Transcript cannot write the log printed out by Write-Host, as a workaround, use Write-output instead
        # Write-Output does not support color
        Write-Output "$msg"
    }
    else
    {
        Write-Host ((get-date).ToString() + ": $msg") -ForegroundColor $ForegroundColor -BackgroundColor $BackgroundColor
    }    
}

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
[string]$logFile = $MyInvocation.MyCommand.Path + ".log"
Start-Transcript -Path "$logFile" -Append -Force


#----------------------------------------------------------------------------
# Create checker task
#----------------------------------------------------------------------------
if($action -eq "CreateCheckerTask")
{
    $Task = "PowerShell $scriptName StartChecker"
    WriteInfo "Create controller checker task: $TaskName"    
    WriteInfo "This task will start every 10 minutes to check controller.ps1 process."
    WriteInfo "This task will start only user is logon."
    WriteInfo "This task will be executed by users in Administrators group."
    CMD.exe /C "schtasks /Create /RU Administrators /SC MINUTE /MO 10 /TN `"$TaskName`" /TR `"$Task`" /IT /F" 
}


#----------------------------------------------------------------------------
# Start checker task
#----------------------------------------------------------------------------
if($action -eq "StartChecker")
{
    $autoRunKey = get-ItemProperty -Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Run"
    $installKey = $autoRunKey.Install
    if($installKey -ne $null)
    {
        $process = Get-WmiObject Win32_Process -Filter "Name like '%powershell%'" | where {$_.CommandLine -match "controller.ps1"}
        if($process -ne $null)
        {
            WriteInfo "Controller.ps1 is Running."
        }
        else
        {
            # wait for another 2 minutes and check the controller.ps1 status again
            Start-Sleep -s 120

            $process = Get-WmiObject Win32_Process -Filter "Name like '%powershell%'" | where {$_.CommandLine -match "controller.ps1"}
            if($process -ne $null)
            {
                WriteInfo "Controller.ps1 is Running."
            }
            else
            {
                WriteInfo "Controller.ps1 is NOT Running, restart computer to let it auto run."
                WriteInfo "Call command: shutdown /r /f /t 10 /d P:2:4"
                shutdown /r /f /t 10 /d P:2:4
            }
        }
    }
    else
    {
        WriteInfo "Disable controller checker task: $TaskName because auto run controller.ps1 has been removed."        
        CMD.exe /C "schtasks /Change /TN `"$TaskName`" /Disable" 2>&1 | WriteInfo 
    }
}

#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
Stop-Transcript
exit 0