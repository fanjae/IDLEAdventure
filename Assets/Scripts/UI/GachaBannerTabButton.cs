using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 배너 선택 목록의 한 항목을 표시하고 클릭을 전달함
public sealed class GachaBannerTabButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private AspectRatioFitter thumbnailAspectFitter;
    [SerializeField] private Image selectedImage;

    private string bannerId;

    public string BannerId => bannerId;
    public event Action<string> Clicked;

    // 버튼 컴포넌트가 누락된 경우 같은 오브젝트에서 찾음
    private void Awake()
    {
        button ??= GetComponent<Button>();
    }

    // 런타임 클릭 알림 연결함
    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    // 비활성화 시 중복 클릭 알림 해제함
    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    // 배너 데이터로 탭 문구와 선택 상태 갱신함
    public void Configure(GachaBannerDataSO banner, bool isSelected)
    {
        bannerId = banner != null ? banner.BannerId : string.Empty;

        if (label != null)
        {
            label.text = banner != null ? banner.DisplayName : string.Empty;
        }

        if (thumbnailImage != null)
        {
            thumbnailImage.sprite = banner != null ? banner.TabThumbnail : null;
            thumbnailImage.enabled = thumbnailImage.sprite != null;

            if (thumbnailAspectFitter != null && thumbnailImage.sprite != null)
            {
                Rect spriteRect = thumbnailImage.sprite.rect;
                thumbnailAspectFitter.aspectRatio = spriteRect.width / spriteRect.height;
            }
        }

        SetSelected(isSelected);
    }

    // 선택 여부에 맞춰 임시 선택 배경 표시함
    public void SetSelected(bool isSelected)
    {
        if (selectedImage != null)
        {
            selectedImage.enabled = isSelected;
        }
    }

    // 클릭된 배너 ID를 상위 목록에 전달함
    private void HandleClick()
    {
        if (!string.IsNullOrWhiteSpace(bannerId))
        {
            Clicked?.Invoke(bannerId);
        }
    }
}
