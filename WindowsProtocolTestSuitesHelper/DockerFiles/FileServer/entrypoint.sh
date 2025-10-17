#!/bin/bash
#
# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

echo "Usage: $Usage"

pwsh ./configSshClient.ps1 "/data/fileserver/id_rsa"

if [ "$Usage" = "RunTestCases" ]; then
	pwsh ./runTestCases.ps1
elif [ "$Usage" = "RunPTMCli" ]; then
	pwsh ./runPTMCli.ps1
elif [ "$Usage" = "RunPTMService" ]; then
	pwsh ./runPTMService.ps1
else
	echo "The usage \"$Usage\" for the image is invalid."
fi
