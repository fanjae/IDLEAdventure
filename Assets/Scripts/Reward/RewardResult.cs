using UnityEngine;

/// <summary>
/// 계산된 보상 결과와 보상 정산 후 남은 시간을 담아두는 클래스. <br/>
/// 각각의 결과의 사용처마다 계산을 따로하지 않기 위함. <br/>
/// 더 좋은 방식이 있을까? 편의를 위해 클래스를 너무 늘린 것 같은데...
/// </summary>
public class RewardResult
{
    private int finalReward;    // 지급할 보상 정보
    private decimal leftoverReward;    // 정산 후 남은 보상 (0.5 Upgrade, 0.5 EquipBox, ...)

    public int FinalReward => finalReward;
    public decimal LeftoverReward => leftoverReward;

    public RewardResult(int finalReward, decimal leftoverReward)
    {
        this.finalReward = finalReward;
        this.leftoverReward = leftoverReward;
    }
}