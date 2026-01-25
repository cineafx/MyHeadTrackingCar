using System.Diagnostics.CodeAnalysis;
using Harmony;
using MSCLoader;
using UnityEngine;

namespace MyHeadTrackingCar;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MyHeadTrackingCarMod : Mod
{
    /// <summary>
    /// Const define that is used for <see cref="Mod.Version"/> and AssemblyFileVersion.<br/>
    /// AssemblyFileVersion will automatically add an additional <c>.0</c> revision number when building.
    /// </summary>
    public const string VersionConst = "0.2.3";

    #region Mod definition

    public override string ID => "myheadtrackingcar";
    public override string Name => "My Head Tracking Car";
    public override string Author => "icdb / cineafx";
    public override string Version => VersionConst;
    public override string Description => "Adds the TrackIR SDK";
    //public override byte[] Icon => Properties.Resources.icon;

    public override Game SupportedGames => Game.MySummerCar_And_MyWinterCar;

    #endregion

    private MyHeadTrackingCarSettings? MyHeadTrackingCarSettings { get; set; }

    /// <summary>
    /// Property which mirrors the ModLoader enable setting for this mod.
    /// <remarks>I'm not happy with this being static, but I'm not sure how else
    /// <see cref="MouseLookUpdatePatch"/> could access it without performance regression.</remarks>
    /// </summary>
    public static bool ModActive { get; private set; }

    #region ModLoader Hooks

    public override void ModSetup()
    {
        base.ModSetup();

        HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.cineafx.myheadtrackingcar.patch");
        harmonyInstance.PatchAll();

        SetupFunction(Setup.ModSettings, OnModSettings);
        SetupFunction(Setup.OnLoad, OnLoad);
        SetupFunction(Setup.OnModEnabled, OnModEnabled);
        SetupFunction(Setup.OnModDisabled, OnModDisabled);
    }

    private void OnModSettings()
    {
        MyHeadTrackingCarSettings = new MyHeadTrackingCarSettings();
    }

    private void OnLoad()
    {
        SetupHeadTracking();
    }

    private void OnModEnabled()
    {
        ModActive = true;
        if (_trackIrComponent != null)
            _trackIrComponent.enabled = true;
        else if (ModLoader.CurrentScene == CurrentScene.Game)
            SetupHeadTracking();
    }

    private void OnModDisabled()
    {
        ModActive = false;
        if (_trackIrComponent != null)
            _trackIrComponent.enabled = false;
    }

    #endregion

    #region HeadTracking setup

    private MyHeadTrackingCarComponent? _trackIrComponent;

    private void SetupHeadTracking()
    {
        GameObject? playerGameObj = GameObject.Find("/PLAYER");
        if (playerGameObj == null)
        {
            // Most likely trying to set up head tracking while still in the main menu
            ModConsole.Warning("Trying to setup head tracking while no PLAYER object exists.");
            return;
        }

        // If head tracking is still active on the old game object this will reset everything.
        // This should realistically never happen and is just a fallback.
        _trackIrComponent?.enabled = false;

        if (MyHeadTrackingCarSettings == null)
        {
            ModConsole.Error("Trying to setup head tracking before ModSettings SetupFunction ran.");
            return;
        }

        _trackIrComponent = playerGameObj.AddComponent<MyHeadTrackingCarComponent>();
        _trackIrComponent.Settings = MyHeadTrackingCarSettings;
    }

    #endregion
}