Param
(
    [string]$TestSuiteName      = "Kerberos",
    [string]$EnvironmentName    = "Kerberos_Local_12R2.xml",
    [string]$ClientAnswerFile="",
    [string]$ClientDiskName="",
    [string]$ServerAnswerFile="",
    [string]$ServerDiskName="",
    [string]$BuildShareFolder="",
    #regressionType:Local or Azure
    [string]$RegressionType="Local"
)

Function CleanUp{
    Push-Location "$BuildShareFolder\LocalRegression" 
        .\Cleanup-TestSuiteEnvironment.ps1 -TestSuiteName $TestSuiteName -EnvironmentName $EnvironmentName
    Pop-Location
}

Function SetUpEnvironment{
    Push-Location "$BuildShareFolder\LocalRegression" 
        .\Setup-TestSuiteEnvironment.ps1 -TestSuiteName $TestSuiteName -EnvironmentName $EnvironmentName -ServerDiskName $ServerDiskName -ServerAnswerFile $ServerAnswerFile -ClientDiskName $ClientDiskName -ClientAnswerFile $ClientAnswerFile
    Pop-Location
}

Function ExcuteTestSuiteCases{
    Push-Location "$BuildShareFolder\LocalRegression" 
        .\Execute-TestSuiteCases.ps1 -TestSuiteName $TestSuiteName -EnvironmentName $EnvironmentName
    Pop-Location
}

Function Write-TestSuiteInfo {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    [string]$Message,
    [string]$ForegroundColor = "White",
    [string]$BackgroundColor = "DarkBlue")

    # WinBlue issue: Start-Transcript cannot write the log printed out by Write-Host, as a workaround, use Write-output instead
    # Write-Output does not support color
    if([Double]$Script:HostOsBuildNumber -eq [Double]"6.3") {
        ((Get-Date).ToString() + ": $Message") | Out-Host
    }
    else {
        Write-Host ((Get-Date).ToString() + ": $Message") -ForegroundColor $ForegroundColor -BackgroundColor $BackgroundColor
    }
}

Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "              Clean up                  "
Write-TestSuiteInfo "============================================================"
CleanUp

Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "              Setup-TestSuiteEnvironment                  "
Write-TestSuiteInfo "============================================================"
SetUpEnvironment

Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "              Execute-TestSuiteCases                  "
Write-TestSuiteInfo "============================================================"
ExcuteTestSuiteCases

Write-TestSuiteInfo ""
Write-TestSuiteInfo "============================================================"
Write-TestSuiteInfo "              Complete Run-LocalTestSuiteRegression                  "
Write-TestSuiteInfo "============================================================"

