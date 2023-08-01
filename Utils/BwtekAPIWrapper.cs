using System;
using System.Runtime.InteropServices;

class BwtekAPIWrapper
{
    private const string DLLNAME = "BWTEKUSB.dll";
    [DllImport(DLLNAME)]
    public static extern bool InitDevices();

    [DllImport(DLLNAME)]
    public static extern int CloseDevices();

    [DllImport(DLLNAME)]
    public static extern int GetDeviceCount();

    [DllImport(DLLNAME)]
    public static extern Int32 GetCCode(byte[] pCCode, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 GetUSBType(ref int USBType, Int32 nChannel);


    //(1)  Basic Functions
    [DllImport("bwtekusb.dll", EntryPoint = "bwtekTestUSB")]
    public static extern Int32 bwtekTestUSB(Int32 nUSBTiming, Int32 nPixelNo, Int32 nInputMode, Int32 nchannel, Int32 pParam);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetTimeUSB(Int32 lTime, Int32 nchannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetTimeBaseUSB(ref Int32 TimeBase, ref Int32 TimeBaseUnit, Int32 nchannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetTimeBaseUSB(Int32 lTimeBase, Int32 nchannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetTimeBase0USB(Int32 lTimeBase, Int32 nchannel);
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetTimingsUSB(Int32 lTriggerExit, Int32 nMultiple, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekDataReadUSB(Int32 nTriggerMode, UInt16[] pArray, Int32 nchannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekReadResultUSB(Int32 nTriggerMode, Int32 nAverage, Int32 nTypeSmoothing, Int32 nValueSmoothing, UInt16[] pArray, Int32 nchannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekCloseUSB(Int32 nchannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekReadEEPROMUSB(string OutFileName, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekStopIntegration(Int32 nchannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekReadTemperature(Int32 nADChannel, ref Int32 nADValue, ref double dTemperature, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetTemperature(Int32 nDAChannel, Int32 nSetTemp, Int32 nChannel);

    //(2) Supplementary Functions
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSmoothingUSB(int nTypeSmoothing, int nValueSmoothing, double[] pArray, int nNum);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekConvertDerivativeDouble(Int32 nTypeDerivate, Int32 nPolynominalPointHalf, Int32 nPolynominalOrder, Int32 nDerivativeOrder, double[] pSrcArray, double[] pResultArray, Int32 nNum);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekDifferentiateDouble(Int32 nPointInterval, double[] pSrcArray, double[] pWavelengthArray, double[] pResultArray, Int32 nNum);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekPolyFit(double[] x, double[] y, int numPts, double[] coefs, int order);


    [DllImport(DLLNAME)]
    public static extern void bwtekPolyCalc(Double[] coefs, Int32 order, Int32 x, ref Double y);

    //[DllImport("bwtekusb. dll ")]
    //public static extern int bwtekDataExport(
    //    ref DataExport_Parameter_Struct exportdata_struct_In,
    //    ref DataExport_Parameter_Struct0 exportdata_struct0_In,
    //    double[] pRawArray, double[] pResultArray, ref int nResultArrayLen);

    //[DllImport("bwtekusb. dll ")]
    //public static extern int bwtekDataExport1(
    //    ref DataExport_Parameter_Struct exportdata_struct_In,
    //    ref DataExport_Parameter_Struct1 exportdata_struct1_In,
    //    double[] pRawArray, double[] pResultArray, ref int nResultArrayLen);


    //(3) Multi-Purpose TTL Output
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetExtLaser(Int32 onoff, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetExtSync(Int32 onoff, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetExtShutter(Int32 onoff, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetedMode(Int32 nGateTime, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetExtPulse(Int32 nOnOff, Int32 nDelayTime, Int32 nHigh, Int32 nLow, Int32 nPulse, Int32 nInverse, Int32 nChannel);


    //(4) AUX Port Functions
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetExtStatus(Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetTTLIn(Int32 nNo, ref Int32 pGetValue, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetTTLOut(Int32 nNo, Int32 nSetValue, Int32 nInverse, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetAnalogIn(Int32 nNo, ref Int32 nValue, ref double dVoltage, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern int bwtekSetAnalogOut(int nNo, int nValue, int nChannel);


    // (5) BTF111
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekLEDOn(Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekLEDOff(Int32 nChannel);


    [DllImport(DLLNAME)]
    public static extern Int32 bwtekLEDDelay(Int32 nDelay, Int32 nChannel);

    // (6) BTF113
    [DllImport(DLLNAME)]
    public static extern void bwtekSetPulseNo(Int32 nPulseNo, Int32 nChannel);


    // (7) Shutter Functions
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekShutterOpen(Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekShutterClose(Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekShutterInverse(Int32 nInverse, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekShutterControl(Int32 nSetShutter1, Int32 nSetShutter2, Int32 nChannel);



    // (8) BTC261, BTC262
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetABGain(Int32 nAB, Int32 nGain, Int32 nChannel);


    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetABOffset(Int32 nAB, Int32 nOffset, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetABGain(Int32 nAB, ref Int32 nGain, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetABOffset(Int32 nAB, ref Int32 nOffset, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetInGaAsMode(Int32 nMode, Int32 nChannel);




    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetInGaAsMode(ref Int32 nMode, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekQueryTemperature(Int32 nCommand, ref Int32 nADValue, ref double dTemperature, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekAccessDeltaTemp(Int32 nReadWrite, ref double dDeltaTemperature, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekAccessDeltaTemp1(Int32 nReadWrite, ref double dDeltaTemperature, ref double dDeltaTemperature1, Int32 nChannel);


    [DllImport(DLLNAME)]
    public static extern Int32 bwtekReadValue(Int32 nMode, ref Int32 GetValue, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekWriteValue(Int32 nMode, Int32 SetValue, Int32 nChannel);




    // (9) BTC263
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetTimeUnitUSB(Int32 nTimeUnit, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetTimeUnitUSB(ref Int32 nTimeUnit, Int32 nChannel);


    // (10) Read/Write Channel Number
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetupChannel(Int32 nFlag, [In, Out] Byte[] pChannelStatus);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSaveEEPROMChannel([In, Out] Byte[] pChannelStatus);


    // (11) Read  CCode 
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetCCode([In, Out] Byte[] pCCode, Int32 nChannel);

    //(12) Read x-axis reverse flag 
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekGetXaxisInverseByte(ref Int32 InverseByte, Int32 nChannel);

    //(13) USB3.0 Spectrometer  (BRC115, BRC115P, BTC655,BTC655N, BTC665,BTC665N)
    [DllImport(DLLNAME)]
    public static extern Int32 bwtekDSPDataReadUSB(Int32 nAveNum, Int32 nSmoothing, Int32 nDarkCompensate, Int32 nTriggerMode, UInt16[] pArray, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekFrameDataReadUSB(Int32 nFrameNum, Int32 nTriggerMode, UInt16[] pArray, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekEraseBlockUSB(Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekWriteBlockUSB(Int32 nAddrress, byte[] pDataArray, Int32 nNum, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekReadBlockUSB(Int32 nAddrress, byte[] pDataArray, Int32 nNum, Int32 nChannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSetLowNoiseModeUSB(Int32 nEanbleLowNoiseMode, Int32 nchannel);

    [DllImport(DLLNAME)]
    public static extern Int32 bwtekSoftReset_CEnP(Int32 nChannel);

}
