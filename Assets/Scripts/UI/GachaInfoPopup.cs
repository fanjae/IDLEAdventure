using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 현재 배너의 확률과 천장 정보를 보여줄 팝업 골격임
public sealed class GachaInfoPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Button closeButton;

    // 닫기 버튼 클릭을 팝업 숨김에 연결함
    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
    }

    // 배너 데이터로 확률 안내 문구를 구성해 표시함
    public void Show(GachaBannerDataSO banner)
    {
        if (popupRoot == null || banner == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = $"{banner.DisplayName} 확률 정보";
        }

        if (contentText != null)
        {
            contentText.text = $"1티어 {banner.Tier1Weight}%\n2티어 {banner.Tier2Weight}%\n{banner.Tier2PityCount}회 내 2티어 확정\n픽업 가중치 x{banner.PickupWeightMultiplier:0.#}\n중복 전환: 1티어 {banner.GetDuplicateGold(GachaRarity.Tier1)} 골드 / 2티어 {banner.GetDuplicateGold(GachaRarity.Tier2)} 골드";
        }

        popupRoot.SetActive(true);
    }

    // 확률 안내 팝업을 숨김
    public void Hide()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
    }
}
