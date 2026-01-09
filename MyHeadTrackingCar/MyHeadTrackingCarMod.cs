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

    private readonly MyHeadTrackingCarSettings _settings = new();

    #region ModLoader Hooks

    public override void ModSetup()
    {
        base.ModSetup();

        HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.cineafx.myheadtrackingcar.patch");
        harmonyInstance.PatchAll();

        SetupFunction(Setup.ModSettings, ModSettings);
        SetupFunction(Setup.OnLoad, OnLoad);
        SetupFunction(Setup.OnModEnabled, OnModEnabled);
        SetupFunction(Setup.OnModDisabled, OnModDisabled);
    }

    private void ModSettings()
    {
        Settings.AddHeader("General");
        _settings.TrackingOutsideOfVehicles = Settings.AddCheckBox(
            "headTrackingOutsideOfVehicles",
            "Head tracking outside of vehicles",
            true
        );
        _settings.ForceMouseOnHorizonLevelInVehicle = Settings.AddCheckBox(
            "forceMouseOnHorizonLevelInVehicle",
            "Force mouse to horizon level in vehicles",
            false
        );
        _settings.ForceMouseOnHorizonLevelOnFoot = Settings.AddCheckBox(
            "forceMouseOnHorizonLevelOnFoot",
            "Force mouse to horizon level on foot",
            false
        );

        Settings.CreateGroup();
        Settings.AddHeader("Tracking configuration");
        _settings.EnablePitch = Settings.AddCheckBox(
            "enablePitch",
            "Enable pitch",
            true
        );
        _settings.EnableYaw = Settings.AddCheckBox(
            "enableYaw",
            "Enable yaw",
            true
        );
        _settings.EnableRoll = Settings.AddCheckBox(
            "enableRoll",
            "Enable roll",
            true
        );
        _settings.EnableX = Settings.AddCheckBox(
            "enableX",
            "Enable x (left <--> right)",
            true
        );
        _settings.EnableY = Settings.AddCheckBox(
            "enableY",
            "Enable y (up <--> down)",
            true
        );
        _settings.EnableZ = Settings.AddCheckBox(
            "enableZ",
            "Enable z (front <--> back)",
            true
        );

        Settings.CreateGroup();
        Settings.AddHeader("Lost tracking");
        _settings.ShouldRecenterIfLostTracking = Settings.AddCheckBox(
            "shouldRecenterIfLostTracking",
            "Should recenter camera if tracking is lost",
            true
        );
        _settings.LostTrackingTimeoutSeconds = Settings.AddSlider(
            "lostTrackingTimeoutS",
            "Lost tracking re-centering delay (seconds)",
            0.2f, 5.0f, 1.0f,
            null, 2);
        _settings.LostTrackingRecenterDurationSeconds = Settings.AddSlider(
            "lostTrackingRecenterDurationS",
            "Re-centering duration (seconds)",
            0.0f, 5.0f, 0.5f,
            null, 2);
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
        _trackIrComponent.Settings = _settings;
    }

    #endregion
}