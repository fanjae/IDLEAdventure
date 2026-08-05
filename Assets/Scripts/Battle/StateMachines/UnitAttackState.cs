
public class UnitAttackState : IUnitState
{
    private readonly BattleUnit unit;
    private readonly UnitStateMachine stateMachine;

    public UnitAttackState(BattleUnit unit, UnitStateMachine stateMachine)
    {
        this.unit = unit;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        unit.StopMove();
    }
    public void Update()
    {
        if (!unit.CanBattle)
        {
            unit.StopMove();
            return;
        }
        if (!unit.HasValidTarget())
        {
            unit.ClearTarget();
            stateMachine.ChangeState(UnitState.Idle);
            return;
        }
        if (!unit.IsTargetInAttackRange(0.2f))//공격->이동->공격이 반복되는 덜덜 떨리는 현상 방지용
        {                                     //기본 사거리에 여유값을 더해주었음.
            stateMachine.ChangeState(UnitState.Move);
            return;
        }

        unit.FaceTarget();
        unit.TryAttack();
    }
    public void Exit()
    {
        unit.CancelAttack();
    }
}
