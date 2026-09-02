using System.Collections;
using UnityEngine;

// 메인 화면 진입 후 상점 데이터가 준비되면 공용 미구매 패키지 안내를 시작함
public sealed class MainPackageNoticeEntry : MonoBehaviour
{
    [SerializeField] private ShopPanelPresenter shopPanelPresenter;
    [SerializeField] private GameObject mainBottomPanelRoot;
    [SerializeField] private GameObject allPanelRoot;
    [SerializeField] private GameObject allMenuRoot;

    public static MainPackageNoticeEntry Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private IEnumerator Start()
    {
        while (!ShopManager.TryGetExistingInstance(out ShopManager shopManager) || !shopManager.IsInitialized)
        {
            yield return null;
        }

        shopPanelPresenter?.ShowUnpurchasedPackageNotices();
    }

    // 기존 메인 메뉴 컨트롤러를 변경하지 않고 패키지 안내의 구매 확인 화면만 열어줌
    public bool OpenShopForPackageNotice(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId) || shopPanelPresenter == null ||
            allPanelRoot == null || allMenuRoot == null)
        {
            return false;
        }

        allPanelRoot.SetActive(true);
        allMenuRoot.SetActive(false);

        shopPanelPresenter.OpenPurchaseConfirmFromPackageNotice(productId);
        shopPanelPresenter.gameObject.SetActive(true);
        shopPanelPresenter.PlayOpenAnimation();

        if (mainBottomPanelRoot != null)
        {
            mainBottomPanelRoot.SetActive(false);
        }

        return true;
    }
}
