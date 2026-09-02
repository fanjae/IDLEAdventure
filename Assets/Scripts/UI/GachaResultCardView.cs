using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 결과 오버레이에 표시할 영웅 한 장의 텍스트 골격임
public sealed class GachaResultCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text heroNameText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image portraitImage;

    [Header("결과 카드 배경")]
    [SerializeField] private Sprite tier1Background;
    [SerializeField] private Sprite tier2Background;
    [SerializeField] private Sprite tier1DuplicateBackground;
    [SerializeField] private Sprite tier2DuplicateBackground;

    [Header("중복 골드 표시")]
    [SerializeField] private Image duplicateGoldIconImage;
    [SerializeField] private Sprite duplicateGoldIcon;

    // 영웅 이름과 획득 상태 문구를 결과 카드에 표시함
    public void Bind(
        GachaPullResult result,
        string heroName,
        Sprite heroPortrait,
        bool showDuplicateGoldIcon = true,
        bool showText = true)
    {
        if (result == null)
        {
            return;
        }

        ApplyBackground(result, showDuplicateGoldIcon);

        if (portraitImage != null)
        {
            portraitImage.sprite = heroPortrait;
            portraitImage.preserveAspect = heroPortrait != null;
            portraitImage.enabled = heroPortrait != null;
        }

        if (heroNameText != null)
        {
            heroNameText.gameObject.SetActive(showText);
            heroNameText.text = result.IsDuplicate ? "골드" : heroName;
        }

        if (stateText == null)
        {
            return;
        }

        stateText.gameObject.SetActive(showText);

        if (!showText)
        {
            return;
        }

        string state = result.Rarity == GachaRarity.Tier2 ? "2티어" : "1티어";
        if (result.IsPickup) state += " · 픽업";
        if (result.IsPity) state += " · 천장";
        if (result.IsDuplicate) state = $"중복 영웅 변환 · +{result.ConvertedGold} 골드";
        stateText.text = state;
    }

    private void ApplyBackground(GachaPullResult result, bool showDuplicateGoldIcon)
    {
        if (backgroundImage != null)
        {
            Sprite background = result.IsDuplicate
                ? result.Rarity == GachaRarity.Tier2 ? tier2DuplicateBackground : tier1DuplicateBackground
                : result.Rarity == GachaRarity.Tier2 ? tier2Background : tier1Background;

            if (background != null)
            {
                backgroundImage.sprite = background;
            }
        }

        if (duplicateGoldIconImage != null)
        {
            duplicateGoldIconImage.sprite = duplicateGoldIcon;
            duplicateGoldIconImage.enabled = showDuplicateGoldIcon && result.IsDuplicate && duplicateGoldIcon != null;
        }
    }

}
