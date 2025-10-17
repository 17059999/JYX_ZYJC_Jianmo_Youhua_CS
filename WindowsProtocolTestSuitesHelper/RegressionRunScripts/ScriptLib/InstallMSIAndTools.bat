set protocolName=%1
set testDirInVM=%2
set targetEndpoint=%3
set IPVersion=%4
set workgroupDomain=%5
set userNameInVM=%6
set userPwdInVM=%7
set domainInVM=%8
set endPoint=%9

set scriptsPath=%testDirInVM%\Scripts

set powershellPath=%SystemRoot%\system32\WindowsPowerShell\v1.0\
set path=%path%;%powershellPath%
set path                                           

powershell set-itemproperty -path "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell" -name ExecutionPolicy -value Unrestricted
powershell %scriptsPath%\Install-MSIAndTools.ps1 %protocolName% %testDirInVM% %targetEndpoint% %IPVersion% %workgroupDomain% %userNameInVM% %userPwdInVM% %domainInVM% %endPoint%

exit 0