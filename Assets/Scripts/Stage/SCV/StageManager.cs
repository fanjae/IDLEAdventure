using System;
using UnityEngine;

public sealed class StageManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private FormationManager formationManager;
    [SerializeField] private StageEnemySpawner stageEnemySpawner;

    [Header("현재 진행 스테이지")]
    [SerializeField, Min(1)] private int currentStageId = 1;

    private bool isStagePrepared;
    private bool isResultHandled;

    public int CurrentStageId => currentStageId;
    public bool IsStagePrepared => isStagePrepared;

    public event Action<int> OnStagePrepared;
    public event Action<int> OnStageCleared;
    public event Action<int> OnStageFailed;

    private void Awake()
    {
        if (formationManager == null)
        {
            throw new Exception("StageManager의 Formation Manager가 연결되어 있지 않습니다.");
        }

        if (stageEnemySpawner == null)
        {
            throw new Exception("StageManager의 Stage Enemy Spawner가 연결되어 있지 않습니다.");
        }
    }

    private void Start()
    {
        PrepareCurrentStage();
    }

    public void PrepareCurrentStage()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleRunning)
        {
            Debug.LogWarning("전투 중에는 스테이지를 다시 준비할 수 없습니다.");
            return;
        }

        stageEnemySpawner.LoadStage(currentStageId);

        isStagePrepared = true;
        isResultHandled = false;

        OnStagePrepared?.Invoke(currentStageId);

        Debug.Log($"{currentStageId}번 스테이지 준비 완료");
    }

    public void RequestBattleStart()
    {
        if (!isStagePrepared)
        {
            Debug.LogWarning("스테이지가 준비되지 않았습니다.");
            return;
        }

        if (formationManager.PlacedHeroCount <= 0)
        {
            Debug.LogWarning("배치된 영웅이 없습니다.");
            return;
        }

        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없습니다.");
            return;
        }

        if (BattleManager.Instance.IsBattleRunning)
        {
            Debug.LogWarning("이미 전투가 진행 중입니다.");
            return;
        }

        BattleManager.Instance.StartBattle();

        if (!BattleManager.Instance.IsBattleRunning)
        {
            Debug.LogWarning("BattleManager에서 전투가 시작되지 않았습니다.");
            return;
        }

        Debug.Log($"{currentStageId}번 스테이지 전투 시작");
    }

    public void HandleStageClear()
    {
        if (!CanHandleResult())
        {
            return;
        }

        int clearedStageId = currentStageId;

        isResultHandled = true;
        isStagePrepared = false;

        currentStageId++;

        OnStageCleared?.Invoke(clearedStageId);

        Debug.Log($"{clearedStageId}번 스테이지 클리어 → 다음 진행 스테이지: {currentStageId}");
    }

    public void HandleStageFail()
    {
        if (!CanHandleResult())
        {
            return;
        }

        int failedStageId = currentStageId;

        isResultHandled = true;
        isStagePrepared = false;

        OnStageFailed?.Invoke(failedStageId);

        Debug.Log($"{failedStageId}번 스테이지 패배 → 현재 진행 스테이지 유지: {currentStageId}");
    }

    private bool CanHandleResult()
    {
        if (!isStagePrepared)
        {
            Debug.LogWarning("준비된 스테이지가 없습니다.");
            return false;
        }

        if (isResultHandled)
        {
            Debug.LogWarning("이미 스테이지 결과가 처리되었습니다.");
            return false;
        }

        return true;
    }
}