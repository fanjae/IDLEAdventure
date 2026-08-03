using UnityEngine;

// 클래스 공용 장비 데이터
[CreateAssetMenu(fileName = "NewEquipment",menuName = "Game Data/Item/Equipment")]
public class EquipmentSO : ItemSO
{
    [Header("Equipment Classification")]

    // 이 장비의 효과를 공유하는 영웅 클래스
    [SerializeField] private HeroClassType targetClass;

    // 장비가 적용되는 부위
    [SerializeField] private EquipmentSlotType slotType;

    [Header("Equipment Requirement")]

    // 장비를 사용하거나 획득하는 기준 레벨
    [SerializeField, Min(1)] private int requiredLevel = 1;

    [Header("Base Stats")]

    [SerializeField, Min(0)] private int attack;
    [SerializeField, Min(0)] private int defense;
    [SerializeField, Min(0)] private int health;

    public override ItemCategory Category => ItemCategory.Equipment;

    public HeroClassType TargetClass => targetClass;
    public EquipmentSlotType SlotType => slotType;
    public int RequiredLevel => requiredLevel;

    public int Attack => attack;
    public int Defense => defense;
    public int Health => health;
}
