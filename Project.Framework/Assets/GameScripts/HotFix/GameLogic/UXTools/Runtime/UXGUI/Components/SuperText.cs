using System;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/SuperText", 11)]
    public class SuperText : UXText
    {
        private bool _originColorCached;
        private Color _originColor;

        public void SetLanguage(string key = "", string def = "", params object[] values)
        {
            if (!string.IsNullOrEmpty(key))
            {
                localizationID = key;
            }

            if (!ignoreLocalization && !string.IsNullOrEmpty(localizationID))
            {
                ChangeLanguage(LocalizationHelper.GetLanguage());
                return;
            }

            if (string.IsNullOrEmpty(def))
            {
                return;
            }

            text = values is { Length: > 0 } ? string.Format(def, values) : def;
        }

        public void SetGrey(bool isGrey)
        {
            if (!_originColorCached)
            {
                _originColor = color;
                _originColorCached = true;
            }

            color = isGrey ? Color.Lerp(_originColor, Color.gray, 0.7f) : _originColor;
        }

        public void SetColor(Color targetColor)
        {
            color = targetColor;
            _originColor = targetColor;
            _originColorCached = true;
        }

        public void SetOutLineColor(Color outlineColor)
        {
            var outline = GetComponent<UXOutline>();
            if (outline != null)
            {
                outline.effectColor = outlineColor;
            }
        }
    }
}
