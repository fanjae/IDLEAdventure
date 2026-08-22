using DG.Tweening;
using UnityEngine;

// 메인 화면 모험 패널 UI 관리
public sealed class MainAdventurePanelController : MonoBehaviour
{
    [SerializeField] private GameObject mainBottomPanel;
    [SerializeField] private GameObject adventurePanel;
    [SerializeField] private CanvasGroup adventureCanvasGroup;
    [SerializeField] private RectTransform adventurePanelTransform;
    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float closeDuration = 0.2f;
    [SerializeField] private float startOffsetY = -60f;
    [SerializeField] private float closeOffsetY = 60f;

    private Sequence panelSequence;
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
        panelSequence?.Kill();
    }

    // 모험 패널 표시
    public void OpenAdventurePanel()
    {
        if (adventurePanel == null || adventureCanvasGroup == null || adventurePanelTransform == null)
        {
            return;
        }

        panelSequence?.Kill();

        adventurePanel.SetActive(true);

        adventureCanvasGroup.alpha = 0f;
        adventureCanvasGroup.interactable = false;
        adventureCanvasGroup.blocksRaycasts = false;

        adventurePanelTransform.anchoredPosition = defaultPosition + new Vector2(0f, startOffsetY);

        panelSequence = DOTween.Sequence();

        // 아래쪽에서 원래 위치로 이동하면서 표시
        panelSequence.Join(adventureCanvasGroup.DOFade(1f, openDuration));
        panelSequence.Join(adventurePanelTransform.DOAnchorPos(defaultPosition, openDuration).SetEase(Ease.OutCubic));

        panelSequence.OnComplete(() =>
        {
            adventureCanvasGroup.interactable = true;
            adventureCanvasGroup.blocksRaycasts = true;
        });
    }

    // 모험 패널 숨김
    public void CloseAdventurePanel()
    {
        if (adventurePanel == null || adventureCanvasGroup == null || adventurePanelTransform == null)
        {
            return;
        }

        panelSequence?.Kill();

        adventureCanvasGroup.interactable = false;
        adventureCanvasGroup.blocksRaycasts = false;

        panelSequence = DOTween.Sequence();

        // 위쪽으로 이동하면서 투명하게 처리
        panelSequence.Join(adventureCanvasGroup.DOFade(0f, closeDuration));
        panelSequence.Join(adventurePanelTransform.DOAnchorPos(defaultPosition + new Vector2(0f, closeOffsetY), closeDuration).SetEase(Ease.InCubic));

        panelSequence.OnComplete(() =>
        {
            adventurePanel.SetActive(false);

            // 다음 오픈을 위해 기본 상태 복원
            adventurePanelTransform.anchoredPosition = defaultPosition;
            adventureCanvasGroup.alpha = 1f;

            // 기존 하단 메뉴 다시 표시
            if (mainBottomPanel != null)
            {
                mainBottomPanel.SetActive(true);
            }
        });
    }
}