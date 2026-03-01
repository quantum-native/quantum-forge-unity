// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using QRG.QuantumForge.Runtime;

namespace QRG.QuantumForge.Runtime
{
    [System.Serializable]
    public class QuantumPropertyEvent : UnityEvent<QuantumProperty> { };

    public class QuantumPropertyTrigger : MonoBehaviour
    {
        [SerializeField] private QuantumPropertyEvent onTriggerEnter, onTriggerExit;

        void OnTriggerEnter2D(Collider2D otherCollider)
        {
            var q = otherCollider.gameObject.GetComponent<QuantumProperty>();
            if (q != null)
            {
                onTriggerEnter.Invoke(q);
            }
        }

        void OnTriggerEnter(Collider otherCollider)
        {
            var q = otherCollider.gameObject.GetComponent<QuantumProperty>();
            if (q != null)
            {
                onTriggerEnter.Invoke(q);
            }
        }
    }
}