using System;
using System.Collections.Generic;

// 상점 구매 제한과 출석 수령 상태를 저장함
[Serializable]
public sealed class ShopSaveData
{
    // 한 번만 구매 가능한 상품 ID 목록임
    public List<string> PurchasedOnceProductIds { get; set; } = new();

    // 일일 제한 상품의 당일 구매 횟수 목록임
    public List<ShopPurchaseCountSaveEntry> DailyPurchaseCounts { get; set; } = new();

    // 일일 구매 제한을 마지막으로 초기화한 UTC 날짜임
    public string LastDailyResetDate { get; set; }

    // 미구매 패키지 안내를 나중에 보기로 넘긴 UTC 날짜임
    public string PackageNoticeDismissDate { get; set; }

    // 해당 날짜에 안내를 숨긴 미구매 패키지 상품 ID 목록임
    public List<string> DismissedPackageNoticeProductIds { get; set; } = new();

    // 해당 날짜에 나중에 보기를 선택해 모든 패키지 안내를 숨겼는지 여부임
    public bool ArePackageNoticesDismissedForToday { get; set; }

    // 이전 순차 출석 방식에서 마지막으로 수령한 UTC 날짜임
    public string LastAttendanceClaimDate { get; set; }

    // 이전 순차 출석 방식에서 받은 전체 횟수임. 기존 저장 이관에만 사용함
    public int AttendanceClaimCount { get; set; }

    // 출석 보상 1일차가 열린 UTC 날짜임
    public string AttendanceCycleStartDate { get; set; }

    // 수령 완료한 출석 보상 일차 인덱스 목록임
    public List<int> ClaimedAttendanceRewardIndices { get; set; } = new();
}

// 상품 ID와 당일 구매 횟수를 저장할 항목임
[Serializable]
public sealed class ShopPurchaseCountSaveEntry
{
    public string ProductId { get; set; }
    public int Count { get; set; }
}
