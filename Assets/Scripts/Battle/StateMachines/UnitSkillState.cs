
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

        unit.UpdateSkill();//SkillEnd 누락 대비 체크용

        //스킬이 진행 중이면 현재 상태 유지
        if (unit.IsUsingSkill) return;
        //보스 스킬 종료 후, 다음 행동 결정 BT가 함. 
        if (unit.UsesExternalDecision)
        {
            //Animator상태 때문에 시작 실패할 수도 있으므로 재시도
            if (unit.CanUseSkill()) unit.UseSkill();
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

        stateMachine.ChangeState(UnitState.Move);
    }
    public void Exit()
    {
        
    }
}
