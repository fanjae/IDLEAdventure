using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// 공명 UI에서 영웅 한 명의 정보를 표시
public sealed class ResonanceHeroCardView : MonoBehaviour
{ 
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedVisual;
    [SerializeField] private Image heroPortrait;
    [SerializeField] private Image classIcon;
    [SerializeField] private TMP_Text levelValueText;
    [SerializeField] private TMP_Text levelText;

    private string heroId;
    private Action<string> onSelected;

    public string HeroId => heroId;

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

        onSelected?.Invoke(heroId);
    }

    // 보유 영웅 정보를 카드에 표시
    public void Bind(OwnedHeroData hero, HeroClassIconCatalog classIconCatalog, Action<string> onSelected = null)
    {
        if (hero == null || hero.HeroData == null)
        {
            Clear();
            return;
        }

        heroId = hero.HeroId;
        this.onSelected = onSelected;

        SetSelected(false);

        if (button != null)
        {
            button.interactable = true;
        }

        if (heroPortrait != null)
        {
            heroPortrait.gameObject.SetActive(true);
            heroPortrait.sprite = hero.HeroData.Portrait;
            heroPortrait.preserveAspect = heroPortrait.sprite != null;
        }

        if (classIcon != null)
        {
            classIcon.gameObject.SetActive(true);
            classIcon.sprite = classIconCatalog != null ? classIconCatalog.GetIcon(hero.HeroData.ClassType) : null;
            classIcon.preserveAspect = classIcon.sprite != null;
        }

        if (levelValueText != null)
        {
            levelValueText.gameObject.SetActive(true);
            levelValueText.text = $"<color=#373737>{hero.Level}</color>";
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            levelText.text = "레벨";
        }
    }

    // 영웅 카드 선택 표시 갱신
    public void SetSelected(bool selected)
    {
        if (selectedVisual != null)
        {
            selectedVisual.SetActive(selected);
        }
    }

    // 영웅 카드 정보를 빈 상태로 초기화
    public void Clear()
    {
        heroId = null;
        onSelected = null;

        SetSelected(false);

        if (heroPortrait != null)
        {
            heroPortrait.sprite = null;
            heroPortrait.gameObject.SetActive(false);
        }

        if (classIcon != null)
        {
            classIcon.sprite = null;
            classIcon.gameObject.SetActive(false);
        }

        if (levelValueText != null)
        {
            levelValueText.gameObject.SetActive(false);
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
        }

        if (button != null)
        {
            button.interactable = false;
        }
    }
}