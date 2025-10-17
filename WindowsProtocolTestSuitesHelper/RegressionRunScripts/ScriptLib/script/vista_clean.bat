cd \
del %temp% /s /q /f /a
del *.tmp /s /q /f /a
del thumbs.db /s /q /f /a
del "%USERPROFILE%\AppData\Local\Microsoft\Terminal Server Client" /s /q
del "%USERPROFILE%\AppData\Local\Microsoft\Windows\WER" /s /q
del "%USERPROFILE%\AppData\Local\Microsoft\Windows\Explorer" /s /q
del "%USERPROFILE%\AppData\Local\Microsoft\Windows\Temporary Internet Files" /s /q
del "%USERPROFILE%\AppData\Local\Microsoft\Communicator" /s /q 
del "\ProgramData\Microsoft\Windows\WER" /s /q
del "%windir%\SoftwareDistribution\Download" /s /q