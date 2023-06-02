using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MagesnShadows.Items;


namespace MagesnShadows.Database
{
    //[CreateAssetMenu(fileName = "New Item Database", menuName = "Assets/Databases/Item Database")]
    [System.Serializable]
    public class ItemDatabase : MonoBehaviour
    {
        [SerializeField] private List<ItemBase> allItems;


        public ItemBase FindByID(int id)
        {
            var r = from a in allItems
                    where a.ItemID == id
                    select a;
            var o = r.FirstOrDefault();
            return o;
        }
        public ItemBase FindByName(string itemName)
        {
            var r = from a in allItems
                    where a.ItemName == itemName
                    select a;
            var o = r.FirstOrDefault();
            return o;
        }
        public ItemBase FindByInternalName(string internalName)
        {
            var r = from a in allItems
                    where a.ItemInternalName == internalName
                    select a;
            var o = r.FirstOrDefault();
            return o;
        }
    }
}
