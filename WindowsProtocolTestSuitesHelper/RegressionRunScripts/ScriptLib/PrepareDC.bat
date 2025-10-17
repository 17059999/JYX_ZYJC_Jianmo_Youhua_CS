set testDirInVM=%1
set clientOS=%2
set serverCPUArchitecture=%3
set IPVersion=%4
set userNameInVM=%5
set userPwdInVM=%6
set domainInVM=%7
set NetBiosdomainName=%8

set binPath=%testDirInVM%\Bin
set toolsPath=%testDirInVM%\Tools
set scriptsPath=%testDirInVM%\Scripts
set testResultsPath=%testDirInVM%\TestResults

if not exist %testResultsPath% md %testResultsPath%
set logFile=%testResultsPath%\PrepareDC.log
ECHO _________________________________                    >  %logFile%
Echo Step into [PrepareDC.bat]                            >> %logFile%

Echo Start to install server common tools.                >> %logFile%
start /w %scriptsPath%\InstallServerTools.bat %toolsPath% %scriptsPath% %serverCPUArchitecture% %logFile% %cluster% %testResultsPath%
Echo Finished installing server common tools.             >> %logFile%

Echo Add powershell path into system variable PATH.       >> %logFile%
set powershellPath=%SystemRoot%\system32\WindowsPowerShell\v1.0\
set path=%path%;%powershellPath%
set path                                                  >> %logFile%


Echo Config IP version.                                   >> %logFile%
powershell %scriptsPath%\Config-IP.ps1 %IPVersion% %testResultsPath%

rem Prepare DC
Echo SetupDC                                              >> %logFile%
Echo dcpromo /unattend /InstallDns:yes /dnsOnNetwork:yes /replicaOrNewDomain:domain /newDomain:forest /newDomainDnsName:%domainInVM% /DomainNetbiosName:%NetBiosdomainName%  /databasePath:"%SystemDrive%\ntds" /logPath:"%SystemDrive%\ntdslogs" /sysvolpath:"%SystemDrive%\sysvol" /safeModeAdminPassword:%userPwdInVM% /forestLevel:3 /domainLevel:3 /rebootOnCompletion:no >> %logFile%
cmd /c dcpromo /unattend /InstallDns:yes /dnsOnNetwork:yes /replicaOrNewDomain:domain /newDomain:forest /newDomainDnsName:%domainInVM% /DomainNetbiosName:%NetBiosdomainName%  /databasePath:"%SystemDrive%\ntds" /logPath:"%SystemDrive%\ntdslogs" /sysvolpath:"%SystemDrive%\sysvol" /safeModeAdminPassword:%userPwdInVM% /forestLevel:3 /domainLevel:3 /rebootOnCompletion:no

reg add HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon /v DefaultUserName /d %domainInVM%\%userNameInVM% /f
reg add HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon /v AltDefaultDomainName /d %domainInVM%\%userNameInVM% /f
reg add HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon /v DefaultPassword /d %userPwdInVM% /f

rem After setup domain, the machine need to reboot
%scriptsPath%\RestartAndRun.bat "cmd /C ECHO CONFIG FINISHED>%SystemDrive%\dc.config.finished.signal"

ECHO Error: This should not be logged, because of the RestartAndRun.  >> %logFile%
Echo Step out [StartConfigServer.bat]                     >> %logFile%

exit 0