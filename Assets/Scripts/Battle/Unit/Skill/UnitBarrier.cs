using UnityEngine;
using UnityEngine.Pool;

public class UnitBarrier : MonoBehaviour
{
    [Header("배리어 VFX 위치")]
    [SerializeField] private Transform vfxPoint;

    private int remainingBlockCount;//남은 블락 카운트
    private GameObject activeVfx;

    private ObjectPool<GameObject> vfxPool;
    private GameObject vfxPoolPrefab;

    public bool IsActive => remainingBlockCount > 0;
    public int RemainingBlockCount => remainingBlockCount;

    public void Activate(int blockCount, GameObject vfxPrefab)
    {
        if (blockCount <= 0) return;

        Clear();

        remainingBlockCount = Mathf.Max(1, blockCount);

        if (vfxPrefab != null)
        {
            CreateVfxPool(vfxPrefab);
            if (vfxPool != null)
            {
                Transform parent = vfxPoint != null ? vfxPoint : transform;
                activeVfx = vfxPool.Get();
                activeVfx.transform.SetParent(parent);
                activeVfx.transform.localPosition = Vector3.zero;
                activeVfx.transform.localRotation = Quaternion.identity;
                activeVfx.SetActive(true);
                RestartParticles(activeVfx);
            }
        }
        //확인용
        Debug.Log($"{name} 배리어 활성 / 블락 횟수 : {remainingBlockCount}");
    }
    public bool TryBlockDamage()
    {
        if (!IsActive) return false;

        remainingBlockCount--;

        Debug.Log($"{name} 공격 블락 / " + $"남은 횟수 : {remainingBlockCount}");

        if (remainingBlockCount <= 0) Clear();

        return true;
    }
    public void Clear()
    {
        remainingBlockCount = 0;

        ReleaseActiveVfx();
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
                1,
                2);
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
        vfx.SetActive(false);
    }
    private void OnDestroyVfx(GameObject vfx)
    {
        if (vfx != null) Destroy(vfx);
    }
    private void ReleaseActiveVfx()
    {
        if (activeVfx == null) return;
        if (vfxPool != null)
        {
            vfxPool.Release(activeVfx);
        }
        else
        {
            Destroy(activeVfx);
        }
        activeVfx = null;
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


    private void OnDestroy()
    {
        ReleaseActiveVfx();
        if (vfxPool != null)
        {
            vfxPool.Clear();
            vfxPool = null;
            vfxPoolPrefab = null;
        }
    }
}
