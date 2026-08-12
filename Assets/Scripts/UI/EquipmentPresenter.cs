using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentPresenter : MonoBehaviour
{
    [SerializeField] private ItemDatabaseSO itemDatabase;

    [SerializeField] private Button tankClassButton;
    [SerializeField] private Button warriorClassButton;
    [SerializeField] private Button mageClassButton;
    [SerializeField] private Button marksmanClassButton;
    [SerializeField] private Button supportClassButton;
    [SerializeField] private Button rogueClassButton;
    [SerializeField] private TMP_Text selectedClassLabel;
    [SerializeField] private Image selectedClassIcon;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;

    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image handsIcon;
    [SerializeField] private Image accessoryIcon;
    [SerializeField] private Image headIcon;
    [SerializeField] private Image bodyIcon;
    [SerializeField] private Image legsIcon;
    [SerializeField] private Sprite emptySlotIcon;
    [SerializeField] private Sprite missingEquipmentIcon;

    [SerializeField] private TMP_Text effectTitleText;
    [SerializeField] private TMP_Text attackValueText;
    [SerializeField] private TMP_Text defenseValueText;
    [SerializeField] private TMP_Text healthValueText;

    [SerializeField] private RectTransform inventoryContent;
    [SerializeField] private EquipmentInventoryItemView inventoryItemTemplate;
    [SerializeField] private Button equipButton;

    private readonly List<EquipmentInventoryItemView> inventoryItemViews = new();

    private InventoryController inventoryController;
    private EquipmentStatCalculator statCalculator;
    private HeroClassType currentClass = HeroClassType.Tank;
    private string selectedEquipmentInstanceId;

    private void Awake()
    {
        tankClassButton?.onClick.AddListener(() => SelectClass(HeroClassType.Tank));
        warriorClassButton?.onClick.AddListener(() => SelectClass(HeroClassType.Warrior));
        mageClassButton?.onClick.AddListener(() => SelectClass(HeroClassType.Mage));
        marksmanClassButton?.onClick.AddListener(() => SelectClass(HeroClassType.Marksman));
        supportClassButton?.onClick.AddListener(() => SelectClass(HeroClassType.Support));
        rogueClassButton?.onClick.AddListener(() => SelectClass(HeroClassType.Rogue));
        equipButton?.onClick.AddListener(EquipSelectedEquipment);

        if (inventoryItemTemplate != null)
        {
            inventoryItemTemplate.gameObject.SetActive(false);
        }

        if (inventoryContent != null)
        {
            foreach (Transform child in inventoryContent)
            {
                if (inventoryItemTemplate == null || child != inventoryItemTemplate.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnEnable()
    {
        SubscribeToInventory();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromInventory();
    }

    private void SubscribeToInventory()
    {
        if (inventoryController != null || InventoryManager.Instance == null || !InventoryManager.Instance.IsInitialized)
        {
            return;
        }

        inventoryController = InventoryManager.Instance.Controller;
        statCalculator = new EquipmentStatCalculator(inventoryController);
        inventoryController.OnInventoryChanged += Refresh;
        inventoryController.OnEquipmentChanged += Refresh;
    }

    private void UnsubscribeFromInventory()
    {
        if (inventoryController == null)
        {
            return;
        }

        inventoryController.OnInventoryChanged -= Refresh;
        inventoryController.OnEquipmentChanged -= Refresh;
        inventoryController = null;
        statCalculator = null;
    }

    public void SelectClass(HeroClassType heroClass)
    {
        currentClass = heroClass;
        selectedEquipmentInstanceId = null;
        Refresh();
    }

    public void Refresh()
    {
        SubscribeToInventory();
        UpdateSelectedClass();
        UpdateEquipmentSlots();
        UpdateEquipmentStats();
        UpdateInventory();
        UpdateEquipButton();
    }

    private void UpdateSelectedClass()
    {
        if (selectedClassLabel != null)
        {
            selectedClassLabel.text = $"{GetClassName(currentClass)} 장비";
        }

        if (selectedClassIcon != null)
        {
            selectedClassIcon.sprite = classIconCatalog != null
                ? classIconCatalog.GetIcon(currentClass)
                : null;
            selectedClassIcon.preserveAspect = selectedClassIcon.sprite != null;
        }
    }

    private void UpdateEquipmentSlots()
    {
        UpdateSlot(EquipmentSlotType.Weapon, weaponIcon);
        UpdateSlot(EquipmentSlotType.Hands, handsIcon);
        UpdateSlot(EquipmentSlotType.Accessory, accessoryIcon);
        UpdateSlot(EquipmentSlotType.Head, headIcon);
        UpdateSlot(EquipmentSlotType.Body, bodyIcon);
        UpdateSlot(EquipmentSlotType.Legs, legsIcon);
    }

    private void UpdateSlot(EquipmentSlotType slotType, Image iconImage)
    {
        if (iconImage == null)
        {
            return;
        }

        Sprite icon = emptySlotIcon;

        if (inventoryController != null
            && inventoryController.TryGetEquippedEquipment(currentClass, slotType, out EquipmentSO equipment))
        {
            icon = equipment.Icon != null ? equipment.Icon : missingEquipmentIcon;
        }

        iconImage.sprite = icon;
        iconImage.preserveAspect = icon != null;
    }

    private void UpdateEquipmentStats()
    {
        EquipmentStat totalStat = statCalculator != null
            ? statCalculator.Calculate(currentClass)
            : EquipmentStat.Zero;

        if (effectTitleText != null)
        {
            effectTitleText.text = $"{GetClassName(currentClass)} 장비 효과";
        }

        if (attackValueText != null)
        {
            attackValueText.text = $"{totalStat.Attack}";
        }

        if (defenseValueText != null)
        {
            defenseValueText.text = $"{totalStat.Defense}";
        }

        if (healthValueText != null)
        {
            healthValueText.text = $"{totalStat.Health}";
        }
    }

    private void UpdateInventory()
    {
        if (inventoryContent == null || inventoryItemTemplate == null)
        {
            return;
        }

        List<OwnedEquipmentData> equipments = new();

        if (inventoryController != null && itemDatabase != null)
        {
            foreach (OwnedEquipmentData equipment in inventoryController.Equipments)
            {
                if (!itemDatabase.TryGetItem<EquipmentSO>(equipment.EquipmentId, out EquipmentSO equipmentData)
                    || equipmentData.TargetClass != currentClass)
                {
                    continue;
                }

                equipments.Add(equipment);
            }

            equipments.Sort(CompareEquipment);
        }

        bool isSelectedEquipmentVisible = false;

        for (int index = 0; index < equipments.Count; index++)
        {
            EquipmentInventoryItemView itemView = GetInventoryItemView(index);
            EquipmentSO equipmentData = null;

            if (inventoryController != null
                && itemDatabase != null
                && inventoryController.TryGetEquipment(equipments[index].InstanceId, out OwnedEquipmentData ownedEquipment))
            {
                itemDatabase.TryGetItem<EquipmentSO>(ownedEquipment.EquipmentId, out equipmentData);
            }

            string instanceId = equipments[index].InstanceId;
            bool isSelected = instanceId == selectedEquipmentInstanceId;

            if (isSelected)
            {
                isSelectedEquipmentVisible = true;
            }

            itemView.Bind(
                equipmentData,
                instanceId,
                missingEquipmentIcon,
                isSelected,
                SelectEquipment);
            itemView.gameObject.SetActive(true);
        }

        if (!isSelectedEquipmentVisible)
        {
            selectedEquipmentInstanceId = null;
        }

        for (int index = equipments.Count; index < inventoryItemViews.Count; index++)
        {
            inventoryItemViews[index].gameObject.SetActive(false);
        }
    }

    private void SelectEquipment(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            return;
        }

        selectedEquipmentInstanceId = instanceId;
        Refresh();
    }

    private void EquipSelectedEquipment()
    {
        if (inventoryController == null || string.IsNullOrEmpty(selectedEquipmentInstanceId))
        {
            return;
        }

        if (!inventoryController.TryEquip(
                currentClass,
                selectedEquipmentInstanceId,
                out _,
                out _))
        {
            return;
        }

        selectedEquipmentInstanceId = null;

        if (SaveManager.Instance.CurrentData != null)
        {
            SaveManager.Instance.Save();
        }

        Refresh();
    }

    private void UpdateEquipButton()
    {
        if (equipButton == null)
        {
            return;
        }

        bool canEquip = !string.IsNullOrEmpty(selectedEquipmentInstanceId)
            && inventoryController != null
            && inventoryController.ContainsEquipment(selectedEquipmentInstanceId)
            && !inventoryController.IsEquipped(selectedEquipmentInstanceId);

        equipButton.interactable = canEquip;
    }

    private EquipmentInventoryItemView GetInventoryItemView(int index)
    {
        while (inventoryItemViews.Count <= index)
        {
            EquipmentInventoryItemView itemView = Instantiate(inventoryItemTemplate, inventoryContent);
            itemView.gameObject.SetActive(false);
            inventoryItemViews.Add(itemView);
        }

        return inventoryItemViews[index];
    }

    private static int CompareEquipment(OwnedEquipmentData left, OwnedEquipmentData right)
    {
        int result = left.EquipmentId.CompareTo(right.EquipmentId);
        return result != 0 ? result : string.CompareOrdinal(left.InstanceId, right.InstanceId);
    }

    private static string GetClassName(HeroClassType heroClass)
    {
        return heroClass switch
        {
            HeroClassType.Tank => "탱커",
            HeroClassType.Warrior => "전사",
            HeroClassType.Mage => "마법사",
            HeroClassType.Marksman => "궁수",
            HeroClassType.Support => "지원가",
            HeroClassType.Rogue => "도적",
            _ => heroClass.ToString()
        };
    }
}
