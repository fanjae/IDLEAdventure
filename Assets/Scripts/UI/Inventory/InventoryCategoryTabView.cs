using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 인벤토리 카테고리 탭 표시
public sealed class InventoryCategoryTabView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.08f;

    private Tween hoverTween;

    public Button Button => button;

    private void OnDisable()
    {
        hoverTween?.Kill();
        iconImage.rectTransform.localScale = Vector3.one;
    }

    // 카테고리 선택 상태 표시
    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            iconImage.sprite = selectedSprite;
            return;
        }

        if (normalSprite != null)
        {
            iconImage.sprite = normalSprite;
        }
    }

    // 마우스가 탭 위에 올라왔을 때 아이콘 확대
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayScaleAnimation(hoverScale);
    }

    // 마우스가 탭에서 벗어났을 때 아이콘 크기 복원
    public void OnPointerExit(PointerEventData eventData)
    {
        PlayScaleAnimation(1f);
    }

    // 아이콘 크기 변경 애니메이션 재생
    private void PlayScaleAnimation(float targetScale)
    {
        hoverTween?.Kill();
        hoverTween = iconImage.rectTransform.DOScale(targetScale, hoverDuration).SetEase(Ease.OutQuad);
    }
}