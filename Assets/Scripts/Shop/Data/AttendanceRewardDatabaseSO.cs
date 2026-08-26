using System.Collections.Generic;
using UnityEngine;

// 7일 출석 보상 순서와 보상을 분리해 관리하는 SO임
[CreateAssetMenu(fileName = "AttendanceRewardDatabase", menuName = "Game Data/Shop/Attendance Reward Database")]
public sealed class AttendanceRewardDatabaseSO : ScriptableObject
{
    [SerializeField] private List<ShopRewardEntry> rewards = new();

    public IReadOnlyList<ShopRewardEntry> Rewards => rewards;

    // 리소스 에셋이 없을 때 기능 확인용 7일 보상을 생성함
    public static AttendanceRewardDatabaseSO CreateDevelopmentDatabase()
    {
        AttendanceRewardDatabaseSO database = CreateInstance<AttendanceRewardDatabaseSO>();
        database.name = "RuntimeDevelopmentAttendanceRewardDatabase";
        database.rewards = new List<ShopRewardEntry>
        {
            ShopRewardEntry.CreateCurrency(CurrencyType.GOLD, 300),
            ShopRewardEntry.CreateCurrency(CurrencyType.EXP, 150),
            ShopRewardEntry.CreateCurrency(CurrencyType.UPGRADE, 25),
            ShopRewardEntry.CreateCurrency(CurrencyType.GEM, 10),
            ShopRewardEntry.CreateCurrency(CurrencyType.GOLD, 700),
            ShopRewardEntry.CreateCurrency(CurrencyType.UPGRADE, 50),
            ShopRewardEntry.CreateCurrency(CurrencyType.GEM, 30)
        };
        return database;
    }
}
