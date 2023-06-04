using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows.NPC.Enemy
{
    public class EnemyBase : CharStats
    {

        [SerializeField] private GameObject targetPlayer;
        [SerializeField] private GameObject spawner;
        public override void OnStartServer()
        {
            base.OnStartServer();


        }
        // Update is called once per frame
        void Update()
        {
        
        }

        [Server]
        public void SetSpawner(GameObject spawnerObj)
        {
            spawner = spawnerObj;

        }
    }


}
