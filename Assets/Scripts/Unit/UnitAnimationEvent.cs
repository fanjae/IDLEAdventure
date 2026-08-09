using UnityEngine;
//이벤트 전달용 스크립트
public class UnitAnimationEvent : MonoBehaviour
{
    private BattleUnit unit;

    //중복 체크
    private Animator animator;

    private void Awake()
    {
        unit = GetComponentInParent<BattleUnit>();

        animator = GetComponent<Animator>();
    }

    //Attack애니메이션 타격 프레임 때
    public void AttackHit()
    {
        if (unit == null) return;


        //확인용
        if (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            Debug.Log(
                $"[AnimationEvent AttackHit] " +
                $"Unit : {unit.name} / " +
                $"AttackTag : {state.IsTag("Attack")} / " +
                $"Transition : {animator.IsInTransition(0)} / " +
                $"NormalizedTime : {state.normalizedTime:F2}",
                gameObject);
        }





        unit.AttackHitEvent();
    }
    //Attack애니메이션 끝 부분
    public void AttackEnd()
    {
        if (unit == null) return;


        //확인용
        Debug.Log($"[AnimationEvent AttackEnd]" + 
            $"Unit : {unit.name} / ",
            gameObject);



        unit.AttackEndEvent();
    }
}
