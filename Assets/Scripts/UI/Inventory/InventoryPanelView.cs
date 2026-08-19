using DG.Tweening;
using UnityEngine;

// 인벤토리 패널 표시 애니메이션 관리
public sealed class InventoryPanelView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private InventoryPanelPresenter presenter;
    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float startOffsetY = -60f;

    private Sequence openSequence;
    private Vector2 defaultPosition;

    private void Awake()
    {
        defaultPosition = panelTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        PlayOpenAnimation();
    }

    private void OnDisable()
    {
        openSequence?.Kill();
    }

    // 인벤토리 오픈 애니메이션 재생
    private void PlayOpenAnimation()
    {
        openSequence?.Kill();

        canvasGroup.alpha = 0f;
        panelTransform.anchoredPosition = defaultPosition + new Vector2(0f, startOffsetY);

        openSequence = DOTween.Sequence();
        openSequence.Join(canvasGroup.DOFade(1f, openDuration));
        openSequence.Join(panelTransform.DOAnchorPos(defaultPosition, openDuration).SetEase(Ease.OutCubic));
        openSequence.OnComplete(HandleOpenAnimationCompleted);
    }

    // 패널 오픈 애니메이션 완료 처리
    private void HandleOpenAnimationCompleted()
    {
        presenter?.PlaySlotAnimations();
    }
}