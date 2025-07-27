using GameLogic.Binding.Builder;
using UnityEngine;
using UnityEngine.UI;
using UnityFramework;

namespace GameLogic
{
    [Window(UILayer.UI)]
    class HomeWindow : UIWindow
    {
        #region 脚本工具生成的代码
        private Button _vmBtnGoAircraftBattleDemo;
        private Button _vmBtnGoTurnBaseDemo;
        private Button _vmBtnGoRpgAbilityDemo;
        private Button _vmBtnGoRpgCharacterDemo;
        private Button _vmBtnGoXGDemo;
        private HomeViewModel _homeViewModel;

        protected override void ScriptGenerator()
        {
            _vmBtnGoAircraftBattleDemo = FindChildComponent<Button>("m_vmBtnGoAircraftBattleDemo");
            _vmBtnGoTurnBaseDemo = FindChildComponent<Button>("m_vmBtnGoTurnBaseDemo");
            _vmBtnGoRpgAbilityDemo = FindChildComponent<Button>("m_vmBtnGoRpgAbilityDemo");
            _vmBtnGoRpgCharacterDemo = FindChildComponent<Button>("m_vmBtnGoRpgCharacterDemo");
            _vmBtnGoXGDemo = FindChildComponent<Button>("m_vmBtnGoXGDemo");
        }
        #endregion
        protected override void OnCreate()
        {
            base.OnCreate();
            _homeViewModel = new HomeViewModel();
            BindingSet<HomeWindow, HomeViewModel> bindingSet = this.CreateBindingSet(_homeViewModel);
            bindingSet.Bind(_vmBtnGoAircraftBattleDemo).From(v => v.onClick).To(vm => vm.GoAircraftBattleDemoCommand);
            bindingSet.Bind(_vmBtnGoTurnBaseDemo).From(v => v.onClick).To(vm => vm.GoTurnBaseDemoCommand);
            bindingSet.Bind(_vmBtnGoRpgAbilityDemo).From(v => v.onClick).To(vm => vm.GoRpgAbilityDemoCommand);
            bindingSet.Bind(_vmBtnGoRpgCharacterDemo).From(v => v.onClick).To(vm => vm.GoRpgCharacterDemoCommand);
            bindingSet.Bind(_vmBtnGoXGDemo).From(v => v.onClick).To(vm => vm.GoXGDemoCommand);
            bindingSet.Build();
        }


    }
}