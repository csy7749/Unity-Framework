using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/SuperButton", 10)]
    public class SuperButton : Button
    {
        [SerializeField] public SuperImage Image;
        [SerializeField] public SuperText Text;
        [SerializeField] public SuperImage OutLineImage;

        protected override void Awake()
        {
            base.Awake();
            EnsureReferences();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            EnsureReferences();
        }

        public void SetSprite(string spriteName)
        {
            EnsureReferences();
            if (Image != null)
            {
                Image.SetSprite(spriteName);
            }
        }

        public void SetText(string content)
        {
            EnsureReferences();
            if (Text != null)
            {
                Text.text = content;
            }
        }

        public void SetLocationText(string key, params object[] values)
        {
            EnsureReferences();
            if (Text != null)
            {
                Text.SetLanguage(key, key, values);
            }
        }

        public void SetGrey(bool isGrey)
        {
            EnsureReferences();
            if (Image != null)
            {
                Image.SetGrey(isGrey);
            }

            if (Text != null)
            {
                Text.SetGrey(isGrey);
            }
        }

        public void SetOutLine(int limit = -1)
        {
            EnsureReferences();
            if (OutLineImage != null)
            {
                OutLineImage.SetOutLine(limit);
                return;
            }

            if (Image != null)
            {
                Image.SetOutLine(limit);
            }
        }

        public void HideOutLine()
        {
            if (OutLineImage != null)
            {
                OutLineImage.HideOutLine();
            }
        }

        public void DisableOutLine()
        {
            HideOutLine();
        }

        private void EnsureReferences()
        {
            if (Image == null)
            {
                Image = GetComponentInChildren<SuperImage>(true);
            }

            if (Text == null)
            {
                Text = GetComponentInChildren<SuperText>(true);
            }
        }
    }
}
