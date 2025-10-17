set testDirInVM=%1
set clientOS=%2
set serverCPUArchitecture=%3
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
set logFile=%testResultsPath%\ConfigServer.log
ECHO _________________________________                    >  %logFile%
Echo Step into [StartConfigServer.bat]                    >> %logFile%

Echo Start to install server common tools.                >> %logFile%
start /w %scriptsPath%\InstallServerTools.bat %toolsPath% %scriptsPath% %serverCPUArchitecture% %logFile% %cluster% %testResultsPath%
Echo Finished installing server common tools.             >> %logFile%

Echo Add powershell path into system variable PATH.       >> %logFile%
set powershellPath=%SystemRoot%\system32\WindowsPowerShell\v1.0\
set path=%path%;%powershellPath%
set path                                                  >> %logFile%

Echo Config IP version.                                   >> %logFile%
powershell %scriptsPath%\Config-IP.ps1 %IPVersion% %testResultsPath%

Echo Config domain/workgroup environment.                 >> %logFile%
powershell %scriptsPath%\Join-Domain.ps1 %workgroupDomain% %domainInVM% %userNameInVM% %userPwdInVM% %testResultsPath%

rem After configIPAndDomain, the machine need to reboot
Echo Ready to run next step of server config.             >> %logFile%
Echo [RestartAndRun] Run protocol's script: %scriptsPath%\Config-Server.ps1 >> %logFile%
%scriptsPath%\RestartAndRun.bat "cmd /C powershell %scriptsPath%\Config-Server.ps1 %toolsPath% %scriptsPath% %binPath% %testResultsPath% %clientOS% %IPVersion% %workgroupDomain% %userNameInVM% %userPwdInVM% %domainInVM%"

ECHO Error: This should not be logged, because of the RestartAndRun.  >> %logFile%
Echo Step out [StartConfigServer.bat]                     >> %logFile%

exit 0