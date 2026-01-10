using System.Diagnostics.CodeAnalysis;
using Harmony;
using MSCLoader;
using UnityEngine;

namespace MyHeadTrackingCar;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class MyHeadTrackingCarMod : Mod
{
    #region Mod definition

    public override string ID => "myheadtrackingcar";
    public override string Name => "My Head Tracking Car";
    public override string Author => "icdb / cineafx";
    public override string Version => "0.2";
    public override string Description => "Adds the TrackIR SDK";
    //public override byte[] Icon => Properties.Resources.icon;

    public override Game SupportedGames => Game.MySummerCar_And_MyWinterCar;

    #endregion

    private readonly MyHeadTrackingCarSettings _modSettings = new();

    #region ModLoader Hooks

    public override void ModSetup()
    {
        base.ModSetup();

        HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.cineafx.myheadtrackingcar.patch");
        harmonyInstance.PatchAll();

        SetupFunction(Setup.ModSettings, _modSettings.SetupModSettings);
        SetupFunction(Setup.OnLoad, OnLoad);
        SetupFunction(Setup.OnModEnabled, OnModEnabled);
        SetupFunction(Setup.OnModDisabled, OnModDisabled);
    }

    private void OnLoad()
    {
        SetupHeadTracking();
    }

    private void OnModEnabled()
    {
        if (_trackIrComponent != null)
            _trackIrComponent.enabled = true;
        else
            SetupHeadTracking();
    }

    private void OnModDisabled()
    {
        if (_trackIrComponent != null)
            _trackIrComponent.enabled = false;
    }

    #endregion

    #region HeadTracking setup

    private MyHeadTrackingCarComponent _trackIrComponent;

    private void SetupHeadTracking()
    {
        GameObject playerGameObj = GameObject.Find("/PLAYER");
        _trackIrComponent = playerGameObj.AddComponent<MyHeadTrackingCarComponent>();
        _trackIrComponent.Settings = _modSettings;
    }

    #endregion
}