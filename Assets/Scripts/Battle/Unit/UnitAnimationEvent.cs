using UnityEngine;
//이벤트 전달용 스크립트
public class UnitAnimationEvent : MonoBehaviour
{
    private BattleUnit unit;

    private void Awake()
    {
        unit = GetComponentInParent<BattleUnit>();
    }

    //Attack애니메이션 실제 타격 시점
    public void AttackHit()
    {
        if (unit == null) return;

        unit.AttackHitEvent();
    }
    //Attack애니메이션 종료 시점
    public void AttackEnd()
    {
        if (unit == null) return;

        unit.AttackEndEvent();
    }
    //Skill애니메이션 실제 발생 시점
    public void SkillActivate()
    {
        if (unit == null) return;

        Debug.Log($"[SkillActivate Event] {unit.name}", gameObject);

        unit.SkillActivateEvent();
    }
    //Skill애니메이션 종료 시점
    public void SkillEnd()
    {
        if (unit == null) return;

        Debug.Log($"[SkillEnd Event] {unit.name}", gameObject);

        unit.SkillEndEvent();
    }
}
