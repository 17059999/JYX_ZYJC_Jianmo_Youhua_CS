@if "%1"=="" goto usage
@echo off

@set ProtocolName=%1
@set LogShare=%2
@set MainEnlistment=%3
@set DevBuild=%4
@set DVDImage=%5
@if "%2"=="" (@set LogShare="\\protoreport\SDTestSuites")
@if "%3"=="" (@set MainEnlistment="C:\ts_main_sixiao\TestSuites")
@if "%4"=="" (@set DevBuild="\\pet-labserver\TST\DailyBuild")
@if "%5"=="" (@set DVDImage="\\nmtest\DVDImages\C8 DVD Contents")


@echo You provide the following augments: 
@echo %ProtocolName% 
@echo %LogShare% 
@echo %MainEnlistment% 
@echo %DevBuild%
@echo %DVDImage%
@echo Please confirm. Enter for Yes; Close Window for No. 
@rem @pause

@pushd "C:\Program Files\Windows Resource Kits\Tools"


@echo *************************************************
@echo 1) Copying Logs:
@robocopy /MIR %LogShare%\%ProtocolName%\Logs                   %DVDImage%\%ProtocolName%\Logs

@echo 2) Copying Docs:
@robocopy /MIR %MainEnlistment%\%ProtocolName%\Docs             %DVDImage%\%ProtocolName%\Docs

@echo 3) Copying TestCode:
@robocopy /MIR %MainEnlistment%\%ProtocolName%\TestCode         %DVDImage%\%ProtocolName%\TestCode

@echo 4) Copying Bin:
@robocopy /MIR %DevBuild%\%ProtocolName%\Bin                    %DVDImage%\%ProtocolName%\Bin

@echo Finished copying test contect for %ProtocolName%.


@echo *************************************************
@echo Deleting Review Folder and CoreXT files:
@echo Map local drive Z: to DVDImage's folder
@net use z: %DVDImage%
@pushd z:\%ProtocolName%\TestCode

@echo 1) Delete Review Folder
@rd %DVDImage%\%ProtocolName%\Docs\Review /s /q

@echo 2) Delete sources file
@del sources /s /f

@echo 3) Delete dirs file
@del dirs /s /f

@echo 4) Delete placefile file
@del placefile /s /f

@echo 5) Delete asmmeta file
@del *.asmmeta /s /f

@popd
@net use z: /del /y
@echo Finished removing unreleased files for %ProtocolName%.
@goto end




:usage
@echo Example: CopyDVDImages.bat MS-XXXX \\protoreport\SDTestSuites D:\PT3\ts_main\TestSuites \\pet-labserver\TST\DailyBuild "\\nmtest\DVDImages\C8 DVD Contents"
@goto end

:end
@popd
@pause
