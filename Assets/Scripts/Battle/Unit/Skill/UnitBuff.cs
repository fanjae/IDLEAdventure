using System.Collections;
using UnityEngine;

public class UnitBuff : MonoBehaviour
{
    private BattleUnit unit;

    private GameObject currentVfx;
    private Coroutine attackBuffRoutine;


    private void Awake()
    {
        unit = GetComponent<BattleUnit>();
    }
    private void OnDisable()
    {
        if (attackBuffRoutine != null)
        {
            StopCoroutine(attackBuffRoutine);
            attackBuffRoutine = null;
        }
        if (unit != null) unit.ClearAttackModifier();
        if (currentVfx != null)
        {
            Destroy(currentVfx);
            currentVfx = null;
        }
    }

    public void ApplyAttackBuff(int amount, float duration, GameObject vfxPrefab, Transform vfxPoint)
    {
        if (unit == null || unit.IsDead) return;
        //기존 버프가 남아있으면 새 버프로
        if (attackBuffRoutine != null)
        {
            StopCoroutine(attackBuffRoutine);
            attackBuffRoutine = null;
        }
        if (currentVfx != null)
        {
            Destroy(currentVfx);
            currentVfx = null;
        }

        attackBuffRoutine = StartCoroutine(AttackBuffRoutine(amount, duration, vfxPrefab, vfxPoint));
    }
    private IEnumerator AttackBuffRoutine(int amount, float duration, GameObject vfxPrefab, Transform vfxPoint)
    {
        int buffAmount = Mathf.Abs(amount);
        unit.SetAttackModifier(buffAmount);//공격력 증가
        //오라 VFX
        if (vfxPrefab != null)
        {
            Transform point = vfxPoint != null ? vfxPoint : transform;
            currentVfx = Instantiate(vfxPrefab, point.position, point.rotation, point);
        }
        //기능 확인 용
        Debug.Log($"[공격력 버프] {unit.name} / +{buffAmount} / 현재 공격력 : {unit.AttackPower}");

        yield return new WaitForSeconds(duration);
        //원래 공격력으로
        if (unit != null && !unit.IsDead)
        {
            unit.ClearAttackModifier();
            //기능 확인용
            Debug.Log($"[공격력 버프 종료] {unit.name} / 현재 공격력 : {unit.AttackPower}");
        }
        if (currentVfx != null)
        {
            Destroy(currentVfx);
            currentVfx = null;
        }

        attackBuffRoutine = null;
    }
}
