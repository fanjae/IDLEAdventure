using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public sealed class AutoStagePreviewBattleController : MonoBehaviour
{
    [Header("3D 전투 미리보기")]
    [SerializeField] private RawImage battlePreviewImage;
    [SerializeField] private Camera battleCamera;
    [SerializeField] private RenderTexture battleRenderTexture;
    [SerializeField] private List<Transform> allySpawnPoints = new();
    [SerializeField] private List<Transform> enemySpawnPoints = new();
    [SerializeField] private List<GameObject> allyPreviewPrefabs = new();
    [SerializeField] private List<GameObject> enemyPreviewPrefabs = new();

    [Header("프리뷰 레이어")]
    [SerializeField] private LayerMask previewLayerMask;

    [Header("공격 연출")]
    [SerializeField, Min(0.1f)] private float battleTurnInterval = 0.2f;
    [SerializeField, Min(0.0f)] private float attackLungeDistance = 0.28f;
    [SerializeField, Min(0.05f)] private float attackLungeDuration = 0.18f;

    [Header("프리뷰 모델 크기")]
    [SerializeField] private List<PreviewUnitData> allyPreviewUnits = new();
    [SerializeField] private List<PreviewUnitData> enemyPreviewUnits = new();

    private readonly List<GameObject> previewUnits = new();

    private Coroutine previewRoutine;
    private int previewLayer;

    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int DamagedTriggerHash = Animator.StringToHash("Damaged");
    private static readonly int MoveParameterHash = Animator.StringToHash("Move");

    [System.Serializable]
    private class PreviewUnitData
    {
        public GameObject prefab;

        [Range(0.1f, 2.0f)]
        public float scale = 1.0f;
    }

    private void OnEnable()
    {
        RestartPreview();
    }

    private void OnDisable()
    {
        StopPreview();
    }

    // 프리뷰 모델을 다시 생성하고 반복 연출 시작
    private void RestartPreview()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        StopPreview();

        if (!ValidatePreviewSettings())
        {
            return;
        }

        battleCamera.targetTexture = battleRenderTexture;
        battlePreviewImage.texture = battleRenderTexture;
        battleCamera.enabled = true;

        previewRoutine = StartCoroutine(BattlePreviewLoop());
    }

    // 프리뷰 설정 유효성 검사
    private bool ValidatePreviewSettings()
    {
        if (battlePreviewImage == null || battleCamera == null || battleRenderTexture == null)
        {
            Debug.LogWarning($"{name} : 전투 미리보기 UI 또는 카메라 설정이 누락되었습니다.", this);
            return false;
        }

        previewLayer = GetPreviewLayer();

        if (previewLayer < 0)
        {
            return false;
        }

        if (allySpawnPoints == null || enemySpawnPoints == null)
        {
            Debug.LogWarning($"{name} : 아군 또는 적군 SpawnPoint가 설정되지 않았습니다.", this);
            return false;
        }

        if (allySpawnPoints.Count == 0 || enemySpawnPoints.Count == 0 || allyPreviewPrefabs.Count == 0 || enemyPreviewPrefabs.Count == 0)
        {
            Debug.LogWarning($"{name} : 프리뷰 캐릭터 또는 SpawnPoint가 설정되지 않았습니다.", this);
            return false;
        }

        return true;
    }

    // 단일 레이어 마스크에서 실제 레이어 번호 반환
    private int GetPreviewLayer()
    {
        int mask = previewLayerMask.value;

        if (mask == 0 || (mask & (mask - 1)) != 0)
        {
            Debug.LogWarning($"{name} : Preview Layer는 하나의 레이어만 설정해야 합니다.", this);
            return -1;
        }

        int layer = 0;

        while (mask > 1)
        {
            mask >>= 1;
            layer++;
        }

        return layer;
    }

    // 생성된 프리뷰 오브젝트와 카메라 정리
    private void StopPreview()
    {
        if (previewRoutine != null)
        {
            StopCoroutine(previewRoutine);
            previewRoutine = null;
        }

        foreach (GameObject previewUnit in previewUnits)
        {
            if (previewUnit != null)
            {
                Destroy(previewUnit);
            }
        }

        previewUnits.Clear();

        if (Application.isPlaying && battleCamera != null)
        {
            battleCamera.enabled = false;
        }
    }

    // 무작위 진영의 캐릭터가 상대 진영을 공격하는 반복 연출
    private IEnumerator BattlePreviewLoop()
    {
        List<GameObject> allies = SpawnPreviewTeam(allyPreviewUnits, allySpawnPoints, true);
        List<GameObject> enemies = SpawnPreviewTeam(enemyPreviewUnits, enemySpawnPoints, false);

        yield return new WaitForSeconds(0.35f);

        while (isActiveAndEnabled && allies.Count > 0 && enemies.Count > 0)
        {
            bool allyAttacker = Random.Range(0, 2) == 0;
            List<GameObject> attackers = allyAttacker ? allies : enemies;
            List<GameObject> targets = allyAttacker ? enemies : allies;

            GameObject attacker = attackers[Random.Range(0, attackers.Count)];
            GameObject target = targets[Random.Range(0, targets.Count)];

            yield return PlayPreviewAttack(attacker, target);
            yield return new WaitForSeconds(battleTurnInterval);
        }

        previewRoutine = null;
    }

    // SpawnPoint 위치에 프리뷰 모델 생성
    // SpawnPoint 위치에 프리뷰 모델 생성
    private List<GameObject> SpawnPreviewTeam(List<PreviewUnitData> units, List<Transform> spawnPoints, bool isAlly)
    {
        List<GameObject> spawnedUnits = new();

        if (units == null || spawnPoints == null)
        {
            return spawnedUnits;
        }

        int count = Mathf.Min(units.Count, spawnPoints.Count);

        for (int i = 0; i < count; i++)
        {
            PreviewUnitData unitData = units[i];

            if (unitData == null || unitData.prefab == null || spawnPoints[i] == null)
            {
                continue;
            }

            GameObject previewUnit = Instantiate(unitData.prefab, spawnPoints[i], false);
            previewUnit.name = $"{(isAlly ? "Ally" : "Enemy")}_Preview_{i + 1}";
            previewUnit.transform.localPosition = Vector3.zero;
            previewUnit.transform.localRotation = Quaternion.identity;

            Vector3 baseScale = previewUnit.transform.localScale;
            previewUnit.transform.localScale = baseScale * unitData.scale;

            SetLayerRecursively(previewUnit, previewLayer);
            ConfigurePreviewUnit(previewUnit);

            spawnedUnits.Add(previewUnit);
            previewUnits.Add(previewUnit);
        }

        return spawnedUnits;
    }

    // 프리뷰 모델의 불필요한 UI와 충돌체 제거
    private void ConfigurePreviewUnit(GameObject previewUnit)
    {
        foreach (Canvas canvas in previewUnit.GetComponentsInChildren<Canvas>(true))
        {
            canvas.gameObject.SetActive(false);
        }

        foreach (Collider collider in previewUnit.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        Animator animator = previewUnit.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(AttackTriggerHash);
        animator.ResetTrigger(DamagedTriggerHash);
        animator.SetBool(MoveParameterHash, false);
    }

    // 공격 애니메이션과 전진 및 복귀 연출 실행
    private IEnumerator PlayPreviewAttack(GameObject attacker, GameObject target)
    {
        if (attacker == null || target == null || !attacker.activeInHierarchy || !target.activeInHierarchy)
        {
            yield break;
        }

        PlayAttackAnimation(attacker);

        Transform attackerTransform = attacker.transform;
        Transform parent = attackerTransform.parent;
        Vector3 start = attackerTransform.localPosition;
        Vector3 direction = target.transform.position - attackerTransform.position;

        direction.y = 0.0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            direction.Normalize();

            Vector3 localDirection = parent != null ? parent.InverseTransformDirection(direction) : direction;

            Vector3 hitPosition = start + localDirection.normalized * attackLungeDistance;

            yield return MovePreviewUnit(attackerTransform, start, hitPosition, attackLungeDuration);

            PlayDamagedAnimation(target);

            yield return new WaitForSeconds(0.12f);
            yield return MovePreviewUnit(attackerTransform, hitPosition, start, attackLungeDuration);
        }

        yield return new WaitForSeconds(0.2f);
    }

    // 공격 애니메이션 재생
    private static void PlayAttackAnimation(GameObject previewUnit)
    {
        Animator animator = previewUnit.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(AttackTriggerHash);
        animator.SetTrigger(AttackTriggerHash);
    }

    // 피격 애니메이션 재생
    private static void PlayDamagedAnimation(GameObject previewUnit)
    {
        Animator animator = previewUnit.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(DamagedTriggerHash);
        animator.SetTrigger(DamagedTriggerHash);
    }

    // 프리뷰 유닛 위치를 짧게 이동
    private static IEnumerator MovePreviewUnit(Transform unit, Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0.0f)
        {
            unit.localPosition = to;
            yield break;
        }

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            unit.localPosition = Vector3.LerpUnclamped(from, to, Mathf.SmoothStep(0.0f, 1.0f, t));

            yield return null;
        }

        unit.localPosition = to;
    }

    // 프리뷰 모델과 하위 오브젝트의 레이어를 동일하게 설정
    private static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;

        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}