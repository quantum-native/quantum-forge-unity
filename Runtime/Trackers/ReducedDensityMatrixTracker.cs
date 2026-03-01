// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using QRG.QuantumForge.Runtime;
using Unity.VisualScripting;

namespace QRG.QuantumForge.Runtime
{
    /// <summary>
    /// Tracks the reduced density matrix of specified quantum properties.
    /// </summary>
    public class ReducedDensityMatrixTracker : MonoBehaviour
    {
        /// <summary>
        /// Quantum properties to track the reduced density matrix for.
        /// </summary>
        [Tooltip("Quantum properties to track the reduced density matrix for.")]
        [SerializeField] private QuantumProperty[] quantumProperties;

        /// <summary>
        /// Matrix representing the reduced density matrix of the quantum properties.
        /// </summary>
        [Tooltip("Matrix representing the reduced density matrix of the quantum properties.")]
        [SerializeField] private Complex[,] reducedDensityMatrix;
        public Complex[,] ReducedDensityMatrix => reducedDensityMatrix;

        /// <summary>
        /// Indicates whether the reduced density matrix should be updated continuously.
        /// </summary>
        [Tooltip("Indicates whether the reduced density matrix should be updated continuously.")]
        [SerializeField] private bool continuous = true;

        /// <summary>
        /// String representation of the reduced density matrix for debugging purposes.
        /// </summary>
        [Tooltip("String representation of the reduced density matrix for debugging purposes.")]
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
        }

        /// <summary>
        /// Updates the reduced density matrix if continuous tracking is enabled.
        /// </summary>
        void Update()
        {
            if (continuous)
            {
                GetReducedDensityMatrix();
            }
        }

        /// <summary>
        /// Calculates and returns the reduced density matrix of the quantum properties.
        /// </summary>
        /// <returns>The reduced density matrix as a 2D complex array.</returns>
        public Complex[,] GetReducedDensityMatrix()
        {
            reducedDensityMatrix = QuantumProperty.ReducedDensityMatrix(quantumProperties);
            SetMatrixData();
            return reducedDensityMatrix;
        }

        /// <summary>
        /// Updates the string representation of the reduced density matrix for debugging.
        /// </summary>
        private void SetMatrixData()
        {
            matrixData = "";
            for(int i = 0; i < reducedDensityMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < reducedDensityMatrix.GetLength(1); j++)
                {
                    matrixData += "(" + reducedDensityMatrix[i, j].Real.ToString("0.00") + "," + reducedDensityMatrix[i, j].Imaginary.ToString("0.00") + ") ";
                }
                matrixData += "\n";
            }
        }
    }
}

