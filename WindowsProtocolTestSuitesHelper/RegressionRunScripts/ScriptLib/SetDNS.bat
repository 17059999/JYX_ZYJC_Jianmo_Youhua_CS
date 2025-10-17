set OSVersion=%1
set IPVersion=%2
set dns=%3
set logPath=%4
set flag=%5

@rem ---- Deal parameters ----
if "%logPath%"==""  set logPath=.
if not exist %logPath% md %logPath%

set logFile=%logPath%\SetDNS_%COMPUTERNAME%.log
if not exist %logFile% echo _______________________________ >  %logFile%
@echo Step into [SetDNS.bat]                                >> %logFile%
@echo ---- Parameter List ---- >> %logFile%
@echo OSVersion = %OSVersion%  >> %logFile%
@echo IPVersion = %IPVersion%  >> %logFile%
@echo dns       = %dns%        >> %logFile%
@echo logPath   = %logPath%    >> %logFile%
@echo flag      = %flag%       >> %logFile%

@rem ---- Delete tmp files ----
@if exist setdns_1.tmp del setdns_1.tmp
@if exist setdns_2.tmp del setdns_2.tmp


@echo ---- Get Network Interface Names ---- >> %logFile%
netsh interface show interface > setdns_1.tmp
find /I "Local Area Connection" < setdns_1.tmp > setdns_2.tmp
find /I "Local Area Connection" < setdns_1.tmp >> %logFile%


@rem ---- Set IPv4 DNS ---- 
if "%IPVersion%"=="IPv4" (   
    @echo ---- Set IPv4 DNS ---- >> %logFile%
    if "%OSVersion%"=="WXP" (
        if "%flag%"=="add" (
            for /F "tokens=1,2* delims= " %%i in (setdns_2.tmp) do netsh interface ip add dnsserver "%%k" %dns% >> %logFile%
        ) else (
            for /F "tokens=1,2* delims= " %%i in (setdns_2.tmp) do netsh interface ip set dns "%%k" static %dns% primary >> %logFile%
        )
    ) else (        
        if "%flag%"=="add" (
            for /F "tokens=2,3* delims= " %%i in (setdns_2.tmp) do netsh interface ip add dnsserver "%%k" %dns% >> %logFile%
        ) else (
            for /F "tokens=2,3* delims= " %%i in (setdns_2.tmp) do netsh interface ip set dns "%%k" static %dns% primary >> %logFile%
        )       
    )
)


@rem ---- Set IPv6 DNS ----
if "%IPVersion%"=="IPv6" (
    if "%OSVersion%"=="W2K3"  (
        @echo ---- Set W2K3 IPv6 DNS ---- >> %logFile%
        if "%flag%"=="add" (
            for /F "tokens=2,3* delims= " %%i in (setdns_2.tmp) do netsh interface ipv6 add dnsserver "%%k" %dns% >> %logFile%
        ) else (
            for /F "tokens=2,3* delims= " %%i in (setdns_2.tmp) do netsh interface ipv6 delete dns "%%k" all >> %logFile%
            for /F "tokens=2,3* delims= " %%i in (setdns_2.tmp) do netsh interface ipv6 add dns "%%k" %dns%  >> %logFile%
        )       
    ) else (
        if "%OSVersion%"=="WXP" (
            @echo ---- Set WXP IPv6 DNS ---- >> %logFile%
            if "%flag%"=="add" (
                for /F "tokens=1,2* delims= " %%i in (setdns_2.tmp) do netsh interface ipv6 add dnsserver "%%k" %dns% >> %logFile%
            ) else (
                for /F "tokens=1,2* delims= " %%i in (setdns_2.tmp) do netsh interface ipv6 delete dns "%%k" all >> %logFile%
                for /F "tokens=1,2* delims= " %%i in (setdns_2.tmp) do netsh interface ipv6 add dns "%%k" %dns%  >> %logFile%
            )             
        ) else (
            @echo ---- Set W2K8/Vista or later OS IPv6 DNS ---- >> %logFile%            
            if "%flag%"=="add" (
                for /F "tokens=2,3* delims= " %%i in (setdns_2.tmp) do netsh interface ipv6 add dnsserver "%%k" %dns% >> %logFile%
            ) else (
                for /F "tokens=2,3* delims= " %%i in (setdns_2.tmp) do netsh interface ipv6 set dns "%%k" static %dns% primary >> %logFile%
            )             
        )
    )
)


@rem ---- delete tmp files -----
@if exist setdns_1.tmp del setdns_1.tmp
@if exist setdns_2.tmp del setdns_2.tmp

@echo ---- Write signal file ---- >> %logFile%
@set sigFileName=dns.%OSVersion%.%IPVersion%.config.finished.signal
@echo Signal File Name: %sigFileName% >> %logFile%
@echo CONFIG FINISHED > %logPath%\%sigFileName%

@echo Step out [SetDNS.bat]           >> %logFile%
@echo _______________________________ >> %logFile%

exit 0
