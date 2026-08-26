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
    [SerializeField] private Button selectButton;

    // 선택한 상품을 큰 배너에 표시하고 구매 확인을 열게 함
    public void Bind(ShopProductSO product, Action<string> selectAction)
    {
        artworkImage.sprite = product.Artwork != null ? product.Artwork : product.Icon;
        badgeImage.sprite = product.BadgeImage;
        badgeImage.gameObject.SetActive(product.BadgeImage != null);
        nameText.text = product.DisplayName;
        descriptionText.text = product.Description;
        priceText.text = ShopProductCardView.GetPriceText(product);
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => selectAction(product.ProductId));
    }
}
