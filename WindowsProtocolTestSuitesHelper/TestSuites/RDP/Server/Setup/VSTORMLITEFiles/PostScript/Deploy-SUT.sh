#!/bin/sh
echo "Start to install freerdp ..."
sudo /usr/bin/apt-get -y remove freerdp-x11 > /dev/null 2>&1
sudo /usr/bin/apt-get -y install freerdp-x11 > /dev/null 2>&1 
echo "Freerdp is installed"
