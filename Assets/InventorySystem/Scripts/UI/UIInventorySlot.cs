using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishNet.InventorySystem.UI
{
    public class UIInventorySlot : MonoBehaviour,
        IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler,
        IDragHandler, IDropHandler, IBeginDragHandler, IEndDragHandler
    {

        [HideInInspector]
        public bool OnDropped = false;

        [SerializeField] private Image _icon = null;
        [SerializeField] private TMP_Text _quantityText = null;

        private InventoryItem _invItem;
        public InventoryItem InvItem => _invItem;
        public NetworkInventoryItem NetInvItem => _invItem.Item != null ? _invItem.ToNetInventoryItem() : NetworkInventoryItem.Null;

        private UIInventory _parent;
        public UIInventory Parent => _parent;

        private int _invIndex;
        public int InvIndex => _invIndex;

        public void Setup(NetworkInventoryItem invItem, UIInventory parent, int index)
        {
            Setup(invItem.ToInventoryItem(), parent, index);
        }
        public void Setup(InventoryItem invItem, UIInventory parent, int index)
        {
            _invIndex = index;
            _parent = parent;
            _invItem = invItem;

            if (invItem.Item != null)
            {
                _icon.color = Color.white;
                _icon.sprite = invItem.Item.Icon;
                _quantityText.text = invItem.Item.Stackable ? invItem.Quantity.ToString() : "";
            }
            else
            {
                _icon.color = Color.clear;
                _icon.sprite = null;
                _quantityText.text = "";
            }
        }

        /// <summary>
        /// Used for showing tooltip.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_invItem.IsNull) return;
            //Tooltip.Open($"{_invItem.Item.name}\n{_invItem.Item.Description}");
        }

        /// <summary>
        /// Used for hiding tooltip.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_invItem.IsNull) return;
            //Tooltip.Close();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_invItem.IsNull) return;

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Right:
                    break;

                case PointerEventData.InputButton.Left:
                    // shift-click to split stacks
                    if (InputUtils.IsShiftDown() 
                        // valid item
                        && !InvItem.IsNull 
                        // must be stackable to be splittable
                        && InvItem.Item.Stackable
                        // cant split single quantity items
                        && InvItem.Quantity > 1)
                    {
                        UIInventory.StackSplitter.Open(InvItem, this);
                    }
                    break;

                case PointerEventData.InputButton.Middle:
                    break;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!UIInventory.DraggedIcon.enabled) return;

            UIInventory.DraggedIcon.transform.position = Input.mousePosition;
        }

        // Dropped on another UI slot
        // IMPORTANT: not called when dropping outside of UI
        public void OnDrop(PointerEventData eventData)
        {
            // make sure raycast gameObjects exist, else it will error
            if (eventData == null || !eventData.pointerCurrentRaycast.isValid) return;

            UIInventorySlot toSlot = eventData?.pointerCurrentRaycast.gameObject?.GetComponent<UIInventorySlot>();

            // if not dropped on inv slot, then drop on ground/destroy item
            if (eventData.pointerCurrentRaycast.gameObject != null && toSlot != null)
            {
                UIInventorySlot fromSlot = eventData?.pointerPress?.GetComponent<UIInventorySlot>();
                if (fromSlot == null) return;
                if (fromSlot.InvItem.IsNull) return;

                fromSlot.OnDropped = true;

                // do swap
                fromSlot.Parent.Events.OnSwap?.Invoke(fromSlot.InvIndex, toSlot.Parent.Inventory, toSlot.InvIndex);
            }
        }

        // used for setting item icon on cursor
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_invItem.IsNull) return;

            ResetVars();
            _parent.Drag(_invItem.Item.Icon);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _parent.Drag(null);

            if (OnDropped)
                ResetVars();
            // was dropped outside of UI
            else if (_parent.TrashIfDroppedOutsideUi && !EventSystem.current.IsPointerOverGameObject())
                _parent.Trash(this);
        }

        void ResetVars()
        {
            OnDropped = false;
        }

        public void SplitStack(int quantity)
        {
            // invalid quantity
            if (InvItem.Quantity < quantity) return;

            // tell server to split stack
            _parent.SplitStack(InvIndex, quantity);

            // close stack splitter
            UIInventory.StackSplitter.Close();
        }

    }

}
