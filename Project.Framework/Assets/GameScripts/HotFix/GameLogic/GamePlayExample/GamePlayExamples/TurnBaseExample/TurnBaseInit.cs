using GameLogic;
using Sirenix.OdinInspector;

namespace GameLogic
{
    
public class TurnBaseInit : SerializedMonoBehaviour
{
    public static TurnBaseInit Instance { get; private set; }
    public int JumpToTime;


    private void Start()
    {
        Instance = this;

        var combatFlow = ECSNode.Instance.AddChild<CombatFlow>();
        combatFlow.ToEnd();
        combatFlow.JumpToTime = JumpToTime;
        combatFlow.Startup();
    }
}
}
