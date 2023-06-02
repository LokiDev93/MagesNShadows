using System;

namespace FishNet.InventorySystem
{

    /// <summary>
    /// This is a struct because it is more efficient over the network.
    /// </summary>
    [System.Serializable]
    public struct NetworkInventoryItem : IComparable<NetworkInventoryItem>
    {

        public static NetworkInventoryItem Null => new NetworkInventoryItem { ItemName = null, Quantity = 0 };

        public string ItemName;
        public int Quantity;

        public NetworkInventoryItem(string itemName, int quantity)
        {
            ItemName = itemName;
            Quantity = quantity;
        }

        public NetworkInventoryItem(string itemName) : this()
        {
            ItemName = itemName;
        }

        public Item Item => ItemDatabase.GetItem(ItemName);

        public bool IsNull => string.IsNullOrEmpty(ItemName) || Quantity <= 0;

        public static NetworkInventoryItem Parse(string s)
        {
            try
            {
                string[] splits = s.Split('(');
                string itemName = splits[0].Trim(' ');
                int quantity = int.Parse(splits[1].Trim(')'));
                return new NetworkInventoryItem(itemName, quantity);
            }
            catch
            {
                return Null;
            }
        }

        public InventoryItem ToInventoryItem()
        {
            return new InventoryItem()
            {
                Item = ItemDatabase.GetItem(ItemName),
                Quantity = this.Quantity
            };
        }

        public override string ToString()
        {
            return $"{ItemName} ({Quantity})";
        }

        // IComparable sorting
        public int CompareTo(NetworkInventoryItem other)
        {
            // null is always lesser
            if (other.IsNull)
                return 1;
            else // sort by name
                return ItemName.CompareTo(other.ItemName);
        }

        public override bool Equals(object obj)
        {
            if (obj is NetworkInventoryItem netInvItem)
            {
                return netInvItem.ItemName == this.ItemName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

}
