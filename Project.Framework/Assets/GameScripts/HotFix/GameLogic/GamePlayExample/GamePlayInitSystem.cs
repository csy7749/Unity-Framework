using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameLogic.Combat;
using UnityEngine;

namespace GameLogic
{
    public class GamePlayInitSystem : SingletonSerializedBehaviour<GamePlayInitSystem>
    {
        private GameObject _roomRoot;
        public ReferenceCollector ConfigsCollector;
        public bool EntityLog;
        
        private void Awake()
        {
            _roomRoot = new GameObject("BattleRoom");
            var handle = PoolManager.Instance.GetGameObject("Hero",parent: _roomRoot.transform);
            
            handle.transform.position = Vector3.zero;
            handle.transform.rotation = Quaternion.identity;
            
            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
            Entity.EnableLog = EntityLog;
            var ecsNode = ECSNode.Create();
            ecsNode.AddChild<TimerManager>();
            ecsNode.AddChild<CombatContext>();
            ecsNode.AddComponent<ConfigManageComponent>(ConfigsCollector);
        }
        
        
        private void Update()
        {
            ThreadSynchronizationContext.Instance.Update();
            ECSNode.Instance.Update();
            TimerManager.Instance.Update();
        }

        private void FixedUpdate()
        {
            ECSNode.Instance.FixedUpdate();
        }

        private void OnApplicationQuit()
        {
            ECSNode.Destroy();
        }
    }
}