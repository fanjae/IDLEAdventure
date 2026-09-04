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

        title.outlineWidth = 0.22f;
        title.outlineColor = new Color(0.47f, 0.12f, 0.015f, 1.0f);

        SetAlpha(dimBackground, dimAlpha);
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
        RectTransform ribbon = ribbonImage.rectTransform;
        RectTransform burst = burstImage.rectTransform;
        RectTransform swordsEmblem = swordsEmblemImage.rectTransform;
        RectTransform sparkles = sparklesImage.rectTransform;
        RectTransform titleRect = title.rectTransform;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 1.0f;

        SetAlpha(ribbonImage, 0.0f);
        SetAlpha(burstImage, 0.0f);
        SetAlpha(swordsEmblemImage, 0.0f);
        SetAlpha(sparklesImage, 0.0f);
        SetAlpha(title, 0.0f);

        // 2026.09.04 전투 시작 연출과 동일한 등장 위치 및 크기 적용
        ribbon.anchoredPosition = new Vector2(-900.0f, -520.0f);
        ribbon.localScale = Vector3.one * 0.88f;
        burst.localScale = Vector3.one * 0.72f;
        swordsEmblem.localScale = Vector3.one * 0.68f;
        sparkles.localScale = Vector3.one * 0.86f;
        titleRect.localScale = Vector3.one * 0.72f;

        float elapsed = 0.0f;
        float safeDuration = Mathf.Max(0.1f, effectDuration);

        while (elapsed < safeDuration)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / safeDuration);

            float intro = EaseOutCubic(Mathf.Clamp01(normalizedTime / 0.34f));
            float titleIntro = EaseOutBack(Mathf.Clamp01((normalizedTime - 0.08f) / 0.30f));
            float outro = Mathf.Clamp01((normalizedTime - 0.78f) / 0.22f);

            canvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f, EaseInCubic(outro));

            SetAlpha(ribbonImage, Mathf.Clamp01(intro * 1.25f));
            SetAlpha(burstImage, Mathf.Clamp01((normalizedTime - 0.14f) / 0.24f));
            SetAlpha(swordsEmblemImage, Mathf.Clamp01((normalizedTime - 0.16f) / 0.25f));
            SetAlpha(sparklesImage, Mathf.Clamp01((normalizedTime - 0.08f) / 0.30f));
            SetAlpha(title, Mathf.Clamp01((normalizedTime - 0.12f) / 0.22f));

            ribbon.anchoredPosition = Vector2.Lerp(new Vector2(-900.0f, -520.0f), Vector2.zero, intro);
            ribbon.localScale = Vector3.one * Mathf.Lerp(0.88f, 1.0f, intro);
            burst.localScale = Vector3.one * Mathf.Lerp(0.72f, 1.0f, titleIntro);
            swordsEmblem.localScale = Vector3.one * Mathf.Lerp(0.68f, 0.92f, titleIntro);
            sparkles.localScale = Vector3.one * Mathf.Lerp(0.86f, 1.0f, intro);
            titleRect.localScale = Vector3.one * Mathf.Lerp(0.72f, 1.0f, titleIntro);

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

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null)
        {
            return;
        }

        Color color = graphic.color;
        color.a = Mathf.Clamp01(alpha);
        graphic.color = color;
    }

    private static float EaseOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return 1.0f - Mathf.Pow(1.0f - value, 3.0f);
    }

    private static float EaseInCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * value;
    }

    private static float EaseOutBack(float value)
    {
        value = Mathf.Clamp01(value);
        const float overshoot = 1.70158f;
        float adjusted = value - 1.0f;
        return 1.0f + (overshoot + 1.0f) * adjusted * adjusted * adjusted + overshoot * adjusted * adjusted;
    }
}