using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 클리어 시 설정해둔 보상 테이블.CSV 파일을 읽어와 알맞는 보상을 지급하는 클래스.
/// </summary>
public class StageClearRewardTest : MonoBehaviour
{
    [Header("Setting Reward")]
    [SerializeField] private TextAsset rewardCSVData;    // 보상 테이블로 사용할 CSV 데이터 연결

    // 스테이지 단계별 보상 테이블 저장용 딕셔너리 | Key: 스테이지 번호, Value: CSV에서 받아온 보상 테이블 데이터
    private Dictionary<int, StageRewardData> stageRewards = new Dictionary<int, StageRewardData>();

    private void Start()
    {
        stageRewards = RewardCSVParser.Parse(rewardCSVData);
    }

    // 스테이지 보상 딕셔너리 기반 보상 지급 함수
    public void GiveStageClearReward(int stageNum)
    {
        if (stageRewards.TryGetValue(stageNum, out StageRewardData stageRewardData))
        {
            foreach (KeyValuePair<string, IReward> reward in stageRewardData.Rewards)
            {
                int amount = (int)reward.Value.RewardValue;

                if (amount > 0)
                {
                    reward.Value.GiveReward(amount);
                }
            }
            Debug.Log($"스테이지 {stageNum}의 클리어 보상을 획득했습니다.");
        }
        else
        {
            Debug.Log($"스테이지 {stageNum}의 클리어 보상 데이터가 존재하지 않습니다.");
        }
    }

    // 강제로 스테이지 1 클리어 보상 지급 함수 (스테이지 단계에 맞는 보상 지급 잘 되는지 확인용)
    public void TestGiveRewardButtonClick()
    {
        GiveStageClearReward(1);
    }
}