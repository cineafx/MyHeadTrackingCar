using MSCLoader;

namespace MyHeadTrackingCar;

public class MyHeadTrackingCarSettings
{
    // General
    public SettingsCheckBox TrackingOutsideOfVehicles { get; }
    public SettingsCheckBox ForceMouseOnHorizonLevelInVehicle { get; }
    public SettingsCheckBox ForceMouseOnHorizonLevelOnFoot { get; }

    // Tracking configuration
    public SettingsCheckBox EnablePitch { get; }
    public SettingsCheckBox EnableYaw { get; }
    public SettingsCheckBox EnableRoll { get; }
    public SettingsCheckBox EnableX { get; }
    public SettingsCheckBox EnableY { get; }
    public SettingsCheckBox EnableZ { get; }

    // Lost tracking
    public SettingsCheckBox ShouldRecenterIfLostTracking { get; }
    public SettingsSlider LostTrackingTimeoutSeconds { get; }
    public SettingsSlider LostTrackingRecenterDurationSeconds { get; }

    public MyHeadTrackingCarSettings()
    {
        Settings.AddHeader("General");
        TrackingOutsideOfVehicles = Settings.AddCheckBox(
            "headTrackingOutsideOfVehicles",
            "Head tracking outside of vehicles",
            true
        );
        ForceMouseOnHorizonLevelInVehicle = Settings.AddCheckBox(
            "forceMouseOnHorizonLevelInVehicle",
            "Force mouse to horizon level in vehicles",
            false
        );
        ForceMouseOnHorizonLevelOnFoot = Settings.AddCheckBox(
            "forceMouseOnHorizonLevelOnFoot",
            "Force mouse to horizon level on foot",
            false
        );

        Settings.CreateGroup();
        Settings.AddHeader("Tracking configuration");
        EnablePitch = Settings.AddCheckBox(
            "enablePitch",
            "Enable pitch",
            true
        );
        EnableYaw = Settings.AddCheckBox(
            "enableYaw",
            "Enable yaw",
            true
        );
        EnableRoll = Settings.AddCheckBox(
            "enableRoll",
            "Enable roll",
            true
        );
        EnableX = Settings.AddCheckBox(
            "enableX",
            "Enable x (left <--> right)",
            true
        );
        EnableY = Settings.AddCheckBox(
            "enableY",
            "Enable y (up <--> down)",
            true
        );
        EnableZ = Settings.AddCheckBox(
            "enableZ",
            "Enable z (front <--> back)",
            true
        );

        Settings.CreateGroup();
        Settings.AddHeader("Lost tracking");
        ShouldRecenterIfLostTracking = Settings.AddCheckBox(
            "shouldRecenterIfLostTracking",
            "Should recenter camera if tracking is lost",
            true
        );
        LostTrackingTimeoutSeconds = Settings.AddSlider(
            "lostTrackingTimeoutS",
            "Lost tracking re-centering delay (seconds)",
            0.2f, 5.0f, 1.0f,
            null, 2);
        LostTrackingRecenterDurationSeconds = Settings.AddSlider(
            "lostTrackingRecenterDurationS",
            "Re-centering duration (seconds)",
            0.0f, 5.0f, 0.5f,
            null, 2);
    }
}