using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 공명 UI에서 영웅 한 명의 정보를 표시
public sealed class ResonanceHeroCardView : MonoBehaviour, IPointerClickHandler
{ 
    [SerializeField] private Button button;
    [SerializeField] private Image heroPortrait;
    [SerializeField] private Image classIcon;
    [SerializeField] private TMP_Text levelValueText;
    [SerializeField] private TMP_Text levelText;

    private string heroId;
    private Action<string> onClicked;
    private Action<string> onRightClicked;

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
    public void Bind(OwnedHeroData hero, HeroClassIconCatalog classIconCatalog, Action<string> onClicked = null, Action<string> onRightClicked = null)
    {
        if (hero == null || hero.HeroData == null)
        {
            Clear();
            return;
        }

        heroId = hero.HeroId;
        this.onClicked = onClicked;
        this.onRightClicked = onRightClicked;

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

    // 영웅 카드 우클릭 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        onRightClicked?.Invoke(heroId);
    }

    // 영웅 카드 정보를 빈 상태로 초기화
    public void Clear()
    {
        heroId = null;
        onClicked = null;
        onRightClicked = null;

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