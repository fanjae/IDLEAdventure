using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 가챠 배너 선택, 소환 버튼, 결과 오버레이 흐름을 관리함
public sealed class GachaPanelPresenter : MonoBehaviour
{
    [SerializeField] private string bannerId = "Standard";
    [SerializeField] private GachaDrawButton singleDrawButton;
    [SerializeField] private GachaDrawButton tenDrawButton;
    [SerializeField] private Image bannerArtworkImage;
    [SerializeField] private TMP_Text bannerNameText;
    [SerializeField] private TMP_Text bannerDescriptionText;
    [SerializeField] private TMP_Text periodText;
    [SerializeField] private TMP_Text pityText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GachaBannerTabList bannerTabList;
    [SerializeField] private GachaRevealSequence revealSequence;
    [SerializeField] private GachaResultOverlay resultOverlay;
    [SerializeField] private GachaInfoPopup infoPopup;
    [SerializeField] private GameObject probabilityButton;

    private GachaDrawButton subscribedSingleButton;
    private GachaDrawButton subscribedTenButton;
    private bool isBannerListInitialized;
    private bool isDrawFlowLocked;

    // 패널이 열릴 때 소환 버튼과 탭 알림을 연결함
    private void OnEnable()
    {
        BindButtons();
        BindBannerTabList();
        BindResultOverlay();
        RefreshPanel();
    }

    // 최초 표시 시 배너 탭을 생성하고 현재 정보를 표시함
    private void Start()
    {
        BindButtons();
        BindBannerTabList();
        InitializeBannerTabs();
        RefreshPanel();
    }

    // 패널이 닫힐 때 중복 구독을 막기 위해 해제함
    private void OnDisable()
    {
        revealSequence?.Cancel();
        SetDrawFlowLocked(false);
        UnbindButtons();
        UnbindBannerTabList();
        UnbindResultOverlay();
    }

    // 외부 배너 탭이 호출할 선택 진입점임
    public void SelectBanner(string targetBannerId)
    {
        if (isDrawFlowLocked)
        {
            return;
        }

        if (!TryGetBanner(targetBannerId, out _))
        {
            ShowStatus("선택할 수 없는 배너임");
            return;
        }

        bannerId = targetBannerId;
        bannerTabList?.SetSelected(bannerId);
        RefreshPanel();
    }

    // 확률 안내 버튼이 호출할 현재 배너 정보 표시 진입점임
    public void OpenProbabilityInfo()
    {
        if (TryGetBanner(bannerId, out GachaBannerDataSO banner))
        {
            infoPopup?.Show(banner);
        }
    }

    // 결과 오버레이의 계속 버튼과 연결할 닫기 진입점임
    public void CloseResult()
    {
        resultOverlay?.Close();
    }

    // 버튼 소환 성공 알림을 한 번만 연결함
    private void BindButtons()
    {
        if (singleDrawButton != null && subscribedSingleButton != singleDrawButton)
        {
            UnbindSingleButton();
            subscribedSingleButton = singleDrawButton;
            subscribedSingleButton.DrawCompleted += HandleDrawCompleted;
            subscribedSingleButton.DrawFailed += ShowDrawFailure;
        }

        if (tenDrawButton != null && subscribedTenButton != tenDrawButton)
        {
            UnbindTenButton();
            subscribedTenButton = tenDrawButton;
            subscribedTenButton.DrawCompleted += HandleDrawCompleted;
            subscribedTenButton.DrawFailed += ShowDrawFailure;
        }
    }

    // 모든 버튼 알림 구독 해제함
    private void UnbindButtons()
    {
        UnbindSingleButton();
        UnbindTenButton();
    }

    // 1회 소환 버튼 알림 해제함
    private void UnbindSingleButton()
    {
        if (subscribedSingleButton == null)
        {
            return;
        }

        subscribedSingleButton.DrawCompleted -= HandleDrawCompleted;
        subscribedSingleButton.DrawFailed -= ShowDrawFailure;
        subscribedSingleButton = null;
    }

    // 10회 소환 버튼 알림 해제함
    private void UnbindTenButton()
    {
        if (subscribedTenButton == null)
        {
            return;
        }

        subscribedTenButton.DrawCompleted -= HandleDrawCompleted;
        subscribedTenButton.DrawFailed -= ShowDrawFailure;
        subscribedTenButton = null;
    }

    // 배너 탭 목록 클릭 알림 연결함
    private void BindBannerTabList()
    {
        if (bannerTabList != null)
        {
            bannerTabList.BannerSelected -= SelectBanner;
            bannerTabList.BannerSelected += SelectBanner;
        }
    }

    // 배너 탭 목록 클릭 알림 해제함
    private void UnbindBannerTabList()
    {
        if (bannerTabList != null)
        {
            bannerTabList.BannerSelected -= SelectBanner;
        }
    }

    // 결과 요약을 닫은 뒤에만 다음 소환 입력을 허용함
    private void BindResultOverlay()
    {
        if (resultOverlay != null)
        {
            resultOverlay.Closed -= HandleResultOverlayClosed;
            resultOverlay.Closed += HandleResultOverlayClosed;
        }
    }

    private void UnbindResultOverlay()
    {
        if (resultOverlay != null)
        {
            resultOverlay.Closed -= HandleResultOverlayClosed;
        }
    }

    // 실제 데이터베이스 기준으로 노출 배너 탭을 처음 한 번 생성함
    private void InitializeBannerTabs()
    {
        if (isBannerListInitialized || bannerTabList == null || GachaManager.Instance == null || !GachaManager.Instance.IsInitialized)
        {
            return;
        }

        bannerTabList.Initialize(GachaManager.Instance.Controller.Banners, bannerId);
        isBannerListInitialized = true;
    }

    // 현재 배너 데이터에 맞춰 소환 버튼과 안내 문구를 갱신함
    private void RefreshPanel()
    {
        InitializeBannerTabs();

        if (!TryGetBanner(bannerId, out GachaBannerDataSO banner))
        {
            return;
        }

        singleDrawButton?.Configure(banner.BannerId, 1);
        tenDrawButton?.Configure(banner.BannerId, 10);

        if (bannerArtworkImage != null)
        {
            bannerArtworkImage.sprite = banner.BannerArtwork;
            bannerArtworkImage.preserveAspect = true;
            bannerArtworkImage.gameObject.SetActive(bannerArtworkImage.sprite != null);
        }

        if (bannerNameText != null)
        {
            bannerNameText.text = banner.DisplayName;
        }

        if (bannerDescriptionText != null)
        {
            bannerDescriptionText.text = banner.Description;
        }

        if (periodText != null)
        {
            periodText.text = banner.PeriodText;
        }

        RefreshPityText();
    }

    // 현재 천장까지 남은 횟수를 표시함
    private void RefreshPityText()
    {
        if (pityText == null || GachaManager.Instance == null || !GachaManager.Instance.IsInitialized)
        {
            return;
        }

        if (GachaManager.Instance.Controller.TryGetPullsUntilTier2Pity(bannerId, out int pullsUntilPity))
        {
            pityText.text = $"2티어 확정까지 {pullsUntilPity}회";
        }
    }

    // 소환 결과를 확정한 뒤, 연출 완료 시점까지 요약 화면 표시를 지연함
    private void HandleDrawCompleted(GachaDrawResult result)
    {
        if (result == null)
        {
            return;
        }

        SetDrawFlowLocked(true);
        if (revealSequence != null && revealSequence.CanPlay)
        {
            revealSequence.Play(result, () => ShowDrawResult(result, revealSequence.ResultBackgroundSprite));
            return;
        }

        ShowDrawResult(result);
    }

    // 결과 오버레이가 있으면 카드 목록으로, 없으면 임시 텍스트로 표시함
    private void ShowDrawResult(GachaDrawResult result) => ShowDrawResult(result, null);

    private void ShowDrawResult(GachaDrawResult result, Sprite resultBackgroundSprite)
    {
        if (resultOverlay != null)
        {
            resultOverlay.Show(result, GetHeroName, GetHeroPortrait, resultBackgroundSprite);
        }
        else
        {
            StringBuilder builder = new("소환 결과");
            foreach (GachaPullResult pullResult in result.PullResults)
            {
                builder.Append('\n').Append(GetHeroName(pullResult.HeroId));
            }

            ShowStatus(builder.ToString());
        }

        RefreshPityText();

        if (resultOverlay == null || !resultOverlay.IsOpen)
        {
            SetDrawFlowLocked(false);
        }
    }

    // 실패 원인을 사용자용 문구로 표시함
    private void ShowDrawFailure(GachaDrawFailure failure)
    {
        string status = failure switch
        {
            GachaDrawFailure.NotEnoughGem => "보석이 부족함",
            GachaDrawFailure.InvalidDrawCount => "1회 또는 10회 소환만 가능함",
            GachaDrawFailure.InvalidPool => "소환 영웅 풀이 설정되지 않음",
            GachaDrawFailure.HeroSystemUnavailable => "영웅 데이터를 불러오는 중임",
            GachaDrawFailure.HeroDataNotFound => "소환 영웅 데이터를 찾을 수 없음",
            GachaDrawFailure.HeroGrantFailed => "영웅 지급에 실패함",
            _ => "소환을 진행할 수 없음"
        };

        ShowStatus(status);
        SetDrawFlowLocked(false);
    }

    // 결과 요약이 닫힐 때 연출 흐름 입력 잠금을 해제함
    private void HandleResultOverlayClosed()
    {
        if (isDrawFlowLocked)
        {
            SetDrawFlowLocked(false);
        }
    }

    // 소환 중 배너 변경과 중복 소환을 함께 막음
    private void SetDrawFlowLocked(bool isLocked)
    {
        isDrawFlowLocked = isLocked;
        singleDrawButton?.SetInputLocked(isLocked);
        tenDrawButton?.SetInputLocked(isLocked);
        probabilityButton?.SetActive(!isLocked);
    }

    // 임시 상태 텍스트를 갱신함
    private void ShowStatus(string status)
    {
        if (resultText != null)
        {
            resultText.text = status;
        }
    }

    // 현재 런타임 가챠 데이터베이스에서 배너를 조회함
    private static bool TryGetBanner(string targetBannerId, out GachaBannerDataSO banner)
    {
        banner = null;
        return GachaManager.Instance != null && GachaManager.Instance.IsInitialized &&
               GachaManager.Instance.Controller.TryGetBannerData(targetBannerId, out banner);
    }

    // 보유 영웅 데이터에서 화면 표시 이름을 조회함
    private static string GetHeroName(string heroId)
    {
        if (HeroManager.Instance != null && HeroManager.Instance.IsInitialized &&
            HeroManager.Instance.Controller.TryGetHero(heroId, out OwnedHeroData hero))
        {
            return hero.HeroData.UnitName;
        }

        return heroId;
    }

    // 결과 카드가 영웅 데이터에 지정된 초상화를 사용하도록 조회함
    private static Sprite GetHeroPortrait(string heroId)
    {
        return HeroManager.Instance != null && HeroManager.Instance.IsInitialized &&
               HeroManager.Instance.Controller.TryGetHero(heroId, out OwnedHeroData hero)
            ? hero.HeroData.Portrait
            : null;
    }
}
