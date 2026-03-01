// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a quantum action that swaps the states of two quantum properties.
    /// </summary>
    [Serializable]
    public class Swap : MonoBehaviour, IQuantumAction
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
        /// Applies the swap operation to the target quantum properties.
        /// </summary>
        public void apply()
        {
            if (TargetProperties.Length != 2)
            {
                Debug.LogError("Swap requires exactly 2 target properties");
                return;
            }
            QuantumProperty.Swap(TargetProperties[0], TargetProperties[1], Predicates);
        }
    }

}
