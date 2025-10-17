#-----------------------------------------------------------------------------
# Script: InstallWebServer
# Usage : Install IIS on the computer.
# Remark: A restart is needed after installation.
#-----------------------------------------------------------------------------

Try
{
    Add-WindowsFeature Web-Server -IncludeAllSubFeature -Restart:$False -ErrorAction Stop
}
catch
{
    Throw "Unable to install IIS. Error happeded: " + $_.Exception.Message
}
# Restart is needed