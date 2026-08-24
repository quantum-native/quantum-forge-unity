// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using QRG.QuantumForge.Core;
using Unity.Properties;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace QRG.QuantumForge.Runtime
{
    using QuantumForge = QuantumForge.Core.QuantumForge;

    /// <summary>
    /// Represents a condition or predicate for quantum operations.
    /// </summary>
    [Tooltip("Represents a condition or predicate for quantum operations.")]
    [Serializable]
    public class Predicate
    {
        /// <summary>
        /// The quantum property associated with this predicate.
        /// </summary>
        [Tooltip("The quantum property associated with this predicate.")]
        public QuantumProperty property = null;

        /// <summary>
        /// The basis value to compare against.
        /// </summary>
        [Tooltip("The basis value to compare against.")]
        [BasisValueDropdown]
        public BasisValue value;

        /// <summary>
        /// Indicates whether the predicate checks for equality or inequality.
        /// </summary>
        [Tooltip("Indicates whether the predicate checks for equality or inequality.")]
        public bool is_equal;
    }

    /// <summary>
    /// Gives an object the ability to exist in a quantum state.
    /// </summary>
    [Tooltip("Gives an object the ability to exist in a quantum state.")]
    [Serializable]
    public class QuantumProperty : MonoBehaviour
    {
        /// <summary>
        /// The native quantum property associated with this object.
        /// </summary>
        private QuantumForge.NativeQuantumProperty _nativeNativeQuantumProperty;

        /// <summary>
        /// The basis associated with this quantum property.
        /// </summary>
        [Tooltip("The basis associated with this quantum property.")]
        public Basis basis = null;

        /// <summary>
        /// The initial basis value for this quantum property.
        /// </summary>
        [Tooltip("The initial basis value for this quantum property.")]
        [SerializeField, BasisValueDropdown] private BasisValue Initial;

        /// <summary>
        /// The dimension, or number of basis values, of the quantum property.
        /// </summary>
        [Tooltip("The dimension of the basis associated with this quantum property.")]
        public int Dimension
        {
            get => basis.Dimension;
        }

        void Awake()
        {
            try
            {
                if (basis == null)
                {
                    throw new Exception("Basis not set. Try setting basis in Editor. Reload/recompile sometimes corrupts this field.");
                }
                int initial = basis.values.IndexOf(Initial);
                if(initial == -1)
                {
                    initial = 0;
                    throw new Exception($"Initial value {Initial.Name} not found in basis, setting it to {basis.values[0].Name}. Try selecting initial value again in Editor. Reload/recompile sometimes corrupts this field.");
                }
                _nativeNativeQuantumProperty = new QuantumForge.NativeQuantumProperty(Dimension, initial);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(gameObject.name + ": " + e.Message);
                _nativeNativeQuantumProperty = null;
            }

        }

        /// <summary>
        /// Creates a predicate that checks if the quantum property has the specified basis value.
        /// </summary>
        /// <param name="value">The basis value to check.</param>
        /// <returns>A predicate representing the condition.</returns>
        public Predicate is_value(BasisValue value)
        {
            return new Predicate()
            {
                property = this,
                value = value,
                is_equal = true
            };
        }

        /// <summary>
        /// Creates a predicate that checks if the quantum property has the specified basis value by name.
        /// </summary>
        /// <param name="valueName">The name of the basis value to check.</param>
        /// <returns>A predicate representing the condition.</returns>
        public Predicate is_value(string valueName)
        {
            return is_value(basis.values.Find(v => v.Name == valueName));
        }

        /// <summary>
        /// Creates a predicate that checks if the quantum property has the specified basis value by index.
        /// </summary>
        /// <param name="valueIndex">The index of the basis value to check.</param>
        /// <returns>A predicate representing the condition.</returns>
        public Predicate is_value(int valueIndex)
        {
            return is_value(basis.values[valueIndex]);
        }

        /// <summary>
        /// Creates a predicate that checks if the quantum property does not have the specified basis value.
        /// </summary>
        /// <param name="value">The basis value to check against.</param>
        /// <returns>A predicate representing the condition.</returns>
        public Predicate is_not_value(BasisValue value)
        {
            return new Predicate()
            {
                property = this,
                value = value,
                is_equal = false
            };
        }

        /// <summary>
        /// Creates a predicate that checks if the quantum property does not have the specified basis value by name.
        /// </summary>
        /// <param name="valueName">The name of the basis value to check against.</param>
        /// <returns>A predicate representing the condition.</returns>
        public Predicate is_not_value(string valueName)
        {
            return is_not_value(basis.values.Find(v => v.Name == valueName));
        }

        /// <summary>
        /// Creates a predicate that checks if the quantum property does not have the specified basis value by index.
        /// </summary>
        /// <param name="value">The index of the basis value to check against.</param>
        /// <returns>A predicate representing the condition.</returns>
        public Predicate is_not_value(int value)
        {
            return is_not_value(basis.values[value]);
        }

        /// <summary>
        /// Owns the native predicate handles built for a single quantum operation.
        /// </summary>
        /// <remarks>
        /// Every <see cref="QuantumForge.Predicate"/> produced by
        /// <c>NativeQuantumProperty.is_value</c> / <c>is_not_value</c> wraps a native
        /// handle that must be released with <c>Dispose()</c>, or the package leaks
        /// native memory on every gate application. This scope owns those handles for
        /// exactly the duration of the operation, so ALWAYS consume
        /// <see cref="ConvertPredicates"/> with <c>using</c>:
        /// <code>
        /// using (var preds = ConvertPredicates(predicates))
        /// {
        ///     QuantumForge.Cycle(native, preds.Native);
        /// }
        /// </code>
        /// The <c>using</c> guarantees disposal even when the native call throws, and
        /// <see cref="Dispose"/> is idempotent per handle (<c>Predicate.Dispose</c>
        /// nulls its handle), so a double dispose cannot double-free.
        /// </remarks>
        internal readonly struct PredicateScope : IDisposable
        {
            private readonly QuantumForge.Predicate[] _native;

            internal PredicateScope(QuantumForge.Predicate[] native)
            {
                _native = native;
            }

            /// <summary>
            /// The native predicates. Valid only until this scope is disposed -- never
            /// store this array, only pass it straight into a QuantumForge call.
            /// </summary>
            internal QuantumForge.Predicate[] Native => _native;

            public void Dispose()
            {
                if (_native == null)
                {
                    return;
                }

                for (int i = 0; i < _native.Length; ++i)
                {
                    _native[i]?.Dispose();
                }
            }
        }

        /// <summary>
        /// Builds the native predicates for <paramref name="predicates"/>.
        /// The caller OWNS the returned scope and must dispose it (use <c>using</c>).
        /// </summary>
        internal static PredicateScope ConvertPredicates(Predicate[] predicates)
        {
            if (predicates == null || predicates.Length == 0)
            {
                // Fast path: no native handles are created, so there is nothing to
                // free. Array.Empty allocates nothing and disposing is a no-op.
                return new PredicateScope(Array.Empty<QuantumForge.Predicate>());
            }

            var native = new QuantumForge.Predicate[predicates.Length];
            try
            {
                for (int i = 0; i < predicates.Length; ++i)
                {
                    var p = predicates[i];
                    if (p == null || p.property == null)
                    {
                        throw new ArgumentException(
                            $"Predicate at index {i} is null or has no QuantumProperty assigned.",
                            nameof(predicates));
                    }

                    var nativeProperty = p.property._nativeNativeQuantumProperty;
                    if (nativeProperty == null)
                    {
                        throw new InvalidOperationException(
                            $"QuantumProperty '{p.property.name}' has no native quantum property " +
                            "(its Awake failed -- check the basis and initial value in the Editor).");
                    }

                    int valueIndex = p.property.basis.values.IndexOf(p.value);
                    native[i] = p.is_equal
                        ? nativeProperty.is_value(valueIndex)
                        : nativeProperty.is_not_value(valueIndex);
                }
            }
            catch
            {
                // Do not leak the handles that were created before the failure.
                new PredicateScope(native).Dispose();
                throw;
            }

            return new PredicateScope(native);
        }

        public static void Cycle(QuantumProperty prop, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Cycle(prop._nativeNativeQuantumProperty, preds.Native);
            }
        }

        public void Cycle(params Predicate[] predicates)
        {
            Cycle(this, predicates);
        }

        public static void Cycle(QuantumProperty prop, float fraction, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Cycle(prop._nativeNativeQuantumProperty, fraction, preds.Native);
            }
        }

        public void Cycle(float fraction, params Predicate[] predicates)
        {
            Cycle(this, fraction, predicates);
        }

        public static void Shift(QuantumProperty prop, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Shift(prop._nativeNativeQuantumProperty, preds.Native);
            }
        }

        public void Shift(params Predicate[] predicates)
        {
            Shift(this, predicates);
        }

        public static void Shift(QuantumProperty prop, float fraction, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Shift(prop._nativeNativeQuantumProperty, fraction, preds.Native);
            }
        }

        public void Shift(float fraction, params Predicate[] predicates)
        {
            Shift(this, fraction, predicates);
        }

        public static void Clock(QuantumProperty property, float fraction, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Clock(property._nativeNativeQuantumProperty, fraction, preds.Native);
            }
        }

        /// <summary>
        /// The full qudit Z gate.
        /// Applies a phase rotation to all basis values, based on the value_string.
        /// Let w = exp(2*pi*i/Dimension)
        /// For basis value_string v, the phase rotation is w^v
        /// </summary>
        public void Clock(float fraction, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Clock(_nativeNativeQuantumProperty, fraction, preds.Native);
            }
        }

        public static void Clock(QuantumProperty property, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Clock(property._nativeNativeQuantumProperty, preds.Native);
            }
        }

        /// <summary>
        /// The full qudit Z gate.
        /// Applies a phase rotation to all basis values, based on the value_string.
        /// Let w = exp(2*pi*i/Dimension)
        /// For basis value_string v, the phase rotation is w^v
        /// </summary>
        public void Clock(params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Clock(_nativeNativeQuantumProperty, preds.Native);
            }
        }

        public static void X(QuantumProperty prop, float fraction, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.X(prop._nativeNativeQuantumProperty, fraction, preds.Native);
            }
        }

        public void X(float fraction, params Predicate[] predicates)
        {
            X(this, fraction, predicates);
        }

        public static void X(QuantumProperty prop, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.X(prop._nativeNativeQuantumProperty, preds.Native);
            }
        }

        public void X(params Predicate[] predicates)
        {
            X(this, predicates);
        }

        public static void Z(QuantumProperty prop, float fraction, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Z(prop._nativeNativeQuantumProperty, fraction, preds.Native);
            }
        }

        public void Z(float fraction, params Predicate[] predicates)
        {
            Z(this, fraction, predicates);
        }

        public static void Z(QuantumProperty prop, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Z(prop._nativeNativeQuantumProperty, preds.Native);
            }
        }

        public void Z(params Predicate[] predicates)
        {
            Z(this, predicates);
        }

        public static void Y(QuantumProperty prop, float fraction, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Y(prop._nativeNativeQuantumProperty, fraction, preds.Native);
            }
        }

        public void Y(float fraction, params Predicate[] predicates)
        {
            Y(this, fraction, predicates);
        }

        public static void Y(QuantumProperty prop, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Y(prop._nativeNativeQuantumProperty, preds.Native);
            }
        }

        public void Y(params Predicate[] predicates)
        {
            Y(this, predicates);
        }

        public static void Hadamard(QuantumProperty prop, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Hadamard(prop._nativeNativeQuantumProperty, preds.Native);
            }
        }

        public void Hadamard(params Predicate[] predicates)
        {
            Hadamard(this, predicates);
        }

        public static void Hadamard(QuantumProperty prop, float fraction, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Hadamard(prop._nativeNativeQuantumProperty, fraction, preds.Native);
            }
        }

        public void Hadamard(float fraction, params Predicate[] predicates)
        {
            Hadamard(this, fraction, predicates);
        }

        public static void InverseHadamard(QuantumProperty prop, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.InverseHadamard(prop._nativeNativeQuantumProperty, preds.Native);
            }
        }

        public void InverseHadamard(params Predicate[] predicates)
        {
            InverseHadamard(this, predicates);
        }

        public static void PhaseRotate(float angle, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.PhaseRotate(angle, preds.Native);
            }
        }

        public static void Swap(QuantumProperty prop1, QuantumProperty prop2, params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.Swap(prop1._nativeNativeQuantumProperty, prop2._nativeNativeQuantumProperty,
                    preds.Native);
            }
        }

        public static void ISwap(QuantumProperty prop1, QuantumProperty prop2)
        {
            ISwap(prop1, prop2, 1.0f);
        }

        public static void ISwap(QuantumProperty prop1, QuantumProperty prop2, params Predicate[] predicates)
        {
            ISwap(prop1, prop2, 1.0f, predicates);
        }

        public static void ISwap(QuantumProperty prop1, QuantumProperty prop2, float fraction)
        {
            ISwap(prop1, prop2, fraction, Array.Empty<Predicate>());
        }

        public static void ISwap(QuantumProperty prop1, QuantumProperty prop2, float fraction,
            params Predicate[] predicates)
        {
            using (var preds = ConvertPredicates(predicates))
            {
                QuantumForge.ISwap(prop1._nativeNativeQuantumProperty, prop2._nativeNativeQuantumProperty, fraction,
                    preds.Native);
            }
        }

        /// <summary>
        /// Entangling operation. Performs a number of predicated (controlled) cycles on prop2 based on the value_string of prop1.
        /// Ex. Prop1 is in superposition of 0, 1, and 2 : |prop1> = 1/sqrt(3) * (|0> + |1> + |2>)
        ///     Prop2 starts in state 0: |prop2> = |0>
        ///     Result: |prop1,prop2> = 1/sqrt(3) * (|0,0> + |1,1> + |2,2>)
        /// Note: if prop2 starts in a different state, the result will be a different entanglement structure.
        /// Ex. Prop2 starts in state 1: |prop2> = |1>
        ///     Result: |prop1,prop2> = 1/sqrt(3) * (|0,1> + |1,2> + |2,0>)
        /// </summary>
        /// <param Name="prop1"></param>
        /// <param Name="prop2"></param>
        public static void NCycle(QuantumProperty prop1, QuantumProperty prop2)
        {
            QuantumForge.NCycle(prop1._nativeNativeQuantumProperty, prop2._nativeNativeQuantumProperty);
        }

        public static void Reset(QuantumProperty prop, int currentValue)
        {
            QuantumForge.Reset(prop._nativeNativeQuantumProperty, currentValue);
        }

        public void Reset(int currentValue)
        {
            Reset(this, currentValue);
        }

        [Serializable]
        public struct BasisProbability
        {
            public float Probability;
            public BasisValue[] BasisValues;
            [SerializeField] private string _basisValues; // Editor conveinence

            public BasisProbability(float probability, BasisValue[] basisValues)
            {
                Probability = probability;
                BasisValues = basisValues;
                _basisValues = string.Join(",", basisValues.Select(x => x.Name));
            }

            public override string ToString()
            {
                return $"{Probability.ToString("0.00")} : {string.Join(",", BasisValues.Select(x => x.Name))}";
            }
        }

        public static BasisProbability[] Probabilities(params QuantumProperty[] properties)
        {
            if (properties == null || properties.Length == 0)
            {
                Debug.LogWarning("No properties provided to calculate probabilities");
                return new BasisProbability[0];
            }
            var props = Array.ConvertAll(properties, p => p._nativeNativeQuantumProperty);
            var probs = QuantumForge.Probabilities(props);
            var numValues = properties.Length;
            var result = new BasisProbability[probs.Length];
            for (int i = 0; i < probs.Length; i++)
            {
                var values = new BasisValue[properties.Length];
                for (int j = 0; j < properties.Length; ++j)
                {
                    values[j] = properties[j].basis.values[probs[i].QuditValues[j]];
                }

                result[i] = new BasisProbability(probs[i].Probability, values);
            }

            return result;
        }

        public static Complex[,] ReducedDensityMatrix(params QuantumProperty[] properties)
        {
            var props = Array.ConvertAll(properties, p => p._nativeNativeQuantumProperty);
            return QuantumForge.ReducedDensityMatrix(props);
        }

        public static float[] MutualInformation(params QuantumProperty[] properties)
        {
            var props = Array.ConvertAll(properties, p => p._nativeNativeQuantumProperty);
            return QuantumForge.MutualInformation(props);
        }

        public static float[,] CorrelationMatrix(params QuantumProperty[] properties)
        {
            var props = Array.ConvertAll(properties, p => p._nativeNativeQuantumProperty);
            return QuantumForge.CorrelationMatrix(props);
        }

        public static int[] Measure(params QuantumProperty[] properties)
        {
            var props = Array.ConvertAll(properties, p => p._nativeNativeQuantumProperty);
            return QuantumForge.Measure(props);
        }

        public static int[] MeasureProperties(params QuantumProperty[] properties)
        {
            return Measure(properties);
        }

        public static int Measure(params Predicate[] predicates)
        {
            // The predicates must outlive the native call but not the return: `using`
            // disposes them after Measure produces its value, and also if it throws.
            using (var preds = ConvertPredicates(predicates))
            {
                return QuantumForge.Measure(preds.Native);
            }
        }

        public static int MeasurePredicate(params Predicate[] predicates)
        {
            return Measure(predicates);
        }

    }
}
