using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagesnShadows.Items;
using MagesnShadows.Database;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing.Internal;
using static MagesnShadows.Inventory.InventoryScript;
using MagesnShadows.Assets.Script.Database;
using FishNet.Managing;
using System.Threading.Tasks;
using System.Linq;
using System;
using FishNet.InventorySystem;
using MagesnShadows.Assets.Script.System.Spawners;

namespace MagesnShadows.Inventory
{
    public class InventoryScript : NetworkBehaviour
    {
        [SerializeField] private NetworkManager nm;

        [Header("Inventory Settings")]

        //[SerializeField]
        //private List<InventoryObject> container = new List<InventoryObject>();


        [SyncObject]
        public readonly SyncList<InventoryObject> SyncItems = new SyncList<InventoryObject>();

        [SerializeField] int inventorySize = 16;
        [SerializeField] KeyCode invButton = KeyCode.Tab;


        [Header("Inventory Settings")]
        [SerializeField] LayerMask pickupLayer;
        [SerializeField] float pickupDistance;
        
        [SerializeField] private ItemBase inventoryPlaceholder;
        [SerializeField] private MagesnShadows.Database.ItemDatabase _itemDb;



        Camera cam;

        [SerializeField]private ItemBase tmpItem;

#if UNITY_SERVER
        public override void OnStartServer()
        {
            base.OnStartServer();

            SyncItems.Clear();

            // LOADING: You can load your data from a persistent file or database into here
            List<InventoryObject> inv = MagesnShadows.Assets.Script.Database.Saver.LoadInventory(name);
            SyncItems.AddRange(inv != null && inv.Any() ? inv : new InventoryObject[inventorySize]);

            SyncItems.OnChange += SyncItems_OnChange;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            // SAVING:  You can save the SyncItems list (using new List<NetworkInventoryItem>(SyncItems))
            //          to your persistent file or database here.
            MagesnShadows.Assets.Script.Database.Saver.SaveInventory(new List<InventoryObject>(SyncItems), name);

            SyncItems.OnChange -= SyncItems_OnChange;
        }
#endif
        private void SyncItems_OnChange(SyncListOperation op, int index,
    InventoryObject oldItem, InventoryObject newItem, bool asServer)
        {
            // Optionally, you can save everything on the server after every change.
            // (There are better ways of saving, this is just an example).
#if UNITY_SERVER
            if (asServer)
                MagesnShadows.Assets.Script.Database.Saver.SaveInventory(new List<InventoryObject>(SyncItems), name);
#endif
        }
        public bool IsItemValid(int index)
        {
            return index >= 0 && index < SyncItems.Count
                && !SyncItems[index].IsNull;
        }
        public bool IsIndexValid(int index, out InventoryObject item)
        {
            if (index >= 0 && index < SyncItems.Count)
            {
                item = SyncItems[index];
                return true;
            }

            item = InventoryObject.Null;
            return false;
        }
        public virtual int GetItemQuantity(string itemName)
        {
            int count = 0;

            foreach (InventoryObject item in SyncItems)
                if (item.Item.ItemName == itemName)
                    count += item.Amount;

            return count;
        }
        public virtual int GetItemQuantity(InventoryObject itemObj)
        {
            int count = 0;

            foreach (InventoryObject netItem in SyncItems)
                if (netItem.Item.ItemName == itemObj.Item.name)
                    count += netItem.Amount;

            return count;
        }
        private int FindEmptySlotIndex()
        {
            for (int i = 0; i < SyncItems.Count; i++)
                if (SyncItems[i].IsNull)
                    return i;
            return -1;
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!base.IsOwner)
            {
                enabled = false;
                return;
            }



            

            cam = Camera.main;

        }
        private void Update()
        {
           
        }

#region ServerRPCs

        [ServerRpc]
        public void PickUpItem()
        {
            int a = 1;
            //#if UNITY_SERVER
            bool o = Add(new(tmpItem),out a );

            Debug.Log($"Success: {o}, with remaining: {a}");
//#endif
        }


        [ServerRpc]
        public void HarvestItem(GameObject harvestObject)
        {

            //#if UNITY_SERVER
            #region Testcode
            //var hBase = harvestObject.GetComponent<HarvestBase>().HeldItems;


            //foreach (var items in hBase)
            //{
            //    Debug.Log($"Key: {items.Key}, value: {items.Value}");
            //    int a = items.Value;
            //    Add(new(items.Key), out a);
            //    Debug.Log($"Success: {items.Key}, with remaining: {a}");
            //    harvestObject.GetComponent<HarvestBase>().Harvested();
            //}
            #endregion


            //Debug.Log($"Success: {o}, with remaining: {a}");
            //#endif
        }
        [ServerRpc]
        public void HarvestItem(Dictionary<ItemBase, int> itemsToHarvest)
        {
            if (itemsToHarvest.Count == 0) return;
            var itemArray = itemsToHarvest.ToArray();
            Debug.Log($"Here {itemsToHarvest.Count}");
            //#if UNITY_SERVER
            #region Testcode
            for (int i = 1; i <= itemsToHarvest.Count; i++)
            {
                itemsToHarvest.Keys.ElementAt(i);
                Debug.Log($"Key: {itemsToHarvest.Keys.ElementAt(i)}, value: {itemsToHarvest.Values.ElementAt(i)}");
                int a = itemsToHarvest.Values.ElementAt(i);
                bool o = Add(new(itemsToHarvest.Keys.ElementAt(i)), out a);
                Debug.Log($"Success: {o}, Item: {itemsToHarvest.Keys.ElementAt(i)}, with remaining: {a}");
                
            }


            #endregion


            //Debug.Log($"Success: {o}, with remaining: {a}");
            //#endif
        }
        [ServerRpc]
        public void HarvestItem(List<ResourceObjects> itemsToHarvest)
        {
            if (itemsToHarvest.Count == 0) return;
            var itemArray = itemsToHarvest.ToArray();
            Debug.Log($"Here {itemsToHarvest.Count}");
            //#if UNITY_SERVER
            #region Testcode
            foreach(var item in itemsToHarvest)
            {
                
                Debug.Log($"Item: {item.item.ItemName}, Amount: {item.amount}");
                int remain = item.amount;
                bool o = Add(new InventoryObject(item.item), out remain);
                Debug.Log($"Success: {o}, Item: {item.item.ItemName}, with remaining: {remain}");

            }


            #endregion


            //Debug.Log($"Success: {o}, with remaining: {a}");
            //#endif
        }

        #endregion


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

            InventoryScript toInv = toInventoryGo.GetComponent<InventoryScript>();
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

            InventoryObject fromInvItem = SyncItems[fromIndex];

            // if same item, add stacks
            if (fromInvItem.Item.ItemName == toInv.SyncItems[toIndex].Item.ItemName && fromInvItem.Item.Stackable)
            {
                InventoryObject toInvItem = toInv.SyncItems[toIndex];
                ItemBase item = toInvItem.Item;

                // combination is more than max stack, just add until full and keep original stack
                if (fromInvItem.Amount + toInvItem.Amount > item.MaxStack)
                {
                    fromInvItem.Amount -= item.MaxStack - toInvItem.Amount;
                    SyncItems[fromIndex] = fromInvItem;

                    toInvItem.Amount = item.MaxStack;
                    toInv.SyncItems[toIndex] = toInvItem;
                }
                // combine stack
                else
                {
                    toInvItem.Amount += fromInvItem.Amount;
                    toInv.SyncItems[toIndex] = toInvItem;
                    SyncItems[fromIndex] = InventoryObject.Null;
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
        public bool Add(InventoryObject addingItem, out int remainingQuantity)
        {
            ItemBase item = addingItem.Item;
            int maxStack = item.Stackable ? item.MaxStack : 1;
            Debug.Log($"Max stack {maxStack} /nItembase {item.name}, {item.MaxStack}, {item.Stackable}");
            // check for existing stacks
            if (item.Stackable)
            {

                for (int i = 0; i < this.SyncItems.Count; i++)
                {
                    if (addingItem.Amount <= 0)
                    {
                        Debug.Log("addingItem.Amount <= 0");
                        break;
                    }
                    var invItem = this.SyncItems[i];

                    // stack exists (continues if there there is no match)
                    if (invItem.Item.ItemName != addingItem.Item.ItemName)
                    {
                        Debug.Log("Stack exists.");
                        continue;
                    }
                    // add to existing stack if not over max
                    if (invItem.Amount + addingItem.Amount <= maxStack)
                    {
                        Debug.Log("Adding to existing stack if not over max");
                        invItem.Amount += addingItem.Amount;
                        addingItem.Amount = 0;
                        this.SyncItems[i] = invItem;

                        break;
                    }
                    else
                    {
                        Debug.Log("Adding quantity until inv full");
                        // add quantity until inv full
                        int diff = maxStack - invItem.Amount;
                        addingItem.Amount -= diff;
                        invItem.Amount = maxStack;
                    }

                    this.SyncItems[i] = invItem;
                }
            }

            // stacks filled, more items to add
            if (addingItem.Amount > 0)
            {
                Debug.Log("Stacks filled if statement");
                while (addingItem.Amount > 0)
                {
                    // inventory full?
                    int emptyIndex = FindEmptySlotIndex();
                    if (emptyIndex == -1) break;

                    // partial stack
                    if (addingItem.Amount <= maxStack)
                    {
                        Debug.Log("Partial stack");
                        SyncItems[emptyIndex] = addingItem;

                        addingItem.Amount = 0;

                    }
                    else
                    {
                        SyncItems[emptyIndex] = new InventoryObject
                        {
                            Item = addingItem.Item,
                            Amount = addingItem.Amount
                        };
                        Debug.Log(addingItem.Amount);
                        addingItem.Amount -= maxStack;
                        Debug.Log(addingItem.Amount);
                    }
                    Debug.Log($"SuncItem {SyncItems[emptyIndex].Amount}, adding amount: {addingItem.Amount}");
                }
                
            }

            remainingQuantity = addingItem.Amount;
            Debug.Log($"Remaining Q: {remainingQuantity}");
            // Inventory was full even after adding the items
            if (this.SyncItems.Count >= this.inventorySize && remainingQuantity > 0)
                return false;

            return true;
        }

        [Server]
        public bool Remove(ItemBase removingItem, int amount)
        {
            string itemName = removingItem.ItemName;
            int quantity = amount;

            // check if we have quantity first
            if (GetItemQuantity(itemName) < quantity) return false;

            List<int> indicesToRemove = new List<int>();

            for (int i = 0; i < SyncItems.Count; i++)
            {
                var invItem = SyncItems[i];

                if (invItem.Item.ItemName == itemName)
                {
                    // more in inv than needed
                    if (invItem.Amount >= quantity)
                    {
                        invItem.Amount -= quantity;
                        if (invItem.Amount <= 0)
                            indicesToRemove.Add(i);
                        SyncItems[i] = invItem;
                        break;
                    }
                    // remove from this stack and move on to next
                    else
                    {
                        quantity -= invItem.Amount;
                        indicesToRemove.Add(i);

                        if (quantity <= 0)
                            break;
                    }
                }
            }

            if (indicesToRemove.Any())
                for (int i = 0; i < indicesToRemove.Count; i++)
                    SyncItems[indicesToRemove[i]] = InventoryObject.Null;

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

            SyncItems[index] = InventoryObject.Null;
            return true;
        }
        /// <summary>
        /// Removes an item from the inventory at the given index and returns the item, if there was one.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>Returns true if removal was successful.
        [Server]
        public bool RemoveAt(int index, out InventoryObject item)
        {
            if (!IsItemValid(index))
            {
                item = InventoryObject.Null;
                return false;
            }

            item = SyncItems[index];
            SyncItems[index] = InventoryObject.Null;
            return true;
        }

        /// <summary>
        /// Returns a list of items potentially lost during resize (if new size is smaller). 
        /// This list can be used to re-add the items to the inventory, create item drops, etc.
        /// </summary>
        /// <param name="size"></param>New size of inventory.
        /// <param name="items"></param>List of items potentially lost if size is smaller.
        /// <returns></returns>Returns true if resize was performed successfully.
        /*[Server]
        //public bool ResizeMaintainItems(int size, out List<InventoryObject> items)
        //{
            items = new List<InventoryObject>();

            if (size == Item.MaxSize) return false;

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
        */

        /// <summary>
        /// Resizes the inventory. 
        /// Use ResizeMaintainItems() if you want to keep any potentially lost items during resize if the new size is smaller.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        /*[Server]
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
                for (int i = 0; i < size - MaxSize; i++)
                    SyncItems.Add(NetworkInventoryItem.Null);
            }

            MaxSize = size;
            return true;
        }
        */

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
            if (SyncItems[index].Amount <= quantity) return false;

            // no empty slots to split stack to
            int emptyIndex = FindEmptySlotIndex();
            if (emptyIndex == -1) return false;

            // set new stack
            SyncItems[emptyIndex] = new InventoryObject(SyncItems[index].Item, quantity);

            // set old stack
            InventoryObject oldStack = SyncItems[index];
            oldStack.Amount -= quantity;
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
            List<InventoryObject> items = new List<InventoryObject>(SyncItems);

            switch (type)
            {
                // by name
                case SortType.Name:
                    items = items.OrderBy(x => string.IsNullOrEmpty(x.Item.ItemName))
                        .ThenBy(x => x.Item.ItemName)
                        .ToList();
                    break;
                case SortType.NameDesc:
                    items = items.OrderBy(x => string.IsNullOrEmpty(x.Item.ItemName))
                        .ThenByDescending(x => x.Item.ItemName)
                        .ToList();
                    break;

                // by quantity
                case SortType.Quantity:
                    items = items.OrderBy(x => string.IsNullOrEmpty(x.Item.ItemName))
                        .ThenBy(x => x.Amount)
                        .ToList();
                    break;
                case SortType.QuantityDesc:
                    items = items.OrderBy(x => string.IsNullOrEmpty(x.Item.ItemName))
                        .ThenByDescending(x => x.Amount)
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
        public void Deposit(InventoryScript otherInventory, TransactonType transactonType)
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
                    SyncItems[i] = new InventoryObject(SyncItems[i].Item, remaining);
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
        public void Withdraw(InventoryScript otherInventory, TransactonType transactonType)
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
                    otherInventory.SyncItems[i] = new InventoryObject(otherInventory.SyncItems[i].Item, remaining);
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
                    || SyncItems[i].Amount == SyncItems[i].Item.MaxStack) continue;

                if (!itemIndices.ContainsKey(SyncItems[i].Item.ItemName))
                    itemIndices.Add(SyncItems[i].Item.ItemName, new List<int>());

                itemIndices[SyncItems[i].Item.ItemName].Add(i);
            }

            foreach (var itemIndexEntry in itemIndices)
            {
                // skip items with only one stack
                if (itemIndexEntry.Value.Count == 1) continue;

                // cache item for ease
                Debug.Log("Have to cache item from database");
                //InventoryObject item = MagesnShadows.Database.ItemDatabase(itemIndexEntry.Key);
                InventoryObject item = new InventoryObject(_itemDb.FindByName(itemIndexEntry.Key));
                // backwards addition
                for (int i = itemIndexEntry.Value.Count - 1; i >= 0; i--)
                {
                    int index = itemIndexEntry.Value[i];

                    // already max stack
                    if (SyncItems[index].Amount == item.Item.MaxStack) continue;

                    // find an item with a non-full stack
                    for (int j = 0; j < itemIndexEntry.Value.Count; j++)
                    {
                        int tempIndex = itemIndexEntry.Value[j];

                        // dont add to itself
                        if (tempIndex == index) continue;

                        // item can go null if we modified it while combining stacks
                        if (SyncItems[tempIndex].IsNull || SyncItems[tempIndex].Amount == item.Item.MaxStack) continue;

                        InventoryObject tempItem = SyncItems[tempIndex];
                        int diff = item.Item.MaxStack - tempItem.Amount;

                        // enough to combine entire stack to the base stack and continue to next item stack
                        if (SyncItems[index].Amount <= diff)
                        {
                            tempItem.Amount += SyncItems[index].Amount;
                            SyncItems[index] = InventoryObject.Null;
                            SyncItems[tempIndex] = tempItem;
                            break;
                        }
                        // only combine a portion and move on to find the next stack to add to
                        else
                        {
                            tempItem.Amount = item.Item.MaxStack;
                            SyncItems[index] = new InventoryObject(item.Item, SyncItems[index].Amount - diff);
                            SyncItems[tempIndex] = tempItem;
                        }
                    }

                }
            }
        }

//#endif
#endregion






        [System.Serializable]
        public class InventoryObject
        {
            public static InventoryObject Null => new InventoryObject { Item = null, Amount = 0 };

            public ItemBase Item = null;
            public int Amount = -1;

            public bool IsNull => string.IsNullOrEmpty(Item.ItemName) || Amount <= 0;

            public InventoryObject(ItemBase itemBase, int quantity)
            {
                Item = itemBase;
                Amount = quantity;
            }

            public InventoryObject()
            {
                // ItemBase = itemName;
            }
            public InventoryObject(ItemBase itemBase)
            {
                Item = itemBase;
            }
            public int CompareTo(InventoryObject other)
            {
                throw new NotImplementedException();
            }
        }

    }








}