##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################
if(Test-Path "$env:programfiles\Microsoft Visual Studio 11.0")
{
    Write-Host -ForegroundColor Green "VS2012 already installed"
}
else
{
    # Build the string to run the install
    Write-Host -ForegroundColor Green "========================================="
    Write-Host -ForegroundColor Green "Installing Visual Studio 2012"
    cmd /c D:\vs_ultimate.exe /norestart /passive
    Write-Host -ForegroundColor Green "Installing Visual Studio 2012 Update 4"
    cmd /c D:\Updates\VS2012.4.exe /norestart /passive
    Write-Host -ForegroundColor Green "========================================="
}