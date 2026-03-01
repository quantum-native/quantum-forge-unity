// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a quantum action that applies a shift operation to quantum properties.
    /// </summary>
    [Serializable]
    public class Shift : MonoBehaviour, IQuantumAction
    {
        /// <summary>
        /// Gets or sets the predicates that determine the conditions for this action.
        /// </summary>
        [field: SerializeField] public Predicate[] Predicates { get; set; }

        /// <summary>
        /// Gets or sets the quantum properties that this action targets.
        /// </summary>
        [field: SerializeField] public QuantumProperty[] TargetProperties { get; set; }

        /// <summary>
        /// The fraction of the shift operation to apply.
        /// </summary>
        [SerializeField] private float fraction = 1.0f;

        /// <summary>
        /// Applies the shift operation to the target quantum properties.
        /// </summary>
        public void apply()
        {
            foreach (var quantumProperty in TargetProperties)
            {
                Debug.Log($"Applying {fraction} shift to {quantumProperty.gameObject.name} with {Predicates.Length} predicates.");
                quantumProperty.Shift(fraction, Predicates);
            }
        }
    }

}
