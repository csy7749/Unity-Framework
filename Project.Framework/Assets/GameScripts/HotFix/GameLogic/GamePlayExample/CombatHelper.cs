using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.Combat
{
    public static class CombatHelper
    {
        /// <summary>
        /// 获取最近的敌方单位（可配置阵营、距离限制等）
        /// </summary>
        /// <param name="self">自己的实体</param>
        /// <param name="enemyCamp">敌方阵营（如 Player、Enemy）</param>
        /// <param name="maxDistance">最大感知距离，默认无限制</param>
        /// <returns>最近的敌方单位 CombatEntity</returns>
        public static CombatEntity FindNearestEnemy(CombatEntity self, CampType enemyCamp, float maxDistance = float.MaxValue)
        {
            CombatEntity nearest = null;
            float nearestDist = maxDistance;

            foreach (var entityDic in CombatContext.Instance.Object2Entities)
            {
                var entity = entityDic.Value;
                if (entity == self || entity.CurrentHealth.Value <= 0)
                    continue;

                if (entity.CampType != enemyCamp)
                    continue;

                float dist = Vector3.Distance(self.Position, entity.Position);
                if (dist < nearestDist)
                {
                    nearest = entity;
                    nearestDist = dist;
                }
            }

            return nearest;
        }
    }
}