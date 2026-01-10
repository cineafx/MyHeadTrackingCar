using System;
using System.Collections.Generic;
using System.Linq;
using MSCLoader;
using UnityEngine;

namespace MyHeadTrackingCar.TrackIRFromKerbTrack;

/// <summary>
/// Taken from <a href="https://github.com/FirstPersonKSP/KerbTrack/blob/c5850ffee11600cc54b305902175144ec0c5a820/KerbTrack/TrackIRTracker.cs" >FirstPersonKSP KerbTrack</a>
/// under GPL-2.0 license.<br/>
/// Removed missing TrackIR NPClient DLL registry NullRef workaround as the root cause was fixed,
/// adjusted logger, adjusted namespace and implemented style / formatting changes. <br/>
/// Functionality stays pretty much the same.
/// </summary>
public class TrackIRTracker
{
    private readonly TrackIRClient _trackIRClient;

    public TrackIRTracker()
    {
        ModConsole.Log("[MyHeadTrackingCar] Initialising TrackIR...");

        _trackIRClient = new TrackIRClient();
        List<string> statuses = _trackIRClient.TrackIR_Enhanced_Init();

        ModConsole.Log(
            "[MyHeadTrackingCar] TrackIR status: \n" +
            string.Join(Environment.NewLine, statuses.Select(status => $"        {status}").ToArray()) +
            "\n[MyHeadTrackingCar] End of TrackIR status"
        );
    }

    public void GetData(ref Vector3 rot, ref Vector3 pos, ref ulong staleFrames)
    {
        if (_trackIRClient == null)
            return;

        // https://docs.trackir.com/trackir-sdk/trackir-data
        TrackIRClient.LPTRACKIRDATA data = _trackIRClient.client_HandleTrackIRData();

        const float kEncodedRangeMinMax = 16383.0f;
        const float kDecodedTranslationMinMaxMeters = 0.5f; // +/- 50 cm
        const float kDecodedRotationMinMaxDegrees = 180.0f;

        // Negate right-hand rule rotations 
        // to be consistent with left-handed coordinate basis.
        rot.z = -data.fNPRoll / kEncodedRangeMinMax * kDecodedRotationMinMaxDegrees;
        rot.x = -data.fNPPitch / kEncodedRangeMinMax * kDecodedRotationMinMaxDegrees;
        rot.y = -data.fNPYaw / kEncodedRangeMinMax * kDecodedRotationMinMaxDegrees;

        pos.x = -data.fNPX / kEncodedRangeMinMax * kDecodedTranslationMinMaxMeters;
        pos.y = data.fNPY / kEncodedRangeMinMax * kDecodedTranslationMinMaxMeters;
        pos.z = data.fNPZ / kEncodedRangeMinMax * kDecodedTranslationMinMaxMeters;

        staleFrames = _trackIRClient.NPStaleFrames;
    }

    public void ResetOrientation()
    {
    }

    public void Stop()
    {
        ModConsole.Log("[MyHeadTrackingCar] Shutting down TrackIR...");
        _trackIRClient.TrackIR_Shutdown();
    }
}