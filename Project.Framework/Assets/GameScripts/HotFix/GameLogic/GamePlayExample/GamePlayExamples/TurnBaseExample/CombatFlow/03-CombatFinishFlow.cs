using GameLogic;
using UnityEngine;
using UnityFramework;

namespace GameLogic
{
    
public class CombatFinishFlow : WorkFlow
{
    public override async void Startup()
    {
        base.Startup();
        Log.Debug("CombatFinishFlow Startup");

        GameObject.Destroy(GameObject.Find("CombatRoot"));

        await GameLogic.TimeHelper.WaitAsync(100);

        Finish();
    }
}
}
