using System.Collections.Generic;
using UnityEngine;

namespace FishNet.InventorySystem
{

    [CreateAssetMenu(menuName = "FishNet/Inventory System/Item Database")]
    public class ItemDatabase : ScriptableObject
    {

        public List<Item> Items;
        private static Dictionary<string, Item> _itemDict;

        public static Item GetItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return null;
            if (!_itemDict.TryGetValue(itemName, out Item item))
                Debug.LogError($"Item with name '{itemName}' not found.");
            return item;
        }

#if UNITY_EDITOR
        private void Awake()
        {
            Items = EditorUtils.FindScriptableObjects<Item>();
            InitItemDict();
        }

        private void OnValidate()
        {
            Items = EditorUtils.FindScriptableObjects<Item>();
            //InitItemDict();
        }
#endif

        void InitItemDict()
        {
            //_itemDict = new Dictionary<string, Item>();
            //foreach (Item item in Items)
            //    _itemDict.Add(item.name, item);
        }

        public void Init()
        {
            InitItemDict();
        }

    }

}
