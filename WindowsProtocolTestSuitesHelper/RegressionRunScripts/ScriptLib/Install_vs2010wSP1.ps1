##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################

    # Build the string to run the install
    Write-Host -ForegroundColor Green "========================================="
    Write-Host -ForegroundColor Green "Installing Visual Studio 2010 SP1"
    cmd /c "D:\vs2010.ultimate.vl\dvd\setup\setup.exe" /full /norestart /NoPendingRebootChk /q
    cmd /c "D:\vs2010sp1.dvd1\setup.exe" /norestart /passive
    Write-Host -ForegroundColor Green "========================================="
