set toolsPath=%1
set scriptsPath=%2
set CPUArchitecture=%3
set logFile=%4

ECHO ____________________________________       >> %logFile%
Echo Step into [InstallClientToolsC6.bat]       >> %logFile%

Echo Start to install .Net framework if needed: >> %logFile%
Echo Source file:  %toolsPath%\DotNetFramework\2.1.21022\%CPUArchitecture%\NetFx20SP1.exe    >> %logFile%
if not exist %SystemDrive%\Windows\Microsoft.NET\Framework\v2.0.50727\AppLaunch.exe (
%toolsPath%\DotNetFramework\2.1.21022\%CPUArchitecture%\NetFx20SP1.exe /q
)

Echo Start to install VCRedist:     >> %logFile%
Echo Source file:  %toolsPath%\vcredist\9.0.21022\%CPUArchitecture%\vcredist.exe >> %logFile%
%toolsPath%\vcredist\9.0.21022\%CPUArchitecture%\vcredist.exe /q

Echo Start to install MSTEST:       >> %logFile%
Echo Source file:  %toolsPath%\mstest\9.0.21206\%CPUArchitecture%\setup.exe >> %logFile%
%toolsPath%\mstest\9.0.21206\%CPUArchitecture%\setup.exe /quiet

Echo Start to call SN.exe:          >> %logFile%
Echo Source file:  %toolsPath%\mstest\9.0.21206\%CPUArchitecture%\sn.exe >> %logFile%
%toolsPath%\mstest\9.0.21206\%CPUArchitecture%\sn.exe /Vr *

Echo Start to install NetMon:       >> %logFile%
Echo Source file: %toolsPath%\NetworkMonitor\3.2.901\%CPUArchitecture%\Netmonpt3.msi >> %logFile%
msiexec /quiet /i %toolsPath%\NetworkMonitor\3.2.901\%CPUArchitecture%\Netmonpt3.msi

Echo Start to install PTF:          >> %logFile%
Echo Source file:  %toolsPath%\PTF\2.1.861\%CPUArchitecture%\ProtocolTestFramework.msi >> %logFile%
msiexec /quiet /i  %toolsPath%\PTF\2.1.861\%CPUArchitecture%\ProtocolTestFramework.msi

Echo Start to install PowerShell:   >> %logFile%
Echo Source file:  %toolsPath%\PowerShell\2.0_CTP\%CPUArchitecture%\PowerShell_Setup.msi >> %logFile%
msiexec /quiet /i "%toolsPath%\PowerShell\2.0_CTP\%CPUArchitecture%\PowerShell_Setup.msi"

Echo Start to change PowerShell config to RemoteSigned:     >> %logFile%
Echo Command: reg add HKLM\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell /v ExecutionPolicy /d "RemoteSigned" >> %logFile%
reg add HKLM\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell /v ExecutionPolicy /d "RemoteSigned"

Echo Finish to install C6 tools.           >> %logFile%
Echo Step out [InstallClientToolsC6.bat]   >> %logFile%
ECHO ___________________________________   >> %logFile%

exit 0
