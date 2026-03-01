// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using QRG.QuantumForge.Runtime;
using UnityEngine;

namespace QRG.QuantumForge.Utility
{
    public class RotateOnPhase : MonoBehaviour
    {
        [SerializeField] private PhaseTracker _phaseTracker;
        [SerializeField] private Vector2Int _phaseMatrixIndex;

        [SerializeField] private float _phase;

        [SerializeField] private bool _rotateX;
        [SerializeField] private bool _rotateY;
        [SerializeField] private bool _rotateZ;

        private Vector3 _initialRotation;

        // Start is called before the first frame update
        void Start()
        {
            _initialRotation = transform.localEulerAngles;
        }

        // Update is called once per frame
        void Update()
        {
            if (_phaseTracker == null)
            {
                Debug.Log("Set phase tracker to rotate by phase.");
                return;
            }
            var phaseMatrix = _phaseTracker.PhaseMatrix;
            if (phaseMatrix != null)
            {
                _phase = phaseMatrix[_phaseMatrixIndex.x, _phaseMatrixIndex.y];
                var rotation = _initialRotation;
                var angle = _phase * 180 / Mathf.PI;
                if (_rotateX)
                {
                    rotation.x += angle;
                }
                if (_rotateY)
                {
                    rotation.y += angle;
                }
                if (_rotateZ)
                {
                    rotation.z += angle;
                }
                transform.localEulerAngles = rotation;
            }
        }
    }
}
