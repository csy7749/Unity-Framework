using System.Threading;
using GameLogic;
using GameLogic.Combat;
using Sirenix.OdinInspector;


#if UNITY
public class GamePlayInit : SerializedMonoBehaviour
{
    public static GamePlayInit Instance { get; private set; }
    public ReferenceCollector ConfigsCollector;
    public bool EntityLog;

    private void Awake()
    {
        Instance = this;
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
#endif
}