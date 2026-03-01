// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a quantum cycle action that applies a cycle operation to quantum properties.
    /// </summary>
    [Serializable]
    public class Cycle : MonoBehaviour, IQuantumAction
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
        /// The fraction of the cycle to apply.
        /// </summary>
        [Tooltip("Fraction of the cycle to apply.")]
        [SerializeField] private float fraction = 1.0f;

        /// <summary>
        /// Applies the cycle operation to the target quantum properties.
        /// </summary>
        public void apply()
        {
            foreach (var quantumProperty in TargetProperties)
            {
                Debug.Log($"Applying {fraction} cycle to {quantumProperty.gameObject.name} with {Predicates.Length} predicates.");
                quantumProperty.Cycle(fraction, Predicates);
            }
        }
    }

}
