using System;
using DG.Tweening;
using UnityEngine;

public sealed class SkillPanelTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("스킬 전환 연출")]
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float fadeInDuration = 0.2f;

    private Tween currentTween;

    private void OnDisable()
    {
        ResetState();
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }

    // 현재 내용을 숨긴 뒤 새 내용으로 교체하고 다시 표시
    public void PlayChange(Action changeContent)
    {
        if (canvasGroup == null)
        {
            changeContent?.Invoke();
            return;
        }

        currentTween?.Kill();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));
        sequence.AppendCallback(() => changeContent?.Invoke());
        sequence.Append(canvasGroup.DOFade(1f, fadeInDuration));

        currentTween = sequence;
    }

    // 전환 상태 초기화
    public void ResetState()
    {
        currentTween?.Kill();
        currentTween = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
}