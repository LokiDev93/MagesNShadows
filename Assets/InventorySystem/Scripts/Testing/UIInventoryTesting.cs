using FishNet.InventorySystem.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishNet.InventorySystem.Testing
{

    public class UIInventoryTesting : MonoBehaviour
    {

        [SerializeField] private ItemDatabase _itemDb = null;
        [SerializeField] private TMP_Dropdown _itemList = null;
        [SerializeField] private TMP_InputField _itemQuantity = null;

        [SerializeField] private Button _addButton = null;
        public Button AddButton => _addButton;

        [SerializeField] private Button _removeButton = null;
        public Button RemoveButton => _removeButton;

        [Header("Resizing")]
        [SerializeField] private TMP_InputField _invSize = null;
        [SerializeField] private Toggle _resizeMaintainItemsToggle = null;

        [Header("Sorting")]
        [SerializeField] private Button _sortButton = null;
        [SerializeField] private TMP_Dropdown _sortType = null;

        [Header("Deposit/Withdraw")]
        [SerializeField] private Button _depositAllButton = null;
        [SerializeField] private Button _withdrawAllButton = null;
        [SerializeField] private Button _depositExistingButton = null;
        [SerializeField] private Button _withdrawExistingButton = null;

        [Header("Combining Stacks")]
        [SerializeField] private Button _combineStacksButton = null;

        private UIInventory _uiInventory;

        public NetworkInventoryItem SelectedItem => new NetworkInventoryItem(
                _itemList.options[_itemList.value].text,
                ItemDatabase.GetItem(_itemList.options[_itemList.value].text).Stackable ? int.Parse(_itemQuantity.text) : 1
            );

        public SortType SelectedSortType => (SortType)_sortType.value;

        private void Awake()
        {
            _uiInventory = GetComponent<UIInventory>();
        }

        private void Start()
        {
            InitItemList();
            InitSortDropdown();
        }

        private void OnEnable()
        {
            _invSize.onEndEdit.AddListener(OnInvSizeChanged);
            _uiInventory.Events.OnOpen.AddListener(UIInventory_OnOpen);

            _sortButton.onClick.AddListener(OnSortButton);

            _depositAllButton.onClick.AddListener(OnDepositAllButton);
            _withdrawAllButton.onClick.AddListener(OnWithdrawAllButton);

            _depositExistingButton.onClick.AddListener(OnDepositExistingButton);
            _withdrawExistingButton.onClick.AddListener(OnWithdrawExistingButton);

            _combineStacksButton.onClick.AddListener(OnCombineStacksButton);
        }

        private void OnDisable()
        {
            _invSize.onEndEdit.RemoveListener(OnInvSizeChanged);
            _uiInventory.Events.OnOpen.RemoveListener(UIInventory_OnOpen);

            _sortButton.onClick.RemoveListener(OnSortButton);

            _depositAllButton.onClick.RemoveListener(OnDepositAllButton);
            _withdrawAllButton.onClick.RemoveListener(OnWithdrawAllButton);

            _depositExistingButton.onClick.RemoveListener(OnDepositExistingButton);
            _withdrawExistingButton.onClick.RemoveListener(OnWithdrawExistingButton);

            _combineStacksButton.onClick.RemoveListener(OnCombineStacksButton);
        }

        private void OnCombineStacksButton() => _uiInventory.Events.OnCombineStacks?.Invoke();

        private void OnSortButton() => _uiInventory.Events.OnSort?.Invoke(SelectedSortType);

        private void OnWithdrawAllButton() => _uiInventory.Events.OnWithdrawAll?.Invoke();
        private void OnDepositAllButton() => _uiInventory.Events.OnDepositAll?.Invoke();

        private void OnWithdrawExistingButton() => _uiInventory.Events.OnWithdrawExisting?.Invoke();
        private void OnDepositExistingButton() => _uiInventory.Events.OnDepositExisting?.Invoke();

        private void UIInventory_OnOpen()
        {
            _invSize.text = _uiInventory.Inventory.MaxSize.ToString();
        }

        private void OnInvSizeChanged(string size)
        {
            if (int.TryParse(size, out int newSize))
            {
                // invalid size
                if (newSize <= 0) return;

                // same size
                if (_uiInventory.Inventory.MaxSize == newSize) return;

                _uiInventory.Events.OnResize?.Invoke(newSize, _resizeMaintainItemsToggle.isOn);
            }
        }

        void InitItemList()
        {
            _itemList.ClearOptions();
            List<string> items = new List<string>();
            foreach (var item in _itemDb.Items)
                items.Add(item.name);
            _itemList.AddOptions(items);
        }

        void InitSortDropdown()
        {
            _sortType.ClearOptions();
            _sortType.AddOptions(new List<string>(System.Enum.GetNames(typeof(SortType))));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
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

    }

}
