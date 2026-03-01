// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace QRG.QuantumForge.Runtime
{

    [Serializable]
    public class MeasurePredicateEvent : UnityEvent<bool> { };

    /// <summary>
    /// Represents a quantum action that measures predicates and triggers events.
    /// </summary>
    [Serializable]
    public class MeasurePredicates : MonoBehaviour, IQuantumAction
    {
        /// <summary>
        /// Event triggered when a measurement is performed.
        /// </summary>
        [Tooltip("Event triggered when a measurement is performed.")]
        public UnityEvent OnMeasure;

        /// <summary>
        /// Event triggered with a QuantumProperty when a measurement is performed.
        /// </summary>
        [Tooltip("Event triggered with a bool when a measurement is performed. Triggers with true if the predicates are satisfied, false otherwise.")]
        public MeasurePredicateEvent OnMeasurePredicates;
        /// <summary>

        /// Gets or sets the predicates that determine the conditions for this action.
        /// </summary>
        [Tooltip("Predicates that determine the conditions for this action.")]
        [field: SerializeField] public Predicate[] Predicates { get; set; }

        /// <summary>
        /// Gets or sets the quantum properties that this action targets.
        /// </summary>
        public QuantumProperty[] TargetProperties { get; set; } // not shown in inspector

        /// <summary>
        /// Gets the last result of the measurement.
        /// </summary>
        [Tooltip("The last result of the measurement.")]
        [field: SerializeField] public int LastResult { get; private set; }

        /// <summary>
        /// Applies the measurement action to the target quantum properties.
        /// </summary>
        public void apply()
        {
            if (Predicates == null || Predicates.Length == 0) return;
            LastResult = QuantumProperty.MeasurePredicate(Predicates);
            OnMeasurePredicates?.Invoke(LastResult == 1);
            OnMeasure.Invoke();
        }

    }

}
