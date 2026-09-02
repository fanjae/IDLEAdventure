using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 소환 완료 후 입력을 막고 결과 카드 목록을 보여줌
public sealed class GachaResultOverlay : MonoBehaviour
{
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private Image resultBackgroundImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform cardContent;
    [SerializeField] private GachaResultCardView cardTemplate;
    [SerializeField] private Button continueButton;

    private readonly List<GachaResultCardView> cards = new();

    public event Action Closed;
    public bool IsOpen => overlayRoot != null && overlayRoot.activeSelf;

    // 계속 버튼 클릭을 결과 닫기 동작에 연결함
    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(Close);
        }
    }

    // 결과 수만큼 텍스트 카드 골격을 생성해 표시함
    public void Show(
        GachaDrawResult result,
        Func<string, string> getHeroName,
        Func<string, Sprite> getHeroPortrait,
        Sprite resultBackgroundSprite = null)
    {
        if (result == null || overlayRoot == null || cardTemplate == null || cardContent == null)
        {
            return;
        }

        ClearCards();
        cardTemplate.gameObject.SetActive(false);

        if (resultBackgroundImage != null && resultBackgroundSprite != null)
        {
            resultBackgroundImage.sprite = resultBackgroundSprite;
            resultBackgroundImage.preserveAspect = true;
        }

        if (titleText != null)
        {
            titleText.text = result.PullResults.Count == 10 ? "10회 소환 결과" : "소환 결과";
        }

        foreach (GachaPullResult pullResult in result.PullResults)
        {
            GachaResultCardView card = Instantiate(cardTemplate, cardContent);
            card.gameObject.SetActive(true);
            card.Bind(
                pullResult,
                getHeroName?.Invoke(pullResult.HeroId) ?? pullResult.HeroId,
                getHeroPortrait?.Invoke(pullResult.HeroId));
            cards.Add(card);
        }

        overlayRoot.SetActive(true);
    }

    // 결과 오버레이를 닫고 배너 화면 입력을 되돌림
    public void Close()
    {
        if (overlayRoot == null || !overlayRoot.activeSelf)
        {
            return;
        }

        overlayRoot.SetActive(false);
        Closed?.Invoke();
    }

    // 이전 소환 결과 카드만 정리함
    private void ClearCards()
    {
        foreach (GachaResultCardView card in cards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        cards.Clear();
    }
}
