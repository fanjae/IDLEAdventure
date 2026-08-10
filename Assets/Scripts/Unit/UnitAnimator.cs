using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField] private Animator animator;

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
        if (animator.IsInTransition(0)) return false;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsTag("Damaged") || state.IsTag("Dead") || state.IsTag("Attack")) return false;

        animator.ResetTrigger(attackHash);
        animator.SetTrigger(attackHash);
        return true;
    }
    public bool TryPlaySkill()
    {
        if (animator == null) return false;
        if (animator.IsInTransition(0)) return false; //다른 상태로 전환 중이면 잠시 대기

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

        //피격 연출 중복 재생을 막기 위함
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsTag("Damaged")) return;
        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            if (nextState.IsTag("Damaged")) return;
        }


        animator.ResetTrigger(damagedHash);
        animator.SetTrigger(damagedHash);
    }
    public void PlayDead()
    {
        if (animator == null) return;

        animator.ResetTrigger(attackHash);
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
}
