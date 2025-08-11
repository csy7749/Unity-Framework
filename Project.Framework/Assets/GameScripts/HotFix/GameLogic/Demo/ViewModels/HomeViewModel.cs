using System.ComponentModel;
using Cysharp.Threading.Tasks;
using GameLogic.Commands;
using GameLogic.ViewModel;
using UnityEngine;
using UnityEngine.UI;
using UnityFramework;

namespace GameLogic
{
    class HomeViewModel : ViewModelBase
    {
        private SimpleCommand _goAircraftBattleDemoCommand;
        public ICommand GoAircraftBattleDemoCommand => _goAircraftBattleDemoCommand;
        private SimpleCommand _goTurnBaseDemoCommand;
        public ICommand GoTurnBaseDemoCommand => _goTurnBaseDemoCommand;
        private SimpleCommand _goRpgAbilityDemoCommand;
        public ICommand GoRpgAbilityDemoCommand => _goRpgAbilityDemoCommand;
        private SimpleCommand _goRpgCharacterDemoCommand;
        public ICommand GoRpgCharacterDemoCommand => _goRpgCharacterDemoCommand;
        private SimpleCommand _goXGDemoCommand;
        public ICommand GoXGDemoCommand => _goXGDemoCommand;
        public HomeViewModel()
        {
            _goAircraftBattleDemoCommand = new SimpleCommand(UniTask.UnityAction(GoAircraftBattleDemo));
            _goTurnBaseDemoCommand = new SimpleCommand(GoTurnBaseDemo);
            _goRpgAbilityDemoCommand = new SimpleCommand(GoRpgAbilityDemo);
            _goRpgCharacterDemoCommand = new SimpleCommand(GoRpgCharacterDemo);
            _goXGDemoCommand = new SimpleCommand(GoXGDemo);
        }

        private async UniTaskVoid GoAircraftBattleDemo()
        {
            await GameModule.Scene.LoadSceneAsync("scene_battle");
            GameModule.Scene.IsMainScene("scene_battle");
            
            
            
            BattleSystem.Instance.LoadRoom().Forget();
            GameModule.Scene.UnloadAsync("scene_home").Forget();
            GameModule.UI.CloseUI<GameOverTipWindow>();
            HomeSystem.Instance.DestroyHome();
        }
        
        private void GoTurnBaseDemo()
        {
            GameModule.Scene.LoadSceneAsync("scene_turn-base").Forget();
            GameModule.Scene.UnloadAsync("scene_home").Forget();
            GameModule.UI.CloseUI<GameOverTipWindow>();
            HomeSystem.Instance.DestroyHome();
        }
        
        private void GoRpgAbilityDemo()
        {
            GameModule.Scene.LoadSceneAsync("scene_rpg").Forget();
            GameModule.Scene.UnloadAsync("scene_home").Forget();
            GameModule.UI.CloseUI<GameOverTipWindow>();
            HomeSystem.Instance.DestroyHome();
        }
        
        private void GoRpgCharacterDemo()
        {
            GameModule.Scene.LoadSceneAsync("scene_rpgCC").Forget();
            GameModule.Scene.UnloadAsync("scene_home").Forget();
            GameModule.UI.CloseUI<GameOverTipWindow>();
            HomeSystem.Instance.DestroyHome();
        }
        
        private void GoXGDemo()
        {
            GameModule.Scene.LoadSceneAsync("scene_battle_xiaogong").Forget();
            GameModule.Scene.UnloadAsync("scene_home").Forget();
            GameModule.UI.CloseUI<GameOverTipWindow>();
            HomeSystem.Instance.DestroyHome();
        }
        
        

        // private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        // {
        //     RaisePropertyChanged(e.PropertyName);
        // }
    }
}