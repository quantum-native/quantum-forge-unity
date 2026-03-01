// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Defines the interface for quantum actions that can be applied to quantum properties.
    /// </summary>
    public interface IQuantumAction 
    {
        /// <summary>
        /// Gets or sets the predicates that determine the conditions for this action.
        /// </summary>
        public Predicate[] Predicates { get; set; }

        /// <summary>
        /// Gets or sets the quantum properties that this action targets.
        /// </summary>
        public QuantumProperty[] TargetProperties { get; set; }

        /// <summary>
        /// Applies the quantum action to the target quantum properties.
        /// </summary>
        public void apply();
    }

}
