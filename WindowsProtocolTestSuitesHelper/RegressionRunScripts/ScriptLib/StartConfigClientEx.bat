set testDirInVM=%1
set serverOS=%2
set clientCPUArchitecture=%3
set IPVersion=%4
set workgroupDomain=%5
set cluster=%6
set userNameInVM=%7
set userPwdInVM=%8
set domainInVM=%9

set binPath=%testDirInVM%\Bin
set toolsPath=%testDirInVM%\Tools
set scriptsPath=%testDirInVM%\Scripts
set testResultsPath=%testDirInVM%\TestResults

if not exist %testResultsPath% md %testResultsPath%
set logFile=%testResultsPath%\ConfigClient.log
ECHO _________________________________                > %logFile%
Echo Step into [StartConfigClient.bat]                >> %logFile%

Echo Start to install client common tools.            >> %logFile%
start /w %scriptsPath%\InstallClientTools.bat %toolsPath% %scriptsPath% %clientCPUArchitecture% %logFile% %cluster% %testResultsPath%
Echo Finished installing client common tools.         >> %logFile%

Echo Add powershell path into system variable PATH.   >> %logFile%
set powershellPath=%SystemRoot%\system32\WindowsPowerShell\v1.0\
set path=%path%;%powershellPath%
set path                                              >> %logFile%

Echo Config IP version.                               >> %logFile%
powershell %scriptsPath%\Config-IP.ps1 %IPVersion% %testResultsPath%

Echo Config domain/workgroup environment.             >> %logFile%
powershell %scriptsPath%\Join-Domain.ps1 %workgroupDomain% %domainInVM% %userNameInVM% %userPwdInVM% %testResultsPath%

Echo Config TestRunConfig file.                       >> %logFile%
powershell %scriptsPath%\Modify-TestRunConfig.ps1 %binPath%

rem After configIPAndDomain, the machine need to reboot
Echo Ready to run next step of client config.         >> %logFile%
Echo [RestartAndRun] Run protocol's script: %scriptsPath%\Config-Client.ps1    >> %logFile%
%scriptsPath%\RestartAndRun.bat "cmd /C powershell %scriptsPath%\Config-Client-Ex.ps1 %toolsPath% %scriptsPath% %binPath% %testResultsPath% %serverOS% %IPVersion% %workgroupDomain% %userNameInVM% %userPwdInVM% %domainInVM%"

ECHO Error: This should not be logged, because of the RestartAndRun.           >> %logFile%
Echo Step out [StartConfigClient.bat]                 >> %logFile%

exit 0