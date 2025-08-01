using System;
using System.ComponentModel;
using GameLogic.Commands;
using GameLogic.Model;
using GameLogic.RedNote;
using GameLogic.ViewModel;
using UnityEngine;
using UnityEngine.UI;
using UnityFramework;

namespace GameLogic
{
    class BattleViewModel : ViewModelBase
    {
        private bool _firstRedNodeIsOn = false;
        private bool _secondRedNodeIsOn = false;
        
        private BattleModel _battleModel;

        public bool IsGameOver
        {
            get => _battleModel.IsGameOver;
            set
            {
                _battleModel.IsGameOver = value;
                RaisePropertyChanged(nameof(IsGameOver));
            }
        }

        private SimpleCommand _testRedNodeChild1Command;
        public ICommand TestRedNodeChild1Command => _testRedNodeChild1Command;
        private SimpleCommand _testRedNodeChild2Command;
        public ICommand TestRedNodeChild2Command => _testRedNodeChild2Command;
        public string Score
        {
            get => _battleModel.Score;
            set
            {
                _battleModel.Score = value;
                RaisePropertyChanged(nameof(Score));
            }
        }
        public string ScoreReddotPath
        {
            get => _battleModel.ScoreReddotPath;
            set
            {
                _battleModel.ScoreReddotPath = value;
                RaisePropertyChanged(nameof(ScoreReddotPath));
            }
        }
        public string TestRedNodeChild1Path
        {
            get => _battleModel.TestRedNodeChild1Path;
            set
            {
                _battleModel.TestRedNodeChild1Path = value;
                RaisePropertyChanged(nameof(TestRedNodeChild1Path));
            }
        }
        public string TestRedNodeChild2Path
        {
            get => _battleModel.TestRedNodeChild2Path;
            set
            {
                _battleModel.TestRedNodeChild2Path = value;
                RaisePropertyChanged(nameof(TestRedNodeChild2Path));
            }
        }
        public BattleViewModel()
        {
            _battleModel = new BattleModel();
            _battleModel.PropertyChanged += OnPropertyChanged;
            
            _testRedNodeChild1Command = new SimpleCommand(TestRedNodeChild1);
            _testRedNodeChild2Command = new SimpleCommand(TestRedNodeChild2);
        }
        
        private void TestRedNodeChild2()
        {
            _secondRedNodeIsOn = !_secondRedNodeIsOn;
            ReddotManager.SetRedDotData(_secondRedNodeIsOn, _battleModel.TestRedNodeChild2Path);
        }

        private void TestRedNodeChild1()
        {
            _firstRedNodeIsOn = !_firstRedNodeIsOn;
            ReddotManager.SetRedDotData(_firstRedNodeIsOn, _battleModel.TestRedNodeChild1Path);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsGameOver))
            {
                GameModule.UI.ShowUIAsync<GameOverTipWindow>();
            }
            RaisePropertyChanged(e.PropertyName);
        }

    }
}
