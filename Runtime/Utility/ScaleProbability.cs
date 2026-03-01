// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QRG.QuantumForge.Runtime;
using UnityEngine;

namespace QRG.QuantumForge.Utility
{
    public class ScaleProbability : MonoBehaviour
    {
        [SerializeField] private ProbabilityTracker _probabilityTracker;

        [SerializeField] private BasisValue _scaleOnValue;

        [SerializeField] private bool _scaleX;
        [SerializeField] private bool _scaleY;
        [SerializeField] private bool _scaleZ;

        private Vector3 _initialScale;

        // Start is called before the first frame update
        void Start()
        {
            _initialScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            var p = _probabilityTracker.Probabilities.Where(x => x.BasisValues[0] == _scaleOnValue);
            if (p.Any()) {
                var prob = p.First().Probability;
                transform.localScale = new Vector3(
                    _scaleX ? _initialScale.x * prob : _initialScale.x,
                    _scaleY ? _initialScale.y * prob : _initialScale.y,
                    _scaleZ ? _initialScale.z * prob : _initialScale.z
                );
            }

        }
    }
}
