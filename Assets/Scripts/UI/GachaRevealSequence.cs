using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// 확정된 가챠 결과의 등급에 따라 요약 화면 직전 연출을 재생함
public sealed class GachaRevealSequence : MonoBehaviour
{
    private enum RevealGrade
    {
        Tier1,
        Tier2,
        PickupTier2
    }

    [Header("표시 대상")]
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private Image sequenceBackgroundImage;
    [Tooltip("열린 문 상태에서 사용할 단일 빛 이미지. openedDoorLightImages가 비어 있을 때 대체로 사용됩니다.")]
    [SerializeField] private Image flashImage;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button tapToContinueButton;
    [SerializeField] private GameObject touchToGachaPrompt;

    [Header("10회 소환 카드 미리보기")]
    [SerializeField] private GameObject tenPullPreviewRoot;
    [SerializeField] private Transform tenPullPreviewContent;
    [SerializeField] private GachaResultCardView previewCardTemplate;
    [Min(0.1f)] [SerializeField] private float previewCardDuration = 0.42f;
    [Min(1f)] [SerializeField] private float previewCardScale = 3f;

    [Header("열린 문 빛 이펙트 대상")]
    [SerializeField] private Image[] openedDoorLightImages;

    [Header("배경 이미지 슬롯")]
    [SerializeField] private Sprite closedDoorWaitingSprite;
    [SerializeField] private Sprite openedDoorEffectSprite;
    [SerializeField] private Sprite resultBackgroundSprite;

    [Header("빛 이펙트 슬롯")]
    [Tooltip("문이 열린 뒤에만 표시할 빛 이미지")]
    [SerializeField] private Sprite lightEffectSprite;

    [Header("빛 움직임")]
    [Min(0f)] [SerializeField] private float lightDriftDistance = 6f;
    [Min(0f)] [SerializeField] private float lightDriftSpeed = 1.4f;

    [Header("연출 시간")]
    [Min(0.1f)] [SerializeField] private float totalDuration = 1.8f;
    [Range(0.05f, 0.9f)] [SerializeField] private float openedDoorPhaseRatio = 0.45f;

    public bool IsPlaying => playRoutine != null || isWaitingForTap;
    public bool CanPlay => sequenceBackgroundImage != null && closedDoorWaitingSprite != null &&
                           openedDoorEffectSprite != null;
    public Sprite ResultBackgroundSprite => resultBackgroundSprite;

    private Coroutine playRoutine;
    private bool skipRequested;
    private bool isWaitingForTap;
    private Action onCompleted;
    private GachaResultCardView previewCard;
    private readonly Dictionary<RectTransform, Vector2> lightBasePositions = new Dictionary<RectTransform, Vector2>();

    private static Sprite runtimeGlowSprite;

    private void Awake()
    {
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(Skip);
        }

        if (tapToContinueButton != null)
        {
            tapToContinueButton.onClick.AddListener(ContinueFromTap);
        }

        ConfigureLightEffectImage();
        SetTouchPromptVisible(false);
        SetTenPullPreviewVisible(false);
        SetOverlayVisible(false);
    }

    private void OnDestroy()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(Skip);
        }

        if (tapToContinueButton != null)
        {
            tapToContinueButton.onClick.RemoveListener(ContinueFromTap);
        }
    }

    // 확정된 결과를 기준으로 한 번의 공통 길이 연출을 시작함
    public void Play(GachaDrawResult result, Action completed)
    {
        if (result == null || !CanPlay)
        {
            completed?.Invoke();
            return;
        }

        Cancel();
        ConfigureLightEffectImage();
        onCompleted = completed;
        skipRequested = false;
        playRoutine = StartCoroutine(PlayRoutine(result, ResolveRevealGrade(result)));
    }

    // 스킵은 결과를 바꾸지 않고 남은 연출만 건너뜀
    public void Skip()
    {
        if (IsPlaying)
        {
            skipRequested = true;
        }
    }

    // 패널을 닫는 등 연출을 중단할 때 콜백 없이 UI만 정리함
    public void Cancel()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        skipRequested = false;
        isWaitingForTap = false;
        onCompleted = null;
        SetTouchPromptVisible(false);
        ClearPreviewCard();
        SetTenPullPreviewVisible(false);
        SetOverlayVisible(false);
    }

    private IEnumerator PlayRoutine(GachaDrawResult result, RevealGrade grade)
    {
        Color revealColor = GetRevealColor(grade);

        SetOverlayVisible(true);
        SetStage(closedDoorWaitingSprite, 1f, Vector3.one);

        // 닫힌 문 대기 중에도 결과를 은은하게 암시함
        SetOpenedDoorLights(revealColor, 0.12f, Vector3.one);

        // 닫힌 문 상태에서 클릭 대기
        yield return WaitForTapAtClosedDoor(grade);

        // 클릭 후 열린 문 연출 재생
        if (!skipRequested)
        {
            float openedDuration = totalDuration * openedDoorPhaseRatio;
            yield return PlayOpenedDoorPhase(openedDuration, revealColor);
        }

        if (!skipRequested && result.PullResults.Count == 10)
        {
            yield return PlayTenPullCardPreview(result);
        }

        Complete();
    }

    private IEnumerator PlayOpenedDoorPhase(float duration, Color flashColor)
    {
        for (float elapsed = 0f; elapsed < duration && !skipRequested; elapsed += Time.unscaledDeltaTime)
        {
            float progress = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            SetStage(openedDoorEffectSprite, 1f, Vector3.Lerp(Vector3.one, Vector3.one * 1.08f, progress));
            SetOpenedDoorLights(flashColor, 1f, Vector3.one * Mathf.Lerp(1f, 1.08f, progress));
            yield return null;
        }
    }

    // 닫힌 문에서 희미한 빛을 유지하며 플레이어 입력을 기다림
    private IEnumerator WaitForTapAtClosedDoor(RevealGrade grade)
    {
        Color lightColor = GetRevealColor(grade);
        isWaitingForTap = true;
        SetTouchPromptVisible(true);

        SetOpenedDoorLights(lightColor, 0.12f, Vector3.one);

        while (isWaitingForTap && !skipRequested)
        {
            SetStage(closedDoorWaitingSprite, 1f, Vector3.one);
            float pulse = (Mathf.Sin(Time.unscaledTime * 1.5f) + 1f) * 0.5f;
            SetOpenedDoorLights(
                lightColor,
                Mathf.Lerp(0.1f, 0.16f, pulse),
                Vector3.one * Mathf.Lerp(1f, 1.02f, pulse));
            yield return null;
        }

        SetTouchPromptVisible(false);
    }

    // 10회 소환 결과를 한 장씩 크게 보여준 뒤 기존 결과 그리드로 넘김
    private IEnumerator PlayTenPullCardPreview(GachaDrawResult result)
    {
        if (tenPullPreviewRoot == null || tenPullPreviewContent == null || previewCardTemplate == null)
        {
            yield break;
        }

        SetTenPullPreviewVisible(true);

        foreach (GachaPullResult pullResult in result.PullResults)
        {
            if (skipRequested)
            {
                break;
            }

            ClearPreviewCard();
            previewCard = Instantiate(previewCardTemplate, tenPullPreviewContent);
            previewCard.gameObject.SetActive(true);
            previewCard.Bind(
                pullResult,
                GetHeroName(pullResult.HeroId),
                GetHeroPortrait(pullResult.HeroId),
                showDuplicateGoldIcon: false,
                showText: false);

            RectTransform cardRect = previewCard.transform as RectTransform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;

            for (float elapsed = 0f; elapsed < previewCardDuration && !skipRequested; elapsed += Time.unscaledDeltaTime)
            {
                float progress = previewCardDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / previewCardDuration);
                float popProgress = Mathf.Clamp01(progress / 0.6f);
                float eased = Mathf.SmoothStep(0f, 1f, popProgress);
                cardRect.localScale = Vector3.one * Mathf.Lerp(0.7f, previewCardScale, eased);
                yield return null;
            }

            if (previewCard != null)
            {
                previewCard.transform.localScale = Vector3.one * previewCardScale;
            }
        }

        ClearPreviewCard();
        SetTenPullPreviewVisible(false);
    }

    private void Complete()
    {
        playRoutine = null;
        isWaitingForTap = false;
        SetTouchPromptVisible(false);
        ClearPreviewCard();
        SetTenPullPreviewVisible(false);
        SetOverlayVisible(false);

        Action completed = onCompleted;
        onCompleted = null;
        completed?.Invoke();
    }

    // 마지막 연출 대기 중에만 오버레이 클릭을 결과 표시로 진행함
    private void ContinueFromTap()
    {
        if (isWaitingForTap)
        {
            isWaitingForTap = false;
        }
    }

    private void SetStage(Sprite sprite, float alpha, Vector3 scale)
    {
        if (sequenceBackgroundImage == null)
        {
            return;
        }

        sequenceBackgroundImage.sprite = sprite;
        sequenceBackgroundImage.preserveAspect = true;
        Color color = sequenceBackgroundImage.color;
        color.a = alpha;
        sequenceBackgroundImage.color = color;
        sequenceBackgroundImage.rectTransform.localScale = scale;
    }

    private static Sprite GetRuntimeGlowSprite()
    {
        if (runtimeGlowSprite != null)
        {
            return runtimeGlowSprite;
        }

        const int size = 128;

        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "Runtime_GachaGlow";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];

        float center = (size - 1) * 0.5f;
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / radius;
                float dy = (y - center) / radius;

                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01(1f - distance);

                // 중심은 밝고 바깥으로 갈수록 부드럽게 사라짐
                alpha = Mathf.SmoothStep(0f, 1f, alpha);
                alpha = Mathf.Pow(alpha, 1.6f);

                pixels[y * size + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        runtimeGlowSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f)
        );

        runtimeGlowSprite.name = "Runtime_GachaGlowSprite";

        return runtimeGlowSprite;
    }

    private void ConfigureLightEffectImage()
    {
        ConfigureLightEffectImages(openedDoorLightImages, flashImage);
    }

    private void ConfigureLightEffectImages(Image[] lightImages, Image fallbackImage)
    {
        bool hasAssignedImage = false;
        if (lightImages != null)
        {
            foreach (Image lightImage in lightImages)
            {
                if (lightImage == null)
                {
                    continue;
                }

                ConfigureLightEffectImage(lightImage);
                hasAssignedImage = true;
            }
        }

        if (!hasAssignedImage && fallbackImage != null)
        {
            ConfigureLightEffectImage(fallbackImage);
        }
    }

    private void ConfigureLightEffectImage(Image lightImage)
    {
        if (lightImage == null)
        {
            return;
        }

        if (lightImage.sprite == null)
        {
            lightImage.sprite = lightEffectSprite != null
                ? lightEffectSprite
                : GetRuntimeGlowSprite();
        }

        lightImage.preserveAspect = true;
        lightImage.raycastTarget = false;

        CacheLightBasePosition(lightImage.rectTransform);
    }

    private void SetOpenedDoorLights(Color color, float alpha, Vector3 scale)
    {
        SetLightImages(openedDoorLightImages, flashImage, color, alpha, scale);
    }

    private void SetLightImages(Image[] lightImages, Image fallbackImage, Color color, float alpha, Vector3 scale)
    {
        bool hasAssignedImage = false;
        int lightIndex = 0;
        if (lightImages != null)
        {
            foreach (Image lightImage in lightImages)
            {
                if (lightImage == null)
                {
                    continue;
                }

                SetLightImage(lightImage, color, alpha, scale, lightIndex++);
                hasAssignedImage = true;
            }
        }

        if (!hasAssignedImage && fallbackImage != null)
        {
            SetLightImage(fallbackImage, color, alpha, scale, 0);
        }
    }

    private void SetLightImage(Image lightImage, Color color, float alpha, Vector3 scale, int lightIndex)
    {
        color.a = alpha;
        lightImage.color = color;
        RectTransform rectTransform = lightImage.rectTransform;
        rectTransform.localScale = scale;

        CacheLightBasePosition(rectTransform);
        if (alpha <= 0f)
        {
            rectTransform.anchoredPosition = lightBasePositions[rectTransform];
            return;
        }

        float phase = Time.unscaledTime * lightDriftSpeed + lightIndex * 1.73f;
        Vector2 drift = new Vector2(Mathf.Sin(phase), Mathf.Cos(phase * 1.31f) * 0.55f) * lightDriftDistance;
        rectTransform.anchoredPosition = lightBasePositions[rectTransform] + drift;
    }

    private void CacheLightBasePosition(RectTransform rectTransform)
    {
        if (rectTransform != null && !lightBasePositions.ContainsKey(rectTransform))
        {
            lightBasePositions.Add(rectTransform, rectTransform.anchoredPosition);
        }
    }

    private void SetOverlayVisible(bool isVisible)
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(isVisible);
        }
    }

    private void SetTouchPromptVisible(bool isVisible)
    {
        if (touchToGachaPrompt != null)
        {
            touchToGachaPrompt.SetActive(isVisible);
        }
    }

    private void SetTenPullPreviewVisible(bool isVisible)
    {
        if (tenPullPreviewRoot != null)
        {
            tenPullPreviewRoot.SetActive(isVisible);
        }
    }

    private void ClearPreviewCard()
    {
        if (previewCard != null)
        {
            Destroy(previewCard.gameObject);
            previewCard = null;
        }
    }

    private static string GetHeroName(string heroId)
    {
        return HeroManager.Instance != null && HeroManager.Instance.IsInitialized &&
               HeroManager.Instance.Controller.TryGetHero(heroId, out OwnedHeroData hero)
            ? hero.HeroData.UnitName
            : heroId;
    }

    private static Sprite GetHeroPortrait(string heroId)
    {
        return HeroManager.Instance != null && HeroManager.Instance.IsInitialized &&
               HeroManager.Instance.Controller.TryGetHero(heroId, out OwnedHeroData hero)
            ? hero.HeroData.Portrait
            : null;
    }

    private static RevealGrade ResolveRevealGrade(GachaDrawResult result)
    {
        bool hasPickupTier2 = result.PullResults.Any(pull => pull.IsPickup && pull.Rarity == GachaRarity.Tier2);
        if (hasPickupTier2)
        {
            return RevealGrade.PickupTier2;
        }

        return result.PullResults.Any(pull => pull.Rarity == GachaRarity.Tier2)
            ? RevealGrade.Tier2
            : RevealGrade.Tier1;
    }

    private static Color GetRevealColor(RevealGrade grade) => grade switch
    {
        RevealGrade.PickupTier2 => new Color(1f, 0.2f, 0.16f),
        RevealGrade.Tier2 => Color.white,
        _ => new Color(0.08f, 0.24f, 0.85f)
    };
}
