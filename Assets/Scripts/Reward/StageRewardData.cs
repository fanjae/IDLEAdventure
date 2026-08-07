using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSV에서 보상 데이터를 받아와 저장해두는 클래스. <br/>
/// 기존 클래스에 선언해둔 형태에서 분리. <br/>
/// 보상 관리 딕셔너리를 기존 재화 종류를 Key로, 획득량을 Value로 사용하던 방식에서
/// 재화 ID값을 Key로, 보상용 객체를 Value로 변경
/// </summary>
public class StageRewardData
{
    private Dictionary<string, IReward> rewards = new Dictionary<string, IReward>();

    public Dictionary<string, IReward> Rewards => rewards;

    public void GetReward(IReward reward)
    {
        rewards[reward.RewardID] = reward;
    }
}