using System;
using UnityEngine;

// 플레이어가 보유한 일반 아이템의 수량 데이터
// 장비는 개별 상태가 존재하기 때문에 별도의 OwnedEquipmentData로 관리
[Serializable]
public sealed class InventoryItemData
{
    [SerializeField] private int itemId;
    [SerializeField] private int quantity;

    public int ItemId => itemId;
    public int Quantity => quantity;

    public InventoryItemData(int itemId, int quantity)
    {
        this.itemId = itemId;
        this.quantity = quantity;
    }
}

// 플레이어가 보유한 장비 한 개의 데이터
// 같은 종류의 장비라도 서로 다른 InstanceId와 강화 단계를 가질 수 있음
[Serializable]
public sealed class OwnedEquipmentData
{
    // 보유 장비 한 개를 식별하는 고유 ID
    [SerializeField] private string instanceId;

    // 원본 EquipmentSO를 식별하기 위한 ID
    [SerializeField] private int equipmentId;

    // 플레이어가 보유한 장비 한 개의 데이터
    // 같은 종류의 장비라도 각각 별도의 InstanceId를 가짐
    // enhancementLevel은 기존 저장 데이터 호환을 위해 유지 (기획상 삭제)
    [SerializeField] private int enhancementLevel;

    public string InstanceId => instanceId;
    public int EquipmentId => equipmentId;
    public int EnhancementLevel => enhancementLevel;

    public OwnedEquipmentData(string instanceId, int equipmentId, int enhancementLevel = 0)
    {
        this.instanceId = instanceId;
        this.equipmentId = equipmentId;
        this.enhancementLevel = enhancementLevel;
    }
}