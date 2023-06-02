using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FishNet.InventorySystem
{

    public class Inventory : NetworkBehaviour
    {

        public int MaxSize = 12;

        [SyncObject]
        public readonly SyncList<NetworkInventoryItem> SyncItems = new SyncList<NetworkInventoryItem>();

        [SerializeField] private ItemDatabase _itemDb = null;
        private static ItemDatabase _dbRef;

        protected virtual void Awake()
        {
            if (_dbRef == null)
            {
                _dbRef = _itemDb;
                _dbRef.Init();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            SyncItems.Clear();

            // LOADING: You can load your data from a persistent file or database into here
            List<NetworkInventoryItem> inv = Saver.LoadInventory(name);
            SyncItems.AddRange(inv != null && inv.Any() ? inv : new NetworkInventoryItem[MaxSize]);

            SyncItems.OnChange += SyncItems_OnChange;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            // SAVING:  You can save the SyncItems list (using new List<NetworkInventoryItem>(SyncItems))
            //          to your persistent file or database here.
            Saver.SaveInventory(new List<NetworkInventoryItem>(SyncItems), name);

            SyncItems.OnChange -= SyncItems_OnChange;
        }

        private void SyncItems_OnChange(SyncListOperation op, int index, 
            NetworkInventoryItem oldItem, NetworkInventoryItem newItem, bool asServer)
        {
            // Optionally, you can save everything on the server after every change.
            // (There are better ways of saving, this is just an example).
            if (asServer)
                Saver.SaveInventory(new List<NetworkInventoryItem>(SyncItems), name);
        }

        /// <summary>
        /// Returns true if the index is not out of range and the item at the index is not null.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsItemValid(int index)
        {
            return index >= 0 && index < SyncItems.Count
                && !SyncItems[index].IsNull;
        }
        /// <summary>
        /// Checks if the index is not out of range and returns the item at the given index, if any.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsIndexValid(int index, out NetworkInventoryItem item)
        {
            if (index >= 0 && index < SyncItems.Count)
            {
                item = SyncItems[index];
                return true;
            }

            item = NetworkInventoryItem.Null;
            return false;
        }

        /// <summary>
        /// Returns the quantity of items with the given name.
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public virtual int GetItemQuantity(string itemName)
        {
            int count = 0;

            foreach (NetworkInventoryItem item in SyncItems)
                if (item.ItemName == itemName)
                    count += item.Quantity;

            return count;
        }
        /// <summary>
        /// Returns the quantity of the given item in the inventory item list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual int GetItemQuantity(Item item)
        {
            int count = 0;

            foreach (NetworkInventoryItem netItem in SyncItems)
                if (netItem.ItemName == item.name)
                    count += netItem.Quantity;

            return count;
        }

        /// <summary>
        /// Finds an empty slot in the inventory item list and returns its index.
        /// Returns -1 if no slots are empty.
        /// </summary>
        /// <returns></returns>Returns index of an empty item slot, or -1 if none.
        private int FindEmptySlotIndex()
        {
            for (int i = 0; i < SyncItems.Count; i++)
                if (SyncItems[i].IsNull)
                    return i;
            return -1;
        }


        #region Server

        // Uncomment this preprocessor directive if you want FishNet to automatically strip the code from a server build
        //#if UNITY_SERVER

        [Server]
        public bool Swap(int fromIndex, GameObject toInventoryGo, int toIndex)
        {
            if (toInventoryGo == null)
            {
                Debug.LogError("ToInvGo was null");
                return false;
            }

            if (fromIndex < 0 || fromIndex >= SyncItems.Count)
            {
                Debug.LogError("Invalid fromIndex");
                return false;
            }

            Inventory toInv = toInventoryGo.GetComponent<Inventory>();
            if (toInv == null)
            {
                Debug.LogError("Toinv was null on GO: " + toInventoryGo.name);
                return false;
            }

            if (toIndex < 0 || toIndex >= toInv.SyncItems.Count)
            {
                Debug.LogError("Invalid toIndex");
                return false;
            }

            NetworkInventoryItem fromInvItem = SyncItems[fromIndex];

            // if same item, add stacks
            if (fromInvItem.ItemName == toInv.SyncItems[toIndex].ItemName && fromInvItem.Item.Stackable)
            {
                NetworkInventoryItem toInvItem = toInv.SyncItems[toIndex];
                Item item = toInvItem.Item;

                // combination is more than max stack, just add until full and keep original stack
                if (fromInvItem.Quantity + toInvItem.Quantity > item.MaxStack)
                {
                    fromInvItem.Quantity -= item.MaxStack - toInvItem.Quantity;
                    SyncItems[fromIndex] = fromInvItem;
                    
                    toInvItem.Quantity = item.MaxStack;
                    toInv.SyncItems[toIndex] = toInvItem;
                }
                // combine stack
                else
                {
                    toInvItem.Quantity += fromInvItem.Quantity;
                    toInv.SyncItems[toIndex] = toInvItem;
                    SyncItems[fromIndex] = NetworkInventoryItem.Null;
                }
            }
            // just swap the two
            else
            {
                SyncItems[fromIndex] = toInv.SyncItems[toIndex];
                toInv.SyncItems[toIndex] = fromInvItem;
            }

            return true;
        }

        [Server]
        public bool Add(NetworkInventoryItem addingItem, out int remainingQuantity)
        {
            Item item = addingItem.Item;
            int maxStack = item.Stackable ? item.MaxStack : 1;

            // check for existing stacks
            if (item.Stackable)
            {
                for (int i = 0; i < this.SyncItems.Count; i++)
                {
                    if (addingItem.Quantity <= 0) break;

                    var invItem = this.SyncItems[i];

                    // stack exists
                    if (invItem.ItemName != addingItem.ItemName) continue;

                    // add to existing stack if not over max
                    if (invItem.Quantity + addingItem.Quantity <= maxStack)
                    {
                        invItem.Quantity += addingItem.Quantity;
                        addingItem.Quantity = 0;
                        this.SyncItems[i] = invItem;

                        break;
                    }
                    else
                    {
                        // add quantity until inv full
                        int diff = maxStack - invItem.Quantity;
                        addingItem.Quantity -= diff;
                        invItem.Quantity = maxStack;
                    }

                    this.SyncItems[i] = invItem;
                }
            }

            // stacks filled, more items to add
            if (addingItem.Quantity > 0)
            {
                while (addingItem.Quantity > 0)
                {
                    // inventory fyll?
                    int emptyIndex = FindEmptySlotIndex();
                    if (emptyIndex == -1) break;

                    // partial stack
                    if (addingItem.Quantity <= maxStack)
                    {
                        SyncItems[emptyIndex] = addingItem;
                        addingItem.Quantity = 0;
                    }
                    else
                    {
                        SyncItems[emptyIndex] = new NetworkInventoryItem
                        {
                            ItemName = addingItem.ItemName,
                            Quantity = maxStack
                        };
                        addingItem.Quantity -= maxStack;
                    }
                }
            }

            remainingQuantity = addingItem.Quantity;

            // Inventory was full even after adding the items
            if (this.SyncItems.Count >= this.MaxSize && remainingQuantity > 0)
                return false;

            return true;
        }

        [Server]
        public bool Remove(NetworkInventoryItem removingItem)
        {
            string itemName = removingItem.ItemName;
            int quantity = removingItem.Quantity;

            // check if we have quantity first
            if (GetItemQuantity(itemName) < quantity) return false;

            List<int> indicesToRemove = new List<int>();

            for (int i = 0; i < SyncItems.Count; i++)
            {
                var invItem = SyncItems[i];

                if (invItem.ItemName == itemName)
                {
                    // more in inv than needed
                    if (invItem.Quantity >= quantity)
                    {
                        invItem.Quantity -= quantity;
                        if (invItem.Quantity <= 0)
                            indicesToRemove.Add(i);
                        SyncItems[i] = invItem;
                        break;
                    }
                    // remove from this stack and move on to next
                    else
                    {
                        quantity -= invItem.Quantity;
                        indicesToRemove.Add(i);

                        if (quantity <= 0)
                            break;
                    }
                }
            }

            if (indicesToRemove.Any())
                for (int i = 0; i < indicesToRemove.Count; i++)
                    SyncItems[indicesToRemove[i]] = NetworkInventoryItem.Null;

            return true;
        }

        /// <summary>
        /// Removes an item from the inventory at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>Returns true if removal was successful.
        [Server]
        public bool RemoveAt(int index)
        {
            if (!IsItemValid(index)) return false;

            SyncItems[index] = NetworkInventoryItem.Null;
            return true;
        }
        /// <summary>
        /// Removes an item from the inventory at the given index and returns the item, if there was one.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>Returns true if removal was successful.
        [Server]
        public bool RemoveAt(int index, out NetworkInventoryItem item)
        {
            if (!IsItemValid(index))
            {
                item = NetworkInventoryItem.Null;
                return false;
            }

            item = SyncItems[index];
            SyncItems[index] = NetworkInventoryItem.Null;
            return true;
        }

        /// <summary>
        /// Returns a list of items potentially lost during resize (if new size is smaller). 
        /// This list can be used to re-add the items to the inventory, create item drops, etc.
        /// </summary>
        /// <param name="size"></param>New size of inventory.
        /// <param name="items"></param>List of items potentially lost if size is smaller.
        /// <returns></returns>Returns true if resize was performed successfully.
        [Server]
        public bool ResizeMaintainItems(int size, out List<NetworkInventoryItem> items)
        {
            items = new List<NetworkInventoryItem>();

            if (size == MaxSize) return false;

            // no need to maintain if size isnt smaller
            if (size > MaxSize)
            {
                Debug.LogWarning("New size is greater than current size, so no items will be lost. Use Resize() instead.");
                return Resize(size);
            }

            // maintain items if new size is smaller
            if (size < MaxSize)
                items.AddRange(SyncItems.GetCollection(true).GetRange(size, MaxSize - size));

            return Resize(size);
        }

        /// <summary>
        /// Resizes the inventory. 
        /// Use ResizeMaintainItems() if you want to keep any potentially lost items during resize if the new size is smaller.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [Server]
        public bool Resize(int size)
        {
            if (size == MaxSize) return false;

            if (size < MaxSize)
            {
                for (int i = MaxSize - 1; i >= size; i--)
                    SyncItems.RemoveAt(i);
            }
            else if (size > MaxSize)
            {
                for (int i = 0; i < size-MaxSize; i++)
                    SyncItems.Add(NetworkInventoryItem.Null);
            }

            MaxSize = size;
            return true;
        }

        /// <summary>
        /// Splits the item stack at the given index by the given quantity and
        /// assigns it to an empty inventory slot.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [Server]
        public bool SplitStack(int index, int quantity)
        {
            // invalid index
            if (!IsItemValid(index)) return false;

            // item is not stackable, thus not splittable
            if (!SyncItems[index].Item.Stackable) return false;

            // invalid quantity
            if (SyncItems[index].Quantity <= quantity) return false;

            // no empty slots to split stack to
            int emptyIndex = FindEmptySlotIndex();
            if (emptyIndex == -1) return false;

            // set new stack
            SyncItems[emptyIndex] = new NetworkInventoryItem(SyncItems[index].ItemName, quantity);

            // set old stack
            NetworkInventoryItem oldStack = SyncItems[index];
            oldStack.Quantity -= quantity;
            SyncItems[index] = oldStack;

            return true;
        }

        /// <summary>
        /// Sorts the inventory by the given type.
        /// </summary>
        /// <param name="type"></param>How to sort items.
        [Server]
        public void Sort(SortType type)
        {
            List<NetworkInventoryItem> items = new List<NetworkInventoryItem>(SyncItems);

            switch (type)
            {
                // by name
                case SortType.Name:
                    items = items.OrderBy(x => string.IsNullOrEmpty(x.ItemName))
                        .ThenBy(x=>x.ItemName)
                        .ToList();
                    break;
                case SortType.NameDesc:
                    items = items.OrderBy(x => string.IsNullOrEmpty(x.ItemName))
                        .ThenByDescending(x => x.ItemName)
                        .ToList();
                    break;

                // by quantity
                case SortType.Quantity:
                    items = items.OrderBy(x => string.IsNullOrEmpty(x.ItemName))
                        .ThenBy(x => x.Quantity)
                        .ToList();
                    break;
                case SortType.QuantityDesc:
                    items = items.OrderBy(x => string.IsNullOrEmpty(x.ItemName))
                        .ThenByDescending(x => x.Quantity)
                        .ToList();
                    break;
            }

            SyncItems.Clear();
            SyncItems.AddRange(items);
        }

        /// <summary>
        /// Deposits items from this inventory to another by the given transactionType.
        /// </summary>
        /// <param name="otherInventory"></param>
        /// <param name="transactonType"></param>
        [Server]
        public void Deposit(Inventory otherInventory, TransactonType transactonType)
        {
            for (int i = 0; i < SyncItems.Count; i++)
            {
                // skip null items
                if (SyncItems[i].IsNull) continue;

                // only withdraw existing items?
                if (transactonType == TransactonType.Existing 
                    && (!otherInventory.SyncItems.Contains(SyncItems[i])
                        // only stackables can be deposited if they already exist
                        || !SyncItems[i].Item.Stackable)) continue;

                // if an add failed, then we ran out of space. return the remaining items to the inventory and exit
                if (!otherInventory.Add(SyncItems[i], out int remaining))
                {
                    SyncItems[i] = new NetworkInventoryItem(SyncItems[i].ItemName, remaining);
                    return;
                }
                else
                {
                    // succesfully added to other, so remove it from this inventory
                    RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Withdraws items from the given inventory to this one by the given transactionType.
        /// </summary>
        /// <param name="otherInventory"></param>
        /// <param name="transactonType"></param>
        [Server]
        public void Withdraw(Inventory otherInventory, TransactonType transactonType)
        {
            for (int i = 0; i < otherInventory.SyncItems.Count; i++)
            {
                if (otherInventory.SyncItems[i].IsNull) continue;

                // only withdraw existing items?
                if (transactonType == TransactonType.Existing 
                    && (!SyncItems.Contains(otherInventory.SyncItems[i])
                        // only stackables can be withdrawn if they already exist
                        || !otherInventory.SyncItems[i].Item.Stackable)) continue;

                // if an add failed, then we ran out of space. return the remaining items to the inventory and exit
                if (!Add(otherInventory.SyncItems[i], out int remaining))
                {
                    otherInventory.SyncItems[i] = new NetworkInventoryItem(otherInventory.SyncItems[i].ItemName, remaining);
                    return;
                }
                else
                {
                    // succesfully added to this inventory, so remove it from other
                    otherInventory.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Combines stacks of items, if possible.
        /// </summary>
        [Server]
        public void CombineStacks()
        {
            // first get all the existing stacks
            // indexed by item name and list of indices the item exists at
            Dictionary<string, List<int>> itemIndices = new Dictionary<string, List<int>>();
            for (int i = 0; i < SyncItems.Count; i++)
            {
                // skip null, non-stackable items, and fully stacked items
                if (SyncItems[i].IsNull || !SyncItems[i].Item.Stackable 
                    || SyncItems[i].Quantity == SyncItems[i].Item.MaxStack) continue;

                if (!itemIndices.ContainsKey(SyncItems[i].ItemName))
                    itemIndices.Add(SyncItems[i].ItemName, new List<int>());

                itemIndices[SyncItems[i].ItemName].Add(i);
            }

            foreach (var itemIndexEntry in itemIndices)
            {
                // skip items with only one stack
                if (itemIndexEntry.Value.Count == 1) continue;

                // cache item for ease
                Item item = ItemDatabase.GetItem(itemIndexEntry.Key);

                // backwards addition
                for (int i = itemIndexEntry.Value.Count-1; i >= 0; i--)
                {
                    int index = itemIndexEntry.Value[i];

                    // already max stack
                    if (SyncItems[index].Quantity == item.MaxStack) continue;

                    // find an item with a non-full stack
                    for (int j = 0; j < itemIndexEntry.Value.Count; j++)
                    {
                        int tempIndex = itemIndexEntry.Value[j];

                        // dont add to itself
                        if (tempIndex == index) continue;

                        // item can go null if we modified it while combining stacks
                        if (SyncItems[tempIndex].IsNull || SyncItems[tempIndex].Quantity == item.MaxStack) continue;

                        NetworkInventoryItem tempItem = SyncItems[tempIndex];
                        int diff = item.MaxStack - tempItem.Quantity;

                        // enough to combine entire stack to the base stack and continue to next item stack
                        if (SyncItems[index].Quantity <= diff)
                        {
                            tempItem.Quantity += SyncItems[index].Quantity;
                            SyncItems[index] = NetworkInventoryItem.Null;
                            SyncItems[tempIndex] = tempItem;
                            break;
                        }
                        // only combine a portion and move on to find the next stack to add to
                        else
                        {
                            tempItem.Quantity = item.MaxStack;
                            SyncItems[index] = new NetworkInventoryItem(item.name, SyncItems[index].Quantity - diff);
                            SyncItems[tempIndex] = tempItem;
                        }
                    }

                }
            }
        }

        //#endif
        #endregion


        #region Editor
#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();

            if (_itemDb == null)
            {
                var dbs = EditorUtils.FindScriptableObjects<ItemDatabase>();
                if (dbs.Count > 1)
                    Debug.LogError("There should only be one ItemDatabase in your project.");
                else if (dbs.Count == 0)
                {
                    Debug.LogError("No ItemDatabase found. Create one using the right-click menu > FishNet > InventorySystem.");
                    return;
                }
                _itemDb = dbs[0];
            }
        }

#endif
        #endregion


    }

    public enum TransactonType : byte
    {
        /// <summary>
        /// Deposit/withdraw all items.
        /// </summary>
        All,
        /// <summary>
        /// Only withdraw/deposit existing stackable items to the given inventory.
        /// </summary>
        Existing,
    }

    public enum SortType : byte
    {
        /// <summary>
        /// Sort by name ascending.
        /// </summary>
        Name,
        /// <summary>
        /// Sort by name descending.
        /// </summary>
        NameDesc,

        /// <summary>
        /// Sort by quantity ascending.
        /// </summary>
        Quantity,
        /// <summary>
        /// Sort by quantity descending.
        /// </summary>
        QuantityDesc,


    }

}
