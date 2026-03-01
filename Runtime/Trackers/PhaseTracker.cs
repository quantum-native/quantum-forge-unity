// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using QRG.QuantumForge.Runtime;
using Unity.VisualScripting;

namespace QRG.QuantumForge.Runtime
{
    /// <summary>
    /// Tracks phase information for specified quantum properties.
    /// </summary>
    public class PhaseTracker : MonoBehaviour
    {
        /// <summary>
        /// Quantum properties to track phase information for.
        /// </summary>
        [Tooltip("Quantum properties to track phase information for.")]
        [SerializeField] private QuantumProperty[] quantumProperties;

        /// <summary>
        /// Matrix representing the phase information between quantum properties.
        /// </summary>
        [Tooltip("Matrix representing the phase information between quantum properties.")]
        [SerializeField] private float[,] phaseMatrix;
        public float[,] PhaseMatrix => phaseMatrix;

        /// <summary>
        /// Indicates whether the phase matrix should be updated continuously.
        /// </summary>
        [Tooltip("Indicates whether the phase matrix should be updated continuously.")]
        [SerializeField] private bool continuous = true;

        /// <summary>
        /// Indicates whether the phase values are represented in radians.
        /// </summary>
        [Tooltip("Indicates whether the phase values are represented in radians.")]
        [SerializeField] private bool radians = true;

        /// <summary>
        /// String representation of the phase matrix for debugging purposes.
        /// </summary>
        [Tooltip("String representation of the phase matrix for debugging purposes.")]
        [SerializeField, TextArea(5,20)] private string matrixData = "";

        /// <summary>
        /// Initializes the tracker and ensures quantum properties are set.
        /// </summary>
        void OnEnable()
        {
            if (quantumProperties == null || quantumProperties.Length == 0)
            {
                var prop = GetComponent<QuantumProperty>();
                if (prop != null)
                {
                    quantumProperties = new QuantumProperty[] { prop };
                }
                else
                {
                    Debug.LogError($"{gameObject.name}: No NativeQuantumProperty found on this object. Set properties to track");
                }
            }

            int size = 1;
            foreach (var prop in quantumProperties)
            {
                size *= prop.Dimension;
            }
            phaseMatrix = new float[size, size];
        }

        /// <summary>
        /// Updates the phase matrix if continuous tracking is enabled.
        /// </summary>
        void Update()
        {
            if (continuous)
            {
                UpdatePhaseMatrix();
            }
        }

        /// <summary>
        /// Calculates and returns the phase matrix of the quantum properties.
        /// </summary>
        /// <returns>The phase matrix as a 2D float array.</returns>
        public float[,] UpdatePhaseMatrix()
        {
            var rdm = QuantumProperty.ReducedDensityMatrix(quantumProperties);
            for (int i = 0; i < rdm.GetLength(0); ++i)
            {
                for (int j = 0; j < rdm.GetLength(1); ++j)
                {
                    phaseMatrix[i,j] = (float)rdm[i, j].Phase;
                    if(!radians)
                    {
                        phaseMatrix[i, j] *= 180 / Mathf.PI;
                    }
                }
            }
            SetMatrixData();
            return phaseMatrix;
        }

        /// <summary>
        /// Updates the string representation of the phase matrix for debugging.
        /// </summary>
        private void SetMatrixData()
        {
            matrixData = "";
            for(int i = 0; i < phaseMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < phaseMatrix.GetLength(1); j++)
                {
                    matrixData += phaseMatrix[i, j].ToString("0.00") + " ";
                }
                matrixData += "\n";
            }
        }
    }
}

