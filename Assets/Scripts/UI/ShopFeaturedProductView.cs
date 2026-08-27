using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상점 상품 한 개를 큰 배너 형태로 표시함
public sealed class ShopFeaturedProductView : MonoBehaviour
{
    [SerializeField] private Image artworkImage;
    [SerializeField] private Image badgeImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text remainingExchangeCountText;
    [SerializeField] private Button selectButton;

    // 선택한 상품과 남은 구매 횟수를 큰 배너에 표시하고 구매 확인을 열게 함
    public void Bind(ShopProductSO product, ShopProductAvailability availability, Action<string> selectAction)
    {
        artworkImage.sprite = product.Artwork != null ? product.Artwork : product.Icon;
        badgeImage.sprite = product.BadgeImage;
        badgeImage.gameObject.SetActive(product.BadgeImage != null);
        nameText.text = product.DisplayName;
        descriptionText.text = product.Description;
        priceText.text = ShopProductCardView.GetPriceText(product);
        SetRemainingExchangeCountText(product, availability);
        selectButton.interactable = availability.CanPurchase;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => selectAction(product.ProductId));
    }

    // 일일 재화 교환 상품의 남은 횟수를 가격과 분리한 전용 텍스트에 표시함
    private void SetRemainingExchangeCountText(ShopProductSO product, ShopProductAvailability availability)
    {
        if (remainingExchangeCountText == null)
            return;

        bool isDailyExchange = product.Category == ShopProductCategory.Exchange &&
                               product.PurchaseLimitType == ShopPurchaseLimitType.Daily;
        remainingExchangeCountText.gameObject.SetActive(isDailyExchange);
        if (isDailyExchange)
            remainingExchangeCountText.text = $"{availability.RemainingPurchaseCount}회 남음";
    }
}
