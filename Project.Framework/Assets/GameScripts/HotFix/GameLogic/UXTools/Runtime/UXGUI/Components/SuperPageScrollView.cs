using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class SuperPageScrollView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum Mode
        {
            Horizontal,
            Vertical
        }

        public GameObject Template;

        [SerializeField] private Mode touchMode = Mode.Horizontal;
        [SerializeField] private RectTransform viewport;

        public Action<GameObject, bool> OnFocusCell;
        public Action<int, GameObject> OnCellValid;

        private readonly List<GameObject> _cells = new List<GameObject>();
        private Func<GameObject> _onGetFunc;
        private int _cellCount;
        private int _currentIndex = -1;
        private float _dragOffset;

        public void InitList(Action<int, GameObject> onCellValid, Action<GameObject, bool> onFocusCell, Func<GameObject> onGetFunc, object ownerUI = null, string groupName = "")
        {
            OnCellValid = onCellValid;
            OnFocusCell = onFocusCell;
            _onGetFunc = onGetFunc;

            if (Template != null)
            {
                Template.SetActive(false);
            }
        }

        public void LoadData(int len, int normalIdx, bool bAnimation = false, bool bImmediately = false)
        {
            _cellCount = Mathf.Max(0, len);
            EnsureCellCount(_cellCount);
            for (var i = 0; i < _cellCount; i++)
            {
                OnCellValid?.Invoke(i, _cells[i]);
            }

            BringUp(normalIdx, bAnimation, bImmediately);
        }

        public bool IsOnTop(GameObject check)
        {
            if (_currentIndex < 0 || _currentIndex >= _cells.Count)
            {
                return false;
            }

            return _cells[_currentIndex] == check;
        }

        public void BringUp(int idx, bool bAnimation = false, bool bImmediately = false)
        {
            if (_cellCount <= 0)
            {
                _currentIndex = -1;
                return;
            }

            var nextIndex = Mathf.Clamp(idx, 0, _cellCount - 1);
            if (_currentIndex == nextIndex)
            {
                return;
            }

            if (_currentIndex >= 0 && _currentIndex < _cells.Count)
            {
                OnFocusCell?.Invoke(_cells[_currentIndex], false);
            }

            _currentIndex = nextIndex;
            if (_currentIndex < _cells.Count)
            {
                OnFocusCell?.Invoke(_cells[_currentIndex], true);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragOffset = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _dragOffset += touchMode == Mode.Horizontal ? eventData.delta.x : eventData.delta.y;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            const float switchThreshold = 20f;
            if (Mathf.Abs(_dragOffset) < switchThreshold || _cellCount <= 1)
            {
                return;
            }

            var next = _dragOffset < 0 ? _currentIndex + 1 : _currentIndex - 1;
            BringUp(next);
        }

        public void Clear()
        {
            for (var i = 0; i < _cells.Count; i++)
            {
                if (_cells[i] != null)
                {
                    Destroy(_cells[i]);
                }
            }

            _cells.Clear();
            _cellCount = 0;
            _currentIndex = -1;
        }

        private void EnsureCellCount(int count)
        {
            if (Template == null)
            {
                return;
            }

            var parent = viewport != null ? viewport : Template.transform.parent as RectTransform;
            while (_cells.Count < count)
            {
                var generatedCell = _onGetFunc?.Invoke();
                if (generatedCell == null)
                {
                    generatedCell = Instantiate(Template, parent);
                    generatedCell.transform.localScale = Template.transform.localScale;
                    generatedCell.transform.localRotation = Template.transform.localRotation;
                }
                else
                {
                    generatedCell.transform.SetParent(parent, false);
                }

                generatedCell.SetActive(true);
                _cells.Add(generatedCell);
            }

            for (var i = 0; i < _cells.Count; i++)
            {
                _cells[i].SetActive(i < count);
            }
        }
    }
}
