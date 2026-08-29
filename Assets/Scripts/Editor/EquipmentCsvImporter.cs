using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EquipmentCsvImporter
{
    private const string CsvPath = "Assets/Data/ItemData/Equipment/Equipment.csv";
    private const string OutputPath = "Assets/Data/ItemData/Equipment";
    private const string ItemDatabasePath = "Assets/Resources/GameData/ItemDatabase.asset";

    private sealed class EquipmentCsvRow
    {
        public int ItemId;
        public string ItemName;
        public HeroClassType TargetClass;
        public EquipmentSlotType SlotType;
        public string IconName;

        [CsvOptional] public string CraftLevel;
        [CsvOptional] public string Grade;
        [CsvOptional] public string Attack;
        [CsvOptional] public string Defense;
        [CsvOptional] public string Health;
        [CsvOptional] public string Description;
    }

    // 장비 CSV를 기준으로 EquipmentSO 생성 및 갱신
    [MenuItem("Tools/Data/Import Equipment CSV")]
    public static void Import()
    {
        TextAsset csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CsvPath);

        if (csvAsset == null)
        {
            Debug.LogError($"[EquipmentCsvImporter] CSV 파일을 찾을 수 없습니다. Path: {CsvPath}");
            return;
        }

        try
        {
            List<EquipmentCsvRow> rows = CsvMapper.Read<EquipmentCsvRow>(csvAsset);
            Dictionary<int, EquipmentSO> existingEquipments = LoadExistingEquipments();
            List<EquipmentSO> importedEquipments = new(rows.Count);

            foreach (EquipmentCsvRow row in rows)
            {
                ValidateRow(row);

                EquipmentSO equipment = GetOrCreateEquipment(row, existingEquipments);
                ApplyRow(equipment, row);
                importedEquipments.Add(equipment);
            }

            RegisterToItemDatabase(importedEquipments);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EquipmentCsvImporter] 장비 CSV Import 완료 - {importedEquipments.Count}개");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[EquipmentCsvImporter] 장비 CSV Import 실패 - {exception.Message}");
        }
    }

    // 기존 EquipmentSO를 ItemId 기준으로 조회
    private static Dictionary<int, EquipmentSO> LoadExistingEquipments()
    {
        Dictionary<int, EquipmentSO> equipments = new();
        string[] guids = AssetDatabase.FindAssets("t:EquipmentSO", new[] { OutputPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            EquipmentSO equipment = AssetDatabase.LoadAssetAtPath<EquipmentSO>(assetPath);

            if (equipment == null)
            {
                continue;
            }

            if (!equipments.TryAdd(equipment.ItemId, equipment))
            {
                throw new InvalidOperationException($"중복된 EquipmentSO ItemId가 있습니다. ItemId: {equipment.ItemId}");
            }
        }

        return equipments;
    }

    // 기존 장비가 있으면 재사용하고 없으면 새 Asset 생성
    private static EquipmentSO GetOrCreateEquipment(EquipmentCsvRow row, Dictionary<int, EquipmentSO> existingEquipments)
    {
        if (existingEquipments.TryGetValue(row.ItemId, out EquipmentSO equipment))
        {
            return equipment;
        }

        equipment = ScriptableObject.CreateInstance<EquipmentSO>();
        string assetName = $"Equipment_{row.ItemId}_{row.TargetClass}_{row.SlotType}.asset";
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{OutputPath}/{assetName}");
        AssetDatabase.CreateAsset(equipment, assetPath);
        existingEquipments.Add(row.ItemId, equipment);

        return equipment;
    }

    // CSV 한 행의 데이터를 EquipmentSO에 반영
    private static void ApplyRow(EquipmentSO equipment, EquipmentCsvRow row)
    {
        SerializedObject serializedObject = new(equipment);

        serializedObject.FindProperty("itemId").intValue = row.ItemId;
        serializedObject.FindProperty("itemName").stringValue = row.ItemName;
        serializedObject.FindProperty("targetClass").enumValueIndex = GetEnumIndex(row.TargetClass);
        serializedObject.FindProperty("slotType").enumValueIndex = GetEnumIndex(row.SlotType);

        if (!string.IsNullOrWhiteSpace(row.Description)) serializedObject.FindProperty("description").stringValue = row.Description;
        if (!string.IsNullOrWhiteSpace(row.CraftLevel)) serializedObject.FindProperty("craftLevel").intValue = ParsePositiveInt(row.CraftLevel, row.ItemId, nameof(row.CraftLevel));
        if (!string.IsNullOrWhiteSpace(row.Grade)) serializedObject.FindProperty("grade").enumValueIndex = ParseEnumIndex<ItemGrade>(row.Grade, row.ItemId, nameof(row.Grade));
        if (!string.IsNullOrWhiteSpace(row.Attack)) serializedObject.FindProperty("attack").intValue = ParseNonNegativeInt(row.Attack, row.ItemId, nameof(row.Attack));
        if (!string.IsNullOrWhiteSpace(row.Defense)) serializedObject.FindProperty("defense").intValue = ParseNonNegativeInt(row.Defense, row.ItemId, nameof(row.Defense));
        if (!string.IsNullOrWhiteSpace(row.Health)) serializedObject.FindProperty("health").intValue = ParseNonNegativeInt(row.Health, row.ItemId, nameof(row.Health));

        Sprite icon = FindSprite(row.IconName);

        if (icon == null)
        {
            throw new InvalidOperationException($"ItemId {row.ItemId}의 Sprite를 찾을 수 없습니다. IconName: {row.IconName}");
        }

        serializedObject.FindProperty("icon").objectReferenceValue = icon;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(equipment);
    }

    // CSV 장비를 ItemDatabase에 등록
    private static void RegisterToItemDatabase(List<EquipmentSO> equipments)
    {
        ItemDatabaseSO itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);

        if (itemDatabase == null)
        {
            throw new InvalidOperationException($"ItemDatabase를 찾을 수 없습니다. Path: {ItemDatabasePath}");
        }

        SerializedObject serializedDatabase = new(itemDatabase);
        SerializedProperty itemsProperty = serializedDatabase.FindProperty("items");
        HashSet<int> registeredIds = new();

        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            ItemSO item = itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue as ItemSO;

            if (item != null)
            {
                registeredIds.Add(item.ItemId);
            }
        }

        foreach (EquipmentSO equipment in equipments)
        {
            if (!registeredIds.Add(equipment.ItemId))
            {
                continue;
            }

            int index = itemsProperty.arraySize;
            itemsProperty.InsertArrayElementAtIndex(index);
            itemsProperty.GetArrayElementAtIndex(index).objectReferenceValue = equipment;
        }

        serializedDatabase.ApplyModifiedProperties();
        EditorUtility.SetDirty(itemDatabase);
    }

    // ItemId 규칙과 CSV 분류 값이 일치하는지 검사
    private static void ValidateRow(EquipmentCsvRow row)
    {
        if (row.ItemId < 1000 || row.ItemId > 1999)
        {
            throw new InvalidOperationException($"장비 ItemId는 1000번대여야 합니다. ItemId: {row.ItemId}");
        }

        int categoryCode = row.ItemId / 1000;
        int classCode = row.ItemId / 100 % 10;
        int slotCode = row.ItemId / 10 % 10;

        if (categoryCode != 1) throw new InvalidOperationException($"장비 ItemId의 천의 자리는 1이어야 합니다. ItemId: {row.ItemId}");
        if (GetClassCode(row.TargetClass) != classCode) throw new InvalidOperationException($"ItemId와 TargetClass가 일치하지 않습니다. ItemId: {row.ItemId}, TargetClass: {row.TargetClass}");
        if (GetSlotCode(row.SlotType) != slotCode) throw new InvalidOperationException($"ItemId와 SlotType이 일치하지 않습니다. ItemId: {row.ItemId}, SlotType: {row.SlotType}");
        if (string.IsNullOrWhiteSpace(row.ItemName)) throw new InvalidOperationException($"ItemName이 비어 있습니다. ItemId: {row.ItemId}");
        if (string.IsNullOrWhiteSpace(row.IconName)) throw new InvalidOperationException($"IconName이 비어 있습니다. ItemId: {row.ItemId}");
    }

    // HeroClassType을 ItemId 백의 자리 코드로 변환
    private static int GetClassCode(HeroClassType heroClass)
    {
        return heroClass switch
        {
            HeroClassType.Warrior => 1,
            HeroClassType.Marksman => 2,
            HeroClassType.Tank => 3,
            HeroClassType.Mage => 4,
            HeroClassType.Support => 5,
            HeroClassType.Rogue => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(heroClass), heroClass, null)
        };
    }

    // EquipmentSlotType을 ItemId 십의 자리 코드로 변환
    private static int GetSlotCode(EquipmentSlotType slotType)
    {
        return slotType switch
        {
            EquipmentSlotType.Weapon => 1,
            EquipmentSlotType.Hands => 2,
            EquipmentSlotType.Accessory => 3,
            EquipmentSlotType.Head => 4,
            EquipmentSlotType.Body => 5,
            EquipmentSlotType.Legs => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(slotType), slotType, null)
        };
    }

    // Sprite 이름으로 Sprite Sheet 하위 Sprite 조회
    private static Sprite FindSprite(string iconName)
    {
        int separatorIndex = iconName.LastIndexOf('_');
        string sheetName = separatorIndex > 0 ? iconName[..separatorIndex] : iconName;
        string[] guids = AssetDatabase.FindAssets($"{sheetName} t:Texture2D", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == iconName)
                {
                    return sprite;
                }
            }
        }

        return null;
    }

    private static int ParsePositiveInt(string value, int itemId, string fieldName)
    {
        if (!int.TryParse(value, out int result) || result < 1)
        {
            throw new InvalidOperationException($"{fieldName}은 1 이상의 정수여야 합니다. ItemId: {itemId}, Value: {value}");
        }

        return result;
    }

    private static int ParseNonNegativeInt(string value, int itemId, string fieldName)
    {
        if (!int.TryParse(value, out int result) || result < 0)
        {
            throw new InvalidOperationException($"{fieldName}은 0 이상의 정수여야 합니다. ItemId: {itemId}, Value: {value}");
        }

        return result;
    }

    private static int ParseEnumIndex<T>(string value, int itemId, string fieldName) where T : struct, Enum
    {
        if (!Enum.TryParse(value, true, out T enumValue))
        {
            throw new InvalidOperationException($"{fieldName} 값을 변환할 수 없습니다. ItemId: {itemId}, Value: {value}");
        }

        return GetEnumIndex(enumValue);
    }

    private static int GetEnumIndex<T>(T value) where T : struct, Enum
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        return Array.IndexOf(values, value);
    }
}
