using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MagesnShadows.Assets.Script.System.Spawners;
using MagesnShadows.Database;
using MagesnShadows.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows
{
    public class HarvestBase : NetworkBehaviour, IHarvestable
    {
        //[SerializeField] public Dictionary<ItemBase, int> HeldItems {  get; private set; }
        [SyncObject][SerializeField] public readonly SyncList<ResourceObjects> HeldItems = new SyncList<ResourceObjects>();
        [SerializeField] GameObject spawnerRef;

        [Server] public GameObject GetGameObject()
        {
            return spawnerRef;
        }
        [Server]
        public void SetSpawner(GameObject spawner)
        {
            spawnerRef = spawner;
        }

        void Start()
        {
            //HeldItems = new Dictionary<ItemBase, int>();

        }


        
        public void Harvested()
        {
            ServerDespawn();
        }

        private void SyncItems_OnChange(SyncListOperation op, int index,
    ResourceObjects oldItem, ResourceObjects newItem, bool asServer)
        {
            // Optionally, you can save everything on the server after every change.
            // (There are better ways of saving, this is just an example).
#if UNITY_SERVER
            if (asServer)
                MagesnShadows.Assets.Script.Database.Saver.SaveInventory(new List<InventoryObject>(SyncItems), name);
#endif
        }

        //#if UNITY_SERVER
        [Server]
        //public void ItemSet(Dictionary<ItemBase, int> items)
        public void Add(List<ResourceObjects> items)
        {
            HeldItems.AddRange(items);
        }
        public override void OnSpawnServer(NetworkConnection connection)
        {
            base.OnSpawnServer(connection);
            
        }
        [ServerRpc(RequireOwnership = false)]
        private void ServerDespawn()
        {
            spawnerRef.GetComponent<ResourceSpawner>().Regen();
            Despawn();
        }

        //#endif
    }
}
