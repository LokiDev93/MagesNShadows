namespace FishNet.InventorySystem
{

    [System.Serializable]
    public struct InventoryItem
    {
        public static InventoryItem Null => new InventoryItem() { Item = null, Quantity = 0 };

        public Item Item;
        public int Quantity;

        public bool IsNull => Item == null || Quantity <= 0;

        public NetworkInventoryItem ToNetInventoryItem()
        {
            return new NetworkInventoryItem
            {
                ItemName = Item.name,
                Quantity = Quantity
            };
        }

        public override string ToString()
        {
            return $"{Item?.name ?? "null"} ({Quantity})";
        }

    }

}
