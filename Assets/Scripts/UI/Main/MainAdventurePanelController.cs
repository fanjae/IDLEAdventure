using DG.Tweening;
using UnityEngine;

// 메인 화면 모험 패널 UI 관리
public sealed class MainAdventurePanelController : MonoBehaviour
{
    [SerializeField] private GameObject adventurePanel;
    [SerializeField] private CanvasGroup adventureCanvasGroup;
    [SerializeField] private RectTransform adventurePanelTransform;
    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float startOffsetY = -60f;

    private Sequence openSequence;
    private Vector2 defaultPosition;

    private void Awake()
    {
        if (adventurePanelTransform != null)
        {
            defaultPosition = adventurePanelTransform.anchoredPosition;
        }
    }

    private void OnDestroy()
    {
        openSequence?.Kill();
    }

    // 모험 패널 표시
    public void OpenAdventurePanel()
    {
        if (adventurePanel == null || adventureCanvasGroup == null || adventurePanelTransform == null)
        {
            return;
        }

        openSequence?.Kill();

        adventurePanel.SetActive(true);

        adventureCanvasGroup.alpha = 0f;
        adventurePanelTransform.anchoredPosition = defaultPosition + new Vector2(0f, startOffsetY);

        openSequence = DOTween.Sequence();
        openSequence.Join(adventureCanvasGroup.DOFade(1f, openDuration));
        openSequence.Join(adventurePanelTransform.DOAnchorPos(defaultPosition, openDuration).SetEase(Ease.OutCubic));
    }

    // 모험 패널 숨김
    public void CloseAdventurePanel()
    {
        openSequence?.Kill();

        if (adventurePanel != null)
        {
            adventurePanel.SetActive(false);
        }
    }
}