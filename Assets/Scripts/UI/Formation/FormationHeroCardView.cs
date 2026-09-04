using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 배치 영웅 카드 UI 관리
public sealed class FormationHeroCardView : MonoBehaviour
{
    [SerializeField] private Image heroPortrait;
    [SerializeField] private Image classIcon;
    [SerializeField] private TMP_Text levelValueText;
    [SerializeField] private Button selectButton;

    // 출전 상태를 캐릭터가 보이는 상태로 구분하기 위한 선택 효과 (0903 추가)
    [SerializeField] private GameObject selectedTint;
    [SerializeField] private GameObject selectedBorder;
    [SerializeField] private GameObject selectedCheck;

    private OwnedHeroData hero;

    public event Action<string> OnSelected;

    public HeroData HeroData => hero != null ? hero.HeroData : null;

    private void Awake()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(HandleClick);
        }
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(HandleClick);
        }
    }

    // 보유 영웅 정보를 카드에 표시
    public void Bind(OwnedHeroData hero, HeroClassIconCatalog classIconCatalog)
    {
        this.hero = hero;

        // ScrollRect 카드 재사용 시 이전 영웅의 선택 표시가 남지 않도록 초기화 (0903 초기화)
        SetSelected(false);

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

    // 출전 영웅 카드에 회색 처리, 초록 외곽선, V 표시를 함께 적용 (0903 추가)
    public void SetSelected(bool selected)
    {
        if (selectedTint != null)
        {
            selectedTint.SetActive(selected);
        }

        if (selectedBorder != null)
        {
            selectedBorder.SetActive(selected);
        }

        if (selectedCheck != null)
        {
            selectedCheck.SetActive(selected);
        }
    }

    // 영웅 카드 클릭 처리
    private void HandleClick()
    {
        if (hero == null)
        {
            return;
        }

        OnSelected?.Invoke(hero.HeroId);
    }
}
