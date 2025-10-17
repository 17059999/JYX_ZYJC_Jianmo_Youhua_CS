'Script to install the VHidPen virtual driver for automation
'This script does not copy the helper exes required. This is to run it in the Rhino environment. This script just takes care of installing the driver, given that the other file are present. I.e. VHidPen.sys, VHidPen.inf, devcon.exe, DismissDialog.exe, IsDriverInstalled.exe
'Usage :
'	cscript SetupVHidPenWithoutCopy.vbs <install|remove> 
'	where,
'This setup script produces a log file named SetupVHidPenWithoutCopyLog.txt
Option Explicit
'On Error Resume Next

Dim fso
Dim wshShell
Dim vSubFolder




'--------------Main. I love C++ :) ----------------
Dim strLogTextFile
Dim vLogTextStream    'For tracing
Dim objArgs
Dim strCommandLine
Dim varRet
Dim strOperation 'install or remove


Set fso = WScript.CreateObject("Scripting.FileSystemObject")
Set wshShell = WScript.CreateObject("WScript.Shell")
Set objArgs = WScript.Arguments

strLogTextFile=fso.GetParentFolderName(WScript.ScriptFullName) + "\SetupVHidPenWithoutCopy.txt"
Set vLogTextStream = fso.CreateTextFile(strLogTextFile,True)

if objArgs.Count = 0 then
	WScript.Echo "Usage Error : Correct Usage is --->"
	WScript.Echo "cscript SetupVHidPenWithoutCopy.vbs <install|remove> "
	WScript.Echo "where,"
 	WScript.Echo "	<install|remove>: is the operation to be performed"
	WScript.Echo "This setup script produces a log file named SetupVHidPenLogWithoutCopy.txt"
	WScript.Quit 
end if

'Operation to be performed on the driver
if objArgs.Item(0) = "install" then
	strOperation = "install"
else
	strOperation = "remove"
end if


'Start the process if the driver is not installed
strCommandLine= "IsDriverInstalled.exe"  
vLogTextStream.WriteLine("Started : " + strCommandLine)
varRet=wshShell.Run (strCommandLine,1,True)

if strOperation = "install" then
	if varRet<>0 then
		'Driver is already installed
		vLogTextStream.WriteLine("VHidPen driver is already installed")

		'Set system environment variable used to prevent accidental uninstall of existing driver.
		strCommandLine="sysset.wsf _VHidPenPreviousInstall TRUE"
		WshShell.Run strCommandLine, 1, false

                'Set return value to indicate success
                varRet=0
	else
		'Clear system env variable used to prevent automated uninstall of existing installed driver.
		strCommandLine="sysset.wsf _VHidPenPreviousInstall /r"
		WshShell.Run strCommandLine, 1, false

		'Starting up the utility to dismiss the Driver Package Installation Error
		strCommandLine="DismissDialog.exe -m newdev.dll -r 2180 -i 2 -w 30000"
		WshShell.Run strCommandLine, 1, false
		vLogTextStream.WriteLine("Started : " + strCommandLine)

		'Starting up the utility to dismiss the 3 New Hardware Found wizards
		strCommandLine="DismissDialog.exe -m newdev.dll -r 2040 -i 3 -w 60000"  
		WshShell.Run strCommandLine, 1, false
		vLogTextStream.WriteLine("Started : " + strCommandLine)
		
		'Install Virtual Driver
		strCommandLine="devcon.exe install vhidpen.inf HID\VirtualHidTablet" 
		WScript.Echo "Installing Virtual Tablet Driver..."
		vLogTextStream.WriteLine("Executing : " + strCommandLine)
		varRet = WshShell.Run (strCommandLine, 1, true)
	end if
else
	if varRet=0 then
		'Driver is already uninstalled
		vLogTextStream.WriteLine("VHidPen driver is already removed")
		strCommandLine="sysset.wsf _VHidPenPreviousInstall /r"
		WshShell.Run strCommandLine, 1, false
		'varRet already 0 so no change needed to indicate success.
	else
                'IFF VHidPen was not installed manually 
		'  (i.e. IsDriverInstalled.exe did not return true when this script was run to install the driver) remove it now.
                if WshShell.ExpandEnvironmentStrings("%_VHidPenPreviousInstall%") = "%_VHidPenPreviousInstall%" then
			'Remove Virtual Driver
			strCommandLine="devcon.exe remove HID\VirtualHidTablet" 
			WScript.Echo "Removing Virtual Driver..."
			vLogTextStream.WriteLine("Executing : " + strCommandLine)
			varRet=WshShell.Run (strCommandLine, 1, true)
                else
			WScript.Echo "Not Removing previously installed Virtual Tablet Driver"
			vLogTextStream.WriteLine("Not Removing previously installed Virtual Tablet Driver")
			'Indicate success -- no need to uninstall.
			varRet=0
 		end if
	end if
end if

WScript.Quit (varRet)
