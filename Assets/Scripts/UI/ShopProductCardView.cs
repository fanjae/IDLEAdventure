using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상점 상품 한 개의 실제 카드 프리팹 표시를 담당함
public sealed class ShopProductCardView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text limitText;
    [SerializeField] private Image badgeImage;
    [SerializeField] private TMP_Text purchaseLabel;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image[] rewardIconSlots;
    [SerializeField] private TMP_Text rewardMoreText;

    // 상품 데이터와 구매 가능 상태를 카드에 표시함
    public void Bind(ShopProductSO product, ShopProductAvailability availability, Action<string> purchaseAction, Func<CurrencyType, Sprite> currencyIconProvider)
    {
        iconImage.sprite = product.Icon != null ? product.Icon : product.Artwork != null ? product.Artwork : currencyIconProvider(product.PriceCurrency);
        nameText.text = product.DisplayName;
        descriptionText.text = product.Description;
        limitText.text = GetLimitText(product, availability);
        limitText.color = availability.CanPurchase ? new Color(0.24f, 0.45f, 0.25f) : new Color(0.55f, 0.22f, 0.16f);
        badgeImage.sprite = product.BadgeImage;
        badgeImage.gameObject.SetActive(product.BadgeImage != null);
        purchaseLabel.text = GetPriceText(product);
        purchaseButton.interactable = availability.CanPurchase;
        BindRewardIcons(product, currencyIconProvider);
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(() => purchaseAction(product.ProductId));
    }

    // 재화 이름을 짧은 표시 문구로 변환함
    public static string GetCurrencyName(CurrencyType type) => type switch
    {
        CurrencyType.GOLD => "골드",
        CurrencyType.EXP => "경험치",
        CurrencyType.UPGRADE => "강화 재료",
        CurrencyType.GEM => "보석",
        _ => "재화"
    };

    // 가격 타입에 맞는 버튼 문구를 반환함
    public static string GetPriceText(ShopProductSO product) => product.PriceType switch
    {
        ShopPriceType.Free => "무료 수령",
        _ => $"구매\n{GetCurrencyName(product.PriceCurrency)} {product.PriceAmount}"
    };

    // 여러 보상을 카드 안의 아이콘 칸에 표시함
    private void BindRewardIcons(ShopProductSO product, Func<CurrencyType, Sprite> currencyIconProvider)
    {
        int rewardCount = product.Rewards.Count;
        for (int index = 0; index < rewardIconSlots.Length; index++)
        {
            bool hasReward = index < rewardCount;
            rewardIconSlots[index].gameObject.SetActive(hasReward);
            if (!hasReward)
                continue;

            ShopRewardEntry reward = product.Rewards[index];
            rewardIconSlots[index].sprite = reward.RewardType == ShopRewardType.Hero
                ? reward.HeroData != null ? reward.HeroData.Portrait : null
                : currencyIconProvider(reward.CurrencyType);
        }

        rewardMoreText.gameObject.SetActive(rewardCount > rewardIconSlots.Length);
        if (rewardCount > rewardIconSlots.Length)
            rewardMoreText.text = $"+{rewardCount - rewardIconSlots.Length}";
    }

    // 상품 제한 상태를 표시용 문구로 변환함
    private static string GetLimitText(ShopProductSO product, ShopProductAvailability availability) => product.PurchaseLimitType switch
    {
        ShopPurchaseLimitType.Once => availability.CanPurchase ? "1회 한정" : "구매 완료",
        ShopPurchaseLimitType.Daily => $"오늘 {availability.RemainingPurchaseCount}/{product.DailyPurchaseLimit}회 구매 가능",
        _ => "상시 구매"
    };
}
