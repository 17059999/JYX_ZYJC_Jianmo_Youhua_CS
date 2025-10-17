#!/bin/sh
# parameter list: user, password, IP
echo "Startup FreeRDP ..."    
DISPLAY=:0 xfreerdp -u $1 -p $2 --rfx -f -a 32 -x 1 --sec rdp --composition --ignore-certificate --plugin cliprdr --plugin rdpsnd --plugin rdpdr --data disk:administrator:/ --  $3