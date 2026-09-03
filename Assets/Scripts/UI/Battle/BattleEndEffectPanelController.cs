using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 2026.09.04 전투 종료 후 결과 패널 표시 전에 종료 연출을 재생한다.
public sealed class BattleEndEffectPanelController : MonoBehaviour
{
    [Header("프리팹 UI 참조")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image dimBackground;
    [SerializeField] private Image ribbonImage;
    [SerializeField] private Image burstImage;
    [SerializeField] private Image swordsEmblemImage;
    [SerializeField] private Image sparklesImage;
    [SerializeField] private TextMeshProUGUI title;

    [Header("연출 설정")]
    [SerializeField, Min(0.1f)] private float effectDuration = 0.95f;
    [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.46f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine playRoutine;

    private void Awake()
    {
        RectTransform root = GetComponent<RectTransform>();
        if (root != null)
        {
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.localScale = Vector3.one;
        }

        HideImmediate();
    }

    public void PlayEffect(UnitTeam winner, Action onComplete = null)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        title.text = winner == UnitTeam.Hero ? "클리어" : "패배";
        playRoutine = StartCoroutine(PlayEffectRoutine(onComplete));
    }

    private IEnumerator PlayEffectRoutine(Action onComplete)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, effectDuration);

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        HideImmediate();
        playRoutine = null;
        onComplete?.Invoke();
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }
}