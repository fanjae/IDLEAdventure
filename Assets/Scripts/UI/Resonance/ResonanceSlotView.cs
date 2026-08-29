using System;
using UnityEngine;

// 공명 슬롯 한 칸의 영웅 카드 표시
public sealed class ResonanceSlotView : MonoBehaviour
{
    [SerializeField] private ResonanceHeroCardView heroCardView;

    // 공명 슬롯에 등록된 영웅 정보 표시
    public void Bind(OwnedHeroData hero, HeroClassIconCatalog classIconCatalog, Action<string> onClicked, Action<string> onRightClicked = null)
    {
        if (heroCardView == null)
        {
            return;
        }

        if (hero == null || hero.HeroData == null)
        {
            Clear();
            return;
        }

        heroCardView.gameObject.SetActive(true);
        heroCardView.Bind(hero, classIconCatalog, onClicked, onRightClicked);
    }

    // 공명 슬롯을 빈 상태로 표시
    public void Clear()
    {
        if (heroCardView == null)
        {
            return;
        }

        heroCardView.gameObject.SetActive(true);
        heroCardView.Clear();
    }
}