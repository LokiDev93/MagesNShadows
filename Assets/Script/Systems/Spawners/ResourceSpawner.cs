using FishNet.Connection;
using FishNet.InventorySystem;
using FishNet.Object;
using MagesnShadows.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace MagesnShadows.Assets.Script.System.Spawners
{
    public class ResourceSpawner : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] GameObject[] posObjToSpawn;
        [SerializeField] List<ResourceObjects> posResources;

        [Header("Running Info")]
        [SerializeField] float respawnTime, timeTillRespawn;
        [SerializeField] GameObject spawnedObject;
        [SerializeField] Transform trans;
        [SerializeField] bool regenerating;

        // Start is called before the first frame update
        void Start()
        {



        }

        // Update is called once per frame
        void Update()
        {

            if(regenerating == true)
            {
                if (timeTillRespawn <= 0)
                {
                    SpawnResourceObject();
                    regenerating = false;
                }
                else
                {
                    timeTillRespawn = timeTillRespawn - Time.deltaTime;
                }
            }


        }

        [Server]
        public override void OnSpawnServer(NetworkConnection connection)
        {
            base.OnSpawnServer(connection);
            /* This callback occurs after a spawn message for this
            * object has been sent out to a client. For example: if
            * this object will be visible to five clients, then this
            * callback will occur five times, where the connection
            * parameter will be for each client. 
            * Primarily you will use this callback to send
            * tailored communications to the client
            * the object is being spawned for. */
            //#if UNITY_SERVER
            int r = 0;
            if (posObjToSpawn.Length == 0)
            {
                Debug.Log($"{this.gameObject.name} has not possible objects to spawn.");
                r++;
            }
            if (posResources.Count == 0)
            {
                Debug.Log($"{this.gameObject.name} has not possible resource(s) to give is empty.");
                r++;
            }

            if (r != 0)
            {
                Debug.Log($"{this.gameObject.name} is being destroyed. It was not fully set up. Fail score is: {r}");
                ServerManager.Despawn(this.gameObject);
            }
            trans = this.gameObject.transform;
            timeTillRespawn = respawnTime;



            //#endif

            SpawnResourceObject();

        }

        [ServerRpc(RequireOwnership = false)]
        public void Regen()
        {
            regenerating = true;
        }

        [Server]
        private void SpawnResourceObject()
        {
            List<ResourceObjects> itemsToGive = new List<ResourceObjects>();
            foreach (var item in posResources)
            {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance == 0) continue;
                if (item.lowerChance > chance && item.upperChance < chance)
                {
                    item.amount = UnityEngine.Random.Range(item.minAmount, item.maxAmount);
                }

            }
            int i = UnityEngine.Random.Range(0, posObjToSpawn.Length - 1);
            spawnedObject = Instantiate(posObjToSpawn[i], trans);
            spawnedObject.GetComponent<HarvestBase>().HeldItems.AddRange(itemsToGive);

            ServerManager.Spawn(spawnedObject);
            spawnedObject.GetComponent<HarvestBase>().SetSpawner(this.gameObject);

        }







    }

    //[CreateAssetMenu(fileName = "New Resource Object", menuName = "Assets/Items/Item")]
    [Serializable]
    public class ResourceObjects //: ScriptableObject
    {
        public ItemBase item;
        public int amount, minAmount, maxAmount;
        public int lowerChance, upperChance;
    }
}
