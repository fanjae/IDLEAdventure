using System;
using System.Collections.Generic;

// 장비 장착 실패 원인
public enum EquipmentEquipFailureReason
{
    None,
    InvalidInstanceId,
    EquipmentNotOwned,
    EquipmentDataNotFound,
    ClassMismatch,
    EquipmentSetNotFound,
    AlreadyEquipped
}

// 보유 장비 제거 실패 원인
public enum EquipmentRemoveFailureReason
{
    None,
    InvalidInstanceId,
    EquipmentNotOwned,
    EquipmentIsEquipped
}

// 클래스별 장비 장착 상태와 장착 규칙 관리
public sealed class ClassEquipmentService
{
    // 플레이어가 실제로 장비를 보유하고 있는지 확인
    private readonly Inventory inventory;

    // EquipmentId를 이용해 장비 원본 데이터 조회
    private readonly ItemDatabaseSO itemDatabase;

    // 영웅 클래스별 장비 장착 상태 저장
    private readonly Dictionary<HeroClassType, ClassEquipmentSet> equipmentSets = new();

    public ClassEquipmentService(Inventory inventory, ItemDatabaseSO itemDatabase)
    {
        this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));

        InitializeEquipmentSets();
    }

    // 지정한 영웅 클래스에 보유 장비 장착
    // 같은 슬롯에 장비가 있으면 기존 장비의 InstanceId 반환
    public bool TryEquip(HeroClassType heroClass, string instanceId, out string replacedInstanceId, out EquipmentEquipFailureReason failureReason)
    {
        replacedInstanceId = string.Empty;
        failureReason = EquipmentEquipFailureReason.None;

        if (string.IsNullOrEmpty(instanceId))
        {
            failureReason = EquipmentEquipFailureReason.InvalidInstanceId;
            return false;
        }

        // 플레이어가 실제로 보유하지 않은 장비 장착 방지
        if (!inventory.TryGetEquipment(instanceId, out OwnedEquipmentData ownedEquipment))
        {
            failureReason = EquipmentEquipFailureReason.EquipmentNotOwned;
            return false;
        }

        // 보유 장비와 연결된 원본 EquipmentSO 확인
        if (!TryGetEquipmentData(ownedEquipment, out EquipmentSO equipment))
        {
            failureReason = EquipmentEquipFailureReason.EquipmentDataNotFound;
            return false;
        }

        // 장비 대상 클래스와 장착 대상 클래스가 다른 경우 장착 방지
        if (equipment.TargetClass != heroClass)
        {
            failureReason = EquipmentEquipFailureReason.ClassMismatch;
            return false;
        }

        if (!TryGetEquipmentSet(heroClass, out ClassEquipmentSet equipmentSet))
        {
            failureReason = EquipmentEquipFailureReason.EquipmentSetNotFound;
            return false;
        }

        // 같은 장비 인스턴스가 여러 슬롯에 중복 장착되는 것을 방지
        if (IsEquipped(instanceId))
        {
            failureReason = EquipmentEquipFailureReason.AlreadyEquipped;
            return false;
        }

        EquipmentSlotType slotType = equipment.SlotType;

        // 교체되는 장비도 Inventory에서는 제거하지 않음
        replacedInstanceId = equipmentSet.GetEquippedInstanceId(slotType);
        equipmentSet.SetEquippedInstanceId(slotType, instanceId);

        return true;
    }

    // 지정한 영웅 클래스의 슬롯에서 장비 해제
    // 해제된 장비는 Inventory에 계속 남아 있음
    public bool TryUnequip(HeroClassType heroClass, EquipmentSlotType slotType, out string removedInstanceId)
    {
        removedInstanceId = string.Empty;

        if (!TryGetEquipmentSet(heroClass, out ClassEquipmentSet equipmentSet))
        {
            return false;
        }

        removedInstanceId = equipmentSet.RemoveEquippedInstance(slotType);
        return !string.IsNullOrEmpty(removedInstanceId);
    }

    // 지정한 클래스와 슬롯에 장착된 보유 장비 조회
    public bool TryGetEquippedOwnedEquipment(HeroClassType heroClass, EquipmentSlotType slotType, out OwnedEquipmentData ownedEquipment)
    {
        ownedEquipment = null;

        if (!TryGetEquippedInstanceId(heroClass, slotType, out string instanceId))
        {
            return false;
        }

        return inventory.TryGetEquipment(instanceId, out ownedEquipment);
    }

    // 지정한 클래스와 슬롯에 장착된 장비 원본 조회
    public bool TryGetEquippedEquipment(HeroClassType heroClass, EquipmentSlotType slotType, out EquipmentSO equipment)
    {
        equipment = null;

        if (!TryGetEquippedOwnedEquipment(heroClass, slotType, out OwnedEquipmentData ownedEquipment))
        {
            return false;
        }

        return TryGetEquipmentData(ownedEquipment, out equipment);
    }

    // 지정한 클래스와 슬롯에 장착된 InstanceId 조회
    public bool TryGetEquippedInstanceId(HeroClassType heroClass, EquipmentSlotType slotType, out string instanceId)
    {
        instanceId = string.Empty;

        if (!TryGetEquipmentSet(heroClass, out ClassEquipmentSet equipmentSet))
        {
            return false;
        }

        instanceId = equipmentSet.GetEquippedInstanceId(slotType);
        return !string.IsNullOrEmpty(instanceId);
    }

    // 해당 장비 인스턴스가 어떤 클래스에서든 장착 중인지 확인
    public bool IsEquipped(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            return false;
        }

        foreach (ClassEquipmentSet equipmentSet in equipmentSets.Values)
        {
            if (equipmentSet.ContainsInstance(instanceId))
            {
                return true;
            }
        }

        return false;
    }

    // 장착하지 않은 보유 장비를 Inventory에서 제거
    // 이후 장비 분해나 판매 기능에서 사용
    public bool TryRemoveOwnedEquipment(string instanceId, out EquipmentRemoveFailureReason failureReason)
    {
        failureReason = EquipmentRemoveFailureReason.None;

        if (string.IsNullOrEmpty(instanceId))
        {
            failureReason = EquipmentRemoveFailureReason.InvalidInstanceId;
            return false;
        }

        if (!inventory.ContainsEquipment(instanceId))
        {
            failureReason = EquipmentRemoveFailureReason.EquipmentNotOwned;
            return false;
        }

        // 장착 중인 장비가 분해나 판매로 제거되는 것을 방지
        if (IsEquipped(instanceId))
        {
            failureReason = EquipmentRemoveFailureReason.EquipmentIsEquipped;
            return false;
        }

        if (!inventory.TryRemoveEquipment(instanceId))
        {
            failureReason = EquipmentRemoveFailureReason.EquipmentNotOwned;
            return false;
        }

        return true;
    }

    // 모든 영웅 클래스의 장비 세트 생성
    private void InitializeEquipmentSets()
    {
        equipmentSets.Clear();

        foreach (HeroClassType heroClass in Enum.GetValues(typeof(HeroClassType)))
        {
            equipmentSets.Add(heroClass, new ClassEquipmentSet());
        }
    }

    // 지정한 영웅 클래스의 장비 세트 조회
    private bool TryGetEquipmentSet(HeroClassType heroClass, out ClassEquipmentSet equipmentSet)
    {
        return equipmentSets.TryGetValue(heroClass, out equipmentSet);
    }

    // 보유 장비와 연결된 원본 EquipmentSO 조회
    private bool TryGetEquipmentData(OwnedEquipmentData ownedEquipment, out EquipmentSO equipment)
    {
        equipment = null;

        if (ownedEquipment == null)
        {
            return false;
        }

        return itemDatabase.TryGetItem<EquipmentSO>(ownedEquipment.EquipmentId, out equipment);
    }

    // 지정한 클래스에서 자동 장착 후보로 사용할 수 있는 장비인지 확인
    private bool IsEquippableCandidate(HeroClassType heroClass, OwnedEquipmentData ownedEquipment, out EquipmentSO equipment)
    {
        equipment = null;

        if (ownedEquipment == null || IsEquipped(ownedEquipment.InstanceId))
        {
            return false;
        }

        if (!TryGetEquipmentData(ownedEquipment, out equipment))
        {
            return false;
        }

        return equipment.TargetClass == heroClass;
    }

    // 후보 장비가 현재 장착 장비보다 좋은 장비인지 확인
    private bool IsBetterEquipment(EquipmentSO candidateEquipment, EquipmentSO equippedEquipment)
    {
        if (candidateEquipment == null)
        {
            return false;
        }

        // 현재 슬롯이 비어 있으면 후보 장비를 바로 장착 가능
        if (equippedEquipment == null)
        {
            return true;
        }

        return candidateEquipment.CraftLevel > equippedEquipment.CraftLevel;
    }

    // 지정한 클래스에서 슬롯별 가장 높은 제작 레벨의 미장착 장비 조회
    private Dictionary<EquipmentSlotType, OwnedEquipmentData> FindBestEquipmentBySlot(HeroClassType heroClass)
    {
        Dictionary<EquipmentSlotType, OwnedEquipmentData> bestEquipmentBySlot = new();

        foreach (OwnedEquipmentData ownedEquipment in inventory.Equipments)
        {
            if (!IsEquippableCandidate(heroClass, ownedEquipment, out EquipmentSO candidateEquipment))
            {
                continue;
            }

            // 해당 슬롯에서 아직 후보 장비가 없으면 현재 장비를 기준으로 등록
            if (!bestEquipmentBySlot.TryGetValue(candidateEquipment.SlotType, out OwnedEquipmentData currentBest))
            {
                bestEquipmentBySlot.Add(candidateEquipment.SlotType, ownedEquipment);
                continue;
            }

            // 기존 후보의 원본 장비 데이터 조회
            if (!TryGetEquipmentData(currentBest, out EquipmentSO currentBestEquipment))
            {
                bestEquipmentBySlot[candidateEquipment.SlotType] = ownedEquipment;
                continue;
            }

            // 기존 후보보다 좋은 장비인 경우 슬롯별 최적 장비 교체
            if (IsBetterEquipment(candidateEquipment, currentBestEquipment))
            {
                bestEquipmentBySlot[candidateEquipment.SlotType] = ownedEquipment;
            }
        }

        return bestEquipmentBySlot;
    }

    // 현재 클래스별 장비 장착 상태를 저장 데이터로 생성
    public EquipmentSaveData CreateSaveData()
    {
        EquipmentSaveData saveData = new();

        foreach (KeyValuePair<HeroClassType, ClassEquipmentSet> pair in equipmentSets)
        {
            ClassEquipmentSaveData classSaveData = new()
            {
                HeroClass = pair.Key
            };

            // 장착 중인 슬롯의 InstanceId 저장
            foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                string instanceId = pair.Value.GetEquippedInstanceId(slotType);

                if (!string.IsNullOrEmpty(instanceId))
                {
                    classSaveData.EquippedInstanceIds.Add(slotType, instanceId);
                }
            }

            saveData.Classes.Add(classSaveData);
        }

        return saveData;
    }

    // 저장 데이터를 기준으로 클래스별 장비 장착 상태 복원
    public void LoadSaveData(EquipmentSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        // 기존 장착 상태를 모두 초기화
        InitializeEquipmentSets();

        if (saveData.Classes == null)
        {
            return;
        }

        foreach (ClassEquipmentSaveData classData in saveData.Classes)
        {
            if (classData == null || classData.EquippedInstanceIds == null)
            {
                continue;
            }

            foreach (KeyValuePair<EquipmentSlotType, string> pair in classData.EquippedInstanceIds)
            {
                TryLoadEquipment(classData.HeroClass, pair.Key, pair.Value);
            }
        }
    }

    // 저장된 장비 장착 상태를 복원할 수 있으면 해당 슬롯에 적용
    private bool TryLoadEquipment(HeroClassType heroClass, EquipmentSlotType slotType, string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            return false;
        }

        // 저장된 장비가 현재 인벤토리에 존재하는지 확인
        if (!inventory.TryGetEquipment(instanceId, out OwnedEquipmentData ownedEquipment))
        {
            return false;
        }

        // 보유 장비와 연결된 원본 EquipmentSO 확인
        if (!TryGetEquipmentData(ownedEquipment, out EquipmentSO equipment))
        {
            return false;
        }

        // 저장된 클래스와 실제 장비 대상 클래스가 다른 경우 복원하지 않음
        if (equipment.TargetClass != heroClass)
        {
            return false;
        }

        // 저장된 슬롯과 실제 장비 슬롯이 다른 경우 복원하지 않음
        if (equipment.SlotType != slotType)
        {
            return false;
        }

        if (!TryGetEquipmentSet(heroClass, out ClassEquipmentSet equipmentSet))
        {
            return false;
        }

        // 같은 장비 인스턴스가 여러 슬롯에 중복 복원되는 것을 방지
        if (IsEquipped(instanceId))
        {
            return false;
        }

        equipmentSet.SetEquippedInstanceId(slotType, instanceId);
        return true;
    }

    // 지정한 클래스에 현재 장착 장비보다 좋은 미장착 장비가 있는지 확인
    public bool HasBetterEquippableEquipment(HeroClassType heroClass)
    {
        foreach (OwnedEquipmentData ownedEquipment in inventory.Equipments)
        {
            if (!IsEquippableCandidate(heroClass, ownedEquipment, out EquipmentSO candidateEquipment))
            {
                continue;
            }

            TryGetEquippedEquipment(heroClass, candidateEquipment.SlotType, out EquipmentSO equippedEquipment);

            if (IsBetterEquipment(candidateEquipment, equippedEquipment))
            {
                return true;
            }
        }

        return false;
    }

    // 지정한 클래스에 장착 가능한 미장착 장비가 있는지 확인
    public bool HasEquippableEquipment(HeroClassType heroClass)
    {
        foreach (OwnedEquipmentData ownedEquipment in inventory.Equipments)
        {
            if (IsEquippableCandidate(heroClass, ownedEquipment, out _))
            {
                return true;
            }
        }

        return false;
    }

    // 지정한 클래스에 부위별 기준 제작 레벨이 가장 높은 장비 일괄 장착
    public bool TryAutoEquipBetterEquipment(HeroClassType heroClass)
    {
        Dictionary<EquipmentSlotType, OwnedEquipmentData> bestEquipmentBySlot = FindBestEquipmentBySlot(heroClass);
        bool equippedAny = false;

        foreach (KeyValuePair<EquipmentSlotType, OwnedEquipmentData> pair in bestEquipmentBySlot)
        {
            if (!TryGetEquipmentData(pair.Value, out EquipmentSO candidateEquipment))
            {
                continue;
            }

            TryGetEquippedEquipment(heroClass, pair.Key, out EquipmentSO equippedEquipment);

            // 현재 장착 장비보다 좋은 경우에만 교체
            if (!IsBetterEquipment(candidateEquipment, equippedEquipment))
            {
                continue;
            }

            if (TryEquip(heroClass, pair.Value.InstanceId, out _, out _))
            {
                equippedAny = true;
            }
        }

        return equippedAny;
    }
}