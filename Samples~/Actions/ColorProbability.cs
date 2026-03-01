// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using QRG.QuantumForge.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QRG.QuantumForge.Samples
{
    public class ColorProbability : MonoBehaviour
    {
        public Color[] basisColors;

        // Update is called once per frame
        void Update()
        {
            var probabilities = GetComponent<ProbabilityTracker>().Probabilities;
            if (probabilities.Length == 0)
            {
                return;
            }
            Color color = new Color(0, 0, 0, 0);
            for (int i = 0; i < probabilities.Length; i++)
            {
                color += basisColors[i] * probabilities[i].Probability;
            }
            GetComponent<Image>().color = color;
        }
    }
}
