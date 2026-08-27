using System;
using System.Collections.Generic;
using UnityEngine;

// 가챠에서 사용할 영웅별 티어를 전투 영웅 데이터와 분리해 관리함
[CreateAssetMenu(fileName = "GachaHeroTierDatabase", menuName = "Game Data/Gacha/Hero Tier Database")]
public sealed class GachaHeroTierDatabaseSO : ScriptableObject
{
    [SerializeField] private List<GachaHeroTierEntry> entries = new();

    private Dictionary<string, GachaRarity> tierByHeroId;

    // 영웅 ID의 가챠 티어를 반환함
    public bool TryGetRarity(string heroId, out GachaRarity rarity)
    {
        EnsureInitialized();
        return tierByHeroId.TryGetValue(heroId, out rarity);
    }

    // 영웅 에셋의 가챠 티어를 반환함
    public bool TryGetRarity(HeroData heroData, out GachaRarity rarity) =>
        TryGetRarity(heroData != null ? heroData.UnitID : string.Empty, out rarity);

    // 리소스 에셋이 없을 때 기본 가챠 배너와 함께 사용할 티어 데이터를 생성함
    public static GachaHeroTierDatabaseSO CreateDevelopmentDatabase(HeroDatabaseSO heroDatabase)
    {
        if (heroDatabase == null)
        {
            throw new ArgumentNullException(nameof(heroDatabase));
        }

        GachaHeroTierDatabaseSO database = CreateInstance<GachaHeroTierDatabaseSO>();
        database.name = "RuntimeDevelopmentGachaHeroTierDatabase";
        foreach (HeroData heroData in heroDatabase.Heroes)
        {
            if (heroData == null)
            {
                continue;
            }

            GachaRarity rarity = heroData.UnitID == "Hero_Healer" || heroData.UnitID == "Hero_Ranger_A"
                ? GachaRarity.Tier2
                : GachaRarity.Tier1;
            database.entries.Add(new GachaHeroTierEntry(heroData, rarity));
        }

        return database;
    }

    // Inspector 목록을 영웅 ID 기준 조회용 사전으로 변환함
    private void EnsureInitialized()
    {
        if (tierByHeroId != null)
        {
            return;
        }

        tierByHeroId = new Dictionary<string, GachaRarity>(StringComparer.Ordinal);
        foreach (GachaHeroTierEntry entry in entries)
        {
            if (entry == null || entry.HeroData == null || string.IsNullOrWhiteSpace(entry.HeroData.UnitID))
            {
                continue;
            }

            if (!tierByHeroId.TryAdd(entry.HeroData.UnitID, entry.Rarity))
            {
                Debug.LogError($"[GachaHeroTierDatabaseSO] 중복 영웅 티어 설정 있음: {entry.HeroData.UnitID}", this);
            }
        }
    }
}

// 영웅 하나의 가챠 티어를 정의함
[Serializable]
public sealed class GachaHeroTierEntry
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private GachaRarity rarity;

    public HeroData HeroData => heroData;
    public GachaRarity Rarity => rarity;

    public GachaHeroTierEntry(HeroData heroData, GachaRarity rarity)
    {
        this.heroData = heroData;
        this.rarity = rarity;
    }
}
