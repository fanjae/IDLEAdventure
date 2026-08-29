using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// SFX AudioClip을 검색하고 에디터에서 바로 확인하는 프리뷰 툴
public sealed class SfxPreviewTool : EditorWindow
{
    private const string DefaultSfxSearchRoot = "Assets/Images/IdleAdventureAssets/SFX";
    private const string DefaultSfxCopyRoot = "Assets/Prefabs/SFX";
    private const float ListRowHeight = 24f;
    private const float MinVolume = 0f;
    private const float MaxVolume = 1f;
    private const float MinPitch = 0.1f;
    private const float MaxPitch = 3f;

    private readonly List<AudioClip> sfxClips = new();
    private readonly List<AudioClip> filteredClips = new();

    // sfxClips와 동일한 인덱스로 Asset 경로를 저장하여 검색 시 AssetDatabase 조회를 줄임
    private readonly List<string> clipPaths = new();
    private readonly List<string> sfxFilterNames = new();
    private readonly List<string> sfxFilterPaths = new();

    private AudioClip selectedClip;
    private Vector2 listScrollPosition;
    private string searchKeyword = string.Empty;
    private string appliedSearchKeyword = string.Empty;
    private float volume = 1f;
    private float pitch = 1f;
    private bool loopPlayback;
    private bool isPlaying;
    private int selectedFilterIndex;
    private string sfxSearchRoot = DefaultSfxSearchRoot;
    private string sfxCopyRoot = DefaultSfxCopyRoot;

    // UnityEditor.AudioUtil은 공개 API가 아니므로 에디터 프리뷰 재생을 위해 Reflection으로 접근
    private static Type audioUtilType;
    private static MethodInfo playPreviewClipMethod;
    private static MethodInfo stopAllPreviewClipsMethod;
    private static MethodInfo isPreviewClipPlayingMethod;
    private static PropertyInfo previewVolumeProperty;

    // SFX 프리뷰 창 열기
    [MenuItem("Tools/SFX/SFX Preview")]
    public static void OpenWindow()
    {
        GetWindow<SfxPreviewTool>("SFX Preview");
    }

    private void OnEnable()
    {
        InitializeAudioUtilReflection();
        RefreshSfxFilters();
        RefreshSfxList();
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        StopPreview();
    }

    // 에디터 오디오 재생 상태 확인
    private void OnEditorUpdate()
    {
        if (!isPlaying)
        {
            return;
        }

        bool previewPlaying = IsPreviewClipPlaying();

        if (previewPlaying)
        {
            return;
        }

        if (loopPlayback && selectedClip != null)
        {
            PlayPreview();
            return;
        }

        isPlaying = false;
        Repaint();
    }

    // SFX 프리뷰 에디터 UI 구성
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
        DrawSfxPathSettings();
        EditorGUILayout.Space(6f);
        DrawSfxList();
        EditorGUILayout.EndVertical();

        GUILayout.Space(8f);

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DrawSettings();
        EditorGUILayout.Space(6f);
        DrawPlaybackControls();
        EditorGUILayout.Space(6f);
        DrawPathAndCopySection();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    // 프리뷰 대상 SFX 설정
    private void DrawSettings()
    {
        EditorGUILayout.LabelField("SFX 프리뷰", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        AudioClip newSelectedClip = (AudioClip)EditorGUILayout.ObjectField("SFX Clip", selectedClip, typeof(AudioClip), false);

        if (EditorGUI.EndChangeCheck())
        {
            StopPreview();
            selectedClip = newSelectedClip;
        }

        EditorGUILayout.HelpBox("SFX 목록에서 선택하거나 AudioClip을 직접 지정해 미리 들을 수 있습니다. 검색 범위는 왼쪽 폴더 필터에서 선택합니다.", MessageType.Info);
    }

    // 선택한 SFX의 재생 설정 및 제어 버튼 표시
    private void DrawPlaybackControls()
    {
        EditorGUILayout.LabelField("재생 설정", EditorStyles.boldLabel);

        volume = EditorGUILayout.Slider("볼륨", volume, MinVolume, MaxVolume);
        pitch = EditorGUILayout.Slider("피치", pitch, MinPitch, MaxPitch);
        loopPlayback = EditorGUILayout.Toggle("반복 재생", loopPlayback);

        using (new EditorGUI.DisabledScope(selectedClip == null))
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("재생"))
            {
                PlayPreview();
            }

            if (GUILayout.Button("정지"))
            {
                StopPreview();
            }

            EditorGUILayout.EndHorizontal();
        }

        string state = isPlaying ? "재생 중" : "정지";
        EditorGUILayout.LabelField("상태", state);

        if (selectedClip != null)
        {
            EditorGUILayout.LabelField("길이", $"{selectedClip.length:0.00} sec");
            EditorGUILayout.LabelField("채널", selectedClip.channels.ToString());
            EditorGUILayout.LabelField("샘플레이트", $"{selectedClip.frequency} Hz");
        }
    }

    // 선택한 SFX의 경로 확인 및 복사 기능 표시
    private void DrawPathAndCopySection()
    {
        EditorGUILayout.LabelField("SFX 정보", EditorStyles.boldLabel);

        string assetPath = GetSelectedSfxPath();

        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(assetPath)))
        {
            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(assetPath) ? "선택된 SFX가 없습니다." : assetPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("PATH 복사"))
            {
                EditorGUIUtility.systemCopyBuffer = assetPath;
                Debug.Log($"[SfxPreviewTool] SFX PATH 복사 - {assetPath}");
            }

            if (GUILayout.Button("SFX 복사"))
            {
                CopySelectedSfx();
            }

            EditorGUILayout.EndHorizontal();
        }
    }


    // SFX 검색 및 복사 경로 설정 표시
    private void DrawSfxPathSettings()
    {
        EditorGUILayout.LabelField("SFX 경로 설정", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        string newSearchRoot = EditorGUILayout.DelayedTextField("검색 경로", sfxSearchRoot);

        if (GUILayout.Button("폴더 선택", GUILayout.Width(80f)))
        {
            string selectedPath = SelectProjectFolder(sfxSearchRoot);

            if (!string.IsNullOrEmpty(selectedPath))
            {
                newSearchRoot = selectedPath;
            }
        }

        EditorGUILayout.EndHorizontal();

        if (newSearchRoot != sfxSearchRoot)
        {
            sfxSearchRoot = NormalizeAssetPath(newSearchRoot);
            selectedFilterIndex = 0;
            RefreshSfxFilters();
            RefreshSfxList();
        }

        EditorGUILayout.BeginHorizontal();
        string newCopyRoot = EditorGUILayout.DelayedTextField("복사 경로", sfxCopyRoot);

        if (GUILayout.Button("폴더 선택", GUILayout.Width(80f)))
        {
            string selectedPath = SelectProjectFolder(sfxCopyRoot);

            if (!string.IsNullOrEmpty(selectedPath))
            {
                newCopyRoot = selectedPath;
            }
        }

        EditorGUILayout.EndHorizontal();
        sfxCopyRoot = NormalizeAssetPath(newCopyRoot);

        if (!AssetDatabase.IsValidFolder(sfxSearchRoot))
        {
            EditorGUILayout.HelpBox("검색 경로는 프로젝트 Assets 폴더 내부의 유효한 폴더를 지정해야 합니다.", MessageType.Warning);
        }

        if (string.IsNullOrEmpty(sfxCopyRoot) || !sfxCopyRoot.StartsWith("Assets", StringComparison.Ordinal))
        {
            EditorGUILayout.HelpBox("복사 경로는 프로젝트 Assets 폴더 내부 경로를 지정해야 합니다.", MessageType.Warning);
        }
    }
    // 프로젝트 내 SFX 목록 표시
    private void DrawSfxList()
    {
        EditorGUILayout.LabelField($"SFX 목록 ({filteredClips.Count} / {sfxClips.Count})", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        selectedFilterIndex = EditorGUILayout.Popup("폴더 필터", selectedFilterIndex, sfxFilterNames.ToArray());

        if (EditorGUI.EndChangeCheck())
        {
            RefreshSfxList();
        }

        EditorGUILayout.BeginHorizontal();
        searchKeyword = EditorGUILayout.TextField("검색", searchKeyword);

        if (!string.Equals(searchKeyword, appliedSearchKeyword, StringComparison.Ordinal))
        {
            ApplySearchFilter();
        }

        if (GUILayout.Button("새로고침", GUILayout.Width(80f)))
        {
            RefreshSfxFilters();
            RefreshSfxList();
        }

        EditorGUILayout.EndHorizontal();

        // 대량의 SFX를 모두 그리지 않고 현재 스크롤 영역에 필요한 행만 렌더링
        float listHeight = Mathf.Max(320f, position.height - 60f);
        float contentHeight = filteredClips.Count * ListRowHeight;
        Rect viewportRect = GUILayoutUtility.GetRect(0f, 10000f, listHeight, listHeight, GUILayout.ExpandWidth(true));
        Rect contentRect = new(0f, 0f, Mathf.Max(0f, viewportRect.width - 16f), contentHeight);

        listScrollPosition = GUI.BeginScrollView(viewportRect, listScrollPosition, contentRect);

        if (filteredClips.Count > 0)
        {
            int firstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(listScrollPosition.y / ListRowHeight));

            // 스크롤 경계에서 행이 비어 보이지 않도록 화면 높이보다 여유 있게 행을 계산
            int visibleRowCount = Mathf.CeilToInt(viewportRect.height / ListRowHeight) + 2;
            int lastVisibleIndex = Mathf.Min(filteredClips.Count - 1, firstVisibleIndex + visibleRowCount);

            for (int index = firstVisibleIndex; index <= lastVisibleIndex; index++)
            {
                DrawSfxRow(index, contentRect.width);
            }
        }

        GUI.EndScrollView();
    }

    // 현재 스크롤 영역에 보이는 SFX 행 하나만 렌더링
    private void DrawSfxRow(int index, float rowWidth)
    {
        AudioClip clip = filteredClips[index];

        if (clip == null)
        {
            return;
        }

        Rect rowRect = new(0f, index * ListRowHeight, rowWidth, ListRowHeight);
        Rect boxRect = new(rowRect.x, rowRect.y + 1f, rowRect.width, rowRect.height - 2f);
        GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

        float buttonWidth = 52f;
        float spacing = 4f;
        Rect locateButtonRect = new(rowRect.xMax - buttonWidth, rowRect.y + 2f, buttonWidth, rowRect.height - 4f);
        Rect selectButtonRect = new(locateButtonRect.x - buttonWidth - spacing, rowRect.y + 2f, buttonWidth, rowRect.height - 4f);
        Rect labelRect = new(rowRect.x + 6f, rowRect.y + 3f, Mathf.Max(0f, selectButtonRect.x - rowRect.x - 12f), rowRect.height - 6f);

        GUI.Label(labelRect, clip.name);

        if (GUI.Button(selectButtonRect, "선택"))
        {
            SelectSfx(clip);
        }

        if (GUI.Button(locateButtonRect, "위치"))
        {
            EditorGUIUtility.PingObject(clip);
        }
    }

    // SFX 검색 루트의 직속 하위 폴더를 필터 목록으로 갱신
    private void RefreshSfxFilters()
    {
        sfxFilterNames.Clear();
        sfxFilterPaths.Clear();

        sfxFilterNames.Add("전체");
        sfxFilterPaths.Add(sfxSearchRoot);

        if (!AssetDatabase.IsValidFolder(sfxSearchRoot))
        {
            selectedFilterIndex = 0;
            return;
        }

        string[] subFolders = AssetDatabase.GetSubFolders(sfxSearchRoot);

        foreach (string folderPath in subFolders)
        {
            int separatorIndex = folderPath.LastIndexOf('/');
            string folderName = separatorIndex >= 0 ? folderPath[(separatorIndex + 1)..] : folderPath;
            sfxFilterNames.Add(folderName);
            sfxFilterPaths.Add(folderPath);
        }

        selectedFilterIndex = Mathf.Clamp(selectedFilterIndex, 0, sfxFilterNames.Count - 1);
    }

    // 현재 폴더 필터를 기준으로 AudioClip 목록 갱신
    private void RefreshSfxList()
    {
        StopPreview();
        sfxClips.Clear();
        clipPaths.Clear();

        if (sfxFilterPaths.Count == 0)
        {
            RefreshSfxFilters();
        }

        if (sfxFilterPaths.Count == 0)
        {
            filteredClips.Clear();
            selectedClip = null;
            Repaint();
            return;
        }

        selectedFilterIndex = Mathf.Clamp(selectedFilterIndex, 0, sfxFilterPaths.Count - 1);
        string searchRoot = sfxFilterPaths[selectedFilterIndex];

        if (!AssetDatabase.IsValidFolder(searchRoot))
        {
            filteredClips.Clear();
            Repaint();
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { searchRoot });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            if (clip == null)
            {
                continue;
            }

            sfxClips.Add(clip);
            clipPaths.Add(path);
        }

        SortSfxList();
        ApplySearchFilter();
        listScrollPosition = Vector2.zero;
        Repaint();
    }

    // 이름 기준 정렬 시 경로 캐시도 동일한 순서로 유지
    private void SortSfxList()
    {
        List<int> indices = new(sfxClips.Count);

        for (int index = 0; index < sfxClips.Count; index++)
        {
            indices.Add(index);
        }

        indices.Sort((left, right) => string.Compare(sfxClips[left].name, sfxClips[right].name, StringComparison.OrdinalIgnoreCase));

        List<AudioClip> sortedClips = new(sfxClips.Count);
        List<string> sortedPaths = new(clipPaths.Count);

        foreach (int index in indices)
        {
            sortedClips.Add(sfxClips[index]);
            sortedPaths.Add(clipPaths[index]);
        }

        sfxClips.Clear();
        sfxClips.AddRange(sortedClips);
        clipPaths.Clear();
        clipPaths.AddRange(sortedPaths);
    }

    // 검색어 변경 시 전체 AssetDatabase 재검색 없이 캐시된 목록만 필터링
    private void ApplySearchFilter()
    {
        appliedSearchKeyword = searchKeyword;
        filteredClips.Clear();

        if (string.IsNullOrWhiteSpace(searchKeyword))
        {
            filteredClips.AddRange(sfxClips);
            listScrollPosition.y = 0f;
            return;
        }

        for (int index = 0; index < sfxClips.Count; index++)
        {
            AudioClip clip = sfxClips[index];

            if (clip == null)
            {
                continue;
            }

            // 검색할 때마다 AssetDatabase를 조회하지 않고 미리 저장한 경로 캐시를 우선 사용
            string path = index < clipPaths.Count ? clipPaths[index] : AssetDatabase.GetAssetPath(clip);

            if (clip.name.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                filteredClips.Add(clip);
            }
        }

        listScrollPosition.y = 0f;
    }

    // 목록에서 선택한 SFX를 프리뷰 대상으로 지정하고 자동 재생
    private void SelectSfx(AudioClip clip)
    {
        selectedClip = clip;
        PlayPreview();
        Repaint();
    }

    // 선택한 SFX를 현재 볼륨과 피치 설정으로 재생
    private void PlayPreview()
    {
        if (selectedClip == null)
        {
            return;
        }

        StopPreview();
        InitializeAudioUtilReflection();

        if (playPreviewClipMethod == null)
        {
            Debug.LogError("[SfxPreviewTool] UnityEditor.AudioUtil의 프리뷰 재생 API를 찾지 못했습니다.");
            return;
        }

        SetPreviewVolume(volume);

        try
        {
            // Unity 버전에 따라 프리뷰 재생 메서드의 시그니처가 달라질 수 있어 파라미터 수에 맞게 호출
            ParameterInfo[] parameters = playPreviewClipMethod.GetParameters();

            if (parameters.Length == 3)
            {
                playPreviewClipMethod.Invoke(null, new object[] { selectedClip, 0, false });
            }
            else if (parameters.Length == 2)
            {
                playPreviewClipMethod.Invoke(null, new object[] { selectedClip, 0 });
            }
            else
            {
                playPreviewClipMethod.Invoke(null, new object[] { selectedClip });
            }

            ApplyPitchToPreviewClip(pitch);
            isPlaying = true;
        }
        catch (Exception exception)
        {
            isPlaying = false;
            Debug.LogError($"[SfxPreviewTool] SFX 재생 실패 - {exception.GetBaseException().Message}");
        }
    }

    // 현재 재생 중인 모든 에디터 프리뷰 오디오 정지
    private void StopPreview()
    {
        InitializeAudioUtilReflection();

        try
        {
            stopAllPreviewClipsMethod?.Invoke(null, null);
        }
        catch
        {
            // Unity 버전에 따라 내부 프리뷰 API가 달라질 수 있으므로 종료 과정에서는 예외를 무시
        }

        isPlaying = false;
    }

    // 현재 프리뷰 오디오가 실제로 재생 중인지 확인
    private static bool IsPreviewClipPlaying()
    {
        InitializeAudioUtilReflection();

        if (isPreviewClipPlayingMethod == null)
        {
            return false;
        }

        try
        {
            object result = isPreviewClipPlayingMethod.Invoke(null, null);
            return result is bool playing && playing;
        }
        catch
        {
            return false;
        }
    }

    // 선택한 SFX Asset 경로 반환
    private string GetSelectedSfxPath()
    {
        return selectedClip == null ? string.Empty : AssetDatabase.GetAssetPath(selectedClip);
    }

    // 선택한 SFX 파일을 프로젝트 Prefabs/SFX 경로에 복사
    private void CopySelectedSfx()
    {
        string sourcePath = GetSelectedSfxPath();

        if (string.IsNullOrEmpty(sourcePath))
        {
            return;
        }

        EnsureCopyFolderExists();

        string extension = Path.GetExtension(sourcePath);
        string destinationPath = AssetDatabase.GenerateUniqueAssetPath($"{sfxCopyRoot}/{selectedClip.name}{extension}");

        if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
        {
            Debug.LogError($"[SfxPreviewTool] SFX 복사 실패 - {sourcePath}");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AudioClip copiedClip = AssetDatabase.LoadAssetAtPath<AudioClip>(destinationPath);
        EditorGUIUtility.PingObject(copiedClip);
        Debug.Log($"[SfxPreviewTool] SFX 복사 완료 - {destinationPath}");
    }

    // SFX 복사 폴더가 없으면 생성
    private void EnsureCopyFolderExists()
    {
        if (string.IsNullOrEmpty(sfxCopyRoot) || !sfxCopyRoot.StartsWith("Assets", StringComparison.Ordinal))
        {
            return;
        }

        string[] folders = sfxCopyRoot.Split('/');
        string currentPath = folders[0];

        // AssetDatabase.CreateFolder는 부모 폴더가 필요하므로 상위 경로부터 순서대로 생성
        for (int i = 1; i < folders.Length; i++)
        {
            string nextPath = $"{currentPath}/{folders[i]}";

            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }

            currentPath = nextPath;
        }
    }

    // 프로젝트 Assets 내부 폴더를 선택하고 AssetDatabase 경로로 변환
    private static string SelectProjectFolder(string currentAssetPath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
        string currentAbsolutePath = projectRoot;

        if (!string.IsNullOrEmpty(currentAssetPath) && currentAssetPath.StartsWith("Assets", StringComparison.Ordinal))
        {
            currentAbsolutePath = Path.Combine(projectRoot, currentAssetPath).Replace('/', Path.DirectorySeparatorChar);
        }

        string selectedAbsolutePath = EditorUtility.OpenFolderPanel("SFX 폴더 선택", currentAbsolutePath, string.Empty);

        if (string.IsNullOrEmpty(selectedAbsolutePath))
        {
            return string.Empty;
        }

        selectedAbsolutePath = selectedAbsolutePath.Replace('\\', '/');
        projectRoot = projectRoot.Replace('\\', '/');

        if (!selectedAbsolutePath.StartsWith(projectRoot + "/Assets", StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("SFX 경로 설정", "프로젝트의 Assets 폴더 내부 경로만 선택할 수 있습니다.", "확인");
            return string.Empty;
        }

        // 절대 경로에서 프로젝트 루트를 제거하여 AssetDatabase에서 사용하는 "Assets/..." 경로로 변환
        return NormalizeAssetPath(selectedAbsolutePath[projectRoot.Length..]);
    }

    // AssetDatabase에서 사용할 수 있도록 경로 형식 정리
    private static string NormalizeAssetPath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim().Replace('\\', '/').TrimEnd('/');
    }

    // Unity 버전별 AudioUtil 내부 메서드를 한 번만 탐색
    private static void InitializeAudioUtilReflection()
    {
        if (audioUtilType != null)
        {
            return;
        }

        audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

        if (audioUtilType == null)
        {
            return;
        }

        BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo[] methods = audioUtilType.GetMethods(flags);

        // Unity 버전에 따라 AudioUtil 내부 메서드명이 달라질 수 있어 이전/현재 이름을 함께 탐색
        foreach (MethodInfo method in methods)
        {
            if (playPreviewClipMethod == null && (method.Name == "PlayPreviewClip" || method.Name == "PlayClip"))
            {
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length >= 1 && parameters[0].ParameterType == typeof(AudioClip))
                {
                    playPreviewClipMethod = method;
                }
            }

            if (stopAllPreviewClipsMethod == null && (method.Name == "StopAllPreviewClips" || method.Name == "StopAllClips"))
            {
                stopAllPreviewClipsMethod = method;
            }

            if (isPreviewClipPlayingMethod == null && (method.Name == "IsPreviewClipPlaying" || method.Name == "IsClipPlaying"))
            {
                if (method.GetParameters().Length == 0)
                {
                    isPreviewClipPlayingMethod = method;
                }
            }
        }

        previewVolumeProperty = audioUtilType.GetProperty("previewVolume", flags);
    }

    // AudioUtil 프리뷰 볼륨에 0~1 값을 그대로 적용
    private static void SetPreviewVolume(float targetVolume)
    {
        InitializeAudioUtilReflection();

        if (previewVolumeProperty == null || !previewVolumeProperty.CanWrite)
        {
            return;
        }

        try
        {
            previewVolumeProperty.SetValue(null, Mathf.Clamp01(targetVolume));
        }
        catch
        {
            // 내부 API가 지원되지 않는 Unity 버전에서는 기본 프리뷰 볼륨 사용
        }
    }

    // Unity 내부 AudioSource를 찾아 프리뷰 피치 적용
    private static void ApplyPitchToPreviewClip(float targetPitch)
    {
        // AudioUtil에서 Pitch를 직접 설정할 수 없어 에디터 내부 프리뷰 AudioSource를 찾아 적용
        AudioSource[] audioSources = Resources.FindObjectsOfTypeAll<AudioSource>();
        float clampedPitch = Mathf.Clamp(targetPitch, MinPitch, MaxPitch);

        foreach (AudioSource audioSource in audioSources)
        {
            // Scene의 일반 AudioSource에는 영향을 주지 않고 에디터 내부 AudioSource만 대상으로 제한
            if (audioSource == null || !audioSource.hideFlags.HasFlag(HideFlags.HideAndDontSave))
            {
                continue;
            }

            audioSource.pitch = clampedPitch;
        }
    }
}



