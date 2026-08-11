using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class StageEnemySpawner : MonoBehaviour
{
    [Serializable]
    private sealed class EnemyPrefabEntry
    {
        public string enemyId;
        public GameObject prefab;
    }

    [Header("0번 슬롯은 사용하지 않음")]
    [SerializeField] private Transform[] tileSlots;

    [Header("몬스터 프리팹")]
    [SerializeField] private List<EnemyPrefabEntry> enemyPrefabs = new();

    [Header("소환된 몬스터 부모")]
    [SerializeField] private Transform enemyRoot;

    [Header("Highlight")]
    [SerializeField] private SlotHighlightController slotHighlightController;

    [Header("몬스터 위치")]
    [SerializeField] private float spawnHeight = 0.1f;
    [SerializeField] private Vector3 spawnRotation = new Vector3(0f, 180f, 0f);

    private readonly List<GameObject> spawnedEnemies = new();

    public void LoadStage(int stageId)
    {
        if (StageDatabase.Instance == null)
        {
            throw new Exception("StageDatabase가 Scene에 존재하지 않습니다.");
        }

        if (!StageDatabase.Instance.IsInitialized)
        {
            StageDatabase.Instance.Initialize();
        }

        StageData stage = StageDatabase.Instance.GetStage(stageId);

        if (stage == null)
        {
            throw new Exception($"{stageId}번 스테이지 데이터를 찾을 수 없습니다.");
        }

        ClearSpawnedEnemies();

        if (slotHighlightController != null)
        {
            slotHighlightController.ClearEnemyHighlights();
        }

        foreach (StageEnemyData enemyData in stage.enemies)
        {
            SpawnEnemy(enemyData);
        }

        Debug.Log($"{stageId}번 스테이지 로드 완료: 몬스터 {stage.enemies.Count}마리");
    }

    public void ClearSpawnedEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        spawnedEnemies.Clear();
    }

    private void SpawnEnemy(StageEnemyData enemyData)
    {
        int slotNumber = enemyData.slotNumber;

        Debug.Log(
            $"[StageEnemySpawner 확인] " +
            $"Object={gameObject.name}, " +
            $"InstanceID={GetInstanceID()}, " +
            $"Enemy={enemyData.enemyId}, " +
            $"Slot={slotNumber}, " +
            $"TileSlotsNull={tileSlots == null}, " +
            $"TileSlotsLength={(tileSlots == null ? -1 : tileSlots.Length)}"
        );

        if (slotNumber <= 0 || tileSlots == null || slotNumber >= tileSlots.Length)
        {
            throw new Exception($"{enemyData.enemyId}의 슬롯 번호 {slotNumber}가 올바르지 않습니다.");
        }

        Transform tile = tileSlots[slotNumber];

        if (tile == null)
        {
            throw new Exception($"Tile Slots의 Element {slotNumber}에 타일이 연결되지 않았습니다.");
        }

        GameObject prefab = FindEnemyPrefab(enemyData.enemyId);

        Transform parent = enemyRoot != null ? enemyRoot : transform;

        Vector3 spawnPosition = tile.position + tile.up * spawnHeight;
        Quaternion spawnQuaternion = Quaternion.Euler(spawnRotation);

        GameObject spawnedEnemy = Instantiate(prefab, spawnPosition, spawnQuaternion, parent);

        spawnedEnemy.name = $"{enemyData.enemyId}_Stage{enemyData.stageId}_Slot{enemyData.slotNumber}";

        spawnedEnemies.Add(spawnedEnemy);

        if (slotHighlightController != null)
        {
            slotHighlightController.SetEnemy(tile);
        }

        Debug.Log($"몬스터 소환: Enemy={enemyData.enemyId}, Slot={enemyData.slotNumber}, Level={enemyData.enemyLevel}, MaxHp={enemyData.maxHp}");
    }

    private GameObject FindEnemyPrefab(string enemyId)
    {
        foreach (EnemyPrefabEntry entry in enemyPrefabs)
        {
            if (string.Equals(entry.enemyId, enemyId, StringComparison.OrdinalIgnoreCase))
            {
                if (entry.prefab == null)
                {
                    throw new Exception($"{enemyId}에 Prefab이 연결되지 않았습니다.");
                }

                return entry.prefab;
            }
        }

        throw new Exception($"{enemyId}에 해당하는 몬스터 Prefab을 찾을 수 없습니다.");
    }
}