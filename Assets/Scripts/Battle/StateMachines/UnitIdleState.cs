
public class UnitIdleState : IUnitState
{
    private readonly BattleUnit unit;
    private readonly UnitStateMachine stateMachine;

    public UnitIdleState(BattleUnit unit, UnitStateMachine stateMachine)
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
        if (!unit.CanBattle) return;
        //보스는 외부에서 판단하고 있기 때문에, Idle에서 별도의 행동 결정을 하지 않음.
        if (unit.UsesExternalDecision) return;
        
        if (!unit.HasValidTarget()) unit.FindTarget(); //타겟이 없거나 사망했거나 비활성화된 상태라면 새 타겟을 찾는다
        if (!unit.HasValidTarget()) return; //그래도 없으면 Idle상태로
        if (unit.IsTargetInAttackRange())//공격 범위 안에 있으면 바로 공격
        {
            stateMachine.ChangeState(UnitState.Attack);
            return;
        }
        if (unit.CanUseSkill())
        {
            stateMachine.ChangeState(UnitState.Skill);
            return;
        }
        //공격 범위 밖에 있으면 이동
        stateMachine.ChangeState(UnitState.Move);
    }
    public void Exit()
    {

    }
}
