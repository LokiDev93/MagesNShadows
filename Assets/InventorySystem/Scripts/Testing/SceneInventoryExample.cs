using FishNet.InventorySystem.UI;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FishNet.InventorySystem.Testing
{

    /// <summary>
    /// This is an example of an inventory that doesn't require ownership to be used,
    /// such as stashes, banks, etc.
    /// </summary>
    public class SceneInventoryExample : NetworkBehaviour
    {

        [SerializeField] private UIInventory _uiSceneInventory = null;
        [SerializeField] private UIInventoryTesting _testUiSceneInventory = null;

        private Inventory _inventory;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
        }

        public void OnSwap(int fromIndex, Inventory toInventory, int toIndex)
        {
            if (IsServer)
                Swap(fromIndex, toInventory.gameObject, toIndex);
            else
                CmdSwap(fromIndex, toInventory.gameObject, toIndex);
        }

        void OnAddButton()
        {
            if (IsServer)
                Add(_testUiSceneInventory.SelectedItem);
            else
                CmdAdd(_testUiSceneInventory.SelectedItem);
        }

        void OnRemoveButton()
        {
            if (IsServer)
                Remove(_testUiSceneInventory.SelectedItem);
            else
                CmdRemove(_testUiSceneInventory.SelectedItem);
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            _testUiSceneInventory.AddButton.onClick.AddListener(OnAddButton);
            _testUiSceneInventory.RemoveButton.onClick.AddListener(OnRemoveButton);

            // swap callback
            _uiSceneInventory.Events.OnSwap.AddListener(OnSwap);
            // trash/drop callback

            if (IsServer)
            {
                _uiSceneInventory.Events.OnTrash.AddListener((index) => Trash(index));
                _uiSceneInventory.Events.OnSplitStack.AddListener((invIndex, quantity) => SplitStack(invIndex, quantity));
                _uiSceneInventory.Events.OnResize.AddListener((size, maintainItems) => Resize(size, maintainItems));
                _uiSceneInventory.Events.OnSort.AddListener((type) => _inventory.Sort(type));
                _uiSceneInventory.Events.OnCombineStacks.AddListener(() => _inventory.CombineStacks());
            }
            else
            {
                _uiSceneInventory.Events.OnTrash.AddListener((index) => CmdTrash(index));
                _uiSceneInventory.Events.OnSplitStack.AddListener((invIndex, quantity) => CmdSplitStack(invIndex, quantity));
                _uiSceneInventory.Events.OnResize.AddListener((size, maintainItems) => CmdResize(size, maintainItems));
                _uiSceneInventory.Events.OnSort.AddListener((type) => CmdSort(type));
                _uiSceneInventory.Events.OnCombineStacks.AddListener(() => CmdCombineStacks());
            }


            _uiSceneInventory.Open(_inventory);
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            _testUiSceneInventory.AddButton.onClick.RemoveListener(OnAddButton);
            _testUiSceneInventory.RemoveButton.onClick.RemoveListener(OnRemoveButton);
            // swap callback
            _uiSceneInventory.Events.OnSwap.RemoveListener(OnSwap);
            // trash/drop callback
            _uiSceneInventory.Events.OnTrash.RemoveAllListeners();
            // resize callback
            _uiSceneInventory.Events.OnResize.RemoveAllListeners();
            // split stack callback
            _uiSceneInventory.Events.OnSplitStack.RemoveAllListeners();
            // sorting
            _uiSceneInventory.Events.OnSort.RemoveAllListeners();
            // combine stacks
            _uiSceneInventory.Events.OnCombineStacks.RemoveAllListeners();


            _uiSceneInventory.Close();
            _uiSceneInventory.ClearSlots();
        }

        // an important distinction between this example and the PlayerInventoryExample
        // is that the player requires ownership to modify their own inventory.
        // so for these, you will need to provide extra checks according to your game, such as a distance check
        [ServerRpc(RequireOwnership = false)]
        public void CmdAdd(NetworkInventoryItem addingItem) => Add(addingItem);
        [Server]
        void Add(NetworkInventoryItem addingItem)
        {
            if (!_inventory.Add(addingItem, out int remaining))
            {
                // returns false if the entirety of the item was not added,
                // can use remaining quantity to make an item drop
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdRemove(NetworkInventoryItem removingItem) => Remove(removingItem);
        [Server]
        void Remove(NetworkInventoryItem removingItem)
        {
            if (!_inventory.Remove(removingItem))
            {
                // returns false if the given item quantity exceeds the amount contained
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdSwap(int fromIndex, GameObject toInventoryGo, int toIndex) => Swap(fromIndex, toInventoryGo, toIndex);
        [Server]
        void Swap(int fromIndex, GameObject toInventoryGo, int toIndex)
        {
            if (!_inventory.Swap(fromIndex, toInventoryGo, toIndex))
            {
                // returns false if the swap fails
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdTrash(int index) => Trash(index);
        [Server]
        public void Trash(int index)
        {
            if (!_inventory.RemoveAt(index))
            {
                // returns false if index invalid
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void CmdResize(int size, bool maintainItems) => Resize(size, maintainItems);
        [Server]
        private void Resize(int size, bool maintainItems)
        {
            if (maintainItems)
            {
                _inventory.ResizeMaintainItems(size, out List<NetworkInventoryItem> items);
                // You can do whatever you want with the items.
                // Here i just try to add them back to the inventory. Making item drops is another option.
                if (items.Any())
                {
                    foreach (var item in items)
                        if (!item.IsNull)
                            _inventory.Add(item, out int remaining);
                }
            }
            else
            {
                _inventory.Resize(size);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void CmdSplitStack(int index, int quantity) => SplitStack(index, quantity);
        [Server]
        void SplitStack(int index, int quantity)
        {
            if (!_inventory.SplitStack(index, quantity))
            {
                // false if no room in inventory to split the stack
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void CmdSort(SortType type)
        {
            _inventory.Sort(type);
        }

        [ServerRpc(RequireOwnership = false)]
        void CmdCombineStacks() => _inventory.CombineStacks();

    }

}
