using System.Collections.Generic;
using GameLogic.Binding.Builder;
using GameLogic.RedNote;
using GameLogic.ViewModel;
using UnityEngine;
using UnityEngine.UI;
using UnityFramework;

namespace GameLogic
{
    [Window(UILayer.UI)]
    class BattleWindow : UIWindow
    {
        #region 脚本工具生成的代码
        private Text _vmTextScore;
        private Button _vmBtnTestRedNodeChild1;
        private Button _vmBtnTestRedNodeChild2;
        private BattleViewModel _battleViewModel;

        protected override void ScriptGenerator()
        {
            _vmTextScore = FindChildComponent<Text>("ScoreView/m_vmTextScore");
            _vmBtnTestRedNodeChild1 = FindChildComponent<Button>("ScoreView/m_vmBtnTestRedNoteChild1");
            _vmBtnTestRedNodeChild2 = FindChildComponent<Button>("ScoreView/m_vmBtnTestRedNoteChild2");
        }
        #endregion
        protected override void OnCreate()
        {
            base.OnCreate();
            _battleViewModel = new BattleViewModel();
            
            var scoreReddot = CreateWidgetByPath<Reddot>(_vmTextScore.transform,"RedNoteWidget");
            var testRedNodeChild1 = CreateWidgetByPath<Reddot>(_vmBtnTestRedNodeChild1.transform, "RedNoteWidget");
            var testRedNodeChild2 = CreateWidgetByPath<Reddot>(_vmBtnTestRedNodeChild2.transform, "RedNoteWidget");
            BindingSet<BattleWindow, BattleViewModel> bindingSet = this.CreateBindingSet(_battleViewModel);
            bindingSet.Bind(_vmTextScore).From(v => v.text).To(vm => vm.Score).TwoWay();
            bindingSet.Bind(_vmBtnTestRedNodeChild1).From(v => v.onClick).To(vm => vm.TestRedNodeChild1Command);
            bindingSet.Bind(_vmBtnTestRedNodeChild2).From(v => v.onClick).To(vm => vm.TestRedNodeChild2Command);
            bindingSet.Bind(scoreReddot).From(v => v.Path).To(vm => vm.ScoreReddotPath).TwoWay();
            bindingSet.Bind(testRedNodeChild1).From(v => v.Path).To(vm => vm.TestRedNodeChild1Path).TwoWay();
            bindingSet.Bind(testRedNodeChild2).From(v => v.Path).To(vm => vm.TestRedNodeChild2Path).TwoWay();
            bindingSet.Build();
            _battleViewModel.OnRedNodeChild1Clicked += (isOn) => ReddotManager.SetRedDotData(isOn, testRedNodeChild1.Path);
            _battleViewModel.OnRedNodeChild2Clicked += (isOn) => ReddotManager.SetRedDotData(isOn, testRedNodeChild2.Path);
        }
    }
}