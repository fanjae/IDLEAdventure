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

    [Header("Stack")] // 동일 아이템을 한 슬롯에 여러 개 보관할 수 있는지 여부
    [SerializeField] private bool isStackable;
    [SerializeField, Min(1)] private int maxStack = 1;

    public int ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public ItemGrade Grade => grade;
    public bool IsStackable => isStackable;

    // isStackable이 false라면 인스펙터 값과 관계없이 1을 반환
    public int MaxStack => isStackable ? maxStack : 1;

    // 하위 클래스가 자신의 아이템 분류를 결정
    public abstract ItemCategory Category { get; }

#if UNITY_EDITOR

    // 인스펙터 값이 변경 시 잘못된 스택 값 보정 처리용
    protected virtual void OnValidate()
    {
        if (!isStackable)
        {
            maxStack = 1;
        }
    }
#endif
}
