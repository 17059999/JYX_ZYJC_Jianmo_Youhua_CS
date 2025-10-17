#!/bin/sh
# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
#  File:     CopyWindowsFileToLinux.sh
#  Purpose:  To copy the target file from Windows shared folder to current Linux box.
##############################################################################

# Check the arguments number
if [ $# -ne 6 ]; then
    {
        cat <<HELP
CopyWindowsFileToLinux -- To copy the target file from Windows shared folder to current Linux box.

USAGE: CopyWindowsFileToLinux winIpAddress sharedFolderName winFileName fullDomainName userName password
Example: CopyWindowsFileToLinux 192.168.1.3 C$ DomainBased.txt fareast.corp.microsoft.com userName password
HELP
        echo false
        exit 1
    }
fi

# Parameters
winIpAddress=$1
sharedFolderName=$2
winFileName=$3
fullDomainName=$4
userName=$5
password=$6

# Copy file from Windows Shared folder
[ ! -d /winShared ] && mkdir /winShared
mount -t cifs "//$winIpAddress/$sharedFolderName" /winShared -o username="$userName",password="$password",domain="$fullDomainName"
rm -f /"$winFileName"
cp "/winShared/$winFileName" "/$winFileName"
umount /winShared
if [ -f "/$winFileName" ]; then
    {
        echo true
        exit 0
    }
else
    {
        echo "Copy from Windows to Linux failed."
        echo false
        exit 1
    }
fi
