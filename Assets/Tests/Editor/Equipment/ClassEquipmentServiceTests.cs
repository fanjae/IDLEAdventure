using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ClassEquipmentServiceTests
{
    // 테스트 중 생성한 ScriptableObject를 정리하기 위한 목록
    private readonly List<UnityEngine.Object> createdObjects = new();

    // 각 테스트가 끝난 뒤 생성된 Unity 객체 정리
    [TearDown]
    public void TearDown()
    {
        foreach (UnityEngine.Object createdObject in createdObjects)
        {
            UnityEngine.Object.DestroyImmediate(createdObject);
        }

        createdObjects.Clear();
    }

    // 유효한 클래스와 슬롯의 장비가 정상적으로 장착되는지 확인
    [Test]
    public void TryEquip_WithValidEquipment_EquipsSuccessfully()
    {
        const int itemId = 1;

        EquipmentSO equipment = CreateEquipment(itemId, HeroClassType.Warrior, EquipmentSlotType.Weapon);
        ItemDatabaseSO itemDatabase = CreateItemDatabase(equipment);
        ClassEquipmentService service = new(itemDatabase);

        bool equipResult = service.TryEquip(HeroClassType.Warrior, itemId, out int replacedItemId, out EquipmentEquipFailureReason failureReason);

        Assert.That(equipResult, Is.True);
        Assert.That(replacedItemId, Is.Zero);
        Assert.That(failureReason, Is.EqualTo(EquipmentEquipFailureReason.None));

        bool getResult = service.TryGetEquippedItemId(HeroClassType.Warrior, EquipmentSlotType.Weapon, out int equippedItemId);

        Assert.That(getResult, Is.True);
        Assert.That(equippedItemId, Is.EqualTo(itemId));
    }

    // 장비 대상 클래스와 장착 대상 클래스가 다를 때 장착이 거부되는지 확인
    [Test]
    public void TryEquip_WithMismatchedClass_ReturnsClassMismatch()
    {
        EquipmentSO equipment = CreateEquipment(1, HeroClassType.Mage, EquipmentSlotType.Weapon);
        ItemDatabaseSO itemDatabase = CreateItemDatabase(equipment);
        ClassEquipmentService service = new(itemDatabase);

        bool result = service.TryEquip(HeroClassType.Warrior, equipment.ItemId, out int replacedItemId, out EquipmentEquipFailureReason failureReason);

        Assert.That(result, Is.False);
        Assert.That(replacedItemId, Is.Zero);
        Assert.That(failureReason, Is.EqualTo(EquipmentEquipFailureReason.ClassMismatch));
    }

    // 같은 슬롯에 새로운 장비를 장착했을 때 기존 장비가 교체되는지 확인
    [Test]
    public void TryEquip_WithEquipmentInSameSlot_ReplacesExistingEquipment()
    {
        EquipmentSO firstEquipment = CreateEquipment(1, HeroClassType.Warrior, EquipmentSlotType.Weapon);
        EquipmentSO secondEquipment = CreateEquipment(2, HeroClassType.Warrior, EquipmentSlotType.Weapon);
        ItemDatabaseSO itemDatabase = CreateItemDatabase(firstEquipment, secondEquipment);
        ClassEquipmentService service = new(itemDatabase);

        service.TryEquip(HeroClassType.Warrior, firstEquipment.ItemId, out _, out _);

        bool result = service.TryEquip(HeroClassType.Warrior, secondEquipment.ItemId, out int replacedItemId, out EquipmentEquipFailureReason failureReason);

        Assert.That(result, Is.True);
        Assert.That(replacedItemId, Is.EqualTo(firstEquipment.ItemId));
        Assert.That(failureReason, Is.EqualTo(EquipmentEquipFailureReason.None));

        bool getResult = service.TryGetEquippedItemId(HeroClassType.Warrior, EquipmentSlotType.Weapon, out int equippedItemId);

        Assert.That(getResult, Is.True);
        Assert.That(equippedItemId, Is.EqualTo(secondEquipment.ItemId));
    }

    // 장착된 장비를 해제했을 때 슬롯이 비워지고 기존 장비 ID가 반환되는지 확인
    [Test]
    public void TryUnequip_WithEquippedItem_RemovesEquipment()
    {
        EquipmentSO equipment = CreateEquipment(1, HeroClassType.Warrior, EquipmentSlotType.Weapon);
        ItemDatabaseSO itemDatabase = CreateItemDatabase(equipment);
        ClassEquipmentService service = new(itemDatabase);

        service.TryEquip(HeroClassType.Warrior, equipment.ItemId, out _, out _);

        bool result = service.TryUnequip(HeroClassType.Warrior, EquipmentSlotType.Weapon, out int removedItemId);

        Assert.That(result, Is.True);
        Assert.That(removedItemId, Is.EqualTo(equipment.ItemId));

        bool getResult = service.TryGetEquippedItemId(HeroClassType.Warrior, EquipmentSlotType.Weapon, out int equippedItemId);

        Assert.That(getResult, Is.False);
        Assert.That(equippedItemId, Is.Zero);
    }

    // 테스트용 EquipmentSO 생성
    private EquipmentSO CreateEquipment(int itemId, HeroClassType targetClass, EquipmentSlotType slotType)
    {
        EquipmentSO equipment = ScriptableObject.CreateInstance<EquipmentSO>();
        createdObjects.Add(equipment);

        SetPrivateField(equipment, "itemId", itemId);
        SetPrivateField(equipment, "targetClass", targetClass);
        SetPrivateField(equipment, "slotType", slotType);

        return equipment;
    }

    // 테스트용 ItemDatabaseSO 생성
    private ItemDatabaseSO CreateItemDatabase(params ItemSO[] items)
    {
        ItemDatabaseSO itemDatabase = ScriptableObject.CreateInstance<ItemDatabaseSO>();
        createdObjects.Add(itemDatabase);

        SetPrivateField(itemDatabase, "items", new List<ItemSO>(items));
        SetPrivateField(itemDatabase, "itemMap", null);

        return itemDatabase;
    }

    // items 변경 후 다음 조회 시 ItemDatabaseSO가 Dictionary를 다시 생성하도록 캐시 초기화
    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = FindField(target.GetType(), fieldName);

        if (field == null)
        {
            throw new MissingFieldException(target.GetType().Name, fieldName);
        }

        field.SetValue(target, value);
    }

    // ItemSO에 선언된 itemId처럼 부모 클래스의 private 필드도 설정할 수 있도록 상속 계층 탐색
    private static FieldInfo FindField(Type type, string fieldName)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }
}