using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 배치 영웅 카드 UI 관리
public sealed class FormationHeroCardView : MonoBehaviour
{
    [SerializeField] private Image heroPortrait;
    [SerializeField] private Image classIcon;
    [SerializeField] private TMP_Text levelValueText;

    // 보유 영웅 정보를 카드에 표시
    public void Bind(OwnedHeroData hero, HeroClassIconCatalog classIconCatalog)
    {
        if (hero == null || hero.HeroData == null)
        {
            return;
        }

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
            levelValueText.text = hero.Level.ToString();
        }
    }
}