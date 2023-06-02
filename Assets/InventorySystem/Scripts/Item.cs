using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FishNet.InventorySystem
{

    [CreateAssetMenu(menuName = "FishNet/Inventory System/Item")]
    public class Item : ScriptableObject
    {

        [Header("Visuals")]
        public Sprite Icon;
        public Color Color = Color.white;

        [Header("Data")]
        public bool Stackable = true;
        public int MaxStack = 200;
        [TextArea(5, 50)]
        public string Description;

        public NetworkInventoryItem ToNetInvItem(int quantity = 1)
        {
            return new NetworkInventoryItem
            {
                ItemName = name,
                Quantity = quantity
            };
        }

    }


    #region Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(Item))]
    public class ItemEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            var item = (Item)target;
            if (item == null) return;

            Texture2D tex = AssetPreview.GetAssetPreview(item.Icon);
            GUILayout.Label(tex);

            base.OnInspectorGUI();
        }

    }

#endif
    #endregion

}
