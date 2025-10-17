call d:\script\DISABLE_StrongName.cmd
call d:\script\ENABLE_PING.cmd
call d:\script\ENABLE_RDP.cmd
call d:\script\ENABLE_WinRM.cmd

net user administrator /active:yes
wmic useraccount where "name='Administrator'" set PasswordExpires=FALSE

reg add HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run /v BGINFO /t REG_SZ /d "%systemdrive%\temp\bginfo /i%systemdrive%\temp\mysettings.bgi /timer:0 /AcceptEula" /f
c:\windows\system32\WindowsPowerShell\v1.0\powershell.exe -noexit -command "D:\controller.ps1"