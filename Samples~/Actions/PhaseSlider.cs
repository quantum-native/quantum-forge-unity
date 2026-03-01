// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QRG.QuantumForge.Runtime;
using UnityEngine.UI;

namespace QRG.QuantumForge.Samples
{
    public class PhaseSlider : MonoBehaviour
    {
        public PhaseRotate phaseRotate;

        void Start()
        {
            GetComponent<Slider>().value = phaseRotate.Radians / (2 * Mathf.PI);
        }

        public void SetPhaseAngle()
        {
            var ratio = GetComponent<Slider>().value;
            phaseRotate.Radians = (ratio * 2 * Mathf.PI);
        }
    }
}
