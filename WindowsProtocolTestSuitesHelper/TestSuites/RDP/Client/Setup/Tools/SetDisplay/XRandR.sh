# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

#############################################################################
##
## Microsoft Bash Scripting
## File:           XRandR.sh
## Requirements:   Bash 
## Supported OS:   Linux/Unix
##
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $1:                      The desired Width of the screen
# $2:                      The desired height of the screen
#----------------------------------------------------------------------------

#!/bin/bash
# Supply arguments as width height refresh rate respectively. e.g. 1024 720 60
# Below uses gtf to generate a new modeline for generating custom resolution
# e.g modeline "1024x720"  59.44  1024 1072 1176 1328  720 721 724 746  -Hsync  +Vsync

# checks that arguments are passed
if [ $# -neq 3 ]; then
    echo "No arguments provided, pass arguments as: width, height & refresh rate"
    exit 1
fi

# checks for active monitor/screen name
display=$(xrandr | grep -Po '.+(?=\sconnected)')

# uses the arguments supplied to form a new mode name 
# e.g. "1024x720"
modename="$1x$2"

# reads the result of cvt and command arguments, then converts into an array
readarray -d Modeline tempModeline<<< $(cvt -r $1 $2)

# retrieves only the substring to get the modeline values from character 25 - end of string
modeline=$(cut -c 25-${#tempModeline[2]} <<< ${tempModeline[2]})

xrandr --newmode $modename $modeline && xrandr --addmode $display $modename

# forces the monitor to take on newly created custom resolution
xrandr --size $modename
xrandr --output $display --mode $modename