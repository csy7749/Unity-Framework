using GameLogic.Observables;
using UnityFramework;

namespace GameLogic.Model
{
    public class BattleModel : ObservableObject
    {
        private bool _isGameOver;
        private string _score;
        private string _scoreReddotPath;
        private string _testRedNodeChild1Path;
        private string _testRedNodeChild2Path;

        public bool IsGameOver
        {
            get => _isGameOver;
            set=>Set(ref _isGameOver,value);
        }
        public string Score
        {
            get => _score;
            set => Set(ref _score, value);
        }
        public string ScoreReddotPath
        {
            get => _scoreReddotPath;
            set => Set(ref _scoreReddotPath, value);
        }
        public string TestRedNodeChild1Path
        {
            get => _testRedNodeChild1Path;
            set => Set(ref _testRedNodeChild1Path, value);
        }
        public string TestRedNodeChild2Path
        {
            get => _testRedNodeChild2Path;
            set => Set(ref _testRedNodeChild2Path, value);
        }

        public BattleModel()
        {
            _scoreReddotPath = "1";
            _testRedNodeChild1Path = "1/2";
            _testRedNodeChild2Path = "1/3";
            GameEvent.AddEventListener<int>(ActorEventDefine.ScoreChange, OnScoreChange);
            GameEvent.AddEventListener(ActorEventDefine.GameOver, OnGameOver);
        }

        private void OnGameOver()
        {
            IsGameOver = true;
        }

        private void OnScoreChange(int obj)
        {
            Score = obj.ToString();
        }
    }
}