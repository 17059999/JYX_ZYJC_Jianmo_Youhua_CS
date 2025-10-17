echo START LAB DAILY RUN
rem the file record the local buildnumber
set DoneFile=E:\LabDailyRun\BVT-Windows\DoneFile.txt
echo DoneFile is %DoneFile%

rem the toolpath
set ToolPath=E:\LabDailyRun\BVT-Windows
echo ToolPath is %ToolPath%

rem the job datastore
set DataStore=ProtocolTest
echo DataStore is %DataStore%

set DailyBuildSource=\\PET-STORAGE-01\DailyBuildShare\drop\pt3_ts_dev\1.0\
set DailyBuildSourceFlag=\\PET-STORAGE-01\DailyBuildShare\source\pt3_ts_dev\1.0
set DailyBuildDestin=\\pet-storage-01\PETWindowsTSDTest\DailyBuild\
rem the build file path in build server 
echo DailyBuildSource is %DailyBuildSource%
echo DailyBuildSourceFlag is %DailyBuildSourceFlag%
echo DailyBuildDestin is %DailyBuildDestin%

rem the local enlisment path
set inetroot=E:\sourcedepot\ts_dev
echo inetroot is %inetroot%

rem the local config file for daily run
set configFile=%ToolPath%\LabDailyRunSettings.xml
echo configFile is %configFile%

Powershell %ToolPath%\LabDailyRunMonitor.ps1 %DoneFile% %ToolPath% %DataStore% %DailyBuildSource% %DailyBuildSourceFlag% %DailyBuildDestin% %inetroot% %configFile% 