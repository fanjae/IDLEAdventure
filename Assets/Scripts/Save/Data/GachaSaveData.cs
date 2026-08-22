using System;
using System.Collections.Generic;

[Serializable]
public sealed class GachaSaveData
{
    // 천장을 배너 그룹별로 저장함
    public List<GachaBannerProgressSaveData> BannerProgresses { get; set; } = new();
}

[Serializable]
public sealed class GachaBannerProgressSaveData
{
    // 같은 천장을 공유하는 배너 그룹 ID임
    public string PityGroupId { get; set; }

    // 마지막 2티어 이후 진행한 소환 횟수임
    public int PullCountSinceTier2 { get; set; }

    // 통계 및 검증용 누적 소환 횟수임
    public int TotalPullCount { get; set; }
}
