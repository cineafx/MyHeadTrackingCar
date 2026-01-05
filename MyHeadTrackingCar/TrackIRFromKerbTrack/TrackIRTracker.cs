using TrackIRUnity;
using UnityEngine;
using Microsoft.Win32;

public class TrackIRTracker
{
    TrackIRClient trackIRclient;

    public TrackIRTracker()
    {
        Debug.Log("[KerbTrack] Initialising TrackIR...");

        // TrackIRUnity's init throws a NullRef if the DLL location isn't found.
        // Check this before starting.
        bool keyFound = false;
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\NaturalPoint\\NATURALPOINT\\NPClient Location", false);
        if (registryKey != null && registryKey.GetValue("Path") != null)
            keyFound = true;
        registryKey.Close();

        string status;
        if (keyFound)
        {
            trackIRclient = new TrackIRUnity.TrackIRClient();
            if (trackIRclient == null)
                status = "Failed to start.";
            else
                status = trackIRclient.TrackIR_Enhanced_Init();
        }
        else
            status = "TrackIR not installed";

        Debug.Log("[KerbTrack] TrackIR status: " + status);
    }

    public void GetData(ref Vector3 rot, ref Vector3 pos, ref ulong staleFrames)
    {
        if (trackIRclient != null)
        {
			// https://docs.trackir.com/trackir-sdk/trackir-data
			TrackIRClient.LPTRACKIRDATA data = trackIRclient.client_HandleTrackIRData();

			const float kEncodedRangeMinMax = 16383.0f;
			const float kDecodedTranslationMinMaxMeters = 0.5f; // +/- 50 cm
            const float kDecodedRotationMinMaxDegrees = 180.0f;

			// Negate right-hand rule rotations 
			// to be consistent with left-handed coordinate basis.
			rot.z = (-data.fNPRoll/ kEncodedRangeMinMax) * kDecodedRotationMinMaxDegrees;
			rot.x = (-data.fNPPitch / kEncodedRangeMinMax) * kDecodedRotationMinMaxDegrees;
			rot.y = (-data.fNPYaw / kEncodedRangeMinMax) * kDecodedRotationMinMaxDegrees;

			pos.x = (-data.fNPX / kEncodedRangeMinMax) * kDecodedTranslationMinMaxMeters;
			pos.y = (data.fNPY / kEncodedRangeMinMax) * kDecodedTranslationMinMaxMeters;
			pos.z = (data.fNPZ / kEncodedRangeMinMax) * kDecodedTranslationMinMaxMeters;
			
			staleFrames = trackIRclient.NPStaleFrames;
        }
	}

    public void ResetOrientation() { }

    public void Stop()
    {
	    trackIRclient.TrackIR_Shutdown();
    }
}
