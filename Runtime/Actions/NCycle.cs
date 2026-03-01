// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a quantum action that applies an NCycle operation to quantum properties.
    /// </summary>
    [Serializable]
    public class NCycle : MonoBehaviour, IQuantumAction
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
        /// Applies the NCycle operation to the target quantum properties.
        /// </summary>
        public void apply()
        {
            if (TargetProperties.Length != 2)
            {
                Debug.LogError("Must supply exactly 2 QuantumProperties to NCycle");
            }
            else
            {
                QuantumProperty.NCycle(TargetProperties[0], TargetProperties[1]);
            }
        }

    }

}
