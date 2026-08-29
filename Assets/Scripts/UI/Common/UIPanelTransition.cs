using System;
using DG.Tweening;
using UnityEngine;

public sealed class UIPanelTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelTransform;

    [Header("패널 이동")]
    [SerializeField] private Vector2 openOffset = new Vector2(0f, -100f);
    [SerializeField] private Vector2 closeOffset = new Vector2(0f, -100f);

    [Header("패널 연출")]
    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float closeDuration = 0.2f;

    private Vector2 originPosition;
    private Tween currentTween;

    private void Awake()
    {
        if (panelTransform != null)
        {
            originPosition = panelTransform.anchoredPosition;
        }
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }

    // 패널 오픈 연출
    public void PlayOpen(Action onComplete = null)
    {
        if (canvasGroup == null || panelTransform == null)
        {
            return;
        }

        currentTween?.Kill();

        panelTransform.anchoredPosition = originPosition + openOffset;
        canvasGroup.alpha = 0f;
        SetInteractable(false);

        Sequence sequence = DOTween.Sequence();
        sequence.Join(panelTransform.DOAnchorPos(originPosition, openDuration).SetEase(Ease.OutCubic));
        sequence.Join(canvasGroup.DOFade(1f, openDuration));

        sequence.OnComplete(() =>
        {
            SetInteractable(true);
            onComplete?.Invoke();
        });

        currentTween = sequence;
    }

    // 패널 닫기 연출
    public void PlayClose(Action onComplete = null)
    {
        if (canvasGroup == null || panelTransform == null)
        {
            onComplete?.Invoke();
            return;
        }

        currentTween?.Kill();
        SetInteractable(false);

        Vector2 closePosition = originPosition + closeOffset;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(panelTransform.DOAnchorPos(closePosition, closeDuration).SetEase(Ease.InCubic));
        sequence.Join(canvasGroup.DOFade(0f, closeDuration));

        sequence.OnComplete(() =>
        {
            ResetState();
            onComplete?.Invoke();
        });

        currentTween = sequence;
    }

    // 패널 기본 상태 복원
    private void ResetState()
    {
        panelTransform.anchoredPosition = originPosition;
        canvasGroup.alpha = 1f;
        SetInteractable(true);
    }

    // 패널 입력 상태 변경
    private void SetInteractable(bool value)
    {
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }
}