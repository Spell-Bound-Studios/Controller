// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Interface for providing camera input.
    /// Implement this interface in your game's input system.
    /// </summary>
    public interface ICameraInput {
        /// <summary>
        /// Current look direction input (typically mouse delta or right stick).
        /// </summary>
        Vector3 LookDirection { get; }

        /// <summary>
        /// Fired when the mouse wheel scrolls.
        /// </summary>
        event Action<Vector2> OnMouseWheelInput;
    }
}