using FishNet.Object;
using MagesnShadows.NPC.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows
{
    public class EnemySpawner : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] EnemyBase[] posEnemies;

        [Header("Running Info")]
        [SerializeField] float timeBetweenWaves, timeTillWave;
        //[SerializeField] GameObject spawnedObject;
        [SerializeField] Transform trans;
        [SerializeField] bool regenerating;

        public override void OnStartServer()
        {
            base.OnStartServer();
            timeTillWave = timeBetweenWaves;

        }

        // Update is called once per frame
        void Update()
        {
        
        }
        [Server]
        public void StartWave()
        {

        }


        [Server]
        private void SpawnEnemy()
        {
            GameObject spawnedEnemy = null;

            int i = UnityEngine.Random.Range(0, posEnemies.Length - 1);
            spawnedEnemy = Instantiate(posEnemies[i].gameObject, trans);

            ServerManager.Spawn(spawnedEnemy);
            spawnedEnemy.GetComponent<EnemyBase>().SetSpawner(gameObject);

        }
    }
}
