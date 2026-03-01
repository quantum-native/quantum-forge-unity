// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using QRG.QuantumForge.Runtime;

namespace QRG.QuantumForge.Utility
{
    public class Trigger : MonoBehaviour
    {
        [SerializeField] private UnityEvent
            onTriggerEnter,
            onTriggerExit;

        void OnTriggerEnter2D(Collider2D otherCollider)
        {
            onTriggerEnter.Invoke();
        }

        void OnTriggerEnter(Collider otherCollider)
        {
            onTriggerEnter.Invoke();
        }
    }
}
