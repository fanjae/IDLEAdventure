
public class UnitStateMachine
{
    private readonly UnitIdleState idleState;
    private readonly UnitMoveState moveState;
    private readonly UnitAttackState attackState;
    private readonly UnitDeadState deadState;

    private IUnitState currentState;

    public UnitState CurrentState { get; private set; }

    public UnitStateMachine(BattleUnit unit)
    {
        idleState = new UnitIdleState(unit, this);
        moveState = new UnitMoveState(unit, this);
        attackState = new UnitAttackState(unit, this);
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
            //일단 기본동작만 넣고, 나중에 스킬상태 추가할 예정

            case UnitState.Dead:
                return deadState;
            default:
                return null;
        }
    }

}
