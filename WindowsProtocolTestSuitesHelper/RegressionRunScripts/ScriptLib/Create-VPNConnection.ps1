###################################################################################
##
## Create-VPNConnection.ps1
## Originally from Microsoft, used internal only.
## 
## Create a VPN connection with configuration xml file
##
###################################################################################

#-----------------------------------------------------------------------------------------------------------------
# Disclaimer
#
# This sample script and its accompanying sample data file are not supported under any Microsoft standard 
# support program or service. The sample script and data file are provided AS IS without warranty of any 
# kind. Microsoft further disclaims all implied warranties including, without limitation, any implied 
# warranties of merchantability or of fitness for a particular purpose. The entire risk arising out of the 
# use or performance of the sample scripts and documentation remains with you. In no event shall Microsoft,
# its authors, or anyone else involved in the creation, production, or delivery of the scripts be liable for
# any damages whatsoever (including, without limitation, damages for loss of business profits, business 
# interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability 
# to use the sample scripts or documentation, even if Microsoft has been advised of the possibility of such damages.
#
#-------------------------------------------------------------------------------------------------------------------
#
# Create-RAS-Connection.ps1
#
# This sample script reads an .xml data file and uses the information in the file to create VPN or dial-up remote
# connections on the computer on which the script is run. For more information, see the accompanying documentation.
#
#--------------------------------------------------------------------------------------------------------------------


function Compile-Csharp ([string] $code, [Array]$References)
{
    # Get an instance of the CSharp code provider
    $cp = New-Object Microsoft.CSharp.CSharpCodeProvider
    $refs = New-Object Collections.ArrayList
    $refs.AddRange( @("${framework}System.dll",
    #"${PsHome}\System.Management.Automation.dll",
    #"${PsHome}\Microsoft.PowerShell.ConsoleHost.dll",
    "${framework}System.Windows.Forms.dll",
    "${framework}System.Data.dll",
    "${framework}System.Drawing.dll",
    "${framework}System.XML.dll"))
    if ($References.Count -ge 1) 
    {
        $refs.AddRange($References)
    }
    # Build up a compiler params object...
    $cpar = New-Object System.CodeDom.Compiler.CompilerParameters
    $cpar.GenerateInMemory = $true
    $cpar.GenerateExecutable = $true
    $cpar.IncludeDebugInformation = $false
    $cpar.CompilerOptions = "/target:library"
    $cpar.ReferencedAssemblies.AddRange($refs)
    $cr = $cp.CompileAssemblyFromSource($cpar, $code)
    if ( $cr.Errors.Count)
    {
        $codeLines = $code.Split("`n");
        foreach ($ce in $cr.Errors)
        {
            write-host "Error: $($codeLines[$($ce.Line - 1)])"
            $ce | out-default
        }
    Throw "INVALID DATA: Errors encountered while compiling code"
    }
}

#CSharp code goes here
$code = @'
    using System;
    using System.IO;
    using System.Text;
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    namespace RemoteAccessSettings
    {
        using RasApi32.RasApiConstants;
        using WinInet.WinInetConstants;

        [XmlRoot("RemoteAccessEntries")]
        public class RemoteAccessEntries
        {
            private ArrayList EntryList;

            public RemoteAccessEntries()
            {
                EntryList = new ArrayList();
            }

            [XmlElement("RemoteAccessEntry")]
            public RemoteAccessEntry[] Entries
            {
                get
                {
                    RemoteAccessEntry[] entries = new RemoteAccessEntry[EntryList.Count];
                    EntryList.CopyTo(entries);
                    return entries;
                }
                set
                {
                    if (value == null) return;

                    RemoteAccessEntry[] entries = (RemoteAccessEntry[])value;
                    EntryList.Clear();
                    foreach (RemoteAccessEntry entry in entries)
                    {
                        EntryList.Add(entry);
                    }
                }
            }

            public static RemoteAccessEntries LoadXML(string xmlFilePath)
            {
                FileStream stream = null;
                XmlSerializer serializer = null;
                RemoteAccessEntries connections = null;

                try
                {
                    serializer = new XmlSerializer(typeof(RemoteAccessEntries));
                    stream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read);
                    connections = (RemoteAccessEntries)serializer.Deserialize(stream);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Operation to load the XML file in memory failed with error: {0}", ex.Message);
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }

                return connections;
            }
        }

        public class Destination
        {
            public string DestinationAddress;

            public Destination()
            {
            }

            public Destination(string destinationAddress)
            {
                DestinationAddress = destinationAddress;
            }
        }

        public class RemoteAccessEntry
        {
            //
            //  RAS connection settings
            //
            public string Name;
            public bool SharedProfile;
            public RasConnectionType ConnectionType;
            public string DefaultDestination;
            public bool Negotiate_IPv4;
            public bool Negotiate_IPv6;
            public RasVpnStrategy VpnStrategy;
            public RasEncryptionType EncryptionType;
            public bool RouteIPv4TrafficOverRAS;
            public bool RouteIPv6TrafficOverRAS;
            public bool ShowUsernamePassword;
            public bool ShowDomain;
            public bool ShowDialProgressBar;
            public bool RequireCHAP;
            public bool RequireMSCHAPv2;
            public bool RequireEAP;
            public bool RequireEncryptedPassword;
            public bool RequireMsEncryptedPassword;
            public bool DontCacheRASCredentialsInCredman;
            public bool ReconnectIfDropped;

            private ArrayList DestinationList;

            [XmlElement("Destination")]
            public Destination[] Destinations
            {
                get
                {
                    Destination[] destinations = new Destination[DestinationList.Count];
                    DestinationList.CopyTo(destinations);
                    return destinations;
                }
                set
                {
                    if (value == null) return;

                    Destination[] destinations = (Destination[])value;
                    DestinationList.Clear();
                    foreach (Destination item in destinations)
                    {
                        DestinationList.Add(item);
                    }
                }
            }

            [XmlElement("ProxySettings")]
            public WinInet.ProxySettings ProxyInfo;

            //
            //  All the default values for the connection.
            //
            public RemoteAccessEntry()
            {
                SharedProfile = true;
                ConnectionType = RasConnectionType.VPN;
                Negotiate_IPv4 = true;
                Negotiate_IPv6 = true;
                VpnStrategy = RasVpnStrategy.IKEv2withSSTP;
                EncryptionType = RasEncryptionType.RequireEncryption;
                RouteIPv4TrafficOverRAS = true;
                RouteIPv6TrafficOverRAS = true;
                ShowUsernamePassword = true;
                ShowDomain = true;
                ShowDialProgressBar = true; 
                RequireCHAP = true;
                RequireMSCHAPv2 = true;
                RequireEAP = false;
                RequireEncryptedPassword = true;
                RequireMsEncryptedPassword = true;
                DontCacheRASCredentialsInCredman = false;
                ReconnectIfDropped = true;
                DestinationList = new ArrayList();
            }
        }
    }

    namespace Win32Native
    {
        internal class Kernel32Exports
        {
            [DllImport("kernel32.dll", EntryPoint = "CopyMemory")]
            public static extern void CopyMemory(IntPtr destination, IntPtr source, IntPtr length);
        }
    }

    namespace WinInet
    {
        using WinInetConstants;

        namespace WinInetConstants
        {
            [Flags]
            public enum ProxyFlags : uint
            {
                DirectProxy = 0x00000001,
                ManualProxy = 0x00000002,
                UseAutoConfigScript = 0x00000004,
                AutoProxy = 0x00000008
            }

            internal enum InternetOption : uint
            {
                InternetOptionPerConnectionOption = 75
            }

            internal enum PerConnectionOptionFlags
            {
                InternetPerConnectionOptionFlags = 1,
                InternetPerConnectionProxyServer = 2,
                InternetPerConnectionProxyByPass = 3,
                InternetPerConnectionAutoConfigURL = 4
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct InternetPerConnectionOptionList
            {
                internal uint dwSize;
                internal IntPtr pszConnection;
                internal uint dwOptionCount;
                internal uint dwOptionError;
                internal IntPtr pOptions;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct InternetPerConnectionOption
            {
                internal PerConnectionOptionFlags dwOption;
                internal InternetPerConnectionOptionValue Value;
            }

            [StructLayout(LayoutKind.Explicit)]
            internal struct InternetPerConnectionOptionValue
            {
                [FieldOffset(0)]
                internal ProxyFlags dwValue;

                [FieldOffset(0)]
                internal IntPtr pszValue;

                [FieldOffset(0)]
                internal System.Runtime.InteropServices.ComTypes.FILETIME ftValue;
            }
        }

        public class ProxySettings
        {
            public bool UseManualProxy;
            public bool UseAutoProxy;
            public bool UseAutoConfigurationScript;
            public string ManualProxyServer;
            public string ProxyOverride;
            public bool ByPassProxyForLocal;
            public string AutoConfigurationScript;

            public ProxySettings()
            {
                UseManualProxy = false;
                UseAutoProxy = false;
                UseAutoConfigurationScript = false;
                ByPassProxyForLocal = false;
            }
        }

        internal class WinInetExports
        {
            [DllImport("WinInet.dll", EntryPoint = "InternetSetOptionW", CharSet = CharSet.Unicode)]
            internal static extern uint InternetSetOption(IntPtr hInternet, InternetOption dwOption, IntPtr lpBuffer, uint dwBufferLength);
        }

        public class WinInetWrapper
        {
            public static uint InternetSetOption(string ConnectionName, bool UseAutoProxy, bool UseManualProxy, string ManualProxyServer, bool ByPassProxyForLocal, string ProxyOverride, bool UseAutoConfigScript, string AutoConfigScript)
            {
                uint returnValue = 0;
                WinInetConstants.InternetPerConnectionOption[] ConnectionOptions = null;
                WinInetConstants.InternetPerConnectionOptionList ConnectionOptionsList;

                ConnectionOptionsList.pOptions = IntPtr.Zero;
                ConnectionOptionsList.dwOptionCount = 4;
                ConnectionOptionsList.dwOptionError = 0;

                try
                {
                    ConnectionOptions = new InternetPerConnectionOption[4];

                    ConnectionOptionsList.dwSize = (uint)Marshal.SizeOf(typeof(InternetPerConnectionOptionList));
                    ConnectionOptionsList.pszConnection = Marshal.StringToHGlobalUni(string.IsNullOrEmpty(ConnectionName) ? "" : ConnectionName);

                    ConnectionOptions[0].dwOption = PerConnectionOptionFlags.InternetPerConnectionOptionFlags;

                    ConnectionOptions[0].Value.dwValue = 0;
                    if (UseAutoProxy)
                    {
                        ConnectionOptions[0].Value.dwValue |= ProxyFlags.AutoProxy;
                    }

                    if (UseManualProxy)
                    {
                        ConnectionOptions[0].Value.dwValue |= ProxyFlags.ManualProxy;
                    }

                    if (UseAutoConfigScript)
                    {
                        ConnectionOptions[0].Value.dwValue |= ProxyFlags.UseAutoConfigScript;
                    }

                    if (ConnectionOptions[0].Value.dwValue == 0)
                    {
                        ConnectionOptions[0].Value.dwValue = ProxyFlags.DirectProxy;
                    }

                    ConnectionOptions[1].dwOption = PerConnectionOptionFlags.InternetPerConnectionProxyServer;
                    ConnectionOptions[1].Value.pszValue = Marshal.StringToHGlobalUni(string.IsNullOrEmpty(ManualProxyServer) ? "" : ManualProxyServer);

                    ConnectionOptions[2].dwOption = PerConnectionOptionFlags.InternetPerConnectionProxyByPass;
                    ConnectionOptions[2].Value.pszValue = Marshal.StringToHGlobalUni(string.IsNullOrEmpty(ProxyOverride) ? "" : ProxyOverride);

                    ConnectionOptions[3].dwOption = PerConnectionOptionFlags.InternetPerConnectionAutoConfigURL;
                    ConnectionOptions[3].Value.pszValue = Marshal.StringToHGlobalUni(string.IsNullOrEmpty(AutoConfigScript) ? "" : AutoConfigScript);

                    int OptionSize = Marshal.SizeOf(typeof(InternetPerConnectionOption));
                    IntPtr OptionsPtr = Marshal.AllocCoTaskMem(OptionSize * (int)ConnectionOptionsList.dwOptionCount);
                    for (uint Index = 0; Index < ConnectionOptionsList.dwOptionCount; ++Index)
                    {
                        IntPtr OptionOffset = new IntPtr(OptionsPtr.ToInt64() + (Index * OptionSize));
                        Marshal.StructureToPtr(ConnectionOptions[Index], OptionOffset, false);
                    }

                    ConnectionOptionsList.pOptions = OptionsPtr;

                    IntPtr OptionsListPtr = Marshal.AllocCoTaskMem((int)ConnectionOptionsList.dwSize);
                    Marshal.StructureToPtr(ConnectionOptionsList, OptionsListPtr, false);

                    if (WinInetExports.InternetSetOption(new IntPtr(), InternetOption.InternetOptionPerConnectionOption, OptionsListPtr, ConnectionOptionsList.dwSize) == 0)
                    {
                        returnValue = (uint)Marshal.GetLastWin32Error();
                        if (returnValue != 0)
                        {
                            throw new Win32Exception((int)returnValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Failed to set proxy with error: {0}", ex.Message);
                }
                finally
                {
                    if (ConnectionOptions != null)
                    {
                        if (ConnectionOptions[1].Value.pszValue != null)
                        {
                            Marshal.FreeHGlobal(ConnectionOptions[1].Value.pszValue);
                        }

                        if (ConnectionOptions[2].Value.pszValue != null)
                        {
                            Marshal.FreeHGlobal(ConnectionOptions[2].Value.pszValue);
                        }

                        if (ConnectionOptions[3].Value.pszValue != null)
                        {
                            Marshal.FreeHGlobal(ConnectionOptions[3].Value.pszValue);
                        }
                    }

                    if (ConnectionOptionsList.pOptions != null)
                    {
                        Marshal.FreeCoTaskMem(ConnectionOptionsList.pOptions);
                    }
                }

                return returnValue;
            }
        }
    }

    namespace RasApi32
    {
        using Utility;
        using RemoteAccessSettings;
        using RasApiConstants;
        
        namespace RasApiConstants
        {
            //
            // RASENTRY dwfOptions bit flags
            //
            [Flags]
            public enum RASEO : uint
            {
                RASEO_None = 0,
                RASEO_UseCountryAndAreaCodes = 0x00000001,
                RASEO_SpecificIpAddr = 0x00000002,
                RASEO_SpecificNameServers = 0x00000004,
                RASEO_IpHeaderCompression = 0x00000008,
                RASEO_RemoteDefaultGateway = 0x00000010,
                RASEO_DisableLcpExtensions = 0x00000020,
                RASEO_TerminalBeforeDial = 0x00000040,
                RASEO_TerminalAfterDial = 0x00000080,
                RASEO_ModemLights = 0x00000100,
                RASEO_SwCompression = 0x00000200,
                RASEO_RequireEncryptedPw = 0x00000400,
                RASEO_RequireMsEncryptedPw = 0x00000800,
                RASEO_RequireDataEncryption = 0x00001000,
                RASEO_NetworkLogon = 0x00002000,
                RASEO_UseLogonCredentials = 0x00004000,
                RASEO_PromoteAlternates = 0x00008000,
                RASEO_SecureLocalFiles = 0x00010000,
                RASEO_RequireEAP = 0x00020000,
                RASEO_RequirePAP = 0x00040000,
                RASEO_RequireSPAP = 0x00080000,
                RASEO_Custom = 0x00100000,
                RASEO_PreviewPhoneNumber = 0x00200000,
                RASEO_SharedPhoneNumbers = 0x00800000,
                RASEO_PreviewUserPw = 0x01000000,
                RASEO_PreviewDomain = 0x02000000,
                RASEO_ShowDialingProgress = 0x04000000,
                RASEO_RequireCHAP = 0x08000000,
                RASEO_RequireMsCHAP = 0x10000000,
                RASEO_RequireMsCHAP2 = 0x20000000,
                RASEO_RequireW95MSCHAP = 0x40000000,
                RASEO_CustomScript = 0x80000000
            }

            //
            // RASENTRY dwfOptions2 bit flags
            //
            [Flags]
            public enum RASEO2 : uint
            {
                RASEO2_None = 0,
                RASEO2_SecureFileAndPrint = 0x00000001,
                RASEO2_SecureClientForMSNet = 0x00000002,
                RASEO2_DontNegotiateMultilink = 0x00000004,
                RASEO2_DontUseRasCredentials = 0x00000008,
                RASEO2_UsePreSharedKey = 0x00000010,
                RASEO2_Internet = 0x00000020,
                RASEO2_DisableNbtOverIP = 0x00000040,
                RASEO2_UseGlobalDeviceSettings = 0x00000080,
                RASEO2_ReconnectIfDropped = 0x00000100,
                RASEO2_SharePhoneNumbers = 0x00000200,
                RASEO2_SecureRoutingCompartment = 0x00000400,
                RASEO2_UseTypicalSettings = 0x00000800,
                RASEO2_IPv6SpecificNameServers = 0x00001000,
                RASEO2_IPv6RemoteDefaultGateway = 0x00002000,
                RASEO2_RegisterIpWithDNS = 0x00004000,
                RASEO2_UseDNSSuffixForRegistration = 0x00008000,
                RASEO2_IPv4ExplicitMetric = 0x00010000,
                RASEO2_IPv6ExplicitMetric = 0x00020000,
                RASEO2_DisableIKENameEkuCheck = 0x00040000,
                RASEO2_DisableClassBasedStaticRoute = 0x00080000,
                RASEO2_SpecificIPv6Addr = 0x00100000,
                RASEO2_DisableMobility = 0x00200000,
                RASEO2_RequireMachineCertificates = 0x00400000
            }

            //
            // RAS connection type
            //
            public enum RasConnectionType : uint
            {
                Dialup = 1,
                VPN = 2,
                Direct = 3,
                Internet = 4,
                Broadband = 5
            }

            //
            // VPN Strategy
            //
            public enum RasVpnStrategy : uint
            {
                PPTPOnly = 1,
                PPTPwithSSTP = 12,
                L2TPOnly = 3,
                L2TPwithSSTP = 13,
                SSTPOnly = 5,
                IKEv2Only = 7,
                IKEv2withSSTP = 14
            }

            [Flags]
            public enum RasEncryptionType : uint
            {
                NoEncryption = 0,                  // No encryption
                RequireEncryption,                 // Require Encryption
                RequireMaxEncryption,              // Require max encryption
                OptionalEncryption                 // Do encryption if possible. None Ok
            }
            
            //
            // Ras supported Network Protocols
            //
            [Flags]
            public enum RasNp : uint
            {
                None            = 0,
                RASNP_Ip        = 0x00000004,
                RASNP_Ipv6      = 0x00000008
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct MRASIPADDR
            {
                byte a;
                byte b;
                byte c;
                byte d;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct MRASIPV6ADDR
            {
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
                public string address;
            }

            internal class RasConstants
            {
                public const int MAX_PATH = 260;
                public const int RAS_MaxDeviceType = 16;
                public const int RAS_MaxPhoneNumber = 128;
                public const int RAS_MaxIpAddress = 15;
                public const int RAS_MaxEntryName = 256;
                public const int RAS_MaxDeviceName = 128;
                public const int RAS_MaxCallbackNumber = 128;
                public const int RAS_MaxAreaCode = 10;
                public const int RAS_MaxPadType = 32;
                public const int RAS_MaxX25Address = 200;
                public const int RAS_MaxFacilities = 200;
                public const int RAS_MaxUserData = 200;
                public const int RAS_MaxDnsSuffix = 256;
            }

            internal class RasEntrySize
            {
                public const int RASENTRY_VERSION_501 = 5616;
                public const int RASENTRY_VERSION_600 = 5656;
                public const int RASENTRY_VERSION_601 = 5680;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
            public struct RasEntryV501
            {
                public uint dwSize;
                public RASEO dwfOptions;

                //
                //  Location/phone number
                //
                public uint dwCountryID;
                public uint dwCountryCode;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxAreaCode + 1)]
                public string szAreaCode;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxPhoneNumber + 1)]
                public string szLocalPhoneNumber;
                public uint dwAlternateOffset;

                //
                //  PPP/Ip
                //
                public MRASIPADDR ipaddr;
                public MRASIPADDR ipaddrDns;
                public MRASIPADDR ipaddrDnsAlt;
                public MRASIPADDR ipaddrWins;
                public MRASIPADDR ipaddrWinsAlt;

                //
                //  Framing
                //
                public uint dwFrameSize;
                public RasNp dwfNetProtocols;
                public uint dwFramingProtocol;

                //
                //  Scripting
                //
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.MAX_PATH)]
                public string szScript;

                //
                //  Autodial
                //
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.MAX_PATH)]
                public string szAutodialDll;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.MAX_PATH)]
                public string szAutodialFunc;

                //
                //  Device
                //
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxDeviceType + 1)]
                public string szDeviceType;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxDeviceName + 1)]
                public string szDeviceName;

                //
                //  X.25
                //
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxPadType + 1)]
                public string szX25PadType;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxX25Address + 1)]
                public string szX25Address;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxFacilities + 1)]
                public string szX25Facilities;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxUserData + 1)]
                public string szX25UserData;
                public uint dwChannels;

                //
                //  Reserved
                //
                public uint dwReserved1;
                public uint dwReserved2;

                //
                //  Multilink
                //
                public uint dwSubEntries;
                public uint dwDialMode;
                public uint dwDialExtraPercent;
                public uint dwDialExtraSampleSeconds;
                public uint dwHangUpExtraPercent;
                public uint dwHangUpExtraSampleSeconds;

                //
                //  Idle timeout
                //
                public uint dwIdleDisconnectSeconds;

                //
                //  Entry Type
                //
                public RasConnectionType dwType;

                //
                //  Encryption type
                //
                public RasEncryptionType dwEncryptionType;

                //
                // CustomAuthKey to be used for EAP
                //
                public uint dwCustomAuthKey;

                //
                // Guid of the connection
                //
                public Guid guidId;

                //
                // Custom Dial Dll
                //
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.MAX_PATH)]
                public string szCustomDialDll;

                //
                // VPN strategy
                //
                public RasVpnStrategy dwVpnStrategy;

                //
                // More RASEO_* options
                //
                public RASEO2 dwfOptions2;

                //
                // For future use
                //
                public uint dwfOptions3;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxDnsSuffix)]
                public string szDnsSuffix;
                public uint dwTcpWindowSize;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.MAX_PATH)]
                public string szPrerequisitePbk;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RasConstants.RAS_MaxEntryName + 1)]
                public string szPrerequisiteEntry;
                public uint dwRedialCount;
                public uint dwRedialPause;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
            public struct RasEntryV600
            {
                public RasEntryV501 entryV501;

                //
                // PPP/IPv6
                //
                public MRASIPV6ADDR ipv6addrDns;
                public MRASIPV6ADDR ipv6addrDnsAlt;

                //
                //  Interface metric
                //
                public uint dwIPv4InterfaceMetric;
                public uint dwIPv6InterfaceMetric;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
            public struct RasEntryV601
            {
                public RasEntryV600 entryV600;

                //
                // PPP/IPv6
                //
                public MRASIPV6ADDR ipv6addrDns;
                public MRASIPV6ADDR ipv6addrDnsAlt;

                //
                //  Interface metric
                //
                public uint dwIPv4InterfaceMetric;
                public uint dwIPv6InterfaceMetric;

                //
                // Fields required for supporting static IPv6 address 
                // configuration for a vpn interface by the user 
                //
                public MRASIPV6ADDR ipv6addr;
                public uint dwIPv6PrefixLength;

                //
                // IKEv2 network outage time
                //
                public uint dwNetworkOutageTime;
            }
        }

        internal class RasApi32Exports
        {
            [DllImport("RasApi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RasSetEntryPropertiesW")]
            extern internal static uint RasSetEntryProperties(string phonebookPath, string entryName, IntPtr rasEntry, uint size, IntPtr deviceInfo, uint deviceInfoSize);
        }

        internal class RasApi32Wrapper
        {
            internal static string GetAlternateList(Destination[] destinations, out uint alternateListLength)
            {
                StringBuilder stringBuilder = new StringBuilder();

                alternateListLength = 0;
                if (destinations != null && destinations.Length > 0)
                {
                    //
                    //  Add each of the destination list into string builder
                    //  in a multi string format.
                    //
                    foreach (Destination item in destinations)
                    {
                        if (!string.IsNullOrEmpty(item.DestinationAddress))
                        {
                            stringBuilder.Append(item.DestinationAddress).Append('\x00');
                        }

                    }

                    //
                    //  Add the additional '\0' character
                    //  at the end of the buffer to double
                    //  null terminate the string.
                    //
                    stringBuilder.Append('\x00');

                    //
                    //  Multiply by 2 which is sizeof(WCHAR)
                    //
                    alternateListLength = (uint)stringBuilder.Length * 2;
                }

                return stringBuilder.ToString();
            }

            internal static IntPtr ConstructRASEntry(OSPlatform.OSPlatformType platform, RemoteAccessEntry entry, out uint size)
            {
                uint offset = 0;
                uint versionSize = 0;
                IntPtr entryPtr = IntPtr.Zero;
                RasEntryV601 entryV601;
                
                entryV601 = new RasEntryV601();
                entryV601.entryV600 = new RasEntryV600();
                entryV601.entryV600.entryV501 = new RasEntryV501();

                size = 0;
                switch (platform)
                {
                    case OSPlatform.OSPlatformType.WindowsXP:
                        {
                            versionSize = RasEntrySize.RASENTRY_VERSION_501;
                            break;
                        }

                    case OSPlatform.OSPlatformType.WindowsVista:
                        {
                            versionSize = RasEntrySize.RASENTRY_VERSION_600;
                            break;
                        }

                    case OSPlatform.OSPlatformType.Windows7:
                        {
                            versionSize = RasEntrySize.RASENTRY_VERSION_601;
                            break;
                        }

                    default:
                        {
                            versionSize = 0;
                            break;
                        }
                }

                //
                //  Initially set all the fields in MRasEntry
                //  structure as read from the XML file.
                //
                entryV601.entryV600.entryV501.dwSize = versionSize;
                entryV601.entryV600.entryV501.szLocalPhoneNumber = entry.DefaultDestination;
                entryV601.entryV600.entryV501.dwType = entry.ConnectionType;
                entryV601.entryV600.entryV501.dwVpnStrategy = entry.VpnStrategy;
                entryV601.entryV600.entryV501.dwEncryptionType = entry.EncryptionType;

                if (entry.ConnectionType == RasConnectionType.VPN)
                {
                    entryV601.entryV600.entryV501.szDeviceType = "vpn";
                }

                if (entry.Negotiate_IPv4)
                {
                    entryV601.entryV600.entryV501.dwfNetProtocols |= RasNp.RASNP_Ip;
                }

                if (entry.Negotiate_IPv6)
                {
                    entryV601.entryV600.entryV501.dwfNetProtocols |= RasNp.RASNP_Ipv6;
                }

                if (entry.RouteIPv4TrafficOverRAS)
                {
                    entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_RemoteDefaultGateway;
                }

                if (entry.ShowUsernamePassword)
                {
                    entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_PreviewUserPw;
                }

                if (entry.ShowDomain)
                {
                    entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_PreviewDomain;
                }

                if (entry.ShowDialProgressBar)
                {
                    entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_ShowDialingProgress;
                }

                entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_RequireDataEncryption;

                if (entry.RequireCHAP)
                {
                    entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_RequireCHAP | RASEO.RASEO_RequireMsEncryptedPw;
                }

                if (entry.RequireMSCHAPv2)
                {
                    entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_RequireMsCHAP2 | RASEO.RASEO_RequireMsEncryptedPw;
                }

                if (entry.RequireEAP)
                {
                    entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_RequireEAP;
                }

                if (entry.RouteIPv6TrafficOverRAS)
                {
                    entryV601.entryV600.entryV501.dwfOptions2 |= RASEO2.RASEO2_IPv6RemoteDefaultGateway;
                }

                if (entry.DontCacheRASCredentialsInCredman)
                {
                    entryV601.entryV600.entryV501.dwfOptions2 |= RASEO2.RASEO2_DontUseRasCredentials;
                }

                if (entry.ReconnectIfDropped)
                {
                    entryV601.entryV600.entryV501.dwfOptions2 |= RASEO2.RASEO2_ReconnectIfDropped;
                }

                string alternateList = GetAlternateList(entry.Destinations, out offset);
                try
                {
                    //
                    //  Allocate enough to hold the RASENTRY + alternate numbers
                    //
                    if( offset != 0 )
                    {
                        entryV601.entryV600.entryV501.dwfOptions |= RASEO.RASEO_PreviewPhoneNumber;
                    }

                    entryV601.entryV600.entryV501.dwAlternateOffset = offset;
                    entryPtr = Marshal.AllocHGlobal((Int32)(versionSize + offset));

                    switch (platform)
                    {
                        case OSPlatform.OSPlatformType.WindowsXP:
                            {
                                Marshal.StructureToPtr(entryV601.entryV600.entryV501, entryPtr, false);
                                break;
                            }

                        case OSPlatform.OSPlatformType.WindowsVista:
                            {
                                Marshal.StructureToPtr(entryV601.entryV600, entryPtr, false);
                                break;
                            }

                        case OSPlatform.OSPlatformType.Windows7:
                            {
                                Marshal.StructureToPtr(entryV601, entryPtr, false);
                                break;
                            }

                        default:
                            {
                                versionSize = 0;
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Failed to marshal the managed structure to a native buffer with error: {0}", ex.Message);
                    throw ex;
                }

                IntPtr source = IntPtr.Zero;
                IntPtr destination = IntPtr.Zero;
                try
                {
                    //
                    //  Now copy the fields to past the RASENTRY.
                    //
                    if (offset > 0)
                    {
                        destination = new IntPtr(entryPtr.ToInt64() + offset);
                        source = Marshal.StringToHGlobalUni(alternateList);
                        if (source != IntPtr.Zero && destination != IntPtr.Zero)
                        {
                            Win32Native.Kernel32Exports.CopyMemory(destination, source, new IntPtr(offset));
                        }

                        offset += versionSize;
                        size = offset;
                    }

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Failed to copy the alternate list past the RASENTRY structure with error: {0}", ex.Message);
                    throw ex;
                }
                finally
                {
                    if (source != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(source);
                    }
                }

                return entryPtr;
            }

            internal static void RasSetEntryProperties(OSPlatform.OSPlatformType platform, RemoteAccessEntry entry)
            {
                uint returnValue = 0;
                uint size = 0;
                IntPtr entryPtr = IntPtr.Zero;

                try
                {
                    entryPtr = ConstructRASEntry(platform, entry, out size);

                    string phonebookPath;

                    if(entry.SharedProfile)
                    {
                        if( platform < OSPlatform.OSPlatformType.WindowsVista )
                        {
                            phonebookPath = Environment.ExpandEnvironmentVariables("%allusersprofile%\\Application Data\\Microsoft\\Network\\Connections\\Pbk\\rasphone.pbk");
                        }
                        else
                        {
                            phonebookPath = Environment.ExpandEnvironmentVariables("%programdata%\\Microsoft\\Network\\Connections\\Pbk\\rasphone.pbk");
                        }
                    }
                    else
                    {
                        phonebookPath = Environment.ExpandEnvironmentVariables("%appdata%\\Microsoft\\Network\\Connections\\Pbk\\rasphone.pbk");
                    }

                    returnValue = RasApi32Exports.RasSetEntryProperties(phonebookPath, entry.Name, entryPtr, size, IntPtr.Zero, 0);
                    if (returnValue != 0)
                    {
                        System.Console.WriteLine("Failed to create RAS connection at: {0}-{1}", phonebookPath, entry.Name);
                    }
                }
                catch (Exception ex)
                {
                    if (entryPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(entryPtr);
                    }

                    System.Console.WriteLine("Failed to create the RAS connection entry with error: {0}", ex.Message);
                    throw ex;
                }
            }
        }
    }

    namespace Utility
    {
        internal class OSPlatform
        {
            internal enum OSPlatformType
            {
                Unsupported,
                WindowsXP,
                WindowsVista,
                Windows7
            }

            internal static OSPlatformType GetPlatform()
            {
                Version version = Environment.OSVersion.Version;
                OSPlatformType platformType = OSPlatformType.Unsupported;

                switch (version.Major)
                {
                    case 5:
                        {
                            if (version.Minor == 1)
                            {
                                platformType = OSPlatformType.WindowsXP;
                            }
                            break;
                        }

                    case 6:
                        {
                            if (version.Minor == 0)
                            {
                                platformType = OSPlatformType.WindowsVista;
                            }
                            else if (version.Minor == 1)
                            {
                                platformType = OSPlatformType.Windows7;
                            }
                            break;
                        }
                }

                return platformType;
            }
        }
    }

    namespace RemoteAccessInstaller
    {
        using Win32Native;
        using RasApi32.RasApiConstants;
        using WinInet.WinInetConstants;
        using RemoteAccessSettings;

        public class Installer
        {
            public static void CreateEntries(string xmlFilePath)
            {
                RemoteAccessEntries entries = null;

                try
                {
                    //
                    //  Check if input file path is valid.
                    //
                    if (!string.IsNullOrEmpty(xmlFilePath))
                    {
                        //
                        //  Load the XML file into memory.
                        //
                        entries = RemoteAccessEntries.LoadXML(xmlFilePath);
                    }
                    else
                    {
                        System.Console.WriteLine("The input file path containing Remote Access settings is null.");
                    }

                    foreach (RemoteAccessEntry entry in entries.Entries)
                    {
                        WinInet.ProxySettings proxy = entry.ProxyInfo;
                        RasApi32.RasApi32Wrapper.RasSetEntryProperties(Utility.OSPlatform.GetPlatform(), entry);

                        if (proxy != null)
                        {
                            WinInet.WinInetWrapper.InternetSetOption(entry.Name, proxy.UseAutoProxy, proxy.UseManualProxy, proxy.ManualProxyServer, proxy.ByPassProxyForLocal, proxy.ProxyOverride, proxy.UseAutoConfigurationScript, proxy.AutoConfigurationScript);
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Console.WriteLine("CreateEntries failed with exception: {0}", ex.Message);
                }
            }
        }
    }
'@

$length = $args.length

if( $length -lt 1 )
{
    write-host "Error: You must specify the path to the connection file."
    write-host "Syntax: create-ras-connection <Settings XML file>"
    exit
}

Compile-CSharp $code
[RemoteAccessInstaller.Installer]::CreateEntries($args[0]);