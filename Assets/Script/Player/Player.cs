using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagesnShadows.Inventory;
using System.Linq;
using FishNet.Object.Synchronizing;
using System;
using FishNet.Connection;
using FishNet.Object;

namespace MagesnShadows.PlayerSystems
{

    public class Player : NetworkBehaviour
    {
        [SerializeField] InventoryScript inventory;
        [SerializeField] GameObject gameManager;
        [SerializeField] GameObject databases;
        [SerializeField] GameObject playerObj;
        [SerializeField] GameObject cameraHolder;
        [SerializeField] Camera cameraPrefab;
        [SerializeField] public Camera cameraMain { get; private set; }

        //[SerializeField] List<InventorySlot> playerSlots;

        private void Awake()
        {
            //inventory = gameManager.GetComponent<InventoryObject>();
            playerObj = GetComponentInParent<Transform>().root.gameObject;
            //for (int i = 0; i < 20; i++)
            //{
            //    playerSlots.Add(new InventorySlot());
                
            //}
            
        }
        private void Start()
        {
            
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            /* This is called on each client when the object
            * becomes visible to them. Networked values such as
            * Owner, ObjectId, and SyncTypes will already be
            * synchronized prior to this callback. */

            if (!base.IsOwner)
            {
                //GameObject cam = this.transform.Find("Ocean Camera").gameObject;
                //if (cam == null)
                //{
                //    Debug.Log("Null camera");
                //    return;
                //}
                //if (!cam.transform.IsChildOf(this.transform))
                //{
                //    Debug.Log("Cam isnt a child of this object");
                //}
                //else
                //{
                //    Destroy(cameraMain);
                //    this.enabled = false;
                //}
                Destroy(cameraMain);
                this.enabled = false;


            }

            //cameraHolder = this.gameObject.transform.Find("CameraHolder").gameObject;
            // var sC = Instantiate(cameraPrefab, cameraHolder.transform);
            //sC.gameObject.SetActive(true);
            // Debug.Log(sC.gameObject.activeSelf);

        }


        // Start is called before the first frame update
        void Update()
        {

            if (Input.GetKey(KeyCode.P))
            {
                //inventory.AddItem(inventory.tmpItem, 1);
            }

            
        }

        // Update is called once per frame
        void FixedUpdate()
        {

        }


    }
}
