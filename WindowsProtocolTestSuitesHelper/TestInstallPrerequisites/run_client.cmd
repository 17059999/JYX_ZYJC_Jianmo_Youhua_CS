call d:\script\DISABLE_StrongName.cmd
call d:\script\ENABLE_PING.cmd
call d:\script\ENABLE_RDP.cmd
call d:\script\ENABLE_WinRM.cmd

c:\windows\system32\WindowsPowerShell\v1.0\powershell.exe -noexit -command "D:\run.ps1"