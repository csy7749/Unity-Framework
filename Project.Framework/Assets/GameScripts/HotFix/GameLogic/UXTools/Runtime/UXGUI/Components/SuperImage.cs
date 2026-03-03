using System;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/SuperImage", 12)]
    public class SuperImage : UXImage
    {
        [SerializeField] private UXOutline _outline;

        public void SetSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                sprite = null;
                return;
            }

            sprite = ResourceManager.Load<Sprite>(spriteName);
        }

        public void SetSprite(string spriteName, Action callback, bool nativeSize = false, string group = "common")
        {
            SetSprite(spriteName);
            if (nativeSize && sprite != null)
            {
                SetNativeSize();
            }

            callback?.Invoke();
        }

        public void SetOverrideSprite(string spriteName, Action callback, bool nativeSize = false, string group = "common")
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                overrideSprite = null;
            }
            else
            {
                overrideSprite = ResourceManager.Load<Sprite>(spriteName);
            }

            if (nativeSize && overrideSprite != null)
            {
                SetNativeSize();
            }

            callback?.Invoke();
        }

        public void SetOutLine(int limit = -1)
        {
            if (_outline == null)
            {
                _outline = GetComponent<UXOutline>();
            }

            if (_outline == null)
            {
                _outline = gameObject.AddComponent<UXOutline>();
            }

            _outline.enabled = limit != 0;
            if (limit > 0)
            {
                _outline.effectWidth = limit;
            }
        }

        public void HideOutLine()
        {
            if (_outline != null)
            {
                _outline.enabled = false;
            }
        }

        public void DisableOutLine()
        {
            HideOutLine();
        }
    }
}
