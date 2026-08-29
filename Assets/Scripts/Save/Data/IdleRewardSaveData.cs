using System;
using System.Collections.Generic;

// [0823 추가]
// 방치 보상 기준 시간과 소수점 잔여 보상을 저장함
[Serializable]
public sealed class IdleRewardSaveData
{
    // 마지막으로 방치 보상을 정상 수령한 UTC Unix Time
    public long LastClaimedAtUnixTime { get; set; }

    // 보상 ID별 지급 후 남은 소수점 보상량
    public List<IdleRewardRemainderSaveData> Remainders { get; set; } = new();
}

// 보상 ID 하나에 대응되는 소수점 잔여 보상 저장 데이터
[Serializable]
public sealed class IdleRewardRemainderSaveData
{
    // CSV에서 사용하는 보상 ID
    public string RewardId { get; set; }

    // 이전 방치 보상 계산에서 정수 지급 후 남은 소수점 보상량
    public decimal Amount { get; set; }
}