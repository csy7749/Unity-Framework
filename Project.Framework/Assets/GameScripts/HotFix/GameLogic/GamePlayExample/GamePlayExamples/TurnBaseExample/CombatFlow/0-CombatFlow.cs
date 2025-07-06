namespace GameLogic
{
    
public class CombatFlow : WorkFlow
{
    public int JumpToTime { get; set; }


    public override void Awake()
    {
        FlowSource = AddChild<WorkFlowSource>();
        FlowSource.ToEnter<CombatCreateFlow>().ToEnter<CombatRunFlow>().ToEnter<CombatFinishFlow>().ToRestart();
    }

    public override void Startup()
    {
        FlowSource.Startup();
    }
}
}
