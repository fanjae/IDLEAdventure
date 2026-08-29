using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GachaDatabase", menuName = "Game Data/Gacha/Database")]
public sealed class GachaDatabaseSO : ScriptableObject
{
    [SerializeField] private List<GachaBannerDataSO> banners = new();
    [SerializeField] private GachaHeroTierDatabaseSO heroTierDatabase;

    private Dictionary<string, GachaBannerDataSO> bannerMap;

    public IReadOnlyList<GachaBannerDataSO> Banners => banners;
    public GachaHeroTierDatabaseSO HeroTierDatabase => heroTierDatabase;

    // 배너 ID로 소환 데이터를 조회함
    public bool TryGetBanner(string bannerId, out GachaBannerDataSO banner)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(bannerId))
        {
            banner = null;
            return false;
        }

        return bannerMap.TryGetValue(bannerId, out banner);
    }

    // Resources 에셋이 없을 때 개발 확인용 기본 배너를 제공함
    public static GachaDatabaseSO CreateDevelopmentDatabase(HeroDatabaseSO heroDatabase)
    {
        if (heroDatabase == null)
        {
            throw new ArgumentNullException(nameof(heroDatabase));
        }

        GachaDatabaseSO database = CreateInstance<GachaDatabaseSO>();
        database.name = "RuntimeDevelopmentGachaDatabase";
        database.banners = new List<GachaBannerDataSO> { GachaBannerDataSO.CreateDevelopmentBanner(heroDatabase) };
        database.heroTierDatabase = GachaHeroTierDatabaseSO.CreateDevelopmentDatabase(heroDatabase);
        return database;
    }

    // Inspector 목록을 조회용 사전으로 변환함
    private void EnsureInitialized()
    {
        if (bannerMap != null)
        {
            return;
        }

        bannerMap = new Dictionary<string, GachaBannerDataSO>(StringComparer.Ordinal);

        foreach (GachaBannerDataSO banner in banners)
        {
            if (banner == null || string.IsNullOrWhiteSpace(banner.BannerId))
            {
                continue;
            }

            if (!bannerMap.TryAdd(banner.BannerId, banner))
            {
                Debug.LogError($"[GachaDatabaseSO] 중복 배너 ID 있음: {banner.BannerId}", banner);
            }
        }
    }
}
