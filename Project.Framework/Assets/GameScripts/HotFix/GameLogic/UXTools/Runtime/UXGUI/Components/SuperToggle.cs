using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public enum SuperToggleSelectMode
    {
        SingleSelect = 0,
        MultiSelect = 1,
    }

    public class SuperToggle : MonoBehaviour
    {
        [SerializeField] private List<SuperToggleItem> _items = new List<SuperToggleItem>();
        [SerializeField] private SuperToggleItem _itemPrefab;

        public string itemPrefabName;
        public string itemClassName;

        private readonly List<SuperToggleItem> _runtimeItems = new List<SuperToggleItem>();
        private Action<GameObject, int> _onProvideData;
        private Func<GameObject> _onGetFunc;

        private int _selectedIndex;
        private OnExToggleItemClick _onItemClick;
        private OnExToggleItemClick _onItemIsTrueClick;

        public delegate void OnExToggleItemClick(int index);

        public int Count => _items.Count;
        public SuperToggleItem this[int index] => index >= 0 && index < _items.Count ? _items[index] : null;

        private void OnEnable()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i]?.SetParentCtrlNode(this, i);
            }
        }

        public void SetOnToggleItemClick(OnExToggleItemClick onExToggleItemClick)
        {
            _onItemClick = onExToggleItemClick;
        }

        public void SetOnToggleItemIsTrueClick(OnExToggleItemClick onExToggleItemClick)
        {
            _onItemIsTrueClick = onExToggleItemClick;
        }

        public void InvokeOnToggleItemIsTrueClick()
        {
            _onItemIsTrueClick?.Invoke(_selectedIndex);
        }

        public void OnItemClick(int index)
        {
            _onItemClick?.Invoke(index);
        }

        public void SetSelectedIndexAndCallBack(int index)
        {
            SetSelectedIndex(index);
            OnItemClick(_selectedIndex);
        }

        public void SetSelectedIndex(int index)
        {
            if (_items.Count == 0)
            {
                _selectedIndex = -1;
                return;
            }

            _selectedIndex = Mathf.Clamp(index, 0, _items.Count - 1);
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null)
                {
                    _items[i].IsOn = i == _selectedIndex;
                }
            }
        }

        public void SetAllItemSelectedStatus(bool isSelected)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null)
                {
                    _items[i].IsOn = isSelected;
                }
            }
        }

        public int GetSelectedIndex()
        {
            return _selectedIndex;
        }

        public SuperToggleItem GetCurSuperToggleItem()
        {
            return this[_selectedIndex];
        }

        public List<int> GetAllSelectedIndex()
        {
            var result = new List<int>();
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null && _items[i].IsOn)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        public void SetToggleItemCount(int count, int index = 0, bool isAsync = false, string assetGroupName = "")
        {
            if (_itemPrefab == null)
            {
                RebuildFromSceneItems(count, index);
                return;
            }

            Clear();
            for (var i = 0; i < count; i++)
            {
                var item = Instantiate(_itemPrefab, transform);
                item.gameObject.SetActive(true);
                _runtimeItems.Add(item);
                AddItem(item);
            }

            SetSelectedIndexAndCallBack(index);
        }

        public void InitWidget(Action<GameObject, int> onProvideData, Func<GameObject> onGetFunc, object ownerUI = null, object uiCtrl = null, string groupName = "")
        {
            _onProvideData = onProvideData;
            _onGetFunc = onGetFunc;
        }

        public void SetToggleWidgetCount(int count, int index = 0, bool isAsync = false)
        {
            SetToggleItemCount(count, index, isAsync);
            if (_onProvideData == null || _onGetFunc == null)
            {
                return;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                _onProvideData(_onGetFunc(), i);
            }
        }

        public void AddItem(SuperToggleItem item)
        {
            if (item == null || _items.Contains(item))
            {
                return;
            }

            item.SetParentCtrlNode(this, _items.Count);
            _items.Add(item);
        }

        public void SetAllItem(List<SuperToggleItem> exItems)
        {
            _items.Clear();
            AddItem(exItems);
        }

        public void AddItem(List<SuperToggleItem> exItems)
        {
            if (exItems == null)
            {
                return;
            }

            for (var i = 0; i < exItems.Count; i++)
            {
                AddItem(exItems[i]);
            }
        }

        public void AddItem(SuperToggleItem[] exItems)
        {
            if (exItems == null)
            {
                return;
            }

            for (var i = 0; i < exItems.Length; i++)
            {
                AddItem(exItems[i]);
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _runtimeItems.Count; i++)
            {
                if (_runtimeItems[i] != null)
                {
                    Destroy(_runtimeItems[i].gameObject);
                }
            }

            _runtimeItems.Clear();
            _items.Clear();
            _selectedIndex = -1;
        }

        private void RebuildFromSceneItems(int count, int index)
        {
            _items.Clear();
            var sceneItems = GetComponentsInChildren<SuperToggleItem>(true);
            for (var i = 0; i < sceneItems.Length; i++)
            {
                AddItem(sceneItems[i]);
            }

            if (count < _items.Count)
            {
                for (var i = count; i < _items.Count; i++)
                {
                    if (_items[i] != null)
                    {
                        _items[i].gameObject.SetActive(false);
                    }
                }
            }

            SetSelectedIndexAndCallBack(index);
        }
    }
}
