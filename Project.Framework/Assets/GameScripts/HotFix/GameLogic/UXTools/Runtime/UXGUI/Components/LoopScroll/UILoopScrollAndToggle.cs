using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class UILoopScrollAndToggle : UILoopScroll
    {
        public SuperToggle superToggle;

        private readonly Dictionary<Transform, SuperToggleItem> _toggleItemMap = new Dictionary<Transform, SuperToggleItem>();

        public override void ProvideData(Transform transform, int idx)
        {
            base.ProvideData(transform, idx);
            if (transform == null || superToggle == null)
            {
                return;
            }

            if (!_toggleItemMap.TryGetValue(transform, out var toggleItem))
            {
                toggleItem = transform.GetComponent<SuperToggleItem>();
                if (toggleItem == null)
                {
                    return;
                }

                _toggleItemMap[transform] = toggleItem;
                superToggle.AddItem(toggleItem);
            }

            toggleItem.SetParentCtrlNode(superToggle, idx);
            toggleItem.SetIndex(idx);
            toggleItem.IsOn = idx == superToggle.GetSelectedIndex();
        }
    }
}
