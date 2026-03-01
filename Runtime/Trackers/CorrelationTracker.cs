// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QRG.QuantumForge.Runtime;
using Unity.VisualScripting;

namespace QRG.QuantumForge.Runtime
{
    /// <summary>
    /// Tracks correlations between specified quantum properties.
    /// </summary>
    public class CorrelationTracker : MonoBehaviour
    {
        /// <summary>
        /// Quantum properties to track correlations for.
        /// </summary>
        [Tooltip("Quantum properties to track correlations for.")]
        [SerializeField] private QuantumProperty[] quantumProperties;

        /// <summary>
        /// Matrix representing the correlations between quantum properties.
        /// </summary>
        [Tooltip("Matrix representing the correlations between quantum properties.")]
        [SerializeField] private float[,] correlationMatrix;

        /// <summary>
        /// Indicates whether the correlation matrix should be updated continuously.
        /// </summary>
        [Tooltip("Indicates whether the correlation matrix should be updated continuously.")]
        [SerializeField] private bool continuous = true;

        /// <summary>
        /// String representation of the correlation matrix for debugging purposes.
        /// </summary>
        [Tooltip("String representation of the correlation matrix for debugging purposes.")]
        [SerializeField, TextArea(5, 20)] private string matrixData = "";

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
        }

        /// <summary>
        /// Updates the correlation matrix if continuous tracking is enabled.
        /// </summary>
        void Update()
        {
            if (continuous)
            {
                UpdateCorrelationMatrix();
            }
        }

        /// <summary>
        /// Calculates and returns the correlation matrix of the quantum properties.
        /// </summary>
        /// <returns>The correlation matrix as a 2D float array.</returns>
        public float[,] UpdateCorrelationMatrix()
        {
            correlationMatrix = QuantumProperty.CorrelationMatrix(quantumProperties);
            SetMatrixData();
            return correlationMatrix;
        }

        /// <summary>
        /// Updates the string representation of the correlation matrix for debugging.
        /// </summary>
        private void SetMatrixData()
        {
            matrixData = "";
            for (int i = 0; i < correlationMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < correlationMatrix.GetLength(1); j++)
                {
                    matrixData += correlationMatrix[i, j].ToString("0.00") + " ";
                }
                matrixData += "\n";
            }
        }
    }
}

