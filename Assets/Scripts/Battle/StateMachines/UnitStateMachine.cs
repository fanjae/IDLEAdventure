
public class UnitStateMachine
{
    private readonly UnitIdleState idleState;
    private readonly UnitMoveState moveState;
    private readonly UnitAttackState attackState;
    private readonly UnitSkillState skillState;
    private readonly UnitDeadState deadState;

    private IUnitState currentState;

    public UnitState CurrentState { get; private set; }

    public UnitStateMachine(BattleUnit unit)
    {
        idleState = new UnitIdleState(unit, this);
        moveState = new UnitMoveState(unit, this);
        attackState = new UnitAttackState(unit, this);
        skillState = new UnitSkillState(unit, this);
        deadState = new UnitDeadState(unit);

    }

    public void Start()
    {
        ChangeState(UnitState.Idle);
    }
    public void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(UnitState newState)
    {
        if (currentState != null && CurrentState == newState) return;

        IUnitState nextState = GetState(newState);
        if (nextState == null) return;

        currentState?.Exit();

        CurrentState = newState;
        currentState = nextState;

        currentState.Enter();
    }
    private IUnitState GetState(UnitState state)
    {
        switch (state)
        {
            case UnitState.Idle:
                return idleState;
            case UnitState.Move:
                return moveState;
            case UnitState.Attack:
                return attackState;
            case UnitState.Skill:
                return skillState;
            case UnitState.Dead:
                return deadState;
        }
        return null;
    }

}
