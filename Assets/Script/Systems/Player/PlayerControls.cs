using MagesnShadows.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows
{
    public class PlayerControls : MonoBehaviour
    {
        [SerializeField] private InventoryScript script;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                string o = string.Empty;
                foreach(var i in script.SyncItems)
                {
                    o = o + $"Item {i.Item.ItemName} with {i.Amount} \n";
                }
                Debug.Log(o);
            }
        }
    }
}
