using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game Data/Item/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    // 전체 아이템 원본 데이터
    [SerializeField] private List<ItemSO> items = new();

    // ItemId 기반 런타임 조회용 Dictionary
    private Dictionary<int, ItemSO> itemMap;

    // 외부에서 제공 아이템 목록
    public IReadOnlyList<ItemSO> Items => items;

    private void OnEnable()
    {
        Initialize();
    }

    // Items 목록을 ItemId 기준 Dictionary로 변환
    private void Initialize()
    {
        itemMap = new Dictionary<int, ItemSO>();

        foreach (ItemSO item in items)
        {
            
            if (item == null)
            {
                continue;
            }

            // 중복 데이터가 기존 데이터 덮어쓰지 않도록 방지
            if (!itemMap.TryAdd(item.ItemId, item))
            {
                Debug.LogError($"[ItemDatabaseSO] 중복된 ItemId가 있습니다. " + $"ItemId: {item.ItemId}, Item: {item.name}",this);
            }
        }
    }

    // ItemId에 해당하는 ItemSO 반환
    public ItemSO GetItem(int itemId)
    {
        EnsureInitialized();

        itemMap.TryGetValue(itemId, out ItemSO item);
        return item;
    }

    // ItemId에 해당하는 ItemSO 조회
    public bool TryGetItem(int itemId, out ItemSO item)
    {
        EnsureInitialized();

        return itemMap.TryGetValue(itemId, out item);
    }

    // ItemId에 해당하는 EquipmentSO 반환
    public EquipmentSO GetEquipment(int itemId)
    {
        return GetItem(itemId) as EquipmentSO;
    }

    // ItemId에 해당하는 EquipmentSO 조회
    public bool TryGetEquipment(int itemId, out EquipmentSO equipment)
    {
        equipment = GetItem(itemId) as EquipmentSO;
        return equipment != null;
    }

    // itemMap이 아직 생성되지 않은 경우에만 Initialize 호출.
    private void EnsureInitialized()
    {
        if (itemMap == null)
        {
            Initialize();
        }
    }
#if UNITY_EDITOR

    private void OnValidate()
    {
        ValidateItems();
    }

    // 에디터 단계에서 비어있는 항목 및 중복 ItemID 검사
    private void ValidateItems()
    {
        // 이미 확인한 ItemID 저장하여 중복 여부 검사
        HashSet<int> itemIds = new();

        foreach (ItemSO item in items)
        {
            if (item == null)
            {
                Debug.LogWarning("[ItemDatabaseSO] 비어 있는 아이템 항목이 있습니다.", this);
                continue;
            }

            if (item.ItemId <= 0)
            {
                Debug.LogError($"[ItemDatabaseSO] ItemId는 1 이상이어야 합니다. " + $"Item: {item.name}, ItemId: {item.ItemId}",item);
                continue;
            }

            if (!itemIds.Add(item.ItemId))
            {
                Debug.LogError($"[ItemDatabaseSO] 중복된 ItemId가 있습니다. " + $"ItemId: {item.ItemId}, Item: {item.name}",item);
            }
        }

        itemMap = null;
    }
#endif
}
