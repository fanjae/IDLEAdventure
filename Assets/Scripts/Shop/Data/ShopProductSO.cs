using System;
using System.Collections.Generic;
using UnityEngine;

// 상점에서 보여줄 상품 분류임
public enum ShopProductCategory
{
    Exchange,
    Package
}

// 상품 구매 제한 방식임
public enum ShopPurchaseLimitType
{
    Unlimited,
    Once,
    Daily
}

// 상품 비용을 어떤 방식으로 지불하는지 정의함
public enum ShopPriceType
{
    Currency,
    Free
}

// 상품 카드와 추천 배너에 표시할 강조 배지 종류임
public enum ShopProductBadgeType
{
    None,
    Recommended,
    New,
    Limited
}

// 상품과 출석 보상에서 공통으로 사용할 보상 종류임
public enum ShopRewardType
{
    Currency,
    Hero
}

// 상점 상품의 비용과 보상, 구매 제한을 정의하는 SO임
[CreateAssetMenu(fileName = "ShopProduct", menuName = "Game Data/Shop/Product")]
public sealed class ShopProductSO : ScriptableObject
{
    [Header("식별 정보")]
    [SerializeField] private string productId;
    [SerializeField] private string displayName;
    [TextArea(2, 4)] [SerializeField] private string description;
    [SerializeField] private ShopProductCategory category;
    [SerializeField] private bool isVisible = true;
    [SerializeField] private int displayOrder;

    [Header("등장 조건")]
    [Tooltip("0이면 처음부터 등장하며, 지정한 번호의 스테이지를 클리어한 뒤 등장함")]
    [Min(0)] [SerializeField] private int requiredClearedStageId;
    [Tooltip("0이면 패배 조건이 없으며, 지정한 번호의 스테이지에 한 번 이상 패배한 뒤 등장함")]
    [Min(0)] [SerializeField] private int requiredDefeatedStageId;

    [Header("구매 비용")]
    [SerializeField] private ShopPriceType priceType = ShopPriceType.Currency;
    [SerializeField] private CurrencyType priceCurrency = CurrencyType.GEM;
    [Min(1)] [SerializeField] private int priceAmount = 1;

    [Header("구매 제한")]
    [SerializeField] private ShopPurchaseLimitType purchaseLimitType;
    [Min(1)] [SerializeField] private int dailyPurchaseLimit = 1;

    [Header("지급 보상")]
    [SerializeField] private List<ShopRewardEntry> rewards = new();

    [Header("UI")]
    [SerializeField] private ShopProductBadgeType badgeType;
    [SerializeField] private Sprite badgeImage;
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite artwork;

    public string ProductId => productId;
    public string DisplayName => displayName;
    public string Description => description;
    public ShopProductCategory Category => category;
    public bool IsVisible => isVisible;
    public int DisplayOrder => displayOrder;
    public int RequiredClearedStageId => Mathf.Max(0, requiredClearedStageId);
    public int RequiredDefeatedStageId => Mathf.Max(0, requiredDefeatedStageId);
    public ShopPriceType PriceType => priceType;
    public CurrencyType PriceCurrency => priceCurrency;
    public int PriceAmount => priceAmount;
    public ShopPurchaseLimitType PurchaseLimitType => purchaseLimitType;
    public int DailyPurchaseLimit => Mathf.Max(1, dailyPurchaseLimit);
    public ShopProductBadgeType BadgeType => badgeType;
    public Sprite BadgeImage => badgeImage;
    public IReadOnlyList<ShopRewardEntry> Rewards => rewards;
    public Sprite Icon => icon;
    public Sprite Artwork => artwork;

    // 저장된 현재 스테이지가 요구 스테이지 다음으로 진행됐는지 확인함
    public bool IsUnlockedAtCurrentStage(int currentStageId) =>
        RequiredClearedStageId == 0 || currentStageId > RequiredClearedStageId;

    // 클리어와 패배 조건을 모두 만족했는지 반환함
    public bool IsUnlockedAtCurrentProgress(int currentStageId, System.Func<int, bool> hasDefeatedStage) =>
        IsUnlockedAtCurrentStage(currentStageId) &&
        (RequiredDefeatedStageId == 0 || (hasDefeatedStage != null && hasDefeatedStage(RequiredDefeatedStageId)));

    // 에디터 에셋이 준비되기 전 기능 확인용 상품을 생성함
    public static ShopProductSO CreateDevelopmentProduct(
        string id,
        string name,
        ShopProductCategory productCategory,
        CurrencyType costCurrency,
        int costAmount,
        ShopPurchaseLimitType limitType,
        int dailyLimit,
        params ShopRewardEntry[] productRewards)
    {
        ShopProductSO product = CreateInstance<ShopProductSO>();
        product.name = $"Runtime_{id}";
        product.productId = id;
        product.displayName = name;
        product.category = productCategory;
        product.priceCurrency = costCurrency;
        product.priceAmount = Mathf.Max(1, costAmount);
        product.purchaseLimitType = limitType;
        product.dailyPurchaseLimit = Mathf.Max(1, dailyLimit);
        product.rewards = productRewards == null ? new List<ShopRewardEntry>() : new List<ShopRewardEntry>(productRewards);
        return product;
    }
}

// 상품이나 출석 보상이 지급할 재화 또는 영웅 한 종류를 정의함
[Serializable]
public sealed class ShopRewardEntry
{
    [SerializeField] private ShopRewardType rewardType;
    [SerializeField] private CurrencyType currencyType = CurrencyType.GOLD;
    [Min(1)] [SerializeField] private int amount = 1;
    [SerializeField] private HeroData heroData;

    public ShopRewardType RewardType => rewardType;
    public CurrencyType CurrencyType => currencyType;
    public int Amount => amount;
    public HeroData HeroData => heroData;

    // 재화 보상 항목을 생성함
    public static ShopRewardEntry CreateCurrency(CurrencyType type, int value)
    {
        return new ShopRewardEntry
        {
            rewardType = ShopRewardType.Currency,
            currencyType = type,
            amount = Mathf.Max(1, value)
        };
    }

    // 영웅 보상 항목을 생성함
    public static ShopRewardEntry CreateHero(HeroData hero)
    {
        return new ShopRewardEntry
        {
            rewardType = ShopRewardType.Hero,
            heroData = hero
        };
    }
}
