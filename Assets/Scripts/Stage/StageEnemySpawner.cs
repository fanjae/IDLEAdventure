using System;
using UnityEngine;

public sealed class StageEnemySpawner : MonoBehaviour
{
    [Header("0번 슬롯은 사용하지 않음")]
    [SerializeField] private Transform[] slots;

    [Header("몬스터 Prefab 경로")]
    [SerializeField] private string enemyPrefabPath = "Prefab/Enemy";

    [Header("소환된 몬스터 부모")]
    [SerializeField]
    private Transform enemyRoot;

    [Header("Highlight")]
    [SerializeField] private SlotHighlightController slotHighlightController;

    [Header("몬스터 위치")]
    [SerializeField] private float spawnHeight = -0.2f;

    [SerializeField] private Vector3 spawnRotation = new Vector3(0f, 180f, 0f);


    private void Awake()
    {
        if (slots == null || slots.Length <= 1)
        {
            throw new Exception("StageEnemySpawner의 Tile Slots가 설정되지 않음");
        }

        if (enemyRoot == null)
        {
            throw new Exception("StageEnemySpawner의 Enemy Root가 설정되지 않음");
        }
    }


    public void LoadStage(StageData stage)
    {
        if (stage == null)
        {
            throw new Exception("StageEnemySpawner에 전달된 StageData가 없음");
        }

        foreach (StageEnemyData enemyData in stage.enemies)
        {
            SpawnEnemy(enemyData);
        }

        Debug.Log($"{stage.stageId}번 스테이지 적 생성 완료: {stage.enemies.Count}마리");
    }


    private void SpawnEnemy(StageEnemyData enemyData)
    {
        int slotNumber = enemyData.slotNumber;

        if (slotNumber <= 0 || slots == null || slotNumber >= slots.Length)
        {
            throw new Exception($"{enemyData.enemyId}의 슬롯 번호 {slotNumber}가 올바르지 않음");
        }

        Transform tile = slots[slotNumber];

        if (tile == null)
        {
            throw new Exception($"Tile Slots의 Element {slotNumber}에 타일이 연결되지 않음");
        }

        GameObject prefab = LoadEnemyPrefab(enemyData.enemyId);

        Vector3 spawnPosition = tile.position + tile.up * spawnHeight;

        Quaternion spawnQuaternion = Quaternion.Euler(spawnRotation);

        GameObject spawnedEnemy = Instantiate(prefab, spawnPosition, spawnQuaternion, enemyRoot);

        spawnedEnemy.name = $"{enemyData.enemyId}_Stage{enemyData.stageId}_Slot{enemyData.slotNumber}";

        BattleUnit battleUnit = spawnedEnemy.GetComponent<BattleUnit>();

        if (battleUnit == null)
        {
            throw new Exception($"{enemyData.enemyId} Prefab에 BattleUnit이 없음");
        }

        battleUnit.Initialize(enemyData.enemyLevel);
        //없어도 됨
        //battleUnit.ApplyStats(enemyData.maxHp, battleUnit.AttackPower, battleUnit.Defense);

        if (slotHighlightController != null)
        {
            slotHighlightController.SetEnemy(tile);
        }

        Debug.Log($"몬스터 소환: Enemy={enemyData.enemyId}, Slot={enemyData.slotNumber}, " +
            $"Level={battleUnit.Level}, MaxHp={battleUnit.MaxHp}");
    }

    private GameObject LoadEnemyPrefab(string enemyId)
    {
        string path = $"{enemyPrefabPath}/{enemyId}";

        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            throw new Exception($"몬스터 Prefab을 찾을 수 없음. 경로: {path}");
        }

        return prefab;
    }
}