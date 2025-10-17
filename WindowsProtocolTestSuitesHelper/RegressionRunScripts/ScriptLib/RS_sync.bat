set INETROOT=%1
set ProtocolName=%2
echo INETROOT = %INETROOT%
cd /d %INETROOT%
echo calling %INETROOT%\tools\path1st\myenv.cmd
call %INETROOT%\tools\path1st\myenv.cmd
echo Syncing %ProtocolName% RS File...
sd.exe sync %INETROOT%\TestSuites\%ProtocolName%\Docs\%ProtocolName%_RequirementSpec.xlsm