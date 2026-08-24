using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 인벤토리 아이템 슬롯 하나의 UI 표시
public sealed class InventoryItemSlotView : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image classIcon;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform slotTransform;

    private Tween showTween;
    private Vector2 defaultPosition;

    private void Awake()
    {
        defaultPosition = slotTransform.anchoredPosition;
    }

    private void OnDisable()
    {
        showTween?.Kill();
    }

    // 일반 아이템 데이터 표시
    public void BindItem(ItemSO item, int quantity)
    {
        if (item == null)
        {
            return;
        }

        itemIcon.sprite = item.Icon;
        itemIcon.preserveAspect = true;

        classIcon.gameObject.SetActive(false);
        levelText.gameObject.SetActive(false);

        countText.text = quantity.ToString();
    }

    // 보유 장비 데이터 표시
    public void BindEquipment(EquipmentSO equipment, OwnedEquipmentData ownedEquipment, HeroClassIconCatalog classIconCatalog)
    {
        if (equipment == null || ownedEquipment == null)
        {
            return;
        }

        itemIcon.sprite = equipment.Icon;
        itemIcon.preserveAspect = true;

        Sprite targetClassIcon = classIconCatalog != null ? classIconCatalog.GetIcon(equipment.TargetClass) : null;
        classIcon.sprite = targetClassIcon;
        classIcon.gameObject.SetActive(targetClassIcon != null);

        levelText.gameObject.SetActive(true);
        levelText.text = $"{ownedEquipment.EnhancementLevel}레벨";

        // 현재 장비 구조는 장비 한 개마다 별도의 InstanceId를 가지므로 슬롯 하나의 수량은 1
        countText.text = "1";
    }

    // 슬롯을 숨김 상태로 준비
    public void PrepareHiddenState()
    {
        showTween?.Kill();
        canvasGroup.alpha = 0f;
        slotTransform.anchoredPosition = defaultPosition + new Vector2(0f, -35f);
    }

    // 슬롯 등장 애니메이션 생성
    public Tween CreateShowTween()
    {
        showTween?.Kill();

        Sequence sequence = DOTween.Sequence();
        sequence.Join(canvasGroup.DOFade(1f, 0.12f));
        sequence.Join(slotTransform.DOAnchorPos(defaultPosition, 0.12f).SetEase(Ease.OutCubic));

        showTween = sequence;
        return showTween;
    }
}