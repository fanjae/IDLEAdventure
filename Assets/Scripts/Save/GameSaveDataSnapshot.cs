using System;
using System.Collections.Generic;

// 비동기 저장 중 원본 저장 데이터가 변경되지 않도록 독립된 저장 데이터를 생성함
public static class GameSaveDataSnapshot
{
    // 현재 저장 데이터를 비동기 직렬화용 독립 데이터로 복사함
    public static GameSaveData Create(GameSaveData source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        // 저장에 사용하는 모든 하위 데이터를 원본과 참조가 겹치지 않도록 복사
        GameSaveData snapshot = new()
        {
            Version = source.Version,
            SavedAtUnixTime = source.SavedAtUnixTime,
            Inventory = CloneInventory(source.Inventory),
            Equipment = CloneEquipment(source.Equipment),
            Heroes = CloneHeroes(source.Heroes),
            Currency = CloneCurrency(source.Currency),
            Resonance = CloneResonance(source.Resonance),
            StageProgress = CloneStageProgress(source.StageProgress),
            Quest = CloneQuest(source.Quest),
            Formation = CloneFormation(source.Formation),
            Gacha = CloneGacha(source.Gacha),
            Achievements = CloneAchievements(source.Achievements),
            IdleReward = CloneIdleReward(source.IdleReward),
            Shop = CloneShop(source.Shop),
            PlayerPosition = ClonePlayerPosition(source.PlayerPosition),
            FieldObjects = CloneFieldObjects(source.FieldObjects),
            Option = CloneOption(source.Option)
        };

        return snapshot;
    }

    // 인벤토리 아이템과 보유 장비 목록 복사
    private static InventorySaveData CloneInventory(InventorySaveData source)
    {
        if (source == null)
        {
            return new InventorySaveData();
        }

        InventorySaveData clone = new();

        if (source.Items != null)
        {
            clone.Items = source.Items.ConvertAll(item => new InventoryItemSaveData { ItemId = item.ItemId, Quantity = item.Quantity });
        }

        if (source.Equipments != null)
        {
            clone.Equipments = source.Equipments.ConvertAll(equipment => new OwnedEquipmentSaveData { InstanceId = equipment.InstanceId, EquipmentId = equipment.EquipmentId, EnhancementLevel = equipment.EnhancementLevel });
        }

        return clone;
    }

    // 클래스별 장비 장착 상태와 슬롯 정보 복사
    private static EquipmentSaveData CloneEquipment(EquipmentSaveData source)
    {
        if (source == null)
        {
            return new EquipmentSaveData();
        }

        EquipmentSaveData clone = new();

        if (source.Classes == null)
        {
            return clone;
        }

        foreach (ClassEquipmentSaveData classData in source.Classes)
        {
            ClassEquipmentSaveData classClone = new()
            {
                HeroClass = classData.HeroClass,
                EquippedInstanceIds = classData.EquippedInstanceIds != null ? new Dictionary<EquipmentSlotType, string>(classData.EquippedInstanceIds) : new Dictionary<EquipmentSlotType, string>()
            };

            clone.Classes.Add(classClone);
        }

        return clone;
    }

    // 보유 영웅 ID와 레벨 정보 복사
    private static HeroSaveData CloneHeroes(HeroSaveData source)
    {
        if (source == null)
        {
            return new HeroSaveData();
        }

        HeroSaveData clone = new();

        if (source.OwnedHeroes != null)
        {
            clone.OwnedHeroes = source.OwnedHeroes.ConvertAll(hero => new OwnedHeroSaveData { HeroId = hero.HeroId, Level = hero.Level });
        }

        return clone;
    }

    // 현재 보유 재화 값 복사
    private static CurrencySaveData CloneCurrency(CurrencySaveData source)
    {
        if (source == null)
        {
            return new CurrencySaveData();
        }

        return new CurrencySaveData { Gold = source.Gold, Exp = source.Exp, Upgrade = source.Upgrade, Gem = source.Gem };
    }

    // 공명 슬롯에 등록된 영웅 ID 목록 복사
    private static ResonanceSaveData CloneResonance(ResonanceSaveData source)
    {
        ResonanceSaveData clone = new();

        if (source?.ResonanceSlotHeroIds != null)
        {
            clone.ResonanceSlotHeroIds = new List<string>(source.ResonanceSlotHeroIds);
        }

        return clone;
    }

    // 현재 스테이지와 클리어 진행도 복사
    private static StageProgressSaveData CloneStageProgress(StageProgressSaveData source)
    {
        if (source == null)
        {
            return new StageProgressSaveData();
        }

        return new StageProgressSaveData
        {
            CurrentStageId = source.CurrentStageId,
            HighestClearedStageId = source.HighestClearedStageId,
            DefeatedStageIds = source.DefeatedStageIds != null ? new List<int>(source.DefeatedStageIds) : new List<int>()
        };
    }

    // 메인 퀘스트와 서브 퀘스트 진행 상태 복사
    private static QuestSaveData CloneQuest(QuestSaveData source)
    {
        if (source == null)
        {
            return new QuestSaveData();
        }

        return new QuestSaveData
        {
            CurrentMainQuestId = source.CurrentMainQuestId,
            AcceptedSubQuestIds = source.AcceptedSubQuestIds != null ? new List<int>(source.AcceptedSubQuestIds) : new List<int>(),
            ClearedSubQuestIds = source.ClearedSubQuestIds != null ? new List<int>(source.ClearedSubQuestIds) : new List<int>()
        };
    }

    // 영웅 배치 슬롯 정보 복사
    private static FormationSaveData CloneFormation(FormationSaveData source)
    {
        FormationSaveData clone = new();

        if (source?.Slots != null)
        {
            clone.Slots = source.Slots.ConvertAll(slot => new FormationSlotSaveData { SlotNumber = slot.SlotNumber, HeroId = slot.HeroId });
        }

        return clone;
    }

    // 배너별 가챠 천장 진행도 복사
    private static GachaSaveData CloneGacha(GachaSaveData source)
    {
        GachaSaveData clone = new();

        if (source?.BannerProgresses != null)
        {
            clone.BannerProgresses = source.BannerProgresses.ConvertAll(progress => new GachaBannerProgressSaveData { PityGroupId = progress.PityGroupId, PullCountSinceTier2 = progress.PullCountSinceTier2, TotalPullCount = progress.TotalPullCount });
        }

        return clone;
    }

    // 업적 진행도와 보상 수령 상태 복사
    private static AchievementSaveData CloneAchievements(AchievementSaveData source)
    {
        if (source == null)
        {
            return new AchievementSaveData();
        }

        AchievementSaveData clone = new()
        {
            MetricValuesMigrated = source.MetricValuesMigrated,
            HasFirstLogin = source.HasFirstLogin,
            TotalGachaPulls = source.TotalGachaPulls,
            MaxClearedStage = source.MaxClearedStage,
            HasMigratedGachaPulls = source.HasMigratedGachaPulls
        };

        if (source.MetricValues != null)
        {
            clone.MetricValues = source.MetricValues.ConvertAll(metric => new AchievementMetricSaveEntry { Metric = metric.Metric, Value = metric.Value });
        }

        if (source.ClaimedAchievementIds != null)
        {
            clone.ClaimedAchievementIds = new List<string>(source.ClaimedAchievementIds);
        }

        return clone;
    }

    // 방치 보상 수령 시간과 잔여 보상 값 복사
    private static IdleRewardSaveData CloneIdleReward(IdleRewardSaveData source)
    {
        if (source == null)
        {
            return new IdleRewardSaveData();
        }

        IdleRewardSaveData clone = new() { LastClaimedAtUnixTime = source.LastClaimedAtUnixTime };

        if (source.Remainders != null)
        {
            clone.Remainders = source.Remainders.ConvertAll(remainder => new IdleRewardRemainderSaveData { RewardId = remainder.RewardId, Amount = remainder.Amount });
        }

        return clone;
    }

    // 상점 구매 제한과 출석 보상 수령 상태 복사
    private static ShopSaveData CloneShop(ShopSaveData source)
    {
        if (source == null)
        {
            return new ShopSaveData();
        }

        ShopSaveData clone = new()
        {
            LastDailyResetDate = source.LastDailyResetDate,
            PackageNoticeDismissDate = source.PackageNoticeDismissDate,
            ArePackageNoticesDismissedForToday = source.ArePackageNoticesDismissedForToday,
            LastAttendanceClaimDate = source.LastAttendanceClaimDate,
            AttendanceClaimCount = source.AttendanceClaimCount,
            AttendanceCycleStartDate = source.AttendanceCycleStartDate
        };

        if (source.PurchasedOnceProductIds != null)
        {
            clone.PurchasedOnceProductIds = new List<string>(source.PurchasedOnceProductIds);
        }

        if (source.DailyPurchaseCounts != null)
        {
            clone.DailyPurchaseCounts = source.DailyPurchaseCounts.ConvertAll(entry => new ShopPurchaseCountSaveEntry { ProductId = entry.ProductId, Count = entry.Count });
        }

        if (source.DismissedPackageNoticeProductIds != null)
        {
            clone.DismissedPackageNoticeProductIds = new List<string>(source.DismissedPackageNoticeProductIds);
        }

        if (source.ClaimedAttendanceRewardIndices != null)
        {
            clone.ClaimedAttendanceRewardIndices = new List<int>(source.ClaimedAttendanceRewardIndices);
        }

        return clone;
    }

    // 필드 플레이어의 마지막 위치 복사
    private static PlayerPositionSaveData ClonePlayerPosition(PlayerPositionSaveData source)
    {
        if (source == null)
        {
            return new PlayerPositionSaveData();
        }

        return new PlayerPositionSaveData { HasPosition = source.HasPosition, X = source.X, Y = source.Y, Z = source.Z };
    }

    // 필드에서 획득하거나 처치한 오브젝트 상태 복사
    private static FieldObjectSaveData CloneFieldObjects(FieldObjectSaveData source)
    {
        if (source == null)
        {
            return new FieldObjectSaveData();
        }

        return new FieldObjectSaveData
        {
            OpenedChestIds = source.OpenedChestIds != null ? new List<int>(source.OpenedChestIds) : new List<int>(),
            DefeatedEnemyIds = source.DefeatedEnemyIds != null ? new List<int>(source.DefeatedEnemyIds) : new List<int>()
        };
    }

    // 저장된 BGM과 효과음 설정 값 복사
    private static OptionSaveData CloneOption(OptionSaveData source)
    {
        if (source == null)
        {
            return new OptionSaveData();
        }

        return new OptionSaveData { BgmVolume = source.BgmVolume, SfxVolume = source.SfxVolume };
    }
}
