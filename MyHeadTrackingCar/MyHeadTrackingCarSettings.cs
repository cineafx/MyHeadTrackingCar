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
}