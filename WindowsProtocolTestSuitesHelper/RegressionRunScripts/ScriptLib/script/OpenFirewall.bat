::
:: Start Firewall service in case UIP has stopped it
::
sc.exe start SharedAccess

::Start Firewall to add exceptions to it.
net start "Windows Firewall"

:: Give the service time to start and check for it
::
NET START | FIND /I "FireWall"
IF ERRORLEVEL 1 c:\machine\setup\sleep 5

::Remove it by micmao to avoid firewall block
::NET START | FIND /I "Windows FireWall"
::IF ERRORLEVEL 1 GOTO end

ver | find "6.1" > nul
if not errorlevel 1 goto :WIN7

:preWIN7
netsh firewall add portopening protocol = TCP port = 3389 name = RmtDskT mode = ENABLE scope = ALL profile = ALL
netsh firewall add portopening protocol = TCP port = 1778 name = WTTSvc mode = ENABLE scope = ALL profile = ALL

goto :common

:WIN7

netsh advfirewall set domainprofile firewallpolicy allowinbound,allowoutbound

netsh advfirewall firewall set rule group="Remote Desktop" new enable=yes

netsh advfirewall firewall add rule name="RmtDskT" protocol=TCP dir=in localport=3389 action=allow
netsh advfirewall firewall add rule name="WTTSvc" protocol=TCP dir=in localport=1778 action=allow

goto :common


:common


:end
