# Quantum Forge API Reference

Welcome to the Quantum Forge API Reference. There are two levels at which the user can interact with the Quantum Forge API:
1. The [Quantum Forge Core API](QRG.QuantumForge.Core.yml): This is a C# wrapper for the underlying Quantum Forge c-api, and provides a code interface for interacting with the underlying quantum state simulator.
2. The [Quantum Forge Runtime API](QRG.QuantumForge.Runtime.yml): This is a set of Monobehaviour wrappers that can be used to interact with the Quantum Forge Core API from within he Unity Editor.

## Quantum Forge Core API
The Quantum Forge Core API is designed around the idea of the [NativeQuantumProperty](QRG.QuantumForge.Core.QuantumForge.NativeQuantumProperty.yml), which is a hook into the underlying quantum state simulator. The [QuantumForge](QRG.QuantumForge.Core.QuantumForge.yml) static class provides a set of methods for creating and manipulating NativeQuantumProperties. A [QuantumForge.Predicate](QRG.QuantumForge.Core.QuantumForge.Predicate.yml) can be used in many of the Quantum Forge Core API methods to specify the conditions under which NativeQuantumProperties should be affected by the operation. Note, this creates a controlled operation and can be used to generate entanglement.

## Quantum Forge Runtime API
The Quantum Forge Runtime API provides [QuantumProperty](QRG.QuantumForge.Runtime.QuantumProperty.yml), a Monobehaviour wrapper for the native quantum property.  The [Basis](QRG.QuantumForge.Runtime.Basis.yml) can be created from the Unity `Assets/Create/Quantum` menu.The runtume api also provides a set of `Actions` that can be applied to a quantum property to manipulate its state, and a set of `Trackers` that can be used to gather certain information about the quantum state.

### Actions
Actions are used to manipulate the state of a quantum property. There are several types of actions that can be applied to a quantum property:

- [Hadamard](QRG.QuantumForge.Runtime.Hadamard.yml): **This creates superposition from a classical state.**
- [Clock](QRG.QuantumForge.Runtime.Clock.yml): Details about the `Clock` action.
- [Cycle](QRG.QuantumForge.Runtime.Cycle.yml): Information on the `Cycle` action.
- [InverseHadamard](QRG.QuantumForge.Runtime.InverseHadamard.yml): Details about the `InverseHadamard` action.
- [Swap](QRG.QuantumForge.Runtime.Swap.yml): Explore the `Swap` action.
- [PhaseRotate](QRG.QuantumForge.Runtime.PhaseRotate.yml): Understand the `PhaseRotate` action.
- [MeasurePredicates](QRG.QuantumForge.Runtime.MeasurePredicates.yml): Learn about the `MeasurePredicates` action.
- [MeasureProperties](QRG.QuantumForge.Runtime.MeasureProperties.yml): Explore the `MeasureProperties` action.
- [NCycle](QRG.QuantumForge.Runtime.NCycle.yml): Details about the `NCycle` action.
- [Shift](QRG.QuantumForge.Runtime.Shift.yml): Understand the `Shift` action.
- [ISwap](QRG.QuantumForge.Runtime.ISwap.yml): Learn about the `ISwap` action.

### Trackers
Trackers are used to gather information about the quantum state. 

- [CorrelationTracker](QRG.QuantumForge.Runtime.CorrelationTracker.yml): Learn about the `CorrelationTracker`.
- [EntanglementTracker](QRG.QuantumForge.Runtime.EntanglementTracker.yml): Explore the `EntanglementTracker`.
- [PhaseTracker](QRG.QuantumForge.Runtime.PhaseTracker.yml): Understand the `PhaseTracker`.
- [ProbabilityTracker](QRG.QuantumForge.Runtime.ProbabilityTracker.yml): Details about the `ProbabilityTracker`.