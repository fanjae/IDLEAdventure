using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 공명 UI에서 영웅 한 명의 정보를 표시
public sealed class ResonanceHeroCardView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image heroPortrait;
    [SerializeField] private Image classIcon;
    [SerializeField] private TMP_Text levelValueText;
    [SerializeField] private TMP_Text levelText;

    private string heroId;
    private Action<string> onClicked;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClicked);
        }
    }

    // 현재 카드에 표시된 영웅 선택
    private void HandleClicked()
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        onClicked?.Invoke(heroId);
    }



    // 보유 영웅 정보를 카드에 표시
    public void Bind(OwnedHeroData hero, HeroClassIconCatalog classIconCatalog, Action<string> onClicked = null)
    {
        if (hero == null || hero.HeroData == null)
        {
            return;
        }

        heroId = hero.HeroId;
        this.onClicked = onClicked;

        if (heroPortrait != null)
        {
            heroPortrait.sprite = hero.HeroData.Portrait;
            heroPortrait.preserveAspect = heroPortrait.sprite != null;
        }

        if (classIcon != null)
        {
            classIcon.sprite = classIconCatalog != null ? classIconCatalog.GetIcon(hero.HeroData.ClassType) : null;
            classIcon.preserveAspect = classIcon.sprite != null;
        }

        if (levelValueText != null)
        {
            levelValueText.text = $"<color=#373737>{hero.Level}</color>";
        }

        if (levelText != null)
        {
            levelText.text = "레벨";
        }
    }
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }
}