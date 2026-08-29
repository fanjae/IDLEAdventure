using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 실제 방치 보상 계산 및 제공하는 클래스.
/// </summary>
public class IdleReward : MonoBehaviour
{
    // 방치 보상 데이터 세팅
    [Header("Idle Reward Setting")]
    [SerializeField] private TextAsset rewardCSVData;       // 방치 보상 데이터 테이블.
    
    [Tooltip("X: Hours, Y: Minutes, Z: Seconds")]
    [SerializeField] private Vector3Int maxIdleTimeSetting = new Vector3Int(0, 1, 40);
    private float maxIdleTime;    // 최대 방치 시간.
    
    // 스테이지 번호 별 보상 테이블 저장 딕셔너리.
    private Dictionary<int, StageRewardData> stageRewards = new Dictionary<int, StageRewardData>();
    // 보상 지급 후 남겨질 보상 저장 딕셔너리.
    private Dictionary<string, decimal> leftRewards = new Dictionary<string, decimal>();

    // 이벤트
    public event Action OnGetIdleReward;

    // 프로퍼티
    public float MaxIdleTime => maxIdleTime;

    private readonly StageProgressController stageProgressController = new();

    private void Awake()
    {
        SetMaxIdleTime();
    }

    private void Start()
    {
        // 연결한 CSV 파일을 파싱하여 스테이지별 방치 보상 데이터 저장
        stageRewards = RewardCSVParser.Parse(rewardCSVData);

        // 보상 데이터가 정상적으로 생성되지 않은 경우 이후 보상 처리를 진행하지 않음
        if (stageRewards == null)
        {
            Debug.LogError("[IdleReward] 방치 보상 데이터를 불러오지 못했습니다.");
            return;
        }

        GameSaveData saveData = SaveManager.Instance.CurrentData;

        // 저장된 방치 보상 상태를 현재 런타임 데이터로 복원
        LoadSaveData(saveData);

        // 최초 실행 시 현재 시간을 방치 보상 기준 시간으로 설정
        if (saveData.IdleReward.LastClaimedAtUnixTime <= 0)
        {
            saveData.IdleReward.LastClaimedAtUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveManager.Instance.Save();
        }
    }

    // 마지막 방치 보상 수령 시간을 반환
    public DateTime GetLastRewardTime()
    {
        // 정식 저장 데이터에서 마지막 방치 보상 수령 시간 확인
        long savedTime = SaveManager.Instance.CurrentData.IdleReward.LastClaimedAtUnixTime;

        // 아직 저장된 시간이 없는 경우 현재 시간을 반환하여 최초 실행 보상이 발생하지 않도록 처리
        if (savedTime <= 0)
        {
            return DateTime.UtcNow;
        }

        // 저장된 UTC Unix Time을 방치 시간 계산에 사용할 DateTime으로 변환
        return DateTimeOffset.FromUnixTimeSeconds(savedTime).UtcDateTime;
    }
    // 버튼 클릭 시 보상 획득 함수.
    public void OnClickIdleRewardButton()
    {
        // 최대 스테이지 번호도 받아와서 예외처리 추가 해주는 게 좋아보임.
        if (stageProgressController.HighestClearedStageId <= 0) return;

        // 보상 기준 시간.
        DateTime lastTime = GetLastRewardTime();

        // 보상 받을 방치된 시간.
        float currentIdleTime = IdleCalculator.GetRewardTime(lastTime, maxIdleTime);

        // 스테이지별 보상 데이터 딕셔너리에서 특정 스테이지 번호가 있는지 확인 및 해당 스테이지의 보상 데이터 반환
        if (stageRewards.TryGetValue(stageProgressController.HighestClearedStageId, out StageRewardData stageData))
        {
            // 보상 id 별 계산
            Dictionary<string, RewardResult> rewards =
                IdleCalculator.CalculateRewards(currentIdleTime, maxIdleTime, stageData, leftRewards);

            // 계산된 보상량 중에 증가한 값이 존재하는지 확인.
            if (rewards.Any(reward => reward.Value.FinalReward > 0))
            {
                // 스테이지에 맞게 계산된 보상을 id 별로 지급.
                foreach (KeyValuePair<string, RewardResult> reward in rewards)
                {
                    // 순회 중인 데이터 id 확인.
                    string id = reward.Key;

                    // 순회 중인 데이터 보상량 확인.
                    int amount = reward.Value.FinalReward;

                    // 순회중인 데이터 id를 Key로 갖는 남은 보상량 확인.
                    leftRewards[id] = reward.Value.LeftoverReward;

                    // 보상량이 존재하고, 해당 id를 Key로 갖는 보상이 존재한다면 보상 지급.
                    if (amount > 0 && stageData.Rewards.ContainsKey(id))
                    {
                        stageData.Rewards[id].GiveReward(amount);
                    }
                }
                // 현재 시간을 기준 시간으로 저장.
                SaveManager saveManager = SaveManager.Instance;

                // 현재 시간을 새로운 방치 보상 기준 시간으로 저장
                saveManager.CurrentData.IdleReward.LastClaimedAtUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // 현재 남아있는 소수점 보상 상태를 저장 데이터에 반영
                WriteSaveData(saveManager.CurrentData);

                // 방치 보상 기준 시간, 잔여 보상과 함께 실제 지급된 재화 상태까지 저장
                saveManager.Save();

                OnGetIdleReward?.Invoke();

                if (AchievementManager.TryGetExistingInstance(out AchievementManager achievementManager) &&
                    achievementManager.IsInitialized)
                {
                    achievementManager.RecordIdleRewardClaim();
                }
            }
        }
    }

    // 저장된 방치 보상 상태를 복원
    private void LoadSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.IdleReward ??= new IdleRewardSaveData();

        leftRewards.Clear();

        if (saveData.IdleReward.Remainders == null)
        {
            return;
        }

        foreach (IdleRewardRemainderSaveData remainder in saveData.IdleReward.Remainders)
        {
            if (remainder == null || string.IsNullOrWhiteSpace(remainder.RewardId))
            {
                continue;
            }

            leftRewards[remainder.RewardId] = Math.Max(0m, remainder.Amount);
        }
    }

    // 현재 방치 보상 상태를 저장 데이터에 반영
    private void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.IdleReward ??= new IdleRewardSaveData();

        saveData.IdleReward.Remainders.Clear();

        foreach (KeyValuePair<string, decimal> pair in leftRewards)
        {
            saveData.IdleReward.Remainders.Add(new IdleRewardRemainderSaveData
            {
                RewardId = pair.Key,
                Amount = pair.Value
            });
        }
    }

    public Dictionary<string, int> GetExpectedRewards()
    {
        Dictionary<string, int> expectedRewards = new Dictionary<string, int>();

        if (stageProgressController.HighestClearedStageId <= 0) return expectedRewards;

        // 기준 시간과 방치 시간 계산
        DateTime lastTime = GetLastRewardTime();
        float currentIdleTime = IdleCalculator.GetRewardTime(lastTime, maxIdleTime);

        // 현재 스테이지의 보상 데이터가 있다면 계산
        if (stageRewards.TryGetValue(stageProgressController.HighestClearedStageId, out StageRewardData stageData))
        {
            Dictionary<string, RewardResult> rewards =
                IdleCalculator.CalculateRewards(currentIdleTime, maxIdleTime, stageData, leftRewards);

            foreach (var reward in rewards)
            {
                if (reward.Value.FinalReward > 0)
                {
                    expectedRewards[reward.Key] = reward.Value.FinalReward;
                }
            }
        }

        return expectedRewards;
    }

    private void SetMaxIdleTime()
    {
        maxIdleTime = (maxIdleTimeSetting.x * 3600.0f)
                    + (maxIdleTimeSetting.y * 60.0f)
                    + maxIdleTimeSetting.z;
    }
}
