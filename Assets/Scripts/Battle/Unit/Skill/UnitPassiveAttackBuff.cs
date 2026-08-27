using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

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

    private Coroutine subscribeRoutine;

    private ObjectPool<GameObject> castVfxPool;

    private void Awake()
    {
        unit = GetComponent<BattleUnit>();
    }
    private void OnEnable()
    {
        subscribeRoutine = StartCoroutine(SubscribeRoutine());
    }
    private void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
        StopPassiveRoutine();
    }
    private void OnDestroy()
    {
        if (castVfxPool != null)
        {
            castVfxPool.Clear();
            castVfxPool = null;
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
        if (castVfxPool == null) CreateCastVfxPool();
        if (castVfxPool == null) return;

        Transform point = castVfxPoint != null ? castVfxPoint : transform;
        GameObject vfx = castVfxPool.Get();
        vfx.transform.SetParent(point);
        vfx.transform.SetPositionAndRotation(point.position, point.rotation);
        vfx.SetActive(true);
        RestartParticles(vfx);
        StartCoroutine(ReleaseCastVfxRoutine(vfx, castVfxDuration));
    }
    //풀
    private void CreateCastVfxPool()
    {
        castVfxPool = new ObjectPool<GameObject>(
            CreateCastVfx,
            OnGetCastVfx,
            OnReleaseCastVfx,
            OnDestroyCastVfx,
            true,
            3,
            10);
    }
    private GameObject CreateCastVfx()
    {
        GameObject vfx = Instantiate(castVfxPrefab);
        vfx.SetActive(false);
        return vfx;
    }
    private void OnGetCastVfx(GameObject vfx)
    {

    }
    private void OnReleaseCastVfx(GameObject vfx)
    {
        vfx.transform.SetParent(null);
        vfx.SetActive(false);
    }
    private void OnDestroyCastVfx(GameObject vfx)
    {
        if (vfx != null) Destroy(vfx);
    }

    //파티클
    private void RestartParticles(GameObject vfx)
    {
        ParticleSystem[] particles = vfx.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles[i].Play(true);
        }
    }
    private IEnumerator ReleaseCastVfxRoutine(GameObject vfx, float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
        if (vfx == null) yield break;
        castVfxPool.Release(vfx);
    }

    private void StopPassiveRoutine()
    {
        if (passiveRoutine == null) return;

        StopCoroutine(passiveRoutine);
        passiveRoutine = null;
    }
    private IEnumerator SubscribeRoutine()
    {
        while (BattleManager.Instance == null) yield return null;

        //중복 구독 방지
        BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;

        BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        BattleManager.Instance.OnBattleEnded += HandleBattleEnded;

        subscribeRoutine = null;
    }
}
