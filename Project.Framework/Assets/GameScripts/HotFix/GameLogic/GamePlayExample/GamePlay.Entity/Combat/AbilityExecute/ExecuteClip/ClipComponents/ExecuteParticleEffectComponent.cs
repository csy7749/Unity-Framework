using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY
namespace GameLogic.Combat
{
    /// <summary>
    /// 
    /// </summary>
    public class ExecuteParticleEffectComponent : Component
    {
        public GameObject ParticleEffectPrefab { get; set; }
        public GameObject ParticleEffectObj { get; set; }


        public override void Awake()
        {
            Entity.OnEvent(nameof(ExecuteClip.TriggerEffect), OnTriggerStart);
            Entity.OnEvent(nameof(ExecuteClip.EndEffect), OnTriggerEnd);
        }

        public void OnTriggerStart(Entity entity)
        {
            ParticleEffectObj = GameObject.Instantiate(ParticleEffectPrefab, Entity.GetParent<AbilityExecution>().OwnerEntity.Position, Entity.GetParent<AbilityExecution>().OwnerEntity.Rotation);
#endif
        }

        public void OnTriggerEnd(Entity entity)
        {
            GameObject.Destroy(ParticleEffectObj);
        }
    }
}