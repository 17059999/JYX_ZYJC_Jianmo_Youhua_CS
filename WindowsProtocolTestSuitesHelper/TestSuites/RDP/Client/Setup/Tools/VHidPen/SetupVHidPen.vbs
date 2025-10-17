'Script to install the VHidPen virtual driver for automation
'This script copies the helper exes required. This is to run it in the non-Rhino or  WTT environment. This script takes care of copying and installing the driver files and th other helper binaries. I.e. VHidPen.sys, VHidPen.inf, devcon.exe, DismissDialog.exe, IsDriverInstalled.exe
'Usage :
'	cscript SetupVHidPen.vbs <install|remove> [Share]
'	where,
' 		Share :is the share from which to get the binaries
'                       for DismissDialog.exe and IsDriverInstalled.exe
'			default is %_nttree%\nttest\driverstest\mobilepc and assumes you are running from a razzle window.
'       		[share]\VHidPen
'This setup script produces a log file named SetupVHidPenLog.txt
Option Explicit
'On Error Resume Next

Dim fso
Dim wshShell
Dim vSubFolder



'Script to install the Context End to End testing app

'--------------Main. I love C++ :) ----------------
Dim strLogTextFile
Dim vLogTextStream    'For tracing
Dim strTabletBldRoot
Dim strDriverRoot
Dim strTabletDestination
Dim strTabletDestinationFolder
Dim objArgs
Dim strVirtualDriverSource
Dim strDismissDialogSource
Dim strIsDriverInstalledSource
Dim strXAccSource
Dim strVirtualDriverDestination
Dim strCommandLine
Dim varRet
Dim strOperation 'install or remove


Set fso = WScript.CreateObject("Scripting.FileSystemObject")
Set wshShell = WScript.CreateObject("WScript.Shell")
Set objArgs = WScript.Arguments

strLogTextFile=fso.GetParentFolderName(WScript.ScriptFullName) + "\SetupVHidPen.txt"
Set vLogTextStream = fso.CreateTextFile(strLogTextFile,True)

if objArgs.Count = 0 then
	WScript.Echo "Usage Error : Correct Usage is --->"
	WScript.Echo "cscript SetupVHidPen.vbs <install|remove> [Share]"
	WScript.Echo "where,"
 	WScript.Echo "	<install|remove>: is the operation to be performed"
 	WScript.Echo "	Share: is the share from which to get the binaries for DismissDialog.exe and IsDriverInstalled.exe"
	WScript.Echo "		default is %_nttree%\nttest\driverstest\mobilepc and assumes you are running from a razzle window."
        WScript.Echo "          Binaries for the driver are picked up from [Share]\VHidPen\"
	WScript.Echo "This setup script produces a log file named SetupVHidPenLog.txt"
	WScript.Quit 
end if

'Operation to be performed on the driver
if objArgs.Item(0) = "install" then
	strOperation = "install"
else
	strOperation = "remove"
end if


'Is there a build share specified on the command line
if objArgs.Count > 1 then
	'If there is a path given on command line then take that path for the build drop location
	strTabletBldRoot=objArgs.Item(1)
else
	'Else this is the default
	strTabletBldRoot=WshShell.ExpandEnvironmentStrings("%_nttree%\nttest\driverstest\mobilepc")
end if

'Folder that contains the driver files
strDriverRoot=strTabletBldRoot + "\VHIDPen"

'Set up the folder to copy files on the target machine
strTabletDestination=WshShell.ExpandEnvironmentStrings("%SystemDrive%") + "\VHidPen\"
strTabletDestinationFolder=WshShell.ExpandEnvironmentStrings("%SystemDrive%") + "\VHidPen"

'Source files
strVirtualDriverSource= strDriverRoot+ "\*"
strDismissDialogSource=strTabletBldRoot+"\DismissDialog.exe"
strIsDriverInstalledSource=strTabletBldRoot+"\IsDriverInstalled.exe"
strXAccSource = strTabletBldRoot 

if Not(fso.FolderExists(strTabletDestination)) then
	vLogTextStream.WriteLine("VHidPen desitnation folder does not exist. Creating..." + strTabletDestination)
else
	vLogTextStream.WriteLine("VHidPen desitnation folder already exists. Deleting and recreating..." + strTabletDestination)
	fso.DeleteFolder(strTabletDestinationFolder)
end if
fso.CreateFolder(strTabletDestination)

vLogTextStream.WriteLine("Copying Virutal Driver sources...")
vLogTextStream.WriteLine("From : " + strVirtualDriverSource)
vLogTextStream.WriteLine("To : " + strTabletDestination)
WScript.Echo "Copying Virtual Driver ..."
fso.CopyFile strVirtualDriverSource, strTabletDestination, true

vLogTextStream.WriteLine("Copying DismissDialog.exe  utility...")
vLogTextStream.WriteLine("From : " + strDismissDialogSource)
vLogTextStream.WriteLine("To : " + strTabletDestination)
WScript.Echo "Copying DismissDialog utility ..."
fso.CopyFile strDismissDialogSource, strTabletDestination, true
	
vLogTextStream.WriteLine("Copying IsDriverInstalled.exe  utility...")
vLogTextStream.WriteLine("From : " + strIsDriverInstalledSource)
vLogTextStream.WriteLine("To : " + strTabletDestination)
WScript.Echo "Copying IsDriverInstalled utility ..."
fso.CopyFile strIsDriverInstalledSource, strTabletDestination, true

'XAcc
vLogTextStream.WriteLine("Copying XAcc binaries...")
vLogTextStream.WriteLine("From : " + strXAccSource )
vLogTextStream.WriteLine("To : " + strTabletDestination)
WScript.Echo "Copying XAcc binaries ..."
fso.CopyFile strXAccSource + "\XAcc.dll", strTabletDestination, true
fso.CopyFile strXAccSource + "\XLogging.dll", strTabletDestination, true
vLogTextStream.WriteLine("Registering XAcc binaries...")
WScript.Echo "Registering XAcc binaries ..."
strCommandLine=WshShell.ExpandEnvironmentStrings("%SystemRoot%") + "\system32\regsvr32.exe /s " + strXAccSource  + "\xAcc.dll"  
WshShell.Run strCommandLine, 1, false
strCommandLine=WshShell.ExpandEnvironmentStrings("%SystemRoot%") + "\system32\regsvr32.exe /s " + strXAccSource  + "\xLogging.dll"  
WshShell.Run strCommandLine, 1, false



'Start the process if the driver is not installed
strCommandLine=strTabletDestination + "IsDriverInstalled.exe"  
vLogTextStream.WriteLine("Started : " + strCommandLine)
varRet=wshShell.Run (strCommandLine,1,True)

if strOperation = "install" then
	if varRet<>0 then
		'Driver is already installed
		vLogTextStream.WriteLine("VHidPen driver is already installed")
	else
	
		'Starting up the utility to dismiss the Driver Package Installation Error
		strCommandLine = strTabletDestination + "DismissDialog.exe -m newdev.dll -r 2180 -i 2 -w 30000"
		WshShell.Run strCommandLine, 1, false
		vLogTextStream.WriteLine("Started : " + strCommandLine)

		'Starting up the utility to dismiss the 3 New Hardware Found wizards
		strCommandLine=strTabletDestination + "DismissDialog.exe -m newdev.dll -r 2040 -i 3 -w 60000"  
		WshShell.Run strCommandLine, 1, false
		vLogTextStream.WriteLine("Started : " + strCommandLine)
		
		'Install Virtual Driver
		strCommandLine=strTabletDestination + "devcon.exe install " + strTabletDestination + "vhidpen.inf HID\VirtualHidTablet" 
		WScript.Echo "Installing Virtual Driver..."
		vLogTextStream.WriteLine("Executing : " + strCommandLine)
		WshShell.Run strCommandLine, 1, true
	end if
else
	if varRet=0 then
		'Driver is already uninstalled
		vLogTextStream.WriteLine("VHidPen driver is already removed")
	else
		'Remove Virtual Driver
		strCommandLine=strTabletDestination + "devcon.exe remove HID\VirtualHidTablet" 
		WScript.Echo "Removing Virtual Driver..."
		vLogTextStream.WriteLine("Executing : " + strCommandLine)
		WshShell.Run strCommandLine, 1, true
	end if
	'remove _VHidPenPreviousInstall if set in case automation script (SetupVHidPenWithoutCopy.vbs) is run against this machine at a later date.
        strCommandLine="sysset.wsf _VHidPenPreviousInstall /r"
	WshShell.Run strCommandLine, 1, false
	
end if
