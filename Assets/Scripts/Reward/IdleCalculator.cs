using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방치 시간 계산을 위한 정적 클래스. <br/>
/// 기존 IdleTest에 한 번에 작성되어 있던 함수 중 계싼 관련 함수들 외부 클래스로 분리.
/// </summary>
public static class IdleCalculator
{
    // 보상 시간 계산 함수.
    public static float GetRewardTime(DateTime lastTime, float maxIdleTime)
    {
        // 함수가 호출되는 시간 - 기준 시간.
        TimeSpan rewardTime = DateTime.UtcNow - lastTime;
        // 계산된 시간을 초단위로 변환.
        float passedTime = (float)rewardTime.TotalSeconds;

        // 예외처리 후 반환.
        return Mathf.Clamp(passedTime, 0.0f, maxIdleTime);
    }
    // 보상 ID별 실제 보상량 계산 함수.
    public static Dictionary<string, RewardResult> CalculateRewards(
        float currentIdleTime,      // 현재 방치 시간.
        float maxIdleTime,          // 최대 방치 시간.
        StageRewardData stageData,  // 스테이지 보상 데이터.
        Dictionary<string, decimal> leftRewards)    // 이전 지급 후 남아있는 보상 시간.
    {
        // 보상 결과를 담은 딕셔너리 | Key: id, RewardResult: finalRewardAmount & leftRewardAmount.
        Dictionary<string, RewardResult> calculatedRewards = new Dictionary<string, RewardResult>();
        
        // 현재 방치 시간, 최대 방치 시간 deciaml로 형변환.
        decimal currentIdleTimeDec = (decimal)currentIdleTime;
        decimal maxIdleTimeDec = (decimal)maxIdleTime;

        // 보상 별 계산 | Key: id, IReward: 보상 객체 | 받아오는 객체: 특정 스테이지 보상 데이터.
        foreach (KeyValuePair<string, IReward> reward in stageData.Rewards)
        {
            // 보상의 id 저장
            string id = reward.Key;
            // 보상량 형변환 후 저장
            decimal rewardAmount = (decimal)reward.Value.RewardValue;
            // 남은 보상이 있는지 확인. 있다면 보상 id를 key로 받아오고, 없다면 0.
            decimal leftRewardAmount = leftRewards.ContainsKey(id) ? leftRewards[id] : 0m;

            // 최대 보상량 미리 계산
            decimal maxRewardAmount = maxIdleTimeDec * rewardAmount;
            // 최종 지급될 보상 계산. 방치 시간 * 보상량 + 남은 보상량.
            // Math.Min 함수를 통해 최대 보상량을 넘지 않도록 예외처리.
            decimal finalAmount = Math.Min(((currentIdleTimeDec * rewardAmount) + leftRewardAmount), maxRewardAmount);

            // 최종 보상량 int로 형변환.
            int finalReward = (int)finalAmount;
            // 형변환을 통해 버려지는 보상량 저장.
            decimal leftReward = finalAmount - finalReward;
            
            // 보상 id별로 계산이 끝난 보상량과 버려지는 보상량 저장.
            calculatedRewards[id] = new RewardResult(finalReward, leftReward);
        }
        // 모든 id별 보상 계산이 끝난 딕셔너리 반환.
        return calculatedRewards;
    }
}