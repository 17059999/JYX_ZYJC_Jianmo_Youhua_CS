#-------------------------------------------------------------------------------
# Script  : SetServiceTypeAndStartService
# Usage   : Set a service startup type and start the service.
# Params  : -ServiceName <string>: The service name in WMI Win32_Service.
#           e.g. "Remote Procedure Call(RPC) Locator" is shortened as "rpclocator"
#           [-StartupType <string>]: The service startup type. Must be Automatic,
#           Manual, or Disabled. If not specified, Automatic will be taken.
# Remark  : If the specifid service already exists, a prompt will be printed but 
#           no exception will be thrown.
#-------------------------------------------------------------------------------  
Param
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$ServiceName,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Automatic","Manual","Disabled")]
    [string]$StartupType = "Automatic"
)

try
{
    # Get all the services
    $ServiceList = Get-WmiObject Win32_Service
    # Try to find the specified service
    $Service = ($ServiceList | Where-Object {$_.Name -like $ServiceName} | Select-Object -First 1)
    if($Service -eq $null)
    {
        throw "No service with the given name was found"
    }

    # Set startup type
    $ReturnValue = $Service.ChangeStartMode($StartupType)
    if($ReturnValue.ReturnValue -ne 0)
    {
        throw "Failed to change startup type. Return value: " + $ReturnValue.ReturnValue
    }
    # Start service
    $ReturnValue = $Service.StartService()
    # Return value 10 represents that the service has already started
    if($ReturnValue.ReturnValue -eq 10)
    {
        Write-Host "Service already started"
    }
    elseif($ReturnValue.ReturnValue -ne 0)
    {
        throw "Failed to change startup type. Return value: " + $ReturnValue.ReturnValue
    }
}
catch
{
    throw "Error happened while executing " + $MyInvocation.MyCommand.Name `
          + ": " + $_.Exception.Message
}