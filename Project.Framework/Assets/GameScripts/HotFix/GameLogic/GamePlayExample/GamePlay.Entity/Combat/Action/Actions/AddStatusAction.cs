using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Combat
{
    public class AddStatusActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public bool Enable { get; set; }


        public bool TryMakeAction(out AddStatusAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<AddStatusAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// 施加状态行动
    /// </summary>
    public class AddStatusAction : Entity, IActionExecute
    {
        public Entity SourceAbility { get; set; }
        public AddStatusEffect AddStatusEffect => SourceAssignAction.AbilityEffect.EffectConfig as AddStatusEffect;
        public Ability BuffAbility { get; set; }

        /// 行动能力
        public Entity ActionAbility { get; set; }
        /// 效果赋给行动源
        public EffectAssignAction SourceAssignAction { get; set; }
        /// 行动实体
        public CombatEntity Creator { get; set; }
        /// 目标对象
        public Entity Target { get; set; }


        public void FinishAction()
        {
            Entity.Destroy(this);
        }

        //前置处理
        private void PreProcess()
        {

        }

        public void ApplyAddStatus()
        {
            PreProcess();

//#if EGAMEPLAY_EXCEL
//            var statusConfig = AddStatusEffect.AddStatusConfig;
//            var canStack = statusConfig.CanStack == "是";
//            var enabledLogicTrigger = statusConfig.EnabledLogicTrigger();
//#else
            var buffObject = AddStatusEffect.AddStatus;
            if (buffObject == null)
            {
                var statusId = AddStatusEffect.AddStatusId;
                buffObject = GameModule.Resource.LoadAsset<AbilityConfigObject>($"Buff_{statusId}");
            }
            var buffConfig = AbilityConfigCategory.Instance.Get(buffObject.Id);
            var canStack = buffConfig.CanStack == "是";
//#endif
            var statusComp = Target.GetComponent<StatusComponent>();
            if (canStack == false)
            {
                if (statusComp.HasStatus(buffConfig.KeyName))
                {
                    var status = statusComp.GetStatus(buffConfig.KeyName);
                    var lifeComp = status.GetComponent<AbilityLifeTimeComponent>();
                    if (lifeComp != null)
                    {
                        var statusLifeTimer = lifeComp.LifeTimer;
                        statusLifeTimer.MaxTime = AddStatusEffect.Duration;
                        statusLifeTimer.Reset();
                    }
                    FinishAction();
                    return;
                }
            }

            BuffAbility = statusComp.AttachStatus(buffObject);
            BuffAbility.OwnerEntity = Creator;
            BuffAbility.GetComponent<AbilityLevelComponent>().Level = SourceAbility.GetComponent<AbilityLevelComponent>().Level;
            //Log.Debug($"ApplyAddStatus BuffAbility={BuffAbility.Config.KeyName}");
            ProcessInputKVParams(BuffAbility, AddStatusEffect.Params);

            if (AddStatusEffect.Duration > 0)
            {
                BuffAbility.AddComponent<AbilityLifeTimeComponent>(AddStatusEffect.Duration);
            }
            BuffAbility.TryActivateAbility();

            PostProcess();

            FinishAction();
        }

        //后置处理
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveStatus, this);
            Target.GetComponent<ActionPointComponent>().TriggerActionPoint(ActionPointType.PostReceiveStatus, this);
        }

        /// 这里处理技能传入的参数数值替换
        public void ProcessInputKVParams(Ability ability, Dictionary<string, string> Params)
        {
            foreach (var abilityTrigger in ability.GetComponent<AbilityTriggerComponent>().AbilityTriggers)
            {
                var effect = abilityTrigger.TriggerConfig;

                if (!string.IsNullOrEmpty(effect.ConditionParam))
                {
                    abilityTrigger.ConditionParamValue = ProcessReplaceKV(effect.ConditionParam, Params);
                }
            }

            foreach (var abilityEffect in ability.GetComponent<AbilityEffectComponent>().AbilityEffects)
            {
                var effect = abilityEffect.EffectConfig;

                if (effect is AttributeModifyEffect attributeModify && abilityEffect.TryGet(out EffectAttributeModifyComponent attributeModifyComponent))
                {
                    attributeModifyComponent.ModifyValueFormula = ProcessReplaceKV(attributeModify.NumericValue, Params);
                }
                if (effect is DamageEffect damage && abilityEffect.TryGet(out EffectDamageComponent damageComponent))
                {
                    damageComponent.DamageValueFormula = ProcessReplaceKV(damage.DamageValueFormula, Params);
                }
                if (effect is CureEffect cure && abilityEffect.TryGet(out EffectCureComponent cureComponent))
                {
                    cureComponent.CureValueProperty = ProcessReplaceKV(cure.CureValueFormula, Params);
                }
            }
        }

        private string ProcessReplaceKV(string originValue, Dictionary<string, string> Params)
        {
            foreach (var aInputKVItem in Params)
            {
                if (!string.IsNullOrEmpty(originValue))
                {
                    originValue = originValue.Replace(aInputKVItem.Key, aInputKVItem.Value);
                }
            }
            return originValue;
        }
    }
}