using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField] private Animator animator;
    [Header("피격 애니메이션")]
    [SerializeField, Min(0.0f)] private float damagedInterval = 0.9f;

    private float nextDamagedTime;

    private readonly int moveHash = Animator.StringToHash("Move");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int skillHash = Animator.StringToHash("Skill");
    private readonly int damagedHash = Animator.StringToHash("Damaged");
    private readonly int deadHash = Animator.StringToHash("Dead");

    private readonly int attackAnimatorSpeedHash = Animator.StringToHash("AttackAnimatorSpeed");

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void SetMove(bool isMoving)
    {
        if (animator == null) return;

        animator.SetBool(moveHash, isMoving);
    }
    public void SetAttackSpeed(float attackSpeed)
    {
        if (animator == null) return;

        animator.SetFloat(attackAnimatorSpeedHash, Mathf.Max(0.1f, attackSpeed));
    }
    public bool TryPlayAttack()
    {
        if (animator == null) return false;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsTag("Damaged") || state.IsTag("Dead") || state.IsTag("Attack") || state.IsTag("Skill")) return false;

        animator.ResetTrigger(attackHash);
        animator.SetTrigger(attackHash);
        return true;
    }
    public bool TryPlaySkill()
    {
        if (animator == null) return false;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        //해당 동작 중에는 새로운 스킬 애니메이션을 시작하지 않음.
        if (state.IsTag("Damaged") || state.IsTag("Dead") || state.IsTag("Attack") || state.IsTag("Skill")) return false;

        animator.ResetTrigger(skillHash);
        animator.SetTrigger(skillHash);

        return true;
    }
    public void PlayDamaged()
    {
        if (animator == null) return;
        //연속 피격 모션 방지
        if (Time.time < nextDamagedTime) return;

        //피격 연출 중복 재생을 막기 위함
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        //공격/스킬/사망은 피격으로 절대 끊지 않음
        if (currentState.IsTag("Attack")) return;
        if (currentState.IsTag("Skill")) return;
        if (currentState.IsTag("Dead")) return;
        if (currentState.IsTag("Damaged")) return;

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            if (nextState.IsTag("Attack")) return;
            if (nextState.IsTag("Skill")) return;
            if (nextState.IsTag("Dead")) return;
            if (nextState.IsTag("Damaged")) return;
        }
        nextDamagedTime = Time.time + damagedInterval;

        animator.ResetTrigger(damagedHash);
        animator.SetTrigger(damagedHash);
    }
    public void PlayDead()
    {
        if (animator == null) return;

        animator.ResetTrigger(attackHash);
        animator.ResetTrigger(skillHash);
        animator.ResetTrigger(damagedHash);

        animator.SetBool(moveHash, false);
        animator.SetTrigger(deadHash);
    }
    //공격 상태 확인
    public bool IsAttackAnimationPlaying()
    {
        if (animator == null) return false;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        return state.IsTag("Attack");
    }
    //스킬 상태 확인
    public bool IsSkillAnimationPlaying()
    {
        if (animator == null) return false;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        return state.IsTag("Skill");
    }

    //공격 애니메이션이 현재 상태, 전환 대상으로 존재하는지 확인하는 용
    public bool IsAttackAnimationActive()
    {
        if (animator == null) return false;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsTag("Attack")) return true;
        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            if (nextState.IsTag("Attack")) return true;
        }
        return false;
    }

}
