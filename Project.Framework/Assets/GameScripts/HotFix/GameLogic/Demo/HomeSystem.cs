using Cysharp.Threading.Tasks;
using UnityFramework;

namespace GameLogic
{
    public class HomeSystem : Singleton<HomeSystem>
    {
        public async UniTaskVoid LoadHome()
        {
            await UniTask.Yield();
            GameModule.UI.ShowUIAsync<HomeWindow>();
        }

        public void DestroyHome()
        {
            GameModule.UI.CloseUI<HomeWindow>();
        }
    }
}