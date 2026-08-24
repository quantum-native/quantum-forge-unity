# Quantum Concepts for Game Developers

No physics degree required. This guide explains the quantum mechanics behind Quantum Forge using game design analogies. For the full Unity API, see [Quantum Properties](quantum-properties.md) and [Advanced Topics](advanced-topics.md).

---

## Dimension: How Many States?

Every quantum property has a **dimension** — the number of values it can take, defined by a [Basis](../api/QuantumForge.Basis.yml).

| Dimension | Game analogy | Example |
|-----------|-------------|---------|
| 2 | Coin flip (heads/tails) | A door that's open or closed |
| 3 | Rock-paper-scissors | A traffic light (red/yellow/green) |

Think of dimension as "how many sides does this die have?" A coin is dimension 2. A three-way choice is dimension 3. You define the values in a Basis ScriptableObject (Assets/Create/Quantum).

---

## Superposition: All States at Once

In a classical game, a coin is either heads or tails. In a quantum game, a coin can be **both at once** — that's superposition.

This isn't the same as "we haven't looked yet." The coin is genuinely in both states simultaneously, with mathematical relationships between them. Those relationships are what make quantum mechanics useful for game design, not just a fancy random number generator.

**Game design implication:** A chest in superposition isn't "we randomly decide what's inside when you open it." The contents are in a structured combination of possibilities that the player can influence before opening.

To see superposition in action, add a [ProbabilityTracker](../api/QuantumForge.ProbabilityTracker.yml) to your game object and apply a [Hadamard](../api/QuantumForge.Hadamard.yml) to its QuantumProperty.

---

## Measurement: Resolving to a Definite Outcome

When you **measure** a quantum property, superposition collapses into one definite value. The probabilities are determined by the quantum state.

Use [MeasureProperties](../api/QuantumForge.MeasureProperties.yml) to trigger measurement. After measurement, the property has a single classical value.

**Game design implication:** You control *when* measurement happens. A quantum door stays in superposition until the player opens it. Everything the player does to shape the quantum state before that moment influences what they get.

---

## Interference: Structure, Not Randomness

Here's where quantum mechanics diverges from random number generators.

When a quantum property is in superposition, different possible states have **phases** — think of them as hidden angles. When you apply operations, these phases can reinforce each other (constructive interference) or cancel each other out (destructive interference).

The result: probability distributions with **structure and patterns**, not uniform randomness. Some outcomes become more likely, others become impossible — based on the operations applied. Use the [PhaseTracker](../api/QuantumForge.PhaseTracker.yml) to visualize these patterns.

**Game design implication:** A player who applies the right sequence of quantum operations can make a specific outcome nearly certain. A different sequence might make it impossible. The game mechanics feel designed, but nobody hard-coded those outcomes — they emerge from the math.

---

## Entanglement: Correlated Outcomes

When two quantum properties become **entangled**, their outcomes become correlated. Measuring one instantly affects what's possible for the other.

Use operations like [ISwap](../api/QuantumForge.ISwap.yml) or [NCycle](../api/QuantumForge.NCycle.yml) on two properties to entangle them. After entanglement, measuring one property changes the probabilities of the other. The [EntanglementTracker](../api/QuantumForge.EntanglementTracker.yml) shows when properties are correlated.

**Game design implication:** Entanglement creates correlations across your game that nobody explicitly programmed. In Quantum Chess, players discovered interference and correlation effects that the designers hadn't anticipated. Players find strategies you never plan.

---

## Putting It Together

A typical quantum game mechanic follows three steps:

1. **Define quantum properties** — Add QuantumProperty components to game objects. Create a Basis that fits the mechanic.

2. **Let players shape the state** — Give players buttons, triggers, or actions that apply quantum operations. Hadamard for superposition, Clock for phase manipulation, ISwap for entanglement.

3. **Measure to resolve** — Decide when the quantum state collapses. Everything the player did to shape the state influences the outcome.

That's the core loop. The quantum mechanics handles the rest — creating emergent patterns, surprising correlations, and strategies that feel new.

---

## What Makes This Different from Random?

| Random | Quantum |
|--------|---------|
| Each roll is independent | State carries history of all operations |
| Distribution is fixed or manually tuned | Distribution emerges from player actions |
| No correlations between separate rolls | Entangled properties produce correlated outcomes |
| Outcomes are arbitrary | Interference creates structured, discoverable patterns |

The result is a new kind of emergent gameplay — behaviors that feel designed but weren't hand-authored. Players can learn the system and develop strategies around it, the same way they learn any other game mechanic.

---

## Next Steps

- [Getting Started](getting-started.md) — Add quantum behavior to your first Unity project
- [Quantum Properties](quantum-properties.md) — Full reference for QuantumProperty, Basis, and operations
- [Advanced Topics](advanced-topics.md) — Entanglement, interference, and design patterns
