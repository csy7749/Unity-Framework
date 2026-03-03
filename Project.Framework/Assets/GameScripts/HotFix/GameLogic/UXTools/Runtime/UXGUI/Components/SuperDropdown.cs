using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/SuperDropdown", 17)]
    public class SuperDropdown : Dropdown
    {
        [SerializeField] private RectTransform _arrowRect;

        public void SetDorpdownData(List<string> dataList, UnityAction<int> callBack, int defaultIndex = -1)
        {
            onValueChanged.RemoveAllListeners();
            ClearOptions();
            if (dataList == null || dataList.Count == 0)
            {
                return;
            }

            AddOptions(dataList);
            value = GetSafeDefaultIndex(defaultIndex, dataList.Count);
            RefreshShownValue();
            if (callBack != null)
            {
                onValueChanged.AddListener(callBack);
            }
        }

        public void SetDorpdownData(List<OptionData> dataList, UnityAction<int> callBack, int defaultIndex = -1)
        {
            onValueChanged.RemoveAllListeners();
            ClearOptions();
            if (dataList == null || dataList.Count == 0)
            {
                return;
            }

            AddOptions(dataList);
            value = GetSafeDefaultIndex(defaultIndex, dataList.Count);
            RefreshShownValue();
            if (callBack != null)
            {
                onValueChanged.AddListener(callBack);
            }
        }

        protected override GameObject CreateBlocker(Canvas rootCanvas)
        {
            if (_arrowRect != null)
            {
                _arrowRect.rotation = Quaternion.Euler(0, 0, 180);
            }

            return base.CreateBlocker(rootCanvas);
        }

        protected override void DestroyBlocker(GameObject blocker)
        {
            base.DestroyBlocker(blocker);
            if (_arrowRect != null)
            {
                _arrowRect.rotation = Quaternion.identity;
            }
        }

        private static int GetSafeDefaultIndex(int defaultIndex, int count)
        {
            return defaultIndex >= 0 && defaultIndex < count ? defaultIndex : 0;
        }
    }
}
