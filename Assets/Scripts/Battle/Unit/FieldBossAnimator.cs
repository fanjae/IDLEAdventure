using UnityEngine;

public class FieldBossAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private readonly int projectileSkillHash = Animator.StringToHash("ProjectileSkill");
    private readonly int laserSkillHash = Animator.StringToHash("LaserSkill");
    private readonly int rageHash = Animator.StringToHash("Rage");

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    public bool PlayProjectileSkill()
    {
        if (!CanPlaySpecialAnimation()) return false;

        animator.ResetTrigger(projectileSkillHash);
        animator.SetTrigger(projectileSkillHash);
        return true;
    }
    public bool PlayLaserSkill()
    {
        if (!CanPlaySpecialAnimation()) return false;

        animator.ResetTrigger(laserSkillHash);
        animator.SetTrigger(laserSkillHash);
        return true;
    }
    public bool PlayRage()
    {
        if (!CanPlaySpecialAnimation()) return false;

        animator.ResetTrigger(rageHash);
        animator.SetTrigger(rageHash);
        return true;
    }

    private bool CanPlaySpecialAnimation()
    {
        if (animator == null) return false;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsTag("Dead")) return false;
        if (state.IsTag("Attack")) return false;
        if (state.IsTag("Skill")) return false;

        return true;
    }
}
