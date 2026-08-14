
public class UnitAttackState : IUnitState
{
    //공격 상태를 유지할 때 추가로 허용하는 거리
    private const float AttackExtraRange = 0.2f;

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
            unit.CancelAttack();
            unit.StopMove();
            return;
        }
        if (!unit.HasValidTarget())
        {
            unit.CancelAttack();
            unit.ClearTarget();

            stateMachine.ChangeState(UnitState.Idle);
            return;
        }
        //공격 중에는 새로운 행동을 시작하지 않고, 현재 타겟 방향만 유지
        if (unit.IsAttacking)
        {
            unit.FaceTarget();
            return;
        }
        if (!unit.IsTargetInAttackRange(AttackExtraRange))//공격->이동->공격이 반복되는 덜덜 떨리는 현상 방지용
        {                                     //기본 사거리에 여유값을 더해주었음.
            stateMachine.ChangeState(UnitState.Move);
            return;
        }
        if (unit.CanUseSkill())//스킬이 준비된 상태면 기본 공격보다 먼저 사용
        {
            stateMachine.ChangeState(UnitState.Skill);
            return;
        }

        unit.FaceTarget();
        unit.TryAttack();
    }
    public void Exit()
    {

    }
}
