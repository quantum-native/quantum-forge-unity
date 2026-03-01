// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QRG.QuantumForge.Runtime;

namespace QRG.QuantumForge.Platformer
{
    public class ShowPlayer : MonoBehaviour
    {
        [SerializeField] private ProbabilityTracker probabilityTracker;

        [SerializeField] private GameObject topPlayer;
        [SerializeField] private GameObject bottomPlayer;

        void Start()
        {
            if (probabilityTracker == null)
            {
                probabilityTracker = GetComponent<ProbabilityTracker>();
            }
            if(probabilityTracker == null)
            {
                Debug.LogError($"{gameObject.name}: No ProbabilityTracker found on this object. Set ProbabilityTracker to track");
            }

            if (topPlayer == null)
            {
                Debug.LogError($"{gameObject.name}: No topPlayer found on this object. Set topPlayer to track");
            }
            if(bottomPlayer == null)
            {
                Debug.LogError($"{gameObject.name}: No bottomPlayer found on this object. Set bottomPlayer to track");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (probabilityTracker != null)
            {
                var probabilities = probabilityTracker.Probabilities;
                if (probabilities != null && probabilities.Length > 0)
                {
                    float topProbability = 0.0f;
                    float bottomProbability = 0.0f;
                    topProbability = probabilities.First(x => x.BasisValues[0].Name == "top").Probability;
                    bottomProbability = probabilities.First(x => x.BasisValues[0].Name == "bottom").Probability;

                    topPlayer.SetActive(topProbability > 0.0f);
                    bottomPlayer.SetActive(bottomProbability > 0.0f);
                }
            }

        }
    }
}
