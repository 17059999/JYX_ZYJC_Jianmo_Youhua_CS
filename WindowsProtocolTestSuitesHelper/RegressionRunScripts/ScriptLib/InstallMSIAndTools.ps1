#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           InstallMSIandTools.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 7
##
##############################################################################

$testDir = $env:SystemDrive + "\Temp"
$signalfile = "$testDir\InstallMSIAndTools.Completed.signal"
if(Test-Path $signalfile)
{
    Remove-Item $signalfile -Force
}

Start-Transcript -Path "$testDir\InstallMSIAndTools.ps1.log" -Append -Force

$computerName = Get-Content "$testDir\name.txt"

$ConfigureFile = "$testDir\Protocol.xml"
[xml]$xmlContent = Get-Content $ConfigureFile
if($xmlContent -eq $null)
{
    Write-Host "Protocol configure file $ConfigureFile is invalid." -ForegroundColor Red
    Stop-Transcript
    exit 1
}

$VMs = $xmlContent.SelectNodes("lab/servers/vm")
$ParametersNode = $xmlContent.SelectNodes("lab/Parameters")
$EnableLDAP = $false
if(($null -ne $ParametersNode) -and ($ParametersNode.HasChildNodes))
{
    foreach($arg in $ParametersNode.ChildNodes)
    {
        # Node type is Element and it has no child
        if(($arg.NodeType -eq [Xml.XmlNodeType]::Element) -and (-not $arg.HasChildNodes))
        {
            if(($arg.Name -eq "ContextName") -and ($arg.Value -match "EnableLDAP"))
            {
                $EnableLDAP = $true
            }
        }
    }
}
foreach($VMNode in $VMs)
{
    if($VMNode.name -eq $computerName)
    {
        $VM = $VMNode
    }
}

if($VM -eq $null)
{
    Write-Host "Cannot find Vm configure for Vm $computerName." -ForegroundColor Red
    Stop-Transcript
    exit 1
}

if($null -eq $VM.Tools)
{
    Write-Host "Cannot find Vm Tools configure for Vm $computerName." -ForegroundColor Red
    Stop-Transcript
    exit 0
}

$Tools = $VM.Tools.GetElementsByTagName("tool")
$IsVcredistInstalled = $false
$currentOSBuild = [System.Environment]::OSVersion.Version.Build.ToString();

foreach($Tool in $Tools)
{
    $CPUArchitecture = $Tool.CPUArchitecture
    $ArgumentList = $Tool.ArgumentList
    $MSIName = $Tool.MSIName
    $NotSupportedOSBuilds = $Tool.NotSupportedOSBuilds

    if($MSIName -ne $null)
    {
        if(($NotSupportedOSBuilds -ne $null) -and ($NotSupportedOSBuilds -match $currentOSBuild)) {
            continue;
        }
        Write-host "Install tool: $testDir\deploy\$MSIName"
        if($ArgumentList -ne $null)
        {
            cmd /c "msiexec /i $testDir\deploy\$MSIName $ArgumentList" 2>&1 | Write-Host
        }else {
            cmd /c msiexec /quiet /i $testDir\deploy\$MSIName 2>&1 | Write-Host
        }
    }
    else
    {
        $EXEName = $Tool.EXEName
        if($EXEName -ne $null)
        {
            if($EXEName -eq "vcredist.exe")
            {
                if($IsVcredistInstalled -eq $false)
                {
                    $IsVcredistInstalled = $true
                }
                else
                {
                    $EXEName = $CPUArchitecture + "_vcredist.exe"
                }
            }

            $SupportedOSBuilds = $Tool.SupportedOSBuilds
            $InstallWaitSeconds = $Tool.InstallWaitSeconds
            if($SupportedOSBuilds -ne $null)
            {
                if($EnableLDAP -eq $false)
                {
                    # Only when disabling LDAP, we use PowerShell remoting with installing WMF on 12R2.
                    foreach($SupportedOSBuild in $SupportedOSBuilds.Split(","))
                    {
                        if($SupportedOSBuild -eq $currentOSBuild)
                        {
                            Write-Host "install tool on OSBuild $SupportedOSBuild : $testDir\deploy\$EXEName $ArgumentList"
                            CMD /C "$testDir\deploy\$EXEName $ArgumentList" 2>&1 | write-host
                            # Wait the msu installation to complete.
                            if($InstallWaitSeconds -ne $null)
                            {
                                Start-Sleep -s $InstallWaitSeconds
                            }
                            break
                        }
                    }
                }
                else
                {
                    Write-Host "Will not install $testDir\deploy\$EXEName."
                }
            }
            else
            {
                Write-Host "install tool: $testDir\deploy\$EXEName $ArgumentList"
                CMD /C "$testDir\deploy\$EXEName $ArgumentList" 2>&1 | write-host
            }
        }
        else
        {
            $ZipName = $Tool.ZipName
            if($ZipName -ne $null)
            {
                $targetFolder = $Tool.targetFolder
                $installScript = $Tool.installScript

                if(!(Test-Path -Path $targetFolder))
                {
                    Write-Host "create folder: $targetFolder"
                    New-Item -ItemType directory -Path $targetFolder
                }

                Write-Host "expand zip tool: $testDir\deploy\$ZipName $targetFolder"
                # 12R2 with powershell 4.0 doesn't support Expand-Archive , so workaround as below solution.
                Add-Type -Assembly "System.IO.Compression.FileSystem"
                [IO.Compression.ZipFile]::ExtractToDirectory("$testDir\deploy\$ZipName", "$targetFolder")

                if($installScript -ne $null)
                {
                    Write-Host "install script of zip tool: $targetFolder\$installScript"
                    & "$targetFolder\$installScript"
                }
            }
        }
    }
}

$TestsuiteMSIs = $VM.Tools.GetElementsByTagName("TestsuiteMSI")

foreach($TestsuiteMSI in $TestsuiteMSIs)
{
    $MSIName = $TestsuiteMSI.MSIName
    $targetEndpoint = $TestsuiteMSI.targetEndpoint

    write-host "Install test suite: $MSIName"
    if($targetEndpoint -eq $null -or $targetEndpoint -eq "")
    {
        cmd /c msiexec -i $testDir\deploy\$MSIName -q
    }
    else
    {
        cmd /c msiexec -i $testDir\deploy\$MSIName -q TARGET_ENDPOINT=$targetEndpoint
    }
}

$TestsuiteZips = $VM.Tools.GetElementsByTagName("TestsuiteZip")
foreach($TestsuiteZip in $TestsuiteZips)
{
    $ZipName = $TestsuiteZip.ZipName
    $targetFolder = $TestsuiteZip.targetFolder

    write-host "Expand test suite: $ZipName"
    if ($psversiontable.PSVersion.Major -ge 5)
    {
        Expand-Archive $testDir\deploy\$ZipName -DestinationPath $targetFolder
    }
    else
    {
        $shell = New-Object -com shell.application
        $zip = $shell.NameSpace("$testDir\deploy\$ZipName")
        if(!(Test-Path -Path $targetFolder))
        {
            New-Item -ItemType directory -Path $targetFolder
        }
        $shell.Namespace($targetFolder).CopyHere($zip.items(), 0x14)
    }
}

CMD /C ECHO "Completed" > $signalfile
Stop-Transcript
exit 0