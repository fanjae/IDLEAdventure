using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 퀘스트 생성 툴 클래스 <br/>
/// 기본적인 퀘스트 데이터 + NPC, 대사, 보상 데이터 설정 가능하도록 구상 중.
/// </summary>
public class QuestMakerTool : EditorWindow
{
    private int questId;
    private string questName;
    private QuestType questType;

    private QuestKind questKind;
    private QuestKind lastQuestKind;

    private Vector3 arrivePosition;
    private Vector3 spawnPosition;

    private enum PositionPickMode { None, Arrive, Spawn }
    private PositionPickMode currentPickMode = PositionPickMode.None;

    private int targetCount = 1;
    private float spawnRadius = 0.0f;
    private float spawnDistance = 0.0f;

    private GameObject[] targetPrefabs;
    private string[] targetNames;
    private int selectedTargetIndex;
    private readonly string npcFolderPath = "Assets/Resources/GameData/Quests/QuestInteractablePrefabs/NPCs";
    private readonly string enemyFolderPath = "Assets/Resources/GameData/Quests/QuestInteractablePrefabs/Enemies";
    private readonly string gatherFolderPath = "Assets/Resources/GameData/Quests/QuestInteractablePrefabs/Gatherables";

    private QuestDialogueData tempDialogue;
    private QuestRewardData tempReward;
    private SerializedObject dialogueSO;
    private SerializedObject rewardSO;

    private Vector2 scrollPosition;

    private void OnEnable()
    {
        SetTempData();
        lastQuestKind = questKind;
        LoadTargetPrefabs(questKind);

        SceneView.duringSceneGui += OnSceneGUI;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
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
        questKind = (QuestKind)EditorGUILayout.EnumPopup("Kind", questKind);

        if (questKind != lastQuestKind)
        {
            LoadTargetPrefabs(questKind);
            selectedTargetIndex = 0;
            lastQuestKind = questKind;
        }

        GUILayout.Space(10);

        // Arrive Position 입력 UI 그리기.
        GUILayout.Label("Target Data", EditorStyles.boldLabel);
        arrivePosition = EditorGUILayout.Vector3Field("Arrive Position", arrivePosition);
        GUI.backgroundColor = (currentPickMode == PositionPickMode.Arrive) ? Color.green : Color.white;
        if (GUILayout.Button(currentPickMode == PositionPickMode.Arrive ? "씬 뷰에서 Arrive 위치를 클릭하세요." : "씬 뷰에서 Arrive 위치 지정."))
        {
            currentPickMode = (currentPickMode == PositionPickMode.Arrive) ? PositionPickMode.None : PositionPickMode.Arrive;
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);

        // Spawn Position 입력 UI 그리기.
        spawnPosition = EditorGUILayout.Vector3Field("Spawn Position", spawnPosition);
        GUI.backgroundColor = (currentPickMode == PositionPickMode.Spawn) ? Color.green : Color.white;
        if (GUILayout.Button(currentPickMode == PositionPickMode.Spawn ? "씬 뷰에서 Spawn 위치를 클릭하세요." : "씬 뷰에서 Spawn 위치 지정."))
        {
            currentPickMode = (currentPickMode == PositionPickMode.Spawn) ? PositionPickMode.None : PositionPickMode.Spawn;
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        // 저장해둔 NPC 리스트 선택 UI 그리기.
        selectedTargetIndex = EditorGUILayout.Popup("Target Prefab", selectedTargetIndex, targetNames);

        if (questKind == QuestKind.Gather)
        {
            GUILayout.Space(10);
            GUILayout.Label("Gathering Settings", EditorStyles.boldLabel);
            targetCount = EditorGUILayout.IntField("Target Count", targetCount);
            spawnRadius = EditorGUILayout.FloatField("Spawn Radius", spawnRadius);
            spawnDistance = EditorGUILayout.FloatField("Min Distance", spawnDistance);
        }
        else
        {
            targetCount = 1;
            spawnRadius = 0.0f;
            spawnDistance = 0.0f;
        }

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
    // 씬 뷰를 통해 위치 값을 받아오기 위한 함수.
    private void OnSceneGUI(SceneView sceneView)
    {
        // 씬 뷰에서 발생하는 이벤트 저장.
        Event e = Event.current;

        // PositionPickMode일 때
        if (currentPickMode != PositionPickMode.None)
        {
            // 씬 뷰에서 발생한 이벤트가 마우스 좌클릭 입력 이벤트일 때
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // 마우스 위치를 월드 좌표로 한 ray로 변환 및 충돌 체크.
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    // 충돌 좌표 저장.
                    Vector3 targetPoint = hit.point;
                    Vector3 origin = new Vector3(targetPoint.x, targetPoint.y + 100.0f, targetPoint.z);
                    // 충돌할 레이어 마스크 설정.
                    int terrainLayerMask = LayerMask.GetMask("Terrain");
                    // x, z 좌표값으로 찾은 좌표의 수직 상공에서 레이를 쏴 카메라 뷰 등 보이는 각도와 상관 없이 바닥 체크.
                    if (Physics.Raycast(origin, Vector3.down, out RaycastHit groundHit, 200.0f, terrainLayerMask))
                    {
                        targetPoint = groundHit.point;
                    }
                    // 좌표의 정확도를 위한 보정.
                    targetPoint.x = Mathf.Round(targetPoint.x * 100.0f) / 100.0f;
                    targetPoint.y = Mathf.Round(targetPoint.y * 100.0f) / 100.0f;
                    targetPoint.z = Mathf.Round(targetPoint.z * 100.0f) / 100.0f;
                    // y 좌표값이 0에 충분히 가깝다면 정확히 0.0f로 맞춰주기.
                    if (Mathf.Abs(targetPoint.y) < 0.01f)
                    {
                        targetPoint.y = 0.0f;
                    }

                    // 설정해둔 좌표값에 계산된 좌표값 저장.
                    if (currentPickMode == PositionPickMode.Arrive)
                    {
                        Undo.RecordObject(this, "Set Arrive Position");
                        arrivePosition = targetPoint;
                    }
                    else if (currentPickMode == PositionPickMode.Spawn)
                    {
                        Undo.RecordObject(this, "Set Spawn Position");
                        spawnPosition = targetPoint;
                    }

                    // 모드 초기화.
                    currentPickMode = PositionPickMode.None;
                    // 이벤트 사용 처리. (초기화)
                    e.Use();
                    // 에디터 갱신.
                    Repaint();
                }
            }

            // 마우스 키 입력 받는 이벤트가 활성화된 도중 Esc, 마우스 우클릭 이벤트가 발생하면 중단.
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape || e.type == EventType.MouseDown && e.button == 1)
            {
                // 모드 초기화.
                currentPickMode = PositionPickMode.None;
                // 이벤트 사용 처리. (초기화)
                e.Use();
                // 에디터 갱신.
                Repaint();
            }
        }

        // 수정사항 체크 시작.
        EditorGUI.BeginChangeCheck();
        // 스폰 위치 표시 객체에 Position 핸들 생성.
        Vector3 newArrivePos = Handles.PositionHandle(arrivePosition, Quaternion.identity);
        // 수정사항 체크 종료.
        // 수정된 위치 값을 보정해 저장.
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(this, "Move Arrive Position via Handle");
            arrivePosition = new Vector3(
                Mathf.Round(newArrivePos.x * 100.0f) / 100.0f,
                Mathf.Round(newArrivePos.y * 100.0f) / 100.0f,
                Mathf.Round(newArrivePos.z * 100.0f) / 100.0f
            );
            Repaint();
        }
        // 도착 위치에 구체 생성.
        Handles.color = Color.cyan;
        Handles.SphereHandleCap(0, arrivePosition, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.Label(arrivePosition + Vector3.up * 0.7f, "Arrive Pos");

        // 수정사항 체크 시작.
        EditorGUI.BeginChangeCheck();
        // 스폰 위치 표시 객체에 Position 핸들 생성.
        Vector3 newSpawnPos = Handles.PositionHandle(spawnPosition, Quaternion.identity);
        // 수정사항 체크 종료.
        // 수정된 위치 값을 보정해 저장.
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(this, "Move Spawn Position via Handle");
            spawnPosition = new Vector3(
                Mathf.Round(newSpawnPos.x * 100.0f) / 100.0f,
                Mathf.Round(newSpawnPos.y * 100.0f) / 100.0f,
                Mathf.Round(newSpawnPos.z * 100.0f) / 100.0f
            );
            Repaint();
        }
        // 스폰 위치에 구체 생성.
        Handles.color = Color.green;
        Handles.SphereHandleCap(0, spawnPosition, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.Label(spawnPosition + Vector3.up * 0.7f, "Spawn Pos");

        // 채집 퀘스트라면 반경 표시.
        if (questKind == QuestKind.Gather)
        {
            Handles.color = new Color(0.0f, 1.0f, 0.5f, 0.3f);
            Handles.DrawWireDisc(spawnPosition, Vector3.up, spawnRadius);

            EditorGUI.BeginChangeCheck();
            float newRadius = Handles.RadiusHandle(Quaternion.identity, spawnPosition, spawnRadius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Change Spawn Radius via Handle");
                spawnRadius = newRadius;
                Repaint();
            }
        }
    }

    [MenuItem("Tools/QuestMaker")]
    public static void ShowWindow()
    {
        GetWindow<QuestMakerTool>("Quest Maker").minSize = new Vector2(400, 700);
    }
    // 퀘스트용 NPC 프리팹 폴더에서 목록 가져오는 함수. 
    private void LoadTargetPrefabs(QuestKind kind)
    {
        if (kind == QuestKind.None || kind == QuestKind.Length)
        {
            targetPrefabs = new GameObject[1] { null };
            targetNames = new string[1] { "None" };
            return;
        }

        string currentFolderPath = "";

        switch (kind)
        {
            case QuestKind.Talk: currentFolderPath = npcFolderPath; break;
            case QuestKind.Fight: currentFolderPath = enemyFolderPath; break;
            case QuestKind.Gather: currentFolderPath = gatherFolderPath; break;
        }


        // 경로 존재 확인 및 없다면 생성.
        if (!Directory.Exists(currentFolderPath))
        {
            Directory.CreateDirectory(currentFolderPath);
        }

        // 지정된 경로에서 조건에 맞는 에셋 가져오기.
        // AssetDatabase.FindAssets: 유니티 에디터 검색 함수.
        // 검색 결과는 해당 파읠의 ID값(문자열)으로 반환.
        // t:Prefab: 프리팹 형태의 데이터만 검색.
        string[] datas = AssetDatabase.FindAssets("t:Prefab", new[] { currentFolderPath });
        // 찾은 데이터 수만큼의 크기로 배열 초기화.
        targetPrefabs = new GameObject[datas.Length + 1];
        targetNames = new string[datas.Length + 1];
        // 빈 상태 추가
        targetNames[0] = "None";
        targetPrefabs[0] = null;

        // 출력할 데이터 배열에 저장.
        for (int i = 0; i < datas.Length; i++)
        {
            // 찾은 데이터 ID를 경로 값으로 변환.
            string path = AssetDatabase.GUIDToAssetPath(datas[i]);
            // 경로에 맞는 프리팹 저장.
            targetPrefabs[i + 1] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            // 메뉴에 출력할 문자열 배열에 해당 프리팹 이름 저장.
            targetNames[i + 1] = targetPrefabs[i + 1].name;
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
        questSO.FindProperty("questKind").enumValueIndex = (int)questKind;
        questSO.FindProperty("arrivePosition").vector3Value = arrivePosition;
        questSO.FindProperty("spawnPosition").vector3Value = spawnPosition;
        questSO.FindProperty("interactablePrefab").objectReferenceValue = targetPrefabs[selectedTargetIndex];
        questSO.FindProperty("dialogueData").objectReferenceValue = tempDialogue;
        questSO.FindProperty("targetCount").intValue = targetCount;
        questSO.FindProperty("spawnRadius").floatValue = spawnRadius;
        questSO.FindProperty("spawnDistance").floatValue = spawnDistance;
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