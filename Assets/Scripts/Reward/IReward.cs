using UnityEngine;

/// <summary>
/// 보상으로 제공될 모든 객체가 상속받을 인터페이스. <br/>
/// 지급될 아이템 정보 저장 및 보상 제공 함수 구현 강제
/// </summary>
public interface IReward
{
    string RewardID { get; }    // CSV의 ResourceId
    float RewardValue { get; }  // CSV의 RewardPerSecond

    void GiveReward(int amount);    // 캐스팅해 사용할 보상 지급 함수
}