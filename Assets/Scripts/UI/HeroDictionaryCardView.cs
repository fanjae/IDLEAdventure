using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HeroDictionaryCardView : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button selectButton;

    public void Bind(
        HeroData heroData,
        OwnedHeroData ownedHero,
        HeroPresentationCatalog presentationCatalog,
        Action<HeroData> onSelected)
    {
        bool isOwned = ownedHero != null;

        if (portraitImage != null)
        {
            portraitImage.sprite = presentationCatalog != null
                ? presentationCatalog.GetPortrait(heroData.UnitID)
                : null;
        }

        if (nameText != null)
        {
            nameText.text = heroData.UnitName;
        }

        if (statusText != null)
        {
            statusText.text = isOwned ? $"Lv. {ownedHero.Level}" : "미보유";
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelected?.Invoke(heroData));
        }
    }
}
