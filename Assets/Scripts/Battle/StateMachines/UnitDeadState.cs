
public class UnitDeadState : IUnitState
{
    private readonly BattleUnit unit;

    public UnitDeadState(BattleUnit unit)
    {
        this.unit = unit;
    }

    public void Enter()
    {
        unit.ClearTarget();
        unit.StopMove();

        unit.CancelAttack();
        unit.CancelSkill();
    }
    public void Update()
    {

    }
    public void Exit()
    {

    }
}
