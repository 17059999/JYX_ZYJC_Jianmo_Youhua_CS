#!/bin/bash
#
# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

if [ -e "/data/rdpserver" ]; then
	# copy config file if it exists
	if [ -f "/data/rdpserver/RDP_ServerTestSuite.deployment.ptfconfig" ]; then
		cp /data/rdpserver/RDP_ServerTestSuite.deployment.ptfconfig /opt/rdpserver/Bin
	else
		echo "Cannot find RDP_ServerTestSuite.deployment.ptfconfig under the path /data/rdpserver"
	fi

    # start the script to execute test cases
	chmod 777 /opt/rdpserver/Batch/*.sh &&
	/opt/rdpserver/Batch/RunTestCasesByFilter.sh \"$Filter\" \"$DryRun\"
	if [ ! -z $DryRun ]; then
	echo "Listed all the test cases which match the filter condition"
	else
	cp -r /opt/rdpserver/TestResults/ /data/rdpserver/
	fi
else
	echo "The path /data/rdpserver does not exist, please use -v to mount to it"
fi