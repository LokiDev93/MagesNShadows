using FishNet.Object;
using MagesnShadows.Inventory;
using MagesnShadows.Items;
using MagesnShadows.PlayerSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Object.Synchronizing;
using MagesnShadows.Assets.Script.System.Spawners;

namespace MagesnShadows
{
    [RequireComponent(typeof(InventoryScript))]
    public class CharacterInteraction : NetworkBehaviour
    {
        [SerializeField] private InventoryScript inventoryScript;
        [SerializeField] KeyCode interactKey = KeyCode.E;
        [SerializeField] LayerMask layerMask;
        [SerializeField] private float rayDis = 150;
        [SerializeField] private Player playerScript;
        public override void OnStartClient()
        {
            base.OnStartClient();

            inventoryScript = this.gameObject.GetComponent<InventoryScript>();

            if (!base.IsOwner)
            {
                inventoryScript.enabled = false;
                this.enabled = false;
                return;
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(interactKey))
            {
                HarvestCall();

            }
        }

        
        public void HarvestCall()
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, rayDis, layerMask))
            {
                Transform trans = hitInfo.transform;
                if (trans.GetComponent<HarvestBase>() != null)
                {
                    //inventoryScript.HarvestItem(hitInfo.collider.gameObject);
                    List<ResourceObjects> harvestItems = new List<ResourceObjects>();
                    
                    harvestItems = trans.GetComponent<HarvestBase>().HeldItems.GetCollection(true) ;
                    //hitInfo.transform.GetComponent<IHarvestable>().Harvested();
                    Debug.DrawRay(Camera.main.transform.position, trans.position, Color.green);
                    trans.GetComponent<IHarvestable>().Harvested();
                    inventoryScript.HarvestItem(harvestItems);
                    return;
                }



            }
            else
            {
                Debug.Log("Miss");
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward, Color.red);
            }
        }



    }
}
