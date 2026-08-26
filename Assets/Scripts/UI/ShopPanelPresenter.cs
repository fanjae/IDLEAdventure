using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상점 패널의 실제 프리팹 오브젝트와 데이터 표시를 연결함
public sealed class ShopPanelPresenter : MonoBehaviour
{
    private enum ShopTab { Exchange, Package, Attendance }

    [Header("탭 버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button exchangeTabButton;
    [SerializeField] private Button packageTabButton;
    [SerializeField] private Button attendanceTabButton;

    [Header("탭 페이지")]
    [SerializeField] private GameObject exchangePage;
    [SerializeField] private GameObject packagePage;
    [SerializeField] private GameObject attendancePage;
    [SerializeField] private Transform exchangeContent;
    [SerializeField] private Transform packageContent;
    [SerializeField] private ShopFeaturedProductView exchangeFeaturedBanner;
    [SerializeField] private ShopFeaturedProductView packageFeaturedBanner;

    [Header("출석 선물")]
    [SerializeField] private Transform attendanceContent;
    [SerializeField] private ShopAttendanceRewardView attendanceRewardTemplate;

    [Header("구매 확인")]
    [SerializeField] private GameObject purchaseConfirmPopup;
    [SerializeField] private Image purchaseConfirmIcon;
    [SerializeField] private TMP_Text purchaseConfirmNameText;
    [SerializeField] private TMP_Text purchaseConfirmDescriptionText;
    [SerializeField] private TMP_Text purchaseConfirmPriceText;
    [SerializeField] private Button purchaseConfirmButton;
    [SerializeField] private Button purchaseCancelButton;

    [Header("공통")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite gemIcon;

    private readonly List<ShopFeaturedProductView> generatedBanners = new();
    private readonly List<ShopAttendanceRewardView> generatedAttendanceCards = new();
    private ShopController subscribedController;
    private ShopTab selectedTab = ShopTab.Exchange;
    private string pendingProductId;

    // 프리팹에 연결한 버튼 이벤트를 등록함
    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        exchangeTabButton.onClick.AddListener(OpenExchangeTab);
        packageTabButton.onClick.AddListener(OpenPackageTab);
        attendanceTabButton.onClick.AddListener(OpenAttendanceTab);
        purchaseConfirmButton.onClick.AddListener(ConfirmPurchase);
        purchaseCancelButton.onClick.AddListener(ClosePurchaseConfirm);
        purchaseConfirmPopup.SetActive(false);
    }

    // 페이지가 켜지면 상점 상태 변경을 구독함
    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    // 페이지가 꺼지면 이벤트 구독을 해제함
    private void OnDisable() => Unsubscribe();

    // 뒤로가기 기록을 사용해 상점 페이지를 닫음
    public void Close()
    {
        if (MainUIController.Instance != null)
        {
            MainUIController.Instance.GoBack();
            return;
        }

        gameObject.SetActive(false);
    }

    // 교환소 상품 목록을 표시함
    public void OpenExchangeTab()
    {
        selectedTab = ShopTab.Exchange;
        Refresh();
    }

    // 한정 패키지 목록을 표시함
    public void OpenPackageTab()
    {
        selectedTab = ShopTab.Package;
        Refresh();
    }

    // 오늘 출석 선물을 표시함
    public void OpenAttendanceTab()
    {
        selectedTab = ShopTab.Attendance;
        Refresh();
    }

    // 현재 탭과 저장된 상점 상태를 실제 UI에 반영함
    public void Refresh()
    {
        bool isReady = ShopManager.TryGetExistingInstance(out ShopManager shopManager) && shopManager.IsInitialized;
        SetTabVisible(exchangePage, selectedTab == ShopTab.Exchange);
        SetTabVisible(packagePage, selectedTab == ShopTab.Package);
        SetTabVisible(attendancePage, selectedTab == ShopTab.Attendance);
        ClearGeneratedBanners();
        ClearGeneratedAttendanceCards();

        if (!isReady)
        {
            ShowStatus("상점 데이터를 불러오는 중임");
            return;
        }

        ShopController controller = shopManager.Controller;
        if (selectedTab == ShopTab.Attendance)
            RefreshAttendance(controller);
        else
        {
            ShopProductCategory category = selectedTab == ShopTab.Exchange ? ShopProductCategory.Exchange : ShopProductCategory.Package;
            RefreshProducts(controller, category, selectedTab == ShopTab.Exchange ? exchangeContent : packageContent,
                selectedTab == ShopTab.Exchange ? exchangeFeaturedBanner : packageFeaturedBanner);
        }

        ShowStatus(string.Empty);
    }

    // 선택한 분류의 모든 상품을 큰 배너 카드로 생성함
    private void RefreshProducts(ShopController controller, ShopProductCategory category, Transform content, ShopFeaturedProductView featuredBanner)
    {
        if (featuredBanner == null || content == null)
        {
            ShowStatus("상품 배너 프리팹 연결 필요함");
            return;
        }

        List<ShopProductSO> products = controller.Products.Where(item => item != null && item.IsVisible && item.Category == category).OrderBy(item => item.DisplayOrder).ToList();
        featuredBanner.gameObject.SetActive(products.Count > 0);
        for (int index = 0; index < products.Count; index++)
        {
            ShopFeaturedProductView banner = index == 0 ? featuredBanner : Instantiate(featuredBanner, content);
            banner.gameObject.SetActive(true);
            banner.Bind(products[index], OpenPurchaseConfirm);

            if (index > 0)
                generatedBanners.Add(banner);
        }
    }

    // 출석 보상 SO 길이에 맞춰 템플릿을 복제하고 각 날짜 상태를 표시함
    private void RefreshAttendance(ShopController controller)
    {
        if (attendanceContent == null || attendanceRewardTemplate == null)
        {
            ShowStatus("출석 보상 템플릿 연결 필요함");
            return;
        }

        attendanceRewardTemplate.gameObject.SetActive(false);
        for (int index = 0; index < controller.AttendanceRewards.Count; index++)
        {
            ShopRewardEntry reward = controller.AttendanceRewards[index];
            if (reward == null)
                continue;

            int rewardIndex = index;
            ShopAttendanceRewardState state = controller.GetAttendanceRewardState(rewardIndex);
            string stateText = state.IsClaimed ? "수령 완료" : state.IsClaimable ? "수령 가능" : "아직 미해금";
            ShopAttendanceRewardView card = Instantiate(attendanceRewardTemplate, attendanceContent);
            card.gameObject.SetActive(true);
            card.Bind(rewardIndex + 1, reward, GetRewardIcon(reward), stateText, state.IsClaimable, () => ClaimAttendance(rewardIndex));
            generatedAttendanceCards.Add(card);
        }
    }

    // 상점 컨트롤러의 갱신 이벤트를 화면 갱신에 연결함
    private void Subscribe()
    {
        if (subscribedController != null || !ShopManager.TryGetExistingInstance(out ShopManager shopManager) || !shopManager.IsInitialized)
            return;

        subscribedController = shopManager.Controller;
        subscribedController.OnShopStateChanged += Refresh;
    }

    // 상점 컨트롤러 이벤트 구독을 해제함
    private void Unsubscribe()
    {
        if (subscribedController == null)
            return;

        subscribedController.OnShopStateChanged -= Refresh;
        subscribedController = null;
    }

    // 선택한 상품 정보를 확인 창에 표시함
    private void OpenPurchaseConfirm(string productId)
    {
        if (!ShopManager.TryGetExistingInstance(out ShopManager shopManager) || !shopManager.IsInitialized)
        {
            ShowStatus("상점 데이터를 불러오는 중임");
            return;
        }

        ShopProductSO product = shopManager.Controller.Products.FirstOrDefault(item => item != null && item.ProductId == productId);
        if (product == null)
        {
            ShowStatus("상품 정보를 찾을 수 없음");
            return;
        }

        ShopProductAvailability availability = shopManager.Controller.GetAvailability(productId);
        if (!availability.CanPurchase)
        {
            ShowStatus(GetFailureText(availability.Failure));
            return;
        }

        pendingProductId = productId;
        purchaseConfirmIcon.sprite = product.Icon != null ? product.Icon : product.Artwork != null ? product.Artwork : GetCurrencyIcon(product.PriceCurrency);
        purchaseConfirmNameText.text = product.DisplayName;
        purchaseConfirmDescriptionText.text = product.Description;
        purchaseConfirmPriceText.text = ShopProductCardView.GetPriceText(product);
        purchaseConfirmPopup.SetActive(true);
    }

    // 확인한 상품 구매를 요청하고 결과 문구를 표시함
    private void ConfirmPurchase()
    {
        if (!ShopManager.TryGetExistingInstance(out ShopManager shopManager) || !shopManager.IsInitialized)
        {
            ShowStatus("상점 데이터를 불러오는 중임");
            return;
        }

        if (string.IsNullOrEmpty(pendingProductId))
        {
            ClosePurchaseConfirm();
            return;
        }

        if (!shopManager.Controller.TryPurchase(pendingProductId, out _, out ShopFailure failure))
        {
            ShowStatus(GetFailureText(failure));
            return;
        }

        ClosePurchaseConfirm();
        ShowStatus("구매 완료");
    }

    // 구매 확인 창을 닫고 선택 정보를 비움
    private void ClosePurchaseConfirm()
    {
        pendingProductId = null;
        purchaseConfirmPopup.SetActive(false);
    }

    // 선택한 날짜의 열린 출석 선물 수령을 요청함
    private void ClaimAttendance(int rewardIndex)
    {
        if (!ShopManager.TryGetExistingInstance(out ShopManager shopManager) || !shopManager.IsInitialized)
        {
            ShowStatus("상점 데이터를 불러오는 중임");
            return;
        }

        if (!shopManager.Controller.TryClaimAttendance(rewardIndex, out _, out ShopFailure failure))
        {
            ShowStatus(GetFailureText(failure));
            return;
        }

        ShowStatus("출석 선물 수령 완료");
    }

    // 이전 목록 배너 복제본을 제거함
    private void ClearGeneratedBanners()
    {
        foreach (ShopFeaturedProductView banner in generatedBanners)
            if (banner != null)
                Destroy(banner.gameObject);

        generatedBanners.Clear();
    }

    // 이전 출석 보상 템플릿 복제본을 제거함
    private void ClearGeneratedAttendanceCards()
    {
        foreach (ShopAttendanceRewardView card in generatedAttendanceCards)
            if (card != null)
                Destroy(card.gameObject);

        generatedAttendanceCards.Clear();
    }

    // 선택한 탭만 켜고 나머지는 숨김
    private static void SetTabVisible(GameObject page, bool isVisible)
    {
        if (page != null)
            page.SetActive(isVisible);
    }

    // 재화 아이콘을 반환함
    private Sprite GetCurrencyIcon(CurrencyType type) => type == CurrencyType.GEM ? gemIcon : goldIcon;

    // 보상에 맞는 아이콘을 반환함
    private Sprite GetRewardIcon(ShopRewardEntry reward)
    {
        if (reward != null && reward.RewardType == ShopRewardType.Hero)
            return reward.HeroData != null ? reward.HeroData.Portrait : null;

        return reward == null ? null : GetCurrencyIcon(reward.CurrencyType);
    }

    // 실패값을 사용자 문구로 변환함
    private static string GetFailureText(ShopFailure failure) => failure switch
    {
        ShopFailure.NotEnoughCurrency => "재화가 부족함",
        ShopFailure.AlreadyPurchased => "이미 구매한 상품임",
        ShopFailure.DailyPurchaseLimitReached => "오늘 구매 횟수를 모두 사용함",
        ShopFailure.AttendanceAlreadyClaimed => "오늘 출석 선물을 이미 받음",
        ShopFailure.AttendanceNotAvailableYet => "아직 수령할 수 없는 날짜임",
        ShopFailure.RewardHeroAlreadyOwned => "이미 보유한 영웅이라 구매할 수 없음",
        _ => "처리할 수 없음"
    };

    // 하단 상태 문구를 변경함
    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
