using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleHeroCardView : MonoBehaviour
{
    private Image heroImage;
    private TMP_Text levelText;
    private Image classImage;
    private Button selectButton;
    private GameObject checkIndicator;
    private string heroId;
    private Action<string> onSelected;

    private void Awake()
    {
        CacheReferences();
    }

    public void Bind(
        OwnedHeroData ownedHero,
        HeroPresentationCatalog presentationCatalog,
        HeroClassIconCatalog classIconCatalog,
        bool isSelected,
        Action<string> onSelected)
    {
        if (ownedHero == null || ownedHero.HeroData == null)
        {
            return;
        }

        CacheReferences();
        heroId = ownedHero.HeroId;
        this.onSelected = onSelected;

        if (heroImage != null)
        {
            heroImage.sprite = presentationCatalog != null
                ? presentationCatalog.GetPortrait(ownedHero.HeroId)
                : null;
            heroImage.preserveAspect = heroImage.sprite != null;
        }

        if (levelText != null)
        {
            levelText.text = ownedHero.Level.ToString();
        }

        if (classImage != null)
        {
            classImage.sprite = classIconCatalog != null
                ? classIconCatalog.GetIcon(ownedHero.HeroData.ClassType)
                : null;
            classImage.preserveAspect = classImage.sprite != null;
        }

        SetSelected(isSelected);
    }

    private void CacheReferences()
    {
        heroImage ??= FindComponent<Image>("HeroImage");
        levelText ??= FindComponent<TMP_Text>("Level");
        classImage ??= FindComponent<Image>("Class");
        selectButton ??= GetComponent<Button>();
        selectButton ??= GetComponentInChildren<Button>(true);
        checkIndicator ??= FindChildTransform("Check")?.gameObject;

        if (heroImage != null && levelText != null && classImage != null)
        {
            return;
        }

        Transform nestedCard = FindChildTransform("HeroCard");
        if (nestedCard == null || nestedCard == transform)
        {
            return;
        }

        heroImage ??= FindComponent<Image>(nestedCard, "HeroImage");
        levelText ??= FindComponent<TMP_Text>(nestedCard, "Level");
        classImage ??= FindComponent<Image>(nestedCard, "Class");
    }

    private void SetSelected(bool isSelected)
    {
        if (checkIndicator != null)
        {
            checkIndicator.SetActive(isSelected);
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(HandleSelected);
            selectButton.onClick.AddListener(HandleSelected);
        }
    }

    private void HandleSelected()
    {
        if (!string.IsNullOrEmpty(heroId))
        {
            onSelected?.Invoke(heroId);
        }
    }

    private T FindComponent<T>(string childName) where T : Component
    {
        return FindComponent<T>(transform, childName);
    }

    private T FindComponent<T>(Transform root, string childName) where T : Component
    {
        T[] components = root.GetComponentsInChildren<T>(true);

        foreach (T component in components)
        {
            if (component.gameObject.name == childName)
            {
                return component;
            }
        }

        return null;
    }

    private Transform FindChildTransform(string childName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }
}
