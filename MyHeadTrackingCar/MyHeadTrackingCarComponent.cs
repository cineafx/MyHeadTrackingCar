using System;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using MSCLoader;
using MyHeadTrackingCar.TrackIRFromKerbTrack;
using UnityEngine;

namespace MyHeadTrackingCar;

public class MyHeadTrackingCarComponent : MonoBehaviour
{
    public ushort applicationId = 20430; // Default = Unity 3d plugin

    #region Configurable properties

    public MyHeadTrackingCarSettings Settings { get; set; }

    private FsmString _currentVehicle;

    #endregion

    #region fields and properties for unity transform updates

    [NonSerialized]
    private Transform _positionTransform;

    [NonSerialized]
    private MouseLookWithModifiers _mouseLookWithModifiers;

    private Vector3 LocalPosition
    {
        set
        {
            if (_mouseLookWithModifiers.OriginalMouseLookEnabled)
                _positionTransform.localPosition = value;
        }
    }

    private Vector3 LocalEulerAngles
    {
        set
        {
            if (_mouseLookWithModifiers.OriginalMouseLookEnabled)
                _mouseLookWithModifiers.additionalRotation = new Vector3(
                    Mathf.Clamp(value.x, -85.0f, 85.0f),
                    Mathf.Clamp(value.y, -170.0f, 170.0f),
                    Mathf.Clamp(value.z, -60.0f, 60.0f)
                );

            _mouseLookWithModifiers.blockMouseTilt = _currentVehicle.Value.Length > 0
                ? Settings.ForceMouseOnHorizonLevelInVehicle.GetValue()
                : Settings.ForceMouseOnHorizonLevelOnFoot.GetValue();
        }
    }

    #endregion

    #region private fields HeadTracking

    [CanBeNull]
    private TrackIRTracker _trackIrTracker;

    [NonSerialized]
    private float _timeSinceLastStaleFrameS;

    [NonSerialized]
    private Vector3 _posePosition = Vector3.zero;

    [NonSerialized]
    private Vector3 _poseRotation = Vector3.zero;

    [NonSerialized]
    private ulong _staleFrames;

    #endregion

    private void Awake()
    {
        _positionTransform = GameObject.Find("/PLAYER/Pivot/AnimPivot").transform;

        Transform fpsCamera = GameObject.Find("/PLAYER/Pivot/AnimPivot/Camera/FPSCamera").transform;
        _mouseLookWithModifiers = fpsCamera.gameObject.AddComponent<MouseLookWithModifiers>();

        _currentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");
    }


    private void OnEnable()
    {
        try
        {
            _trackIrTracker = new TrackIRTracker();
        }
        catch (Exception ex)
        {
            ModConsole.LogWarning($"TrackIR failed to load: {ex}");
        }
    }

    private void Update()
    {
        if (_trackIrTracker == null)
            return;

        bool shouldTrack = Settings.TrackingOutsideOfVehicles.GetValue() || _currentVehicle.Value.Length > 0;

        if (shouldTrack)
        {
            UpdatePose();

            // Should we track and do we have up-to-date data?
            if (_staleFrames == 0)
            {
                LocalPosition = _posePosition;
                LocalEulerAngles = _poseRotation;
                _timeSinceLastStaleFrameS = 0.0f;
                return;
            }

            // Should we track but shouldn't recenter
            if (!Settings.ShouldRecenterIfLostTracking.GetValue())
                return;
        }

        // Update time since last stale pose data
        _timeSinceLastStaleFrameS += Time.deltaTime;

        float lostTrackingTimeoutS = shouldTrack
            ? Settings.LostTrackingTimeoutSeconds.GetValue()
            : 0; // Force to start re-centering instantly
        float lostTrackingRecenterDurationS = Settings.LostTrackingRecenterDurationSeconds.GetValue();

        // detect if stale pose data is old enough to recenter
        if (_timeSinceLastStaleFrameS <= lostTrackingTimeoutS)
            return;

        // Recenter the camera if data is stale
        float recenterPercentage = Mathf.Clamp01((_timeSinceLastStaleFrameS - lostTrackingTimeoutS) /
                                                 lostTrackingRecenterDurationS);
        recenterPercentage = Mathf.SmoothStep(0.0f, 1.0f, recenterPercentage);
        LocalPosition = Vector3.Lerp(_posePosition, Vector3.zero, recenterPercentage);
        LocalEulerAngles = Vector3.Lerp(_poseRotation, Vector3.zero, recenterPercentage);
    }

    private void UpdatePose()
    {
        if (_trackIrTracker == null)
            return;

        try
        {
            _trackIrTracker.GetData(ref _poseRotation, ref _posePosition, ref _staleFrames);
        }
        catch (Exception ex)
        {
            ModConsole.LogWarning($"TrackIR failed to update Pose: {ex}");
        }

        _poseRotation = new Vector3(
            Settings.EnablePitch.GetValue() ? -_poseRotation.x : 0,
            Settings.EnableYaw.GetValue() ? _poseRotation.y : 0,
            Settings.EnableRoll.GetValue() ? -_poseRotation.z : 0
        );
        _posePosition = new Vector3(
            Settings.EnableX.GetValue() ? _posePosition.x : 0,
            Settings.EnableY.GetValue() ? _posePosition.y : 0,
            Settings.EnableZ.GetValue() ? -_posePosition.z : 0
        );
    }

    #region Tracking shutdown handling

    private void OnDisable() => DisableTrackingAndResetTransform();

    private void OnDestroy() => DisableTrackingAndResetTransform();

    private void OnApplicationQuit() => DisableTrackingAndResetTransform();

    private void DisableTrackingAndResetTransform()
    {
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        _trackIrTracker?.Stop();
    }

    #endregion
}