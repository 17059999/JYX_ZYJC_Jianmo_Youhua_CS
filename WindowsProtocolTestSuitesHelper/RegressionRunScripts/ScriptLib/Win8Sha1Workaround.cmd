@echo off

if exist %SystemRoot%\SoftwareDistribution\autest.cab (

attrib -r %SystemRoot%\SoftwareDistribution\autest.cab
)

set STDLIB_GDRToolShare=%SystemDrive%\temp
cscript %SystemDrive%\temp\Win8Scripts\Win8Sha1Workaround.js  %STDLIB_GDRToolShare%
