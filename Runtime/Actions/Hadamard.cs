// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a quantum Hadamard action that applies a Hadamard operation to quantum properties.
    /// </summary>
    [Serializable]
    public class Hadamard : MonoBehaviour, IQuantumAction
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

        [Tooltip("Fraction of the Hadamard operation to apply.")]
        [SerializeField] private float fraction = 1.0f;

        /// <summary>
        /// Applies the Hadamard operation to the target quantum properties.
        /// </summary>
        public void apply()
        {
            if (TargetProperties == null)
            {
                Debug.LogError($"No target properties set for {gameObject.name} Hadamard to apply");
                return;
            }
            foreach (var prop in TargetProperties)
            {
                UnityEngine.Debug.Log($"Applying {fraction} Hadamard to {prop} with {Predicates?.Length ?? 0} predicates");
                prop.Hadamard(fraction, Predicates);
            }
        }
    }
}
