/*
 *
 * BitAnswer Client Library class
 *
 * BitAnswer Ltd. (C) 2009 - ?. All rights reserved.
 */
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Com.BitAnswer.Library
{
    public class BitAnswerException : Exception
    {
        public int ErrorCode { get; set; }

        public BitAnswerException(int status)
        {
            ErrorCode = status;
        }

        public override string Message
        {
            get
            {
                return "ErrorCode: " + ErrorCode.ToString();
            }
        }
    }

    public enum BitAnswerExceptionCode
    {
        BIT_SUCCESS = 0,
        BIT_ERR_BUFFER_SMALL = 0x104
    }

    public enum LoginMode
    {
        Local = 1,
        Remote = 2,
        Auto = 3,
        AutoCache = 7,
        Usb = 8,
        Process = 16
    }

    public enum BindingType
    {
        Existing = 0,
        Local = 1,
        UsbStorage = 2
    }

    public enum SessionType
    {
        XML_TYPE_SN_INFO        = 3,
        XML_TYPE_FEATURE_INFO   = 4,

        BIT_ADDRESS             = 0x101,
        BIT_SYS_TIME            = 0x201,
        BIT_CONTROL_TYPE        = 0x302,
        BIT_VOL_NUM             = 0x303,
        BIT_START_DATE          = 0x304,
        BIT_END_DATE            = 0x305,
        BIT_EXPIRATION_DAYS     = 0x306,
        BIT_USAGE_NUM           = 0x307,
        BIT_CONSUMED_USAGE_NUM  = 0x308,
        BIT_CONCURRENT_NUM      = 0x309,
        BIT_ACTIVATE_DATE       = 0x30A,
        BIT_USER_LIMIT          = 0x30B,
        BIT_LAST_REMOTE_ACCESS_DATE = 0x30C,
        BIT_MAX_OFFLINE_MINUTES = 0x30D,
        BIT_NEXT_CONNECT_DATE   = 0x30E,
        BIT_SERVER_HB_STATUS    = 0x1003
    }

    public enum InfoType
    {
        BIT_LIST_SRV_ADDR = 0,
        BIT_LIST_LOCAL_SN_INFO = 1,
        BIT_LIST_LOCAL_SN_FEATURE_INFO = 2,
        BIT_LIST_LOCAL_SN_LIC_INFO = 3,
        BIT_LIST_UPDATE_ERROR = 4,
        BIT_INFO_CONFIG = 5,
        BIT_INFO_TOKEN_LIST = 7
    }

    public struct BIT_DATE_TIME
    {
        public UInt16 year;
        public Byte month;
        public Byte dayOfMonth;
        public Byte hour;
        public Byte minute;
        public Byte second;
        public Byte unused;
    } 

    public enum FEATURE_TYPE
    {
       BIT_FEATURE_CONVERT         = 0x03,
       BIT_FEATURE_READ_ONLY       = 0x04,
       BIT_FEATURE_READ_WRITE      = 0x05,
       BIT_FEATURE_CRYPTION        = 0x09,
       BIT_FEATURE_USER            = 0x0a,
       BIT_FEATURE_UNIFIED         = 0x0b,
    }

    public unsafe struct FEATURE_INFO
    {
       public UInt32         featureId;
       public FEATURE_TYPE   type;
       [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
       public Byte[]         featureName;
       public BIT_DATE_TIME  endDateTime;
       public UInt32         expirationDays;
       public UInt32         users;
    }

    public interface BitAnswerInterface
    {
        int Login(string url, string sn, int mode);
        int LoginEx(string url, string sn, uint featureId, string xmlScope, int mode);
        int Logout();
        int ConvertFeature(uint featureId, uint para1, uint para2, uint para3, uint para4, ref uint result);
        int ReadFeature(uint featureId, ref uint featureValue);
        int WriteFeature(uint featureId, uint featureValue);
        int EncryptFeature(uint featureId, byte[] plainBuffer, byte[] cipherBuffer);
        int DecryptFeature(uint featureId, byte[] cipherBuffer, byte[] plainBuffer);
        int QueryFeature(uint featureId, ref uint capacity);
        int ReleaseFeature(uint featureId, ref uint capacity);
        int GetFeatureInfo(uint featureId, ref FEATURE_INFO featureInfo);
        int GetDataItemNum(ref uint number);
        int GetDataItemName(uint index, byte[] name, ref uint nameLen);
        int GetDataItem(string dataItemName, byte[] dataItemValue, ref uint dataItemValueLen);
        int SetDataItem(string dataItemName, byte[] dataItemValue);
        int RemoveDataItem(string dataItemName);
        int GetSessionInfo(SessionType type, byte[] sessionInfo, ref uint sessionInfoLen);
        int UpdateOnline(string url, string sn);
        int GetRequestInfo(string sn, uint bindingType, byte[] requestInfo, ref uint requestInfoSize);
        int GetUpdateInfo(string url, string sn, string requestInfo, byte[] updateInfo, ref uint updateInfoSize);
        int ApplyUpdateInfo(string updateInfo, byte[] receiptInfo, ref uint receiptInfoSize);
        int SetRootPath(string path);
        int GetProductPath(byte[] productPath, uint productPathSize);
        int Revoke(string url, string sn, byte[] revocationInfo, ref uint revocationInfoSize);
        int GetInfo(string sn, uint type, byte[] info, ref uint infoSize);
        int SetProxy(string hostName, uint port, string userId, string password);
        int SetLocalServer(string host, uint port, uint timeoutSeconds);
        int GetVersion(ref uint version);
        int RemoveSn(string sn);
        int CheckOutSn(string url, uint featureId, uint durationDays);
        int CheckOutFeatures(string url, uint[] featureIds, uint durationDays);
        int CheckIn(string url, uint featureId);
        int TestBitService(string url, string sn, uint featureId);
        int SetCustomInfo(uint infoId, byte[] infoData);
        int SetAppVersion(uint version);
    }

    public class BitAnswerX86 : BitAnswerInterface
    {
        public const string BitAnswerDllName = "00003733_000000BA";

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_Login")]
        static extern int Bit_Login(string url, string sn, byte[] applicationData, ref uint handle, int mode);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_LoginEx")]
        static extern int Bit_LoginEx(string url, string sn, uint featureId, string xmlScope, byte[] applicationData, ref uint handle, int mode);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_Logout")]
        static extern int Bit_Logout(uint handle);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_ConvertFeature")]
        static extern int Bit_ConvertFeature(uint handle, uint featureId, uint para1, uint para2, uint para3, uint para4, ref uint result);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_ReadFeature")]
        static extern int Bit_ReadFeature(uint handle, uint featureId, ref uint featureValue);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_WriteFeature")]
        static extern int Bit_WriteFeature(uint handle, uint featureId, uint featureValue);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_EncryptFeature")]
        static extern int Bit_EncryptFeature(uint handle, uint featureId, byte[] plainBuffer, byte[] cipherBuffer, uint bufferLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_DecryptFeature")]
        static extern int Bit_DecryptFeature(uint handle, uint featureId, byte[] cipherBuffer, byte[] plainBuffer, uint bufferLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_QueryFeature")]
        static extern int Bit_QueryFeature(uint handle, uint featureId, ref uint capacity);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_ReleaseFeature")]
        static extern int Bit_ReleaseFeature(uint handle, uint featureId, ref uint capacity);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetFeatureInfo")]
        static extern int Bit_GetFeatureInfo(uint handle, uint featureId, ref FEATURE_INFO featureInfo);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetDataItemNum")]
        static extern int Bit_GetDataItemNum(uint handle, ref uint number);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetDataItemName")]
        static extern int Bit_GetDataItemName(uint handle, uint index, byte[] dataItemName, ref uint DataItemNameLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetDataItem")]
        static extern int Bit_GetDataItem(uint handle, string dataItemName, byte[] dataItemValue, ref uint dataItemValueLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetDataItem")]
        static extern int Bit_SetDataItem(uint handle, string dataItemName, byte[] dataItemValue, uint dataItemValueLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_RemoveDataItem")]
        static extern int Bit_RemoveDataItem(uint handle, string dataItemName);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetSessionInfo")]
        static extern int Bit_GetSessionInfo(uint handle, uint type, byte[] sessionInfo, ref uint sessionInfoLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetRequestInfo")]
        static extern int Bit_GetRequestInfo(string sn, byte[] applicationData, uint bindingType, byte[] requestInfo, ref uint requestInfoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetUpdateInfo")]
        static extern int Bit_GetUpdateInfo(string url, string sn, byte[] applicationData, string requestInfo, byte[] updateInfo, ref uint updateInfoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_ApplyUpdateInfo")]
        static extern int Bit_ApplyUpdateInfo(byte[] applicationData, string updateInfo, byte[] receiptInfo, ref uint receiptInfoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_UpdateOnline")]
        static extern int Bit_UpdateOnline(string url, string sn, byte[] applicationData);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetRootPath")]
        static extern int Bit_SetRootPath(string szPath);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetProductPath")]
        static extern int Bit_GetProductPath(byte[] applicationData, byte[] productPath, uint productPathSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_Revoke")]
        static extern int Bit_Revoke(string url, string sn, byte[] applicationData, byte[] revocationInfo, ref uint revocationInfoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetInfo")]
        static extern int Bit_GetInfo(string sn, byte[] applicationData, uint type, byte[] info, ref uint infoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetProxy")]
        static extern int Bit_SetProxy(byte[] applicationData, string host, uint port, string userId, string password);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetLocalServer")]
        static extern int Bit_SetLocalServer(byte[] applicationData, string host, uint port, uint timeoutSeconds);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_RemoveSn")]
        static extern int Bit_RemoveSn(string sn, byte[] applicationData);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetVersion")]
        static extern int Bit_GetVersion(ref uint version);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_CheckOutSn")]
        static extern int Bit_CheckOutSn(string url, uint featureId, byte[] applicationData, uint durationDays);
        
        [DllImport(BitAnswerDllName, EntryPoint = "Bit_CheckOutFeatures")]
        static extern int Bit_CheckOutFeatures(string url, byte[] applicationData, uint[] features, uint featuresSize, uint durationDays);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_CheckIn")]
        static extern int Bit_CheckIn(string url, uint featureId, byte[] applicationData);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_TestBitService")]
        static extern int Bit_TestBitService(string url, string sn, uint featureId, byte[] applicationData);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetCustomInfo")]
        static extern int Bit_SetCustomInfo(uint infoId, byte[] infoData, uint infoDataSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetAppVersion")]
        static extern int Bit_SetAppVersion(uint version);

        static byte[] applicationData = {
	        0x40,0x80,0xbc,0x91,0xa4,0x02,0x03,0x40,0x3f,0xa8,0x85,0x81,0xc4,0xb2,0xa6,0x60,
            0xea,0xac,0x84,0xf3,0x95,0xeb,0xcb,0xf4,0xeb,0x9d,0xdd,0x45,0xbd,0xcb,0x8a,0x93,
            0x25,0x3e,0x2d,0x40,0x1e,0x74,0x46,0x06,0x86,0xf9,0x6f,0xef,0xcb,0x11,0x8e,0x30,
            0x45,0xa7,0x8d,0x23,0xa6,0x7c,0xa6,0x3a,0x3b,0x9f,0x3d,0xbd,0x93,0x00,0x16,0x4d,
            0x84,0xd7,0xcc,0xfd,0xa4,0x9e,0x5d,0xad,0x66,0xe8,0x6d,0xa5,0xe4,0x06,0x0f,0x44,
            0xc0,0x8a,0xb4,0x87,0x49,0xd8,0x0b,0x0f,0x11,0x77,0xc3,0x80,0x72,0x2f,0xcc,0x88,
            0xb2,0xe5,0xc1,0x8f,0x0b,0x6c,0xc4,0xeb,0xba,0xa4,0xb6,0x05,0xad,0x46,0xb9,0xa5,
            0xcc,0xdf,0x92,0xd2,0xbc,0xb7,0x8e,0x46,0xea,0xaa,0xf4,0x68,0xc6,0x80,0x4e,0x8c,
            0x13,0x5f,0xac,0xb2,0xf4,0xba,0x57,0xae,0x37,0x4a,0x99,0x23,0x95,0x30,0x03,0xeb,
            0xb0,0x54,0xef,0x88,0x82,0x71,0x89,0x5a,0x4c,0x9d,0xd4,0xb8,0xdd,0x05,0xf3,0x3d,
            0xd3,0x9b,0xcd,0xb2,0xfa,0xa4,0x38,0xa4,0xc4,0xfc,0x6a,0xc4,0x44,0x0f,0x26,0x1b,
            0xc6,0xa6,0xde,0x81,0xf7,0x55,0x3e,0x5a,0xb3,0x81,0x29,0xea,0x4d,0x25,0x1e
        };

        uint handle = 0;

        public int Login(string url, string sn, int mode)
        {
            return Bit_Login(url, sn, applicationData, ref handle, (int)mode);
        }

        public int LoginEx(string url, string sn, uint featureId, string xmlScope, int mode)
        {
            return Bit_LoginEx(url, sn, featureId, xmlScope, applicationData, ref handle, mode);
        }

        public int Logout()
        {
            return Bit_Logout(handle);
        }

        public int ConvertFeature(uint featureId, uint para1, uint para2, uint para3, uint para4, ref uint result)
        {
            return Bit_ConvertFeature(handle, featureId, para1, para2, para3, para4, ref result);
        }

        public int ReadFeature(uint featureId, ref uint featureValue)
        {
            return Bit_ReadFeature(handle, featureId, ref featureValue);
        }

        public int WriteFeature(uint featureId, uint featureValue)
        {
            return Bit_WriteFeature(handle, featureId, featureValue);
        }

        public int EncryptFeature(uint featureId, byte[] plainBuffer, byte[] cipherBuffer)
        {
            return Bit_EncryptFeature(handle, featureId, plainBuffer, cipherBuffer, (uint)plainBuffer.Length);
        }

        public int DecryptFeature(uint featureId, byte[] cipherBuffer, byte[] plainBuffer)
        {
            return Bit_DecryptFeature(handle, featureId, cipherBuffer, plainBuffer, (uint)cipherBuffer.Length);
        }

        public int QueryFeature(uint featureId, ref uint capacity)
        {
            return Bit_QueryFeature(handle, featureId, ref capacity);
        }

        public int ReleaseFeature(uint featureId, ref uint capacity)
        {
            return Bit_ReleaseFeature(handle, featureId, ref capacity);
        }

        public int GetFeatureInfo(uint featureId, ref FEATURE_INFO featureInfo)
        {
            return Bit_GetFeatureInfo(handle, featureId, ref featureInfo);
        }

        public int GetDataItemNum(ref uint number)
        {
            return Bit_GetDataItemNum(handle, ref number);
        }

        public int GetDataItemName(uint index, byte[] name, ref uint nameLen)
        {
            return Bit_GetDataItemName(handle, index, name, ref nameLen);
        }

        public int GetDataItem(string dataItemName, byte[] dataItemValue, ref uint dataItemValueLen)
        {
            return Bit_GetDataItem(handle, dataItemName, dataItemValue, ref dataItemValueLen);
        }

        public int SetDataItem(string dataItemName, byte[] dataItemValue)
        {
            return Bit_SetDataItem(handle, dataItemName, dataItemValue, (uint)dataItemValue.Length);
        }

        public int RemoveDataItem(string dataItemName)
        {
            return Bit_RemoveDataItem(handle, dataItemName);
        }

        public int GetSessionInfo(SessionType type, byte[] sessionInfo, ref uint sessionInfoLen)
        {
            return Bit_GetSessionInfo(handle, (uint)type, sessionInfo, ref sessionInfoLen);
        }

        public int UpdateOnline(string url, string sn)
        {
            return Bit_UpdateOnline(url, sn, applicationData);
        }

        public int GetRequestInfo(string sn, uint bindingType, byte[] requestInfo, ref uint requestInfoSize)
        {
            return Bit_GetRequestInfo(sn, applicationData, bindingType, requestInfo, ref requestInfoSize);
        }

        public int GetUpdateInfo(string url, string sn, string requestInfo, byte[] updateInfo, ref uint updateInfoSize)
        {
            return Bit_GetUpdateInfo(url, sn, applicationData, requestInfo, updateInfo, ref updateInfoSize);
        }

        public int ApplyUpdateInfo(string updateInfo, byte[] receiptInfo, ref uint receiptInfoSize)
        {
            return Bit_ApplyUpdateInfo(applicationData, updateInfo, receiptInfo, ref receiptInfoSize);
        }

        public int SetRootPath(string path)
        {
            return Bit_SetRootPath(path);
        }

        public int GetProductPath(byte[] productPath, uint productPathSize)
        {
            return Bit_GetProductPath(applicationData, productPath, productPathSize);
        }

        public int Revoke(string url, string sn, byte[] revocationInfo, ref uint revocationInfoSize)
        {
            return Bit_Revoke(url, sn, applicationData, revocationInfo, ref revocationInfoSize);
        }
        public int GetInfo(string sn, uint type, byte[] info, ref uint infoSize)
        {
            return Bit_GetInfo(sn, applicationData, type, info, ref infoSize);
        }

        public int SetProxy(string hostName, uint port, string userId, string password)
        {
            return Bit_SetProxy(applicationData, hostName, port, userId, password);
        }

        public int SetLocalServer(string host, uint port, uint timeoutSeconds)
        {
            return Bit_SetLocalServer(applicationData, host, port, timeoutSeconds);
        }

        public int RemoveSn(string sn)
        {
            return Bit_RemoveSn(sn, applicationData);
        }

        public int GetVersion(ref uint version)
        {
            return Bit_GetVersion(ref version);
        }

        public int CheckOutSn(string url, uint featureId, uint durationDays)
        {
            return Bit_CheckOutSn(url, featureId, applicationData, durationDays);
        }

        public int CheckOutFeatures(string url, uint[] featureIds, uint durationDays)
        {
            uint size = 0;
            if (featureIds != null)
            {
                size = (uint)featureIds.Length;
            }
            return Bit_CheckOutFeatures(url, applicationData, featureIds, size, durationDays);
        }

        public int CheckIn(string url, uint featureId)
        {
            return Bit_CheckIn(url, featureId, applicationData);
        }

        public int TestBitService(string url, string sn, uint featureId)
        {
            return Bit_TestBitService(url, sn, featureId, applicationData);
        }

        public int SetCustomInfo(uint infoId, byte[] infoData)
        {
            return Bit_SetCustomInfo(infoId, infoData, (uint)infoData.Length);
        }

        public int SetAppVersion(uint version)
        {
            return Bit_SetAppVersion(version);
        }
    }

    public class BitAnswerX64 : BitAnswerInterface
    {
        public const string BitAnswerDllName = "00003733_000000BA_x64";

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_Login")]
        public static extern int Bit_Login(string url, string sn, byte[] applicationData, ref UInt64 handle, int mode);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_LoginEx")]
        static extern int Bit_LoginEx(string url, string sn, uint featureId, string xmlScope, byte[] applicationData, ref UInt64 handle, int mode);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_Logout")]
        static extern int Bit_Logout(UInt64 handle);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_ConvertFeature")]
        static extern int Bit_ConvertFeature(UInt64 handle, uint featureId, uint para1, uint para2, uint para3, uint para4, ref uint result);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_ReadFeature")]
        static extern int Bit_ReadFeature(UInt64 handle, uint featureId, ref uint featureValue);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_WriteFeature")]
        static extern int Bit_WriteFeature(UInt64 handle, uint featureId, uint featureValue);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_EncryptFeature")]
        static extern int Bit_EncryptFeature(UInt64 handle, uint featureId, byte[] plainBuffer, byte[] cipherBuffer, uint bufferLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_DecryptFeature")]
        static extern int Bit_DecryptFeature(UInt64 handle, uint featureId, byte[] plainBuffer, byte[] cipherBuffer, uint bufferLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_QueryFeature")]
        static extern int Bit_QueryFeature(UInt64 handle, uint featureId, ref uint capacity);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_ReleaseFeature")]
        static extern int Bit_ReleaseFeature(UInt64 handle, uint featureId, ref uint capacity);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetFeatureInfo")]
        static extern int Bit_GetFeatureInfo(UInt64 handle, uint featureId, ref FEATURE_INFO featureInfo);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetDataItemNum")]
        static extern int Bit_GetDataItemNum(UInt64 handle, ref uint number);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetDataItemName")]
        static extern int Bit_GetDataItemName(UInt64 handle, uint index, byte[] dataItemName, ref uint DataItemNameLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetDataItem")]
        static extern int Bit_GetDataItem(UInt64 handle, string dataItemName, byte[] dataItemValue, ref uint dataItemValueLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetDataItem")]
        static extern int Bit_SetDataItem(UInt64 handle, string dataItemName, byte[] dataItemValue, uint dataItemValueLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_RemoveDataItem")]
        static extern int Bit_RemoveDataItem(UInt64 handle, string dataItemName);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetSessionInfo")]
        static extern int Bit_GetSessionInfo(UInt64 handle, uint type, byte[] sessionInfo, ref uint sessionInfoLen);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetRequestInfo")]
        static extern int Bit_GetRequestInfo(string sn, byte[] applicationData, uint bindingType, byte[] requestInfo, ref uint requestInfoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetUpdateInfo")]
        static extern int Bit_GetUpdateInfo(string url, string sn, byte[] applicationData, string requestInfo, byte[] updateInfo, ref uint updateInfoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_ApplyUpdateInfo")]
        static extern int Bit_ApplyUpdateInfo(byte[] applicationData, string updateInfo, byte[] receiptInfo, ref uint receiptInfoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_UpdateOnline")]
        static extern int Bit_UpdateOnline(string url, string sn, byte[] applicationData);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetRootPath")]
        static extern int Bit_SetRootPath(string szPath);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetProductPath")]
        static extern int Bit_GetProductPath(byte[] applicationData, byte[] productPath, uint productPathSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_Revoke")]
        static extern int Bit_Revoke(string url, string sn, byte[] applicationData, byte[] revocationInfo, ref uint revocationInfoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetInfo")]
        static extern int Bit_GetInfo(string sn, byte[] applicationData, uint type, byte[] info, ref uint infoSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetProxy")]
        static extern int Bit_SetProxy(byte[] applicationData, string hostName, uint port, string userId, string password);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetLocalServer")]
        static extern int Bit_SetLocalServer(byte[] applicationData, string host, uint port, uint timeoutSeconds);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_RemoveSn")]
        static extern int Bit_RemoveSn(string sn, byte[] applicationData);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_GetVersion")]
        static extern int Bit_GetVersion(ref uint version);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_CheckOutSn")]
        static extern int Bit_CheckOutSn(string url, uint featureId, byte[] applicationData, uint durationDays);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_CheckOutFeatures")]
        static extern int Bit_CheckOutFeatures(string url, byte[] applicationData, uint[] features, uint featuresSize, uint durationDays);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_CheckIn")]
        static extern int Bit_CheckIn(string url, uint featureId, byte[] applicationData);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_TestBitService")]
        static extern int Bit_TestBitService(string url, string sn, uint featureId, byte[] applicationData);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetCustomInfo")]
        static extern int Bit_SetCustomInfo(uint infoId, byte[] infoData, uint infoDataSize);

        [DllImport(BitAnswerDllName, EntryPoint = "Bit_SetAppVersion")]
        static extern int Bit_SetAppVersion(uint version);

        static byte[] applicationData = {
	        0x40,0x80,0xbc,0x91,0xa4,0x02,0x03,0x40,0x3f,0xa8,0x85,0x81,0xc4,0xb2,0xa6,0x60,
            0xea,0xac,0x84,0xf3,0x95,0xeb,0xcb,0xf4,0xeb,0x9d,0xdd,0x45,0xbd,0xcb,0x8a,0x93,
            0x25,0x3e,0x2d,0x40,0x1e,0x74,0x46,0x06,0x86,0xf9,0x6f,0xef,0xcb,0x11,0x8e,0x30,
            0x45,0xa7,0x8d,0x23,0xa6,0x7c,0xa6,0x3a,0x3b,0x9f,0x3d,0xbd,0x93,0x00,0x16,0x4d,
            0x84,0xd7,0xcc,0xfd,0xa4,0x9e,0x5d,0xad,0x66,0xe8,0x6d,0xa5,0xe4,0x06,0x0f,0x44,
            0xc0,0x8a,0xb4,0x87,0x49,0xd8,0x0b,0x0f,0x11,0x77,0xc3,0x80,0x72,0x2f,0xcc,0x88,
            0xb2,0xe5,0xc1,0x8f,0x0b,0x6c,0xc4,0xeb,0xba,0xa4,0xb6,0x05,0xad,0x46,0xb9,0xa5,
            0xcc,0xdf,0x92,0xd2,0xbc,0xb7,0x8e,0x46,0xea,0xaa,0xf4,0x68,0xc6,0x80,0x4e,0x8c,
            0x13,0x5f,0xac,0xb2,0xf4,0xba,0x57,0xae,0x37,0x4a,0x99,0x23,0x95,0x30,0x03,0xeb,
            0xb0,0x54,0xef,0x88,0x82,0x71,0x89,0x5a,0x4c,0x9d,0xd4,0xb8,0xdd,0x05,0xf3,0x3d,
            0xd3,0x9b,0xcd,0xb2,0xfa,0xa4,0x38,0xa4,0xc4,0xfc,0x6a,0xc4,0x44,0x0f,0x26,0x1b,
            0xc6,0xa6,0xde,0x81,0xf7,0x55,0x3e,0x5a,0xb3,0x81,0x29,0xea,0x4d,0x25,0x1e
        };
        UInt64 handle = 0;

        public int Login(string url, string sn, int mode)
        {
            return Bit_Login(url, sn, applicationData, ref handle, (int)mode);
        }

        public int LoginEx(string url, string sn, uint featureId, string xmlScope, int mode)
        {
            return Bit_LoginEx(url, sn, featureId, xmlScope, applicationData, ref handle, mode);
        }

        public int Logout()
        {
            return Bit_Logout(handle);
        }

        public int ConvertFeature(uint featureId, uint para1, uint para2, uint para3, uint para4, ref uint result)
        {
            return Bit_ConvertFeature(handle, featureId, para1, para2, para3, para4, ref result);
        }

        public int ReadFeature(uint featureId, ref uint featureValue)
        {
            return Bit_ReadFeature(handle, featureId, ref featureValue);
        }

        public int WriteFeature(uint featureId, uint featureValue)
        {
            return Bit_WriteFeature(handle, featureId, featureValue);
        }

        public int EncryptFeature(uint featureId, byte[] plainBuffer, byte[] cipherBuffer)
        {
            return Bit_EncryptFeature(handle, featureId, plainBuffer, cipherBuffer, (uint)plainBuffer.Length);
        }

        public int DecryptFeature(uint featureId, byte[] cipherBuffer, byte[] plainBuffer)
        {
            return Bit_DecryptFeature(handle, featureId, cipherBuffer, plainBuffer, (uint)cipherBuffer.Length);
        }

        public int QueryFeature(uint featureId, ref uint capacity)
        {
            return Bit_QueryFeature(handle, featureId, ref capacity);
        }

        public int ReleaseFeature(uint featureId, ref uint capacity)
        {
            return Bit_ReleaseFeature(handle, featureId, ref capacity);
        }

        public int GetFeatureInfo(uint featureId, ref FEATURE_INFO featureInfo)
        {
            return Bit_GetFeatureInfo(handle, featureId, ref featureInfo);
        }

        public int GetDataItemNum(ref uint number)
        {
            return Bit_GetDataItemNum(handle, ref number);
        }

        public int GetDataItemName(uint index, byte[] name, ref uint nameLen)
        {
            return Bit_GetDataItemName(handle, index, name, ref nameLen);
        }

        public int GetDataItem(string dataItemName, byte[] dataItemValue, ref uint dataItemValueLen)
        {
            return Bit_GetDataItem(handle, dataItemName, dataItemValue, ref dataItemValueLen);
        }

        public int SetDataItem(string dataItemName, byte[] dataItemValue)
        {
            return Bit_SetDataItem(handle, dataItemName, dataItemValue, (uint)dataItemValue.Length);
        }

        public int RemoveDataItem(string dataItemName)
        {
            return Bit_RemoveDataItem(handle, dataItemName);
        }

        public int GetSessionInfo(SessionType type, byte[] sessionInfo, ref uint sessionInfoLen)
        {
            return Bit_GetSessionInfo(handle, (uint)type, sessionInfo, ref sessionInfoLen);
        }

        public int UpdateOnline(string url, string sn)
        {
            return Bit_UpdateOnline(url, sn, applicationData);
        }

        public int GetRequestInfo(string sn, uint bindingType, byte[] requestInfo, ref uint requestInfoSize)
        {
            return Bit_GetRequestInfo(sn, applicationData, bindingType, requestInfo, ref requestInfoSize);
        }

        public int GetUpdateInfo(string url, string sn, string requestInfo, byte[] updateInfo, ref uint updateInfoSize)
        {
            return Bit_GetUpdateInfo(url, sn, applicationData, requestInfo, updateInfo, ref updateInfoSize);
        }

        public int ApplyUpdateInfo(string updateInfo, byte[] receiptInfo, ref uint receiptInfoSize)
        {
            return Bit_ApplyUpdateInfo(applicationData, updateInfo, receiptInfo, ref receiptInfoSize);
        }

        public int SetRootPath(string path)
        {
            return Bit_SetRootPath(path);
        }

        public int GetProductPath(byte[] productPath, uint productPathSize)
        {
            return Bit_GetProductPath(applicationData, productPath, productPathSize);
        }

        public int Revoke(string url, string sn, byte[] revocationInfo, ref uint revocationInfoSize)
        {
            return Bit_Revoke(url, sn, applicationData, revocationInfo, ref revocationInfoSize);
        }

        public int GetInfo(string sn, uint type, byte[] info, ref uint infoSize)
        {
            return Bit_GetInfo(sn, applicationData, type, info, ref infoSize);
        }

        public int SetProxy(string hostName, uint port, string userId, string password)
        {
            return Bit_SetProxy(applicationData, hostName, port, userId, password);
        }

        public int SetLocalServer(string host, uint port, uint timeoutSeconds)
        {
            return Bit_SetLocalServer(applicationData, host, port, timeoutSeconds);
        }

        public int RemoveSn(string sn)
        {
            return Bit_RemoveSn(sn, applicationData);
        }

        public int GetVersion(ref uint version)
        {
            return Bit_GetVersion(ref version);
        }

        public int CheckOutSn(string url, uint featureId, uint durationDays)
        {
            return Bit_CheckOutSn(url, featureId, applicationData, durationDays);
        }

        public int CheckOutFeatures(string url, uint[] featureIds, uint durationDays)
        {
            uint size = 0;
            if (featureIds != null)
            {
                size = (uint)featureIds.Length;
            }
            return Bit_CheckOutFeatures(url, applicationData, featureIds, size, durationDays);
        }

        public int CheckIn(string url, uint featureId)
        {
            return Bit_CheckIn(url, featureId, applicationData);
        }

        public int TestBitService(string url, string sn, uint featureId)
        {
            return Bit_TestBitService(url, sn, featureId, applicationData);
        }

        public int SetCustomInfo(uint infoId, byte[] infoData)
        {
            return Bit_SetCustomInfo(infoId, infoData, (uint)infoData.Length);
        }

        public int SetAppVersion(uint version)
        {
           return Bit_SetAppVersion(version);
        }
    }

    public class BitAnswer
    {
        public bool IsX64
        {
            get
            {
                return (IntPtr.Size == 8);
            }
        }

        BitAnswerInterface bitAnswer;
        public BitAnswer()
        {
            if (IsX64)
            {
                bitAnswer = new BitAnswerX64();
            }
            else
            {
                bitAnswer = new BitAnswerX86();
            }
        }

        public void Login(string url, string sn, LoginMode mode)
        {
            int status = bitAnswer.Login(url, sn, (int)mode);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void LoginEx(string url, string sn, uint featureId, string xmlScope, LoginMode mode)
        {
            int status = bitAnswer.LoginEx(url, sn, featureId, xmlScope, (int)mode);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void Logout()
        {
            int status = bitAnswer.Logout();
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public uint ConvertFeature(uint featureId, uint para1, uint para2, uint para3, uint para4)
        {
            uint result = 0;
            int status = bitAnswer.ConvertFeature(featureId, para1, para2, para3, para4, ref result);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return result;
        }

        public uint ReadFeature(uint featureId)
        {
            uint featureValue = 0;
            int status = bitAnswer.ReadFeature(featureId, ref featureValue);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return featureValue;
        }

        public void WriteFeature(uint featureId, uint featureValue)
        {
            int status = bitAnswer.WriteFeature(featureId, featureValue);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public byte[] EncryptFeature(uint featureId, byte[] plainBuffer)
        {
            byte[] cipherBuffer = new byte[plainBuffer.Length];
            int status = bitAnswer.EncryptFeature(featureId, plainBuffer, cipherBuffer);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return cipherBuffer;
        }

        public byte[] DecryptFeature(uint featureId, byte[] cipherBuffer)
        {
            byte[] plainBuffer = new byte[cipherBuffer.Length];
            int status = bitAnswer.DecryptFeature(featureId, cipherBuffer, plainBuffer);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return plainBuffer;
        }

        public uint QueryFeature(uint featureId)
        {
            uint capacity = 0;
            int status = bitAnswer.QueryFeature(featureId, ref capacity);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return capacity;
        }

        public uint ReleaseFeature(uint featureId)
        {
            uint capacity = 0;
            int status = bitAnswer.ReleaseFeature(featureId, ref capacity);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return capacity;
        }

        public FEATURE_INFO GetFeatureInfo(uint featureId)
        {
            FEATURE_INFO featureInfo = new FEATURE_INFO();
            int status = bitAnswer.GetFeatureInfo(featureId, ref featureInfo);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return featureInfo;
        }

        public uint GetDataItemNum()
        {
            uint num = 0;
            int status = bitAnswer.GetDataItemNum(ref num);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return num;
        }

        public string GetDataItemName(uint index)
        {
            uint nameLen = 129;
            byte[] name = new byte[nameLen];
            int status = bitAnswer.GetDataItemName(index, name, ref nameLen);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return Encoding.GetEncoding("gbk").GetString(name);
        }

        public byte[] GetDataItem(string dataItemName)
        {
            uint dataItemValueLen = 1024;
            byte[] dataItemValueTemp = new byte[dataItemValueLen];
            int status = bitAnswer.GetDataItem(dataItemName, dataItemValueTemp, ref dataItemValueLen);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }

            byte[] dataItemValue = new byte[dataItemValueLen];
            Array.Copy(dataItemValueTemp, dataItemValue, dataItemValueLen);
            return dataItemValue;
        }

        public void SetDataItem(string dataItemName, byte[] dataItemValue)
        {
            int status = bitAnswer.SetDataItem(dataItemName, dataItemValue);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void RemoveDataItem(string dataItemName)
        {
            int status = bitAnswer.RemoveDataItem(dataItemName);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public string GetSessionInfo(SessionType type)
        {
            uint xmlSessionInfoLen = 10240;
            byte[] xmlSessionInfo = new byte[xmlSessionInfoLen];
            int status = bitAnswer.GetSessionInfo(type, xmlSessionInfo, ref xmlSessionInfoLen);
            if (status == (int)BitAnswerExceptionCode.BIT_ERR_BUFFER_SMALL)
            {
                xmlSessionInfo = new byte[xmlSessionInfoLen];
                status = bitAnswer.GetSessionInfo(type, xmlSessionInfo, ref xmlSessionInfoLen);
            }
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return ASCIIEncoding.UTF8.GetString(xmlSessionInfo);
        }

        public string GetInfo(string sn, InfoType type)
        {
            uint infoLen = 10240;
            byte[] info = new byte[infoLen];
            int status = bitAnswer.GetInfo(sn, (uint)type, info, ref infoLen);
            while (status == (int)BitAnswerExceptionCode.BIT_ERR_BUFFER_SMALL)
            {
                infoLen += 10240;
                info = new byte[infoLen];
                status = bitAnswer.GetInfo(sn, (uint)type, info, ref infoLen);
            }
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return ASCIIEncoding.UTF8.GetString(info);
        }

        public void UpdateOnline(string url, string sn)
        {
            int status = bitAnswer.UpdateOnline(url, sn);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public string GetRequestInfo(string sn, BindingType bindingType)
        {
            uint requestInfoSize = 10240;
            byte[] requestInfo = new byte[requestInfoSize];
            int status = bitAnswer.GetRequestInfo(sn, (uint)bindingType, requestInfo, ref requestInfoSize);
            if (status == (int)BitAnswerExceptionCode.BIT_ERR_BUFFER_SMALL)
            {
                requestInfo = new byte[requestInfoSize];
                status = bitAnswer.GetRequestInfo(sn, (uint)bindingType, requestInfo, ref requestInfoSize);
            }
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return ASCIIEncoding.UTF8.GetString(requestInfo);
        }

        public string GetUpdateInfo(string url, string sn, string requestInfo)
        {
            uint updateInfoSize = 1024;
            byte[] updateInfo = new byte[updateInfoSize];
            int status = bitAnswer.GetUpdateInfo(url, sn, requestInfo, updateInfo, ref updateInfoSize);
            if (status == (int)BitAnswerExceptionCode.BIT_ERR_BUFFER_SMALL)
            {
                updateInfo = new byte[updateInfoSize];
                status = bitAnswer.GetUpdateInfo(url, sn, requestInfo, updateInfo, ref updateInfoSize);
            }
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return ASCIIEncoding.UTF8.GetString(updateInfo);
        }

        public string ApplyUpdateInfo(string updateInfo)
        {
            uint receiptInfoSize = 10240;
            byte[] receiptInfo = new byte[receiptInfoSize];
            int status = bitAnswer.ApplyUpdateInfo(updateInfo, receiptInfo, ref receiptInfoSize);
            if (status == (int)BitAnswerExceptionCode.BIT_ERR_BUFFER_SMALL)
            {
                receiptInfo = new byte[receiptInfoSize];
                status = bitAnswer.ApplyUpdateInfo(updateInfo, receiptInfo, ref receiptInfoSize);
            }
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return ASCIIEncoding.UTF8.GetString(receiptInfo);
        }

        public void SetRootPath(string path)
        {
            int status = bitAnswer.SetRootPath(path);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public String GetProductPath()
        {
            byte[] path = new byte[256];
            uint size = (uint)path.Length;
            int status = bitAnswer.GetProductPath(path, size);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return ASCIIEncoding.UTF8.GetString(path);
        }

        public string Revoke(string sn)
        {
            uint revocationInfoSize = 10240;
            byte[] revocationInfo = new byte[revocationInfoSize];
            int status = bitAnswer.Revoke(null, sn, revocationInfo, ref revocationInfoSize);
            if (status == (int)BitAnswerExceptionCode.BIT_ERR_BUFFER_SMALL)
            {
                revocationInfo = new byte[revocationInfoSize];
                status = bitAnswer.Revoke(null, sn, revocationInfo, ref revocationInfoSize);
            }
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return ASCIIEncoding.UTF8.GetString(revocationInfo);
        }

        public void RevokeOnline(string url, string sn)
        {
            uint revocationInfoSize = 10240;
            int status = bitAnswer.Revoke(url, sn, null, ref revocationInfoSize);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        } 

        public void SetProxy(string host, uint port, string userId, string password)
        {
            int status = bitAnswer.SetProxy(host, port, userId, password);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void SetLocalServer(string host, uint port, uint timeoutSeconds)
        {
            int status = bitAnswer.SetLocalServer(host, port, timeoutSeconds);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void RemoveSn(string sn)
        {
            int status = bitAnswer.RemoveSn(sn);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public uint GetVersion()
        {
            uint version = 0;
            int status = bitAnswer.GetVersion(ref version);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
            return version;
        }

        public void CheckOutSn(string url, uint featureId, uint durationDays)
        {
            int status = bitAnswer.CheckOutSn(url, featureId, durationDays);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void CheckOutFeatures(string url, uint[] featureIds, uint durationDays)
        {
            int status = bitAnswer.CheckOutFeatures(url, featureIds, durationDays);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void CheckIn(string url, uint featureId)
        {
            int status = bitAnswer.CheckIn(url, featureId);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void TestBitService(string url, string sn, uint featureId)
        {
            int status = bitAnswer.TestBitService(url, sn, featureId);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void SetCustomInfo(uint infoId, byte[] infoData)
        {
            int status = bitAnswer.SetCustomInfo(infoId, infoData);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }

        public void SetAppVersion(uint version)
        {
            int status = bitAnswer.SetAppVersion(version);
            if (status != 0)
            {
                throw new BitAnswerException(status);
            }
        }
    }
}
