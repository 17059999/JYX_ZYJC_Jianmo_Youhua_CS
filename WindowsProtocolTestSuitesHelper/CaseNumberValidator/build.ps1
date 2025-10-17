param (
    [string]$TestSuiteRoot
)

if ([string]::IsNullOrEmpty($TestSuiteRoot)) {
    $TestSuiteRoot = $env:TestSuiteRoot
}

if (-not (Test-Path $TestSuiteRoot)) {
    throw "The TestSuiteRoot path: $TestSuiteRoot does not exist."
}

$sourceInTestSuiteRoot = Test-Path "$TestSuiteRoot\CaseNumberValidator"

$currentPath = $PSScriptRoot
if (-not $sourceInTestSuiteRoot) {
    Write-Host "Copy source to $TestSuiteRoot"
    Copy-Item -Path "$currentPath\..\CaseNumberValidator" -Destination "$TestSuiteRoot\CaseNumberValidator" -Recurse -Force
}

Write-Host "Set build tool path"
$vswherePath = foreach ($programPath in @($env:ProgramFiles, ${env:ProgramFiles(x86)})) {
    $pathToTest = "$programPath\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $pathToTest) {
        $pathToTest
    }
}

$output = & $vswherePath
$vsPathLine = $output | Where-Object { $_.StartsWith("installationPath") }
$regex = [System.Text.RegularExpressions.Regex]::new(".+: (.+)")
$vsPath = $regex.Match($vsPathLine).Groups[1].Value

$buildToolPath = Get-ChildItem "$vsPath\MSBuild\*" | ForEach-Object {
    if (Test-Path "$($_.FullName)\Bin\MSBuild.exe") {
        return "$($_.FullName)\Bin\MSBuild.exe"
    }
}

Write-Host "Build CaseNumberValidator"
cmd /c "nuget.exe restore `"$TestSuiteRoot\ProtocolTestManager\Kernel\Kernel.csproj`" -SolutionDirectory `"$TestSuiteRoot\ProtocolTestManager`""
cmd /c "nuget.exe restore `"$TestSuiteRoot\CaseNumberValidator\CaseNumberValidator.csproj`" -SolutionDirectory `"$TestSuiteRoot\CaseNumberValidator`""
cmd /c "`"$buildToolPath`" `"$TestSuiteRoot\CaseNumberValidator\CaseNumberValidator.sln`" /t:Clean;Rebuild /p:Platform=`"x64`" /p:Configuration=`"Debug`""

Write-Host "Copy back binaries"
Remove-Item "$currentPath\..\CaseNumberValidator\drop" -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item "$TestSuiteRoot\CaseNumberValidator\x64\Debug\bin" -Destination "$currentPath\..\CaseNumberValidator\drop\CaseNumberValidator\bin" -Recurse -Force
Copy-Item "$TestSuiteRoot\CaseNumberValidator\x64\Debug\etc" -Destination "$currentPath\..\CaseNumberValidator\drop\CaseNumberValidator\etc" -Recurse -Force

if (-not $sourceInTestSuiteRoot) {
    Write-Host "Clean up"
    Remove-Item "$TestSuiteRoot\CaseNumberValidator" -Recurse -Force
}