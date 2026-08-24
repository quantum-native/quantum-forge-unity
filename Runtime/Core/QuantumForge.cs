// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.
//
// ---------------------------------------------------------------------------
// SOURCE OF TRUTH for the Unity C# binding.
//
// This file is the ONLY place this binding is maintained. The shipped UPM
// package copy at unity-package/Runtime/Core/QuantumForge.cs is GENERATED from
// this file, byte for byte, by:
//
//     scripts/sync-unity-package-bindings.sh
//
// Sync direction is one-way:  wrappers/unity/Core/  ->  unity-package/Runtime/Core/
// Never hand-edit the package copy, and never copy changes back the other way.
//
// After changing this file, re-run the sync script. CI enforces both halves:
//   * scripts/check-unity-package-synced.sh      -- the copies are identical
//   * scripts/check-unity-dllimport-symbols.sh   -- every DllImport entry point
//                                                   below actually exists in the
//                                                   C API / built plugin
// (both wired into .github/workflows/unity-package-ci.yml)
//
// These guards exist because the two copies silently diverged for an entire
// release: the shipped one P/Invoked 14 renamed symbols, so every quantum call
// threw EntryPointNotFoundException. See unity-package/CHANGELOG.md.
// ---------------------------------------------------------------------------

using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace QRG.QuantumForge.Core
{
#if !UNITY_5_3_OR_NEWER
    // Simple Debug wrapper for non-Unity environments
    public static class Debug
    {
        public static void Log(string message)
        {
            Console.WriteLine($"[LOG] {message}");
        }
        
        public static void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }
        
        public static void LogWarning(string message)
        {
            Console.WriteLine($"[WARNING] {message}");
        }
    }
#endif

    public static class QuantumForge
    {
        // Define the qforge_result enum to match new API
        public enum QForgeResult
        {
            QFORGE_SUCCESS = 0,
            
            // Parameter errors (1-99)
            QFORGE_ERROR_NULL_POINTER = 1,
            QFORGE_ERROR_INVALID_ARGUMENT = 2,
            QFORGE_ERROR_BUFFER_TOO_SMALL = 3,
            QFORGE_ERROR_INVALID_DIMENSION = 4,
            QFORGE_ERROR_INVALID_QUDIT_NUMBER = 5,
            
            // Operation errors (100-199)
            QFORGE_ERROR_TARGET_CONTROL_OVERLAP = 100,
            QFORGE_ERROR_INCOMPATIBLE_DIMENSIONS = 101,
            QFORGE_ERROR_STATE_SIZE_EXCEEDED = 102,
            
            // Memory errors (200-299)
            QFORGE_ERROR_OUT_OF_MEMORY = 200,
            
            // Internal errors (900-999)
            QFORGE_ERROR_INTERNAL = 900
        }

        // Error context structure matching the new API
        [StructLayout(LayoutKind.Sequential)]
        public struct QForgeErrorInfo
        {
            public QForgeResult code;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string message;
            public IntPtr function; // const char* function
            public int line;
        }

        // Probability result structure matching new API
        [StructLayout(LayoutKind.Sequential)]
        public struct QForgeProbabilityResult
        {
            public float probability;
            public IntPtr qudit_values; // const int* qudit_values
            public UIntPtr num_qudits;  // size_t num_qudits
        }

        // Complex number structure matching new API
        [StructLayout(LayoutKind.Sequential)]
        public struct QForgeComplex
        {
            public float real;
            public float imag;
        }

        // Define the NativeBasisProbability struct (legacy compatibility)
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

        // Predicate now uses opaque handles in the new API
        public class Predicate : IDisposable
        {
            private IntPtr handle;
            internal IntPtr Handle => handle;
            
            internal Predicate(IntPtr handle)
            {
                this.handle = handle;
            }
            
            public void Dispose()
            {
                if (handle != IntPtr.Zero)
                {
                    qforge_predicate_destroy(ref handle);
                    handle = IntPtr.Zero;
                }
            }
        }

        // Unity resolves a native plugin by the name the DllImport asks for, so every
        // name below must be the actual filename of a plugin the package ships. They
        // are kept in lockstep with OUTPUT_NAME in wrappers/unity/CMakeLists.txt and
        // with the files under unity-package/Runtime/Plugins/:
        //     quantum-forge-Windows  -> x86-64/quantum-forge-Windows.dll
        //     quantum-forge-macOS    -> MacOS/quantum-forge-macOS.bundle
        //     quantum-forge-Linux    -> Linux/libquantum-forge-Linux.so
        // Rename one without the other and the package throws DllNotFoundException on
        // the first quantum call, which is exactly how 1.4.0 was first cut.
#if UNITY_EDITOR || UNITY_STANDALONE
    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        const string QUANTUM_FORGE_LIB = "quantum-forge-Windows";
    #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        const string QUANTUM_FORGE_LIB = "quantum-forge-macOS";
    #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        const string QUANTUM_FORGE_LIB = "quantum-forge-Linux";
    #else
        // Unreachable: UNITY_STANDALONE always implies one of WIN/OSX/LINUX. The
        // package ships no plugin under this name; it exists only so the #if chain
        // is total.
        const string QUANTUM_FORGE_LIB = "quantum-forge";
    #endif
#elif UNITY_IOS
    const string QUANTUM_FORGE_LIB = "quantum-forge-iOS";
#elif UNITY_ANDROID
    const string QUANTUM_FORGE_LIB = "quantum-forge-Android";
#elif UNITY_WEBGL && !UNITY_EDITOR
    const string QUANTUM_FORGE_LIB = "__Internal";
#else
    // For non-Unity environments (like tests), detect platform at compile time
    // Use .NET preprocessor symbols for robust platform detection in CI
    #if NETCOREAPP || NET5_0_OR_GREATER
        #if WINDOWS
            const string QUANTUM_FORGE_LIB = "quantum-forge-Windows";
        #elif LINUX
            const string QUANTUM_FORGE_LIB = "libquantum-forge-Linux";
        #elif OSX
            const string QUANTUM_FORGE_LIB = "libquantum-forge-macOS";
        #else
            // Unknown host with no platform symbol defined. This shouldn't happen with
            // proper build configuration, and no shipped plugin carries this name.
            const string QUANTUM_FORGE_LIB = "libquantum-forge";
        #endif
    #else
        // .NET Framework - use Windows by default
        const string QUANTUM_FORGE_LIB = "quantum-forge-Windows";
    #endif
#endif

        // Updated DllImport declarations for new API
        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeResult qforge_quantum_property_create(
            int dimension,
            out IntPtr out_property,
            ref QForgeErrorInfo error_info);

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeResult qforge_quantum_property_destroy(ref IntPtr property);

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeResult qforge_quantum_property_get_dimension(
            IntPtr property,
            out int out_dimension);

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeResult qforge_predicate_create(
            IntPtr property,
            int value,
            bool is_equal,
            out IntPtr out_predicate);

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeResult qforge_predicate_destroy(ref IntPtr predicate);

        // Define the NativeQuantumProperty class as a wrapper
        public class NativeQuantumProperty : IDisposable
        {
            private IntPtr handle;
            internal IntPtr Handle => handle;
            public readonly int Dimension;

            public NativeQuantumProperty(int dimension)
            {
                var errorInfo = new QForgeErrorInfo();
                var result = qforge_quantum_property_create(dimension, out handle, ref errorInfo);
                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Failed to create quantum property: {result} - {errorInfo.message}");
                }
                Dimension = dimension;
            }

            public NativeQuantumProperty(int dimension, int initial)
            {
                if (initial >= dimension || initial < 0)
                {
                    throw new InvalidOperationException(
                        $"Error creating quantum property: Make sure Initial value {initial} is between 0 and {dimension}");
                }

                var errorInfo = new QForgeErrorInfo();
                var result = qforge_quantum_property_create(dimension, out handle, ref errorInfo);
                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error creating quantum property: {result} - {errorInfo.message}");
                }

                Dimension = dimension;

                var m = Measure(this);
                while (m[0] != initial)
                {
                    Cycle(this, 1.0);
                    m = Measure(this);
                }

                Debug.Log(
                    $"QuantumForge: Created NativeQuantumProperty of dimension {dimension}, with initial value {initial}. Handle: {handle}");
            }

            public void Dispose()
            {
                if (handle != IntPtr.Zero)
                {
                    var result = qforge_quantum_property_destroy(ref handle);
                    if (result != QForgeResult.QFORGE_SUCCESS)
                    {
                        Debug.LogError($"Failed to destroy quantum property: {result}");
                    }
                    handle = IntPtr.Zero;
                }
            }

            public Predicate is_value(int value)
            {
                var result = qforge_predicate_create(handle, value, true, out IntPtr predicateHandle);
                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Failed to create predicate: {result}");
                }
                return new Predicate(predicateHandle);
            }

            public Predicate is_not_value(int value)
            {
                var result = qforge_predicate_create(handle, value, false, out IntPtr predicateHandle);
                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Failed to create predicate: {result}");
                }
                return new Predicate(predicateHandle);
            }
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_cycle_operation(
            IntPtr property,
            double fraction,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void Cycle(NativeQuantumProperty prop, double fraction, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_cycle_operation(
                    prop.Handle, 
                    fraction, 
                    predicateHandles, 
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Cycle: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        public static void Cycle(NativeQuantumProperty prop, params Predicate[] preds)
        {
            Cycle(prop, 1.0, preds);
        }

        public static void NCycle(NativeQuantumProperty prop1, NativeQuantumProperty prop2, double fraction = 1.0)
        {
            for (int i = 0; i < prop1.Dimension; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    using (var predicate = prop1.is_value(i))
                    {
                        Cycle(prop2, fraction, predicate);
                    }
                }
            }
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_shift_operation(
            IntPtr property,
            double fraction,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void Shift(NativeQuantumProperty prop, double fraction, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_shift_operation(
                    prop.Handle, 
                    fraction, 
                    predicateHandles, 
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Shift: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        public static void Shift(NativeQuantumProperty prop, params Predicate[] preds)
        {
            Shift(prop, 1.0, preds);
        }

        public static void NShift(NativeQuantumProperty prop1, NativeQuantumProperty prop2, double fraction = 1.0)
        {
            for (int i = 0; i < prop1.Dimension; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    using (var predicate = prop1.is_value(i))
                    {
                        Shift(prop2, fraction, predicate);
                    }
                }
            }
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_clock_operation(
            IntPtr property,
            double fraction,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void Clock(NativeQuantumProperty prop, double fraction, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_clock_operation(
                    prop.Handle, 
                    fraction, 
                    predicateHandles, 
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Clock: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        public static void Clock(NativeQuantumProperty prop, params Predicate[] preds)
        {
            Clock(prop, 1.0, preds);
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_x_operation(
            IntPtr property,
            double fraction,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void X(NativeQuantumProperty prop, double fraction, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_x_operation(
                    prop.Handle,
                    fraction,
                    predicateHandles,
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in X: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        public static void X(NativeQuantumProperty prop, params Predicate[] preds)
        {
            X(prop, 1.0, preds);
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_z_operation(
            IntPtr property,
            double fraction,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void Z(NativeQuantumProperty prop, double fraction, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_z_operation(
                    prop.Handle,
                    fraction,
                    predicateHandles,
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Z: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        public static void Z(NativeQuantumProperty prop, params Predicate[] preds)
        {
            Z(prop, 1.0, preds);
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_y_operation(
            IntPtr property,
            double fraction,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void Y(NativeQuantumProperty prop, double fraction, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_y_operation(
                    prop.Handle,
                    fraction,
                    predicateHandles,
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Y: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        public static void Y(NativeQuantumProperty prop, params Predicate[] preds)
        {
            Y(prop, 1.0, preds);
        }

        /// <summary>
        /// Returns a property KNOWN to be in <paramref name="currentValue"/> back to basis
        /// value 0, by applying the remaining cycles of its dimension.
        /// </summary>
        /// <remarks>
        /// Pure C# convenience -- there is no qforge_reset in the C API, so this composes
        /// Cycle. It is only correct when the property is genuinely in a definite basis
        /// state <paramref name="currentValue"/> (e.g. immediately after measuring it);
        /// applied to a superposition it permutes the state rather than resetting it.
        ///
        /// This method previously existed ONLY in the shipped package copy of this
        /// binding and was lost from view when the two copies diverged. It lives here
        /// now because this file is the single source of truth.
        /// </remarks>
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
        private static extern QForgeResult qforge_hadamard_operation(
            IntPtr property,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void Hadamard(NativeQuantumProperty prop, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_hadamard_operation(
                    prop.Handle, 
                    predicateHandles, 
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Hadamard: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_fractional_hadamard_operation(
            IntPtr property,
            double fraction,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        /// <summary>
        /// Fractional Hadamard (H^t). fraction 1.0 is the full Hadamard, 0.0 is identity.
        /// </summary>
        /// <remarks>
        /// The C API has declared qforge_fractional_hadamard_operation since the batch-ops
        /// release, but this binding never bound it, so the package's own
        /// QuantumProperty.Hadamard(prop, fraction, ...) had no overload to call and the
        /// package failed to compile. See unity-package/CHANGELOG.md 1.4.0.
        /// </remarks>
        public static void Hadamard(NativeQuantumProperty prop, double fraction, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_fractional_hadamard_operation(
                    prop.Handle,
                    fraction,
                    predicateHandles,
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Hadamard: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_inverse_hadamard_operation(
            IntPtr property,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void InverseHadamard(NativeQuantumProperty prop, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_inverse_hadamard_operation(
                    prop.Handle, 
                    predicateHandles, 
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in InverseHadamard: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_phase_rotate_operation(
            IntPtr[] predicates,
            UIntPtr predicate_count,
            double angle,
            ref QForgeErrorInfo error_info);

        public static void PhaseRotate(double angle, params Predicate[] preds)
        {
            if (preds == null || preds.Length == 0)
            {
                throw new ArgumentException("Predicates array cannot be null or empty for PhaseRotate.");
            }

            try
            {
                IntPtr[] predicateHandles = Array.ConvertAll(preds, p => p.Handle);

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_phase_rotate_operation(
                    predicateHandles, 
                    (UIntPtr)predicateHandles.Length, 
                    angle,
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in PhaseRotate: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_swap_operation(
            IntPtr property1,
            IntPtr property2,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void Swap(NativeQuantumProperty p1, NativeQuantumProperty p2, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_swap_operation(
                    p1.Handle, 
                    p2.Handle, 
                    predicateHandles, 
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Swap: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_i_swap_operation(
            IntPtr property1,
            IntPtr property2,
            double fraction,
            IntPtr[] predicates,
            UIntPtr predicate_count,
            ref QForgeErrorInfo error_info);

        public static void ISwap(NativeQuantumProperty p1, NativeQuantumProperty p2, double fraction, params Predicate[] preds)
        {
            try
            {
                IntPtr[] predicateHandles = null;
                if (preds != null)
                {
                    predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                }

                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_i_swap_operation(
                    p1.Handle, 
                    p2.Handle, 
                    fraction, 
                    predicateHandles, 
                    (UIntPtr)(predicateHandles?.Length ?? 0),
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in ISwap: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        public static void ISwap(NativeQuantumProperty p1, NativeQuantumProperty p2, params Predicate[] preds)
        {
            ISwap(p1, p2, 1.0, preds);
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_measure_properties(
            IntPtr[] properties,
            UIntPtr property_count,
            int[] output_buffer,
            UIntPtr buffer_size,
            ref QForgeErrorInfo error_info);

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_forced_measure_properties(
            IntPtr[] properties,
            UIntPtr property_count,
            int[] forced_values,
            UIntPtr forced_value_count,
            int[] output_buffer,
            UIntPtr buffer_size,
            ref QForgeErrorInfo error_info);

        // Public method to expose the measure function
        public static int[] Measure(params NativeQuantumProperty[] props)
        {
            IntPtr[] propHandles = Array.ConvertAll(props, p => p.Handle);
            int[] output = new int[props.Length];
            try
            {
                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_measure_properties(
                    propHandles, 
                    (UIntPtr)props.Length, 
                    output, 
                    (UIntPtr)output.Length,
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Measure: {result} - {errorInfo.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }

            return output;
        }

        public static int[] ForcedMeasure(int[] forcedValues, params NativeQuantumProperty[] props)
        {
            if (props == null || props.Length == 0)
            {
                throw new ArgumentException("At least one property is required for forced measurement.", nameof(props));
            }

            if (forcedValues == null)
            {
                throw new ArgumentNullException(nameof(forcedValues));
            }

            if (forcedValues.Length != props.Length)
            {
                throw new ArgumentException("forcedValues must have the same length as props.", nameof(forcedValues));
            }

            IntPtr[] propHandles = Array.ConvertAll(props, p => p.Handle);
            int[] output = new int[props.Length];

            try
            {
                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_forced_measure_properties(
                    propHandles,
                    (UIntPtr)props.Length,
                    forcedValues,
                    (UIntPtr)forcedValues.Length,
                    output,
                    (UIntPtr)output.Length,
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in ForcedMeasure: {result} - {errorInfo.message}");
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
        private static extern QForgeResult qforge_stochastic_projection_modern(
            IntPtr[] predicates,
            UIntPtr predicate_count,
            out int result,
            ref QForgeErrorInfo error_info);

        // Public method to expose the predicated measure function
        public static int Measure(Predicate[] preds)
        {
            if (preds == null || preds.Length == 0)
            {
                throw new ArgumentException("Predicates array cannot be null or empty for predicated Measure.");
            }

            try
            {
                IntPtr[] predicateHandles = Array.ConvertAll(preds, p => p.Handle);
                
                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_stochastic_projection_modern(
                    predicateHandles, 
                    (UIntPtr)predicateHandles.Length, 
                    out int measureResult,
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in predicated Measure: {result} - {errorInfo.message}");
                }

                return measureResult;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
        }

        // NOTE: output_buffer is declared as a raw IntPtr, NOT as a
        // QForgeProbabilityResult[]. A managed array parameter is marshalled
        // [In]-only by default: the interop layer is free to hand the callee a
        // *copy* of the array (and does so for arrays of user-defined structs),
        // so everything the native side writes into it -- here, the
        // `probability` field of every entry -- is silently discarded on the way
        // back. Probabilities() therefore always saw 0.0 for every basis state.
        // Passing unmanaged memory we own removes the marshaller from the
        // write-back path entirely, which behaves identically under CoreCLR,
        // Mono and IL2CPP.
        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_calculate_probabilities(
            IntPtr[] properties,
            UIntPtr property_count,
            IntPtr output_buffer, // qforge_probability_result_t*
            UIntPtr buffer_size,
            out UIntPtr actual_count,
            ref QForgeErrorInfo error_info);

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_calculate_combinations_count(
            IntPtr[] properties,
            UIntPtr property_count,
            out UIntPtr required_size);

        // Public method to expose the probabilities function
        public static BasisProbability[] Probabilities(params NativeQuantumProperty[] props)
        {
            if (props == null || props.Length == 0)
            {
                Debug.LogWarning("No properties provided to calculate probabilities");
                return new BasisProbability[0];
            }

            IntPtr[] propHandles = Array.ConvertAll(props, p => p.Handle);
            
            // First, get the required buffer size
            var result = qforge_calculate_combinations_count(propHandles, (UIntPtr)props.Length, out UIntPtr requiredSize);
            if (result != QForgeResult.QFORGE_SUCCESS)
            {
                throw new InvalidOperationException($"Error getting combinations count: {result}");
            }

            int combinationCount = (int)requiredSize;
            int quditsPerCombination = props.Length;
            int resultStride = Marshal.SizeOf(typeof(QForgeProbabilityResult));

            // Both buffers live in unmanaged memory so that the native writes
            // land in storage the marshaller never copies (see the DllImport
            // comment above).
            //
            // qforge_calculate_probabilities also does NOT allocate the
            // qudit_values arrays -- it memcpy's into CALLER-OWNED storage and
            // silently skips any entry whose qudit_values is NULL (see
            // wrappers/c-api/src/quantum_forge.cpp). Allocate one contiguous int
            // block and point each result at its own slice.
            IntPtr outputBuffer = Marshal.AllocHGlobal(combinationCount * resultStride);
            IntPtr quditValuesBlock = Marshal.AllocHGlobal(
                combinationCount * quditsPerCombination * sizeof(int));

            try
            {
                for (int i = 0; i < combinationCount; i++)
                {
                    var entry = new QForgeProbabilityResult
                    {
                        probability = 0.0f,
                        qudit_values = IntPtr.Add(quditValuesBlock, i * quditsPerCombination * sizeof(int)),
                        num_qudits = (UIntPtr)quditsPerCombination
                    };
                    Marshal.StructureToPtr(entry, IntPtr.Add(outputBuffer, i * resultStride), false);
                }

                var errorInfo = new QForgeErrorInfo();
                result = qforge_calculate_probabilities(
                    propHandles,
                    (UIntPtr)props.Length,
                    outputBuffer,
                    requiredSize,
                    out UIntPtr actualCount,
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in Probabilities: {result} - {errorInfo.message}");
                }

                // Convert to managed BasisProbability array
                var managedResults = new BasisProbability[(int)actualCount];
                for (int i = 0; i < (int)actualCount; i++)
                {
                    var prob = (QForgeProbabilityResult)Marshal.PtrToStructure(
                        IntPtr.Add(outputBuffer, i * resultStride), typeof(QForgeProbabilityResult));
                    managedResults[i] = new BasisProbability(
                        new NativeBasisProbability
                        {
                            Probability = prob.probability,
                            QuditValues = prob.qudit_values
                        },
                        (int)prob.num_qudits);
                }

                return managedResults;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                // BasisProbability copies the values into a managed int[], so the
                // native blocks are dead by the time we get here.
                Marshal.FreeHGlobal(quditValuesBlock);
                Marshal.FreeHGlobal(outputBuffer);
            }
        }

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        // output_buffer is a raw IntPtr for the same reason as
        // qforge_calculate_probabilities above: a managed struct array is
        // marshalled [In]-only, so the native writes never make it back and the
        // density matrix came out all zeros.
        private static extern QForgeResult qforge_calculate_reduced_density_matrix(
            IntPtr[] properties,
            UIntPtr property_count,
            IntPtr output_buffer, // qforge_complex_t*
            UIntPtr buffer_size,
            out UIntPtr matrix_size,
            ref QForgeErrorInfo error_info);

        [DllImport(QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_calculate_matrix_size(
            IntPtr[] properties,
            UIntPtr property_count,
            out UIntPtr matrix_size,
            out UIntPtr total_elements);

        // Public method to expose the reduced density matrix function
        public static Complex[,] ReducedDensityMatrix(params NativeQuantumProperty[] props)
        {
            IntPtr[] propHandles = Array.ConvertAll(props, p => p.Handle);
            
            // First, get the required matrix size
            var result = qforge_calculate_matrix_size(propHandles, (UIntPtr)props.Length, out UIntPtr matrixSize, out UIntPtr totalElements);
            if (result != QForgeResult.QFORGE_SUCCESS)
            {
                throw new InvalidOperationException($"Error getting matrix size: {result}");
            }

            int rowSize = (int)matrixSize;
            int numMatrixEntries = (int)totalElements;

            // Allocate buffer for complex numbers in unmanaged memory so the
            // native writes are visible to us (see the DllImport comment above).
            int complexStride = Marshal.SizeOf(typeof(QForgeComplex));
            IntPtr outputBuffer = Marshal.AllocHGlobal(numMatrixEntries * complexStride);

            try
            {
                var errorInfo = new QForgeErrorInfo();
                result = qforge_calculate_reduced_density_matrix(
                    propHandles,
                    (UIntPtr)props.Length,
                    outputBuffer,
                    totalElements,
                    out UIntPtr actualMatrixSize,
                    ref errorInfo);

                if (result != QForgeResult.QFORGE_SUCCESS)
                {
                    throw new InvalidOperationException($"Error in ReducedDensityMatrix: {result} - {errorInfo.message}");
                }

                // Convert to managed Complex matrix
                Complex[,] complexMatrix = new Complex[rowSize, rowSize];
                for (int i = 0; i < rowSize; i++)
                {
                    for (int j = 0; j < rowSize; j++)
                    {
                        var complexValue = (QForgeComplex)Marshal.PtrToStructure(
                            IntPtr.Add(outputBuffer, (i * rowSize + j) * complexStride),
                            typeof(QForgeComplex));
                        complexMatrix[i, j] = new Complex(complexValue.real, complexValue.imag);
                    }
                }

                return complexMatrix;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(outputBuffer);
            }
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
                    var n = (float)Math.Sqrt(pi * (1 - pi) * pj * (1 - pj));
                    result[i, j] = (pij - pi * pj);
                    if (n != 0.0f)
                    {
                        result[i, j] /= n;
                    }
                }
            }

            return result;
        }

        // Library management and utility functions
        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern IntPtr qforge_get_version();

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeResult qforge_get_version_info(out int major, out int minor, out int patch);

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeResult qforge_initialize();

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern QForgeResult qforge_cleanup();

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern int qforge_get_max_dimension();

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern int qforge_get_max_qudits();

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern bool qforge_is_valid_dimension(int dimension);

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern IntPtr qforge_get_error_string(QForgeResult result);

        // ── Batch operations ─────────────────────────────────────────────────

        /// <summary>Operation codes for batch execution.</summary>
        public enum QForgeOpCode
        {
            Cycle = 0,
            Shift,
            Clock,
            X,
            Z,
            Y,
            Hadamard,
            InverseHadamard,
            Swap,
            ISwap,
            PhaseRotate
        }

        /// <summary>
        /// A single gate operation descriptor for batch execution.
        /// Set fraction to double.NaN for non-fractional (discrete permutation) variant.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct QForgeBatchOp
        {
            public int op;              // QForgeOpCode as int (C enum)
            public IntPtr target;       // qforge_quantum_property_t*
            public IntPtr target2;      // qforge_quantum_property_t* (swap/i_swap)
            public double fraction;     // NaN = non-fractional
            public double angle;        // phase_rotate only
            public IntPtr predicates;   // qforge_predicate_t** array
            public UIntPtr predicateCount;
        }

        /// <summary>Result of a batch execution.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct QForgeBatchResult
        {
            public UIntPtr opsExecuted;
            public QForgeResult errorCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string errorMessage;
        }

        [DllImport(dllName: QUANTUM_FORGE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern QForgeResult qforge_execute_batch(
            [In] QForgeBatchOp[] ops,
            UIntPtr opCount,
            ref QForgeBatchResult result,
            ref QForgeErrorInfo errorInfo);

        /// <summary>
        /// Execute a sequence of gate operations in a single native call.
        /// Operations execute sequentially. On the first error, execution stops.
        /// </summary>
        /// <returns>Batch result with ops_executed count and error info.</returns>
        public static QForgeBatchResult ExecuteBatch(params (QForgeOpCode op, NativeQuantumProperty target, NativeQuantumProperty target2, double fraction, double angle, Predicate[] predicates)[] ops)
        {
            var nativeOps = new QForgeBatchOp[ops.Length];
            // We need to pin predicate handle arrays so GC doesn't move them
            var predicateArrays = new IntPtr[ops.Length][];
            var pinnedArrays = new GCHandle[ops.Length];

            try
            {
                for (int i = 0; i < ops.Length; i++)
                {
                    nativeOps[i].op = (int)ops[i].op;
                    nativeOps[i].target = ops[i].target?.Handle ?? IntPtr.Zero;
                    nativeOps[i].target2 = ops[i].target2?.Handle ?? IntPtr.Zero;
                    nativeOps[i].fraction = ops[i].fraction;
                    nativeOps[i].angle = ops[i].angle;

                    if (ops[i].predicates != null && ops[i].predicates.Length > 0)
                    {
                        predicateArrays[i] = Array.ConvertAll(ops[i].predicates, p => p.Handle);
                        pinnedArrays[i] = GCHandle.Alloc(predicateArrays[i], GCHandleType.Pinned);
                        nativeOps[i].predicates = pinnedArrays[i].AddrOfPinnedObject();
                        nativeOps[i].predicateCount = (UIntPtr)predicateArrays[i].Length;
                    }
                    else
                    {
                        nativeOps[i].predicates = IntPtr.Zero;
                        nativeOps[i].predicateCount = UIntPtr.Zero;
                    }
                }

                var batchResult = new QForgeBatchResult();
                var errorInfo = new QForgeErrorInfo();
                QForgeResult result = qforge_execute_batch(
                    nativeOps, (UIntPtr)ops.Length, ref batchResult, ref errorInfo);

                return batchResult;
            }
            finally
            {
                for (int i = 0; i < ops.Length; i++)
                {
                    if (pinnedArrays[i].IsAllocated)
                        pinnedArrays[i].Free();
                }
            }
        }

        /// <summary>
        /// Simplified batch execution for operations without predicates.
        /// </summary>
        public static QForgeBatchResult ExecuteBatch(params (QForgeOpCode op, NativeQuantumProperty target)[] ops)
        {
            var fullOps = Array.ConvertAll(ops, o =>
                (o.op, o.target, (NativeQuantumProperty)null, double.NaN, 0.0, (Predicate[])null));
            return ExecuteBatch(fullOps);
        }

        // Error callback support
        public delegate void ErrorCallback(QForgeErrorInfo errorInfo, IntPtr userData);

        [DllImport(QUANTUM_FORGE_LIB)]
        private static extern void qforge_set_error_callback(ErrorCallback callback, IntPtr userData);

        public static void SetErrorCallback(ErrorCallback callback, IntPtr userData)
        {
            qforge_set_error_callback(callback, userData);
        }

        public static void SetErrorCallback(ErrorCallback callback)
        {
            qforge_set_error_callback(callback, IntPtr.Zero);
        }

        // Public utility methods
        public static string GetVersion()
        {
            IntPtr versionPtr = qforge_get_version();
            return Marshal.PtrToStringAnsi(versionPtr);
        }

        public static (int major, int minor, int patch) GetVersionInfo()
        {
            var result = qforge_get_version_info(out int major, out int minor, out int patch);
            if (result != QForgeResult.QFORGE_SUCCESS)
            {
                throw new InvalidOperationException($"Failed to get version info: {result}");
            }
            return (major, minor, patch);
        }

        public static void Initialize()
        {
            var result = qforge_initialize();
            if (result != QForgeResult.QFORGE_SUCCESS)
            {
                throw new InvalidOperationException($"Failed to initialize QuantumForge: {result}");
            }
        }

        public static void Cleanup()
        {
            var result = qforge_cleanup();
            if (result != QForgeResult.QFORGE_SUCCESS)
            {
                Debug.LogError($"Failed to cleanup QuantumForge: {result}");
            }
        }

        public static int GetMaxDimension()
        {
            return qforge_get_max_dimension();
        }

        public static int GetMaxQudits()
        {
            return qforge_get_max_qudits();
        }

        public static bool IsValidDimension(int dimension)
        {
            return qforge_is_valid_dimension(dimension);
        }

        public static string GetErrorString(QForgeResult result)
        {
            IntPtr errorPtr = qforge_get_error_string(result);
            return Marshal.PtrToStringAnsi(errorPtr);
        }
    }
}
