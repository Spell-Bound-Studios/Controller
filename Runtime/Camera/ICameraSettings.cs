// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Controller {
    /// <summary>
    /// Runtime-tunable look/feel settings the camera reads each frame. Consumers implement this (or use the
    /// sample's serialized data container) so any settings UI can drive the camera live.
    /// </summary>
    public interface ICameraSettings {
        float SensitivityX { get; set; }
        float SensitivityY { get; set; }
        bool InvertY { get; set; }
        bool SmoothLook { get; set; }
        float SmoothingFactor { get; set; }
        float MinPitch { get; set; }
        float MaxPitch { get; set; }
    }
}
