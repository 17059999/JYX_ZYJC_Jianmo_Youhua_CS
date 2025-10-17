#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-Tools.ps1
## Purpose:        Install windows component MSMQ.
## Version:        1.1 (16 Mar, 2009)
##
##############################################################################

param(
[string]$toolsPath,
[string]$testResultsPath,
[string]$CPUArchitecture,
[string]$platForm,
[string]$cluster,
[string]$configFile = $testResultsPath + "\..\scripts\InstallToolsConfig.xml",
[string]$customConfigFile = $testResultsPath + "\..\scripts\CustomConfigFile.xml"
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
$logFile = $testResultsPath + "\Install-Tools.ps1.log"
Start-Transcript $logFile -force
$cluster = $cluster.toUpper()

Write-Host "EXECUTING [Install-Tools.ps1] ..." -foregroundcolor cyan
Write-Host "`$toolsPath       = $toolsPath"
Write-Host "`$testResultsPath = $testResultsPath"
Write-Host "`$CPUArchitecture = $CPUArchitecture"
Write-Host "`$platForm        = $platForm"
Write-Host "`$cluster         = $cluster"
Write-Host "`$configFile = $configFile"
Write-Host "`$customConfigFile = $customConfigFile"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Install tools by config file"
    Write-host "Parm1: Path of tools. (Required)"
    Write-host "Parm2: Path to write execute log. (Required)"
    Write-host "Parm3: CPU Architecture. (Required)"
    Write-host "Parm4: Server or client side. (Required)"
    Write-host "Parm5: Milestone of cluster (Required)"
    Write-host "Parm6: Install tools by this config file. (Optional)"
    Write-host "Parm7: User defined install config file. (Optional)"
    Write-host
    Write-host "Example: Install-Tools.ps1 c:\test\tools c:\test\testResults x86 Server C8"
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
# Verify required parameters
#----------------------------------------------------------------------------
if ($toolsPath -eq $null -or $toolsPath -eq "")
{
    Throw "Parameter $toolsPath is required."
}
if ($testResultsPath -eq $null -or $testResultsPath -eq "")
{
    Throw "Parameter $testResultsPath is required."
}
if ($CPUArchitecture -eq $null -or $CPUArchitecture -eq "")
{
    Throw "Parameter $CPUArchitecture is required."
}
if ($platForm -eq $null -or $platForm -eq "")
{
    Throw "Parameter $platForm is required."
}
if ($cluster -eq $null -or $cluster -eq "")
{
    Throw "Parameter $cluster is required."
}
#----------------------------------------------------------------------------
# Retrieve config from config file
#----------------------------------------------------------------------------
# keep this CPU architecture for all tools defined in the config file
$CPUArchitectureOriginal = $CPUArchitecture

if((Test-Path $configFile) -eq $true)
{
    [xml]$configContent = Get-Content $configFile
    $toolNodes = $configContent.selectnodes("/Configuration/$platForm/$cluster/tool")
    
    if ($toolNodes -eq $null -or $toolNodes -eq "")
    {
        Throw "There is no tools config under Configuration/$platForm/$cluster path in $configFile file."
    }
    else
    {
        if (Test-Path $customConfigFile) 
        {
            [xml]$customConfigContent = Get-Content $customConfigFile
            $customToolNodes = $customConfigContent.selectnodes("/Configuration/$platForm/tool")
        }
        $isCustermized = $false
        
        if ($customToolNodes -eq $null -or $toolNodes -eq "") 
        {
            Write-Host "There is no tools config under Configuration/$platForm/$cluster path in $customConfigFile file, so take default install config"
            $isCustermized = $false
        }
        else
        {
            $isCustermized = $true
        }
        
        foreach($node in $toolNodes)
        {        
            $toolDisabled = $false;
            $name = $node.GetAttribute("name")
            if ($name -eq $null -or $name -eq "")
            {
                Throw "Tool's name must be specified in $configFile file under configuration/$platForm/$cluster path."
            }
            
            $version = $node.GetAttribute("version")
            if ($version -eq $null -or $version -eq "")
            {
                Throw "Tool's verion must be specified in $configFile file under configuration/$platForm/$cluster path."
            }
            
            # override the CPU architecture from custermized config file
            $CPUArchitectureInConfig = $node.GetAttribute("CPUArchitecture")
            
            if ($isCustermized -eq $true)
            {
                foreach ($customNode in $customToolNodes) 
                {
                    if ($name -eq $customNode.GetAttribute("name")) 
                    {
                        $toolDisabled = $customNode.GetAttribute("disable")
                        $version = $customNode.GetAttribute("version")
                        $CPUArchitectureInConfig = $customNode.GetAttribute("CPUArchitecture")
                    }
                }
            }
            
            if ($CPUArchitectureInConfig -ne $null -and $CPUArchitectureInConfig -ne "") 
            {
                $CPUArchitecture = $CPUArchitectureInConfig
            }
            else
            {
                $CPUArchitecture = $CPUArchitectureOriginal
            }
            
            # if this tool is disabled in custermized config file, do not install it.
            if ( $toolDisabled -ne $null)
            {
                if ($toolDisabled -eq "true")
                {
                    break
                }
            }
            
            # go throgh all tools  and install tools 
            if ($name -eq "DotNetFramework") 
            {
                Write-Host "Start to install .Net framework if needed:"
                
                if ($version.StartsWith("2.")) 
                {
                    if ((Test-Path "$env:HOMEDRIVE\Windows\Microsoft.NET\Framework\v2.0.50727\AppLaunch.exe") -eq $false)
                    {
                        Write-Host "Source file: $toolsPath\$name\$version\$CPUArchitecture\NetFx20SP1.exe"
                        cmd /c "$toolsPath\$name\$version\$CPUArchitecture\NetFx20SP1.exe /q" 2>&1 | Write-Host
                    }
                    else
                    {
                        Write-Host "The DotNetFramework $version is already installed."
                    }
                }
                else
                {
                    throw "The DotNetFramework $version is not supportted yet."
                }
            }
            elseif ($name -eq "vcredist")
            {
                Write-Host "Start to install VCRedist:"
                Write-Host "Source file: $toolsPath\vcredist\$version\$CPUArchitecture\vcredist.exe"
                cmd /c "$toolsPath\vcredist\$version\$CPUArchitecture\vcredist.exe /q" 2>&1 | Write-Host
            }
            elseif ($name -eq "mstest")
            {
                Write-Host "Start to install MSTest:"
                Write-Host "Source file: $toolsPath\mstest\$version\$CPUArchitecture\setup.exe"
                cmd /c "$toolsPath\mstest\$version\$CPUArchitecture\setup.exe /quiet" 2>&1 | Write-Host
                Write-Host "Add mstest into system variable PATH."
                $env:Path += ";$env:ProgramFiles\mstest\"
                cmd /c "set path=$env:Path" 2>&1 | Write-Host
                cmd /c "set path" 2>&1 | Write-Host
                
                Write-Host "Start to call SN.exe:"
                Write-Host "Source file: $toolsPath\mstest\$version\$CPUArchitecture\sn.exe"
                cmd /c "$toolsPath\mstest\$version\$CPUArchitecture\sn.exe /Vr *" 2>&1 | Write-Host
                
            }
            elseif ($name -eq "NetworkMonitor")
            {
                Write-Host "Start to install network monitor:"
                Write-Host "Source file: $toolsPath\NetworkMonitor\$version\$CPUArchitecture\Netmonpt3.msi"
                cmd /c "msiexec /quiet /i $toolsPath\NetworkMonitor\$version\$CPUArchitecture\Netmonpt3.msi" 2>&1 | Write-Host
            }
            elseif ($name -eq "NetworkMonitorParser")
            {
                Write-Host "Start to install network monitor parser:"
                Write-Host "Source file: $toolsPath\NetworkMonitor\$version\$CPUArchitecture\Microsoft_PT3_Parsers.msi"
                cmd /c "msiexec /quiet /i $toolsPath\NetworkMonitor\$version\$CPUArchitecture\Microsoft_PT3_Parsers.msi" 2>&1 | Write-Host
            }
            elseif ($name -eq "PTF")
            {
                Write-Host "Start to install PTF:"
                Write-Host "Source file: $toolsPath\PTF\$version\$CPUArchitecture\ProtocolTestFramework.msi"
                cmd /c "msiexec /quiet /i $toolsPath\PTF\$version\$CPUArchitecture\ProtocolTestFramework.msi" 2>&1 | Write-Host
            }
            elseif ($name -eq "ProtocolSDK")
            {
                Write-Host "Start to install ProtocolSDK:"
                Write-Host "Source file: $toolsPath\ProtocolSDK\$version\$CPUArchitecture\ProtocolStackSDK.msi"
                cmd /c "msiexec /quiet /i $toolsPath\ProtocolSDK\$version\$CPUArchitecture\ProtocolStackSDK.msi" 2>&1 | Write-Host
            }
            else
            {
                throw "The tool: $name is not supportted yet."
            }
        }
    }
}
else
{
    Throw "Config failed: The config file $configFile does not existed!" 
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Install-Tools.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow
Stop-Transcript

exit 0
