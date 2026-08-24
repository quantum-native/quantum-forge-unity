# Changelog

All notable changes to the Quantum Forge Unity Package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.4.0] - 2026-08-21

### ⚠️ Read this before upgrading

**Every previously released version of this package was non-functional.** This is not a
routine update — it is the first release in which the package compiles at all and in which
quantum calls actually reach the native plugin.

If you are on 1.3.0 or earlier, you were not hitting "a bug". Two independent defects meant
nothing worked:

1. **The package did not compile.** `Runtime/QuantumProperty.cs` called `QuantumForge.X(...)`,
   `.Y(...)` and `.Z(...)` in six places. Those methods did not exist in the package's own
   `Runtime/Core/QuantumForge.cs`, so Unity reported `CS0117: 'QuantumForge' does not contain
   a definition for 'X'` on import. (The 1.2.0 entry below claims "Pauli X, Y, Z gate aliases
   across all layers" — they were added to the monorepo binding, but never reached this
   package.)
2. **Every quantum call would have thrown `EntryPointNotFoundException` regardless.** The
   package's binding P/Invoked 14 old-style native symbols (`qforge_make_quantum_property`,
   `qforge_cycle`, `qforge_measure`, ...) that the C API had since renamed
   (`qforge_quantum_property_create`, `qforge_cycle_operation`, `qforge_measure_properties`,
   ...). No `EntryPoint=` overrides bridged the gap. A `DllImport` is resolved only on first
   invocation, so no compiler could have caught it.

### Root cause

`wrappers/unity/Core/QuantumForge.cs` (monorepo) and `unity-package/Runtime/Core/QuantumForge.cs`
(shipped) were two hand-maintained copies of the same binding. The monorepo copy tracked the
C API — 33 `DllImport`s, X/Y/Z present. The shipped copy did not — 14 `DllImport`s, no X/Y/Z.
Nothing in the build related the two files, so the divergence was invisible and the working
binding was never distributed.

### Fixed

- `Runtime/Core/QuantumForge.cs` is now **generated** from the monorepo source of truth
  (`wrappers/unity/Core/`) by `scripts/sync-unity-package-bindings.sh`. It is no longer
  maintained by hand and must not be edited directly.
- All 33 `DllImport` entry points now match the current C API, so calls resolve instead of
  throwing `EntryPointNotFoundException`.
- `QuantumForge.X` / `.Y` / `.Z` now exist, so the package compiles. Verified by compiling
  every source under `Runtime/` with plain .NET.
- `QuantumForge.Reset(prop, currentValue)` — a pure-C# helper that existed only in the old
  shipped copy — was carried forward into the source of truth rather than being lost.
- **`Probabilities()` returned 0 for every basis state**, and `ReducedDensityMatrix()`
  returned an all-zero matrix. The output buffer was passed as a managed struct array, which
  .NET marshals `[In]`-only, so everything native wrote was discarded on return. Both now use
  explicit unmanaged buffers, which behaves the same under Mono and IL2CPP.
- Added the fractional Hadamard overload `Hadamard(prop, fraction, ...)`. The C API has
  declared `qforge_fractional_hadamard_operation` since the batch-ops release, but the binding
  never bound it — so `QuantumProperty.Hadamard(prop, fraction, ...)` had nothing to call.
- **Predicate handles no longer leak.** Predicates are native handles in the current API; the
  package created one per predicate per gate call and never released them. All 21 call sites
  now scope them deterministically.
- Unity detection now uses `UNITY_5_3_OR_NEWER` instead of
  `UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL`, which silently excluded iOS and Android.
- **The native plugin files are now named what the binding actually asks for.** This release
  moved the binding to per-platform library names (`quantum-forge-macOS`,
  `quantum-forge-Windows`, `quantum-forge-Linux`), but the committed plugins kept the old
  shared name (`quantum-forge.bundle`, `quantum-forge.dll`, `libquantum-forge.so`). Unity
  resolves a native plugin by the name in its `DllImport`, so on every platform the first
  quantum call threw `DllNotFoundException`. The files under `Runtime/Plugins/` are now
  `MacOS/quantum-forge-macOS.bundle`, `x86-64/quantum-forge-Windows.dll`,
  `Linux/libquantum-forge-Linux.so` and `WebGL/libquantum-forge-WebGL.a`, matching both the
  binding and the names the CMake build emits. `.meta` GUIDs were carried across the rename,
  so existing plugin import settings are preserved.
- **macOS plugin rebuilt against the current C API.** The shipped bundle exported 33
  `qforge_*` symbols and predated `qforge_execute_batch`, so `QuantumForge.ExecuteBatch(...)`
  threw `EntryPointNotFoundException`. The rebuilt universal (x86_64 + arm64) bundle exports
  38 and resolves all 34 `DllImport` entry points. It is built with
  `QUANTUM_FORGE_ALLOW_DYNAMIC_FALLBACK=OFF` (dimension 3, 12 qudits), so it also carries the
  dynamic-fallback hard lock the previous binaries were missing.
- `Utility/ScaleProbability` never scaled anything. It matched the tracked basis value with
  `==`, and `BasisValue` is a class with no `operator==`, so it compared object references —
  and the value serialized on the component is always a different instance from the one held
  by the `Basis` asset. It now compares by value, consistent with `BasisValue.Equals`.
- `QuantumPropertyTrigger` and `Utility/Trigger` both exposed an `onTriggerExit` event in the
  inspector that nothing ever raised: neither component implemented `OnTriggerExit`. Both now
  handle exit symmetrically with enter, in 2D and 3D.

### Gate semantics (unchanged in intent, now actually reachable)

Matching the C++ core and the TypeScript binding:
- `X` is **shift**, not cycle. The two coincide only at dimension 2; for d > 2 they are
  inverses of each other.
- `Z` is **clock**.
- `Y` is qubit-only, composed as `clock(-0.5); shift; clock(0.5)`, and errors on dimension != 2.

### Known issue — three of the four native plugins are still stale

Only macOS could be rebuilt on the release machine. The other three were renamed to the
filenames the binding loads, but their contents are unchanged:

| Platform | State |
|---|---|
| macOS (`quantum-forge-macOS.bundle`) | **Rebuilt.** Current ABI, 38 exports, hard-locked, universal. |
| Linux (`libquantum-forge-Linux.so`) | Old 14-symbol ABI, unlocked. Unusable. |
| Windows (`quantum-forge-Windows.dll`) | Old 14-symbol ABI, unlocked. Unusable. |
| WebGL (`libquantum-forge-WebGL.a`) | Old 14-symbol ABI, unlocked. Unusable. |

Those three need a rebuild from the `unity-wrapper.yml` hard-lock legs, which now publish each
one as a `unity-plugin-upm-<platform>` artifact rooted at its exact `Runtime/Plugins/` path and
filename — so replacing them is a copy, not a hand-rename. Until that lands, only macOS works.
CI fails loudly on it (`enforce-plugin-abi`, `enforce-plugin-lock`) instead of shipping it
silently.

### Added (CI, monorepo-side)

- `scripts/check-unity-package-synced.sh` — fails if the shipped binding drifts from the
  source of truth.
- `scripts/check-unity-dllimport-symbols.sh` — fails if any `DllImport` entry point is
  missing from the C API header, or (with `--committed-plugins`) from the shipped binaries.
- `scripts/check-unity-package-compiles.sh` — compiles `Runtime/` with plain .NET.
- `unity-wrapper.yml` asserts each built plugin carries the filename the binding P/Invokes,
  and stages it at its `Runtime/Plugins/` destination, so recommitting a plugin is a copy
  rather than a hand-rename. Its packaging step also no longer copies from directories CMake
  never writes to, which used to yield an artifact containing no native code at all.
- The release path now assembles the shipped package and every per-platform download from the
  **hard-locked** artifacts, covering all four platforms, and fails rather than publishing a
  partial set. It previously built from the ordinary CI legs, which compile with the dynamic
  fallback ON and have no WebGL leg — so a release shipped unlocked binaries and no WebGL.
- The generated package manifest derives its version and Unity compatibility from
  `unity-package/package.json` instead of hardcoding 1.0.0 / `0b1`.

### Documentation

- README now states the Unity version `package.json` actually enforces (2022.3.5f1, via
  `unityRelease`), not 2022.3.1f1.

## [1.3.0] - 2026-03

### Changed
- Rebrand to Quantum Native; update package metadata and URLs
- Switch to split licensing model: MIT (C# source) + proprietary (native binaries)
- UPM package moved to public repo: `quantum-native/quantum-forge-unity`
- Package ID changed from `com.quantumrealmgames.quantum-forge` to `com.qrg.quantumforge`
- macOS plugin rebuilt and code-signed

### Fixed
- Error enum values now match C API error codes (prevents silent error mishandling)

## [1.2.0] - 2026-02

### Added
- Pauli X, Y, Z gate aliases across all layers
- `QUANTUM_FORGE_ALLOW_DYNAMIC_FALLBACK` compile-time flag for binary builds
- Dynamic fallback disabled in shipped binaries (hard limit enforcement)

### Changed
- Native binaries rebuilt with dimension 3, 12 qudits configuration

## [1.1.4] - 2025

### Added
- `QuantumForge.Reset(...)` and `QuantumProperty.Reset(...)`
- Predicate-aware `QuantumProperty.ISwap(...)` overloads and ISwap action predicates
- `QuantumProperty.MeasureProperties(...)` / `MeasurePredicate(...)` helper aliases
- Optional fractional Hadamard overload with explicit legacy-plugin limitation error

### Fixed
- `QuantumProperty.is_not_value(int)` and measurement actions to match framework semantics

## [1.1.3] - 2025

### Fixed
- Missing ReflectionSystem dependency

## [1.1.2] - 2025

### Added
- Copyright notices to source files

### Removed
- Unused code

## [1.1.1] - 2025

### Added
- Branding prefab and branding to Actions sample

## [1.1.0] - 2025

### Changed
- Native library updated to support 16 qudits of dimension 4

## [1.0.8] - 2025

### Fixed
- EntanglementTracker issues

## [1.0.7] - 2025

### Fixed
- Bug with MeasurePredicates MonoBehaviour

## [1.0.6] - 2024

### Added
- Documentation

## [1.0.5] - 2024

### Fixed
- Bug with predicated measurement

## [1.0.4] - 2024

### Changed
- Windows DLL switched to release instead of debug binary

## [1.0.3] - 2024

### Added
- Updated macOS, Linux, and WebGL support

## [1.0.2] - 2024

### Added
- macOS support

## [1.0.1] - 2024

### Fixed
- Bug with fractional shift; added test

## [1.0.0] - 2024

First release.

## [0.2.1] - 2024

### Fixed
- Broken samples

## [0.2.0] - 2024

### Changed
- **Breaking:** Rename BasisValues to Basis
- Includes WebGL library for Unity 2022.3
- Other renames and QoL fixes

## [0.1.5] - 2024

### Added
- FractionalISwap, Swap, ControlledCycle, and ControlledSwap Actions

## [0.1.4] - 2024

### Fixed
- LICENSE typo

### Added
- Lecture sample to Samples

## [0.1.3] - 2024

### Changed
- Target Windows SDK 8.1

## [0.1.2] - 2024

### Changed
- Target Windows SDK 10.0.18362.0 (Release)

## [0.1.1] - 2024

### Changed
- Target Windows SDK 10.0.17763.0
