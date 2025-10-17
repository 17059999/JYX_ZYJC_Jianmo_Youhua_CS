Param
(
    # The name of the Test Suite, only used to fetch XML configuration file and specify log and vm folder
    [string]$TestSuiteName = "RDPServer",
    [string]$TSHelperFolder = "_Helper",
    [string]$TSRegressionFolder = "RunRegression",
    [string]$MsiFolder
)

    $InvocationPath = Split-Path $MyInvocation.MyCommand.Definition -parent
    Push-Location "$InvocationPath\..\..\"
    # Test Suite Root Folder under Jenkins workspace
    $TestSuiteRootPath = Get-Location
    $RegressionRunRoot = "$TestSuiteRootPath\$TSRegressionFolder"

    $testSuitePath      = "$TestSuiteName"

    if($TestSuiteName.Equals("RDPServer")){
       $testSuitePath = "RDP\Server"
    }
    if($TestSuiteName.Equals("RDPClient")){
       $testSuitePath = "RDP\Client"
    }

    # Create ProtocolTestSuite Folder, this folder will store Built MSI and files
    if (!(Test-Path -Path "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName")) {
       New-Item -ItemType Directory "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName"
    }else{
       Remove-Item "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\*" -Recurse -Force
    }

    if(($MsiFolder -ne [string]::Empty) -and (Test-Path $MsiFolder)){
    # start copy Msi file in MsiFolder to ProtocolTestSuite
    if (!(Test-Path -Path "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\deploy")) {
       New-Item -ItemType Directory "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\deploy"
    }

    if (!(Test-Path -Path "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\Scripts")) {
       New-Item -ItemType Directory "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\Scripts"
    }
      $MsiFolder=$MsiFolder.Replace("/","\").TrimEnd("\")
      Copy-Item "$MsiFolder\$testSuitePath\*" -Destination "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\deploy\" -Recurse -Force
      Copy-Item "$TestSuiteRootPath\CommonScripts\*" -Destination "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\Scripts\" -Recurse -Force
    }

    else{
    # start copy drop folder to ProtocolTestSuite
      Copy-Item "$TestSuiteRootPath\drop\TestSuites\$testSuitePath\*" -Destination "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\" -Recurse -Force
    }

      Copy-Item "$TestSuiteRootPath\TestSuites\$testSuitePath\setup\*" -Destination "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\" -Recurse -Force
      Copy-Item "$TestSuiteRootPath\$TSHelperFolder\TestSuites\$testSuitePath\setup\*" -Destination "$RegressionRunRoot\ProtocolTestSuite\$TestSuiteName\" -Recurse -Force

      # Create ScriptLib Folder, this folder will store shared scripts
    if (!(Test-Path -Path "$RegressionRunRoot\ScriptLib")) {
      New-Item -ItemType Directory "$RegressionRunRoot\ScriptLib"
     }
     # Copy ScriptLib and CommonScript to ScriptLib folder
      Copy-Item "$TestSuiteRootPath\$TSHelperFolder\RegressionRunScripts\ScriptLib\*" -Destination "$RegressionRunRoot\ScriptLib\" -Recurse -Force
      Copy-Item "$TestSuiteRootPath\CommonScripts\*" -Destination "$RegressionRunRoot\ScriptLib\" -Recurse -Force

     # Create TestResults Folder
    if (!(Test-Path -Path "$RegressionRunRoot\TestResults")) {
      New-Item -ItemType Directory "$RegressionRunRoot\TestResults"
     }

    # Create VSTORMLITE Folder
    if (!(Test-Path -Path "$RegressionRunRoot\VSTORMLITE")) {
       New-Item -ItemType Directory "$RegressionRunRoot\VSTORMLITE"
     }
     Copy-Item "$TestSuiteRootPath\$TSHelperFolder\RegressionRunScripts\VSTORMLITE\*" -Destination "$RegressionRunRoot\VSTORMLITE\" -Recurse -Force

     # Create AzureScripts Folder
    if (!(Test-Path -Path "$RegressionRunRoot\AzureScripts")) {
       New-Item -ItemType Directory "$RegressionRunRoot\AzureScripts"
     }
      Copy-Item "$TestSuiteRootPath\$TSHelperFolder\AzureScripts\*" -Destination "$RegressionRunRoot\AzureScripts\" -Recurse -Force

     Pop-Location