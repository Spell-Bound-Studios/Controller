BaseSoState: Modular ScriptableObject states with lifecycle methods
State Driver: Constant POCO that "drives" the current variant of a state type
Context (Ctx): User-defined data container passed to states
State Machine: Manages drivers and handles state/variant changes
Variants: Different implementations of the same logical state (e.g., GroundedState vs ModifiedGroundedState)