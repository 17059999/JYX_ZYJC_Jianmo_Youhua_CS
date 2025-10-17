##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################

Param (
    [int]$Phase = 1
)
# Full Path of the work folder where all scripts are located
[string]$WorkFolderPath = "C:\Temp"

$DvdDrive = Get-PSDrive -PSProvider FileSystem | foreach { New-Object System.IO.DriveInfo($_.name) | where { $_.drivetype -eq "CDRom" }} | Select-Object -First 1

Write-Host "=========================================================="
Write-Host "                  Start the installation                  "
Write-Host "=========================================================="

#------------------------------------------------------------------------------------------
# Format the input xml file and display it to the screen
#------------------------------------------------------------------------------------------
Function Format-TestSuiteXml {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    [xml]$Xml,
    [int]$Indent = 2)

    Process {
        $StringWriter = New-Object System.IO.StringWriter
        $XmlWriter = New-Object System.Xml.XmlTextWriter $StringWriter
        $XmlWriter.Formatting = "indented"
        $XmlWriter.Indentation = $Indent
        [xml]$Xml.WriteContentTo($XmlWriter)
        $XmlWriter.Flush()
        $StringWriter.Flush()

        # Output the result
        Write-Output $("`n" + $StringWriter.ToString())
    }
}

#------------------------------------------------------------------------------------------
# Install MSI
#------------------------------------------------------------------------------------------
function Install-MSI {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Path        
    )
    Write-Host "Execute the installation file: $Path"
    [int]$num = 0
    [bool]$flag = $false
    while (((Get-Process msiexec -ErrorAction Ignore) | Where-Object {$_.SI -eq 1}).Count -gt 0) {
        Write-Host "There are unfinished processes, Try again in 5 seconds"
        if($num -ge 20){
            Write-Host "Exceeding maximum quantity"
            $flag = $true
            break
        }
        Start-Sleep -Seconds 10
        $num++
    }
    if($flag){
        return
    }
    msiexec.exe /passive /I $Path
    Write-Host "The installation is complete."
    Start-Sleep -Seconds 10
}
Function Copy-FilesFromDVD {
    
    Write-Host "Prepare the server for setup."

    Write-Host "Create work folder if it does not exist."
    if(!(Test-Path $WorkFolderPath)) {
        mkdir $WorkFolderPath 
    }

    Write-Host "Check hardware."
    if ([System.String]::IsNullOrEmpty($Script:DvdDrive)) {
        Write-Host "No DVD Drive found in this server." -Exit
    }

    Write-Host "Copy test files from DVD to $WorkFolderPath"
    robocopy $Script:DvdDrive $Script:WorkFolderPath /E
}

function Read-Configuration {
    Write-Host "Get test suite setup configurations."
    Write-Host "Get contents from $($WorkFolderPath)\InstallPrerequisites.xml."
    [xml]$Script:Setup = Get-Content "$WorkFolderPath\InstallPrerequisites.xml"
    $Script:Setup | Format-TestSuiteXml -Indent 4
}

Function Set-AutoLogon {
    Param (
    $Username,
    $Password,
    $Count)

    Write-Host "Set AutoLogin for $Username, with AutologonCount $Count"

    # Setup Autologon on
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name AutoAdminLogon -Value 1
    
    # Set User Name
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name DefaultUserName -Value $Username
    
    # Set Password
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name DefaultPassword -Value $Password
    
    # Set Logon Count
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" -Name AutologonCount -Value $Count
}

function Set-Environment {
    Write-Host "Get test suite setup configurations."
    Read-Configuration  
    if($env:UserName -ne 'Administrator'){
        net user administrator /active:yes
        wmic useraccount where "name='$env:UserName'" set PasswordExpires=FALSE
    }

    $CmdLine = "net user $($Script:Setup.lab.servers.vm.username) $($Script:Setup.lab.servers.vm.password)"
    Write-Host $CmdLine
    Invoke-Expression $CmdLine

    Write-Host "Set AutoLogon $($Script:Setup.lab.servers.vm.username) - $($Script:Setup.lab.servers.vm.password)"    
    Set-AutoLogon -Username $Script:Setup.lab.servers.vm.username -Password $Script:Setup.lab.servers.vm.password -Count 999
}

function Execute-InstallPrerequisites {
    Write-Host "Execute the InstallPrerequisites.ps1"
    Push-Location "$WorkFolderPath\InstallPrerequisites"
    [array]$CategoryList = "BuildTestSuites"
    $Script:Setup.lab.testcase.test | ForEach-Object {
        $CategoryList += $_.protocol.replace('MS-','').replace("\Server","").replace("\Client","")
        #Install tools
        if(![String]::IsNullOrEmpty($_.tools)){
            $_.tools.split(";") | ForEach-Object {
                Write-Host "Install tool: $_"
                $toolName = $_
                $tool = $Setup.lab.tools.tool | Where-Object {$_.display -eq $toolName}
                if($tool -ne $null){
                    Install-MSI "$WorkFolderPath\Tools\$($tool.Name)"
                }
            }          
        }
    } 

    $CategoryList | Select -Unique | ForEach-Object {
        Write-Host "Call InstallPrerequisites.ps1, Category: $_"
        .\InstallPrerequisites.ps1 -Category $_ -ConfigPath ".\PrerequisitesConfig.xml"
        Start-Sleep -Seconds 30
    } 
    Pop-Location
}

function Execute-RunPhase2 {
    Write-Host "It will restart and call run.ps1, phase 2."
    New-ItemProperty -Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "ExecuteRun.ps1" -PropertyType String -Value "powershell `"$Script:WorkFolderPath\run.ps1 -Phase 2`"  -NoExit "
    Stop-Transcript
    Restart-Computer
}

function Execute-BuildCommand {
    Write-Host "Execute to build command"
    $Script:Setup.lab.testcase.test | ForEach-Object{
        Write-Host "Build: $($_.protocol)"
        & "$WorkFolderPath\TestSuites\$($_.protocol)\src\build.cmd"
     }
    Write-Host "Build all Testsuites finished"
}

function InstallMSIAndRunCase {
    Push-Location $WorkFolderPath
    Write-Host "Install Release MSI"    
    Get-Item "$WorkFolderPath\ReleaseMSI\*.msi" | ForEach-Object {
        Write-Host "Install $($_.Name)"
        Install-MSI "$WorkFolderPath\ReleaseMSI\$($_.Name)"
    }
    Write-Host "Install completed"
    Write-Host "Run All Case"
    $Script:Setup.lab.testcase.test | ForEach-Object {
        $path = "C:\MicrosoftProtocolTests\"+$_.protocol
        if($_.protocol.Contains("RDP")){
            $path += "-Endpoint"
        }
        [string]$dllPath = Get-ChildItem -Path $path -Name $_.dll -Recurse
        $dllPath = $path + "\" + $dllPath
        if([string]::IsNullOrEmpty($dllPath)) {
            Write-Error "Cannot find the dll :  $($_.dll)"  Exit
        }
        # It is used to store the address of the CaptureFileFolder
        [string]$CaptureFileFolder
        if(![System.String]::IsNullOrEmpty($_.networkCapture)) {
            $CaptureFileFolder = Check-NetworkCapture -dllPath $dllPath -networkCapture $_.networkCapture
            if(Test-Path $CaptureFileFolder){
                Remove-Item $CaptureFileFolder -Force
            }
        }
        try {
            $installPath = $dllPath.SubString(0,$dllPath.LastIndexOf('\'))
            cmd /c "$WorkFolderPath\common\runTestCase.cmd `"$installPath\$($_.dll)`" `"$installPath\$($_.config)`" $($_.case) $($_.protocol.replace("\","_"))" 
        }
        catch {
            $ErrorMessage = $_.Exception.Message
            Write-Host "Run $($_.protocol) case failed : $ErrorMessage"
        }
        if(!(Test-Path "$WorkFolderPath\TestResults")){
            mkdir "$WorkFolderPath\TestResults"
        }
        if(Test-Path $CaptureFileFolder){
            Copy-Item $CaptureFileFolder "$WorkFolderPath\TestResults" -Force -Recurse
        }
    }
    Write-Host "Run all TestCase finished"
}

function Check-NetworkCapture {
    Param (
        [string]$dllPath,
        [string]$networkCapture
    )
    $itemProperty = (Get-ItemProperty $dllPath).DirectoryName
    Write-Host "itemProperty: $itemProperty"
    if(!(Test-Path "$itemProperty\$($_.networkCapture)")) {
        Write-Error "Cannot find specified path: $itemProperty\$($_.networkCapture)" -Exit
    }
    # Load the file
    [xml]$xml = New-Object -TypeName XML
    $xml.Load("$itemProperty\$($_.networkCapture)")
    $groups = ($xml.TestSite.Properties.Group | Where-Object {$_.name -eq "PTF"}).Group | Where-Object {$_.name -eq "NetworkCapture"}
    $state = ($groups.Property | Where-Object {$_.name -eq "Enabled"}).value
    if($state -ne "true") {
        # Modify auto-cupture state
        ((($xml.TestSite.Properties.Group | Where-Object {$_.name -eq "PTF"}).Group | Where-Object {$_.name -eq "NetworkCapture"}).Property| Where-Object {$_.name -eq "Enabled"}).value = "true"
        $xml.Save("$itemProperty\$($_.networkCapture)");
    }
    [string]$CaptureFileFolder =  ($groups.Property | Where-Object {$_.name -eq "CaptureFileFolder"}).value
    return $CaptureFileFolder
}

function Execute-CaseNumberValidation {
    Write-Host "Build CaseNumberValidator"
    $testSuiteRoot = "$WorkFolderPath"
    $currentPath = Get-Location
    $caseNumberValidatorRoot = "$WorkFolderPath\CaseNumberValidator"
    Write-Host "testSuiteRoot: $testSuiteRoot"
    Write-Host "currentPath: $currentPath"
    Write-Host "caseNumberValidatorRoot: $caseNumberValidatorRoot"
    
    & "$caseNumberValidatorRoot\build.ps1" -TestSuiteRoot $testSuiteRoot
    $dropPath = "$caseNumberValidatorRoot\drop\CaseNumberValidator"
    $binPath = "$dropPath\bin"
    $testSuiteIntroPath = "$binPath\TestSuiteIntro.xml"
    if (-not (Test-Path $testSuiteIntroPath)) {
        Write-Host "TestSuiteIntro.xml does not exist."
        return
    }
  
    [xml]$testSuiteIntro = Get-Content $testSuiteIntroPath
    $testSuites = $testSuiteIntro.TestSuiteSelectionConfig.SelectNodes("//TestSuite")
    $testSuiteNames = $testSuites | ForEach-Object { $_.name }
    $validationResultsPath = "$WorkFolderPath\TestResults"
    Write-Host "validationResultsPath: $validationResultsPath"
  
    Set-Location $binPath
    foreach ($testSuiteName in $testSuiteNames) {
        $reportOutputPath = "$validationResultsPath\$testSuiteName.CaseNumberValidationResult.txt"
        cmd /c "CaseNumberValidator.exe --TestSuiteName `"$testSuiteName`" --ReportOutputPath `"$reportOutputPath`""  
    }
    Set-Location $currentPath
}

function End-Regression {
    Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "ExecuteRun.ps1" -ErrorAction Ignore
    Write-Host "Eject-DVDDrive"
    $ShellApplication = New-Object -Com Shell.Application
    $ShellApplication.Namespace(17).ParseName($($Script:DvdDrive).Name.Replace("\", "")).InvokeVerb("Eject")  
    Write-Host "Regression Compeleted"
}


function Main {
    Write-Host "Start logging."
    Start-Transcript -Path "$Script:WorkFolderPath\install.log" -Append -Force
    # Phase 1: Copy files from DVD to workfolder
    if($Phase -eq 1){        
        Write-Host "Execute run.ps1 - Phase 1"
        Write-Host "Copy files to workfolder"
        Copy-FilesFromDVD        
        Set-Environment        
        Execute-InstallPrerequisites
        Execute-RunPhase2
    }
    # Phase 2: Restart driver and run all case
    elseif ($Phase -eq 2) {
        Write-Host "Execute run.ps1 - Phase 2: Run all case"
        Read-Configuration
        Execute-BuildCommand
        InstallMSIAndRunCase
        Execute-CaseNumberValidation
        End-Regression
    }
    Stop-Transcript
}

Main