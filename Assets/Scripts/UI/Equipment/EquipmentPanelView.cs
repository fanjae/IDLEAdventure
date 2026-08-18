using System;
using TMPro;
using UnityEngine;

// 장비 패널의 UI 상태 갱신 처리
public class EquipmentPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text equipmentText;
    [SerializeField] private EquipmentAutoEquipButtonView autoEquipButtonView;
    [SerializeField] private EquipmentSlotView weaponSlotView;
    [SerializeField] private EquipmentSlotView handsSlotView;
    [SerializeField] private EquipmentSlotView accessorySlotView;
    [SerializeField] private EquipmentSlotView headSlotView;
    [SerializeField] private EquipmentSlotView bodySlotView;
    [SerializeField] private EquipmentSlotView legsSlotView;

    public event Action OnAutoEquipClicked;

    private void Awake()
    {
        if (autoEquipButtonView != null)
        {
            autoEquipButtonView.OnClicked += HandleAutoEquipClicked;
        }
    }

    private void OnDestroy()
    {
        if (autoEquipButtonView != null)
        {
            autoEquipButtonView.OnClicked -= HandleAutoEquipClicked;
        }
    }

    // 일괄 장착 버튼 상태 갱신
    public void SetAutoEquipAvailable(bool available)
    {
        if (autoEquipButtonView == null)
        {
            return;
        }

        autoEquipButtonView.SetAvailable(available);
    }

    // 일괄 장착 버튼 클릭 이벤트 전달
    private void HandleAutoEquipClicked()
    {
        OnAutoEquipClicked?.Invoke();
    }

    // 지정한 장비 슬롯의 장착 장비 표시 갱신
    public void SetEquipmentSlot(EquipmentSlotType slotType, EquipmentSO equipment)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Weapon:
                weaponSlotView?.SetEquipment(equipment);
                break;

            case EquipmentSlotType.Hands:
                handsSlotView?.SetEquipment(equipment);
                break;

            case EquipmentSlotType.Accessory:
                accessorySlotView?.SetEquipment(equipment);
                break;

            case EquipmentSlotType.Head:
                headSlotView?.SetEquipment(equipment);
                break;

            case EquipmentSlotType.Body:
                bodySlotView?.SetEquipment(equipment);
                break;

            case EquipmentSlotType.Legs:
                legsSlotView?.SetEquipment(equipment);
                break;
        }
    }

    // 표시할 클래스에 맞게 장비 제목 갱신
    public void SetClassTitle(HeroClassType heroClass)
    {
        if (equipmentText == null)
        {
            return;
        }

        equipmentText.text = heroClass switch
        {
            HeroClassType.Warrior => "<color=#00A30C>전</color>사 장비",
            HeroClassType.Marksman => "<color=#00A30C>궁</color>수 장비",
            HeroClassType.Tank => "<color=#00A30C>탱</color>커 장비",
            HeroClassType.Mage => "<color=#00A30C>마</color>법사 장비",
            HeroClassType.Support => "<color=#00A30C>서</color>포터 장비",
            HeroClassType.Rogue => "<color=#00A30C>도</color>적 장비",
            _ => "장비"
        };
    }
}