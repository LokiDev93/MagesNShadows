using FishNet.Connection;
using FishNet.InventorySystem.UI;
using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FishNet.InventorySystem.Testing
{

    public class PlayerInventoryExample : NetworkBehaviour
    {

        private UIInventory _uiInventory;
        private Inventory _inventory;

        private UIInventory _sceneInventory;

        /// <summary>
        /// Used for testing only, in order to add/remove items at will for the example.
        /// </summary>
        private UIInventoryTesting _testing;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);

            // gained ownership
            if (base.IsOwner)
            {
                // Get the UI reference however you prefer, this is not the best way to do it but it works for the example
                _uiInventory = GameObject.Find("PlayerInventory").GetComponent<UIInventory>();
                _uiInventory.Open(_inventory);

                // Same here, for this example we can just find it
                // It's probably best to keep a list of open/active inventories on client/server
                _sceneInventory = GameObject.Find("SceneInventory").GetComponent<UIInventory>();

                // add/remove callbacks (for testing)
                _testing = _uiInventory.GetComponent<UIInventoryTesting>();
                _testing.AddButton.onClick.AddListener(() => CmdAdd(_testing.SelectedItem));
                _testing.RemoveButton.onClick.AddListener(() => CmdRemove(_testing.SelectedItem));

                // swap callback
                _uiInventory.Events.OnSwap.AddListener((fromIndex, toInv, toIndex) => CmdSwap(fromIndex, toInv.gameObject, toIndex));

                // trash/drop callback
                _uiInventory.Events.OnTrash.AddListener((index) => CmdTrash(index));

                // resize callback
                _uiInventory.Events.OnResize.AddListener((size, maintainItems) => CmdResize(size, maintainItems));

                // split stack callback
                _uiInventory.Events.OnSplitStack.AddListener((invIndex, quantity) => CmdSplitStack(invIndex, quantity));

                // sorting
                _uiInventory.Events.OnSort.AddListener((type) => CmdSort(type));

                // withdraw/deposit
                _uiInventory.Events.OnWithdrawAll.AddListener(() => CmdWithdrawAll(_sceneInventory.Inventory.gameObject));
                _uiInventory.Events.OnDepositAll.AddListener(() => CmdDepositAll(_sceneInventory.Inventory.gameObject));
                _uiInventory.Events.OnWithdrawExisting.AddListener(() => CmdWithdrawExisting(_sceneInventory.Inventory.gameObject));
                _uiInventory.Events.OnDepositExisting.AddListener(() => CmdDepositExisting(_sceneInventory.Inventory.gameObject));

                // combine stacks
                _uiInventory.Events.OnCombineStacks.AddListener(() => CmdCombineStacks());
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (base.IsOwner)
            {
                if (_testing != null)
                {
                    _testing.AddButton.onClick.RemoveAllListeners();
                    _testing.RemoveButton.onClick.RemoveAllListeners();
                }

                // clean up event listeners
                _uiInventory.Events.OnSwap.RemoveAllListeners();
                _uiInventory.Events.OnTrash.RemoveAllListeners();
                _uiInventory.Events.OnResize.RemoveAllListeners();
                _uiInventory.Events.OnSplitStack.RemoveAllListeners();
                _uiInventory.Events.OnSort.RemoveAllListeners();
                _uiInventory.Events.OnWithdrawAll.RemoveAllListeners();
                _uiInventory.Events.OnDepositAll.RemoveAllListeners();
                _uiInventory.Events.OnWithdrawExisting.RemoveAllListeners();
                _uiInventory.Events.OnDepositExisting.RemoveAllListeners();

                // close UI
                _uiInventory.Close();
            }
        }

        // an important distinction between this example and the SceneInventoryExample
        // is that the player requires ownership to modify their own inventory.
        [ServerRpc]
        public void CmdAdd(NetworkInventoryItem addingItem)
        {
            if (!_inventory.Add(addingItem, out int remaining))
            {
                // returns false if not the entirety of item was added,
                // can use remaining quantity to make an item drop
            }
        }

        [ServerRpc]
        public void CmdRemove(NetworkInventoryItem removingItem)
        {
            if (!_inventory.Remove(removingItem))
            {
                // returns false if the given item quantity exceeds the amount contained
            }
        }

        [ServerRpc]
        public void CmdSwap(int fromIndex, GameObject toInventoryGo, int toIndex)
        {
            if (!_inventory.Swap(fromIndex, toInventoryGo, toIndex))
            {
                // returns false if the swap fails
            }
        }

        [ServerRpc]
        public void CmdTrash(int index)
        {
            if (!_inventory.RemoveAt(index))
            {
                // returns false if index invalid
            }
            else
            {
                // can send confirmation back to client for them to play a sound
            }
        }

        [ServerRpc]
        private void CmdResize(int size, bool maintainItems)
        {
            if (maintainItems)
            {
                _inventory.ResizeMaintainItems(size, out List<NetworkInventoryItem> items);
                // You can do whatever you want with the items.
                // Here i just try to add them back to the inventory, but making item drops is another option.
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

        [ServerRpc]
        void CmdSplitStack(int index, int quantity)
        {
            if (!_inventory.SplitStack(index, quantity))
            {
                // false if no room in inventory to split the stack
            }
        }

        [ServerRpc]
        void CmdSort(SortType type)
        {
            _inventory.Sort(type);
        }

        [ServerRpc]
        void CmdCombineStacks() => _inventory.CombineStacks();

        [ServerRpc]
        void CmdWithdrawAll(GameObject otherInventory)
        {
            if (otherInventory.TryGetComponent<Inventory>(out Inventory inventory))
                _inventory.Withdraw(inventory, TransactonType.All);
            else
                Debug.LogWarning($"GameObject {otherInventory?.name} does not have an Inventory component");
        }

        [ServerRpc]
        void CmdDepositAll(GameObject otherInventory)
        {
            if (otherInventory.TryGetComponent<Inventory>(out Inventory inventory))
                _inventory.Deposit(inventory, TransactonType.All);
            else
                Debug.LogWarning($"GameObject {otherInventory?.name} does not have an Inventory component");
        }

        [ServerRpc]
        void CmdWithdrawExisting(GameObject otherInventory)
        {
            if (otherInventory.TryGetComponent<Inventory>(out Inventory inventory))
                _inventory.Withdraw(inventory, TransactonType.Existing);
            else
                Debug.LogWarning($"GameObject {otherInventory?.name} does not have an Inventory component");
        }

        [ServerRpc]
        void CmdDepositExisting(GameObject otherInventory)
        {
            if (otherInventory.TryGetComponent<Inventory>(out Inventory inventory))
                _inventory.Deposit(inventory, TransactonType.Existing);
            else
                Debug.LogWarning($"GameObject {otherInventory?.name} does not have an Inventory component");
        }

    }

}
