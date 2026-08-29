using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 퀘스트 생성 툴 클래스 <br/>
/// 기본적인 퀘스트 데이터 + NPC, 대사, 보상 데이터 설정 가능하도록 구상 중.
/// </summary>
public class QuestMakerTool : EditorWindow
{
    private int questId;
    private string questName;
    private QuestType questType;

    private Vector3 arrivePosition;
    private Vector3 spawnPosition;

    private GameObject[] npcPrefabs;
    private string[] npcNames;
    private int selectedNPCIndex;
    private readonly string npcPrefabPath = "Assets/Resources/GameData/Quests/QuestNPCPrefabs";

    private QuestDialogueData tempDialogue;
    private QuestRewardData tempReward;
    private SerializedObject dialogueSO;
    private SerializedObject rewardSO;

    private Vector2 scrollPosition;

    private void OnEnable()
    {
        LoadNPCPrefabs();
        SetTempData();
    }
    private void OnGUI()
    {
        if (dialogueSO == null || rewardSO == null)
        {
            SetTempData();
        }
        // 입력에 따른 변경사항 메모리 데이터 최신 상태 갱신.
        dialogueSO.Update();
        rewardSO.Update();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // ID, Name, Type 입력 UI 그리기.
        GUILayout.Label("Quest Info", EditorStyles.boldLabel);
        questId = EditorGUILayout.IntField("ID", questId);
        questName = EditorGUILayout.TextField("Name", questName);
        questType = (QuestType)EditorGUILayout.EnumPopup("Type", questType);

        GUILayout.Space(10);

        // Arrive, Spawn Position 입력 UI 그리기.
        GUILayout.Label("Target Data", EditorStyles.boldLabel);
        arrivePosition = EditorGUILayout.Vector3Field("Arrive Position", arrivePosition);
        spawnPosition = EditorGUILayout.Vector3Field("Spawn Position", spawnPosition);

        // 저장해둔 NPC 리스트 선택 UI 그리기.
        selectedNPCIndex = EditorGUILayout.Popup("NPC Prefab", selectedNPCIndex, npcNames);

        GUILayout.Space(15);

        GUILayout.Label("Dialogue Data", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        // 대사 데이터 입력 UI 그리기.
        EditorGUILayout.PropertyField(dialogueSO.FindProperty("dialogueDatas"), true);
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        GUILayout.Label("Reward Data", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        // 보상 데이터 입력 UI 그리기.
        EditorGUILayout.PropertyField(rewardSO.FindProperty("currencyRewards"), true);
        EditorGUILayout.EndVertical();

        GUILayout.Space(25);

        // Create 버튼 클릭 시 설정된 데이터에 따라 퀘스트 데이터 실제 에셋으로 생성.
        if (GUILayout.Button("Create", GUILayout.Height(40)))
        {
            SetQuestAssets();
        }

        EditorGUILayout.EndScrollView();

        // +,- 를 통해 수정된 데이터 갱신.
        dialogueSO.ApplyModifiedProperties();
        rewardSO.ApplyModifiedProperties();
    }

    // 메뉴 및 윈도우 생성 함수.
    [MenuItem("Tools/QuestMaker")]
    public static void ShowWindow()
    {
        GetWindow<QuestMakerTool>("Quest Maker").minSize = new Vector2(400, 700);
    }
    // 퀘스트용 NPC 프리팹 폴더에서 목록 가져오는 함수. 
    private void LoadNPCPrefabs()
    {
        // 경로 존재 확인 및 없다면 생성.
        if (!Directory.Exists(npcPrefabPath))
        {
            Directory.CreateDirectory(npcPrefabPath);
        }

        // 지정된 경로에서 조건에 맞는 에셋 가져오기.
        // AssetDatabase.FindAssets: 유니티 에디터 검색 함수.
        // 검색 결과는 해당 파읠의 ID값(문자열)으로 반환.
        // t:Prefab: 프리팹 형태의 데이터만 검색.
        string[] datas = AssetDatabase.FindAssets("t:Prefab", new[] { npcPrefabPath });
        // 찾은 데이터 수만큼의 크기로 배열 초기화.
        npcPrefabs = new GameObject[datas.Length + 1];
        npcNames = new string[datas.Length + 1];
        // 빈 상태 추가
        npcNames[0] = "None";
        npcPrefabs[0] = null;

        // 출력할 데이터 배열에 저장.
        for (int i = 0; i < datas.Length; i++)
        {
            // 찾은 데이터 ID를 경로 값으로 변환.
            string path = AssetDatabase.GUIDToAssetPath(datas[i]);
            // 경로에 맞는 프리팹 저장.
            npcPrefabs[i + 1] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            // 메뉴에 출력할 문자열 배열에 해당 프리팹 이름 저장.
            npcNames[i + 1] = npcPrefabs[i + 1].name;
        }
    }
    // 메모리에 임시 데이터 생성 함수.
    private void SetTempData()
    {
        // 메모리에 각 데이터 SO 생성.
        tempDialogue = ScriptableObject.CreateInstance<QuestDialogueData>();
        tempReward = ScriptableObject.CreateInstance<QuestRewardData>();
        // 메모리에 생성해둔 데이터를 SO 변수에 저장.
        dialogueSO = new SerializedObject(tempDialogue);
        rewardSO = new SerializedObject(tempReward);
    }
    // 설정 돼있는 퀘스트 데이터들을 메모리가 아닌 실제 데이터로 생성하는 함수.
    private void SetQuestAssets()
    {
        // 데이터 경로 설정.
        string type = questType.ToString();
        string questPath = SetPath("QuestDatas", type);
        string dialoguePath = SetPath("QuestDialogueDatas", type);
        string rewardPath = SetPath("QuestRewardDatas", type);

        // 대사 에셋 생성.
        AssetDatabase.CreateAsset(tempDialogue, $"{dialoguePath}/{questId}_{questName}_Dialogue.asset");
        // 보상 에셋 생성.
        AssetDatabase.CreateAsset(tempReward, $"{rewardPath}/{questId}_{questName}_Reward.asset");

        // 퀘스트 데이터 생성.
        // 우선 메모리에 SO 추가
        QuestData newQuest = ScriptableObject.CreateInstance<QuestData>();
        SerializedObject questSO = new SerializedObject(newQuest);
        // 메모리에 추가 돼있는 SO에 내용 저장.
        questSO.FindProperty("questId").intValue = questId;
        questSO.FindProperty("questName").stringValue = questName;
        questSO.FindProperty("questType").enumValueIndex = (int)questType;
        questSO.FindProperty("arrivePosition").vector3Value = arrivePosition;
        questSO.FindProperty("spawnPosition").vector3Value = spawnPosition;
        questSO.FindProperty("npcPrefab").objectReferenceValue = npcPrefabs[selectedNPCIndex];
        questSO.FindProperty("dialogueData").objectReferenceValue = tempDialogue;
        questSO.FindProperty("rewardData").objectReferenceValue = tempReward;
        questSO.ApplyModifiedProperties();
        // 실제 SO 파일 생성.
        AssetDatabase.CreateAsset(newQuest, $"{questPath}/{questId}_{questName}_Quest.asset");
        AssetDatabase.SaveAssets();
        // 생성된 파일 하이라이트 표시용 (여러 개(퀘스트, 대사, 보상) 생성 시 하나만 강조되기에 별로인 것 같기도)
        EditorGUIUtility.PingObject(newQuest);
        Selection.activeObject = newQuest;

        // 메모리 초기화.
        SetTempData();
    }
    // 경로 설정 함수.
    // 매개 변수로 전달 받은 문자열을 토대로 경로가 있는지 확인하고, 없다면 해당 경로를 생성 후 경로 반환.
    private string SetPath(string category, string type)
    {
        string path = $"Assets/Resources/GameData/Quests/{category}/{type}";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
}