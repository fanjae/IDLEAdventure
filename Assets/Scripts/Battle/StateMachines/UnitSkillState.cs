
public class UnitSkillState : IUnitState
{
    private readonly BattleUnit unit;
    private readonly UnitStateMachine stateMachine;

    public UnitSkillState(BattleUnit unit, UnitStateMachine stateMachine)
    {
        this.unit = unit;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        unit.StopMove();
        unit.UseSkill();
    }
    public void Update()
    {
        if (!unit.CanBattle) return;

        unit.UpdateSkill();//스킬 행동시간 확인용

        if (unit.IsUsingSkill) return;
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

        stateMachine.ChangeState(UnitState.Move);
    }
    public void Exit()
    {
        
    }
}
