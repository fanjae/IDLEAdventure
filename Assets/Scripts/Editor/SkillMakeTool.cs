using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;




// 스킬 데이터를 생성하고 수정하는 에디터 툴
public sealed class SkillMakeTool : EditorWindow
{
    private enum SkillOwnerType
    {
        Hero,
        Enemy
    }

    private const string HeroSkillPath = "Assets/Data/SkillData/HeroSkills";
    private const string EnemySkillPath = "Assets/Data/SkillData/EnemySkills";

    private SkillOwnerType ownerType;
    private SkillDataSO selectedSkill;

    private readonly List<SkillDataSO> skillList = new();
    private Vector2 skillListScrollPosition;
    private string searchKeyword = string.Empty;

    private string skillName = string.Empty;
    private string displayName = string.Empty;
    private Sprite icon;
    private string description = string.Empty;

    private SkillEffectType effectType;
    private float damageRatio = 1.5f;
    private float cooldown = 6.0f;
    private float skillSafetyDuration = 10.0f;

    private GameObject barrierVfxPrefab;
    private int blockCount = 5;

    private SkillProjectile projectilePrefab;
    private float projectileSpeed = 10.0f;

    private GameObject healCastVfxPrefab;
    private GameObject healTargetVfxPrefab;
    private float healVfxDuration = 1.5f;

    private GameObject buffCastVfxPrefab;
    private GameObject buffVfxPrefab;
    private int attackBuff = 25;
    private float buffDuration = 6.0f;
    private float buffCastVfxDuration = 1.5f;

    private AreaSkillDamage areaDamagePrefab;
    private float areaRadius = 2.5f;

    private GameObject whirlwindPrefab;
    private float whirlwindDuration = 3.0f;
    private float whirlwindHitInterval = 0.2f;

    private GameObject laserPrefab;
    private float laserDuration = 3.0f;
    private float laserHitInterval = 0.2f;

    private Vector2 scrollPosition;

    [MenuItem("Tools/Skill/Skill Maker")]
    public static void OpenWindow()
    {
        GetWindow<SkillMakeTool>("Skill Maker");
    }

    private void OnEnable()
    {
        // 에디터 창을 열거나 스크립트 리컴파일 후 다시 활성화될 때 현재 프로젝트의 스킬 목록을 갱신
        RefreshSkillList();
    }

    // 스킬 메이커 UI 구성
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawSkillList();
        EditorGUILayout.Space(10f);

        DrawSkillSelection();
        EditorGUILayout.Space(10f);

        DrawBasicInfo();
        EditorGUILayout.Space(10f);

        DrawCommonSettings();
        EditorGUILayout.Space(10f);

        DrawEffectSettings();
        EditorGUILayout.Space(10f);

        DrawSaveSettings();

        EditorGUILayout.EndScrollView();
    }

    // 프로젝트에 등록된 스킬 데이터 목록 표시
    private void DrawSkillList()
    {
        EditorGUILayout.LabelField($"스킬 목록 ({skillList.Count})", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        searchKeyword = EditorGUILayout.TextField("검색", searchKeyword);

        if (GUILayout.Button("새로고침", GUILayout.Width(80f)))
        {
            RefreshSkillList();
        }

        EditorGUILayout.EndHorizontal();

        skillListScrollPosition = EditorGUILayout.BeginScrollView(skillListScrollPosition, EditorStyles.helpBox, GUILayout.Height(180f));

        // 전체 스킬 목록은 유지하고 검색 조건에 맞지 않는 항목만 UI에서 제외
        foreach (SkillDataSO skillData in skillList)
        {
            if (skillData == null || !MatchesSearch(skillData))
            {
                continue;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(skillData.SkillName, GUILayout.MinWidth(130f));
            EditorGUILayout.LabelField(skillData.EffectType.ToString(), GUILayout.Width(120f));

            if (GUILayout.Button("선택", GUILayout.Width(60f)))
            {
                selectedSkill = skillData;
                LoadSkillData(selectedSkill);
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    // 검색어가 스킬 이름 또는 표시 이름에 포함되는지 확인
    private bool MatchesSearch(SkillDataSO skillData)
    {
        if (string.IsNullOrWhiteSpace(searchKeyword))
        {
            return true;
        }

        return skillData.SkillName.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
               skillData.DisplayName.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // 기존 스킬 데이터 선택
    private void DrawSkillSelection()
    {
        EditorGUILayout.LabelField("스킬 데이터", EditorStyles.boldLabel);

        SkillDataSO newSelectedSkill = (SkillDataSO)EditorGUILayout.ObjectField("기존 스킬", selectedSkill, typeof(SkillDataSO), false);

        if (newSelectedSkill != selectedSkill)
        {
            selectedSkill = newSelectedSkill;

            if (selectedSkill != null)
            {
                LoadSkillData(selectedSkill);
            }
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("새 스킬"))
        {
            // 기존 SO 선택을 해제하고 새 SkillDataSO 생성을 위한 기본 입력값으로 초기화
            ResetInputData();
        }

        using (new EditorGUI.DisabledScope(selectedSkill == null))
        {
            if (GUILayout.Button("선택 스킬 다시 불러오기"))
            {
                LoadSkillData(selectedSkill);
            }

            if (GUILayout.Button("복제"))
            {
                DuplicateSkillData();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    // 스킬 기본 정보 표시
    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);

        skillName = EditorGUILayout.TextField("Skill Name", skillName);
        displayName = EditorGUILayout.TextField("Display Name", displayName);
        icon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);

        EditorGUILayout.LabelField("Description");
        description = EditorGUILayout.TextArea(description, GUILayout.MinHeight(50f));
    }

    // 모든 스킬에서 공통으로 사용하는 설정 표시
    private void DrawCommonSettings()
    {
        EditorGUILayout.LabelField("공통 설정", EditorStyles.boldLabel);

        effectType = (SkillEffectType)EditorGUILayout.EnumPopup("Effect Type", effectType);

        if (UsesDamageRatio())
        {
            damageRatio = EditorGUILayout.FloatField("Effect Ratio", damageRatio);
        }

        cooldown = EditorGUILayout.FloatField("Cooldown", cooldown);
        skillSafetyDuration = EditorGUILayout.FloatField("Safety Duration", skillSafetyDuration);
    }

    // SkillDataSO의 damageRatio 값을 실제 효과 계수로 사용하는 스킬 타입인지 확인
    // Heal 등 피해가 아닌 타입도 동일 필드를 효과 계수로 사용하므로 타입 기준으로 판별
    private bool UsesDamageRatio()
    {
        switch (effectType)
        {
            case SkillEffectType.Damage:
            case SkillEffectType.Heal:
            case SkillEffectType.ProjectileDamage:
            case SkillEffectType.AreaDamage:
            case SkillEffectType.Whirlwind:
            case SkillEffectType.Laser:
                return true;
        }

        return false;
    }

    // Effect Type에 따라 해당 스킬이 사용하는 전용 데이터만 노출
    // 다른 타입의 값은 UI에서 숨겨질 뿐 SkillDataSO 내부 값 자체가 초기화되지는 않음
    private void DrawEffectSettings()
    {
        EditorGUILayout.LabelField("타입별 설정", EditorStyles.boldLabel);

        switch (effectType)
        {
            case SkillEffectType.Damage:
                DrawDamageSettings();
                break;

            case SkillEffectType.Heal:
                DrawHealSettings();
                break;

            case SkillEffectType.Barrier:
                DrawBarrierSettings();
                break;

            case SkillEffectType.ProjectileDamage:
                DrawProjectileSettings();
                break;

            case SkillEffectType.Buff:
                DrawBuffSettings();
                break;

            case SkillEffectType.AreaDamage:
                DrawAreaDamageSettings();
                break;

            case SkillEffectType.Whirlwind:
                DrawWhirlwindSettings();
                break;

            case SkillEffectType.Laser:
                DrawLaserSettings();
                break;
        }
    }

    private void DrawDamageSettings()
    {
        EditorGUILayout.HelpBox("현재 Damage 스킬은 추가 설정이 없습니다.", MessageType.Info);
    }

    private void DrawHealSettings()
    {
        healCastVfxPrefab = (GameObject)EditorGUILayout.ObjectField("Cast VFX", healCastVfxPrefab, typeof(GameObject), false);
        healTargetVfxPrefab = (GameObject)EditorGUILayout.ObjectField("Target VFX", healTargetVfxPrefab, typeof(GameObject), false);
        healVfxDuration = EditorGUILayout.FloatField("VFX Duration", healVfxDuration);
    }

    private void DrawBarrierSettings()
    {
        barrierVfxPrefab = (GameObject)EditorGUILayout.ObjectField("Barrier VFX", barrierVfxPrefab, typeof(GameObject), false);
        blockCount = EditorGUILayout.IntField("Block Count", blockCount);
    }

    private void DrawProjectileSettings()
    {
        projectilePrefab = (SkillProjectile)EditorGUILayout.ObjectField("Projectile Prefab", projectilePrefab, typeof(SkillProjectile), false);
        projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", projectileSpeed);
    }

    private void DrawBuffSettings()
    {
        buffCastVfxPrefab = (GameObject)EditorGUILayout.ObjectField("Cast VFX", buffCastVfxPrefab, typeof(GameObject), false);
        buffVfxPrefab = (GameObject)EditorGUILayout.ObjectField("Buff VFX", buffVfxPrefab, typeof(GameObject), false);
        attackBuff = EditorGUILayout.IntField("Attack Buff", attackBuff);
        buffDuration = EditorGUILayout.FloatField("Buff Duration", buffDuration);
        buffCastVfxDuration = EditorGUILayout.FloatField("Cast VFX Duration", buffCastVfxDuration);
    }

    private void DrawAreaDamageSettings()
    {
        areaDamagePrefab = (AreaSkillDamage)EditorGUILayout.ObjectField("Area Damage Prefab", areaDamagePrefab, typeof(AreaSkillDamage), false);
        areaRadius = EditorGUILayout.FloatField("Area Radius", areaRadius);
    }

    private void DrawWhirlwindSettings()
    {
        whirlwindPrefab = (GameObject)EditorGUILayout.ObjectField("Whirlwind Prefab", whirlwindPrefab, typeof(GameObject), false);
        whirlwindDuration = EditorGUILayout.FloatField("Duration", whirlwindDuration);
        whirlwindHitInterval = EditorGUILayout.FloatField("Hit Interval", whirlwindHitInterval);
    }

    private void DrawLaserSettings()
    {
        laserPrefab = (GameObject)EditorGUILayout.ObjectField("Laser Prefab", laserPrefab, typeof(GameObject), false);
        laserDuration = EditorGUILayout.FloatField("Duration", laserDuration);
        laserHitInterval = EditorGUILayout.FloatField("Hit Interval", laserHitInterval);
    }

    // 저장 위치 및 생성 버튼 표시
    private void DrawSaveSettings()
    {
        EditorGUILayout.LabelField("저장 설정", EditorStyles.boldLabel);

        ownerType = (SkillOwnerType)EditorGUILayout.EnumPopup("Owner Type", ownerType);

        string savePath = GetSavePath();
        EditorGUILayout.LabelField("저장 경로", savePath);

        EditorGUILayout.Space(5f);

        if (selectedSkill == null)
        {
            if (GUILayout.Button("스킬 생성", GUILayout.Height(30f)))
            {
                CreateSkillData();
            }
        }
        else
        {
            if (GUILayout.Button("스킬 수정", GUILayout.Height(30f)))
            {
                UpdateSkillData();
            }
        }
    }

    // 선택한 스킬 소유 타입에 따라 SkillDataSO가 저장될 기준 폴더 반환
    private string GetSavePath()
    {
        return ownerType == SkillOwnerType.Hero ? HeroSkillPath : EnemySkillPath;
    }

    // Hero / Enemy 스킬 폴더에서 SkillDataSO 목록 새로고침
    private void RefreshSkillList()
    {
        skillList.Clear();

        // Hero / Enemy 폴더를 한 번에 검색하여 SkillDataSO 목록 구성
        string[] searchFolders = { HeroSkillPath, EnemySkillPath };
        string[] guids = AssetDatabase.FindAssets("t:SkillDataSO", searchFolders);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SkillDataSO skillData = AssetDatabase.LoadAssetAtPath<SkillDataSO>(path);

            if (skillData != null)
            {
                skillList.Add(skillData);
            }
        }

        // 검색 결과의 파일 순서와 관계없이 Skill Name 기준으로 목록 정렬
        skillList.Sort((left, right) => string.Compare(left.SkillName, right.SkillName, StringComparison.OrdinalIgnoreCase));
        Repaint();
    }

    // 스킬 생성에 필요한 데이터가 정상적으로 설정되어 있는지 확인
    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(skillName))
        {
            EditorUtility.DisplayDialog("Skill Maker", "Skill Name을 입력해주세요.", "확인");
            return false;
        }

        if (cooldown <= 0.0f)
        {
            EditorUtility.DisplayDialog("Skill Maker", "Cooldown은 0보다 커야 합니다.", "확인");
            return false;
        }

        if (skillSafetyDuration <= 0.0f)
        {
            EditorUtility.DisplayDialog("Skill Maker", "Safety Duration은 0보다 커야 합니다.", "확인");
            return false;
        }

        if (UsesDamageRatio() && damageRatio < 0.0f)
        {
            EditorUtility.DisplayDialog("Skill Maker", "Effect Ratio는 0 이상이어야 합니다.", "확인");
            return false;
        }

        // 공통 데이터 검증이 끝난 뒤 Effect Type에 따른 전용 필수값 검증
        return ValidateEffectData();
    }

    // 스킬 타입별 필수 데이터를 확인
    private bool ValidateEffectData()
    {
        switch (effectType)
        {
            case SkillEffectType.Barrier:
                if (blockCount <= 0)
                {
                    EditorUtility.DisplayDialog("Skill Maker", "Block Count는 1 이상이어야 합니다.", "확인");
                    return false;
                }
                break;

            case SkillEffectType.ProjectileDamage:
                if (projectilePrefab == null)
                {
                    EditorUtility.DisplayDialog("Skill Maker", "Projectile Prefab을 지정해주세요.", "확인");
                    return false;
                }

                if (projectileSpeed <= 0.0f)
                {
                    EditorUtility.DisplayDialog("Skill Maker", "Projectile Speed는 0보다 커야 합니다.", "확인");
                    return false;
                }
                break;

            case SkillEffectType.AreaDamage:
                if (areaDamagePrefab == null)
                {
                    EditorUtility.DisplayDialog("Skill Maker", "Area Damage Prefab을 지정해주세요.", "확인");
                    return false;
                }

                if (areaRadius <= 0.0f)
                {
                    EditorUtility.DisplayDialog("Skill Maker", "Area Radius는 0보다 커야 합니다.", "확인");
                    return false;
                }
                break;

            case SkillEffectType.Whirlwind:
                if (whirlwindPrefab == null)
                {
                    EditorUtility.DisplayDialog("Skill Maker", "Whirlwind Prefab을 지정해주세요.", "확인");
                    return false;
                }
                break;

            case SkillEffectType.Laser:
                if (laserPrefab == null)
                {
                    EditorUtility.DisplayDialog("Skill Maker", "Laser Prefab을 지정해주세요.", "확인");
                    return false;
                }
                break;
        }

        return true;
    }

    // 입력한 정보를 기반으로 새로운 SkillDataSO 생성
    private void CreateSkillData()
    {
        if (!ValidateInput())
        {
            return;
        }

        string savePath = GetSavePath();
        string assetPath = $"{savePath}/{skillName}.asset";

        // Skill Name을 에셋 파일명으로 사용하므로 동일 경로에 같은 이름의 SO가 있는지 먼저 확인
        if (AssetDatabase.LoadAssetAtPath<SkillDataSO>(assetPath) != null)
        {
            EditorUtility.DisplayDialog("Skill Maker", "같은 이름의 스킬 데이터가 이미 존재합니다.", "확인");
            return;
        }

        // 메모리에 SkillDataSO 인스턴스를 생성한 뒤 현재 에디터 입력값을 적용
        SkillDataSO skillData = CreateInstance<SkillDataSO>();
        ApplySkillData(skillData);

        // 실제 프로젝트 Asset으로 생성하고 AssetDatabase에 반영
        AssetDatabase.CreateAsset(skillData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        selectedSkill = skillData;
        RefreshSkillList();

        // 생성 직후 Project 창에서 해당 에셋을 선택하고 위치 표시
        Selection.activeObject = skillData;
        EditorGUIUtility.PingObject(skillData);

        Debug.Log($"[SkillMakeTool] 스킬 데이터 생성 - {assetPath}");
    }

    // 에디터 입력값을 SkillDataSO의 SerializeField에 일괄 반영
    // SkillDataSO의 필드가 private SerializeField이므로 SerializedObject를 통해 수정
    private void ApplySkillData(SkillDataSO skillData)
    {
        SerializedObject serializedObject = new SerializedObject(skillData);

        serializedObject.FindProperty("skillName").stringValue = skillName;
        serializedObject.FindProperty("displayName").stringValue = displayName;
        serializedObject.FindProperty("icon").objectReferenceValue = icon;
        serializedObject.FindProperty("description").stringValue = description;

        serializedObject.FindProperty("effectType").enumValueIndex = (int)effectType;
        serializedObject.FindProperty("damageRatio").floatValue = damageRatio;
        serializedObject.FindProperty("cooldown").floatValue = cooldown;
        serializedObject.FindProperty("skillSafetyDuration").floatValue = skillSafetyDuration;

        serializedObject.FindProperty("barrierVfxPrefab").objectReferenceValue = barrierVfxPrefab;
        serializedObject.FindProperty("blockCount").intValue = blockCount;

        serializedObject.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        serializedObject.FindProperty("projectileSpeed").floatValue = projectileSpeed;

        serializedObject.FindProperty("healCastVfxPrefab").objectReferenceValue = healCastVfxPrefab;
        serializedObject.FindProperty("healTargetVfxPrefab").objectReferenceValue = healTargetVfxPrefab;
        serializedObject.FindProperty("healVfxDuration").floatValue = healVfxDuration;

        serializedObject.FindProperty("buffCastVfxPrefab").objectReferenceValue = buffCastVfxPrefab;
        serializedObject.FindProperty("buffVfxPrefab").objectReferenceValue = buffVfxPrefab;
        serializedObject.FindProperty("attackBuff").intValue = attackBuff;
        serializedObject.FindProperty("buffDuration").floatValue = buffDuration;
        serializedObject.FindProperty("buffCastVfxDuration").floatValue = buffCastVfxDuration;

        serializedObject.FindProperty("areaDamagePrefab").objectReferenceValue = areaDamagePrefab;
        serializedObject.FindProperty("areaRadius").floatValue = areaRadius;

        serializedObject.FindProperty("whirlwindPrefab").objectReferenceValue = whirlwindPrefab;
        serializedObject.FindProperty("whirlwindDuration").floatValue = whirlwindDuration;
        serializedObject.FindProperty("whirlwindHitInterval").floatValue = whirlwindHitInterval;

        serializedObject.FindProperty("laserPrefab").objectReferenceValue = laserPrefab;
        serializedObject.FindProperty("laserDuration").floatValue = laserDuration;
        serializedObject.FindProperty("laserHitInterval").floatValue = laserHitInterval;

        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(skillData);
    }

    // 선택한 기존 스킬 데이터 수정
    private void UpdateSkillData()
    {
        if (selectedSkill == null)
        {
            return;
        }

        if (!ValidateInput())
        {
            return;
        }

        string currentPath = AssetDatabase.GetAssetPath(selectedSkill);
        string targetPath = $"{GetSavePath()}/{skillName}.asset";

        // Skill Name 또는 Owner Type 변경으로 저장 경로가 달라진 경우 기존 에셋을 새 경로로 이동
        if (!string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            // 이동 대상 경로에 다른 SkillDataSO가 존재하면 덮어쓰지 않고 수정 중단
            SkillDataSO existingSkill = AssetDatabase.LoadAssetAtPath<SkillDataSO>(targetPath);

            if (existingSkill != null && existingSkill != selectedSkill)
            {
                EditorUtility.DisplayDialog("Skill Maker", "변경하려는 이름과 경로에 이미 스킬 데이터가 존재합니다.", "확인");
                return;
            }

            // 기존 SO를 유지한 상태에서 파일명 또는 Hero / Enemy 폴더 위치만 변경
            string moveError = AssetDatabase.MoveAsset(currentPath, targetPath);

            if (!string.IsNullOrEmpty(moveError))
            {
                EditorUtility.DisplayDialog("Skill Maker", $"스킬 데이터 이동에 실패했습니다.\n{moveError}", "확인");
                return;
            }
        }

        // 수정 전 상태를 Unity Undo 스택에 기록
        Undo.RecordObject(selectedSkill, "Update Skill Data");
        ApplySkillData(selectedSkill);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RefreshSkillList();

        Selection.activeObject = selectedSkill;
        EditorGUIUtility.PingObject(selectedSkill);

        Debug.Log($"[SkillMakeTool] 스킬 데이터 수정 - {AssetDatabase.GetAssetPath(selectedSkill)}");
    }

    // 선택한 스킬 데이터를 복제
    private void DuplicateSkillData()
    {
        if (selectedSkill == null)
        {
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(selectedSkill);

        // 기존 파일을 덮어쓰지 않도록 Unity가 사용 가능한 고유 복제 경로 생성
        string duplicatePath = AssetDatabase.GenerateUniqueAssetPath($"{GetSavePath()}/{selectedSkill.SkillName}_Copy.asset");

        if (!AssetDatabase.CopyAsset(sourcePath, duplicatePath))
        {
            EditorUtility.DisplayDialog("Skill Maker", "스킬 데이터 복제에 실패했습니다.", "확인");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SkillDataSO duplicatedSkill = AssetDatabase.LoadAssetAtPath<SkillDataSO>(duplicatePath);

        if (duplicatedSkill == null)
        {
            return;
        }

        selectedSkill = duplicatedSkill;
        LoadSkillData(selectedSkill);

        // 복제된 에셋의 파일명을 새로운 Skill Name 기본값으로 사용
        skillName = duplicatedSkill.name;

        // 변경된 Skill Name을 복제된 SkillDataSO 내부 데이터에도 반영
        ApplySkillData(selectedSkill);
        AssetDatabase.SaveAssets();
        RefreshSkillList();

        Selection.activeObject = selectedSkill;
        EditorGUIUtility.PingObject(selectedSkill);

        Debug.Log($"[SkillMakeTool] 스킬 데이터 복제 - {duplicatePath}");
    }

    // 기존 SkillDataSO의 SerializeField 값을 읽어 Skill Maker의 입력 상태로 복원
    private void LoadSkillData(SkillDataSO skillData)
    {
        if (skillData == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(skillData);

        // 에셋의 최신 Serialized 값을 읽기 위해 SerializedObject 상태 갱신
        serializedObject.Update();

        skillName = serializedObject.FindProperty("skillName").stringValue;
        displayName = serializedObject.FindProperty("displayName").stringValue;
        icon = serializedObject.FindProperty("icon").objectReferenceValue as Sprite;
        description = serializedObject.FindProperty("description").stringValue;

        effectType = (SkillEffectType)serializedObject.FindProperty("effectType").enumValueIndex;
        damageRatio = serializedObject.FindProperty("damageRatio").floatValue;
        cooldown = serializedObject.FindProperty("cooldown").floatValue;
        skillSafetyDuration = serializedObject.FindProperty("skillSafetyDuration").floatValue;

        barrierVfxPrefab = serializedObject.FindProperty("barrierVfxPrefab").objectReferenceValue as GameObject;
        blockCount = serializedObject.FindProperty("blockCount").intValue;

        projectilePrefab = serializedObject.FindProperty("projectilePrefab").objectReferenceValue as SkillProjectile;
        projectileSpeed = serializedObject.FindProperty("projectileSpeed").floatValue;

        healCastVfxPrefab = serializedObject.FindProperty("healCastVfxPrefab").objectReferenceValue as GameObject;
        healTargetVfxPrefab = serializedObject.FindProperty("healTargetVfxPrefab").objectReferenceValue as GameObject;
        healVfxDuration = serializedObject.FindProperty("healVfxDuration").floatValue;

        buffCastVfxPrefab = serializedObject.FindProperty("buffCastVfxPrefab").objectReferenceValue as GameObject;
        buffVfxPrefab = serializedObject.FindProperty("buffVfxPrefab").objectReferenceValue as GameObject;
        attackBuff = serializedObject.FindProperty("attackBuff").intValue;
        buffDuration = serializedObject.FindProperty("buffDuration").floatValue;
        buffCastVfxDuration = serializedObject.FindProperty("buffCastVfxDuration").floatValue;

        areaDamagePrefab = serializedObject.FindProperty("areaDamagePrefab").objectReferenceValue as AreaSkillDamage;
        areaRadius = serializedObject.FindProperty("areaRadius").floatValue;

        whirlwindPrefab = serializedObject.FindProperty("whirlwindPrefab").objectReferenceValue as GameObject;
        whirlwindDuration = serializedObject.FindProperty("whirlwindDuration").floatValue;
        whirlwindHitInterval = serializedObject.FindProperty("whirlwindHitInterval").floatValue;

        laserPrefab = serializedObject.FindProperty("laserPrefab").objectReferenceValue as GameObject;
        laserDuration = serializedObject.FindProperty("laserDuration").floatValue;
        laserHitInterval = serializedObject.FindProperty("laserHitInterval").floatValue;

        string assetPath = AssetDatabase.GetAssetPath(skillData);

        // 현재 SO가 위치한 폴더를 기준으로 Hero / Enemy Owner Type 복원
        if (assetPath.StartsWith(HeroSkillPath))
        {
            ownerType = SkillOwnerType.Hero;
        }
        else if (assetPath.StartsWith(EnemySkillPath))
        {
            ownerType = SkillOwnerType.Enemy;
        }
    }

    // 기존 선택을 해제하고 새 스킬 작성에 사용할 기본값으로 에디터 상태 초기화
    // 여기의 기본값은 Skill Maker에서 신규 데이터 생성 시 사용하는 초기 입력값
    private void ResetInputData()
    {
        selectedSkill = null;

        ownerType = SkillOwnerType.Hero;

        skillName = string.Empty;
        displayName = string.Empty;
        icon = null;
        description = string.Empty;

        effectType = SkillEffectType.Damage;
        damageRatio = 1.5f;
        cooldown = 6.0f;
        skillSafetyDuration = 10.0f;

        barrierVfxPrefab = null;
        blockCount = 5;

        projectilePrefab = null;
        projectileSpeed = 10.0f;

        healCastVfxPrefab = null;
        healTargetVfxPrefab = null;
        healVfxDuration = 1.5f;

        buffCastVfxPrefab = null;
        buffVfxPrefab = null;
        attackBuff = 25;
        buffDuration = 6.0f;
        buffCastVfxDuration = 1.5f;

        areaDamagePrefab = null;
        areaRadius = 2.5f;

        whirlwindPrefab = null;
        whirlwindDuration = 3.0f;
        whirlwindHitInterval = 0.2f;

        laserPrefab = null;
        laserDuration = 3.0f;
        laserHitInterval = 0.2f;

        GUI.FocusControl(null);
    }
}
