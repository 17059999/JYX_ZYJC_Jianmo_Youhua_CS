@echo Windows Registry Editor Version 5.00>%3
@echo [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon]>>%3
@echo "DefaultUserName"="%1">>%3
@echo "DefaultDomainName"="">>%3
@echo "AutoAdminLogon"="1">>%3
@echo "AltDefaultUserName"="%1">>%3
@echo "AltDefaultDomainName"="">>%3
@echo "DefaultPassword"="%2">>%3
