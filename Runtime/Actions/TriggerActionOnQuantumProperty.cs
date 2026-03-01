// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QRG.QuantumForge.Runtime;
using UnityEngine.Events;

namespace QRG.QuantumForge.Runtime
{

    /// <summary>
    /// Represents a component that triggers a quantum action when a QuantumProperty is detected.
    /// </summary>
    public class TriggerActionOnQuantumProperty : MonoBehaviour
    {
        /// <summary>
        /// Triggers the quantum action on the specified QuantumProperty.
        /// </summary>
        /// <param name="quantumProperty">The QuantumProperty to act upon.</param>
        void TriggerAction(QuantumProperty quantumProperty)
        {
            if (quantumProperty == null)
            {
                return;
            }
            var action = GetComponent<IQuantumAction>();
            action.TargetProperties = new QuantumProperty[] { quantumProperty };
            action.apply();
        }

        /// <summary>
        /// Called when another collider enters this collider (2D).
        /// </summary>
        /// <param name="otherCollider">The collider that entered.</param>
        void OnTriggerEnter2D(Collider2D otherCollider)
        {
            var q = otherCollider.gameObject.GetComponent<QuantumProperty>();
            TriggerAction(q);
        }

        /// <summary>
        /// Called when another collider enters this collider (3D).
        /// </summary>
        /// <param name="otherCollider">The collider that entered.</param>
        void OnTriggerEnter(Collider otherCollider)
        {
            Debug.Log("Trigger enter");
            var q = otherCollider.gameObject.GetComponent<QuantumProperty>();
            TriggerAction(q);
        }
    }
}
