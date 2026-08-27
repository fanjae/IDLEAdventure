using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class UnitBuff : MonoBehaviour
{
    private BattleUnit unit;

    private GameObject currentVfx;
    private Coroutine attackBuffRoutine;

    private ObjectPool<GameObject> vfxPool;
    private GameObject vfxPoolPrefab;

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
        ReleaseCurrentVfx();
    }
    private void OnDestroy()
    {
        ReleaseCurrentVfx();
        if (vfxPool != null)
        {
            vfxPool.Clear();
            vfxPool = null;
            vfxPoolPrefab = null;
        }
    }

    public void ApplyAttackBuff(int amount, float duration, GameObject vfxPrefab, Transform vfxPoint)
    {
        if (unit == null || unit.IsDead) return;
        //기존 버프 중지
        if (attackBuffRoutine != null)
        {
            StopCoroutine(attackBuffRoutine);
            attackBuffRoutine = null;
        }
        ReleaseCurrentVfx();
        //다른 VFX가 들어온 경우 해당 VFX용 풀로 변경
        if (vfxPrefab != null) CreateVfxPool(vfxPrefab);

        attackBuffRoutine = StartCoroutine(AttackBuffRoutine(amount, duration, vfxPrefab, vfxPoint));
    }
    private IEnumerator AttackBuffRoutine(int amount, float duration, GameObject vfxPrefab, Transform vfxPoint)
    {
        int buffAmount = Mathf.Abs(amount);
        unit.SetAttackModifier(buffAmount);//공격력 증가
        //오라 VFX
        if (vfxPrefab != null && vfxPool != null)
        {
            Transform point = vfxPoint != null ? vfxPoint : transform;
            currentVfx = vfxPool.Get();
            currentVfx.transform.SetParent(point);
            currentVfx.transform.SetPositionAndRotation(point.position, point.rotation);
            currentVfx.SetActive(true);
            RestartParticles(currentVfx);
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
        ReleaseCurrentVfx();
        attackBuffRoutine = null;
    }
    //풀
    private void CreateVfxPool(GameObject prefab)
    {
        if (prefab == null) return;
        if (vfxPool != null && vfxPoolPrefab == prefab) return;
        if (vfxPool != null) vfxPool.Clear();

        vfxPoolPrefab = prefab;
        vfxPool = new ObjectPool<GameObject>(
            CreateVfx,
            OnGetVfx,
            OnReleaseVfx,
            OnDestroyVfx,
            true,
            3,
            10);
    }
    private GameObject CreateVfx()
    {
        if (vfxPoolPrefab == null) return null;

        GameObject vfx = Instantiate(vfxPoolPrefab);
        vfx.SetActive(false);
        return vfx;
    }
    private void OnGetVfx(GameObject vfx)
    {

    }
    private void OnReleaseVfx(GameObject vfx)
    {
        vfx.transform.SetParent(null);
        vfx.SetActive(false);
    }
    private void OnDestroyVfx(GameObject vfx)
    {
        if (vfx != null) Destroy(vfx);
    }
    private void ReleaseCurrentVfx()
    {
        if (currentVfx == null) return;
        if (vfxPool != null)
        {
            vfxPool.Release(currentVfx);
        }
        else
        {
            Destroy(currentVfx);
        }
        currentVfx = null;
    }
    private void RestartParticles(GameObject vfx)
    {
        ParticleSystem[] particles = vfx.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles[i].Play(true);
        }
    }
}
