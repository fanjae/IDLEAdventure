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
    [SerializeField] private float maxIdleTime = 100.0f;    // 최대 방치 시간. (단위: 초)
    [SerializeField] private int testClearStageNum;         // 현재는 스테이지 관리 매니저가 없기에 임의 스테이지 번호.

    // 스테이지 번호 별 보상 테이블 저장 딕셔너리.
    private Dictionary<int, StageRewardData> stageRewards = new Dictionary<int, StageRewardData>();
    // 보상 지급 후 남겨질 보상 저장 딕셔너리.
    private Dictionary<string, decimal> leftRewards = new Dictionary<string, decimal>();

    // 이벤트
    public event Action OnGetIdleReward;

    // 프로퍼티
    public float MaxIdleTime => maxIdleTime;

    private void Start()
    {
        // 연결한 CSV파일 파싱 데이터 저장.
        stageRewards = RewardCSVParser.Parse(rewardCSVData);

        // 게임 시작 시 시간 설정
        long savedTime = TestSaveManager.Instance.CurrentSaveData.LastGetIdleRewardTime;
        if (savedTime == 0)
        {
            TestSaveManager.Instance.CurrentSaveData.SetLastIdleRewardTime(DateTime.UtcNow.ToBinary());
            TestSaveManager.Instance.SaveGame();
        }
    }

    // 보상 기준 시간 반환 함수.
    public DateTime GetLastRewardTime()
    {
        // 세이브 매니저에서 저장 되어있는 기준 시간 받아오기.
        long savedTime = TestSaveManager.Instance.CurrentSaveData.LastGetIdleRewardTime;
        // 저장 데이터가 존재한다면, 해당 값을 DateTime으로 형변환, 없다면 0 반환.
        return savedTime == 0 ? DateTime.UtcNow : DateTime.FromBinary(savedTime);
    }
    // 버튼 클릭 시 보상 획득 함수.
    public void OnClickIdleRewardButton()
    {
        // 최대 스테이지 번호도 받아와서 예외처리 추가 해주는 게 좋아보임.
        if (testClearStageNum <= 0) return;

        // 보상 기준 시간.
        DateTime lastTime = GetLastRewardTime();

        // 보상 받을 방치된 시간.
        float currentIdleTime = IdleCalculator.GetRewardTime(lastTime, maxIdleTime);

        // 스테이지별 보상 데이터 딕셔너리에서 특정 스테이지 번호가 있는지 확인 및 해당 스테이지의 보상 데이터 반환
        if (stageRewards.TryGetValue(testClearStageNum, out StageRewardData stageData))
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
                DateTime currentTime = DateTime.UtcNow;
                TestSaveManager.Instance.CurrentSaveData.SetLastIdleRewardTime(currentTime.ToBinary());
                TestSaveManager.Instance.SaveGame();
                
                OnGetIdleReward?.Invoke();

                if (AchievementManager.TryGetExistingInstance(out AchievementManager achievementManager) &&
                    achievementManager.IsInitialized)
                {
                    achievementManager.RecordIdleRewardClaim();
                }
            }
        }
    }
}
