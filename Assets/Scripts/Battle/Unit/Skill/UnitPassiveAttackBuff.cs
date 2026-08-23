using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPassiveAttackBuff : MonoBehaviour
{
    [Header("패시브 발동 설정")]
    [SerializeField, Min(0.1f)] private float activateDelay = 15.0f;

    [Header("공격력 버프 설정")]
    [SerializeField, Min(1)] private int attackBuff = 20;
    [SerializeField, Min(0.1f)] private float buffDuration = 20.0f;

    [Header("VFX 설정")]
    [SerializeField] private GameObject castVfxPrefab;
    [SerializeField] private GameObject targetVfxPrefab;
    [SerializeField, Min(0.1f)] private float castVfxDuration = 1.5f;
    [SerializeField] Transform castVfxPoint;

    private BattleUnit unit;
    private Coroutine passiveRoutine;

    private void Awake()
    {
        unit = GetComponent<BattleUnit>();
    }
    private void Start()
    {
        if (BattleManager.Instance == null) return;

        BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        BattleManager.Instance.OnBattleEnded += HandleBattleEnded;
    }
    private void OnDisable()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
    }

    private void HandleBattleStarted()
    {
        StopPassiveRoutine();
        passiveRoutine = StartCoroutine(PassiveRoutine());
    }
    private void HandleBattleEnded(UnitTeam winner)
    {
        StopPassiveRoutine();
    }
    private IEnumerator PassiveRoutine()
    {
        yield return new WaitForSeconds(activateDelay);

        if (unit == null || unit.IsDead || BattleManager.Instance == null || !BattleManager.Instance.IsBattleRunning)
        {
            passiveRoutine = null;
            yield break;
        }

        ApplyBuff();
        passiveRoutine = null;
    }
    private void ApplyBuff()
    {
        PlayCastVfx();
        List<BattleUnit> allies = BattleManager.Instance.GetAliveAllies(unit);
        for (int i = 0; i < allies.Count; i++)
        {
            BattleUnit ally = allies[i];
            if (ally == null || ally.IsDead) continue;
            
            UnitBuff buff = ally.GetComponent<UnitBuff>();
            if (buff == null) continue;
            buff.ApplyAttackBuff(attackBuff, buffDuration, targetVfxPrefab, null);
        }
        //확인용
        Debug.Log($"[패시브 공증버프] {unit.name} / 아군 전체 공격력 + {attackBuff} / {buffDuration}초");
    }
    private void PlayCastVfx()
    {
        if (castVfxPrefab == null) return;

        Transform point = castVfxPoint != null ? castVfxPoint : transform;
        GameObject vfx = Instantiate(castVfxPrefab, point.position, point.rotation, point);
        Destroy(vfx, castVfxDuration);
    }
    private void StopPassiveRoutine()
    {
        if (passiveRoutine == null) return;

        StopCoroutine(passiveRoutine);
        passiveRoutine = null;
    }
}
