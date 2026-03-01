// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;

namespace QRG.QuantumForge.Core
{
    public static class QuantumForge
    {
        // Define the qforge_error enum
        public enum QForgeError
        {
            QFORGE_ERR_NONE,
            QFORGE_ERR_NULL_POINTER,
            QFORGE_ERR_INVALID_ARGUMENT,
            QFORGE_ERR_BUFFER_TOO_SMALL,
            QFORGE_ERR_TARGET_CONTROL_OVERLAP,
            QFORGE_ERR_INCOMPATIBLE_DIMENSIONS,
            QFORGE_ERR_BAD_DIMENSION,
            QFORGE_ERR_BAD_QUDIT_NUMBER,
            QFORGE_ERR_OUTPUT_BUFFER_SIZE_NOT_EQUAL_TO_PERMUTATIONS,
            QFORGE_ERR_MAX_STATE_SIZE_EXCEEDED
        }

        // Define the NativeBasisProbability struct
        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeBasisProbability
        {
            internal float Probability;
            internal IntPtr QuditValues; // Pointer to an array of integers
        }

        public readonly struct BasisProbability
        {
            public readonly float Probability;
            public readonly int[] QuditValues;

            // Constructor to convert from NativeBasisProbability to BasisProbability
            internal BasisProbability(NativeBasisProbability nativeBasisProbability, int size)
            {
                Probability = nativeBasisProbability.Probability;
                QuditValues = new int[size];
                Marshal.Copy(nativeBasisProbability.QuditValues, QuditValues, 0, QuditValues.Length);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativePredicate
        {
            internal IntPtr Property; // Corresponds to NativeQuantumProperty* property
            internal int Value; // Corresponds to int value
            [MarshalAs(UnmanagedType.I1)] internal bool IsEqual; // Corresponds to bool is_equal

            // Constructor to convert from Predicate to NativePredicate
            internal NativePredicate(Predicate predicate)
            {
                Property = predicate.Property.Handle;
                Value = predicate.Value;
                IsEqual = predicate.IsEqual;
            }
        }

        public struct Predicate
        {
            public NativeQuantumProperty Property;
            public int Value;
            public bool IsEqual;

            public Predicate(NativeQuantumProperty property, int value, bool isEqual)
            {
                Property = property;
                Value = value;
                IsEqual = isEqual;
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE
   const string QUANTUM_FORGE_LIB = "quantum-forge";
#elif UNITY_WEBGL && !UNITY_EDITOR
   const string QUANTUM_FORGE_LIB = "__Internal";
#endif

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern IntPtr qforge_make_quantum_property(int dimension, out QForgeError err);

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeError qforge_free_quantum_property(IntPtr quantumProperty);

        // Define the NativeQuantumProperty class as a wrapper
        public class NativeQuantumProperty : IDisposable
        {
            internal IntPtr Handle { get; private set; }
            public readonly int Dimension;

            public NativeQuantumProperty(int dimension)
            {
                Handle = qforge_make_quantum_property(dimension, out QForgeError error);
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error creating quantum property: {error}");
                }

                Dimension = dimension;
                Debug.Log($"QuantumForge: Created NativeQuantumProperty of dimension {dimension}. Handle: {Handle}");
            }

            public NativeQuantumProperty(int dimension, int initial)
            {
                if (initial >= dimension || initial < 0)
                {
                    throw new InvalidOperationException(
                        $"Error creating quantum property: Make sure Initial value {initial} is between 0 and {dimension}?");
                }

                Handle = qforge_make_quantum_property(dimension, out QForgeError error);
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error creating quantum property: {error}");
                }

                Dimension = dimension;

                var m = Measure(this);
                while (m[0] != initial)
                {
                    Cycle(this, 1.0f);
                    m = Measure(this);
                }

                Debug.Log(
                    $"QuantumForge: Created NativeQuantumProperty of dimension {dimension}, with initial value {initial}. Handle: {Handle}");
            }

            public void Dispose()
            {
                if (Handle != IntPtr.Zero)
                {
                    Debug.Log($"QuantumForge: Destroying NativeQuantumProperty with handle {Handle}");
                    qforge_free_quantum_property(Handle);
                    Handle = IntPtr.Zero;
                }
            }

            public Predicate is_value(int value)
            {
                return new Predicate(this, value, true);
            }

            public Predicate is_not_value(int value)
            {
                return new Predicate(this, value, false);
            }
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_cycle(IntPtr prop, float fraction, [In] NativePredicate[] preds, int preds_len);

        public static void Cycle(NativeQuantumProperty prop, float fraction, params Predicate[] preds)
        {
            GCHandle handle = default;
            NativePredicate[] nativePreds = null;
            try
            {
                if (preds != null)
                {
                    nativePreds = preds.Select(p => new NativePredicate(p)).ToArray();
                    handle = GCHandle.Alloc(nativePreds, GCHandleType.Pinned);
                }

                QForgeError error = qforge_cycle(prop.Handle, fraction, nativePreds, (nativePreds?.Length ?? 0));
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in Cycle: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        public static void Cycle(NativeQuantumProperty prop, params Predicate[] preds)
        {
            Cycle(prop, 1.0f, preds);
        }

        public static void NCycle(NativeQuantumProperty prop1, NativeQuantumProperty prop2, float fraction = 1.0f)
        {
            for (int i = 0; i < prop1.Dimension; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    Cycle(prop2, fraction, prop1.is_value(i));
                }
            }
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_shift(IntPtr prop, float fraction, [In] NativePredicate[] preds, int preds_len);

        public static void Shift(NativeQuantumProperty prop, float fraction, params Predicate[] preds)
        {
            GCHandle handle = default;
            NativePredicate[] nativePreds = null;
            try
            {
                if (preds != null)
                {
                    nativePreds = preds.Select(p => new NativePredicate(p)).ToArray();
                    handle = GCHandle.Alloc(nativePreds, GCHandleType.Pinned);
                }

                QForgeError error = qforge_shift(prop.Handle, fraction, nativePreds, (nativePreds?.Length ?? 0));
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in Shift: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        public static void Shift(NativeQuantumProperty prop, params Predicate[] preds)
        {
            Shift(prop, 1.0f, preds);
        }

        public static void NShift(NativeQuantumProperty prop1, NativeQuantumProperty prop2, float fraction = 1.0f)
        {
            for (int i = 0; i < prop1.Dimension; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    Shift(prop2, 1.0f, prop1.is_value(i));
                }
            }
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_clock(IntPtr prop, float fraction, [In] NativePredicate[] preds, int preds_len);

        public static void Clock(NativeQuantumProperty prop, float fraction, params Predicate[] preds)
        {
            GCHandle handle = default;
            NativePredicate[] nativePreds = null;
            try
            {
                if (preds != null)
                {
                    nativePreds = preds.Select(p => new NativePredicate(p)).ToArray();
                    handle = GCHandle.Alloc(nativePreds, GCHandleType.Pinned);
                }

                QForgeError error = qforge_clock(prop.Handle, fraction, nativePreds, (nativePreds?.Length ?? 0));
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in Clock: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        public static void Clock(NativeQuantumProperty prop, params Predicate[] preds)
        {
            Clock(prop, 1.0f, preds);
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_hadamard(IntPtr prop, NativePredicate[] preds, UIntPtr preds_len);

        public static void Hadamard(NativeQuantumProperty prop, params Predicate[] preds)
        {
            GCHandle handle = default;
            NativePredicate[] nativePreds = null;
            try
            {
                if (preds != null)
                {
                    nativePreds = preds.Select(p => new NativePredicate(p)).ToArray();
                    handle = GCHandle.Alloc(nativePreds, GCHandleType.Pinned);
                }

                QForgeError error = qforge_hadamard(prop.Handle, nativePreds, (UIntPtr)(preds?.Length ?? 0));
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in Hadamard: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        public static void Hadamard(NativeQuantumProperty prop, float fraction, params Predicate[] preds)
        {
            if (Mathf.Approximately(fraction, 1.0f))
            {
                Hadamard(prop, preds);
                return;
            }

            throw new NotSupportedException(
                "Fractional Hadamard is not supported by the currently bundled Unity native plugins.");
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_inverse_hadamard(IntPtr prop, NativePredicate[] preds, UIntPtr preds_len);

        public static void InverseHadamard(NativeQuantumProperty prop, params Predicate[] preds)
        {
            GCHandle handle = default;
            NativePredicate[] nativePreds = null;
            try
            {
                if (preds != null)
                {
                    nativePreds = preds.Select(p => new NativePredicate(p)).ToArray();
                    handle = GCHandle.Alloc(nativePreds, GCHandleType.Pinned);
                }

                QForgeError error = qforge_inverse_hadamard(prop.Handle, nativePreds, (UIntPtr)(preds?.Length ?? 0));
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in Hadamard: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_phase_rotate(NativePredicate[] preds, UIntPtr preds_len, float angle);

        public static void PhaseRotate(float angle, params Predicate[] preds)
        {
            if (preds == null)
            {
                throw new ArgumentNullException(nameof(preds), "Predicates array cannot be null for PhaseRotate.");
            }

            GCHandle handle = default;

            try
            {
                handle = GCHandle.Alloc(preds, GCHandleType.Pinned);

                NativePredicate[] nativePreds = Array.ConvertAll(preds, p => new NativePredicate(p));

                QForgeError error = qforge_phase_rotate(nativePreds, (UIntPtr)preds.Length, angle);
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in PhaseRotate: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_swap(IntPtr p1, IntPtr p2, NativePredicate[] preds, UIntPtr preds_len);

        public static void Swap(NativeQuantumProperty p1, NativeQuantumProperty p2, params Predicate[] preds)
        {
            GCHandle handle = default;
            NativePredicate[] nativePreds = null;

            try
            {
                if (preds != null)
                {
                    nativePreds = preds.Select(p => new NativePredicate(p)).ToArray();
                    handle = GCHandle.Alloc(nativePreds, GCHandleType.Pinned);
                }

                QForgeError error = qforge_swap(p1.Handle, p2.Handle, nativePreds, (UIntPtr)(preds?.Length ?? 0));
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in Swap: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_i_swap(IntPtr p1, IntPtr p2, float fraction, NativePredicate[] preds,
            UIntPtr preds_len);

        public static void ISwap(NativeQuantumProperty p1, NativeQuantumProperty p2, float fraction, params Predicate[] preds)
        {
            GCHandle handle = default;
            NativePredicate[] nativePreds = null;

            try
            {
                if (preds != null)
                {
                    nativePreds = preds.Select(p => new NativePredicate(p)).ToArray();
                    handle = GCHandle.Alloc(nativePreds, GCHandleType.Pinned);
                }

                QForgeError error = qforge_i_swap(p1.Handle, p2.Handle, fraction, nativePreds, (UIntPtr)(preds?.Length ?? 0));
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in ISwap: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        public static void ISwap(NativeQuantumProperty p1, NativeQuantumProperty p2, params Predicate[] preds)
        {
            ISwap(p1, p2, 1.0f, preds);
        }

        public static void Reset(NativeQuantumProperty prop, int currentValue)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop));
            }
            if (currentValue < 0 || currentValue >= prop.Dimension)
            {
                throw new ArgumentOutOfRangeException(nameof(currentValue),
                    $"currentValue must be between 0 and {prop.Dimension - 1}.");
            }

            int steps = (prop.Dimension - currentValue) % prop.Dimension;
            for (int i = 0; i < steps; ++i)
            {
                Cycle(prop);
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_measure(IntPtr[] props, UIntPtr props_len, int[] output);

        // Public method to expose the measure function
        public static int[] Measure(params NativeQuantumProperty[] props)
        {
            IntPtr[] propHandles = Array.ConvertAll(props, p => p.Handle);
            int[] output = new int[props.Length];
            try
            {
                QForgeError error = qforge_measure(propHandles, (UIntPtr)props.Length, output);
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in Measure: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }

            return output;
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_stochastic_projection(NativePredicate[] preds, UIntPtr preds_len, int[] output);

        // Public method to expose the predicated measure function
        public static int Measure(Predicate[] preds)
        {
            GCHandle handle = default;
            NativePredicate[] nativePreds = null;

            int[] output = new int[1];
            try
            {
                if (preds != null)
                {
                    nativePreds = preds.Select(p => new NativePredicate(p)).ToArray();
                    handle = GCHandle.Alloc(nativePreds, GCHandleType.Pinned);
                }
                
                QForgeError error = qforge_stochastic_projection(nativePreds, (UIntPtr)(preds.Length), output);
                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in predicated Measure: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

            return output[0];
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_probabilities(IntPtr[] props, UIntPtr props_len, IntPtr output);

        // Public method to expose the probabilities function
        public static BasisProbability[] Probabilities(params NativeQuantumProperty[] props)
        {
            if (props == null || props.Length == 0)
            {
                Debug.LogWarning("No properties provided to calculate probabilities");
                return new BasisProbability[0];
            }
            BasisProbability[] result = null;
            IntPtr[] propHandles = Array.ConvertAll(props, p => p.Handle);
            int numProbabilities = 1;
            foreach (var prop in props)
            {
                numProbabilities *= prop.Dimension;
            }

            int quditValuesSize = props.Length;

            // Allocate memory for the array of NativeBasisProbability
            IntPtr output = Marshal.AllocHGlobal(Marshal.SizeOf<NativeBasisProbability>() * numProbabilities);

            // Allocate memory for each qudit_values array within the NativeBasisProbability structures
            for (int i = 0; i < numProbabilities; i++)
            {
                IntPtr probPtr = IntPtr.Add(output, i * Marshal.SizeOf<NativeBasisProbability>());
                NativeBasisProbability prob = new NativeBasisProbability();
                prob.QuditValues = Marshal.AllocHGlobal(quditValuesSize * sizeof(int));
                Marshal.StructureToPtr(prob, probPtr, false);
            }

            try
            {
                QForgeError error = qforge_probabilities(propHandles, (UIntPtr)props.Length, output);

                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in Probabilities: {error}");
                }

            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                // Process the output
                NativeBasisProbability[] probabilities = new NativeBasisProbability[numProbabilities];
                for (int i = 0; i < numProbabilities; i++)
                {
                    IntPtr probPtr = IntPtr.Add(output, i * Marshal.SizeOf<NativeBasisProbability>());
                    NativeBasisProbability prob = Marshal.PtrToStructure<NativeBasisProbability>(probPtr);

                    // Marshal the qudit_values array back to managed code
                    int[] quditValues = new int[quditValuesSize];
                    Marshal.Copy(prob.QuditValues, quditValues, 0, quditValues.Length);

                    probabilities[i] = new NativeBasisProbability
                    {
                        Probability = prob.Probability,
                        QuditValues = prob.QuditValues
                    };
                }

                // Convert to managed BasisProbability array
                result = Array.ConvertAll(probabilities, p => new BasisProbability(p, quditValuesSize));

                // Free the allocated memory
                for (int i = 0; i < numProbabilities; i++)
                {
                    IntPtr probPtr = IntPtr.Add(output, i * Marshal.SizeOf<NativeBasisProbability>());
                    NativeBasisProbability prob = Marshal.PtrToStructure<NativeBasisProbability>(probPtr);
                    Marshal.FreeHGlobal(prob.QuditValues);
                }

                Marshal.FreeHGlobal(output);
            }

            return result;
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeError qforge_reduced_density_matrix(IntPtr[] props, UIntPtr props_len, IntPtr real_output, IntPtr imag_ouput, int output_len);

        // Public method to expose the probabilities function
        public static Complex[,] ReducedDensityMatrix(params NativeQuantumProperty[] props)
        {
            IntPtr[] propHandles = Array.ConvertAll(props, p => p.Handle);
            int rowSize = 1;
            foreach (var prop in props)
            {
                rowSize *= prop.Dimension;
            }

            Complex[,] result = new Complex[rowSize, rowSize];

            var numMatrixEntries = rowSize * rowSize;

            // Allocate memory for the real and imaginary parts of the output
            IntPtr real_output = Marshal.AllocHGlobal(Marshal.SizeOf<float>() * numMatrixEntries);
            IntPtr imag_output = Marshal.AllocHGlobal(Marshal.SizeOf<float>() * numMatrixEntries);

            try
            {
                QForgeError error = qforge_reduced_density_matrix(propHandles, (UIntPtr)props.Length, real_output, imag_output, numMatrixEntries);

                if (error != QForgeError.QFORGE_ERR_NONE)
                {
                    throw new InvalidOperationException($"Error in ReducedDensityMatrix: {error}");
                }

            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                // Marshal the qudit_values array back to managed code
                float[] real = new float[numMatrixEntries];
                Marshal.Copy(real_output, real, 0, numMatrixEntries);

                float[] imag = new float[numMatrixEntries];
                Marshal.Copy(imag_output, imag, 0, numMatrixEntries);

                result = new Complex[rowSize,rowSize];
                for (int i = 0; i < rowSize; i++){
                
                    for(int j = 0; j < rowSize; ++j){
                        result[i, j] = new Complex(real[i * rowSize + j], imag[i * rowSize + j]);
                    }
                }

                Marshal.FreeHGlobal(real_output);
                Marshal.FreeHGlobal(imag_output);
            }

            return result;
        }

        internal static class LinearAlgebra
        {
            /// <summary>
            /// Gets the j-th column of the given matrix.
            /// </summary>
            /// <param name="A">The matrix.</param>
            /// <param name="j">The index of the column.</param>
            /// <returns>The column vector.</returns>
            public static Complex[] GetColumn(Complex[,] A, int j)
            {
                int n = A.GetLength(0);
                Complex[] column = new Complex[n];

                for (int i = 0; i < n; i++)
                {
                    column[i] = A[i, j];
                }

                return column;
            }

            /// <summary>
            /// Duplicates the given matrix.
            /// </summary>
            /// <param name="A">The matrix to duplicate.</param>
            /// <returns>The duplicated matrix.</returns>
            public static Complex[,] Duplicate(Complex[,] A)
            {
                int n = A.GetLength(0);
                Complex[,] B = new Complex[n, n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        B[i, j] = A[i, j];
                    }
                }

                return B;
            }

            /// <summary>
            /// Scales the given vector by a scalar.
            /// </summary>
            /// <param name="a">The vector to scale.</param>
            /// <param name="s">The scalar.</param>
            /// <returns></returns>
            public static Complex[] Scale(Complex[] a, float s)
            {
                int n = a.Length;
                Complex[] b = new Complex[n];

                for (int i = 0; i < n; i++)
                {
                    b[i] = s * a[i];
                }
                return b;
            }

            /// <summary>
            /// Calculates the inner product between two vectors.
            /// </summary>
            /// <param name="a">The first vector.</param>
            /// <param name="b">The second vector.</param>
            /// <returns>The inner product value.</returns>
            public static Complex InnerProduct(Complex[] a, Complex[] b)
            {
                Complex innerProduct = 0;

                for (int i = 0; i < a.Length; i++)
                {
                    innerProduct += a[i] * Complex.Conjugate(b[i]);
                }

                return innerProduct;
            }

            /// <summary>
            /// Calculates the outer product between two vectors.
            /// </summary>
            /// <param name="a">The first vector.</param>
            /// <param name="b">The second vector.</param>
            /// <returns>The outer product matrix.</returns>
            public static Complex[,] OuterProduct(Complex[] a, Complex[] b)
            {
                int n = a.Length;
                Complex[,] outerProduct = new Complex[n, n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        outerProduct[i, j] = Complex.Conjugate(a[i]) * b[j];
                    }
                }

                return outerProduct;
            }

            /// <summary>
            /// Projects the vector "a" orthogonally onto vector "u".
            /// </summary>
            /// <param name="a">The vector to project.</param>
            /// <param name="b">The vector on which will be projected.</param>
            /// <returns>The projection.</returns>
            public static Complex[] Project(Complex[] a, Complex[] b)
            {
                var ab = InnerProduct(a, b).Magnitude;
                if (ab == 0)
                {
                    return new Complex[a.Length];
                }
                return Scale(a, (float)(InnerProduct(a, b).Magnitude / InnerProduct(a, a).Magnitude));
            }

            /// <summary>
            /// Substracts vector "b" from vector "a".
            /// </summary>
            /// <param name="a">The vector to be subtracted.</param>
            /// <param name="b">The substracting vector.</param>
            /// <returns>The substracted vector.</returns>
            public static Complex[] Subtract(Complex[] a, Complex[] b)
            {
                int n = a.Length;
                Complex[] c = new Complex[n];

                for (int i = 0; i < n; i++)
                {
                    c[i] = a[i] - b[i];
                }

                return c;
            }

            /// <summary>
            /// Adds vector "b" from vector "a".
            /// </summary>
            /// <param name="a">The vector to be added.</param>
            /// <param name="b">The adding vector.</param>
            /// <returns>The added vector.</returns>
            public static Complex[] Add(Complex[] a, Complex[] b)
            {
                int n = a.Length;
                Complex[] c = new Complex[n];

                for (int i = 0; i < n; i++)
                {
                    c[i] = a[i] + b[i];
                }

                return c;
            }

            /// <summary>
            /// Adds matrix B to matrix A.
            /// </summary>
            /// <param name="a">The matrix to be added.</param>
            /// <param name="b">The adding matrix.</param>
            /// <returns>The added matrix.</returns>
            public static Complex[,] Add(Complex[,] A, Complex[,] B)
            {
                int n = A.GetLength(0);
                Complex[,] C = new Complex[n, n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        C[i, j] = A[i, j] + B[i, j];
                    }

                }

                return C;
            }

            /// <summary>
            /// Multiplies matrix A with matrix B.
            /// </summary>
            /// <param name="A">The first matrix.</param>
            /// <param name="B">The second matrix.</param>
            /// <returns>The resulting matrix.</returns>
            public static Complex[,] Product(Complex[,] A, Complex[,] B)
            {
                int n = A.GetLength(0);
                Complex[,] C = new Complex[n, n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        for (int k = 0; k < n; k++)
                        {
                            C[i, j] += A[i, k] * B[k, j];
                        }
                    }
                }

                return C;
            }

            public static Complex[] Product(Complex[,] A, Complex[] b)
            {
                int n = A.GetLength(0);
                Complex[] c = new Complex[n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        c[i] += A[i, j] * b[j];
                    }
                }

                return c;
            }

            /// <summary>
            /// Calculates the conjugate transpose of the given matrix.
            /// </summary>
            /// <param name="A">The matrix to transpose.</param>
            /// <returns>The transpose.</returns>
            public static Complex[,] ConjugateTranspose(Complex[,] A)
            {
                int n = A.GetLength(0);
                Complex[,] B = new Complex[n, n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        B[i, j] = Complex.Conjugate(A[j, i]);
                    }
                }

                return B;
            }

            /// <summary>
            /// Calculates the magnitude of the given vector.
            /// </summary>
            /// <param name="a">The vector to compute the magnitude of.</param>
            /// <returns>The magnitude.</returns>
            public static float Magnitude(Complex[] a)
            {
                double magnitude = 0.0f;

                for (int i = 0; i < a.Length; i++)
                {
                    magnitude += (a[i] * Complex.Conjugate(a[i])).Real;
                }

                return (float)System.Math.Sqrt(magnitude);
            }

            /// <summary>
            /// Constructs an n-by-n identity matrix.
            /// </summary>
            /// <param name="n">The size of the matrix.</param>
            /// <returns>The identity matrix.</returns>
            public static Complex[,] Identity(int n)
            {
                Complex[,] I = new Complex[n, n];
                for (int i = 0; i < n; i++)
                {
                    I[i, i] = 1;
                }
                return I;
            }

            /// <summary>
            /// Prints the given matrix.
            /// </summary>
            /// <param name="A">The matrix to print.</param>
            /// <returns>The string representation of the given matrix.</returns>
            public static string ToString(Complex[,] A)
            {
                int rowCount = A.GetLength(0);
                int columnCount = A.GetLength(1);

                string text = "";

                for (int i = 0; i < rowCount; i++)
                {
                    if (i > 0) text += ',';

                    text += '{';
                    for (int j = 0; j < columnCount; j++)
                    {
                        if (j > 0) text += ',';
                        text += A[i, j];
                    }
                    text += '}';
                }

                return text;
            }

            /// <summary>
            /// Prints the given vector.
            /// </summary>
            /// <param name="a">The vector to print.</param>
            /// <returns>The string representation of the given vector.</returns>
            public static string ToString(Complex[] a)
            {
                string text = "{";
                for (int i = 0; i < a.Length; i++)
                {
                    if (i > 0) text += ',';
                    text += a[i];
                }
                text += '}';

                return text;
            }
        }

        internal static class QRAlgorithm
        {
            /// <summary>
            /// Runs the QR algorithm to find the eigenvalues and eigenvectors of the given matrix (see https://en.wikipedia.org/wiki/QR_algorithm).
            /// </summary>
            /// <param name="A">The matrix for which eigenvalues and eigenvectors should be found.</param>
            /// <param name="iterations">The number of iterations.</param>
            /// <param name="eigenvalues">The eigenvalues stored as diagonal entries in a matrix.</param>
            /// <param name="eigenvectors">The eigenvectors stored as columns in a matrix.</param>
            public static void Diagonalize(Complex[,] A, int iterations, out Complex[] eigenvalues, out Complex[,] eigenvectors)
            {
                int n = A.GetLength(0);

                // Duplicate the original matrix A so it stays intact.
                Complex[,] B = LinearAlgebra.Duplicate(A);

                // Initialize the eigenvector matrix C.
                Complex[,] U = LinearAlgebra.Identity(n);

                // Perform the QR decomposition and update the B and C matrixes each iteration.
                for (int i = 0; i < iterations; i++)
                {
                    QRDecomposition(B, out Complex[,] Q, out Complex[,] R);
                    B = LinearAlgebra.Product(R, Q);
                    U = LinearAlgebra.Product(U, Q);
                }
                // The eigenvalues are on the diagonal of the B matrix.
                eigenvalues = new Complex[n];
                for (int i = 0; i < n; i++)
                {
                    eigenvalues[i] = B[i, i];
                }

                // The eigenvectors are the columns of the C matrix.
                eigenvectors = U;
            }

            /// <summary>
            /// Calculates the QR decomposition of the given matrix A (see https://en.wikipedia.org/wiki/QR_decomposition). 
            /// </summary>
            /// <param name="A">The matrix to decompose.</param>
            /// <param name="Q">The Q part of the decomposition.</param>
            /// <param name="R">The R part of the decomposition.</param>
            private static void QRDecomposition(Complex[,] A, out Complex[,] Q, out Complex[,] R)
            {
                int n = A.GetLength(0);

                // Duplicate the original matrix A so it stays intact.
                Complex[,] U = LinearAlgebra.Duplicate(A);

                // Calculate the U matrix using the Gram�Schmidt process (see https://en.wikipedia.org/wiki/Gram%E2%80%93Schmidt_process).
                for (int j = 1; j < n; j++)
                {
                    Complex[] u = LinearAlgebra.GetColumn(U, j);
                    Complex[] v = LinearAlgebra.GetColumn(U, j);

                    for (int k = j - 1; k >= 0; k--)
                    {
                        Complex[] uk = LinearAlgebra.GetColumn(U, k);
                        u = LinearAlgebra.Subtract(u, LinearAlgebra.Project(uk, v));
                    }

                    // Update the column entries in U.
                    for (int i = 0; i < n; i++)
                    {
                        U[i, j] = u[i];
                    }
                }

                // Normalize the column vectors of U.
                for (int j = 0; j < n; j++)
                {
                    Complex[] u = LinearAlgebra.GetColumn(U, j);
                    float magnitude = LinearAlgebra.Magnitude(u);

                    if (magnitude > float.Epsilon)
                    {
                        // Update the column entries in U.
                        for (int i = 0; i < n; i++)
                        {
                            U[i, j] = u[i] / magnitude;
                        }
                    }
                }

                // The U matrix is now the Q part of the decomposition.
                Q = U;

                // Calculate the R part of the decomposition.
                R = LinearAlgebra.Product(LinearAlgebra.ConjugateTranspose(Q), A);
            }
        }

        private static float VonNeumannEntropy(Complex[,] matrix)
        {
            float entropy = 0.0f;
            QRAlgorithm.Diagonalize(matrix, 100, out var eigenvalues, out _);
            foreach (var ev in eigenvalues)
            {
                if (ev.Magnitude < float.Epsilon) continue;
                entropy -= (float)(ev.Magnitude * Math.Log(ev.Magnitude));
            }
            return entropy;
        }

        public static float[] MutualInformation(params NativeQuantumProperty[] props)
        {
            float[] result = new float[props.Length];
            var r = ReducedDensityMatrix(props);
            var s = VonNeumannEntropy(r);

            for (int i = 0; i < props.Length; ++i)
            {
                var ra = ReducedDensityMatrix(new NativeQuantumProperty[] { props[i] });
                var sa = VonNeumannEntropy(ra);
                var propsB = props.Except(new NativeQuantumProperty[] { props[i] }).ToArray();
                var rb = ReducedDensityMatrix(propsB);
                var sb = VonNeumannEntropy(rb);
                result[i] = sa + sb - s;
            }
            return result;
        }

        public static float[,] CorrelationMatrix(params NativeQuantumProperty[] props)
        {
            if (props.Length != 2)
            {
                Debug.LogError("CorrelationMatrix is only defined for two properties.");
                return new float[0, 0];
            }

            var d0 = props[0].Dimension;
            var d1 = props[1].Dimension;
            var result = new float[d0, d1];
            var joint_probs = Probabilities(props);

            for (int i = 0; i < d0; ++i)
            {
                for (int j = 0; j < d1; ++j)
                {
                    var pi = joint_probs.Where(p => p.QuditValues[0] == i).Sum(p => p.Probability);
                    var pj = joint_probs.Where(p => p.QuditValues[1] == j).Sum(p => p.Probability);
                    var pij = joint_probs.Where(p => p.QuditValues[0] == i && p.QuditValues[1] == j).Sum(p => p.Probability);
                    var n = Mathf.Sqrt(pi * (1 - pi) * pj * (1 - pj));
                    result[i, j] = (pij - pi * pj);
                    if (n != 0.0f)
                    {
                        result[i, j] /= n;
                    }
                }
            }

            return result;
        }
    }
}
