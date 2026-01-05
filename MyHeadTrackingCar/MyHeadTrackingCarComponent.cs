using System;
using JetBrains.Annotations;
using MSCLoader;
using UnityEngine;

namespace MyHeadTrackingCar;

public class MyHeadTrackingCarComponent : MonoBehaviour
{
    public ushort applicationId = 20430; // Default = Unity 3d plugin

    #region Configurable properties

    public bool shouldRecenterIfLostTracking = true;

    public float lostTrackingTimeoutS = 1.0f;

    public float lostTrackingRecenterDurationS = 0.5f;

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
                _mouseLookWithModifiers.additionalRotation = new Vector3(value.x, value.y, value.z);
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

        //_yawTransform = GameObject.Find("/PLAYER/Pivot/AnimPivot").transform;
        Transform fpsCamera = GameObject.Find("/PLAYER/Pivot/AnimPivot/Camera/FPSCamera").transform;
        _mouseLookWithModifiers = fpsCamera.gameObject.AddComponent<MouseLookWithModifiers>();
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

        try
        {
            _trackIrTracker.GetData(ref _poseRotation, ref _posePosition, ref _staleFrames);
        }
        catch (Exception ex)
        {
            ModConsole.LogWarning($"TrackIR failed to update Pose: {ex}");
        }

        _poseRotation = new Vector3(-_poseRotation.x, _poseRotation.y, -_poseRotation.z);
        _posePosition = new Vector3(_posePosition.x, _posePosition.y, -_posePosition.z);

        ModConsole.Log($"Pose rotation: {_poseRotation}");

        if (_staleFrames == 0)
        {
            LocalPosition = _posePosition;
            LocalEulerAngles = _poseRotation;
            _timeSinceLastStaleFrameS = 0.0f;
            return;
        }

        if (!shouldRecenterIfLostTracking)
            return;

        // Update time since last stale pose data
        _timeSinceLastStaleFrameS += Time.deltaTime;

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