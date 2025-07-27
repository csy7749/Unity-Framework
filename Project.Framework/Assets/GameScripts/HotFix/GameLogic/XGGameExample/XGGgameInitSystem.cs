using System.Threading;
using GameLogic.Combat;
using UnityEngine;
using UnityFramework;

namespace GameLogic.XGGameExample
{
    public class XGGgameInitSystem : SingletonSerializedBehaviour<XGGgameInitSystem>
    {
        private GameObject _roomRoot;
        public ReferenceCollector ConfigsCollector;
        
        private void Awake()
        {
            _roomRoot = new GameObject("BattleRoom");
            var handle = PoolManager.Instance.GetGameObject("XiaoGong",parent: _roomRoot.transform);
            
            handle.transform.position = Vector3.zero;
            handle.transform.rotation = Quaternion.identity;
        }
        
        
        private void Update()
        {
            
        }

        private void FixedUpdate()
        {
            
        }

        private void OnApplicationQuit()
        {
            
        }
    }
}