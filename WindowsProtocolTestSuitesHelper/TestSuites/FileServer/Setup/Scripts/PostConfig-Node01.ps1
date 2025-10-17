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
$ConfigFile = "c:\temp\Protocol.xml"

$CurrentScriptPath 		 = $MyInvocation.MyCommand.Definition
$ScriptsSignalFile       = "C:\PostScript.Completed.signal" # Config signal file
$IsCluster               = $false
try {
    [xml]$content = Get-Content $ConfigFile -ErrorAction Stop
    $currentHa = $content.lab.ha
    if(![string]::IsNullOrEmpty($currentHa)){
        $IsCluster = $true;
    }
}
catch {

}

# Change to absolute path
Push-Location $WorkingPath

#-----------------------------------------------------------------------------
# Function: Prepare
# Usage   : Start executing the script; Push directory to working directory
# Params  :
# Remark  :
#-----------------------------------------------------------------------------
Function Prepare()
{
    .\Write-Info.ps1 "Executing [PostScript-Node01.ps1] ..." -ForegroundColor Cyan

    # Check signal file
    if(Test-Path -Path $ScriptsSignalFile)
    {
        .\Write-Info.ps1 "The script execution is complete." -ForegroundColor Red
        exit 0
    }

    $LogPath     = "$WorkingPath\PostConfig-Node01.ps1.log"
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

#Uninstall the .NET 6 runtime and SDK
Function Uninstall-DotNet6Components 
{
    # Define two registry paths for uninstall information of 64-bit and 32-bit programs.
    $uninstallPaths = @(
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
    )
    # Search all paths.
    $net6Components = foreach ($path in $uninstallPaths) 
    {
        Get-ItemProperty $path -ErrorAction SilentlyContinue | Where-Object {
            $_.DisplayName -match "Microsoft" -and
            $_.DisplayName -match "NET" -and
            $_.DisplayName -match "6\." -and
            $_.UninstallString
        } | Select-Object DisplayName, UninstallString
    }
    # If no matching components are found, show a message and exit the function.
    if (-not $net6Components -or $net6Components.Count -eq 0) 
    {
        .\Write-Info.ps1 "No .NET 6 related components found."
        exit
    }
    # uninstall
    foreach ($component in $net6Components) {
        $displayName = $component.DisplayName
        $uninstallCmd = $component.UninstallString.Trim()
        .\Write-Info.ps1 "Uninstalling: $displayName"
        if ($uninstallCmd -match '^MsiExec\.exe\s+/X\{.+\}$') 
        {
            Start-Process "MsiExec.exe" -ArgumentList "$($uninstallCmd -replace '^MsiExec\.exe\s+', '') /qn" -Wait
        }
        elseif ($uninstallCmd -match '^MsiExec\.exe\s+/I\{.+\}$') 
        {
            $args = $uninstallCmd -replace '^MsiExec\.exe\s+/I', '/X'
            Start-Process "MsiExec.exe" -ArgumentList "$args /qn" -Wait
        }
        elseif ($uninstallCmd -match '^".+\.exe"') 
        {
            try {
                Invoke-Expression "& $uninstallCmd /quiet /norestart"
            }
            catch {
                .\Write-Info.ps1 "Failed to uninstall using: $uninstallCmd"
            }
        }
        # Unrecognized uninstall command format – display a warning.
        else
        {
            .\Write-Info.ps1 "Unrecognized uninstall string: $uninstallCmd"
        }
    }
    .\Write-Info.ps1 "All .NET 6 related components have been uninstalled."
}

Function Phase1
{
    .\Write-Info.ps1 "Entering Phase 1..."

    # Join Domain
    .\domainjoin.ps1
}

Function Phase2
{
    .\Write-Info.ps1 "Entering Phase 2..."

    # Wait for computer to be stable
    Start-Sleep 30

    # Turn off firewall
    .\Write-Info.ps1 "Disable firewall..." -ForegroundColor Yellow
    .\Disable_Firewall.ps1

    # Set Password Never Expires
    .\Write-Info.ps1 "Set Password Never Expires..." -ForegroundColor Yellow
    .\Scripts\SetPasswordNeverExpires.ps1

    if($IsCluster)
    {
        # Connect IscsiTarget
        .\Write-Info.ps1 "Connect IscsiTarget..." -ForegroundColor Yellow
        .\Scripts\Connect-IscsiTarget.ps1

        # Create Server Failover Environment
        .\Write-Info.ps1 "Create Server Failover Environment..." -ForegroundColor Yellow
        .\Scripts\Create-ServerFailoverEnv.ps1
    }

    # Create QUIC Environment
    .\Write-Info.ps1 "Create QUIC Environment..." -ForegroundColor Yellow
    .\Scripts\Create-QUICEnv.ps1

    # Create SMB2 Environment
    .\Write-Info.ps1 "Create SMB2 Environment..." -ForegroundColor Yellow
    .\Scripts\Create-SMB2Env.ps1

    # Create DFSC Environment
    .\Write-Info.ps1 "Create DFSC Environment..." -ForegroundColor Yellow
    .\Scripts\Create-DFSCEnv.ps1

    # Create FSA Environment
    .\Write-Info.ps1 "Create FSA Environment..." -ForegroundColor Yellow
    .\Scripts\Create-FSAEnv.ps1

    # Create Auth Environment
    .\Write-Info.ps1 "Create Auth Environment..." -ForegroundColor Yellow
    .\Scripts\Create-AuthEnv.ps1

    if($IsCluster)
    {
        # Create Asymmetry Mode
        .\Write-Info.ps1 "Create Asymmetry Mode..." -ForegroundColor Yellow
        .\Scripts\Config-AsymmetryMode.ps1
    }

    # Set RequireSigning
    .\Write-Info.ps1 "Set RequireSigning..." -ForegroundColor Yellow
    .\Scripts\Set-RequireSigning.ps1

    if($IsCluster)
    {
        # Copy Vhdx
        .\Write-Info.ps1 "Copy Vhdx..." -ForegroundColor Yellow
        .\Scripts\Copy-Vhdx.ps1
    }

    # Set ComputerPassword
    .\Write-Info.ps1 "Set ComputerPassword..." -ForegroundColor Yellow
    .\Scripts\Set-ComputerPassword.ps1

    if($IsCluster)
    {
        # Check Cluster Node Status
        .\Write-Info.ps1 "Check Cluster Node Status..." -ForegroundColor Yellow
        .\Scripts\Check-ClusterNodeStatus.ps1
    }

    # Config ForceLevel2
    .\Write-Info.ps1 "Config ForceLevel2..." -ForegroundColor Yellow
    .\Scripts\Config-ForceLevel2.ps1

    #Uninstall the .NET 6 runtime and SDK
    Uninstall-DotNet6Components 
}

Function Finish
{
    # Write signal file
    .\Write-Info.ps1 "Write signal file: PostScript.Completed.signal to system drive."
    cmd /C ECHO CONFIG FINISHED>$ScriptsSignalFile

    # Ending script
    .\Write-Info.ps1 "post Config finished."
    .\Write-Info.ps1 "EXECUTE [PostConfig-Node01.ps1] FINISHED (NOT VERIFIED)." -ForegroundColor Green

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
            Phase2;
            RestartAndResume;
        }
        3{
            Finish;
        }
    }
}

Main

Pop-Location