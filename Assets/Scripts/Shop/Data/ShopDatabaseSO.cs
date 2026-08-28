using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 상점 상품 목록과 출석 보상 SO 참조를 제공하는 SO임
[CreateAssetMenu(fileName = "ShopDatabase", menuName = "Game Data/Shop/Database")]
public sealed class ShopDatabaseSO : ScriptableObject
{
    [SerializeField] private List<ShopProductSO> products = new();
    [SerializeField] private AttendanceRewardDatabaseSO attendanceRewardDatabase;

    private Dictionary<string, ShopProductSO> productMap;

    public IReadOnlyList<ShopProductSO> Products => products;
    public AttendanceRewardDatabaseSO AttendanceRewardDatabase => attendanceRewardDatabase;

    // 상품 ID를 기준으로 상품을 조회함
    public bool TryGetProduct(string productId, out ShopProductSO product)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(productId))
        {
            product = null;
            return false;
        }

        return productMap.TryGetValue(productId, out product);
    }

    // 설정 오류를 초기화 전에 확인함
    public bool TryValidate(out string errorMessage)
    {
        HashSet<string> ids = new(StringComparer.Ordinal);
        foreach (ShopProductSO product in products)
        {
            if (product == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(product.ProductId))
            {
                errorMessage = $"상점 상품 ID 비어 있음: {product.name}";
                return false;
            }

            if (!ids.Add(product.ProductId))
            {
                errorMessage = $"상점 상품 ID 중복됨: {product.ProductId}";
                return false;
            }

            if (product.PriceType == ShopPriceType.Currency &&
                (product.PriceCurrency == CurrencyType.None || product.PriceCurrency >= CurrencyType.Length || product.PriceAmount <= 0))
            {
                errorMessage = $"상점 상품 비용 설정 오류: {product.ProductId}";
                return false;
            }

            if (!TryValidateRewards(product.Rewards))
            {
                errorMessage = $"상점 상품 보상 설정 오류: {product.ProductId}";
                return false;
            }
        }

        if (attendanceRewardDatabase == null)
        {
            errorMessage = "출석 보상 데이터베이스 비어 있음";
            return false;
        }

        if (attendanceRewardDatabase.Rewards == null || attendanceRewardDatabase.Rewards.Count == 0 ||
            !TryValidateRewards(attendanceRewardDatabase.Rewards))
        {
            errorMessage = "출석 보상 설정 오류";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    // 리소스 에셋이 없을 때 런타임 기능 확인용 데이터베이스를 생성함
    public static ShopDatabaseSO CreateDevelopmentDatabase(HeroDatabaseSO heroDatabase)
    {
        ShopDatabaseSO database = CreateInstance<ShopDatabaseSO>();
        database.name = "RuntimeDevelopmentShopDatabase";
        database.products = new List<ShopProductSO>
        {
            ShopProductSO.CreateDevelopmentProduct(
                "exchange_gold_to_gem", "골드 교환", ShopProductCategory.Exchange,
                CurrencyType.GOLD, 500, ShopPurchaseLimitType.Daily, 3,
                ShopRewardEntry.CreateCurrency(CurrencyType.GEM, 10)),
            ShopProductSO.CreateDevelopmentProduct(
                "exchange_exp_to_gold", "경험치 교환", ShopProductCategory.Exchange,
                CurrencyType.EXP, 200, ShopPurchaseLimitType.Daily, 5,
                ShopRewardEntry.CreateCurrency(CurrencyType.GOLD, 150)),
            ShopProductSO.CreateDevelopmentProduct(
                "welcome_package", "웰컴 패키지", ShopProductCategory.Package,
                CurrencyType.GEM, 50, ShopPurchaseLimitType.Once, 1,
                ShopRewardEntry.CreateCurrency(CurrencyType.GOLD, 2000),
                ShopRewardEntry.CreateCurrency(CurrencyType.EXP, 500)),
            CreateDevelopmentTier2Package(heroDatabase)
        };
        database.attendanceRewardDatabase = AttendanceRewardDatabaseSO.CreateDevelopmentDatabase();
        return database;
    }

    // 개발용 2성 패키지에 사용할 테스트 영웅을 찾아 상품을 생성함
    private static ShopProductSO CreateDevelopmentTier2Package(HeroDatabaseSO heroDatabase)
    {
        List<ShopRewardEntry> rewards = new();
        if (heroDatabase != null && heroDatabase.TryGetHero("Hero_Healer_A", out HeroData heroData))
        {
            rewards.Add(ShopRewardEntry.CreateHero(heroData));
        }
        else
        {
            rewards.Add(ShopRewardEntry.CreateCurrency(CurrencyType.GEM, 100));
        }

        return ShopProductSO.CreateDevelopmentProduct(
            "tier2_hero_package", "2성 영웅 패키지", ShopProductCategory.Package,
            CurrencyType.GEM, 300, ShopPurchaseLimitType.Once, 1, rewards.ToArray());
    }

    // 보상 목록이 런타임 지급 가능한 형태인지 확인함
    private static bool TryValidateRewards(IReadOnlyList<ShopRewardEntry> rewards)
    {
        if (rewards == null || rewards.Count == 0)
        {
            return false;
        }

        foreach (ShopRewardEntry reward in rewards)
        {
            if (reward == null)
            {
                return false;
            }

            if (reward.RewardType == ShopRewardType.Currency &&
                (reward.CurrencyType == CurrencyType.None || reward.CurrencyType >= CurrencyType.Length || reward.Amount <= 0))
            {
                return false;
            }

            if (reward.RewardType == ShopRewardType.Hero && reward.HeroData == null)
            {
                return false;
            }

            if (reward.RewardType != ShopRewardType.Currency && reward.RewardType != ShopRewardType.Hero)
            {
                return false;
            }
        }

        return true;
    }

    // Inspector 목록을 빠른 조회용 사전으로 변환함
    private void EnsureInitialized()
    {
        if (productMap != null)
        {
            return;
        }

        productMap = new Dictionary<string, ShopProductSO>(StringComparer.Ordinal);
        foreach (ShopProductSO product in products.Where(product => product != null && !string.IsNullOrWhiteSpace(product.ProductId)))
        {
            if (!productMap.TryAdd(product.ProductId, product))
            {
                Debug.LogError($"[ShopDatabaseSO] 중복 상품 ID 있음: {product.ProductId}", product);
            }
        }
    }
}
