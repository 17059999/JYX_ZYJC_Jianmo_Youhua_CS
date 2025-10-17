#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-IIS7.ps1
## Purpose:        Install IIS7 on Windows Server 2008, Vista or Windows Server 7
## Version:        1.0 (5 Sep, 2008)
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Install IIS7 on Windows Server 2008, Vista or Windows Server 7."
    Write-host
    Write-host "Example: .\Install-IIS7.ps1"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#----------------------------------------------------------------------------
# Install IIS7 according to OS Version
#----------------------------------------------------------------------------
$osObj = Get-WmiObject -Class Win32_OperatingSystem
if ($osObj -eq $null)
{
    Write-Host "EXECUTE [Install-II7.ps1] FAILED. Cannot confirm OS version." -ForegroundColor Red
    return
}
$caption = $osObj.Caption
$version = $osObj.Version
if (($caption -ne $null) -and ($version -ne $null))
{
    if ($caption.ToUpper().Contains("VISTA"))
    {
        Write-Host "Installing IIS7 on VISTA..."
        cmd.exe /c start /w pkgmgr "/iu:IIS-WebServerRole;IIS-WebServer;IIS-CommonHttpFeatures;IIS-StaticContent;IIS-DefaultDocument;IIS-DirectoryBrowsing;IIS-HttpErrors;IIS-HttpRedirect;IIS-ApplicationDevelopment;IIS-ASPNET;IIS-NetFxExtensibility;IIS-ASP;IIS-CGI;IIS-ISAPIExtensions;IIS-ISAPIFilter;IIS-ServerSideIncludes;IIS-HealthAndDiagnostics;IIS-HttpLogging;IIS-LoggingLibraries;IIS-RequestMonitor;IIS-HttpTracing;IIS-CustomLogging;IIS-Security;IIS-BasicAuthentication;IIS-URLAuthorization;IIS-RequestFiltering;IIS-IPSecurity;IIS-Performance;IIS-HttpCompressionStatic;IIS-HttpCompressionDynamic;IIS-WebServerManagementTools;IIS-ManagementConsole;IIS-ManagementScriptingTools;IIS-ManagementService;IIS-IIS6ManagementCompatibility;IIS-Metabase;IIS-WMICompatibility;IIS-LegacyScripts;IIS-LegacySnapIn;WAS-WindowsActivationService;WAS-ProcessModel;WAS-NetFxEnvironment;WAS-ConfigurationAPI" 2>&1 | Write-Host
    }
    elseif ($caption.ToUpper().Contains("2008") -and $version.StartsWith("6.0"))
    {
        Write-Host "Installing IIS7 on Windows Server 2008..."
        cmd.exe /c start /w pkgmgr "/iu:IIS-WebServerRole;IIS-WebServer;IIS-CommonHttpFeatures;IIS-StaticContent;IIS-DefaultDocument;IIS-DirectoryBrowsing;IIS-HttpErrors;IIS-HttpRedirect;IIS-ApplicationDevelopment;IIS-ASPNET;IIS-NetFxExtensibility;IIS-ASP;IIS-CGI;IIS-ISAPIExtensions;IIS-ISAPIFilter;IIS-ServerSideIncludes;IIS-HealthAndDiagnostics;IIS-HttpLogging;IIS-LoggingLibraries;IIS-RequestMonitor;IIS-HttpTracing;IIS-CustomLogging;IIS-ODBCLogging;IIS-Security;IIS-BasicAuthentication;IIS-WindowsAuthentication;IIS-DigestAuthentication;IIS-ClientCertificateMappingAuthentication;IIS-IISCertificateMappingAuthentication;IIS-URLAuthorization;IIS-RequestFiltering;IIS-IPSecurity;IIS-Performance;IIS-HttpCompressionStatic;IIS-HttpCompressionDynamic;IIS-WebServerManagementTools;IIS-ManagementConsole;IIS-ManagementScriptingTools;IIS-ManagementService;IIS-IIS6ManagementCompatibility;IIS-Metabase;IIS-WMICompatibility;IIS-LegacyScripts;IIS-LegacySnapIn;IIS-FTPPublishingService;IIS-FTPServer;IIS-FTPManagement;WAS-WindowsActivationService;WAS-ProcessModel;WAS-NetFxEnvironment;WAS-ConfigurationAPI" 2>&1 | Write-Host
    }
    elseif ($caption.Contains("Windows Server 7") -or $version.StartsWith("6.1") -or $version.StartsWith("7."))
    {
        Write-Host "Installing IIS7 on Windows Server 7..."
        cmd.exe /c start /w pkgmgr "/iu:IIS-WebServerRole;IIS-WebServer;IIS-CommonHttpFeatures;IIS-StaticContent;IIS-DefaultDocument;IIS-DirectoryBrowsing;IIS-HttpErrors;IIS-HttpRedirect;IIS-ApplicationDevelopment;IIS-ASPNET;IIS-NetFxExtensibility;IIS-ASP;IIS-CGI;IIS-ISAPIExtensions;IIS-ISAPIFilter;IIS-ServerSideIncludes;IIS-HealthAndDiagnostics;IIS-HttpLogging;IIS-LoggingLibraries;IIS-RequestMonitor;IIS-HttpTracing;IIS-CustomLogging;IIS-Security;IIS-BasicAuthentication;IIS-URLAuthorization;IIS-RequestFiltering;IIS-IPSecurity;IIS-Performance;IIS-HttpCompressionStatic;IIS-HttpCompressionDynamic;IIS-WebServerManagementTools;IIS-ManagementConsole;IIS-ManagementScriptingTools;IIS-ManagementService;IIS-IIS6ManagementCompatibility;IIS-Metabase;IIS-WMICompatibility;IIS-LegacyScripts;IIS-LegacySnapIn;WAS-WindowsActivationService;WAS-ProcessModel;WAS-NetFxEnvironment;WAS-ConfigurationAPI;IIS-ODBCLogging;IIS-WindowsAuthentication;IIS-DigestAuthentication;IIS-ClientCertificateMappingAuthentication;IIS-IISCertificateMappingAuthentication" 2>&1 | Write-Host
    }
    else
    {
        Write-Host "Do not support to install IIS7 on OS version: $caption ($version)" -ForegroundColor Red
        return
    }
}
else
{
    Write-Host "EXECUTE [Install-II7.ps1] FAILED. Cannot confirm OS version." -ForegroundColor Red
    return
}

#----------------------------------------------------------------------------
# Exit
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Install-II7.ps1] SUCCEED" -Foregroundcolor Green
