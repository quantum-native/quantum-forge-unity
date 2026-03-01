# Advanced Topics in <span class="brand-font">Quantum Forge</span>

This guide covers advanced concepts and techniques for the <span class="brand-font">Quantum Forge</span> Unity package. If you're new to <span class="brand-font">Quantum Forge</span>, we recommend starting with the [Getting Started](getting-started.md) guide first.

## Best Practices for Quantum Game Design

### Keep the Quantum Small
Quantum can be confusing and can quickly grow out of control. Keep things as simple as possible. If you find yourself needing a large Basis, consider whether it's possible to break it down into smaller parts.

**Example**: In chess, there are 12 pieces plus empty squares (13 states total). Rather than creating a 13-element Basis, you could use a 2-element Basis (`empty` and `not empty`) and store the piece type in a separate, classical variable.

Also keep the number of quantum properties small. There is a limit to the number of quantum properties that can exist, and interact with each other. At the time of writing, this limit is 20.

### Give Players Control
Quantum is a powerful tool but can be difficult to understand. Give players access to ways to manipulate the quantum state. They may discover effects in your game that you didn't expect.

During the development of [Quantum Chess](https://store.steampowered.com/app/453870/Quantum_Chess/), players discovered strange interference effects that initially seemed impossible but were actually emergent properties of the quantum system.

## Advanced Quantum Concepts

### Entanglement
Entanglement is a phenomenon where the state of a quantum system cannot be described by the states of its individual components. In <span class="brand-font">Quantum Forge</span>, you can create entanglement between properties using:

- **Predicates**: Apply quantum operations conditionally
- **NCycle**: Entangle two quantum properties
- **Measurement**: Measuring one entangled property affects others

### Phase and Interference
Phase is critical for quantum interference effects. Use these components to manipulate phase:

- **PhaseRotate**: Apply a specific phase to selected elements of a superposition
- **Clock**: Apply phase rotations proportional to basis value indices
- **PhaseTracker**: Visualize the relative phases in your system

### Superposition and Probability
Understanding probability distributions is key to designing quantum game mechanics:

- **Hadamard**: Create equal superpositions
- **Cycle/Shift with Fraction**: Create uneven superpositions
- **ProbabilityTracker**: Monitor the probability distribution of quantum states

## Advanced Trackers

### CorrelationTracker
This tracker helps you visualize the correlation between different quantum properties, which is especially useful for understanding what will happen when measurements occur. It calculates [Pearson's correlation coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient) between properties.

```csharp
// To get the correlation matrix programmatically:
var correlationMatrix = corTracker.UpdateCorrelationMatrix();
```

### EntanglementTracker
Measures the quantum mutual information between properties, providing a quantitative measure of entanglement. This can help players understand when properties are correlated in non-classical ways.

### ReducedDensityMatrixTracker
Provides access to the reduced density matrix of your quantum system, which is the full mathematical representation of the quantum state. This can be useful for understanding the state of the system and for debugging.

## Quantum Operations Reference

### Controlled Operations
Many quantum operations accept predicates that make them conditional. This is how you can create complex quantum behavior and entanglement:

```csharp
// Example: Apply Hadamard only when another property is in state 0
var predicate = new Predicate { 
    property = controlQubit, 
    value = controlQubit.Basis.Values[0], 
    is_equal = true 
};

hadamard.Predicates.Add(predicate);
hadamard.Apply(); // This is now a controlled-Hadamard operation
```

### Custom Quantum Actions
You can create custom quantum actions by extending the `QuantumAction` class:

```csharp
public class MyCustomQuantumAction : QuantumAction 
{
    public override void Apply() 
    {
        // Your quantum operation logic here
    }
}
```

## Debugging Quantum States

### Using Multiple Trackers
Combining trackers gives you a more complete picture of your quantum system:

- **ProbabilityTracker**: Shows the probability of each basis state
- **PhaseTracker**: Visualizes the phase relationships
- **EntanglementTracker**: Quantifies entanglement
- **CorrelationTracker**: Shows correlations between measurements

### Performance Considerations
The quantum simulation becomes more computationally intensive as you add quantum properties. Some tips for optimization:

- Set trackers to update on demand rather than continuously
- Minimize the number of quantum properties active at once
- Use controlled operations efficiently
- Consider separating your quantum system into independent subsystems

## Example: Creating a Quantum Game Mechanic

Here's an example of a quantum-powered game mechanic:

1. Create a quantum door with two basis states: `world 1` and `world 2`
2. Apply a Hadamard to put it in superposition (50% world 1, 50% world 2)
3. Create a quantum key that the player can load quantum operations into
4. When the player uses the key, it applies the quantum operations to the door and measures
5. The door's state is determined at that moment, creating uncertainty

This creates gameplay where the player isn't certain which world they'll enter. But they can influence the probability of each world by loading different quantum operations into the key.

> **Note**: This is a very basic example to illustrate simple quantum mechanics. Once you start introducing multiple quantum objects and allow them to interact (through controlled operations or shared measurements), you can begin to observe unusual quantum behavior such as entanglement, non-local correlations, and complex interference patterns that have no classical analog.

## Glossary of Quantum Concepts

- **Superposition**: A linear combination of basis states
- **Basis**: A set of orthogonal states that represent a quantum system
- **Amplitude**: A complex number describing a basis state's contribution to the whole
- **Phase**: The angle of a complex amplitude, critical for interference effects
- **Entanglement**: When the state of a quantum system cannot be described by individual components
- **Measurement**: The process of obtaining information about a quantum system, causing collapse
- **Interference**: When quantum amplitudes interact, reinforcing or cancelling each other

## Further Resources

- [Quantum Chess](https://store.steampowered.com/app/453870/Quantum_Chess/)
- [Quantum Mutual Information](https://en.wikipedia.org/wiki/Quantum_mutual_information)
- [Pearson's Correlation Coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient)