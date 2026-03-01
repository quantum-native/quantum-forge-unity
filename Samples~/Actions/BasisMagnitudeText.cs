// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QRG.QuantumForge.Runtime;

namespace QRG.QuantumForge.Samples
{
    public class BasisMagnitudeText : MonoBehaviour
    {
        public ProbabilityTracker probabilityTracker;
        public int basisValue = 1;

        // Update is called once per frame
        void Update()
        {
            var probabilities = probabilityTracker.Probabilities;
            if (probabilities.Length == 0)
            {
                return;
            }
            var p = probabilities[basisValue].Probability;
            var magnitude = Mathf.Sqrt(p);
            GetComponent<TMPro.TextMeshProUGUI>().text = magnitude.ToString("F2");
        }
    }
}
