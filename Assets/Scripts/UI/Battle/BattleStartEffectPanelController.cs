using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 시작 직후 화면 전체에 전투 시작 연출을 표시한다.
// 실제 전투 시작 이벤트 이후에 재생되므로 전투 로직과 연출 수명을 분리한다.
public sealed class BattleStartEffectPanelController : MonoBehaviour
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
    [SerializeField, Range(0.0f, 1.0f)] private float dimAlpha = 0.46f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine playRoutine;
    private BattleManager subscribedBattleManager;
    private bool referencesValid;

    private void Awake()
    {
        ConfigureRoot();
        referencesValid = ValidateReferences();

        if (referencesValid)
        {
            ConfigureTitleAppearance();
            SetAlpha(dimBackground, dimAlpha);
            HideImmediate();
        }
    }

    private void Start()
    {
        if (!referencesValid)
        {
            return;
        }

        SubscribeToBattleManager();
    }

    private void OnDestroy()
    {
        UnsubscribeFromBattleManager();
    }

    private void ConfigureRoot()
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

        // 기존 전투 UI보다 앞에 표시
        transform.SetAsLastSibling();
    }

    // 연출에 필요한 UI 참조 확인
    private bool ValidateReferences()
    {
        bool valid = true;

        if (canvasGroup == null)
        {
            Debug.LogError("BattleStartEffectPanel의 CanvasGroup이 연결되지 않았습니다.", this);
            valid = false;
        }

        if (dimBackground == null || ribbonImage == null || burstImage == null || swordsEmblemImage == null || sparklesImage == null || title == null)
        {
            Debug.LogError("BattleStartEffectPanel의 전투 시작 UI 자식 참조가 모두 연결되어야 합니다.", this);
            valid = false;
        }

        return valid;
    }

    // 전투 시작 문구 외곽선 설정
    private void ConfigureTitleAppearance()
    {
        title.outlineWidth = 0.22f;
        title.outlineColor = new Color(0.47f, 0.12f, 0.015f, 1.0f);
    }

    // 전투 시작 및 종료 이벤트 연결
    private void SubscribeToBattleManager()
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없어 전투 시작 연출을 연결하지 못했습니다.", this);
            return;
        }

        subscribedBattleManager = BattleManager.Instance;
        subscribedBattleManager.OnBattleStarted += HandleBattleStarted;
        subscribedBattleManager.OnBattleEnded += HandleBattleEnded;

        // 전투 시작 이후 UI가 활성화된 경우 현재 전투 상태 반영
        if (subscribedBattleManager.IsBattleRunning)
        {
            PlayEffect();
        }
    }

    // 등록한 BattleManager 이벤트 해제
    private void UnsubscribeFromBattleManager()
    {
        if (subscribedBattleManager == null)
        {
            return;
        }

        subscribedBattleManager.OnBattleStarted -= HandleBattleStarted;
        subscribedBattleManager.OnBattleEnded -= HandleBattleEnded;
        subscribedBattleManager = null;
    }

    // 전투 시작 시 연출 재생
    private void HandleBattleStarted()
    {
        PlayEffect();
    }

    // 전투 종료 시 진행 중인 연출 종료
    private void HandleBattleEnded(UnitTeam winner)
    {
        StopEffect();
    }

    // 전투 시작 연출 재생
    public void PlayEffect()
    {
        if (!referencesValid)
        {
            return;
        }

        // 이전 연출이 남아있으면 중단 후 처음부터 재생
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        playRoutine = StartCoroutine(PlayEffectRoutine());
    }

    // 진행 중인 전투 시작 연출 즉시 종료
    public void StopEffect()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        HideImmediate();
    }

    // 전투 시작 UI의 등장 및 퇴장 연출 처리
    private IEnumerator PlayEffectRoutine()
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

        // 연출 시작 위치 및 크기 초기화
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

            // 연출 진행 구간별 보간값 계산
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

        playRoutine = null;
        HideImmediate();
    }

    // 전투 시작 연출 즉시 숨김
    private void HideImmediate()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0.0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    // Graphic의 기존 색상을 유지한 상태로 투명도만 변경
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
