using MSCLoader;

namespace MyHeadTrackingCar;

public class MyHeadTrackingCarSettings
{
    // General
    public SettingsCheckBox TrackingOutsideOfVehicles { get; set; }
    public SettingsCheckBox ForceMouseOnHorizonLevelInVehicle { get; set; }
    public SettingsCheckBox ForceMouseOnHorizonLevelOnFoot { get; set; }

    // Tracking configuration
    public SettingsCheckBox EnablePitch { get; set; }
    public SettingsCheckBox EnableYaw { get; set; }
    public SettingsCheckBox EnableRoll { get; set; }
    public SettingsCheckBox EnableX { get; set; }
    public SettingsCheckBox EnableY { get; set; }
    public SettingsCheckBox EnableZ { get; set; }

    // Lost tracking
    public SettingsCheckBox ShouldRecenterIfLostTracking { get; set; }
    public SettingsSlider LostTrackingTimeoutSeconds { get; set; }
    public SettingsSlider LostTrackingRecenterDurationSeconds { get; set; }

    internal void SetupModSettings()
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