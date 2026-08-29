using UnityEngine;

// 아이템 분류
public enum ItemCategory
{
    Equipment,
    Consumable,
    Material
}

// 아이템 희귀도
public enum ItemGrade
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public abstract class ItemSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField, Min(1)] private int itemId;
    [SerializeField] private string itemName;

    [Header("Display")]
    [SerializeField, TextArea(2,5)] private string description;
    [SerializeField] private Sprite icon;

    [Header("Classification")]
    [SerializeField] private ItemGrade grade;
    public int ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public ItemGrade Grade => grade;

    // 하위 클래스가 자신의 아이템 분류를 결정
    public abstract ItemCategory Category { get; }

}
