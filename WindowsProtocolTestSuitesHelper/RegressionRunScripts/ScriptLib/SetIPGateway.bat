set OSVersion=%1
set IPVersion=%2
set ip=%3
set mask=%4
set gateway=%5
set logPath=%6
set ip6=%7
set gatewayv6=%8

@rem when IPVersion is IPv4IPv6, ip/mask/gateway is for ipv4; ip6/gatewayv6 is for ipv6
@rem ---- Deal parameters ----
if "%logPath%"==""  set logPath=.
if not exist %logPath% md %logPath%

set logFile=%logPath%\SetIPGateway_%COMPUTERNAME%.log
if not exist %logFile% echo _______________________________ >  %logFile%
echo Step into [SetIPGateway.bat]                           >> %logFile%
@echo ---- Parameter List ---- >> %logFile%
@echo OSVersion = %OSVersion%  >> %logFile%
@echo IPVersion = %IPVersion%  >> %logFile%
@echo ip        = %ip%         >> %logFile%
@echo mask      = %mask%       >> %logFile%
@echo gateway   = %gateway%    >> %logFile%
@echo logPath   = %logPath%    >> %logFile%

@rem ---- Delete tmp files ----
@if exist setipgw_1.tmp del setipgw_1.tmp
@if exist setipgw_2.tmp del setipgw_2.tmp


@echo ---- Get Network Interface Names ---- >> %logFile%
netsh interface show interface > setipgw_1.tmp
find /I "Local Area Connection" < setipgw_1.tmp > setipgw_2.tmp
find /I "Local Area Connection" < setipgw_1.tmp >> %logFile%

ipconfig > setipgw_3.tmp
find /I "IPv6 Address" < setipgw_3.tmp > setipgw_4.tmp
find /I "Default Gateway" < setipgw_3.tmp > setipgw_5.tmp

@rem ---- Set IPv4 IP Address and Gateway ---- 
if /I "%IPVersion%"=="IPv4" (   
    @echo ---- Set IPv4 IP Address and Gateway ---- >> %logFile%
    if "%OSVersion%"=="WXP" (
		for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_4.tmp) do ( netsh interface ipv6 delete address "%%k" %%y >> %logFile%
			)
			for /F "tokens=1,* delims=:" %%x in (setipgw_5.tmp) do ( netsh interface ipv6 delete route ::/0 "%%k" %%y >> %logFile%
			)
		)
        for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do netsh interface ip set address "%%k" static %ip% %mask% %gateway% 1 >> %logFile%
    ) else (
		for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_4.tmp) do ( netsh interface ipv6 delete address "%%k" %%y >> %logFile%
			)
			for /F "tokens=1,* delims=:" %%x in (setipgw_5.tmp) do ( netsh interface ipv6 delete route ::/0 "%%k" %%y >> %logFile%
			)
		)
        for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do netsh interface ip set address "%%k" static %ip% %mask% %gateway% 1 >> %logFile%
    )
)


@rem ---- Set IPv6 Address and Gateway ----
if /I "%IPVersion%"=="IPv6" (
    @echo ---- Set IPv6 IP Address ---- >> %logFile%
    if "%OSVersion%"=="WXP" (    
		for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do netsh interface ip set address "%%k" dhcp    >> %logFile%
        for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_4.tmp) do ( netsh interface ipv6 delete address "%%k" %%y >> %logFile%
			)
		)
		for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do netsh interface ipv6 set address "%%k" %ip%  >> %logFile%
    ) else (
		for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do netsh interface ip set address "%%k" dhcp   >> %logFile%
		for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_4.tmp) do ( netsh interface ipv6 delete address "%%k" %%y >> %logFile%
			)
		)
        for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do netsh interface ipv6 set address "%%k" %ip%  >> %logFile%
    )
    @echo ---- Set IPv6 Gateway ---- >> %logFile%   
    if "%OSVersion%"=="WXP" (
		for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_5.tmp) do ( netsh interface ipv6 delete route ::/0 "%%k" %%y >> %logFile%
			)
		)
        for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do netsh interface ipv6 add route ::/0 "%%k" %gateway%  >> %logFile%
    ) else (
		for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_5.tmp) do ( netsh interface ipv6 delete route ::/0 "%%k" %%y >> %logFile%
			)
		)
        for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do netsh interface ipv6 add route ::/0 "%%k" %gateway%  >> %logFile%
    )
)


@rem ---- Set IPv4IPv6 Address and Gateway ----
if /I "%IPVersion%"=="IPv4IPv6" (   
    @echo ---- Set ip4 IP Address and Gateway ---- >> %logFile%
    if "%OSVersion%"=="WXP" (
        for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do netsh interface ip set address "%%k" static %ip% %mask% %gateway% 1 >> %logFile%
		for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_4.tmp) do ( netsh interface ipv6 delete address "%%k" %%y    >> %logFile%
			)
		)
		for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do netsh interface ipv6 set address "%%k" %ip6%         >> %logFile%
		
		for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_5.tmp) do ( netsh interface ipv6 delete route ::/0 "%%k" %%y >> %logFile%
			)
		)
        for /F "tokens=1,2* delims= " %%i in (setipgw_2.tmp) do netsh interface ipv6 add route ::/0 "%%k" %gatewayv6% >> %logFile%
    ) else (
		for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do netsh interface ip set address "%%k" static %ip% %mask% %gateway% 1 >> %logFile%
		for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_4.tmp) do ( netsh interface ipv6 delete address "%%k" %%y    >> %logFile%
			)
		)
        for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do netsh interface ipv6 set address "%%k" %ip6%         >> %logFile%
        
		for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do (
			for /F "tokens=1,* delims=:" %%x in (setipgw_5.tmp) do ( netsh interface ipv6 delete route ::/0 "%%k" %%y >> %logFile%
			)
		)
        for /F "tokens=2,3* delims= " %%i in (setipgw_2.tmp) do netsh interface ipv6 add route ::/0 "%%k" %gatewayv6% >> %logFile%
    )
)

@rem ---- delete tmp files -----
@rem if exist setipgw_1.tmp del setipgw_1.tmp
@rem if exist setipgw_2.tmp del setipgw_2.tmp

@echo ---- Write signal file ---- >> %logFile%
@set sigFileName=gateway.%OSVersion%.%IPVersion%.config.finished.signal
@echo Signal File Name: %sigFileName% >> %logFile%
@echo CONFIG FINISHED > %logPath%\%sigFileName%

echo Step out [SetIPGateway.bat]     >> %logFile%
echo _______________________________ >> %logFile%

rem exit 0
