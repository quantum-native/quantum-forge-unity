0.1.1 Update quantum-forge.dll to target Windows SDK 10.0.17763.0

0.1.2 Update quantum-forge.dll to Release target Windows SDK 10.0.18362.0

0.1.3 Update quantum-forge.dll to Release target Windows SDK 8.1

0.1.4 Fix LICENSE typo, and add lecture sample to Samples

0.1.5 Add FractionalISwap (2 properties), Swap (2 properties), ControlledCycle (2 properties) and ControlledSwap (3 properties) Actions. Controls are the first property in the list. ControlValue is the index of the basis value for the QuantumProperty basis of the control.

0.2.0 Breaking change: Rename BasisValues to Basis.
    Includes webGL library for Unity 2022.3
    Other renames and QOL fixes.

0.2.1 Fixed broken samples.

1.0.0 First release.

1.0.1 Fixed bug with fractional shift, and added test.

1.0.2 Add MacOS support.

1.0.3 Update MacOS, Linux, and WebGL support.

1.0.4 Change Windows dll to release instead of debug binary.

1.0.5 Fix bug with predicated measurement.

1.0.6 Add Documentation.

1.0.7 Fix bug with MeasurePredicates Monobehaviour.

1.0.8 Fixes for EntanglementTracker.

1.1.0 Change quantum-forge.dll to support 16 qudits of dimension 4.

1.1.1 Add Branding prefab, and branding to Actions sample.

1.1.2 Added copyright notices, and removed unused code.

1.1.3 Fix missing ReflectionSystem dependency.

1.1.4 Improve runtime parity with framework operations:
    - Add `QuantumForge.Reset(...)` and `QuantumProperty.Reset(...)`.
    - Add predicate-aware `QuantumProperty.ISwap(...)` overloads and wire ISwap action predicates.
    - Add `QuantumProperty.MeasureProperties(...)` / `MeasurePredicate(...)` helper aliases.
    - Add optional fractional Hadamard overload with explicit legacy-plugin limitation error.
    - Fix `QuantumProperty.is_not_value(int)` and update measurement actions to match framework semantics.
