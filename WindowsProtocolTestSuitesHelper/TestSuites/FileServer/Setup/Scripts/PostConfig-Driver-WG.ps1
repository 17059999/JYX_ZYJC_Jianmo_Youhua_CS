###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.
###########################################################################################

#------------------------------------------------------------------------------------------
# Parameters:
# Help: whether to display the help information
# Step: Current step for configuration
#------------------------------------------------------------------------------------------
Param
(
	$WorkingPath      	 = "C:\Temp",
    [int]$Step 			 = 1
)
$Parameters              = @{}
$CurrentScriptPath 		 = $MyInvocation.MyCommand.Definition
$ScriptsSignalFile = "C:\PostScript.Completed.signal" # Config signal file
Push-Location $WorkingPath

#-----------------------------------------------------------------------------
# Function: Prepare
# Usage   : Start executing the script; Push directory to working directory
# Params  : 
# Remark  : 
#-----------------------------------------------------------------------------
Function Prepare()
{
    .\Write-Info.ps1 "Executing [PostScript-Driver.ps1] ..." -ForegroundColor Cyan
	
	# Check signal file
    if(Test-Path -Path $ScriptsSignalFile)
    {
        .\Write-Info.ps1 "The script execution is complete." -ForegroundColor Red
        exit 0
    }

    # Change to absolute path
    .\Write-Info.ps1 "Current path is $CurrentScriptPath" -ForegroundColor Cyan
    $WorkingPath = (Get-Item $WorkingPath).FullName

    $LogPath     = "$WorkingPath\PostConfig-Driver.ps1.log"
    Start-Transcript -Path $LogPath -Append 2>&1 | Out-Null
}

# Utility Function 
Function RestartAndResume
{
    $NextStep = $Step + 1
    .\RestartAndRun.ps1 -ScriptPath $CurrentScriptPath `
                        -PhaseIndicator "-Step $NextStep" `
                        -AutoRestart $true
}

Function Phase1
{
	.\Write-Info.ps1 "Entering Phase 1..."
	
    # Turn off firewall
    .\Write-Info.ps1 "Disable firewall..." -ForegroundColor Yellow
    .\Disable_Firewall.ps1

    # Set Password Never Expires
    .\Write-Info.ps1 "Set Password Never Expires..." -ForegroundColor Yellow
    .\Scripts\SetPasswordNeverExpires.ps1

    # Config ForceLevel2
    .\Write-Info.ps1 "Config ForceLevel2..." -ForegroundColor Yellow
    .\Scripts\Config-ForceLevel2.ps1
}

Function Finish
{
	# Write signal file
    .\Write-Info.ps1 "Write signal file: PostScript.Completed.signal to system drive."
    cmd /C ECHO CONFIG FINISHED>$ScriptsSignalFile

    # Ending script
    .\Write-Info.ps1 "post Config finished."
    .\Write-Info.ps1 "EXECUTE [PostConfig-Driver.ps1] FINISHED (NOT VERIFIED)." -ForegroundColor Green
    
    .\RestartAndRunFinish.ps1
}

# Main Script Starts
Function Main
{
	Prepare

    switch ($Step)
    {
        1 { 
            Phase1; 
            RestartAndResume; 
        }
        2 { 
            Finish; 
        }
    }
}

Main
Pop-Location