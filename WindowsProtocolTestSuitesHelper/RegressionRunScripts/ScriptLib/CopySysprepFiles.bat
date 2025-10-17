set SourcePathInVM=%1
set DestinPathInVM=%2
set SysprepPathInVM=%3
set sysDriveInVM=%4



xcopy %SourcePathInVM% %DestinPathInVM% /e /c /i /r /y

#xcopy %SysprepPathInVM%\Sysprep %sysDriveInVM%\Sysprep /e /c /i /r /y
#xcopy %SysprepPathInVM%\SysprepWipe.bat %sysDriveInVM%\ /e /c /i /r /y
#xcopy %SysprepPathInVM%\SysprepCleanUp.bat %sysDriveInVM%\ /e /c /i /r /y
#xcopy %SysprepPathInVM%\netset03.exe %sysDriveInVM%\ /e /c /i /r /y
#xcopy %SysprepPathInVM%\IPAnswerfile.inf %sysDriveInVM%\ /e /c /i /r /y

#rmdir /s /q %DestinPathInVM%

exit 0
