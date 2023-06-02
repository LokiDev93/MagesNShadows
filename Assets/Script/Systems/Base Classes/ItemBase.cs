using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MagesnShadows.Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Assets/Items/Item")]
    [System.Serializable]
    public class ItemBase : ScriptableObject
    {
        [SerializeField, TextArea] private string itemName;
        [TextArea] private string itemInternalName;
        [SerializeField] private int itemID = 0;
        [SerializeField, TextArea] public string itemDesc;
        [SerializeField] bool itemBanned = false;
        [SerializeField] private bool tradeAble = false;
        [SerializeField] private int maxStack;
        [SerializeField] private GameObject itemModel;
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private bool itemStackable;

        public string ItemName => itemName;
        public string ItemInternalName => itemInternalName;
        public int ItemID => itemID;
        public int MaxStack => maxStack;
        public bool ItemBanned => itemBanned;
        public bool ItemTradeAble => tradeAble;
        public GameObject ItemModel => itemModel;
        public Sprite ItemIcon => itemIcon;

        public bool Stackable => itemStackable;
    }
}
