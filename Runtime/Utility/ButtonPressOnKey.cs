// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QRG.QuantumForge.Utility
{
    public class ButtonPressOnKey : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private KeyCode key;

        [SerializeField] private TMPro.TextMeshProUGUI text;

        void Start()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button == null)
            {
                Debug.LogError($"{gameObject.name}: ButtonPressOnKey needs a button");
            }

            if (text != null)
            {
                text.text = key.ToString();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(key))
            {
                _button.onClick.Invoke();
                FadeToColor(_button.colors.pressedColor);
            }
            else if (Input.GetKeyUp(key))
            {
                FadeToColor(_button.colors.normalColor);
            }
        }

        void FadeToColor(Color color)
        {
            var graphic = _button.targetGraphic;
            graphic.CrossFadeColor(color, _button.colors.fadeDuration, true, true);
        }
    }
}
