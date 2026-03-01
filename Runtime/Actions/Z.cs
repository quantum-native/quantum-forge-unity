// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a quantum action that applies a Pauli Z gate (alias for clock) to quantum properties.
    /// </summary>
    [Serializable]
    public class Z : MonoBehaviour, IQuantumAction
    {
        /// <summary>
        /// Gets or sets the predicates that determine the conditions for this action.
        /// </summary>
        [Tooltip("Predicates that determine the conditions for this action.")]
        [field: SerializeField] public Predicate[] Predicates { get; set; }

        /// <summary>
        /// Gets or sets the quantum properties that this action targets.
        /// </summary>
        [Tooltip("Quantum properties that this action targets.")]
        [field: SerializeField] public QuantumProperty[] TargetProperties { get; set; }

        /// <summary>
        /// The fraction of the Z gate to apply.
        /// </summary>
        [Tooltip("Fraction of the Z gate to apply.")]
        public float fraction = 1.0f;

        /// <summary>
        /// Applies the Z gate to the target quantum properties.
        /// </summary>
        public void apply()
        {
            foreach (var prop in TargetProperties)
            {
                UnityEngine.Debug.Log($"Applying {fraction} Z to {prop} with {Predicates.Length} predicates");
                prop.Z(fraction, Predicates);
            }
        }
    }

}
