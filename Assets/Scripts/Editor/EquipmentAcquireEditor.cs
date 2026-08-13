using UnityEditor;
using UnityEngine;

public sealed class EquipmentAcquireEditor : EditorWindow
{
    // 테스트에 사용할 장비 아이디
    private const int MarksmanWeaponId = 1211;
    private const int MarksmanAccessoryId = 1231;
    private const int TankWeaponId = 1311;
    private const int TankWeapon2Id = 1312;
    private const int TankBodyId = 1351;
    private const int SupportWeaponId = 1511;
    private const int SupportBodyId = 1551;

    // 전체 장비 획득에 사용할 장비 아이디 목록
    private static readonly int[] EquipmentIds =
    {
        MarksmanWeaponId,
        MarksmanAccessoryId,
        TankWeaponId,
        TankWeapon2Id,
        TankBodyId,
        SupportWeaponId,
        SupportBodyId
    };

    // 직접 입력해서 획득할 장비 아이디
    private int equipmentId = MarksmanWeaponId;

    // 장비 획득 테스트 창 열기
    [MenuItem("Tools/Test/Equipment Acquire")]
    public static void OpenWindow()
    {
        GetWindow<EquipmentAcquireEditor>("Equipment Acquire");
    }

    // 장비 획득 테스트용 에디터 UI 구성
    private void OnGUI()
    {
        EditorGUILayout.LabelField("장비 획득 테스트", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        equipmentId = EditorGUILayout.IntField("Equipment ID", equipmentId);

        if (GUILayout.Button("입력한 장비 획득"))
        {
            AcquireEquipment(equipmentId);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("등록 장비", EditorStyles.boldLabel);

        if (GUILayout.Button("1211 - Marksman Weapon")) AcquireEquipment(MarksmanWeaponId);
        if (GUILayout.Button("1231 - Marksman Accessory")) AcquireEquipment(MarksmanAccessoryId);
        if (GUILayout.Button("1311 - Tank Weapon")) AcquireEquipment(TankWeaponId);
        if (GUILayout.Button("1312 - Tank Weapon 2")) AcquireEquipment(TankWeapon2Id);
        if (GUILayout.Button("1351 - Tank Body")) AcquireEquipment(TankBodyId);
        if (GUILayout.Button("1511 - Support Weapon")) AcquireEquipment(SupportWeaponId);
        if (GUILayout.Button("1551 - Support Body")) AcquireEquipment(SupportBodyId);

        EditorGUILayout.Space();

        if (GUILayout.Button("전체 장비 획득"))
        {
            AcquireAllEquipment();
        }
    }

    // 입력된 장비 아이디를 기준으로 장비 획득
    private static void AcquireEquipment(int targetEquipmentId)
    {
        if (!TryGetInventoryController(out InventoryController controller))
        {
            return;
        }

        if (!controller.TryAcquireEquipment(targetEquipmentId, out string instanceId))
        {
            Debug.LogWarning($"[EquipmentAcquireEditor] 장비 획득 실패 - EquipmentId: {targetEquipmentId}");
            return;
        }

        SaveCurrentData();
        Debug.Log($"[EquipmentAcquireEditor] 장비 획득 완료 - EquipmentId: {targetEquipmentId}, InstanceId: {instanceId}");
    }

    // 현재 등록된 테스트 장비 전체 획득
    private static void AcquireAllEquipment()
    {
        if (!TryGetInventoryController(out InventoryController controller))
        {
            return;
        }

        int acquiredCount = 0;

        foreach (int targetEquipmentId in EquipmentIds)
        {
            if (!controller.TryAcquireEquipment(targetEquipmentId, out _))
            {
                Debug.LogWarning($"[EquipmentAcquireEditor] 장비 획득 실패 - EquipmentId: {targetEquipmentId}");
                continue;
            }

            acquiredCount++;
        }

        if (acquiredCount > 0)
        {
            SaveCurrentData();
        }

        Debug.Log($"[EquipmentAcquireEditor] 전체 장비 획득 완료 - {acquiredCount}/{EquipmentIds.Length}");
    }

    // 플레이 모드 및 InventoryManager 초기화 여부 확인
    private static bool TryGetInventoryController(out InventoryController controller)
    {
        controller = null;

        if (!Application.isPlaying)
        {
            Debug.LogWarning("[EquipmentAcquireEditor] 장비 획득 테스트는 Play Mode에서만 사용할 수 있습니다.");
            return false;
        }

        if (!InventoryManager.TryGetExistingInstance(out InventoryManager inventoryManager) || !inventoryManager.IsInitialized)
        {
            Debug.LogWarning("[EquipmentAcquireEditor] InventoryManager가 초기화되지 않았습니다.");
            return false;
        }

        controller = inventoryManager.Controller;
        return true;
    }

    // 테스트로 변경된 장비 데이터를 저장
    private static void SaveCurrentData()
    {
        if (!SaveManager.TryGetExistingInstance(out SaveManager saveManager) || saveManager.CurrentData == null)
        {
            return;
        }

        saveManager.Save();
    }
}
