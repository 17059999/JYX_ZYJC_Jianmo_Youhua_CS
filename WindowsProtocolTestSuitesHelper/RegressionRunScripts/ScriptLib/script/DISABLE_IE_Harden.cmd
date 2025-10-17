REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}" /v "IsInstalled" /t REG_DWORD /d 0 /f
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}" /v "IsInstalled" /t REG_DWORD /d 0 /f 

::Removing IE Enhanced Security from System 
Rundll32 iesetup.dll,IEHardenUser
Rundll32 iesetup.dll,IEHardenAdmin
Rundll32 iesetup.dll,IEHardenMachineNow
Rundll32 iesetup.dll,IEHardenMachineNow u

:: Disabling IEHarden for user
REG ADD "HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap" /V "IEHarden" /t REG_DWORD /d 0 /f

::Removed the Values from the IEHarden installed components key
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}" /f /va
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}" /f /va 

::END
