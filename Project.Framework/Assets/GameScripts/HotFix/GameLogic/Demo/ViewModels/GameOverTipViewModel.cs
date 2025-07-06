using System;
using System.Collections;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using GameLogic.Commands;
using GameLogic.GoapModule.Demo;
using GameLogic.Model;
using GameLogic.OtherPackage;
using UnityEngine;
using UnityFramework;
using YooAsset;

namespace GameLogic.ViewModel
{
    public class GameOverTipViewModel : ViewModelBase
    {
        
        private GameOverTipModel _gameOverTipModel;

        private SimpleCommand _goHomeCommand;
        public ICommand GoHomeCommand => _goHomeCommand;
        private SimpleCommand _restartCommand;
        public ICommand RestartCommand => _restartCommand;
        public GameOverTipViewModel()
        {
            _restartCommand =  new SimpleCommand(UniTask.UnityAction(Restart));
            _goHomeCommand = new SimpleCommand(GoHome);
            _gameOverTipModel = new GameOverTipModel();
            _gameOverTipModel.PropertyChanged += OnPropertyChanged;
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        private async UniTaskVoid Restart()
        {
            await GameModule.Scene.LoadSceneAsync("scene_battle");
    
            BattleSystem.Instance.DestroyRoom();
            BattleSystem.Instance.LoadRoom().Forget();
            GameModule.UI.CloseUI<GameOverTipWindow>();
        }
        
        private void GoHome()
        {
            GameModule.Scene.LoadSceneAsync("scene_home");
            GameModule.Scene.UnloadAsync("scene_battle");
            GameModule.UI.CloseUI<GameOverTipWindow>();
            BattleSystem.Instance.DestroyRoom();
            HomeSystem.Instance.LoadHome().Forget();
        }
    }

}

