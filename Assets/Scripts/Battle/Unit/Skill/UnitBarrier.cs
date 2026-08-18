using UnityEngine;

public class UnitBarrier : MonoBehaviour
{
    [Header("배리어 VFX 위치")]
    [SerializeField] private Transform vfxPoint;

    private int remainingBlockCount;//남은 블락 카운트
    private GameObject activeVfx;

    public bool IsActive => remainingBlockCount > 0;
    public int RemainingBlockCount => remainingBlockCount;

    public void Activate(int blockCount, GameObject vfxPrefab)
    {
        if (blockCount <= 0) return;

        Clear();
        remainingBlockCount = Mathf.Max(1, blockCount);

        if (vfxPrefab != null)
        {
            Transform parent = vfxPoint != null ? vfxPoint : transform;
            activeVfx = Instantiate(vfxPrefab, parent);
            activeVfx.transform.localPosition = Vector3.zero;
            activeVfx.transform.localRotation = Quaternion.identity;
        }

        //확인용
        Debug.Log($"{name} 배리어 활성 / " + $"블락 횟수 : {remainingBlockCount}");
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

        if (activeVfx != null)
        {
            Destroy(activeVfx);
            activeVfx = null;
        }
    }
    private void OnDestroy()
    {
        if (activeVfx != null) Destroy(activeVfx);
    }
}
