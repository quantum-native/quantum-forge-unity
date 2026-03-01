// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using QRG.QuantumForge.Runtime;

namespace QRG.QuantumForge.Roshambo
{
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class ProbabilityText : MonoBehaviour
    {
        public ProbabilityTracker tracker;
        private TMPro.TextMeshProUGUI text;

        void OnEnable()
        {
            text = GetComponent<TMPro.TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            if (tracker != null && text != null)
            {
                if (tracker.Probabilities != null)
                {
                    var probs = tracker.Probabilities;
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < probs.Length; i++)
                    {
                        if (probs[i].Probability != 0.0f)
                        {
                            sb.AppendLine($"{probs[i].ToString()}");
                        }
                    }

                    text.text = sb.ToString();
                }
            }
        }
    }
}
