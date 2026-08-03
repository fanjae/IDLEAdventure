using UnityEngine;

// 장비 효과를 공유하는 영웅 클래스
// 프로젝트에서 실제 역할군 확정되면 조정 예정.
public enum HeroClassType
{
    Tank,
    Warrior,
    Mage,
    Marksman,
    Support,
    Rogue
}

// 장비 장착 부위
public enum EquipmentSlotType
{
    Weapon,
    Hands,
    Accessory,
    Head,
    Body,
    Legs
}
