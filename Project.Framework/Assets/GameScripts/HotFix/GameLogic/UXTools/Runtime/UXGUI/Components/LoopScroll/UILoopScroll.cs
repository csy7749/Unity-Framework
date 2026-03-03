using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class UILoopScroll : MonoBehaviour
    {
        public GameObject Item;

        protected Action<GameObject, int> _onProvideData;
        protected Func<GameObject> _onGetFunc;
        protected readonly Dictionary<Transform, GameObject> _keyValuePairs = new Dictionary<Transform, GameObject>();

        private readonly List<Transform> _spawned = new List<Transform>();
        private ScrollRect _scrollRect;
        private Action _onReachTopAction;
        private Action _onReachBottomAction;
        private float _edgeThreshold = 1.02f;

        public int TotalCount { get; private set; }
        public RectTransform ContentRect => _scrollRect != null ? _scrollRect.content : null;
        public Vector2 normalizedPosition
        {
            get => _scrollRect != null ? _scrollRect.normalizedPosition : Vector2.zero;
            set
            {
                if (_scrollRect != null)
                {
                    _scrollRect.normalizedPosition = value;
                }
            }
        }

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(OnValueChangedEvent);
            }
        }

        private void OnDestroy()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.RemoveListener(OnValueChangedEvent);
            }
        }

        public void InitListView(Action<GameObject, int> onProvideData, Func<GameObject> onGetFunc, object ownerUI = null, object uiCtrl = null, string groupName = "")
        {
            _onProvideData = onProvideData;
            _onGetFunc = onGetFunc;
        }

        public void SetTotalCount(int totalCount, int startItem = 0, float contentOffset = 0)
        {
            TotalCount = Mathf.Max(0, totalCount);
            RefillCells(startItem, contentOffset);
        }

        public void SetTotalCountFromEnd(int totalCount, int startItem = 0, bool alignStart = false)
        {
            TotalCount = Mathf.Max(0, totalCount);
            RefillCells(startItem);
            ScrollToBottom();
        }

        public void SetHorizontal(bool isHorizontal)
        {
            if (_scrollRect != null)
            {
                _scrollRect.horizontal = isHorizontal;
            }
        }

        public void SetVertical(bool isVertical)
        {
            if (_scrollRect != null)
            {
                _scrollRect.vertical = isVertical;
            }
        }

        public void SetScrollRectDirection(bool isVertical, bool isHorizontal)
        {
            SetVertical(isVertical);
            SetHorizontal(isHorizontal);
        }

        public void SetTotalCountAndRefreshCells(int totalCount)
        {
            TotalCount = Mathf.Max(0, totalCount);
            RefreshCells();
        }

        public void RefreshCells()
        {
            EnsureItemCount(TotalCount);
            for (var i = 0; i < TotalCount; i++)
            {
                var trans = _spawned[i];
                trans.gameObject.SetActive(true);
                ProvideData(trans, i);
            }
        }

        public void RefillCells(int startItem = 0, float contentOffset = 0)
        {
            RefreshCells();
            ScrollToCell(startItem);
        }

        public void SetTopAndBottom(Action topAction, Action bottomAction, float edgeThreshold = 1.02f)
        {
            _onReachTopAction = topAction;
            _onReachBottomAction = bottomAction;
            _edgeThreshold = edgeThreshold;
        }

        public void SetTop(Action topAction, float edgeThreshold = 1.02f)
        {
            _onReachTopAction = topAction;
            _edgeThreshold = edgeThreshold;
        }

        public void SetBottom(Action bottomAction, float edgeThreshold = 1.02f)
        {
            _onReachBottomAction = bottomAction;
            _edgeThreshold = edgeThreshold;
        }

        public GameObject GetObject(int index)
        {
            if (Item == null || _scrollRect == null || _scrollRect.content == null)
            {
                return null;
            }

            var go = Instantiate(Item, _scrollRect.content);
            go.transform.localScale = Item.transform.localScale;
            go.transform.localRotation = Item.transform.localRotation;
            go.SetActive(true);

            _keyValuePairs[go.transform] = _onGetFunc?.Invoke() ?? go;

            _spawned.Add(go.transform);
            return go;
        }

        public virtual void ProvideData(Transform transform, int idx)
        {
            if (transform == null)
            {
                return;
            }

            if (_keyValuePairs.TryGetValue(transform, out var widget))
            {
                widget.SetActive(true);
                _onProvideData?.Invoke(widget, idx);
            }
            else
            {
                _onProvideData?.Invoke(null, idx);
            }
        }

        public void ReturnObject(Transform trans)
        {
            if (trans == null)
            {
                return;
            }

            _keyValuePairs.Remove(trans);
            _spawned.Remove(trans);
            Destroy(trans.gameObject);
        }

        public void Clear()
        {
            for (var i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                {
                    Destroy(_spawned[i].gameObject);
                }
            }

            _spawned.Clear();
            _keyValuePairs.Clear();
            TotalCount = 0;
        }

        public void ScrollToCell(int index, float speed = 1)
        {
            if (_scrollRect == null || TotalCount <= 1)
            {
                return;
            }

            var validIndex = Mathf.Clamp(index, 0, TotalCount - 1);
            var normalized = 1f - (float)validIndex / (TotalCount - 1);
            if (_scrollRect.vertical)
            {
                _scrollRect.verticalNormalizedPosition = normalized;
            }
            else if (_scrollRect.horizontal)
            {
                _scrollRect.horizontalNormalizedPosition = 1f - normalized;
            }
        }

        public void ScrollToBottom(float speed = 1)
        {
            if (_scrollRect == null)
            {
                return;
            }

            if (_scrollRect.vertical)
            {
                _scrollRect.verticalNormalizedPosition = 0f;
            }
            else if (_scrollRect.horizontal)
            {
                _scrollRect.horizontalNormalizedPosition = 1f;
            }
        }

        private void EnsureItemCount(int count)
        {
            for (var i = _spawned.Count; i < count; i++)
            {
                GetObject(i);
            }

            for (var i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                {
                    _spawned[i].gameObject.SetActive(i < count);
                }
            }
        }

        private void OnValueChangedEvent(Vector2 value)
        {
            if (_scrollRect == null)
            {
                return;
            }

            if (_scrollRect.horizontal)
            {
                if (_edgeThreshold < value.x)
                {
                    _onReachBottomAction?.Invoke();
                }
                else if (value.x < 1f - _edgeThreshold)
                {
                    _onReachTopAction?.Invoke();
                }
                return;
            }

            if (_edgeThreshold < value.y)
            {
                _onReachBottomAction?.Invoke();
            }
            else if (value.y < 1f - _edgeThreshold)
            {
                _onReachTopAction?.Invoke();
            }
        }
    }
}
