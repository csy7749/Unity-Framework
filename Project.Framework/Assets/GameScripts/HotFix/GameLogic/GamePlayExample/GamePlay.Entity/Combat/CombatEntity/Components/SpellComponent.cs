using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;
using UnityFramework;


namespace GameLogic.Combat
{
    /// <summary>
    /// 技能施法组件
    /// </summary>
    public class SpellComponent : GameLogic.Component
    {
        private CombatEntity CombatEntity => GetEntity<CombatEntity>();
        public override bool DefaultEnable { get; set; } = true;
        public Dictionary<int, ExecutionObject> ExecutionObjects = new Dictionary<int, ExecutionObject>();


        public override void Awake()
        {

        }

        public void LoadExecutionObjects()
        {
            foreach (var item in CombatEntity.GetComponent<SkillComponent>().IdSkills)
            {
                var executionObj = GameModule.Resource.LoadAsset<ExecutionObject>($"Execution_{item.Key}");
                if (executionObj != null)
                {
                    ExecutionObjects.Add(item.Key, executionObj);
                }
            }
        }

        public void SpellWithTarget(Ability spellSkill, CombatEntity targetEntity)
        {
            if (CombatEntity.SpellingExecution != null)
                return;

            if (CombatEntity.SpellAbility.TryMakeAction(out var spellAction))
            {
                spellAction.SkillAbility = spellSkill;
                spellAction.InputTarget = targetEntity;
                spellAction.InputPoint = targetEntity.Position;

                spellSkill.OwnerEntity.Rotation = Quaternion.LookRotation(targetEntity.Position - spellSkill.OwnerEntity.Position);
                spellAction.InputRadian = spellSkill.OwnerEntity.Rotation.eulerAngles.y;

                spellAction.SpellSkill();
            }
        }

        public SpellAction SpellWithPoint(Ability spellSkill, Vector3 point)
        {
            Log.Info($"SpellComponent SpellWithPoint {spellSkill.Config.Id}");
            if (CombatEntity.SpellingExecution != null)
                return null;

            if (CombatEntity.SpellAbility.TryMakeAction(out var spellAction))
            {
                spellAction.SkillAbility = spellSkill;
                var forward = Vector3.Normalize(point - spellSkill.OwnerEntity.Position);
                var rotation = Quaternion.LookRotation(forward);
                spellSkill.OwnerEntity.Rotation = rotation;
                var angle = rotation.eulerAngles.y;
                var radian = angle * MathF.PI / 180f;
                spellAction.InputDirection = forward;
                spellAction.InputRadian = radian;
                spellAction.InputPoint = point;
                spellAction.SpellSkill();
            }

            return spellAction;
        }
        
    }
}