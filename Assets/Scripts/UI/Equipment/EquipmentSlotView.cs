using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 장착 슬롯의 장비 이미지와 클래스 아이콘, 레벨 표시 처리
public class EquipmentSlotView : MonoBehaviour
{
    [SerializeField] private Image equipmentImage;
    [SerializeField] private Image classIconImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;

    // 장착 장비 표시 갱신
    public void SetEquipment(EquipmentSO equipment)
    {
        bool hasEquipment = equipment != null;

        if (equipmentImage != null)
        {
            equipmentImage.sprite = hasEquipment ? equipment.Icon : null;
            equipmentImage.enabled = hasEquipment;
        }

        if (classIconImage != null)
        {
            classIconImage.sprite = hasEquipment && classIconCatalog != null ? classIconCatalog.GetIcon(equipment.TargetClass) : null;
            classIconImage.enabled = hasEquipment;
        }

        if (levelText != null)
        {
            levelText.text = hasEquipment ? $"{equipment.CraftLevel}레벨" : string.Empty;
            levelText.gameObject.SetActive(hasEquipment);
        }
    }
}