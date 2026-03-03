using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class UILoopScrollMul : MonoBehaviour
    {
        public List<GameObject> Items = new List<GameObject>();

        protected Action<GameObject, int> _onProvideData;
        protected Func<int, GameObject> _onGetFunc;
        protected Func<int, int> _onGetItemIdx;
        protected readonly Dictionary<Transform, GameObject> _keyValuePairs = new Dictionary<Transform, GameObject>();

        private readonly List<Transform> _spawned = new List<Transform>();
        private readonly List<int> _spawnedPrefabIndex = new List<int>();
        private ScrollRect _scrollRect;
        private Action _onReachTopAction;
        private Action _onReachBottomAction;
        private float _edgeThreshold = 1.02f;

        public int TotalCount { get; private set; }

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

        public void InitListView(Action<GameObject, int> onProvideData, Func<int, GameObject> onGetFunc, Func<int, int> onGetItemProvider, object ownerUI = null, object uiCtrl = null, string groupName = "")
        {
            _onProvideData = onProvideData;
            _onGetFunc = onGetFunc;
            _onGetItemIdx = onGetItemProvider;
        }

        public void SetTotalCount(int totalCount)
        {
            TotalCount = Mathf.Max(0, totalCount);
            RefillCells();
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

        public void RefillCells()
        {
            RefreshCells();
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
            var prefabIndex = GetPrefabIndex(index);
            if (_scrollRect == null || _scrollRect.content == null || prefabIndex < 0 || prefabIndex >= Items.Count || Items[prefabIndex] == null)
            {
                return null;
            }

            var prefab = Items[prefabIndex];
            var go = Instantiate(prefab, _scrollRect.content);
            go.transform.localScale = prefab.transform.localScale;
            go.transform.localRotation = prefab.transform.localRotation;
            go.SetActive(true);

            _keyValuePairs[go.transform] = _onGetFunc?.Invoke(prefabIndex) ?? go;

            _spawned.Add(go.transform);
            _spawnedPrefabIndex.Add(prefabIndex);
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

            var index = _spawned.IndexOf(trans);
            if (index >= 0)
            {
                _spawned.RemoveAt(index);
                _spawnedPrefabIndex.RemoveAt(index);
            }

            _keyValuePairs.Remove(trans);
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
            _spawnedPrefabIndex.Clear();
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

        private int GetPrefabIndex(int dataIndex)
        {
            if (_onGetItemIdx != null)
            {
                return Mathf.Clamp(_onGetItemIdx(dataIndex), 0, Mathf.Max(0, Items.Count - 1));
            }

            return Items.Count == 0 ? -1 : Mathf.Clamp(dataIndex, 0, Items.Count - 1);
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
