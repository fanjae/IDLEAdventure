using UnityEngine;

/// <summary>
/// 재화 보상 클래스. <br/>
/// CSv를 받아올 때 문자열을 재화 종류 열거형으로 변환하는 과정을 통해 CSV의 ResourceID 값을 CurrencyType으로 받아오고
/// 이를 바탕으로 타입에 맞는 보상을 제공.
/// </summary>
public class CurrencyReward : IReward
{
    private float rewardValue;    // 재화 지급 량
    private CurrencyType type;    // 재화 종류

    // 프로퍼티
    public string RewardID => type.ToString();    // 재화 종류 열거형을 그대로 문자열 ID값으로 저장
    public float RewardValue => rewardValue; 
    public CurrencyType Type => type;

    public CurrencyReward(CurrencyType type, float rewardValue)
    {
        this.type = type;
        this.rewardValue = rewardValue;
    }
    
    public void GiveReward(int amount)
    {
        CurrencyManager.Instance.AddCurrency(type, amount);
    }
}