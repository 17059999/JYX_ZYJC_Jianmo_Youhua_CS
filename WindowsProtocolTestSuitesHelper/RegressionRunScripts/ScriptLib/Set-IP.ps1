#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Set-IP.ps1
## Purpose:        Configurate the IP Address and Subnet Mask.
## Version:        1.0 (24 Jun, 2008)
##
##############################################################################

#----------------------------------------------------------------------------
# No Parameters
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-IP.ps1] ..." -foregroundcolor cyan

# Constants and defaults
Set-Variable -Name CONST_ERROR       -Option Constant -Value 0
Set-Variable -Name CONST_SHOW_USAGE  -Option Constant -Value 1
Set-Variable -Name CONST_PROCEED     -Option Constant -Value 2

Set-Variable -Name CONST_MODE_NAME   -Option Constant -Value 1
Set-Variable -Name CONST_MODE_INDEX  -Option Constant -Value 2
Set-Variable -Name CONST_MODE_MAC    -Option Constant -Value 3

Set-Variable -Name DEF_MODE        -Option Constant -Value $CONST_MODE_INDEX
Set-Variable -Name DEF_IP_Address  -Option Constant -Value ""
Set-Variable -Name DEF_SubnetMask  -Option Constant -Value ""

# Global variables
$errorMessage = ""

$IPAddress  = $DEF_IP_Address
$SubnetMask = $DEF_SubnetMask
$NetworkConnectionName = ""
$NetworkAdapterIndex   = -1
$NetworkAdapterMac     = ""

$mode = $DEF_MODE
$modeSetFlag = $false
$IPFlag = $false
$maskFlag = $false
$lastNetworkAdapterFlag = $true

#----------------------------------------------------------------------------
# Function: Dump-GlobalVariables
# Usage   : Output all global variables for debug
#----------------------------------------------------------------------------
function Dump-GlobalVariables
{    
    Write-Host "errorMessage  =$errorMessage" -ForegroundColor Yellow
    
    Write-Host "IPAddress     =$IPAddress" -ForegroundColor Yellow
    Write-Host "SubnetMask    =$SubnetMask" -ForegroundColor Yellow
    Write-Host "NetworkConnectionName =$NetworkConnectionName" -ForegroundColor Yellow
    Write-Host "NetworkAdapterIndex   =$NetworkAdapterIndex" -ForegroundColor Yellow
    Write-Host "NetworkAdapterMac     =$NetworkAdapterMac" -ForegroundColor Yellow
    
    Write-Host "mode        =$mode" -ForegroundColor Yellow
    Write-Host "modeSetFlag     =$modeSetFlag" -ForegroundColor Yellow
    Write-Host "IPFlag =$IPFlag " -ForegroundColor Yellow
    Write-Host "maskFlag     =$maskFlag" -ForegroundColor Yellow
    Write-Host "lastNetworkAdapterFlag =$lastNetworkAdapterFlag " -ForegroundColor Yellow    
}



#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Configurate the IP Address and Subnet Mask."
    Write-host "Parameters:"
    Write-host "       -IP <IP Address>  : Specify the IP address you want to set. (Required)"
    Write-host "       -MASK <Subnet Mask>  :  Specify the Subnet Mask you want to set. (Required)"
    Write-host "       -NAME <Conection Name>  :  Set the IP and Mask of a connection named <Conection Name>. (Optional)"
    Write-host "       -INDEX <Interface Index>  :  Set the IP and Mask of a Network Adapter, whose Interface Index in route table is <Interface Index>. (Optional)"
    Write-host "       -MAC <MAC Address>  :  Set the IP and Mask of a Network Adapter, whose MAC Address is <MAC Address>. (Optional)"
    Write-host "Notes:       "
    Write-host "       1. You cannot specify '-NAME', '-INDEX', '-MAC' at the same time."    
    Write-host "       2. When you want to set IP Address of the last Network Adapter, specify '-INDEX' of value '-1'."
    Write-host "       3. Do not support Windows XP so far."    
    Write-host
    Write-host "Examples: "
    Write-host "       1. Set-IP.ps1 -ip `"192.168.0.254`" -mask `"255.255.255.0`" -name `"Local Area Connection 2`" "
    Write-host "          Desciption: Set the IP & Mask of a connection named `"Local Area Connection 2`"."
    Write-host
    Write-host "       2. Set-IP.ps1 -ip `"192.168.0.254`" -mask `"255.255.255.0`" -mac `"00:15:5D:D4:5A:00`" "
    Write-host "          Desciption: Set the IP & Mask of a Network Adapter, whose MAC Address is `"00:15:5D:D4:5A:00`"."
    Write-host
    Write-host "       3. Set-IP.ps1 -ip `"192.168.0.254`" -mask `"255.255.255.0`" -index 10 "
    Write-host "          Desciption: Set the IP & Mask of a Network Adapter, whose Interface Index in route table is 10."    
    Write-host
    Write-host "       4. Set-IP.ps1 -ip `"192.168.0.254`" -mask `"255.255.255.0`" -index -1 "    
    Write-host "          Desciption: Set the IP & Mask of the last Network Adapter."    
    Write-host
    Write-host "       5. Set-IP.ps1 -ip `"192.168.0.254`" -mask `"255.255.255.0`""    
    Write-host "          Desciption: Set the IP & Mask of the last Network Adapter."    
    Write-host
}

#----------------------------------------------------------------------------
# Function: Deal-Parameter
# Usage   : Helper function used by Parse-CmdLine
#----------------------------------------------------------------------------
function Deal-Parameter([string]$varName, [ref]$var, [ref]$argIndex)
{ 
    [bool]$result = $true
    $argIndex.Value++
    #Write-Host "script:args[argIndex.Value]=$script:args[$argIndex.Value]"
    if ($script:args[$argIndex.Value] -eq $null -or $script:args[$argIndex.Value].ToString().StartsWith("-"))
    {
        $script:errorMessage = "Invalid " + $varName
        $result = $false
    }
    else
    {
        $var.Value = $script:args[$argIndex.Value]
    }
    return $result
}

#----------------------------------------------------------------------------
# Function: Valid-Index
# Usage   : Helper function used by Parse-CmdLine
#----------------------------------------------------------------------------
function Valid-Index([string]$varName, [ref]$var, [ref]$argIndex)
{ 
    [bool]$result = $true
    $argIndex.Value++
    if ($script:args[$argIndex.Value] -ne $null)
    {
        $var.Value = $script:args[$argIndex.Value]    
    }
    else
    {
        $script:errorMessage = "Invalid " + $varName
        $result = $false
    }
    return $result
}


#----------------------------------------------------------------------------
# Function: Parse-CmdLine
# Usage   : Parses the command line.
#----------------------------------------------------------------------------
function Parse-CmdLine
{    
    $args = Get-Variable -Name args -valueonly -Scope script
    if ($args.length -lt 1)   #No arguments have been received
    {
        return $CONST_SHOW_USAGE
    }
    if ($args[0] -match '-(\?|(h|(help)))')  #User is asking for help
    {
        return $CONST_SHOW_USAGE
    }
    
    #Parse each parameter
    for ($argIndex = 0; $argIndex -lt $args.length; $argIndex++)
    {
        $argValue = $args[$argIndex]
        #Write-Host "Dealing: " $argValue
        switch ($argValue) 
        {
            "-ip" #IP Address
            {
                if ((Deal-Parameter "IP Address" ([ref]$script:IPAddress) ([ref]$argIndex)) -ne $true){ return $CONST_ERROR }
                $script:IPFlag = $true
            }
            "-mask" #Subnet Mask
            {
                if ((Deal-Parameter "Subnet Mask" ([ref]$script:SubnetMask) ([ref]$argIndex)) -ne $true){ return $CONST_ERROR }
                $script:maskFlag = $true
            }
            "-name" # Specify Network Adapter by connection name
            {
                if ((Deal-Parameter "Network Connection Name" ([ref]$script:NetworkConnectionName) ([ref]$argIndex)) -ne $true){ return $CONST_ERROR }
                if ($script:modeSetFlag -eq $true)
                {
                    $script:errorMessage = "You You can not specify '-name', '-index', '-max' at the same time."
                    return $CONST_ERROR
                }
                $script:modeSetFlag = $true
                $script:mode = $CONST_MODE_NAME
                $script:lastNetworkAdapterFlag = $false
                
            }
            "-index" #Specify Network Adapter by adapter index
            {
                if ((Valid-Index "Network Adapter Index" ([ref]$script:NetworkAdapterIndex) ([ref]$argIndex)) -ne $true){ return $CONST_ERROR }
                if ($script:modeSetFlag -eq $true)
                {
                    $script:errorMessage = "You You can not specify '-name', '-index', '-max' at the same time."
                    return $CONST_ERROR
                }
                $script:modeSetFlag = $true
                $script:mode = $CONST_MODE_INDEX
                if ($script:NetworkAdapterIndex.ToString().Equals("-1"))
                {
                    $script:lastNetworkAdapterFlag = $true
                }
                else
                {
                    $script:lastNetworkAdapterFlag = $false
                }
            }
            "-mac" #Specify Network Adapter by MAC address
            {
                if ((Deal-Parameter "Network Adapter MAC Address" ([ref]$script:NetworkAdapterMac) ([ref]$argIndex)) -ne $true){ return $CONST_ERROR }
                if ($script:modeSetFlag -eq $true)
                {
                    $script:errorMessage = "You You can not specify '-name', '-index', '-max' at the same time."
                    return $CONST_ERROR
                }
                $script:modeSetFlag = $true
                $script:mode = $CONST_MODE_MAC
                $script:lastNetworkAdapterFlag = $false
            }
            default #Unknown parameter
            {
                $script:errorMessage = "Unknown parameter: " + $argValue
                return $CONST_ERROR
            }
        } #switch
    } #for
    
    if ($script:IPFlag -eq $false -or $script:maskFlag -eq $false) # MUST specify IP Address $ Subnet Mask in prameter
    {
        $script:errorMessage = "You MUST specify IP Address $ Subnet Mask in prameter."
        return $CONST_ERROR
    }

    return $CONST_PROCEED
}


#----------------------------------------------------------------------------
# Function: Find-Lask-NetworkAdapter-Index
# Usage   : Find the last Network Adapter's Interface Index
#----------------------------------------------------------------------------
function Find-Lask-NetworkAdapter-Index
{  
    $returnIndex = -1
    $naConfigSet = gwmi Win32_NetworkAdapterConfiguration
    if ($naConfigSet -eq $null)
    {
        Throw "Error: Cannot get WMI Object."
    }
    foreach($obj in $naConfigSet)
    {
        if($obj.IPEnabled -eq $true -and $obj.InterfaceIndex -gt $returnIndex)
        {
            $returnIndex = $obj.InterfaceIndex
        }
    }
    return $returnIndex
}


#----------------------------------------------------------------------------
# Function: Get-NetworkAdapter-Object
# Usage   : Get the Network Adapter Configuration object
#----------------------------------------------------------------------------
function Get-NetworkAdapter-Object
{   
    $naConfigObj = $null
    $naSet       = gwmi Win32_NetworkAdapter
    $naConfigSet = gwmi Win32_NetworkAdapterConfiguration
    if ($naSet -eq $null -or $naConfigSet -eq $null)
    {
        Throw "Error: Cannot get WMI Object."
    }
    
    switch ($script:mode) 
    {
        $CONST_MODE_NAME #find by name
        {
            Write-Host "Finding by connetion name..." -ForegroundColor Yellow
            $interfaceIndex = $null
            foreach ($naObj in $naSet)
            {
                if ($naObj.NetConnectionID -eq $script:NetworkConnectionName)
                {
                    $interfaceIndex = $naObj.InterfaceIndex
                    break
                }
            }
            if ($interfaceIndex -eq $null)
            {
                Throw "Error: Cannot find Network Adapter by name:$script:NetworkConnectionName"
            }

            foreach ($nacObj in $naConfigSet)
            {
                if ($nacObj.IPEnabled -eq $true -and $nacObj.InterfaceIndex -eq $interfaceIndex)
                {
                    $naConfigObj = $nacObj
                    break
                }
            }    
        }
        $CONST_MODE_INDEX #find by index
        {
            Write-Host "Finding by index..." -ForegroundColor Yellow
            if ($script:lastNetworkAdapterFlag -eq $true) # find the last Network Adapter
            {
                Write-Host "Finding the last Network Adapter..." -ForegroundColor Yellow
                $script:NetworkAdapterIndex = Find-Lask-NetworkAdapter-Index
                #Write-Host "The last Network Adapter interface index is $script:NetworkAdapterIndex" -ForegroundColor Yellow
                if ($script:NetworkAdapterIndex -ne $null -and $script:NetworkAdapterIndex.ToString().Equals("-1"))
                {
                    Throw "Error: Cannot find the last Network Adapter."
                }
            }
            
            foreach ($nacObj in $naConfigSet)
            {
                if ($nacObj.IPEnabled -eq $true -and $nacObj.InterfaceIndex -eq $script:NetworkAdapterIndex)
                {
                    $naConfigObj = $nacObj
                    break
                }
            }                
        }
        $CONST_MODE_MAC #find by mac address
        {
            Write-Host "Finding by MAC Address..." -ForegroundColor Yellow
            foreach ($nacObj in $naConfigSet)
            {
                if ($nacObj.IPEnabled -eq $true -and $nacObj.MACAddress -eq $script:NetworkAdapterMac) 
                {
                    $naConfigObj = $nacObj
                    break
                }
            }            
        }
        
    } #switch

    return $naConfigObj
}


#----------------------------------------------------------------------------
# Function: Parse-Result
# Usage   : Parse the run result
#----------------------------------------------------------------------------
function Parse-Result([ref]$resultObj)
{    
    if ($resultObj -eq $null -or $resultObj.Value -eq $null -or $resultObj.Value.ReturnValue -eq $null)
    {
        Write-Host "Cannot parse execution result."
    }
    switch($resultObj.Value.ReturnValue)
    {
            0
            {
                Write-Host "IP address and subnetmask are successfully set." -foregroundcolor Green
                return 0
            }
            1
            {
                Write-Host "IP address and subnetmask are successfully set. Reboot required" -foregroundcolor Yellow
                return 0    
            }
            66
            {
                Write-Host "Invalid subnet mask!" -foregroundcolor Red
                return 66
            }
            70
            {
                Write-Host "Invalid IP address!" -foregroundcolor Red
                return 70
            }
            91
            {
                Write-Host "Access denied!" -foregroundcolor Red
                return 91
            }
            default
            {
                Write-Host "Unknown Failure! IP address and subnetmask set failed!" -foregroundcolor Red
                return $resultObj.Value.ReturnValue
            }
    }
}


#----------------------------------------------------------------------------
# Function: Main
# Usage   : Executes the main script logic
#----------------------------------------------------------------------------
function Main()
{    
    $args = Get-Variable -Name args -valueonly -Scope script
    $parseCmdLineResult = Parse-CmdLine
    switch ($parseCmdLineResult ) 
    {
        $CONST_SHOW_USAGE
        {
            Show-ScriptUsage
            return
        }
        $CONST_PROCEED
        {
            # Command line parameters are ok.
            Write-Host "Main: Parse command line succeed." -ForegroundColor Green
            $configObj = Get-NetworkAdapter-Object
            if ($configObj -eq $null)
            {
                Throw "Error: Cannot find the Network Adapter through specified prameter."
            }
            Write-Host "Sepcified Nework Adapter found." -ForegroundColor Green
            $resultObj = $configObj.EnableStatic($script:IPAddress, $script:SubnetMask) #Set the IP & Mask
            [int]$ret = Parse-Result ([ref]$resultObj)
            if ($ret -ne 0)
            {
                Throw "Error code: $ret"
            }
            Write-Host "Set IP Address & Subnet Mask succefully complete." -ForegroundColor Green
        }
        $CONST_ERROR
        {
            Throw "Error: " + $errorMessage
        }
        default
        {
            #Should not be here
            Throw "Error: Unknown error!"
        }
    }
}


#----------------------------------------------------------------------------
# Start the script
#----------------------------------------------------------------------------
Main
#Dump-GlobalVariables

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Set-IP.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Yellow

