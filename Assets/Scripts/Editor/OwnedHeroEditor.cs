using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 보유 영웅 조회 및 테스트 관리 에디터
public sealed class OwnedHeroEditor : EditorWindow
{
    private const string HeroDatabasePath = "Assets/Resources/GameData/HeroDatabase.asset";
    private const float PortraitSize = 96f;

    private Vector2 scrollPosition;
    private int addHeroIndex;

    // 보유 영웅 관리 창 열기
    [MenuItem("Tools/Test/Owned Hero Manager")]
    public static void OpenWindow()
    {
        GetWindow<OwnedHeroEditor>("Owned Hero Manager");
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    // 보유 영웅 관리 에디터 UI 구성
    private void OnGUI()
    {
        EditorGUILayout.LabelField("보유 영웅 관리", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("보유 영웅 관리는 Play Mode에서 사용할 수 있습니다.", MessageType.Info);
            return;
        }

        if (!TryGetHeroController(out HeroManager heroManager, out HeroController controller))
        {
            EditorGUILayout.HelpBox("HeroManager가 초기화되지 않았습니다.", MessageType.Warning);
            return;
        }

        DrawOwnedHeroList(heroManager, controller);
        EditorGUILayout.Space();
        DrawAddHeroSection(controller);
    }

    // 현재 보유 영웅 목록 표시
    private void DrawOwnedHeroList(HeroManager heroManager, HeroController controller)
    {
        EditorGUILayout.LabelField($"보유 영웅 {controller.Heroes.Count}명", EditorStyles.boldLabel);
        EditorGUILayout.Space(4f);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        List<OwnedHeroData> heroes = new(controller.Heroes);

        foreach (OwnedHeroData hero in heroes)
        {
            DrawHeroCard(heroManager, controller, hero);
            EditorGUILayout.Space(6f);
        }

        EditorGUILayout.EndScrollView();
    }

    // 보유 영웅 한 명의 정보 표시
    private void DrawHeroCard(HeroManager heroManager, HeroController controller, OwnedHeroData hero)
    {
        if (hero == null || hero.HeroData == null)
        {
            return;
        }

        HeroData heroData = hero.HeroData;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        Rect portraitRect = GUILayoutUtility.GetRect(PortraitSize, PortraitSize, GUILayout.Width(PortraitSize), GUILayout.Height(PortraitSize));
        DrawPortrait(portraitRect, heroData.Portrait);

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(heroData.UnitName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Hero ID", hero.HeroId);
        EditorGUILayout.LabelField("Class", heroData.ClassType.ToString());
        EditorGUILayout.LabelField("Role", heroData.Role.ToString());
        EditorGUILayout.LabelField("Level", hero.Level.ToString());

        if (controller.TryGetHeroStat(hero.HeroId, out HeroStat stat))
        {
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("최종 능력치", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Max HP", stat.MaxHp.ToString());
            EditorGUILayout.LabelField("Attack", stat.Attack.ToString());
            EditorGUILayout.LabelField("Defense", stat.Defense.ToString());
        }

        EditorGUILayout.Space(3f);
        EditorGUILayout.LabelField("기본 / 성장 능력치", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Base HP", $"{heroData.MaxHp}  (+{heroData.HpPerLevel}/Lv)");
        EditorGUILayout.LabelField("Base Attack", $"{heroData.Attack}  (+{heroData.AttackPerLevel}/Lv)");
        EditorGUILayout.LabelField("Base Defense", $"{heroData.Defense}  (+{heroData.DefensePerLevel}/Lv)");

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4f);

        if (GUILayout.Button("보유 영웅 삭제"))
        {
            RemoveHero(heroManager, hero);
        }

        EditorGUILayout.EndVertical();
    }

    // 영웅 초상화 표시
    private static void DrawPortrait(Rect rect, Sprite portrait)
    {
        if (portrait == null || portrait.texture == null)
        {
            EditorGUI.HelpBox(rect, "No Portrait", MessageType.None);
            return;
        }

        Rect textureRect = portrait.textureRect;
        Texture2D texture = portrait.texture;
        Rect texCoords = new(textureRect.x / texture.width, textureRect.y / texture.height, textureRect.width / texture.width, textureRect.height / texture.height);
        GUI.DrawTextureWithTexCoords(rect, texture, texCoords, true);
    }

    // 보유하지 않은 영웅 추가 UI 표시
    private void DrawAddHeroSection(HeroController controller)
    {
        HeroDatabaseSO heroDatabase = AssetDatabase.LoadAssetAtPath<HeroDatabaseSO>(HeroDatabasePath);

        if (heroDatabase == null)
        {
            EditorGUILayout.HelpBox($"HeroDatabase를 찾을 수 없습니다.\n{HeroDatabasePath}", MessageType.Error);
            return;
        }

        List<HeroData> availableHeroes = new();

        foreach (HeroData heroData in heroDatabase.Heroes)
        {
            if (heroData == null || string.IsNullOrEmpty(heroData.UnitID) || controller.ContainsHero(heroData.UnitID))
            {
                continue;
            }

            availableHeroes.Add(heroData);
        }

        EditorGUILayout.LabelField("영웅 추가", EditorStyles.boldLabel);

        if (availableHeroes.Count == 0)
        {
            EditorGUILayout.HelpBox("추가할 수 있는 미보유 영웅이 없습니다.", MessageType.Info);
            return;
        }

        string[] heroNames = new string[availableHeroes.Count];

        for (int i = 0; i < availableHeroes.Count; i++)
        {
            HeroData heroData = availableHeroes[i];
            heroNames[i] = $"{heroData.UnitName} ({heroData.UnitID})";
        }

        addHeroIndex = Mathf.Clamp(addHeroIndex, 0, availableHeroes.Count - 1);
        addHeroIndex = EditorGUILayout.Popup("추가할 영웅", addHeroIndex, heroNames);

        if (GUILayout.Button("선택한 영웅 추가"))
        {
            AddHero(controller, availableHeroes[addHeroIndex]);
        }
    }

    // 선택한 영웅 보유 목록에 추가
    private static void AddHero(HeroController controller, HeroData heroData)
    {
        if (heroData == null || !controller.TryAcquireHero(heroData.UnitID))
        {
            Debug.LogWarning("[OwnedHeroEditor] 영웅 추가에 실패했습니다.");
            return;
        }

        SaveCurrentData();
        Debug.Log($"[OwnedHeroEditor] 영웅 추가 완료 - HeroId: {heroData.UnitID}");
    }

    // 선택한 영웅 보유 목록에서 제거
    private static void RemoveHero(HeroManager heroManager, OwnedHeroData hero)
    {
        if (hero == null)
        {
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog("보유 영웅 삭제", $"{hero.HeroData.UnitName} ({hero.HeroId}) 영웅을 삭제하시겠습니까?", "삭제", "취소");

        if (!confirmed)
        {
            return;
        }

        if (!heroManager.TryRemoveOwnedHero(hero.HeroId))
        {
            Debug.LogWarning($"[OwnedHeroEditor] 영웅 삭제에 실패했습니다. - HeroId: {hero.HeroId}");
            return;
        }

        SaveCurrentData();
        Debug.Log($"[OwnedHeroEditor] 영웅 삭제 완료 - HeroId: {hero.HeroId}");
    }

    // 플레이 모드 및 HeroManager 초기화 여부 확인
    private static bool TryGetHeroController(out HeroManager heroManager, out HeroController controller)
    {
        heroManager = null;
        controller = null;

        if (!HeroManager.TryGetExistingInstance(out heroManager) || !heroManager.IsInitialized)
        {
            return false;
        }

        controller = heroManager.Controller;
        return true;
    }

    // 테스트로 변경된 보유 영웅 데이터를 저장
    private static void SaveCurrentData()
    {
        if (!SaveManager.TryGetExistingInstance(out SaveManager saveManager) || saveManager.CurrentData == null)
        {
            Debug.LogWarning("[OwnedHeroEditor] SaveManager가 초기화되지 않아 변경 내용을 저장하지 못했습니다.");
            return;
        }

        saveManager.Save();
    }
}
