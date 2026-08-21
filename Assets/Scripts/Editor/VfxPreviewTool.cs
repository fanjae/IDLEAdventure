using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// VFX 프리팹을 캐릭터와 함께 확인하는 프리뷰 에디터
public sealed class VfxPreviewTool : EditorWindow
{
    private const string DefaultCharacterPath = "Assets/Prefabs/Player/Hero_Tanker.prefab";
    private const string VfxSearchRoot = "Assets/Images/IdleAdventureAssets/VFX";
    private const string VfxCopyRoot = "Assets/Prefabs/VFX";
    private const float MinCameraDistance = 1f;
    private const float MaxCameraDistance = 30f;

    private readonly List<GameObject> vfxPrefabs = new();
    private readonly List<string> vfxFilterNames = new();
    private readonly List<string> vfxFilterPaths = new();

    private PreviewRenderUtility previewUtility;
    private GameObject vfxPrefab;
    private GameObject characterPrefab;
    private GameObject previewVfx;
    private GameObject previewCharacter;
    private Vector2 previewRotation = new(15f, -25f);
    private Vector2 listScrollPosition;
    private float cameraDistance = 6f;
    private float previewTime;
    private float previewDuration = 1f;
    private double lastEditorTime;
    private bool isPlaying;
    private bool isPaused;
    private bool loopPlayback = true;
    private int selectedFilterIndex;
    private string searchKeyword = string.Empty;

    // VFX 프리뷰 창 열기
    [MenuItem("Tools/VFX/VFX Preview")]
    public static void OpenWindow()
    {
        GetWindow<VfxPreviewTool>("VFX Preview");
    }

    private void OnEnable()
    {
        characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultCharacterPath);
        CreatePreviewUtility();
        RefreshVfxFilters();
        RefreshVfxList();
        EditorApplication.update += OnEditorUpdate;
        lastEditorTime = EditorApplication.timeSinceStartup;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        CleanupPreview();
    }

    // 에디터 프레임 기준으로 VFX 시뮬레이션 진행
    private void OnEditorUpdate()
    {
        double currentTime = EditorApplication.timeSinceStartup;
        float deltaTime = (float)(currentTime - lastEditorTime);
        lastEditorTime = currentTime;

        if (!isPlaying || isPaused || previewVfx == null || deltaTime <= 0f)
        {
            return;
        }

        previewTime += deltaTime;

        if (previewTime >= previewDuration)
        {
            if (loopPlayback)
            {
                previewTime %= previewDuration;
                SimulateVfxAtTime(previewTime);
            }
            else
            {
                previewTime = previewDuration;
                SimulateVfxAtTime(previewTime);
                isPlaying = false;
            }
        }
        else
        {
            SimulateVfx(deltaTime);
        }

        Repaint();
    }

    // VFX 프리뷰 에디터 UI 구성
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
        DrawVfxList();
        EditorGUILayout.EndVertical();

        GUILayout.Space(8f);

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DrawSettings();
        EditorGUILayout.Space(6f);
        DrawPlaybackControls();
        EditorGUILayout.Space(6f);
        DrawPathAndCopySection();
        EditorGUILayout.Space(6f);
        DrawPreviewArea();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    // 프리뷰 대상 프리팹 설정
    private void DrawSettings()
    {
        EditorGUILayout.LabelField("VFX 프리뷰", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        GameObject newVfxPrefab = (GameObject)EditorGUILayout.ObjectField("VFX Prefab", vfxPrefab, typeof(GameObject), false);
        GameObject newCharacterPrefab = (GameObject)EditorGUILayout.ObjectField("Character Prefab", characterPrefab, typeof(GameObject), false);

        if (EditorGUI.EndChangeCheck())
        {
            vfxPrefab = newVfxPrefab;
            characterPrefab = newCharacterPrefab;
            RebuildPreview();
        }

        EditorGUILayout.HelpBox("캐릭터와 VFX의 기준 위치를 동일하게 두어 실제 이펙트 크기를 비교합니다. 마우스 드래그로 회전하고 휠로 확대 또는 축소할 수 있습니다.", MessageType.Info);
    }

    // 재생 제어 버튼 표시
    private void DrawPlaybackControls()
    {
        using (new EditorGUI.DisabledScope(vfxPrefab == null))
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("재생"))
            {
                PlayPreview();
            }

            if (GUILayout.Button("일시정지"))
            {
                PausePreview();
            }

            if (GUILayout.Button("정지"))
            {
                StopPreview();
            }

            if (GUILayout.Button("시점 초기화"))
            {
                ResetView();
            }

            EditorGUILayout.EndHorizontal();
        }

        string state = isPlaying ? (isPaused ? "일시정지" : "재생 중") : "정지";
        EditorGUILayout.LabelField("상태", state);

        using (new EditorGUI.DisabledScope(vfxPrefab == null))
        {
            EditorGUI.BeginChangeCheck();
            float newPreviewTime = EditorGUILayout.Slider("재생 시점", previewTime, 0f, previewDuration);

            if (EditorGUI.EndChangeCheck())
            {
                previewTime = newPreviewTime;
                SimulateVfxAtTime(previewTime);
                Repaint();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{previewTime:0.00} / {previewDuration:0.00} sec");
            loopPlayback = EditorGUILayout.ToggleLeft("반복 재생", loopPlayback, GUILayout.Width(90f));
            EditorGUILayout.EndHorizontal();
        }
    }

    // 선택한 VFX의 경로 확인 및 복사 기능 표시
    private void DrawPathAndCopySection()
    {
        EditorGUILayout.LabelField("VFX 정보", EditorStyles.boldLabel);

        string assetPath = GetSelectedVfxPath();

        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(assetPath)))
        {
            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(assetPath) ? "선택된 VFX가 없습니다." : assetPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("PATH 복사"))
            {
                EditorGUIUtility.systemCopyBuffer = assetPath;
                Debug.Log($"[VfxPreviewTool] VFX PATH 복사 - {assetPath}");
            }

            if (GUILayout.Button("VFX 프리팹 복사"))
            {
                CopySelectedVfxPrefab();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    // 프로젝트 내 VFX 프리팹 목록 표시
    private void DrawVfxList()
    {
        EditorGUILayout.LabelField($"VFX 프리팹 목록 ({vfxPrefabs.Count})", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        selectedFilterIndex = EditorGUILayout.Popup("폴더 필터", selectedFilterIndex, vfxFilterNames.ToArray());

        if (EditorGUI.EndChangeCheck())
        {
            RefreshVfxList();
        }

        EditorGUILayout.BeginHorizontal();
        searchKeyword = EditorGUILayout.TextField("검색", searchKeyword);

        if (GUILayout.Button("새로고침", GUILayout.Width(80f)))
        {
            RefreshVfxFilters();
            RefreshVfxList();
        }

        EditorGUILayout.EndHorizontal();

        float listHeight = Mathf.Max(320f, position.height - 60f);
        listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition, GUILayout.Height(listHeight));

        foreach (GameObject prefab in vfxPrefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            string path = AssetDatabase.GetAssetPath(prefab);

            if (!string.IsNullOrEmpty(searchKeyword) && prefab.name.IndexOf(searchKeyword, System.StringComparison.OrdinalIgnoreCase) < 0 && path.IndexOf(searchKeyword, System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(prefab.name);

            if (GUILayout.Button("선택", GUILayout.Width(60f)))
            {
                SelectVfx(prefab);
            }

            if (GUILayout.Button("위치", GUILayout.Width(60f)))
            {
                EditorGUIUtility.PingObject(prefab);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    // 프리뷰 화면 표시
    private void DrawPreviewArea()
    {
        Rect previewRect = GUILayoutUtility.GetRect(100f, 10000f, 220f, 10000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (Event.current.type == EventType.Repaint)
        {
            RenderPreview(previewRect);
        }

        HandlePreviewInput(previewRect);
    }

    // 프리뷰 렌더링 유틸리티 초기화
    private void CreatePreviewUtility()
    {
        if (previewUtility != null)
        {
            return;
        }

        previewUtility = new PreviewRenderUtility();
        previewUtility.cameraFieldOfView = 30f;
        previewUtility.camera.nearClipPlane = 0.01f;
        previewUtility.camera.farClipPlane = 100f;
        previewUtility.lights[0].intensity = 1.2f;
        previewUtility.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
        previewUtility.lights[1].intensity = 0.8f;
        previewUtility.lights[1].transform.rotation = Quaternion.Euler(340f, 218f, 177f);
        previewUtility.ambientColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    }

    // 현재 설정으로 캐릭터와 VFX 프리뷰 다시 생성
    private void RebuildPreview()
    {
        CleanupPreviewObjects();
        CreatePreviewUtility();

        if (characterPrefab != null)
        {
            previewCharacter = previewUtility.InstantiatePrefabInScene(characterPrefab);
            SetPreviewObjectTransform(previewCharacter);
        }

        if (vfxPrefab != null)
        {
            previewVfx = previewUtility.InstantiatePrefabInScene(vfxPrefab);
            SetPreviewObjectTransform(previewVfx);
            previewDuration = CalculatePreviewDuration();
            previewTime = 0f;
            ResetVfxSimulation();
        }
        else
        {
            previewDuration = 1f;
            previewTime = 0f;
        }

        isPlaying = false;
        isPaused = false;
        lastEditorTime = EditorApplication.timeSinceStartup;
        Repaint();
    }

    // 프리뷰 오브젝트를 동일한 기준점에 배치
    private static void SetPreviewObjectTransform(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        target.transform.position = Vector3.zero;
        target.transform.rotation = Quaternion.identity;
    }

    // 선택한 VFX 재생 또는 일시정지 상태에서 재개
    private void PlayPreview()
    {
        if (previewVfx == null)
        {
            RebuildPreview();
        }

        if (previewVfx == null)
        {
            return;
        }

        if (previewTime >= previewDuration)
        {
            previewTime = 0f;
            SimulateVfxAtTime(previewTime);
        }

        ParticleSystem[] particleSystems = previewVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (isPaused)
            {
                particleSystem.Play(false);
            }
        }

        isPlaying = true;
        isPaused = false;
        lastEditorTime = EditorApplication.timeSinceStartup;
        Repaint();
    }

    // 모든 파티클 시스템을 실제 일시정지 상태로 전환
    private void PausePreview()
    {
        if (!isPlaying || previewVfx == null)
        {
            return;
        }

        ParticleSystem[] particleSystems = previewVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Pause(false);
        }

        isPaused = true;
        Repaint();
    }

    // VFX를 처음 상태로 되돌리고 정지
    private void StopPreview()
    {
        isPlaying = false;
        isPaused = false;
        previewTime = 0f;
        ResetVfxSimulation();
        Repaint();
    }

    // 지정한 시점으로 VFX 상태 이동
    private void SimulateVfxAtTime(float targetTime)
    {
        if (previewVfx == null)
        {
            return;
        }

        ParticleSystem[] particleSystems = previewVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Simulate(targetTime, false, true, true);

            if (isPaused)
            {
                particleSystem.Pause(false);
            }
        }
    }

    // 선택한 VFX의 전체 재생 길이 계산
    private float CalculatePreviewDuration()
    {
        if (previewVfx == null)
        {
            return 1f;
        }

        float duration = 0.1f;
        ParticleSystem[] particleSystems = previewVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            float systemDuration = main.duration + main.startDelay.constantMax + main.startLifetime.constantMax;
            duration = Mathf.Max(duration, systemDuration);
        }

        return duration;
    }

    // 모든 파티클 시스템을 한 프레임만큼 시뮬레이션
    private void SimulateVfx(float deltaTime)
    {
        ParticleSystem[] particleSystems = previewVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Simulate(deltaTime, false, false, true);
        }
    }

    // 모든 파티클 시스템을 초기 상태로 되돌림
    private void ResetVfxSimulation()
    {
        if (previewVfx == null)
        {
            return;
        }

        ParticleSystem[] particleSystems = previewVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Simulate(0f, false, true, true);
        }
    }

    // 현재 선택된 VFX의 Asset 경로 반환
    private string GetSelectedVfxPath()
    {
        return vfxPrefab == null ? string.Empty : AssetDatabase.GetAssetPath(vfxPrefab);
    }

    // 현재 선택된 VFX 프리팹을 프로젝트 Prefabs/VFX 경로에 복사
    private void CopySelectedVfxPrefab()
    {
        string sourcePath = GetSelectedVfxPath();

        if (string.IsNullOrEmpty(sourcePath))
        {
            return;
        }

        EnsureCopyFolderExists();

        string destinationPath = AssetDatabase.GenerateUniqueAssetPath($"{VfxCopyRoot}/{vfxPrefab.name}.prefab");

        if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
        {
            Debug.LogError($"[VfxPreviewTool] VFX 프리팹 복사 실패 - {sourcePath}");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GameObject copiedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(destinationPath);
        EditorGUIUtility.PingObject(copiedPrefab);
        Debug.Log($"[VfxPreviewTool] VFX 프리팹 복사 완료 - {destinationPath}");
    }

    // VFX 복사 폴더가 없으면 생성
    private static void EnsureCopyFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        if (!AssetDatabase.IsValidFolder(VfxCopyRoot))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "VFX");
        }
    }

    // VFX 검색 루트의 직속 하위 폴더를 필터 목록으로 갱신
    private void RefreshVfxFilters()
    {
        vfxFilterNames.Clear();
        vfxFilterPaths.Clear();

        vfxFilterNames.Add("전체");
        vfxFilterPaths.Add(VfxSearchRoot);

        if (!AssetDatabase.IsValidFolder(VfxSearchRoot))
        {
            selectedFilterIndex = 0;
            return;
        }

        string[] subFolders = AssetDatabase.GetSubFolders(VfxSearchRoot);

        foreach (string folderPath in subFolders)
        {
            int separatorIndex = folderPath.LastIndexOf('/');
            string folderName = separatorIndex >= 0 ? folderPath[(separatorIndex + 1)..] : folderPath;
            vfxFilterNames.Add(folderName);
            vfxFilterPaths.Add(folderPath);
        }

        selectedFilterIndex = Mathf.Clamp(selectedFilterIndex, 0, vfxFilterNames.Count - 1);
    }

    // 현재 폴더 필터를 기준으로 VFX 프리팹 목록 갱신
    private void RefreshVfxList()
    {
        vfxPrefabs.Clear();

        if (vfxFilterPaths.Count == 0)
        {
            RefreshVfxFilters();
        }

        if (vfxFilterPaths.Count == 0)
        {
            return;
        }

        selectedFilterIndex = Mathf.Clamp(selectedFilterIndex, 0, vfxFilterPaths.Count - 1);
        string searchRoot = vfxFilterPaths[selectedFilterIndex];

        if (!AssetDatabase.IsValidFolder(searchRoot))
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { searchRoot });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                vfxPrefabs.Add(prefab);
            }
        }

        vfxPrefabs.Sort((left, right) => string.Compare(left.name, right.name, System.StringComparison.OrdinalIgnoreCase));
        listScrollPosition = Vector2.zero;
        Repaint();
    }

    // 목록에서 선택한 VFX를 프리뷰 대상으로 지정하고 자동 재생
    private void SelectVfx(GameObject prefab)
    {
        vfxPrefab = prefab;
        RebuildPreview();
        PlayPreview();
    }

    // 현재 카메라 설정으로 프리뷰 렌더링
    private void RenderPreview(Rect previewRect)
    {
        if (previewUtility == null)
        {
            CreatePreviewUtility();
        }

        previewUtility.BeginPreview(previewRect, GUIStyle.none);

        Quaternion rotation = Quaternion.Euler(previewRotation.x, previewRotation.y, 0f);
        Vector3 targetPosition = new(0f, 1f, 0f);
        Vector3 cameraPosition = targetPosition + rotation * new Vector3(0f, 0f, -cameraDistance);

        previewUtility.camera.transform.position = cameraPosition;
        previewUtility.camera.transform.rotation = Quaternion.LookRotation(targetPosition - cameraPosition, Vector3.up);
        previewUtility.camera.Render();

        Texture previewTexture = previewUtility.EndPreview();
        GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill, false);
    }

    // 프리뷰 화면 마우스 입력 처리
    private void HandlePreviewInput(Rect previewRect)
    {
        Event currentEvent = Event.current;

        if (!previewRect.Contains(currentEvent.mousePosition))
        {
            return;
        }

        if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0)
        {
            previewRotation.y += currentEvent.delta.x * 0.5f;
            previewRotation.x = Mathf.Clamp(previewRotation.x - currentEvent.delta.y * 0.5f, -80f, 80f);
            currentEvent.Use();
            Repaint();
            return;
        }

        if (currentEvent.type == EventType.ScrollWheel)
        {
            cameraDistance = Mathf.Clamp(cameraDistance + currentEvent.delta.y * 0.25f, MinCameraDistance, MaxCameraDistance);
            currentEvent.Use();
            Repaint();
        }
    }

    // 프리뷰 카메라 시점 초기화
    private void ResetView()
    {
        previewRotation = new Vector2(15f, -25f);
        cameraDistance = 6f;
        Repaint();
    }

    // 프리뷰 오브젝트 정리
    private void CleanupPreviewObjects()
    {
        previewVfx = null;
        previewCharacter = null;

        if (previewUtility == null)
        {
            return;
        }

        previewUtility.Cleanup();
        previewUtility = null;
        CreatePreviewUtility();
    }

    // 프리뷰 리소스 전체 정리
    private void CleanupPreview()
    {
        previewVfx = null;
        previewCharacter = null;

        if (previewUtility == null)
        {
            return;
        }

        previewUtility.Cleanup();
        previewUtility = null;
    }
}
