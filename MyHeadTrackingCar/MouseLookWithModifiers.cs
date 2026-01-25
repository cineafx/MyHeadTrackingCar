using System.Diagnostics.CodeAnalysis;
using Harmony;
using UnityEngine;

namespace MyHeadTrackingCar;

[HarmonyPatch(typeof(MouseLook), "Update")]
[HarmonyPriority(Priority.High)]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MouseLookUpdatePatch
{
    public static bool Prefix(ref MouseLook __instance)
    {
        // If the mod is not active and therefore MouseLookWithModifiers was never added,
        // the original Update function needs to run.
        // We only overwrite the MouseY version used for Tilt.
        // The MouseX version is used for Yaw, which we do not interfere with.
        // I don't like the static ModActive, but the alternative would be checking with every update
        // whether a MouseLookWithModifiers is present in the same GameObject which hurts performance
        return !MyHeadTrackingCarMod.ModActive || __instance.axes != MouseLook.RotationAxes.MouseY;
    }
}

public class MouseLookWithModifiers : MonoBehaviour
{
    private MouseLook? _sourceMouseLook;

    public float rotationY;

    public bool OriginalMouseLookEnabled => _sourceMouseLook?.enabled ?? false;

    public Vector3 additionalRotation = Vector3.zero;
    public bool blockMouseTilt;

    private void Update()
    {
        if (!OriginalMouseLookEnabled || _sourceMouseLook == null)
            return;

        if (_sourceMouseLook.axes == MouseLook.RotationAxes.MouseXAndY)
        {
            float y = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * _sourceMouseLook.sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * _sourceMouseLook.sensitivityY;
            rotationY = Mathf.Clamp(rotationY, _sourceMouseLook.minimumY, _sourceMouseLook.maximumY);
            transform.localEulerAngles = new Vector3(-rotationY, y, 0.0f);
        }
        else if (_sourceMouseLook.axes == MouseLook.RotationAxes.MouseX)
        {
            transform.Rotate(0.0f, Input.GetAxis("Mouse X") * _sourceMouseLook.sensitivityX, 0.0f);
        }
        else
        {
            if (blockMouseTilt)
                rotationY = 0.0f;
            else
                rotationY += Input.GetAxis("Mouse Y") * _sourceMouseLook.sensitivityY;
            rotationY = Mathf.Clamp(rotationY, _sourceMouseLook.minimumY, _sourceMouseLook.maximumY);
            transform.localEulerAngles = new Vector3(-rotationY, 0.0f, 0.0f) +
                                         additionalRotation;
        }
    }

    private void Start()
    {
        _sourceMouseLook = transform.GetComponent<MouseLook>();
    }
}