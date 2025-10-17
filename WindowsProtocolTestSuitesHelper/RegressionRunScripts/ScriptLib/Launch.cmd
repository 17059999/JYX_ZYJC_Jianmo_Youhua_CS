@echo off

rem ###############################################################################
rem ##
rem ## Purpose:        Script for Launching Test Suite
rem ## Version:        2.0 (1 June, 2011)
rem ## Requirements:   Windows Powershell 2.0
rem ##
rem ## Parameters:
rem ##   - ProtocolName:          Protocol name of the test suite to be 
rem ##                            launched.
rem ##                            E.g. MS-SMB2
rem ##   - Cluster:               Cluster of the test suite.
rem ##                            E.g. C7
rem ##   - ClientOS:              WXP/Vista/W2K3/W2K8/Win7/W2K8R2, etc.
rem ##   - ServerOS:              W2K3/W2K8/W2K8R2, etc.
rem ##   - workgroupDomain:       Workgroup/Domain
rem ##   - IPVersion:             IPv4 / IPv6
rem ##   - ClientCPUArchitecture: x86 / x64
rem ##   - ServerCPUArchitecture: x86 / x64
rem ##   - WorkingDrive:          C: / D: / E: / 
rem ##                            any path with READ&WRITE permissions.
rem ##   - memsize:		  Memory size needed by test suite (GB)
rem ##   - hdsize:		  Hard disk size needed by test suite (GB)
rem ##   - ipsize:                IP address number needed by test VM
rem ##
rem ## Example: To launch MS-SMB2 with context Vista-W2K8-Domain-IPv6-x86-
rem ##          x86, use D: as local working drive.
rem ## Command-line: Launch.cmd MS-SMB2 C7 Vista W2K8 Domain IPv6 x86 x86 D: 3 30 3
rem ##
rem ## WARNING: Existing VMs and visual networks in Hyper-V may be destroyed,
rem ##          please back up any important data in existing VMs or export 
rem ##          them to a safe place before running this script.
rem ##
rem ###############################################################################

set ProtocolName=%1
set Cluster=%2
set ClientOS=%3
set ServerOS=%4
set WorkgroupDomain=%5
set IPVersion=%6
set ClientCPUArchitecture=%7
set ServerCPUArchitecture=%8
set WorkingDrive=%9
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
set memsize=%1
set hdsize=%2
set ipsize=%3
set CentralFileRepositoryServer=%4
set LabAccountDomain=%5
set LabAccountPassword=%6
set LabAccountName=%7
set UserNameInVM=%8
set PasswordInVM=%9
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
SHIFT
set SendTo=%1
set SendCC=%2
set CustomizeScenario=""
set Endpoint=""
set Product=""

rem #------------------------------------------------------------------------
rem # Set Local parameters
rem #------------------------------------------------------------------------
set WinVMBasePath=%CentralFileRepositoryServer%\VMLib
set WinScriptLibPath=%CentralFileRepositoryServer%\ScriptLib
set WinToolsPath=%CentralFileRepositoryServer%\Tools
set WinTestSuitePath=%CentralFileRepositoryServer%\TestSuite
set WinISOPath=%CentralFileRepositoryServer%\ISO

set WorkingDir=%WorkingDrive%\PCTLabTest
set VMDir=%WorkingDir%\VM
set TestResultDir=%WorkingDir%\%ProtocolName%\TestResults
set DomainInVM=contoso.com
set TestDirInVM=%systemdrive%\Test

rem #------------------------------------------------------------------------
rem # Clean up test result directory
rem #------------------------------------------------------------------------
if exist %TestResultDir% rd /s /q %TestResultDir%

rem #------------------------------------------------------------------------
rem # Cleanup test suite directory
rem #------------------------------------------------------------------------
if exist %WorkingDir%\%ProtocolName% rd /s /q %WorkingDir%\%ProtocolName%

rem #------------------------------------------------------------------------
rem # Create working directory
rem #------------------------------------------------------------------------
if not exist %WorkingDir% md %WorkingDir%

rem #------------------------------------------------------------------------
rem # Create common script folder
rem #------------------------------------------------------------------------
if not exist %WorkingDir%\ScriptLib md %WorkingDir%\ScriptLib

rem #------------------------------------------------------------------------
rem # Create test result directory
rem #------------------------------------------------------------------------
if not exist %TestResultDir% md %TestResultDir%

rem #------------------------------------------------------------------------
rem # Give credential for accessing file server
rem #------------------------------------------------------------------------
net use %CentralFileRepositoryServer% %LabAccountPassword% /User:%LabAccountDomain%\%LabAccountName%

rem #------------------------------------------------------------------------
rem # Copy Common Scripts - Recursively
rem #------------------------------------------------------------------------
FOR /R %WinScriptLibPath% %%D IN (.) DO COPY /Y %%D\*.* %WorkingDir%\ScriptLib\

rem #------------------------------------------------------------------------
rem # Copy Common Tools - Robocopy
rem #------------------------------------------------------------------------
robocopy %WinToolsPath% %WorkingDir%\Tools /MIR

rem #------------------------------------------------------------------------
rem # Copy Protocol's folder - Robocopy
rem #------------------------------------------------------------------------
robocopy %WinTestSuitePath%\%ProtocolName%\%Endpoint% %WorkingDir%\%ProtocolName% /MIR

rem #------------------------------------------------------------------------
rem # Run test on host (call Schedule-TestJob.ps1)
rem #------------------------------------------------------------------------
powershell %WorkingDir%\ScriptLib\Schedule-TestJob.ps1 %ProtocolName% %Cluster% %WinVMBasePath% %WinScriptLibPath% %WinToolsPath% %WinTestSuitePath% %WinISOPath% %clientOS% %serverOS% %workgroupDomain% %IPVersion% %ClientCPUArchitecture% %ServerCPUArchitecture% %WorkingDir% %VMDir% %testResultDir% %UserNameInVM% %PasswordInVM% %DomainInVM% %TestDirInVM% %memsize% %hdsize% %ipsize% %CustomizeScenario% %SendTo% %SendCC%

rem #------------------------------------------------------------------------
rem # Remove remote credential
rem #------------------------------------------------------------------------
net.exe use %CentralFileRepositoryServer% /delete /y