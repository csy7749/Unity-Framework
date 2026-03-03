using System;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(SuperButton))]
    public class SuperToggleItem : MonoBehaviour
    {
        public GameObject SelectedGo;
        public GameObject UnSelectedGo;

        [SerializeField] private bool _isOn;

        private SuperButton _itemButton;
        private SuperToggle _parentToggle;
        private int _selfIndex = -1;
        private Action<bool> _onValueChanged;
        private Func<bool> _conditionCheck = () => true;

        private SuperImage _selectedImage;
        private SuperText _selectedText;
        private SuperImage _unSelectedImage;
        private SuperText _unSelectedText;

        public int SelfIndex => _selfIndex;

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value)
                {
                    return;
                }

                _isOn = value;
                SetSelectedStatus(_isOn);
                _onValueChanged?.Invoke(_isOn);
                if (_isOn)
                {
                    _parentToggle?.SetSelectedIndex(_selfIndex);
                }
            }
        }

        private void Awake()
        {
            _itemButton = GetComponent<SuperButton>();
            SetAddListener();

            if (SelectedGo != null)
            {
                _selectedImage = SelectedGo.GetComponentInChildren<SuperImage>(true);
                _selectedText = SelectedGo.GetComponentInChildren<SuperText>(true);
            }

            if (UnSelectedGo != null)
            {
                _unSelectedImage = UnSelectedGo.GetComponentInChildren<SuperImage>(true);
                _unSelectedText = UnSelectedGo.GetComponentInChildren<SuperText>(true);
            }

            SetSelectedStatus(_isOn);
        }

        private void OnDestroy()
        {
            if (_itemButton != null)
            {
                _itemButton.onClick.RemoveListener(OnButtonClick);
            }
        }

        public void SetAddListener()
        {
            if (_itemButton == null)
            {
                return;
            }

            _itemButton.onClick.RemoveListener(OnButtonClick);
            _itemButton.onClick.AddListener(OnButtonClick);
        }

        public void SetOnValueChanged(Action<bool> onValueChanged)
        {
            _onValueChanged = onValueChanged;
        }

        public void SetParentCtrlNode(SuperToggle parentToggle, int index)
        {
            _parentToggle = parentToggle;
            SetIndex(index);
        }

        public void SetIndex(int index)
        {
            _selfIndex = index;
        }

        public void SetConditionCheck(Func<bool> conditionCheck)
        {
            _conditionCheck = conditionCheck ?? (() => true);
        }

        public void SetSeleIconAndText(Sprite icon, string text)
        {
            SetSeleIcon(icon);
            SetSeleText(text);
        }

        public void SetUnSeleIconAndText(Sprite icon, string text)
        {
            SetUnSeleIcon(icon);
            SetUnSeleText(text);
        }

        public void SetSeleIcon(Sprite icon)
        {
            if (_selectedImage != null)
            {
                _selectedImage.sprite = icon;
            }
        }

        public void SetSeleTextLanguage(string seleText)
        {
            _selectedText?.SetLanguage(seleText);
        }

        public void SetSeleText(string seleText)
        {
            if (_selectedText != null)
            {
                _selectedText.text = seleText;
            }
        }

        public void SetUnSeleIcon(Sprite icon)
        {
            if (_unSelectedImage != null)
            {
                _unSelectedImage.sprite = icon;
            }
        }

        public void SetUnSeleTextLanguage(string text)
        {
            _unSelectedText?.SetLanguage(text);
        }

        public void SetUnSeleText(string text)
        {
            if (_unSelectedText != null)
            {
                _unSelectedText.text = text;
            }
        }

        private void OnButtonClick()
        {
            if (_parentToggle == null || _selfIndex < 0 || !_conditionCheck())
            {
                return;
            }

            if (_isOn)
            {
                _parentToggle.InvokeOnToggleItemIsTrueClick();
                return;
            }

            IsOn = true;
            _parentToggle.OnItemClick(_selfIndex);
        }

        private void SetSelectedStatus(bool isSelected)
        {
            if (SelectedGo != null)
            {
                SelectedGo.SetActive(isSelected);
            }

            if (UnSelectedGo != null)
            {
                UnSelectedGo.SetActive(!isSelected);
            }
        }
    }
}
