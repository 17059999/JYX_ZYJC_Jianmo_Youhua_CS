echo START TEST SUITE
set testDirInVM=%1
set CPUArchitecture=%2
set testResultDir=%3
set protocolName=%4
set captureName=%5
set RSinScope=%6
set RSoutOfScope=%7

set binPath=%testDirInVM%\Bin
set toolsPath=%testDirInVM%\Tools
set scriptsPath=%testDirInVM%\Scripts
set testResultsPath=%testDirInVM%\TestResults

if not exist %testResultsPath% md %testResultsPath%
set logFile=%testResultsPath%\RunTestSuite.log
ECHO _________________________________           > %logFile%
Echo Step into [StartRunTestSuite.bat]           >> %logFile%

Echo Ready to run TestSuite...                   >> %logFile%
Echo [RestartAndRun] Run: runTestSuiteEx.bat: %scriptsPath%\RunTestSuiteEx.bat         >> %logFile%
%scriptsPath%\RestartAndRun.bat "%scriptsPath%\RunTestSuiteEx.bat %1 %2 %logFile% %4 %5 %binPath% %toolsPath% %scriptsPath% %testResultsPath% %RSinScope% %RSoutOfScope%"

rem No log needed here, because of Restart and Run

exit 0

