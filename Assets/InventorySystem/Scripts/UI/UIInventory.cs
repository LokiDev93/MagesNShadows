using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishNet.InventorySystem.UI
{

    public class UIInventory : MonoBehaviour
    {

        [System.Serializable]
        public class UIInventoryEvents
        {
            public UnityEvent OnOpen;
            /// <summary>
            /// Invokes the fromIndex, toInventory, and toIndex relating to the swap.
            /// </summary>
            [Tooltip("Invokes the fromIndex, toInventory, and toIndex relating to the swap.")]
            public UnityEvent<int, Inventory, int> OnSwap;
            /// <summary>
            /// Invokes the index of the item to be trashed.
            /// </summary>
            public UnityEvent<int> OnTrash;
            /// <summary>
            /// Invokes the new size of the inventory and if items should be maintained.
            /// </summary>
            public UnityEvent<int, bool> OnResize;
            /// <summary>
            /// Invokes the item index and quantity of item to split from existing stack.
            /// </summary>
            public UnityEvent<int, int> OnSplitStack;
            public UnityEvent<SortType> OnSort;
            public UnityEvent OnWithdrawAll;
            public UnityEvent OnDepositAll;
            public UnityEvent OnWithdrawExisting;
            public UnityEvent OnDepositExisting;
            public UnityEvent OnCombineStacks;
        }

        /// <summary>
        /// Used for preventing other inputs while dragging.
        /// </summary>
        public static bool IsDragging;
        /// <summary>
        /// Single reference to the image of the currently dragged item.
        /// </summary>
        public static Image DraggedIcon;
        /// <summary>
        /// Single reference to the stack splitter menu when shift-clicking an item.
        /// </summary>
        public static UIStackSplitter StackSplitter;

        [Tooltip("Trashes the dragged item if it was dropped outside of an inventory slot.")]
        public bool TrashIfDroppedOutsideUi = true;

        [Tooltip("Reference to the child panel which holds all inventory UI.")]
        [SerializeField] protected GameObject panel = null;
        [Tooltip("Reference to the content transform that holds the UI inventory slots.")]
        [SerializeField] protected Transform _content = null;

        /// <summary>
        /// List of instantiated inventory slots for reuse and count equalization when opening inventory UI.
        /// </summary>
        private List<UIInventorySlot> _invSlots = new List<UIInventorySlot>();

        /// <summary>
        /// Reference to the inventory this UI is drawing from.
        /// </summary>
        private Inventory _inventory;
        /// <summary>
        /// Public reference to the inventory this UI is drawing from.
        /// </summary>
        public Inventory Inventory => _inventory;

        [Header("Prefabs")]
        [Tooltip("Dragged item icon prefab used to populate the singleton reference.")]
        [SerializeField] private Image _draggedIconPrefab = null;
        [Tooltip("Stack splitter prefab used to populate the singleton reference.")]
        [SerializeField] private UIStackSplitter _stackSplitterPrefab = null;
        [Tooltip("Inventory slot prefab reference.")]
        [SerializeField] protected UIInventorySlot _slotPrefab = null;

        [Header("Events")]
        public UIInventoryEvents Events;

        private void Awake()
        {
            if (DraggedIcon == null)
                DraggedIcon = Instantiate(_draggedIconPrefab, transform.root);
            if (StackSplitter == null)
                StackSplitter = Instantiate(_stackSplitterPrefab, transform.root);
        }

        private void Start()
        {
            panel.SetActive(false);
        }

        /// <summary>
        /// Opens the inventory UI and draws the given inventory.
        /// </summary>
        /// <param name="inv"></param>Inventory to draw from.
        public virtual void Open(Inventory inv)
        {
            panel.SetActive(true);
            _inventory = inv;
            _inventory.SyncItems.OnChange += SyncItems_OnChange;
            SetupSlots();
            Events.OnOpen?.Invoke();
        }

        /// <summary>
        /// Closes the inventory UI.
        /// </summary>
        public void Close()
        {
            if (_inventory != null)
            {
                _inventory.SyncItems.OnChange -= SyncItems_OnChange;
                _inventory = null;
            }
            if (panel!=null)
                panel.SetActive(false);
        }

        public void ClearSlots()
        {
            foreach (var item in _invSlots)
                if (item!=null)
                    Destroy(item.gameObject);
            _invSlots.Clear();
        }

        /// <summary>
        /// Event listener required to draw slots whenever the inventory changes over the network.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="index"></param>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <param name="asServer"></param>
        private void SyncItems_OnChange(SyncListOperation op, int index,
            NetworkInventoryItem oldItem, NetworkInventoryItem newItem, bool asServer)
        {
            // setup slots once per change
            if (op == SyncListOperation.Complete || asServer)
                SetupSlots();
        }

        /// <summary>
        /// Equalizes the amount of instantiated UI inventory slots with the size of the inventory.
        /// </summary>
        void EqualizeSlotCount()
        {
            if (_invSlots.Count == _inventory.SyncItems.Count) return;

            // too many UI slots
            if (_invSlots.Count > _inventory.SyncItems.Count)
            {
                for (int i = _invSlots.Count-1; i >= _inventory.SyncItems.Count; i--)
                {
                    Destroy(_invSlots[i].gameObject);
                    _invSlots.RemoveAt(i);
                }
            }
            // not enough UI inv slots
            else if (_inventory.SyncItems.Count > _invSlots.Count)
            {
                for (int i = _invSlots.Count; i < _inventory.SyncItems.Count; i++)
                {
                    UIInventorySlot slot = Instantiate(_slotPrefab, _content);
                    _invSlots.Add(slot);
                }
            }
        }

        /// <summary>
        /// Creates and sets up UI inventory slots based on the inventory item SyncList.
        /// </summary>
        void SetupSlots()
        {
            EqualizeSlotCount();

            for (int i = 0; i < _inventory.SyncItems.Count; i++)
            {
                _invSlots[i].Setup(
                    // pass in inv item if it exists
                    i < _inventory.SyncItems.Count && !_inventory.SyncItems[i].IsNull
                        ? _inventory.SyncItems[i]
                        : NetworkInventoryItem.Null,
                    // this is the parent of the slot
                    this,
                    // slot index
                    i
                );
            }
        }

        /// <summary>
        /// Sets the dragged icon enabled based on if the given icon is null or not.
        /// </summary>
        /// <param name="icon"></param>Icon to draw. Pass null to hide dragged icon.
        public void Drag(Sprite icon)
        {
            DraggedIcon.enabled = icon != null;
            DraggedIcon.transform.SetAsLastSibling();
            DraggedIcon.sprite = icon;
            UIInventory.IsDragging = icon != null;
        }

        // Called from the trash image's EventTrigger component
        // can use this to destroy the item or make an item drop etc. it just removes it from the inventory
        public void OnDropTrash(BaseEventData ev)
        {
            // this should always be true
            if (ev is PointerEventData eventData)
            {
                // get the slot
                UIInventorySlot fromSlot = eventData?.pointerPress?.GetComponent<UIInventorySlot>();
                if (fromSlot == null) return;

                Trash(fromSlot);
            }
        }

        /// <summary>
        /// Trashes the item in a given inventory slot.
        /// </summary>
        /// <param name="slot"></param>
        public void Trash(UIInventorySlot slot)
        {
            // make sure the slot is from this inventory and not from another one
            if (!_invSlots.Contains(slot)) return;

            // trash it!
            Events.OnTrash?.Invoke(slot.InvIndex);
        }

        public void SplitStack(int index, int quantity)
        {
            if (index < 0 || index >= _invSlots.Count) return;
            if (quantity >= _invSlots[index].InvItem.Quantity) return;

            Events.OnSplitStack?.Invoke(index, quantity);
        }

    }

}
