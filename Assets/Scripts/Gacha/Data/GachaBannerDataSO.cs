using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GachaBanner", menuName = "Game Data/Gacha/Banner")]
public sealed class GachaBannerDataSO : ScriptableObject
{
    [Header("식별 정보")]
    [SerializeField] private string bannerId = "Standard";
    [SerializeField] private string pityGroupId = "Standard";
    [SerializeField] private string displayName = "기본 소환";

    [Header("배너 UI")]
    [TextArea(2, 4)] [SerializeField] private string description;
    [SerializeField] private string periodText = "상시";
    [SerializeField] private bool isVisible = true;
    [SerializeField] private int displayOrder;

    [Header("배너 이미지")]
    [SerializeField] private Sprite bannerArtwork;
    [SerializeField] private Sprite tabThumbnail;

    [Header("소환 비용")]
    [Min(1)] [SerializeField] private int singleDrawGemCost = 100;
    [Min(1)] [SerializeField] private int tenDrawGemCost = 1000;

    [Header("천장")]
    [Min(1)] [SerializeField] private int tier2PityCount = 30;

    [Header("등급 확률 가중치")]
    [Min(0)] [SerializeField] private int tier1Weight = 90;
    [Min(0)] [SerializeField] private int tier2Weight = 10;

    [Header("픽업 확률")]
    [Min(1f)] [SerializeField] private float pickupWeightMultiplier = 3f;

    [Header("중복 영웅 전환 골드")]
    [Min(0)] [SerializeField] private int tier1DuplicateGold = 100;
    [Min(0)] [SerializeField] private int tier2DuplicateGold = 300;

    [Header("소환 풀")]
    [SerializeField] private List<GachaHeroPoolEntry> heroPool = new();

    public string BannerId => bannerId;
    public string PityGroupId => string.IsNullOrWhiteSpace(pityGroupId) ? bannerId : pityGroupId;
    public string DisplayName => displayName;
    public string Description => description;
    public string PeriodText => periodText;
    public bool IsVisible => isVisible;
    public int DisplayOrder => displayOrder;
    public Sprite BannerArtwork => bannerArtwork;
    public Sprite TabThumbnail => tabThumbnail;
    public int Tier2PityCount => tier2PityCount;
    public int Tier1Weight => tier1Weight;
    public int Tier2Weight => tier2Weight;
    public float PickupWeightMultiplier => pickupWeightMultiplier;
    public IReadOnlyList<GachaHeroPoolEntry> HeroPool => heroPool;

    // 소환 횟수에 맞는 보석 비용 반환함
    public int GetGemCost(int drawCount)
    {
        if (drawCount <= 0)
        {
            return 0;
        }

        return drawCount == 10 ? tenDrawGemCost : singleDrawGemCost * drawCount;
    }

    // 중복 영웅을 등급별 골드로 전환함
    public int GetDuplicateGold(GachaRarity rarity)
    {
        return rarity switch
        {
            GachaRarity.Tier1 => tier1DuplicateGold,
            GachaRarity.Tier2 => tier2DuplicateGold,
            _ => 0
        };
    }

    // 티어별 설정 가중치를 반환함
    public int GetTierWeight(GachaRarity rarity)
    {
        return rarity switch
        {
            GachaRarity.Tier1 => tier1Weight,
            GachaRarity.Tier2 => tier2Weight,
            _ => 0
        };
    }

    // 초기 테스트용 기본 배너를 영웅 에셋 참조로 구성함
    public static GachaBannerDataSO CreateDevelopmentBanner(HeroDatabaseSO heroDatabase)
    {
        if (heroDatabase == null)
        {
            throw new ArgumentNullException(nameof(heroDatabase));
        }

        GachaBannerDataSO banner = CreateInstance<GachaBannerDataSO>();
        banner.name = "RuntimeDevelopmentGachaBanner";
        banner.heroPool = new List<GachaHeroPoolEntry>
        {
            new(FindDevelopmentHero(heroDatabase, "Hero_Tanker"), GachaRarity.Tier1, false),
            new(FindDevelopmentHero(heroDatabase, "Hero_Ranger"), GachaRarity.Tier1, false),
            new(FindDevelopmentHero(heroDatabase, "Hero_Healer"), GachaRarity.Tier2, true),
            new(FindDevelopmentHero(heroDatabase, "Hero_Tanker_A"), GachaRarity.Tier1, false),
            new(FindDevelopmentHero(heroDatabase, "Hero_Tanker_B"), GachaRarity.Tier1, false),
            new(FindDevelopmentHero(heroDatabase, "Hero_Ranger_A"), GachaRarity.Tier2, false),
            new(FindDevelopmentHero(heroDatabase, "Hero_Ranger_B"), GachaRarity.Tier1, false),
            new(FindDevelopmentHero(heroDatabase, "Hero_Healer_A"), GachaRarity.Tier2, false),
            new(FindDevelopmentHero(heroDatabase, "Hero_Healer_B"), GachaRarity.Tier1, false)
        };
        return banner;
    }

    // 개발용 기본 풀에 필요한 영웅 에셋을 조회함
    private static HeroData FindDevelopmentHero(HeroDatabaseSO heroDatabase, string heroId)
    {
        if (heroDatabase.TryGetHero(heroId, out HeroData heroData))
        {
            return heroData;
        }

        throw new InvalidOperationException($"개발용 가챠 영웅 데이터 없음: {heroId}");
    }
}

[Serializable]
public sealed class GachaHeroPoolEntry
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private GachaRarity rarity;
    [SerializeField] private bool isPickup;

    public HeroData HeroData => heroData;
    public string HeroId => heroData != null ? heroData.UnitID : string.Empty;
    public GachaRarity Rarity => rarity;
    public bool IsPickup => isPickup;

    // 런타임 기본 배너 데이터를 만들 때만 사용함
    public GachaHeroPoolEntry(HeroData heroData, GachaRarity rarity, bool isPickup)
    {
        this.heroData = heroData;
        this.rarity = rarity;
        this.isPickup = isPickup;
    }
}
