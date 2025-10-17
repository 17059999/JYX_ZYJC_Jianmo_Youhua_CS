#!/bin/bash
#
# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#
# function update ptfconfig files
update_ptfconfig() {
	if [ ! -z $1 ]; then
		echo "$1 will be changed into: "${!1}""
		xmlstarlet ed --inplace -u "/_:TestSite/_:Properties/_:Property[@name='$1']/@value" -v "${!1}" /opt/ms-wsp/Bin/MS-WSP_ServerTestSuite.deployment.ptfconfig
	else
		echo "$1 is using default value: "${!1}""
	fi
}

# print the parameters passed in
echo "Filter: $Filter"
echo "DryRun: $DryRun"
echo "ServerComputerName: $ServerComputerName"
echo "UserName: $UserName"
echo "Password: $Password"
echo "DomainName: $DomainName"
echo "ShareName: $ShareName"
echo "QueryPath: $QueryPath"
echo "QueryText: $QueryText"

if [ -e "/data/ms-wsp" ]; then
	# copy config file if it exists
	if [ -f "/data/ms-wsp/MS-WSP_ServerTestSuite.deployment.ptfconfig" ]; then
		cp /data/ms-wsp/MS-WSP_ServerTestSuite.deployment.ptfconfig /opt/ms-wsp/Bin
	fi

	# update ptfconfig if necessary
	update_ptfconfig ServerComputerName
	update_ptfconfig UserName
	update_ptfconfig Password
	update_ptfconfig DomainName
	update_ptfconfig ShareName
	update_ptfconfig QueryPath
	update_ptfconfig QueryText

	# start the script to execute test cases
	chmod 777 /opt/ms-wsp/Batch/*.sh &&
		/opt/ms-wsp/Batch/RunTestCasesByFilter.sh \"$Filter\" \"$DryRun\"
	if [ ! -z $DryRun ]; then
		echo "Listed all the test cases which match the filter condition"
	else
		cp -r /opt/ms-wsp/TestResults/ /data/ms-wsp/
	fi
else
	echo "The path /data/ms-wsp does not exist, please use -v to mount to it"
fi
