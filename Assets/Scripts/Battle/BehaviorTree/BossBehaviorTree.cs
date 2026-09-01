using UnityEngine;

[RequireComponent(typeof(BattleUnit))]
public class BossBehaviorTree : MonoBehaviour
{
    private BattleUnit unit;
    private BTNode root;

    private void Awake()
    {
        unit = GetComponent<BattleUnit>();

        unit.SetExternalDecision(true);//보스만 사용하고 있으니, 보스니까 true

        CreateTree();
    }
    void Update()
    {
        if (unit == null) return;
        if (!unit.CanBattle) return;

        root?.Run();

        //버그 확인용
        //Debug.Log(
        //    $"[Boss] State : {unit.CurrentState}" + 
        //    $"Attack : {unit.IsAttacking}" + 
        //    $"Skill : {unit.IsUsingSkill}" + 
        //    $"Target : {(unit.Target != null ? unit.Target.name : "NULL")}" + 
        //    $"Valid : {unit.HasValidTarget()}" + 
        //    $"Range : {unit.IsTargetInAttackRange()}" + 
        //    $"CanSkill : {unit.CanUseSkill()}");
    }

    private void CreateTree()
    {
        root = new BTSelector(
            
            //스킬 사용 중이면 Skill State 유지
            new BTSequence(new BTCondition(() => unit.IsUsingSkill), ChangeStateNode(UnitState.Skill)),
            //기본 공격 중이면 Attack State 유지
            new BTSequence(new BTCondition(() => unit.IsAttacking), ChangeStateNode(UnitState.Attack)),
            //공격은 취소됐어도 기존 공격 애니메이션이 끝나기 전에는 스킬로 넘어가지 않음
            new BTSequence(new BTCondition(() => unit.IsAttackAnimationActive()), ChangeStateNode(UnitState.Attack)),
            //스킬 사용 가능하면 Skill 우선 사용
            new BTSequence(new BTCondition(() => unit.CanUseSkill()), ChangeStateNode(UnitState.Skill)),
            //타겟이 없으면 탐색
            new BTSequence(new BTCondition(() => !unit.HasValidTarget()), new BTAction(FindTarget)),
            //공격 사거리 안이면 Attack
            new BTSequence(new BTCondition(() => unit.HasValidTarget()),
                           new BTCondition(() => unit.IsTargetInAttackRange()), ChangeStateNode(UnitState.Attack)),
            //타겟은 있지만 멀리 있으면 Move
            new BTSequence(new BTCondition(() => unit.HasValidTarget()), ChangeStateNode(UnitState.Move)),
            //아무런 행동도 할 수 없으면 Idle
            ChangeStateNode(UnitState.Idle)

            );
    }

    private BTNode ChangeStateNode(UnitState state)
    {
        return new BTAction(
                () =>
                {
                    unit.ChangeState(state);
                    return BTStatus.Success;
                }
            );
    }
    private BTStatus FindTarget()
    {
        unit.FindTarget();

        return unit.HasValidTarget() ? BTStatus.Success : BTStatus.Failure;
    }
}
