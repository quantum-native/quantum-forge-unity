// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QRG.QuantumForge.Runtime;
using Unity.VisualScripting;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace QRG.QuantumForge.Roshambo
{
 
    [RequireComponent(typeof(ProbabilityTracker))]
    public class TokenDisplay : MonoBehaviour
    {
        [SerializeField][Range(0,1)] private float delay = 0.1f;
        [SerializeField] private Image player1;
        [SerializeField] private Image player2;

        [SerializeField] private Sprite rock; 
        [SerializeField] private Sprite paper; 
        [SerializeField] private Sprite scissors;

        private ProbabilityTracker probabilityTracker;

        void OnEnable()
        {
            probabilityTracker = GetComponent<ProbabilityTracker>();
        }

        void Start()
        {
            StartCoroutine(UpdateTokenCoroutine());
        }

        void UpdateTokens()
        {
            if (probabilityTracker != null)
            {
                var rand = Random.value;
                var probs = probabilityTracker.Probabilities;
                if (probs != null && probs.Length != 0)
                {
                    float totalProb = 0;
                    foreach (var basisProbability in probs)
                    {
                        totalProb += basisProbability.Probability;
                        if (rand < totalProb)
                        {
                            player1.sprite = basisProbability.BasisValues[0].Name switch
                            {
                                "rock" => rock,
                                "paper" => paper,
                                "scissors" => scissors,
                                _ => null
                            };

                            player2.sprite = basisProbability.BasisValues[1].Name switch
                            {
                                "rock" => rock,
                                "paper" => paper,
                                "scissors" => scissors,
                                _ => null
                            };
                            break;
                        }
                    }
                    

                    
                }
            }
        }

        IEnumerator UpdateTokenCoroutine()
        {
            while (true)
            {
                UpdateTokens();
                yield return new WaitForSeconds(delay);
            }
        }

    }
}
