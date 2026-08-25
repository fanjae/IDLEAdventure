using UnityEngine;

public class FieldBossAnimationEvent : MonoBehaviour
{
    private FieldBossBehaviorTree bossBehaviorTree;

    private void Awake()
    {
        bossBehaviorTree = GetComponentInParent<FieldBossBehaviorTree>();
    }

    public void RageActivate()
    {
        if (bossBehaviorTree == null) return;

        bossBehaviorTree.RageActivateEvent();
    }
    public void RageEnd()
    {
        if (bossBehaviorTree == null) return;

        bossBehaviorTree.RageEndEvent();
    }
}
