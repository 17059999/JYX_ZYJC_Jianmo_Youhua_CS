set toolsPath=%1
set scriptsPath=%2
set CPUArchitecture=%3
set logFile=%4
set cluster=%5
set testResultsPath=%6
set platForm="Client"

ECHO ____________________________________       >> %logFile%
Echo Step into [InstallClientTools.bat]       >> %logFile%

if not exist %SystemDrive%\Windows\Microsoft.NET\Framework\v2.0.50727\AppLaunch.exe (
Echo Start to install .Net framework:        >> %logFile%
Echo Source file: %toolsPath%\DotNetFramework\2.1.21022\%CPUArchitecture%\NetFx20SP1.exe >> %logFile%
%toolsPath%\DotNetFramework\2.1.21022\%CPUArchitecture%\NetFx20SP1.exe /q
) else (
Echo DotNetFx existing: %SystemDrive%\Windows\Microsoft.NET\Framework\v2.0.50727\AppLaunch.exe. Installation skipped.     >> %logFile%
)

Echo Start to install PowerShell: >> %logFile%
Echo Source file:  %toolsPath%\PowerShell\2.0_CTP2\%CPUArchitecture%\PowerShell_Setup.msi >> %logFile%
msiexec /quiet /i "%toolsPath%\PowerShell\2.0_CTP2\%CPUArchitecture%\PowerShell_Setup.msi"

Echo Start to change PowerShell config to RemoteSigned: >> %logFile%
Echo Command: reg add HKLM\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell /v ExecutionPolicy /d "RemoteSigned" /f >> %logFile%
reg add HKLM\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell /v ExecutionPolicy /d "RemoteSigned" /f

Echo Add powershell path into system variable PATH.   >> %logFile%
set powershellPath=%SystemRoot%\system32\WindowsPowerShell\v1.0\
set path=%path%;%powershellPath%
set path

Echo Start to install tools >> %logFile%
Echo %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell %scriptsPath%\Install-Tools.ps1 %toolsPath% %testResultsPath% %CPUArchitecture% %platForm% %cluster% >> %logFile%
powershell %scriptsPath%\Install-Tools.ps1 %toolsPath% %testResultsPath% %CPUArchitecture% %platForm% %cluster% 

Echo Finish to install %cluster% tools.           >> %logFile%
Echo Step out [InstallClientTools.bat]   >> %logFile%
ECHO ___________________________________   >> %logFile%

exit 0
