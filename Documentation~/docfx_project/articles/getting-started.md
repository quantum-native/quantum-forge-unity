# Getting Started with <span class="brand-font">Quantum Forge</span>

This guide will help you quickly integrate quantum behavior into your Unity projects using the <span class="brand-font">Quantum Forge</span> package.

## Why Quantum Mechanics in Games?

Quantum Forge gives you real quantum effects — superposition, entanglement, and interference — as drag-and-drop Unity components. These aren't random number generators dressed up with physics terminology. They're actual quantum operations running in a compiled native simulator.

The difference matters for game design: quantum mechanics produces **structured, emergent behavior**. Interference creates patterns. Entanglement creates correlations across your game. Players discover strategies you never planned. In Quantum Chess, players found interference effects that the developers initially mistook for bugs — that's the kind of emergent gameplay quantum mechanics enables.

In 2024, Caltech IQIM and LCAD ran a quantum game jam using Quantum Forge. Student teams with no prior quantum experience built playable quantum games in one week. If you know Unity, you can learn to use quantum mechanics. For a plain-language introduction to the concepts, see [Quantum Concepts for Game Developers](quantum-concepts.md).

## Installation

<span class="brand-font">Quantum Forge</span> currently supports Unity 2022.3.5f1 or later, and can be used to build for Windows, Mac, Linux, and WebGL.

To install the <span class="brand-font">Quantum Forge</span> Unity Package:

1. Open the Unity Package Manager (Window > Package Manager)
2. Click the "+" button in the top left corner
3. Select "Add package from git URL..."
4. Enter the following URL: `https://github.com/quantum-native/quantum-forge-unity.git`
5. Click "Add"

## Quick Start Guide

Quantum Forge is built on a few main concepts: 

   * Basis: The values your QuantumProperty can exist in (either classically or in superposition).
   * QuantumProperty: Your hook to give a game object quantum state.
   * Operations: Can be applied to QuantumProperties to change their quantum state.

For example, in Rock Paper Scissors (RPS) your basis can be the values: 'rock', 'paper', 'scissors'. You can give a game object a QuantumProperty component, and set its basis to the RPS basis. You can then apply a Hadamard operation to it, so it exists is a superposition of being all three simultaneously.

Follow these simple steps to add quantum behavior to your game:

1. **Create a Basis**
   - Navigate to `Assets/Create/Quantum` in the Unity menu
   - Create a new Basis
   - Define the values this basis can have (up to 3 values)

2. **Add QuantumProperty**
   - Add the `QuantumProperty` component to a game object
   - Drag your newly created Basis into the Basis field
   - Set the initial classical value

3. **Visualize Probabilities**
   - Add the `ProbabilityTracker` component to the same game object

4. **Apply Quantum Operations**
   - Create a UI Button
   - Add the `Hadamard` component to the button
   - Drag the game object with the QuantumProperty to the Hadamard's "Target Properties" field
   - Add the Hadamard `Apply()` method to the button's onClick event

When you run the game and click the button, you'll see the probability distribution change as the quantum property enters a superposition state.

## Testing Your Setup

<span class="brand-font">Quantum Forge</span> includes a test suite to ensure everything is working correctly:

1. Open the package manifest file (`Packages/manifest.json`)
2. Add quantum-forge to the testables list:
   ```json
   {
       "dependencies": {
           ...
       },
       "testables": ["com.qrg.quantumforge"]
   }
   ```
3. Open the Test Runner window (Window > General > Test Runner)
4. Click the "Run All" button

## Troubleshooting

If you encounter a DLL not found error, or if all tests fail, try closing and reopening Unity. The Unity editor sometimes has issues with loading the underlying quantum-forge library immediately after installation.

On MacOS, the quantum-forge library is not signed (yet), so you may need to add an exception to your security settings.
* Under System Preferences > Security & Privacy > General, click the "Open Anyway" button for the quantum-forge library

## Sample Projects

The <span class="brand-font">Quantum Forge</span> package includes several sample projects that demonstrate different aspects of quantum mechanics:

- **Actions**: Demonstrates various quantum operations
- **Platformer**: Shows how quantum mechanics can be applied to a simple platformer game
- **Roshambo**: A quantum version of rock-paper-scissors

To access these samples, install them from the Unity Package Manager window.

## Next Steps

- [Quantum Concepts](quantum-concepts.md) — Plain-language intro to quantum mechanics for game devs
- [Advanced Topics](advanced-topics.md) — Entanglement, interference, trackers, and design patterns