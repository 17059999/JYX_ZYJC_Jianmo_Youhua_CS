###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
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

$ConfigFile = "C:\Temp\Protocol.xml"
$IsCluster = $false
try {
    [xml]$content = Get-Content $ConfigFile -ErrorAction Stop
    $currentHa = $content.lab.ha
    if (![string]::IsNullOrEmpty($currentHa)) {
        $IsCluster = $true;
    }
}
catch {
    .\Write-Info.ps1 "Error occurred during geting the Cluster configuration"
    .\Write-Info.ps1 "Exception message: $($_.Exception.Message)"
}

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
    .\Write-Info.ps1 "Executing [PostScript-DC.ps1] ..." -ForegroundColor Cyan
	
	# Check signal file
    if(Test-Path -Path $ScriptsSignalFile)
    {
        .\Write-Info.ps1 "The script execution is complete." -ForegroundColor Red
        exit 0
    }

    # Change to absolute path
    .\Write-Info.ps1 "Current path is $CurrentScriptPath" -ForegroundColor Cyan
    $WorkingPath = (Get-Item $WorkingPath).FullName

    $LogPath     = "$WorkingPath\PostConfig-DC.ps1.log"
    Start-Transcript -Path $LogPath -Append 2>&1 | Out-Null
}

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
    
    # Turn off firewall
    .\Write-Info.ps1 "Disable firewall..." -ForegroundColor Yellow
    .\Disable_Firewall.ps1

    # Create Test Account
    .\Write-Info.ps1 "Create Test Account..." -ForegroundColor Yellow
    .\Scripts\Create-TestAccount.ps1

    # Create CbacObjectsInDC
    .\Write-Info.ps1 "Create CbacObjectsInDC..." -ForegroundColor Yellow
    .\Scripts\Create-CbacObjectsInDC.ps1

    # Import GPOForClaims
    .\Write-Info.ps1 "Import GPOForClaims..." -ForegroundColor Yellow
    .\Scripts\Import-GPOForClaims.ps1

    # Disable LDAP signing to skip "Novell.Directory.Ldap.LdapException: Strong Authentication Required" error when connect with 389 port.
    .\Write-Info.ps1 "Disable LDAP signing..." -ForegroundColor Yellow
    $params = Get-Item "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -ErrorAction SilentlyContinue
    if($params -ne $null)
    {
	    $ldapServerEnforceIntegrity = Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name 'LDAPServerEnforceIntegrity' -ErrorAction SilentlyContinue
	    if($ldapServerEnforceIntegrity -ne $null)
	    {
		    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name 'LDAPServerEnforceIntegrity' -Value '0x00000000' -Force | Out-Null
	    } else {
		    New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name 'LDAPServerEnforceIntegrity' -Value '0x00000000' -PropertyType 'DWord' -Force | Out-Null
	    }
	    $ldapServerIntegrity = Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name 'LDAPServerIntegrity' -ErrorAction SilentlyContinue
	    if($ldapServerIntegrity -ne $null)
	    {
		    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name 'LDAPServerIntegrity' -Value '0x00000001' -Force | Out-Null
	    } else {
		    New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name 'LDAPServerIntegrity' -Value '0x00000001' -PropertyType 'DWord' -Force | Out-Null
	    }
        $ldapEnforceChannelBinding=Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name "LdapEnforceChannelBinding" -ErrorAction SilentlyContinue
        if($ldapEnforceChannelBinding -ne $null)
        {
            Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name "LdapEnforceChannelBinding" -Value 1 -Force | Out-Null
        } else{
            New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters" -Name "LdapEnforceChannelBinding" -Value 1 -PropertyType DWORD  -Force | Out-Null
        }
    }

    # Create DNS records for cluster hosts
    if ($IsCluster) {
        .\Write-Info.ps1 "Create DNS records for cluster hosts..." -ForegroundColor Yellow
        .\Scripts\Create-DNSRecords.ps1
    }

    # Check DCStatus
    .\Write-Info.ps1 "Check DCStatus..." -ForegroundColor Yellow
    .\Scripts\Check-DCStatus.ps1

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
    .\Write-Info.ps1 "EXECUTE [PostConfig-DC.ps1] FINISHED (NOT VERIFIED)." -ForegroundColor Green
    
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
            Finish
        }
    }
}

Main

Pop-Location