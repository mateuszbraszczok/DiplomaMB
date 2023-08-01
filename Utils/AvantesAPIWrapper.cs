using System;
using System.Runtime.InteropServices;

namespace DiplomaMB.Utils
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    internal class AvantesAPIWrapper
    {
        public const int ERR_SUCCESS = 0;
        public const int ERR_INVALID_PARAMETER = -1;
        public const int ERR_OPERATION_NOT_SUPPORTED = -2;
        public const int ERR_DEVICE_NOT_FOUND = -3;
        public const int ERR_INVALID_DEVICE_ID = -4;
        public const int ERR_OPERATION_PENDING = -5;
        public const int ERR_TIMEOUT = -6;
        public const int ERR_INVALID_PASSWORD = -7;
        public const int ERR_INVALID_MEAS_DATA = -8;
        public const int ERR_INVALID_SIZE = -9;
        public const int ERR_INVALID_PIXEL_RANGE = -10;
        public const int ERR_INVALID_INT_TIME = -11;
        public const int ERR_INVALID_COMBINATION = -12;
        public const int ERR_INVALID_CONFIGURATION = -13;
        public const int ERR_NO_MEAS_BUFFER_AVAIL = -14;
        public const int ERR_UNKNOWN = -15;
        public const int ERR_COMMUNICATION = -16;
        public const int ERR_NO_SPECTRA_IN_RAM = -17;
        public const int ERR_INVALID_DLL_VERSION = -18;
        public const int ERR_NO_MEMORY = -19;
        public const int ERR_DLL_INITIALISATION = -20;
        public const int ERR_INVALID_STATE = -21;
        public const int ERR_INVALID_REPLY = -22;
        public const int ERR_CONNECTION_FAILURE = ERR_COMMUNICATION;
        public const int ERR_ACCESS = -24;
        public const int ERR_INTERNAL_READ = -25;
        public const int ERR_INTERNAL_WRITE = -26;
        public const int ERR_ETHCONN_REUSE = -27;
        public const int ERR_INVALID_DEVICE_TYPE = -28;
        public const int ERR_SECURE_CFG_NOT_READ = -29;
        public const int ERR_UNEXPECTED_MEAS_RESPONSE = -30;

        // Return error codes; DeviceData check
        public const int ERR_INVALID_PARAMETER_NR_PIXELS = -100;
        public const int ERR_INVALID_PARAMETER_ADC_GAIN = -101;
        public const int ERR_INVALID_PARAMETER_ADC_OFFSET = -102;

        // Return error codes; PrepareMeasurement check
        public const int ERR_INVALID_MEASPARAM_AVG_SAT2 = -110;
        public const int ERR_INVALID_MEASPARAM_AVG_RAM = -111;
        public const int ERR_INVALID_MEASPARAM_SYNC_RAM = -112;
        public const int ERR_INVALID_MEASPARAM_LEVEL_RAM = -113;
        public const int ERR_INVALID_MEASPARAM_SAT2_RAM = -114;
        public const int ERR_INVALID_MEASPARAM_FWVER_RAM = -115; // StoreToRAM in 0.20.0.0 and later
        public const int ERR_INVALID_MEASPARAM_DYNDARK = -116;

        // Return error codes; SetSensitivityMode check
        public const int ERR_NOT_SUPPORTED_BY_SENSOR_TYPE = -120;
        public const int ERR_NOT_SUPPORTED_BY_FW_VER = -121;
        public const int ERR_NOT_SUPPORTED_BY_FPGA_VER = -122;

        // Return error codes; SuppressStrayLight check
        public const int ERR_SL_CALIBRATION_NOT_AVAILABLE = -140;
        public const int ERR_SL_STARTPIXEL_NOT_IN_RANGE = -141;
        public const int ERR_SL_ENDPIXEL_NOT_IN_RANGE = -142;
        public const int ERR_SL_STARTPIX_GT_ENDPIX = -143;
        public const int ERR_SL_MFACTOR_OUT_OF_RANGE = -144;

        public const int INVALID_AVS_HANDLE_VALUE = 1000;
        public const byte SW_TRIGGER_MODE = 0;
        public const byte HW_TRIGGER_MODE = 1;
        public const byte SS_TRIGGER_MODE = 2;
        public const byte EXT_TRIGGER_SOURCE = 0;
        public const byte SYNCH_TRIGGER_SOURCE = 1;
        public const byte EDGE_TRIGGER_TYPE = 0;
        public const byte LEVEL_TRIGGER_TYPE = 1;

        public const byte USER_ID_LEN = 64;
        public const byte NR_WAVELEN_POL_COEF = 5;
        public const byte NR_NONLIN_POL_COEF = 8;
        public const byte NR_DEFECTIVE_PIXELS = 30;
        public const ushort MAX_NR_PIXELS = 4096;
        public const byte NR_TEMP_POL_COEF = 5;
        public const byte MAX_TEMP_SENSORS = 3;
        public const byte AVS_SERIAL_LEN = 10;
        public const byte MAX_VIDEO_CHANNELS = 2;
        public const byte NR_DAC_POL_COEF = 2;
        public const byte CLIENT_ID_SIZE = 32;
        public const byte ETHSET_RES_SIZE = 79;

        public const byte UCT_USB2 = 0;
        public const byte UCT_USB3 = 1;
        public const byte UCT_UNKNOWN = 2;

        public const uint BIT_SINGLE_ADC_MASK = 0x00000001; // bit<0>: ADC type (Single ended)
        public const uint BIT_DIF_ADC_MASK = 0x00000002; // bit<1>: ADC type (Differential)
        public const uint BIT_MATRIX_UMF_MASK = 0x00000004; // bit<2>: UMF (FX3 interface status, USB Monitoring Failure)
        public const uint BIT_MATRIX_STE_MASK = 0x00000008; // bit<3>: ST (Sensor type error)
        public const int BIT_MATRIX_UCT_POS = 4;          // bit<4,5>: UCT (USB Connection Type)
        public const uint BIT_MATRIX_UCT_MASK = 0x00000030; // bit<4,5>: UCT (USB Connection Type)
        public const int BIT_MATRIX_SB_POS = 6;          // bit<6,7,8>: SBO/SBME/DMAE (Scan Buffer errors)
        public const uint BUF_OVERFLOW_ERROR_BIT = 0x0001;
        public const uint BUF_MUTEX_ERROR_BIT = 0x0002;
        public const uint BUF_DMA_ERROR_BIT = 0x0004;
        public const uint BIT_MATRIX_EAR_MASK = 0x00000200; // bit<9>: EAR (Ethernet Auto-Recovery status)
        public const uint BIT_MATRIX_SCIS_MASK = 0x00000400; // bit<10>: SCIS (Spectrometer Control Interface Status)
        public const uint BIT_MATRIX_STI_MASK = 0x00002000; // bit<13>: STI (Spurious Trigger Idle Error)
        public const uint BIT_MATRIX_STO_MASK = 0x00004000; // bit<14>: STO (Spurious Trigger Overflow Error)

        public const int BIT_MATRIX_DCS_POS = 15;         // bit<15,16>: DCS (Device Configuration Status), see below for its definitions
        public const uint BIT_MATRIX_DCS_MASK = 0x00018000;
        public const byte DCS_USER_SETTINGS = 0;          // User-specific Device Configuration loaded and used
        public const byte DCS_GOLDEN_SETTINGS = 1;          // Factory Device Configuration loaded and used
        public const byte DCS_ERROR = 2;          // Hard-code Device Configuration used, which is an error situation!

        public const uint BIT_MATRIX_SCS_MASK = 0x00020000; // bit<17>: SCS (Secure Configuration Status)

        public const int BIT_MATRIX_HBI_POS = 20;         // bit<20-23>: HBI value
        public const uint BIT_MATRIX_HBI_MASK = 0x00F00000;
        public const int BIT_MATRIX_HBMW_POS = 24;         // bit<24-31>: HBMW value
        public const uint BIT_MATRIX_HBMW_MASK = 0xFF000000;

        // HEARTBEAT (0x47) message
        public const uint HEARTBEAT_HBCE_MASK = 0x00000001; // bit<0>: HBCE
        public const uint HEARTBEAT_EAR_MASK = 0x00000002; // bit<1>: EAR

        public const int WM_APP = 0x8000;
#if X64  // conditional compilation symbol in project
        public const int        WM_MEAS_READY                     = WM_APP + 1;
#else
        public const int WM_USER = 0x400;
        public const int WM_MEAS_READY = WM_USER + 1;
#endif
        public const int WM_CONN_STATUS = WM_APP + 15;
        public const int WM_DSTR_STATUS = WM_APP + 16;

        // Connection status codes (distributed after being registered with AVS_ActivateConn)
        public const int ETH_CONN_STATUS_CONNECTING = 0;      // Waiting to establish ethernet connection (may be sent more than once on regular time base)
                                                              // This state could also be given after ETH_CONN_STATUS_CONNECTED, in case of connection loss.
        public const int ETH_CONN_STATUS_CONNECTED = 1;       // Eth connection established, with connection recovery enabled
        public const int ETH_CONN_STATUS_CONNECTED_NOMON = 2; // Eth connection ready, without connection recovery 
        public const int ETH_CONN_STATUS_NOCONNECTION = 3;    // Unrecoverable connection failure or disconnect from user, AvaSpec library will stop trying to connect with the spectrometer!

        //---- enumerations ---------------------------------------------------

        public enum DEVICE_STATUS : byte
        {
            UNKNOWN = 0,
            USB_AVAILABLE = 1,
            USB_IN_USE_BY_APPLICATION = 2,
            USB_IN_USE_BY_OTHER = 3,
            ETH_AVAILABLE = 4,
            ETH_IN_USE_BY_APPLICATION = 5,
            ETH_IN_USE_BY_OTHER = 6,
            ETH_ALREADY_IN_USE_USB = 7
        }

        public enum SENS_TYPE : byte
        {
            SENS_HAMS8378_256 = 1,
            SENS_HAMS8378_1024 = 2,
            SENS_ILX554 = 3,
            SENS_HAMS9201 = 4,
            SENS_TCD1304 = 5,
            SENS_TSL1301 = 6,
            SENS_TSL1401 = 7,
            SENS_HAMS8378_512 = 8,
            SENS_HAMS9840 = 9,
            SENS_ILX511 = 10,
            SENS_HAMS10420_11850 = 11,
            SENS_HAMS11071_2048X64 = 12,
            SENS_HAMS7031_11501 = 13,
            SENS_HAMS7031_1024X58 = 14,
            SENS_HAMS11071_2048X16 = 15,
            SENS_HAMS11155 = 16,
            SENS_SU256LSB = 17,
            SENS_SU512LDB = 18,
            SENS_HAMS11638 = 21,
            SENS_HAMS11639 = 22,
            SENS_HAMS12443 = 23,
            SENS_HAMG9208_512 = 24,
            SENS_HAMG13913 = 25,
            SENS_HAMS13496 = 26,
            SENS_HAMS12198_512 = 27,
            SENS_HAMS12198_1024 = 28,
            SENS_HAMS11155_2048_02 = 29,
            SENS_HAMS11155_2048_02_DIFF = 30,
            NUMBER_OF_SENSOR_TYPES = 31 //leave this one at the end
        }

        public enum DEVICE_TYPE : byte
        {
            TYPE_UNKNOWN = 0,
            TYPE_AS5216 = 1,
            TYPE_ASMINI = 2,
            TYPE_AS7010 = 3,
            TYPE_AS7007 = 4
        }

        public enum INTERFACE_TYPE : byte
        {
            RS232 = 0,
            USB5216 = 1,
            USBMINI = 2,
            USB7010 = 3,
            ETH7010 = 4,
            USB7007 = 5
        }

        //---------------------------------------------------------------------
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PixelArrayType
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NR_PIXELS)]
            public double[] Value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SaturatedArrayType
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NR_PIXELS)]
            public byte[] Value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct String16Type
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string String16;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct String20Type
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string String20;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AvsIdentityType
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AVS_SERIAL_LEN)]
            public string m_SerialNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = USER_ID_LEN)]
            public string m_UserFriendlyName;
            public DEVICE_STATUS m_Status;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DetectorType
        {
            public byte m_SensorType;
            public ushort m_NrPixels;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NR_WAVELEN_POL_COEF)]
            public float[] m_aFit;
            public byte m_NLEnable;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NR_NONLIN_POL_COEF)]
            public double[] m_aNLCorrect;
            public double m_aLowNLCounts;
            public double m_aHighNLCounts;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_VIDEO_CHANNELS)]
            public float[] m_Gain;
            public float m_Reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_VIDEO_CHANNELS)]
            public float[] m_Offset;
            public float m_ExtOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NR_DEFECTIVE_PIXELS)]
            public ushort[] m_DefectivePixels;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IrradianceType
        {
            public SpectrumCalibrationType m_IntensityCalib;
            public byte m_CalibrationType;
            public uint m_FiberDiameter;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SpectrumCalibrationType
        {
            public SmoothingType m_Smoothing;
            public float m_CalInttime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NR_PIXELS)]
            public float[] m_aCalibConvers;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SmoothingType
        {
            public ushort m_SmoothPix;
            public byte m_SmoothModel;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SpectrumCorrectionType
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NR_PIXELS)]
            public float[] m_aSpectrumCorrect;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StandAloneType
        {
            public byte m_Enable;
            public MeasConfigType m_Meas;
            public short m_Nmsr;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DynamicStorageType
        {
            public int m_Nmsr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MeasConfigType
        {
            public ushort m_StartPixel;
            public ushort m_StopPixel;
            public float m_IntegrationTime;
            public uint m_IntegrationDelay;
            public uint m_NrAverages;
            public DarkCorrectionType m_CorDynDark;
            public SmoothingType m_Smoothing;
            public byte m_SaturationDetection;
            public TriggerType m_Trigger;
            public ControlSettingsType m_Control;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TimeStampType
        {
            public ushort m_Date;
            public ushort m_Time;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DarkCorrectionType
        {
            public byte m_Enable;
            public byte m_ForgetPercentage;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TriggerType
        {
            public byte m_Mode;
            public byte m_Source;
            public byte m_SourceType;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ControlSettingsType
        {
            public ushort m_StrobeControl;
            public uint m_LaserDelay;
            public uint m_LaserWidth;
            public float m_LaserWaveLength;
            public ushort m_StoreToRam;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BroadcastAnswerType
        {
            public byte InterfaceType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string serial;
            public ushort port;
            public byte status;
            public uint RemoteHostIp;
            public uint LocalIp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TempSensorType
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NR_TEMP_POL_COEF)]
            public float[] m_aFit;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TecControlType
        {
            public byte m_Enable;
            public float m_Setpoint;     // [degree Celsius]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NR_DAC_POL_COEF)]
            public float[] m_aFit;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ProcessControlType
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public float[] AnalogLow;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public float[] AnalogHigh;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public float[] DigitalLow;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public float[] DigitalHigh;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EthernetSettingsType
        {
            public uint m_IpAddr;
            public uint m_NetMask;
            public uint m_Gateway;
            public byte m_DhcpEnabled;
            public short m_TcpPort;
            public byte m_LinkStatus;
            public byte m_ClientIdType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CLIENT_ID_SIZE)]
            public char[] m_ClientIdCustom;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ETHSET_RES_SIZE)]
            public byte[] m_Reserved;
        }

        const ushort OEM_DATA_TYPE_LEN = 4096;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct OemDataType
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = OEM_DATA_TYPE_LEN)]
            public byte[] m_data;
        }

        const ushort SETTINGS_RESERVED_LEN = 9608;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DeviceConfigType
        {
            public ushort m_Len;
            public ushort m_ConfigVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = USER_ID_LEN)]
            public byte[] m_aUserFriendlyId;
            public DetectorType m_Detector;
            public IrradianceType m_Irradiance;
            public SpectrumCalibrationType m_Reflectance;
            public SpectrumCorrectionType m_SpectrumCorrect;
            public StandAloneType m_StandAlone;
            public DynamicStorageType m_DynamicStorage;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_TEMP_SENSORS)]
            public TempSensorType[] m_aTemperature;
            public TecControlType m_TecControl;
            public ProcessControlType m_ProcessControl;
            public EthernetSettingsType m_EthernetSettings;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SETTINGS_RESERVED_LEN)]
            public byte[] m_aReserved;
            public OemDataType m_OemData;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeartbeatReqType
        {
            public uint m_Data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeartbeatRespType
        {
            public uint m_BitMatrix; // Built-In Test matrix
            public uint m_Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DstrStatusType
        {
            public uint m_TotalScans;           // Internal Storage Size; the size of the scan data buffer
            public uint m_UsedScans;            // Internal Storage Scan Count; the number of used elements
            public uint m_Flags;                // DSTR measurement mode flags as described below;
            public byte m_IsStopEvent;          // m_Flags:bit<0> 1 = Measurement stopped due to STOP received or measurement ready, 0 otherwise
            public byte m_IsOverflowEvent;      // m_Flags:bit<1> 1 = FIFO overflow error occurred, 0 otherwise
            public byte m_IsInternalErrorEvent; // m_Flags:bit<2> 1 = DSTR measurement has stopped due to an internal error, 0 otherwise
            public byte m_Reserved;             // Padding byte (reserved for future use)
        }

        private const string DLLNAME = "avaspecx64.dll";

        [DllImport(DLLNAME)]
        public static extern int AVS_Init(short a_Port);

        [DllImport(DLLNAME)]
        public static extern int AVS_Done();

        [DllImport(DLLNAME)]
        public static extern int AVS_StopMeasure(int a_hDevice);

        [DllImport(DLLNAME)]
        public static extern int AVS_PollScan(int a_hDevice);

        [DllImport(DLLNAME)]
        public static extern bool AVS_Register(IntPtr a_hWnd);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetNrOfDevices();

        [DllImport(DLLNAME)]
        public static extern int AVS_UpdateUSBDevices();

        [DllImport(DLLNAME)]
        public static extern int AVS_UpdateETHDevices(uint a_ListSize, ref uint a_pRequiredSize, [In, Out] BroadcastAnswerType[] a_pList);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetVersionInfo(int a_hDevice, ref String16Type a_pFpgaVer, ref String16Type a_pFirmwareVer, ref String16Type a_pDllVer);

        [DllImport(DLLNAME)]
        public static extern int AVS_Deactivate(int a_hDevice);

        [DllImport(DLLNAME)]
        public static extern int AVS_Activate(ref AvsIdentityType a_DeviceId);

        [DllImport(DLLNAME)]
        public static extern int AVS_ActivateConn(ref AvsIdentityType a_DeviceId, IntPtr a_hWnd);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetList(uint a_ListSize, ref uint a_pRequiredSize, [In, Out] AvsIdentityType[] a_pList);

        [DllImport(DLLNAME)]
        public static extern int AVS_PrepareMeasure(int a_hDevice, ref MeasConfigType a_pMeasConfig);

        [DllImport(DLLNAME)]
        public static extern int AVS_Measure(int a_hDevice, IntPtr a_hWnd, short a_Nmsr);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetLambda(int a_hDevice, ref PixelArrayType a_pWavelength);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetNumPixels(int a_hDevice, ref ushort a_pNumPixel);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetParameter(int a_hDevice, uint a_Size, ref uint a_pRqdSize, ref DeviceConfigType a_pData);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetParameter(int a_hDevice, ref DeviceConfigType a_pDeviceParm);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetScopeData(int a_hDevice, ref uint a_pTimeLabel, ref PixelArrayType a_pSpectrum);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetSaturatedPixels(int a_hDevice, ref SaturatedArrayType a_pSaturated);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetAnalogOut(int a_hDevice, byte a_PortId, float a_Value);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetDigOut(int a_hDevice, byte a_PortId, byte a_Status);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetPwmOut(int a_hDevice, byte a_PortId, uint a_Freq, byte a_Duty);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetSyncMode(int a_hDevice, byte a_Enable);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetAnalogIn(int a_hDevice, byte a_AnalogInId, ref float a_pAnalogIn);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetDigIn(int a_hDevice, byte a_DigInId, ref byte a_pDigIn);

        [DllImport(DLLNAME)]
        public static extern int AVS_UseHighResAdc(int a_hDevice, bool a_Enable);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetPrescanMode(int a_hDevice, bool a_Prescan);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetSensitivityMode(int a_hDevice, UInt32 a_SensitivityMode);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetIpConfig(int a_hDevice, ref EthernetSettingsType a_pData);

        [DllImport(DLLNAME)]
        public static extern int AVS_SuppressStrayLight(int a_hDevice, float a_Multifactor, ref PixelArrayType a_pSrcSpectrum, ref PixelArrayType a_pDestSpectrum);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetOemParameter(int a_hDevice, ref OemDataType a_pOemData);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetOemParameter(int a_hDevice, ref OemDataType a_pOemData);

        [DllImport(DLLNAME)]
        public static extern int AVS_Heartbeat(int a_hDevice, ref HeartbeatReqType a_pHbReq, ref HeartbeatRespType a_pHbResp);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetDeviceType(int a_hDevice, ref byte a_pDeviceType);

        [DllImport(DLLNAME)]
        public static extern int AVS_ResetDevice(int a_hDevice);

        [DllImport(DLLNAME)]
        public static extern int AVS_SetDstrStatus(int a_hDevice, IntPtr a_hWnd);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetDstrStatus(int a_hDevice, ref DstrStatusType a_pDstrStatus);

        [DllImport(DLLNAME)]
        public static extern int AVS_GetDetectorName(int a_hDevice, byte a_Sensortype, ref String20Type a_pSensorName);
    }
}
