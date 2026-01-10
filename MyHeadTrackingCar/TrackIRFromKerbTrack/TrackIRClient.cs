using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MyHeadTrackingCar.TrackIRFromKerbTrack;

/// <summary>
/// Taken from <a href="https://github.com/FirstPersonKSP/KerbTrack/blob/c5850ffee11600cc54b305902175144ec0c5a820/KerbTrack/TrackIRClient.cs" >FirstPersonKSP KerbTrack</a>
/// under GPL-2.0 license.<br/>
/// Fixed missing TrackIR NPClient DLL registry NullRef error,
/// adjusted namespace, changed result handling and implemented style / formatting changes.
/// Expanded RequestData and program ID result logging output.<br/>
/// Functionality stays pretty much the same.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class TrackIRClient
{
    private dNP_GetSignatureDelegate NP_GetSignatureDelegate;
    private dNP_RegisterWindowHandle NP_RegisterWindowHandle;
    private dNP_UnregisterWindowHandle NP_UnregisterWindowHandle;
    private dNP_RegisterProgramProfileID NP_RegisterProgramProfileID;
    private dNP_QueryVersion NP_QueryVersion;
    private dNP_RequestData NP_RequestData;
    private dNP_GetData NP_GetData;
    //private TrackIRClient.dNP_UnregisterNotify NP_UnregisterNotify;
    private dNP_StartCursor NP_StartCursor;
    private dNP_StopCursor NP_StopCursor;
    private dNP_ReCenter NP_ReCenter;
    private dNP_StartDataTransmission NP_StartDataTransmission;
    private dNP_StopDataTransmission NP_StopDataTransmission;
    private ulong NPFrameSignature;
    internal ulong NPStaleFrames;

    [DllImport("user32.dll")]
    private static extern int GetForegroundWindow();

    [DllImport("kernel32.dll")]
    private static extern int LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(int hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

    [DllImport("kernel32.dll")]
    private static extern bool FreeLibrary(int hModule);

    public List<string> TrackIR_Enhanced_Init()
    {
        NPFrameSignature = 0UL;
        NPStaleFrames = 0UL;
        string dllPath = "";
        List<string> result = [];
        
        /* ----- Get TrackIR NPClient DLL location ----- */
        GetDLLLocation(ref dllPath);
        if (string.IsNullOrEmpty(dllPath))
        {
            result.Add("TrackIR NPClient DLL not found. TrackIR software might not be installed.");
            return result;
        }
        /* ----- Init client ----- */
        //int NPResult = (int)NPClient_Init(dllPath); // TODO: figure out why this was here twice
        if (NPClient_Init(dllPath) != NPRESULT.NP_OK)
        {
            result.Add("Error initializing NPClient interface!!");
            return result;
        }

        result.Add("NPClient : interface -- initialize OK");
        
        /* ----- Register window handle ----- */
        int foregroundWindow = GetForegroundWindow();
        result.Add($"NPClient : ForegroundWindow handle: {foregroundWindow}");
        if (NP_RegisterWindowHandle(foregroundWindow) != NPRESULT.NP_OK)
        {
            result.Add("NPClient : Error registering window handle!!");
            return result;
        }

        result.Add("NPClient : Window handle registration successful.");
        
        /* ----- Query software version ----- */
        ushort pwVersion = (ushort)0;
        if (NP_QueryVersion(ref pwVersion) != NPRESULT.NP_OK)
        {
            result.Add("NPClient : Error querying NaturalPoint software version!!");
            return result;
        }

        int majorVersion = pwVersion >> 8;
        int minorVersion = pwVersion & 0xFF;
        result.Add($"NPClient : NaturalPoint software version is {majorVersion}.{minorVersion}");
        
        /* ----- Set requested data fields ----- */
        // Roll  =  1
        // Pitch =  2
        // Yaw   =  4
        // X     = 16
        // Y     = 32
        // Z     = 64
        if (NP_RequestData((ushort)(1U | 2U | 4U | 16U | 32U | 64U)) != NPRESULT.NP_OK)
        {
            result.Add("NPClient : Error setting RequestData values!!");
            return result;
        }
        result.Add("NPClient : RequestData values configured.");
        
        /* ----- Register program ID ----- */
        // 20430 is the "Unity 3d plugin" one
        if (NP_RegisterProgramProfileID((ushort)20430) != NPRESULT.NP_OK)
        {
            result.Add("NPClient : Error registering program profile ID!!");
            return result;
        }
        result.Add("NPClient : Program profile ID registered.");
        
        /* ----- Stop cursor (mouse cursor control with TrackIR) ----- */
        if (NP_StopCursor() != NPRESULT.NP_OK)
        {
            result.Add("NPClient : Error stopping cursor");
            return result;
        }

        result.Add("NPClient : Cursor stopped");
        
        /* ----- Start data transmission ----- */
        if (NP_StartDataTransmission() != NPRESULT.NP_OK)
        {
            result.Add("NPClient : Error starting data transmission");
            return result;
        }

        result.Add("NPClient : Data Transmission started");
        return result;
    }

    public LPTRACKIRDATA client_HandleTrackIRData()
    {
        LPTRACKIRDATA pTID = new LPTRACKIRDATA();
        if (NP_GetData(ref pTID) != NPRESULT.NP_OK || (int)pTID.wNPStatus != (int)NPSTATUS.NPSTATUS_REMOTEACTIVE)
            return pTID;
        if ((long)NPFrameSignature != (long)pTID.wPFrameSignature)
        {
            NPFrameSignature = (ulong)pTID.wPFrameSignature;
            NPStaleFrames = 0UL;
            return pTID;
        }
        else
        {
            if (NPStaleFrames > 30UL)
                return pTID;
            ++NPStaleFrames;
            return pTID;
        }
    }

    public string client_TestTrackIRData()
    {
        LPTRACKIRDATA pTID = new LPTRACKIRDATA();
        string result = "";
        if (NP_GetData(ref pTID) == NPRESULT.NP_OK)
        {
            if ((int)pTID.wNPStatus == (int)NPSTATUS.NPSTATUS_REMOTEACTIVE)
            {
                if ((long)NPFrameSignature != (long)pTID.wPFrameSignature)
                {
                    result = result + "Pitch: " + pTID.fNPPitch + "\r\n" +
                             "Roll: " + pTID.fNPRoll + "\r\n" +
                             "Yaw: " + pTID.fNPYaw + "\r\n" +
                             "PosX: " + pTID.fNPX + "\r\n" +
                             "PosY: " + pTID.fNPY + "\r\n" +
                             "PosZ: " + pTID.fNPX + "\r\n";
                    NPFrameSignature = (ulong)pTID.wPFrameSignature;
                    NPStaleFrames = 0UL;
                }
                else if (NPStaleFrames > 30UL)
                {
                    result += "No New Data. Paused or Not Tracking?\r\n" +
                              "Information NPStatus = " + pTID.wNPStatus + "\r\n";
                }
                else
                {
                    ++NPStaleFrames;
                    result += "No New Data for " + NPStaleFrames + " frames\r\n" +
                              "Information NPStatus = " + pTID.wNPStatus + "\r\n";
                }
            }
        }
        else
            result = result + "User Disabled";
        return result;
    }

    public string TrackIR_Shutdown()
    {
        string result = "";
        result += NP_StopDataTransmission() != NPRESULT.NP_OK ? "StopDataTransmission() ERROR!!\r\n" : "StopDataTransmission() OK\r\n";
        result += NP_StartCursor() != NPRESULT.NP_OK ? "StartCursor() ERROR!!\r\n" : "StartCursor() OK\r\n";
        result += NP_UnregisterWindowHandle() != NPRESULT.NP_OK ? "UnregisterWindowHandle() ERROR!!\r\n" : "UnregisterWindowHandle() OK\r\n";
        return result;
    }

    public NPRESULT NPClient_Init(string dllPath)
    {
        //LET THE SORCERY COMMENCE
        if (IntPtr.Size == 4) //32 bit
        {
            dllPath = dllPath + "NPClient.dll";
        }
        else if (IntPtr.Size == 8) //64 bit
        {
            dllPath = dllPath + "NPClient64.dll";
        }
        if (!File.Exists(dllPath))
            return NPRESULT.NP_ERR_DLL_NOT_FOUND;
        int hModule = LoadLibrary(dllPath);
        if (hModule == 0)
            return NPRESULT.NP_ERR_DLL_NOT_FOUND;
        NP_GetSignatureDelegate = (dNP_GetSignatureDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_GetSignature"), typeof(dNP_GetSignatureDelegate));
        LPTRACKIRSIGNATUREDATA signature = new LPTRACKIRSIGNATUREDATA();
        LPTRACKIRSIGNATUREDATA lptrackirsignaturedata = new LPTRACKIRSIGNATUREDATA();
        lptrackirsignaturedata.DllSignature = "precise head tracking\n put your head into the game\n now go look around\n\n Copyright EyeControl Technologies";
        lptrackirsignaturedata.AppSignature = "hardware camera\n software processing data\n track user movement\n\n Copyright EyeControl Technologies";
        NPRESULT npresult;
        if (NP_GetSignatureDelegate(ref signature) == NPRESULT.NP_OK)
        {
            if (string.Compare(lptrackirsignaturedata.DllSignature, signature.DllSignature) == 0 && string.Compare(lptrackirsignaturedata.AppSignature, signature.AppSignature) == 0)
            {
                npresult = NPRESULT.NP_OK;
                NP_RegisterWindowHandle = (dNP_RegisterWindowHandle)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_RegisterWindowHandle"), typeof(dNP_RegisterWindowHandle));
                NP_UnregisterWindowHandle = (dNP_UnregisterWindowHandle)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_UnregisterWindowHandle"), typeof(dNP_UnregisterWindowHandle));
                NP_RegisterProgramProfileID = (dNP_RegisterProgramProfileID)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_RegisterProgramProfileID"), typeof(dNP_RegisterProgramProfileID));
                NP_QueryVersion = (dNP_QueryVersion)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_QueryVersion"), typeof(dNP_QueryVersion));
                NP_RequestData = (dNP_RequestData)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_RequestData"), typeof(dNP_RequestData));
                NP_GetData = (dNP_GetData)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_GetData"), typeof(dNP_GetData));
                NP_StartCursor = (dNP_StartCursor)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_StartCursor"), typeof(dNP_StartCursor));
                NP_StopCursor = (dNP_StopCursor)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_StopCursor"), typeof(dNP_StopCursor));
                NP_ReCenter = (dNP_ReCenter)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_ReCenter"), typeof(dNP_ReCenter));
                NP_StartDataTransmission = (dNP_StartDataTransmission)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_StartDataTransmission"), typeof(dNP_StartDataTransmission));
                NP_StopDataTransmission = (dNP_StopDataTransmission)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, "NP_StopDataTransmission"), typeof(dNP_StopDataTransmission));
            }
            else
                npresult = NPRESULT.NP_ERR_DLL_NOT_FOUND;
        }
        else
            npresult = NPRESULT.NP_ERR_DLL_NOT_FOUND;
        return npresult;
    }

    public void GetDLLLocation(ref string dllPath)
    {
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\NaturalPoint\\NATURALPOINT\\NPClient Location", false);
        if (registryKey == null)
            return;
        dllPath = registryKey.GetValue("Path").ToString();
        registryKey.Close();
    }

    private delegate NPRESULT PF_NOTIFYCALLBACK(ushort a, ushort b);

    private delegate NPRESULT dNP_GetSignatureDelegate(ref LPTRACKIRSIGNATUREDATA signature);

    private delegate NPRESULT dNP_RegisterWindowHandle(int hWnd);

    private delegate NPRESULT dNP_RegisterProgramProfileID(ushort wPPID);

    private delegate NPRESULT dNP_UnregisterWindowHandle();

    private delegate NPRESULT dNP_QueryVersion(ref ushort pwVersion);

    private delegate NPRESULT dNP_RequestData(ushort wDataReq);

    private delegate NPRESULT dNP_GetData(ref LPTRACKIRDATA pTID);

    private delegate NPRESULT dNP_RegisterNotify(PF_NOTIFYCALLBACK pfNotify);

    private delegate NPRESULT dNP_UnregisterNotify();

    private delegate NPRESULT dNP_StartCursor();

    private delegate NPRESULT dNP_StopCursor();

    private delegate NPRESULT dNP_ReCenter();

    private delegate NPRESULT dNP_StartDataTransmission();

    private delegate NPRESULT dNP_StopDataTransmission();

    public struct LPTRACKIRSIGNATUREDATA
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string DllSignature;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string AppSignature;
    }

    public enum NPSTATUS
    {
        NPSTATUS_REMOTEACTIVE = 0,
        NPSTATUS_REMOTEDISABLED = 1
    }

    public enum NPRESULT
    {
        NP_OK,
        NP_ERR_DEVICE_NOT_PRESENT,
        NP_ERR_UNSUPPORTED_OS,
        NP_ERR_INVALID_ARG,
        NP_ERR_DLL_NOT_FOUND,
        NP_ERR_NO_DATA,
        NP_ERR_INTERNAL_DATA,
    }

    public struct LPTRACKIRDATA
    {
        public ushort wNPStatus;
        public ushort wPFrameSignature;
        public uint dwNPIOData;
        public float fNPRoll;
        public float fNPPitch;
        public float fNPYaw;
        public float fNPX;
        public float fNPY;
        public float fNPZ;
        public float fNPRawX;
        public float fNPRawY;
        public float fNPRawZ;
        public float fNPDeltaX;
        public float fNPDeltaY;
        public float fNPDeltaZ;
        public float fNPSmoothX;
        public float fNPSmoothY;
        public float fNPSmoothZ;
    }
}