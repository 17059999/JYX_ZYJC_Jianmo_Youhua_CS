reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v AllowStrongNameBypass /t REG_DWORD /d 0x0 /f
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\.NETFramework" /v AllowStrongNameBypass /t REG_DWORD /d 0x0 /f

