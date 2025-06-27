using CourseworkManagedLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightLib
{
    public class GameManager : MonoBehaviour
    {
        public TimedSpawner collectableSpawner, enemySpawner;
        public ScoreKeeper scoreKeeper;

        public static GameManager instance;


        [SerializeField] internal float outgoingDamageInterval;
        [SerializeField] internal Transform playerTransform;
        [SerializeField] internal float damageImpulseThreshold;
        [SerializeField] internal float damageImpulseMultiplier;
        [SerializeField] internal LayerMask playerMask;
        [SerializeField] internal float viewConeDotThreshold;
        [SerializeField] internal float retreatTime = 10;

        [SerializeField] List<Transform> wandererWaypoints;
        [SerializeField] bool showGizmos;
        [SerializeField] internal Player player;
        [SerializeField] Slider healthBar;
        [SerializeField] TextMeshProUGUI scoreDisplay;
        [SerializeField] internal float pickupHealth;
        [SerializeField] internal CanvasGroup screenBlackout;
        [SerializeField] bool endingGame;
        [SerializeField] float screenFadeSpeed;
        [SerializeField] AnimationCurve blackoutAlphaCurve;
        internal Transform GetRandomWaypoint()
        {
            return wandererWaypoints[UnityEngine.Random.Range(0, wandererWaypoints.Count)];
        }

        private void Awake()
        {
            //Initialise a singleton - this will be needed for a lot of values for the enemies
            InitialiseSingleton();
            collectableSpawner.StartSpawnDelay(this);
            enemySpawner.StartSpawnDelay(this);
            player = FindAnyObjectByType<Player>();
            if (healthBar)
            {
                healthBar.maxValue = player.maxHealth;
                healthBar.value = player.currentHealth;
            }
            scoreDisplay.text = $"Score: {scoreKeeper.CurrentScore}";
            screenBlackout.gameObject.SetActive(false);
        }
        void InitialiseSingleton()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }
        private void FixedUpdate()
        {
            if (!instance)
            {
                InitialiseSingleton();
            }
            if (!playerTransform)
                playerTransform = FindObjectOfType<Player>().transform;

        }
        private void OnDrawGizmos()
        {
            if (showGizmos)
            {
                Gizmos.matrix = Matrix4x4.identity;
                for (int i = 0; i < collectableSpawner.validSpawnPoints.Count; i++)
                {
                    Gizmos.DrawWireSphere(collectableSpawner.validSpawnPoints[i].position, collectableSpawner.spawnPointRadius);
                }
                for (int i = 0; i < enemySpawner.validSpawnPoints.Count; i++)
                {
                    Gizmos.DrawWireSphere(enemySpawner.validSpawnPoints[i].position, collectableSpawner.spawnPointRadius);
                }

                for (int i = 0; i < wandererWaypoints.Count; i++)
                {
                    if (wandererWaypoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(wandererWaypoints[i].position, 1);
                        for (int h = 0; h < wandererWaypoints.Count; h++)
                        {

                            if (h != i)
                            {
                                if (wandererWaypoints[i] == null)
                                {
                                    return;
                                }
                                Gizmos.DrawLine(wandererWaypoints[i].position, wandererWaypoints[h].position);
                            }
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Updates the health bar and checks if the player is dead.
        /// </summary>
        public void PlayerHit()
        {
            healthBar.value = player.currentHealth;
            PlayerDeath();
        }
        /// <summary>
        /// Updates the score counter and incremenets the ScoreKeeper
        /// </summary>
        public void UpdateScore()
        {
            scoreKeeper.IncrementScore();
            scoreDisplay.text = $"Score: {scoreKeeper.CurrentScore}";
        }
        public void PlayerDeath()
        {
            if(player.currentHealth < 0 && !endingGame)
            {
                endingGame = true;
                StartCoroutine(EndGame());
            }
        }
        /// <summary>
        /// Fades the screen to black, removes all enemies, resets score, respawns player and then restores the screen
        /// </summary>
        /// <returns></returns>
        IEnumerator EndGame()
        {
            float t = 0;
            screenBlackout.gameObject.SetActive(true);
            while (t < 1)
            {
                t += Time.fixedDeltaTime * screenFadeSpeed;
                screenBlackout.alpha = blackoutAlphaCurve.Evaluate(t);
                yield return new WaitForFixedUpdate();
            }
            //screen is blacked out, reset everything
            scoreKeeper.ResetScore();
            var enemies = FindObjectsOfType<Enemy>(true);
            for (int i = enemies.Length -1; i > 0; i--)
            {
                Destroy(enemies[i].gameObject);
            }
            player.Respawn();
            while (t > 0)
            {
                t -= Time.fixedDeltaTime * screenFadeSpeed;
                screenBlackout.alpha = blackoutAlphaCurve.Evaluate(t);
                yield return new WaitForFixedUpdate();
            }
            screenBlackout.gameObject.SetActive(false);
            endingGame = false;
            yield break;
        }
    }
}