using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishNet.InventorySystem.UI
{

    public class UIStackSplitter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {


        [SerializeField] private GameObject _panel = null;
        [SerializeField] private TMP_Text _invStackText = null;
        [SerializeField] private Slider _splitAmountSlider = null;
        [SerializeField] private TMP_Text _splitStackText = null;
        [SerializeField] private Button _splitButton = null;

        private UIInventorySlot _slot;
        private InventoryItem _invItem;
        private bool _mouseOver;

        private void Start()
        {
            Close();
        }

        private void Update()
        {
            // close by pressing escape or clicking off
            if (_panel.activeInHierarchy 
                && (Input.GetKeyDown(KeyCode.Escape) 
                    || (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape) && !_mouseOver)))
            {
                Close();
            }
        }

        public void Open(InventoryItem invItem, UIInventorySlot parent)
        {
            // cleanup first if already open
            if (_panel.activeInHierarchy)
                Close();

            // activate panel
            _panel.SetActive(true);

            // appear on top
            transform.SetAsLastSibling();

            // show next to item
            _panel.transform.position = parent.transform.position + new Vector3(0f, _panel.GetComponent<RectTransform>().sizeDelta.y);

            _slot = parent;
            _invItem = invItem;

            // remove listener just in case of back to back splits
            _splitButton.onClick.RemoveListener(OnSplitButton);
            _splitButton.onClick.AddListener(OnSplitButton);

            // setup slider
            _splitAmountSlider.onValueChanged.RemoveListener(OnSliderChanged);
            _splitAmountSlider.onValueChanged.AddListener(OnSliderChanged);
            _splitAmountSlider.wholeNumbers = true;
            _splitAmountSlider.maxValue = invItem.Quantity;
            _splitAmountSlider.minValue = 0;
            _splitAmountSlider.value = 1;

            // update current values
            OnSliderChanged(_splitAmountSlider.value);
        }

        private void OnSliderChanged(float newValue)
        {
            _invStackText.text = (_invItem.Quantity - (int)newValue).ToString();
            _splitStackText.text = newValue.ToString();
        }

        public void Close()
        {
            _splitButton.onClick.RemoveListener(OnSplitButton);
            _splitAmountSlider.onValueChanged.RemoveListener(OnSliderChanged);
            
            _slot = null;
            _invItem = InventoryItem.Null;

            _panel.SetActive(false);
        }

        private void OnSplitButton()
        {
            // nothing to split
            if (_splitAmountSlider.value == 0)
            {
                Close();
                return;
            }

            // send split data back to parent
            _slot.SplitStack((int)_splitAmountSlider.value);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _mouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseOver = false;
        }

    }

}
