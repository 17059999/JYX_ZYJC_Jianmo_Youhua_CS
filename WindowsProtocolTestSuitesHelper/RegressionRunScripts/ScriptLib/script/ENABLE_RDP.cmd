reg add "HKLM\SYSTEM\CurrentControlSet\Control\Terminal Server" /v fDenyTSConnections /t REG_DWORD /d 0x0 /f

netsh firewall set service RemoteDesktop enable 
netsh advfirewall firewall set rule group="remote desktop" new enable=Yes 


