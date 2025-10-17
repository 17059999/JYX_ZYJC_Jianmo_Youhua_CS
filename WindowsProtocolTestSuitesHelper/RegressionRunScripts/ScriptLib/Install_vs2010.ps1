##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################
if(Test-Path "$env:programfiles\Microsoft Visual Studio 10.0")
{
    Write-Host -ForegroundColor Green "VS2010 already installed"
}
else
{
    # Build the string to run the install
    Write-Host -ForegroundColor Green "========================================="
    Write-Host -ForegroundColor Green "Installing Visual Studio 2010"
    cmd /c "D:\setup\setup.exe" /full /norestart /passive /NoPendingRebootChk
    Write-Host -ForegroundColor Green "========================================="
}