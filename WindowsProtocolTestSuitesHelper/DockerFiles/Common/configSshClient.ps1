# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param (
    [string]$PrivateKeyPath
)

if (-not (Test-Path $PrivateKeyPath)) {
    Write-Host "Private key `"$PrivateKeyPath`" cannot be found, skip the SSH client configuration..."
    exit 0
}

Copy-Item $PrivateKeyPath /root/.ssh/ -Force