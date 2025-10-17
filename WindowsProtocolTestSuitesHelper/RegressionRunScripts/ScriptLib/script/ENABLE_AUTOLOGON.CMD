
   REM == Usage:
   REM ==   EnableAutologon.cmd <Username> <UserDomain> <UserPwd> <AltDomain>
   REM ==
   REM == External Dependencies:
   REM ==   REG=%REG%
   REM ==   ISVISTAORLATER=%ISVISTAORLATER%
   REM ==   SCRIPTBASE=%SCRIPTBASE%

set REG=REG.exe

   REM == Read your arguments here
   set MY_Username=%~1
   set MY_UserDomain=%~2
   echo set MY_UserPwd=...
   @set MY_UserPwd=%~3
   set MY_AltDomain=%~4
   

   REM == Execute your stuff here
   %REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v DefaultUserName          /d "%MY_Username%" /f
   %REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v DefaultDomainName        /d "%MY_UserDomain%" /f
   echo %REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v DefaultPassword          /d "MY_UserPwd" /f
   @%REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v DefaultPassword          /d "%MY_UserPwd%" /f
   %REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v AltDefaultUserName       /d "%MY_Username%" /f
   %REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v AltDefaultDomainName     /d "%MY_AltDomain%" /f
   %REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v AutoAdminLogon /t REG_SZ /d "1" /f
   %REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v ForceAutoLogon /t REG_SZ /d "0" /f

   REM == Avoid annoying dialog to remind to change password 14 days in advance...
   %REG% add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v passwordexpirywarning  /t REG_DWORD /d 1 /f

   if "%ISVISTAORLATER%"=="1" (
      REM == The following values are set by setenv.cmd

      "%windir%\system32\autologon.exe" /migrate

   ) else (
      %SCRIPTBASE%%PROCESSOR_ARCHITECTURE%\autologon.exe /migrate
   )



