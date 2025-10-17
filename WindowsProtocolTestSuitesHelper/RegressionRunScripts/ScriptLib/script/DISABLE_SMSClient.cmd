rem block Windows Update service
net stop wuauserv
sc config wuauserv start= disabled

%windir%\ccmsetup\ccmsetup /uninstall
\\ziz-dfsr02\stbclab\qliu\TOOLSW\CcmTools\ccmclean.exe

net stop bits
rd /s /q %windir%\ccmsetup
rd /s /q %windir%\ccm
rd /s /q %windir%\system32\ccm
rd /s /q %windir%\system32\ccmsetup
rd /s /q %windir%\system32\VPCACHE

md %windir%\ccm
md %windir%\ccmsetup
md %windir%\system32\ccm
md %windir%\system32\ccmsetup

icacls %windir%\ccm /deny everyone:f
icacls %windir%\ccmsetup /deny everyone:f
icacls %windir%\system32\ccm /deny everyone:f
icacls %windir%\system32\ccmsetup /deny everyone:f

if /I "%PROCESSOR_ARCHITECTURE%" == "amd64" (
    rd /s /q %windir%\sysWOW64\ccm
    rd /s /q %windir%\sysWOW64\ccmsetup
    md %windir%\sysWOW64\ccm
    md %windir%\sysWOW64\ccmsetup
    icacls %windir%\sysWOW64\ccm /deny everyone:f
    icacls %windir%\sysWOW64\ccmsetup /deny everyone:f
)

net localgroup administrators "fareast\domain admins" /delete



