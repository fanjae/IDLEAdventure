using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class StageDatabase : MonoBehaviour
{
    public static StageDatabase Instance { get; private set; }

    [Header("생명주기")]
    [SerializeField] private bool persistAcrossScenes = true;

    [Header("Stage CSV")]
    [SerializeField] private TextAsset stagesCsv;
    [SerializeField] private TextAsset stageEnemiesCsv;

    public bool IsInitialized => isInitialized;
    public int StageCount => stages.Count;

    private readonly Dictionary<int, StageData> stages = new();

    private bool isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        CsvInspection();
        ClearLoadData();

        try
        {
            LoadStages();
            LoadStageEnemies();
            LoadStage();

            isInitialized = true;

            Debug.Log($"StageDatabase 초기화 완료: {stages.Count}개 스테이지");
        }
        catch
        {
            ClearLoadData();
            throw;
        }
    }

    public StageData GetStage(int stageId)
    {
        if (stages.TryGetValue(stageId, out StageData stage))
        {
            return stage;
        }

        Debug.LogError($"{stageId}번 스테이지 데이터가 없습니다.");

        return null;
    }

    public bool TryGetStage(int stageId, out StageData stage)
    {
        return stages.TryGetValue(stageId, out stage);
    }

    private void LoadStages()
    {
        List<StageData> loadedStages = CsvMapper.Read<StageData>(stagesCsv);

        foreach (StageData stage in loadedStages)
        {
            if (stages.ContainsKey(stage.stageId))
            {
                throw new Exception($"{stagesCsv.name}에 stageId {stage.stageId}가 중복되어 있습니다.");
            }

            stages.Add(stage.stageId, stage);
        }
    }

    private void LoadStageEnemies()
    {
        List<StageEnemyData> loadedEnemies = CsvMapper.Read<StageEnemyData>(stageEnemiesCsv);

        foreach (StageEnemyData enemy in loadedEnemies)
        {
            StageData stage = RequireStage(enemy.stageId, stageEnemiesCsv.name);

            stage.enemies.Add(enemy);
        }
    }
    private void LoadStage()
    {
        if (stages.Count == 0)
        {
            throw new Exception("불러온 스테이지 데이터가 없음");
        }

        foreach (StageData stage in stages.Values)
        {
            StageEnemyInspection(stage);
        }
    }

    private static void StageEnemyInspection(StageData stage)
    {
        if (stage.enemies.Count == 0)
        {
            Debug.LogWarning($"{stage.stageId}번 스테이지에 몬스터가 없음");
            return;
        }

        HashSet<int> usedSlots = new();

        foreach (StageEnemyData enemy in stage.enemies)
        {
            if (!usedSlots.Add(enemy.slotNumber))
            {
                throw new Exception($"{stage.stageId}번 스테이지에서 몬스터 슬롯 번호 {enemy.slotNumber}가 중복");
            }
        }
    }

    private StageData RequireStage(int stageId, string csvName)
    {
        if (stages.TryGetValue(stageId, out StageData stage))
        {
            return stage;
        }

        throw new Exception($"{csvName}의 stageId {stageId}가 {stagesCsv.name}에 존재하지 않음");
    }

    private void CsvInspection()
    {
        CsvInspection(stagesCsv, nameof(stagesCsv));
        CsvInspection(stageEnemiesCsv, nameof(stageEnemiesCsv));
    }

    private static void CsvInspection(TextAsset csvAsset, string fieldName)
    {
        if (csvAsset == null)
        {
            throw new Exception($"StageDatabase의 {fieldName}에 CSV 파일이 연결되지 않았습니다.");
        }
    }

    private void ClearLoadData()
    {
        stages.Clear();
        isInitialized = false;
    }
}