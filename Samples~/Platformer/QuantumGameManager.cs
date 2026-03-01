// Copyright (c) 2025 Quantum Realm Games, LLC. All rights reserved.
// See LICENSE.md for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Platformer;
using UnityEngine.SceneManagement;

namespace QRG.QuantumForge.Platformer
{
    public class QuantumGameManager : MonoBehaviour
    {
        public int coinsCounter = 0;

        public GameObject playerGameObject;
        [SerializeField] private PlayerController[] players;
        public GameObject deathPlayerPrefab;
        public Text coinText;

        void Start()
        {
            if (players == null || players.Length == 0)
            {
                Debug.LogError($"{gameObject.name}: Set Players in editor");
            }
        }

        void Update()
        {
            coinText.text = coinsCounter.ToString();
            foreach (var player in players)
            {
                if (player.deathState == true)
                {
                    playerGameObject.SetActive(false);
                    GameObject deathPlayer = (GameObject)Instantiate(deathPlayerPrefab, playerGameObject.transform.position, playerGameObject.transform.rotation);
                    deathPlayer.transform.localScale = new Vector3(playerGameObject.transform.localScale.x, playerGameObject.transform.localScale.y, playerGameObject.transform.localScale.z);
                    player.deathState = false;
                }
            }
            //Invoke("ReloadLevel", 3);

        }

        private void ReloadLevel()
        {
            //Application.LoadLevel(Application.loadedLevel);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}