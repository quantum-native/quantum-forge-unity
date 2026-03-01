// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a quantum action that measures quantum properties and triggers events.
    /// </summary>
    [Serializable]
    public class MeasureProperties : MonoBehaviour, IQuantumAction
    {
        /// <summary>
        /// Event triggered when a measurement is performed.
        /// </summary>
        [Tooltip("Event triggered when a measurement is performed.")]
        public UnityEvent OnMeasure;

        /// <summary>
        /// Event triggered with a QuantumProperty when a measurement is performed.
        /// </summary>
        [Tooltip("Event triggered with a QuantumProperty when a measurement is performed.")]
        public QuantumPropertyEvent OnMeasureQuantumProperty;

        /// <summary>
        /// Gets or sets the predicates that determine the conditions for this action.
        /// </summary>
        [Tooltip("Predicates that determine the conditions for this action.")]
        public Predicate[] Predicates { get; set; } // not shown in inspector

        /// <summary>
        /// Gets or sets the quantum properties that this action targets.
        /// </summary>
        [Tooltip("Quantum properties that this action targets.")]
        [field: SerializeField] public QuantumProperty[] TargetProperties { get; set; }

        /// <summary>
        /// Gets the last results of the measurements.
        /// </summary>
        [Tooltip("The last results of the measurements.")]
        [field: SerializeField] public int[] LastResult { get; private set; }

        /// <summary>
        /// Applies the measurement action to the target quantum properties.
        /// </summary>
        public void apply()
        {
            if (TargetProperties == null || TargetProperties.Length == 0) return;
            LastResult = QuantumProperty.MeasureProperties(TargetProperties);
            foreach (var quantumProperty in TargetProperties)
            {
                Debug.Log($"Measuring {quantumProperty.gameObject.name}");
                OnMeasureQuantumProperty.Invoke(quantumProperty);
            }
            OnMeasure.Invoke();
        }

    }

}
