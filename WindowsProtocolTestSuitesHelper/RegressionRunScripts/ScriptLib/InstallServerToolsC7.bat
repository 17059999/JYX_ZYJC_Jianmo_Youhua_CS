set toolsPath=%1
set scriptsPath=%2
set CPUArchitecture=%3
set logFile=%4
set installBitmap=%5

ECHO ____________________________________        >> %logFile%
Echo Step into [InstallServerToolsC7.bat]        >> %logFile%

Echo Installation flag: %installBitmap%          >> %logFile%
if "#%installBitmap%" EQU "#" (
ECHO Set default installation flag: set installBitmap=1101   >> %logFile%
set installBitmap=1101
)
ECHO .........................               >> %logFile%

if "%installBitmap:~0,1%" EQU "1" (
if not exist %SystemDrive%\Windows\Microsoft.NET\Framework\v2.0.50727\AppLaunch.exe (
Echo Start to install .Net framework:        >> %logFile%
Echo Source file: %toolsPath%\DotNetFramework\2.1.21022\%CPUArchitecture%\NetFx20SP1.exe >> %logFile%
%toolsPath%\DotNetFramework\2.1.21022\%CPUArchitecture%\NetFx20SP1.exe /q
) else (
Echo DotNetFx existing: %SystemDrive%\Windows\Microsoft.NET\Framework\v2.0.50727\AppLaunch.exe. Installation skipped.     >> %logFile%
)
) else (
Echo Installing .Net framework skipped.      >> %logFile%
)

if "%installBitmap:~1,1%" EQU "1" (
Echo Start to install VCRedist: >> %logFile%
Echo Source file: %toolsPath%\vcredist\9.0.21022\%CPUArchitecture%\vcredist.exe >> %logFile%
%toolsPath%\vcredist\9.0.21022\%CPUArchitecture%\vcredist.exe /q
) else (
Echo Installing vcredist skipped.            >> %logFile%
)

if "%installBitmap:~2,1%" EQU "1" (
Echo Start to install NetMon: >> %logFile%
Echo Source file: %toolsPath%\NetworkMonitor\3.2.1303\%CPUArchitecture%\Netmonpt3.msi >> %logFile%
msiexec /quiet /i %toolsPath%\NetworkMonitor\3.2.1303\%CPUArchitecture%\Netmonpt3.msi
Echo Source file: %toolsPath%\NetworkMonitor\3.2.1303\%CPUArchitecture%\Microsoft_PT3_Parsers.msi >> %logFile%
msiexec /quiet /i %toolsPath%\NetworkMonitor\3.2.1303\%CPUArchitecture%\Microsoft_PT3_Parsers.msi
) else (
Echo Installing NetMon skipped.              >> %logFile%
)

if "%installBitmap:~3,1%" EQU "1" (
Echo Start to install PowerShell: >> %logFile%
Echo Source file:  %toolsPath%\PowerShell\2.0_CTP2\%CPUArchitecture%\PowerShell_Setup.msi >> %logFile%
msiexec /quiet /i "%toolsPath%\PowerShell\2.0_CTP2\%CPUArchitecture%\PowerShell_Setup.msi"
) else (
Echo Installing PowerShell skipped.          >> %logFile%
)

ECHO .........................               >> %logFile%

Echo Start to change PowerShell config to RemoteSigned: >> %logFile%
Echo Command: reg add HKLM\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell /v ExecutionPolicy /d "RemoteSigned" >> %logFile%
reg add HKLM\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell /v ExecutionPolicy /d "RemoteSigned"

Echo Finish to install C7 tools.             >> %logFile%
Echo Step out [InstallServerToolsC7.bat]     >> %logFile%
ECHO ___________________________________     >> %logFile%

exit 0
