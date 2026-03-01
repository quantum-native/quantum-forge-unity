// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a quantum ISwap action that applies an ISwap operation to quantum properties.
    /// </summary>
    [Serializable]
    public class ISwap : MonoBehaviour, IQuantumAction
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
        /// The fraction of the ISwap operation to apply.
        /// </summary>
        [Tooltip("Fraction of the ISwap operation to apply.")]
        public float fraction = 1.0f;

        /// <summary>
        /// Applies the ISwap operation to the target quantum properties.
        /// </summary>
        public void apply()
        {
            if (TargetProperties.Length != 2)
            {
                Debug.LogError("ISwap requires exactly 2 target properties");
                return;
            }
            Debug.Log(
                $"Applying {fraction} ISWap to {TargetProperties[0]} and {TargetProperties[1]} with {Predicates?.Length ?? 0} predicates.");
            QuantumProperty.ISwap(TargetProperties[0], TargetProperties[1], fraction, Predicates);
        }
    }

}
