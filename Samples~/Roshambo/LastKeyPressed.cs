// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace QRG.QuantumForge
{
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class LastKeyPressed : MonoBehaviour
    {
        
        private TMPro.TextMeshProUGUI text;

        void OnEnable()
        {
            if (text == null) text = GetComponent<TextMeshProUGUI>();
        }
        // Update is called once per frame
        void Update()
        {
            if (Input.anyKeyDown)
            {
                text.text = Input.inputString;
            }
        }
    }
}
