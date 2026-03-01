<table>
  <tr>
    <td style="min-width: 200px; border: none;">
      <img src="Documentation~/images/logo.png" alt="Quantum Forge Logo" width="150">
    </td>
    <td style="border: none;">
      <h1 style="margin: 0;">Quantum Forge Unity Package</h1>
      <span style="margin: 0;">Real quantum mechanics for your games.</span>
    </td>
  </tr>
</table>

# Welcome to Quantum Forge 🧪🎮
For developers pushing the boundaries of gameplay design, **Quantum Forge** offers a foundation for creating the next generation of quantum-native games.

This project grew out of years of experimentation with quantum mechanics in games, including lessons learned while creating [Quantum Chess](https://store.steampowered.com/app/453870/Quantum_Chess/). **Quantum Forge** makes it straightforward to add real quantum behavior to your gameplay.

Now, it's your turn. Whether you're here to experiment, prototype, or launch something groundbreaking, we’re excited to have you with us.

If you believe in this vision and want to help push the boundaries of what's possible at the intersection of quantum mechanics and game design, consider supporting the project.

<a href="https://www.buymeacoffee.com/quantum_forge"><img src="https://img.buymeacoffee.com/button-api/?text=Buy me a coffee&emoji=☕&slug=quantum_forge&button_colour=fc6e23&font_colour=000000&font_family=Cookie&outline_colour=000000&coffee_colour=FFDD00" /></a>

Thank you for joining us on this journey.

## Overview
The Quantum Forge Unity Package provides a wrapper around the quantum-forge c-api, and allows developers to plug real quantum behaviour directly into their gameplay mechanics. It also provides a set of MonoBehaviours that can be added to game objects, for easy Unity editor integration.

This README can get you started. For more information, including advanced topics and the full Unity script API, see our [Full Documentation](https://docs.quantum.dev)

## Installation

Quantum Forge currently supports Unity 2022.3.1f1 or later, and can be used to build for Windows, Mac, Linux, and WebGL.

To install the Quantum Forge Unity Package:

1. Open the Unity Package Manager (Window > Package Manager)
2. Click the "+" button in the top left corner
3. Select "Add package from git URL..."
4. Enter the following URL: `https://github.com/quantum-native/quantum-forge-unity.git`
5. Click "Add"


For more information, please refer to Unity's [Package Manager Window](https://docs.unity3d.com/Manual/upm-ui.html) documentation.

## Testing
Quantum Forge comes with a set of tests that can be run to ensure that everything is working as expected. To run the tests, follow these steps:
1. Open the package manifest file (Packages/manifest.json).
2. Add quantum-forge to the testables list:
```
    {
        "dependencies": {
            ...
        },
        "testables": ["com.qrg.quantum-forge"]
    }
```
3. In the Unity Editor open the Test Runner window (Window > General > Test Runner).
4. Click the "Run All" button (bottom right corner).

For more information, please refer to Unity's [Package](https://docs.unity3d.com/Manual/cus-tests.html#tests) and [Automated Testing](https://docs.unity3d.com/Manual/testing-editortestsrunner.html) documentation.

## Troubleshooting
If you encounter a dll not found error, or if all tests fail, try closing and reopening Unity. The Unity editor sometimes has issues with loading the underlying quantum-forge library immediately after installation.


On MacOS, the quantum-forge library is not signed (yet), so you may need to add an exception to your security settings.
* Under System Preferences > Security & Privacy > General, click the "Open Anyway" button for the quantum-forge library


## Getting Started
It is easy to get quantum behaviour into your game quickly.
1. Create a new [Basis](#basis).
2. Add the [QuantumProperty](#quantumproperty) MonoBehaviour to a game object.
3. Drag the newly created [Basis](#basis) into the Basis field in the QuantumProperty editor.

The GameObject can now exist in a superposition of values defined by the Basis. The easist way to see this is to add the [ProbabilityTracker](#probabilitytracker) to the same GameObject, and apply a [Hadamard](#hadamard) to the QuantumProperty.
1. Add the [ProbabilityTracker](#probabilitytracker) to the same GameObject.
2. Create a UI Button, and add the [Hadamard](#hadamard) to it.
3. Drag the GameObjct with the QuantumProperty and ProbabilityTracker to the Hadamard's ```Target Properties``` field.
3. Add the Hadamard ```apply``` method to the button's onClick event.

Now when you run the game, you should see the probability of the QuantumProperty being in each of the values defined by the Basis change when you click the button.

Check out the Samples provided with the package for more examples.

## Tips
### 1. Keep the quantum small
Quantum can be confusing, and can quickly grow out of control. Keep things as simple as possible. If you find yourself needing a large Basis, consider whether it is possible to break it down into smaller parts. 

For example, in chess there are 12 pieces, and empty. You would need a 13 element Basis to represent this. However, if you consider the state of the square, you can encode it in a 2 element Basis with the values ```empty``` and ```not empty```. The type of piece can be stored in a separate, completely classical, list. (Note: this does require extra thought for how the pieces interact)

### 2. Give the players control
Quantum is a powerful tool, but it can be difficult to understand. Give the players access to ways to manipulate the quantum state. They may well discover effects in your game that you didn't expect. This happened in the development of [Quantum Chess](https://store.steampowered.com/app/453870/Quantum_Chess/). Players were given access to quantum evolution through movement, and some discovered strange interference effects that at first glance seemed impossible (we thought they were bugs).

Quantum game development is new, you're at the forefront. Experiment, and have fun!

## Quantum Forge Editor
Quantum Forge is designed to be as easy to use as possible. As such it comes with robust editor integration. This integration is built around the concepts of [Basis](#basis), [QuantumProperty](#quantumproperty), [Predicate](#predicate) and [QuantumAction](#quantumactions), and [Tracker](#trackers).

### Basis
A ScriptableObject that defines the set of values that a QuantumProperty can exist in a superposition of. A new ```Basis``` can be created via the Assets/Create/Quantum menu. 

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Values`        | `List<BasisValue>` | A list of [`BasisValue`](#basisvalue) objects representing the possible states of the basis. |

#### BasisValue
A struct that defines a single value in a Basis.
| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Name`          | `string`         | The name of the basis, used for identification.                                |
| `Dimension`     | `int`            | The number of possible values in the basis.                                    |


The underlying engine has a hard limit of the number of values a Basis can have, **3** at the moment. If you need more, consider your design. If you still think you need more, please open an issue on github.

### QuantumProperty
A MonoBehaviour that can be added to a game object to interface with the underlying Quantum Forge library. This is the hook into the quantum world. A quantum property allows aome aspect of a game object to exist in a superposition of values defined by a Basis. 
| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Basis`         | `Basis`          | The basis that the QuantumProperty exists in.                                   |
| `Initial`         | `BasisValue`            | The initial classical value of the QuantumProperty.                                       |

### Predicate
A struct that defines a condition that can be used to apply a [QuantumAction](#quantumactions) to a subset of the [BasisValues](#basisvalue) of a [QuantumProperty](#quantumproperty). This can be used to [entangle](#entanglement) multiple QuantumProperties together.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `property`| `QuantumProperty`| The QuantumProperty that the Predicate is associated with. The predicate will condition on basis values of this quantum property                          |
| `value`| `BasisValue`| The predicate will condition on this basis value.                          |
| `is_equal`| `bool`| If true, the predicated [QuantumAction](#quantumactions) will apply to the subset of the superposition where the QuantumProperty has the specified basis value. If false, the predicate will apply to the subset of the superposition where the QuantumProperty does not have the specified basis value.                          |

### QuantumActions
These are a set of MonoBehaviours that can be added to a scene, and can be used to trigger operations on [QuantumProperty(s)](#quantumproperty). These are how you change the quantum state of QuantumProperties.

#### Hadamard
Takes something from classical state to equal superposition of all basis values. [Math](https://en.wikipedia.org/wiki/Hadamard_transform)
| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply the Hadamard to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The [QuantumProperties](#quantumproperty) to apply the Hadamard operation to. |

#### Inverse Hadamard
Takes something from classical state to equal superposition of all basis values. 

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The [QuantumProperties](#quantumproperty) to apply this operation to. |

#### Shift
Shifts backward through the [basis values](#basisvalue). For a [basis](#basis) of dimension D: D -> D-1 -> D-2... 2 -> 1 -> 0 -> D-1

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The QuantumProperties to apply this operation to. |
| `Fraction` | `float` | The fraction of the operation to apply. Values less than 1 can create [superposition](#superposition) |

#### Cycle
Cycles through the [basis values](#basisvalue). For a [basis](#basis) of dimension D: 0 -> 1 -> 2... D -> 0

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the [superposition](#superposition). |
| `Target Properties` | `List<QuantumProperty>` | The [QuantumProperties](#quantumproperty) to apply this operation to. |
| `Fraction` | `float` | The fraction of the operation to apply. Values less than 1 can create [superposition](#superposition) |

#### Clock
Applies a [phase](#phase) rotation of $2\pi n/D$ to each [basis value](#basisvalue) in the [superposition](#superposition), where $D$ is the dimension of the [basis](#basis) and $n$ is index value of the basis value.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The [QuantumProperties](#quantumproperty) to apply this operation to. |
| `Fraction` | `float` | The fraction of the operation to apply. |

#### PhaseRotate
Rotate elements of the [superposition](#superposition) selected by a set of [predicates](#predicate) by a speified [phase](#phase).

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Required[predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Radians` | `float` | The phase in the range $[0,2\pi]$. |


#### NCycle
[Entangles](#entanglement) two [QuantumProperties](#quantumproperty). [Cycle](#cycle) the [basis value](#basisvalue) of one QuantumProperty by $N$ for each element in the [superposition](#superposition), where $N$ is the index of the [basis value](#basisvalue) of another QuantumProperty.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The QuantumProperties to apply this operation to. Must be exactly two QuantumProperties. Property 0 supplies the value $N$, and property 1 is the target of the [cycle](#cycle). |

#### Swap
Swap the quantum state of two [QuantumProperties](#quantumproperty) of the same [dimension](#basis).

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The QuantumProperties to apply this operation to. Must be exactly two QuantumProperties. |

#### ISwap
Swap the quantum state of two [QuantumProperties](#quantumproperty) of the same [dimension](#basis), with a [phase](#phase) rotation of $\pi/2$.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The QuantumProperties to apply this operation to. Must be exactly two QuantumProperties. |
| `Fraction` | `float` | The fraction of the operation to apply. A fraction between 0 and 1 can create [superposition](#superposition) of both applying the operation and not applying it. |

#### X
Pauli X gate — alias for [Shift](#shift). Shifts backward through the [basis values](#basisvalue). Works for any [basis](#basis) dimension.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The [QuantumProperties](#quantumproperty) to apply this operation to. |
| `Fraction` | `float` | The fraction of the operation to apply. Values less than 1 can create [superposition](#superposition) |

#### Y
Pauli Y gate — qubit-only (dimension 2 [Basis](#basis) required). Composite gate: applies S, then X (Shift), then S† (inverse S). Throws an error if the [Basis](#basis) dimension is not 2.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The [QuantumProperties](#quantumproperty) to apply this operation to. Must have a dimension 2 [Basis](#basis). |
| `Fraction` | `float` | The fraction of the operation to apply. Values less than 1 can create [superposition](#superposition) |

#### Z
Pauli Z gate — alias for [Clock](#clock). Applies a [phase](#phase) rotation. Works for any [basis](#basis) dimension.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `Predicates` | `List<QuantumProperty.Predicate>` | Optional [predicates](#predicate) to conditionally apply this operation to elements of the superposition. |
| `Target Properties` | `List<QuantumProperty>` | The [QuantumProperties](#quantumproperty) to apply this operation to. |
| `Fraction` | `float` | The fraction of the operation to apply. |

#### MeasureProperties
Measure one or more [QuantumProperties](#quantumproperty) to collapse to a single [basis state](#basis-state) in the [superposition](#superposition) with probability proportional to the [basis value](#basisvalue) [amplitudes](#basisvalue).

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `OnMeasure()` | `UnityEvent` | Triggers when the measurement is complete. |
| `OnMeasureQuantumProperty(QuantumProperty)` | `UnityEvent<QuantumProperty>` | Triggers when the measurement is complete, with each QuantumProperty that was measured. |
| `Target Properties` | `List<QuantumProperty>` | The [QuantumProperties](#quantumproperty) to be measured. |
| `Last Result` | `List<QuantumProperty.BasisValue>` | The last result of the measurement. This is an ordered list of basis values, for each QuantumProperty in the order they were specified in the Target Properties field. |

#### MeasurePredicates
Measure the quantum state to collapse into either a substate of satisfying, or one of not satisfying, a set of [predicates](#predicate).

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `OnMeasure()` | `UnityEvent` | Triggers when the measurement is complete. |
| `OnMeasurePredicates(bool)` | `UnityEvent<bool>` | Triggers when the measurement is complete, with the value true if the predicates were satisfied, and false otherwise. |
| `Predicates` | `List<Predicate>` | The [Pedicates](#predicate) to be measured. |
| `Last Result` | `bool` | The last result of the measurement. True  if the predicates were satisfied, and false otherwise. ||

### Trackers
Convenience components to expose information about an underlying state to the editor / game.

#### ProbabilityTracker
MonoBehaviour that keeps an updated probability distribution for the [basis values](#basisvalue) of a [QuantumProperty](#quantumproperty), or track the joint probability distribution of multiple QuantumProperty's. Can be toggled to continuously update once per frame, or update on demand via the UpdateProbabilities() function. 

Continuous updating calls the native API once per frame, and recalculates probabilities on every call. This can be extremely wasteful, when the underlying state is slow to change.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `QuantumProperties` | `List<QuantumProperty>` | The QuantumProperties to track. |
| `Probabilities` | `List<float>` | The current probability distribution. |
| `Continuous` | `bool` | Whether to update the probabilities in UpdateI() on every frame. |

#### EntanglementTracker
MonoBehaviour that keeps an updated list of [entanglement](https://en.wikipedia.org/wiki/Quantum_mutual_information), as measured by [quantum mutual information](https://en.wikipedia.org/wiki/Quantum_mutual_information), between each [QuantumProperty](#quantumproperty) and the combined state of the other properties in the list. It is presentated as a column of values, where each row corresponds to a QuantumProperty.

This can be used to show the player when different QuantumProperties are correlated in ways that are beyond classical.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `QuantumProperties` | `List<QuantumProperty>` | The QuantumProperties to track. |
| `Mutual Information` | `List<List<float>>` | Private field, for editor display of mutual information. |
| `Last Updated Mutual Information` | `List<float>` | The mutual information between each QuantumProperty, and all others in the list. Can be updated by calling the ```UpdateMutualInformation()``` function. |
| `Continuous` | `bool` | Whether to update the probabilities in UpdateI() on every frame. |

#### CorrelationTracker
MonoBehaviour that keeps an updated matrix of [Pearson's correlations](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient) calculated by the joint probability distribution of all permutations of all basis values for a set of QuantumProperties.

Note: The editor field ```MatrixData``` is generally used for visualization purposes only. The actual data can be obtained by calling the ```UpdateCorrelationMatrix()``` function, which returns a 2D array of floats.

This can be used to get an idea of how measurement outcomes will be correlated when different QuantumProperty's are entangled. The result is a matix of correlations between all pairs of QuantumProperties. So element $(2,3)$ is the correlation between the 2nd and 3rd QuantumProperties in the list.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `QuantumProperties` | `List<QuantumProperty>` | The QuantumProperties to track. |
| `MatrixData` | `string` | The mutual information between each QuantumProperty, and all others in the list. This is useful for editor visualization. |
| `Continuous` | `bool` | Whether to update the probabilities in Update() on every frame. |


#### PhaseTracker
MonoBehaviour that keeps an updated matrix of the relative phases between all permutations of the basis values for a set of QuantumProperties. So element $(2,3)$ is the phase of the 3rd QuantumProperty relative to the 2nd QuantumProperty in the list.

Note: The editor field ```MatrixData``` is generally used for visualization purposes only. The actual data can be obtained by calling the ```UpdatePhaseMatrix()``` function, which returns a 2D array of floats.

This can be used to give the player an idea of what interference might occur.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `QuantumProperties` | `List<QuantumProperty>` | The QuantumProperties to track. |
| `Continuous` | `bool` | Whether to update the probabilities in Update() on every frame. |
| `Radians` | `bool` | Whether to display the phase in radians or degrees. |
| `MatrixData` | `string` | The mutual information between each QuantumProperty, and all others in the list. This is useful for editor visualization. |

#### ReduceDensityMatrixTracker
MonoBehaviour that keeps an updated reduced density matrix for a set of QuantumProperties. Probably not super useful as a game development/design tool, but it was free, and other trackers are based on its functionality.

Note: The editor field ```MatrixData``` is generally used for visualization purposes only. The actual data can be obtained by calling the ```GetReducedDensityMatrix()``` function, which returns a 2D array of Complex numbers.

| **Field**       | **Type**         | **Description**                                                                 |
|------------------|------------------|---------------------------------------------------------------------------------|
| `QuantumProperties` | `List<QuantumProperty>` | The QuantumProperties to track. |
| `Continuous` | `bool` | Whether to update the probabilities in Update() on every frame. |
| `MatrixData` | `string` | The mutual information between each QuantumProperty, and all others in the list. This is useful for editor visualization. |

## Useful Definitions
Here are some useful definitions for understanding quantum mechanics. It could also be useful to understand [bra-ket notation](https://en.wikipedia.org/wiki/Bra-ket_notation).

### Superposition
A superposition is a linear combination of basis states. For example, a qubit in the state |0⟩ + |1⟩ is in a superposition of the basis states |0⟩ and |1⟩.

<a id="basis-definition"></a>
### Basis
A basis is a set of orthogonal states that can be used to represent a quantum system. For example, in the case of a qubit, the basis is the set of states {|0⟩, |1⟩}.

### Basis State
A basis state is a combination of [basis values](#basisvalue) for some number of [QuantumProperties](#quantumproperty). For example, the basis state |01⟩ is the combined basis value for two qubits, where the first qubit is in the state |0⟩ and the second qubit is in the state |1⟩.

### Amplitude
A quantum state is a superposition of basis states with amplitudes describing their contribution to the whole. The amplitude is a complex number, and the probability of a particular basis state being found on measurement is the square of the amplitude.

### Phase
The phase of a basis state is the angle of its complex [amplitude](#amplitude). This is important for interference.

### Entanglement
Entanglement is a phenomenon in which the state of a quantum system cannot be described by the states of its individual components. For example, if you have two entangled qubits, the state of one qubit cannot be described without considering the state of the other qubit.

### Measurement
A measurement is the process of obtaining information about a quantum system. When a measurement is performed on a quantum system in superposition, the system collapses into a subset possible states in the basis.

### Interference
Interference is the phenomenon that occurs when two or more waves interact with each other. In quantum mechanics, interference can occur when two or more quantum states interfere with each other.

## License

C# source code is [MIT licensed](LICENSE.md). Native plugin binaries are
proprietary. See [LICENSING.md](LICENSING.md) for details.

## Links

- [Quantum Chess](https://store.steampowered.com/app/453870/Quantum_Chess/)
- [Quantum Mutual Information](https://en.wikipedia.org/wiki/Quantum_mutual_information)
- [Pearson's Correlation Coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient)
