using UnityEngine;
using UnityEngine.UI;
using UnityFramework;

namespace GameLogic
{
    public class Test : MonoBehaviour
    {
        public Image image;
        private const string TestImageUrl = "https://jrxtest.hanzhigame.com/Test/xx_bg_dikuang1.png";

        // Start is called before the first frame update
        void Start()
        {
            Utility.Http.GetTexture(TestImageUrl, OnTextureLoaded);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnTextureLoaded(Texture2D texture)
        {
            if (texture == null)
            {
                Debug.LogError($"[Test] HTTPS texture request failed: {TestImageUrl}");
                return;
            }

            if (image == null)
            {
                Debug.LogError("[Test] Target Image is null, please assign it in Inspector.");
                return;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            image.sprite = sprite;
            image.SetNativeSize();

            Debug.Log($"[Test] HTTPS texture request success and applied to Image: {TestImageUrl}, size={texture.width}x{texture.height}");
        }
    }
}
