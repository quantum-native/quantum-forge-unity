// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QRG.QuantumForge.Runtime;

namespace QRG.QuantumForge.Roshambo
{
    public class MeasureOnKey : MonoBehaviour
    {
        public QuantumProperty player1;
        public QuantumProperty player2;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                QuantumProperty.Measure(player1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                QuantumProperty.Measure(player2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                QuantumProperty.Measure(player1, player2);
            }
        }
    }
}
