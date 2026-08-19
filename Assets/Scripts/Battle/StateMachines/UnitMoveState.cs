
public class UnitMoveState : IUnitState
{
    private readonly BattleUnit unit;
    private readonly UnitStateMachine stateMachine;

    public UnitMoveState(BattleUnit unit, UnitStateMachine stateMachine)
    {
        this.unit = unit;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {

    }
    public void Update()
    {
        if (!unit.CanBattle)
        {
            unit.StopMove();
            return;
        }
        //보스의 행동 판단은 BT가 함. 여기서는 실행만 담당.
        if (unit.UsesExternalDecision)
        {
            if (!unit.HasValidTarget())
            {
                unit.StopMove();
                return;
            }
            unit.MoveToTarget();
            return;
        }
        if (!unit.HasValidTarget())
        {
            unit.ClearTarget();
            stateMachine.ChangeState(UnitState.Idle);
            return;
        }
        if (unit.IsTargetInAttackRange())
        {
            stateMachine.ChangeState(UnitState.Attack);
            return;
        }
        //이동 중에도 스킬 조건을 확인하기 위해 추가
        if (unit.CanUseSkill())
        {
            stateMachine.ChangeState(UnitState.Skill);
            return;
        }

        unit.MoveToTarget();
    }
    public void Exit()
    {
        unit.StopMove();
    }
}
