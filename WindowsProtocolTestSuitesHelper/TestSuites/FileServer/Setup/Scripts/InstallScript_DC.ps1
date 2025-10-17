###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.
###########################################################################################

Param
(
	$WorkingPath      	 = "C:\Temp"
)
$CurrentScriptPath 		 = $MyInvocation.MyCommand.Definition
$ScriptsSignalFile = "C:\Install.Completed.signal" # Config signal file
Push-Location $WorkingPath

#-----------------------------------------------------------------------------
# Function: Prepare
# Usage   : Start executing the script; Push directory to working directory
# Params  : 
# Remark  : 
#-----------------------------------------------------------------------------
Function Prepare()
{
    .\Write-Info.ps1 "Executing [InstallScript-DC.ps1] ..." -ForegroundColor Cyan
	
	# Check signal file
    if(Test-Path -Path $ScriptsSignalFile)
    {
        .\Write-Info.ps1 "The script execution is complete." -ForegroundColor Red
        exit 0
    }

    # Change to absolute path
    .\Write-Info.ps1 "Current path is $CurrentScriptPath" -ForegroundColor Cyan
    $WorkingPath = (Get-Item $WorkingPath).FullName

    $LogPath     = "$WorkingPath\InstallScript-DC.ps1.log"
    Start-Transcript -Path $LogPath
}

Function Phase1
{
    .\Write-Info.ps1 "Start to Create DC..."
    
    try {
        .\createdc.ps1    
    }
    catch {
        Write-Warning "Promote DC failed:$($_)"
    }
    
}

Function Finish
{
	# Write signal file
    .\Write-Info.ps1 "Write signal file: Install.Completed.signal to system drive."
    cmd /C ECHO CONFIG FINISHED>$ScriptsSignalFile

    # Ending script
    .\Write-Info.ps1 "InstallScript finished."
    .\Write-Info.ps1 "EXECUTE [InstallScript-DC.ps1] FINISHED (NOT VERIFIED)." -ForegroundColor Green

    Restart-Computer -Force
}
# Main Script Starts
Function Main
{
	Prepare

    Phase1
    
    Finish
}

Main

Pop-Location