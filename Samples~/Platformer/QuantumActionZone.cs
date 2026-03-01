// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using QRG.QuantumForge.Platformer;
using QRG.QuantumForge.Runtime;
using UnityEngine;

namespace QRG.QuantumForge.Platformer
{
    public class QuantumActionZone : MonoBehaviour
    {
        [SerializeField] private GameObject _bottomZone;
        [SerializeField] private GameObject _topZone;
        [SerializeField] private GameObject _bottomPlayer;
        [SerializeField] private GameObject _topPlayer;

        [SerializeField] private float _activationDelay;

        private float _lastActivation = -1;

        public void OnZoneEnter()
        {
            if(Time.time - _lastActivation < _activationDelay)
            {
                return;
            }
            _lastActivation = Time.time;
            _topPlayer.transform.position = _topZone.transform.position;
            _bottomPlayer.transform.position = _bottomZone.transform.position;

            GetComponent<IQuantumAction>().apply();
        }

    }
}
