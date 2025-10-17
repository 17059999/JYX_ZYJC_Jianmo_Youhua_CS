# Manual Run Scripts

These PowerShell scripts are used to manually set up event environments. It contains two environments, details see below.

## Prerequisites
1. Run [Pipeline for Manual Run script](https://iatsh.visualstudio.com/WindowsProtocolTestSuites/_build?definitionId=118)

## Local environments

1. System Requirements

   * Microsoft .NET Framework 4.5 or later
   * PowerShell 4.0 or later
   * Hyper-V

2. Config the parameters and Run script
   2.1 Copy as following folders from \\pet-storage-04\PrototestRegressionShare

      2.1.1 Please copy ToolShare to WinteropProtocolTesting\VHD

      2.1.2 Please copy Tools to WinteropProtocolTesting\Tools

      2.1.3 Please copy MediaShare to WinteropProtocolTesting\Media
    
   2.2 Download the artifact from [Pipeline for Manual Run script](https://iatsh.visualstudio.com/WindowsProtocolTestSuites/_build?definitionId=118)
   2.3 Copy the artifact file to Machine and Extract all file, E.g. the folder is D:\ManualScripts\

   2.4 Open file D:\ManualScripts\_Helper\RegressionRunScripts\ManualRunScripts\ManualRun-RegressionWithCSV.ps1 

   2.5 Edit Parameter $ConfigFile to the path of LocalRegression.csv

   2.6 Edit Parameter $msiFolder to the path of storing msi files E.g. msiFolder\FileServer\deploy\xxx.msi

   2.7 Edit $filter, it used to filter out which environments you want to build in the $ConfigFile 

   2.8 Run regression select the file  D:\ManualScripts\_Helper\RegressionRunScripts\ManualRunScripts\ManualRun-RegressionWithCSV.ps1, run with powershell as administrator 

3. Regression results folder 
   D:\WinteropProtocolTesting\TestResults\<TestSuteName>

## Azure environments

1. Requirements

  * Microsoft .NET Framework 4.5 or later
  * PowerShell 5.0 or later
  * Azure VM
  * VLan

2. Prepare Azure VM
   2.1 Download the artifact from [Pipeline for Manual Run script](https://iatsh.visualstudio.com/WindowsProtocolTestSuites/_build?definitionId=118)
   2.2 Copy artifact file to Machine and Extract all file, E.g. the folder is D:\ManualScripts\

   2.3 Install the Azure PowerShell module

   2.4 Import the [cert for azure](AzureScripts\Certificates\WindowsTestSuite.pfx)

2. Config the parameters and Run script
   2.1 Open file D:\ManualScripts\_Helper\RegressionRunScripts\ManualRunScripts\ManualRun-RegressionWithCSV.ps1 

   2.2 Edit Parameter $ConfigFile to the path of AzureRegression.csv

   2.3 Edit Parameter $msiFolder to the path of storing msi files E.g. msiFolder\FileServer\deploy\xxx.msi

   2.4 Edit $filter, it used to filter out which environments you want to build in the $ConfigFile 

   2.5 Run regression select the file  D:\ManualScripts\_Helper\RegressionRunScripts\ManualRunScripts\ManualRun-RegressionWithCSV.ps1, run with powershell

3. Regression results folder 
   ManualRegression\<TestSuiteName>_RunRegression\TestResults\