using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FieldStreamingManager : MonoBehaviour
{
    private enum FieldSceneState
    {
        Unloaded,
        QueuedLoad,
        Loading,
        Loaded,
        WaitingUnload,
        QueuedUnload,
        Unloading
    }

    private class FieldSceneRuntime
    {
        public string SceneName;
        public FieldSceneState State;
        public Coroutine UnloadWaitCoroutine;
        public bool UnloadAfterLoad;
        public bool LoadAfterUnload;
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private FieldStreamingTrigger streamingTrigger;


    [SerializeField] private GameObject loadingPanel;



    [Header("Field")]
    [SerializeField] private float chunkSize = 50f;

    [Header("Streaming Range")]
    [SerializeField] private int loadRadius = 1;

    [Header("Unload")]
    [SerializeField] private float unloadDelay = 2f;

    [Header("Debug")]
    [SerializeField] private bool showLog = true;

    public Vector2Int CurrentChunk { get; private set; }

    private HashSet<Vector2Int> targetChunks = new HashSet<Vector2Int>();
    private readonly Dictionary<Vector2Int, FieldSceneRuntime> sceneRuntimeTable = new Dictionary<Vector2Int, FieldSceneRuntime>();
    private readonly Queue<Vector2Int> loadQueue = new Queue<Vector2Int>();
    private readonly Queue<Vector2Int> unloadQueue = new Queue<Vector2Int>();

    private Coroutine loadQueueCoroutine;
    private Coroutine unloadQueueCoroutine;


    //
    private readonly HashSet<Vector2Int> mapChunks = new HashSet<Vector2Int>();
    private Vector2Int mapCameraChunk;
    private bool mapStreaming;
    //


    private bool checkReferences;
    private bool initialized;

    public Transform Player => player;


    //
    //private CharacterController characterController;
    //
    private void Awake()
    {
        //characterController = player.GetComponent<CharacterController>();
        //if (characterController != null) characterController.enabled = false;
        if (player != null) player.gameObject.SetActive(false);

        checkReferences = CheckReferences();



        if (loadingPanel != null) loadingPanel.SetActive(true);
    }

    private IEnumerator Start()
    {
        if (!checkReferences) yield break;

        CurrentChunk = WorldToChunk(player.position);

        streamingTrigger.Initialize(this, player, chunkSize, CurrentChunk);
        RefreshStreaming();




        yield return new WaitUntil(LoadingOn);

        //if (characterController != null) characterController.enabled = true;
        if (player != null) player.gameObject.SetActive(true);


        initialized = true;



        if (loadingPanel != null) loadingPanel.SetActive(false);



        if (showLog) Debug.Log($"[FieldStreaming] Start Chunk : {CurrentChunk}");
    }


    private bool LoadingOn()
    {
        foreach (Vector2Int chunk in targetChunks)
        {
            FieldSceneRuntime runtime = GetOrCreateRuntime(chunk);
            Scene scene = SceneManager.GetSceneByName(runtime.SceneName);

            if (!scene.IsValid() || !scene.isLoaded) return false;
        }

        return true;
    }



    private bool CheckReferences()
    {
        if (player == null)
        {
            Debug.LogError("[FieldStreaming] Player가 없음");
            return false;
        }

        if (streamingTrigger == null)
        {
            Debug.LogError("[FieldStreaming] StreamingBoundary가 없음");
            return false;
        }

        if (chunkSize <= 0f)
        {
            Debug.LogError("[FieldStreaming] Chunk Size는 0보다 커야 함");
            return false;
        }

        return true;
    }

    public void HandleTriggerExit()
    {
        if (!initialized) return;

        Vector2Int nextChunk = WorldToChunk(player.position);

        if (nextChunk == CurrentChunk)
        {
            streamingTrigger.MoveToChunk(CurrentChunk);
            return;
        }

        if (showLog) Debug.Log($"[FieldStreaming] Chunk Changed : {CurrentChunk} -> {nextChunk}");

        CurrentChunk = nextChunk;

        streamingTrigger.MoveToChunk(CurrentChunk);
        RefreshStreaming();
    }

    public void RefreshAtPlayerPosition()
    {
        if (!initialized || player == null) return;

        CurrentChunk = WorldToChunk(player.position);

        streamingTrigger.MoveToChunk(CurrentChunk);
        RefreshStreaming();

        if (showLog) Debug.Log($"[FieldStreaming] Force Refresh : {CurrentChunk}");
    }

    private Vector2Int WorldToChunk(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / chunkSize);
        int z = Mathf.FloorToInt(worldPos.z / chunkSize);

        return new Vector2Int(x, z);
    }

    private void RefreshStreaming()
    {
        List<Vector2Int> orderedChunks = BuildTargetChunkList();
        HashSet<Vector2Int> newTargetChunks = new HashSet<Vector2Int>(orderedChunks);
        HashSet<Vector2Int> oldTargetChunks = targetChunks;

        targetChunks = newTargetChunks;

        foreach (Vector2Int chunk in orderedChunks)
        {
            TryQueueLoad(chunk);
        }

        foreach (Vector2Int chunk in oldTargetChunks)
        {
            if (newTargetChunks.Contains(chunk)) continue;

            TryDelayedUnload(chunk);
        }
    }

    private List<Vector2Int> BuildTargetChunkList()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int z = -loadRadius; z <= loadRadius; z++)
            {
                result.Add(new Vector2Int(CurrentChunk.x + x, CurrentChunk.y + z));
            }
        }

        result.Sort((a, b) =>
        {
            int aDistance = GetChunkDistance(a, CurrentChunk);
            int bDistance = GetChunkDistance(b, CurrentChunk);

            return aDistance.CompareTo(bDistance);
        });

        return result;
    }

    private static int GetChunkDistance(Vector2Int a, Vector2Int b)
    {
        int dx = a.x - b.x;
        int dz = a.y - b.y;

        return dx * dx + dz * dz;
    }

    private void TryQueueLoad(Vector2Int chunk)
    {
        FieldSceneRuntime runtime = GetOrCreateRuntime(chunk);
        runtime.UnloadAfterLoad = false;

        Scene loadedScene = SceneManager.GetSceneByName(runtime.SceneName);

        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            CancelUnloadWait(runtime);
            runtime.State = FieldSceneState.Loaded;
            return;
        }

        switch (runtime.State)
        {
            case FieldSceneState.QueuedLoad:
            case FieldSceneState.Loading:
            case FieldSceneState.Loaded:
                return;

            case FieldSceneState.WaitingUnload:
                CancelUnloadWait(runtime);
                runtime.State = FieldSceneState.Loaded;
                return;

            case FieldSceneState.QueuedUnload:
                runtime.State = FieldSceneState.Loaded;
                return;

            case FieldSceneState.Unloading:
                runtime.LoadAfterUnload = true;
                return;
        }

        if (!Application.CanStreamedLevelBeLoaded(runtime.SceneName))
        {
            if (showLog) Debug.LogWarning($"[FieldStreaming] Scene을 찾을 수 없음 : {runtime.SceneName}");
            return;
        }

        runtime.State = FieldSceneState.QueuedLoad;
        loadQueue.Enqueue(chunk);

        StartLoadQueue();
    }

    private void StartLoadQueue()
    {
        if (loadQueueCoroutine != null) return;

        loadQueueCoroutine = StartCoroutine(ProcessLoadQueue());
    }

    private IEnumerator ProcessLoadQueue()
    {
        while (loadQueue.Count > 0)
        {
            Vector2Int chunk = loadQueue.Dequeue();

            if (!sceneRuntimeTable.TryGetValue(chunk, out FieldSceneRuntime runtime)) continue;
            if (runtime.State != FieldSceneState.QueuedLoad) continue;
            
            //if (!targetChunks.Contains(chunk))            
            if (!KeepChunk(chunk))
            {
                runtime.State = FieldSceneState.Unloaded;
                continue;
            }

            Scene existingScene = SceneManager.GetSceneByName(runtime.SceneName);

            if (existingScene.IsValid() && existingScene.isLoaded)
            {
                runtime.State = FieldSceneState.Loaded;
                continue;
            }

            runtime.State = FieldSceneState.Loading;

            if (showLog) Debug.Log($"[FieldStreaming] LOAD START : {runtime.SceneName}");

            AsyncOperation operation = SceneManager.LoadSceneAsync(runtime.SceneName, LoadSceneMode.Additive);

            if (operation == null)
            {
                runtime.State = FieldSceneState.Unloaded;
                Debug.LogError($"[FieldStreaming] Load 실패 : {runtime.SceneName}");
                continue;
            }

            yield return operation;

            Scene loadedScene = SceneManager.GetSceneByName(runtime.SceneName);

            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                runtime.State = FieldSceneState.Unloaded;
                Debug.LogError($"[FieldStreaming] Load 완료 후 Scene을 찾지 못함: {runtime.SceneName}");
                continue;
            }

            runtime.State = FieldSceneState.Loaded;

            if (showLog) Debug.Log($"[FieldStreaming] LOAD COMPLETE : {runtime.SceneName}");

            if (/*!targetChunks.Contains(chunk)*/!KeepChunk(chunk) || runtime.UnloadAfterLoad)
            {
                runtime.UnloadAfterLoad = false;
                TryDelayedUnload(chunk);
            }

            yield return null;
        }

        loadQueueCoroutine = null;
    }

    private void TryDelayedUnload(Vector2Int chunk)
    {
        if (!sceneRuntimeTable.TryGetValue(chunk, out FieldSceneRuntime runtime)) return;
        if (KeepChunk(chunk)) return;
        switch (runtime.State)
        {
            case FieldSceneState.Unloaded:
                return;

            case FieldSceneState.QueuedLoad:
                runtime.State = FieldSceneState.Unloaded;
                return;

            case FieldSceneState.Loading:
                runtime.UnloadAfterLoad = true;
                return;

            case FieldSceneState.WaitingUnload:
            case FieldSceneState.QueuedUnload:
            case FieldSceneState.Unloading:
                return;
        }

        runtime.State = FieldSceneState.WaitingUnload;
        runtime.UnloadWaitCoroutine = StartCoroutine(DelayedUnload(chunk, runtime));
    }

    private IEnumerator DelayedUnload(Vector2Int chunk, FieldSceneRuntime runtime)
    {
        yield return new WaitForSecondsRealtime(unloadDelay);

        runtime.UnloadWaitCoroutine = null;

        //if (targetChunks.Contains(chunk))
        if (KeepChunk(chunk))
        {
            runtime.State = FieldSceneState.Loaded;
            yield break;
        }

        runtime.State = FieldSceneState.QueuedUnload;
        unloadQueue.Enqueue(chunk);

        StartUnloadQueue();
    }

    private void StartUnloadQueue()
    {
        if (unloadQueueCoroutine != null) return;

        unloadQueueCoroutine = StartCoroutine(ProcessUnloadQueue());
    }

    private IEnumerator ProcessUnloadQueue()
    {
        while (unloadQueue.Count > 0)
        {
            Vector2Int chunk = unloadQueue.Dequeue();

            if (!sceneRuntimeTable.TryGetValue(chunk, out FieldSceneRuntime runtime)) continue;
            if (runtime.State != FieldSceneState.QueuedUnload) continue;

            //if (targetChunks.Contains(chunk))
            if (KeepChunk(chunk))
            {
                runtime.State = FieldSceneState.Loaded;
                continue;
            }

            Scene scene = SceneManager.GetSceneByName(runtime.SceneName);

            if (!scene.IsValid() || !scene.isLoaded)
            {
                runtime.State = FieldSceneState.Unloaded;
                continue;
            }

            runtime.State = FieldSceneState.Unloading;

            if (showLog) Debug.Log($"[FieldStreaming] UNLOAD START : {runtime.SceneName}");

            AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);

            if (operation != null) yield return operation;

            runtime.State = FieldSceneState.Unloaded;

            if (showLog) Debug.Log($"[FieldStreaming] UNLOAD COMPLETE : {runtime.SceneName}");

            if (runtime.LoadAfterUnload || /*targetChunks.Contains(chunk)*/KeepChunk(chunk))
            {
                runtime.LoadAfterUnload = false;
                TryQueueLoad(chunk);
            }

            yield return null;
        }

        unloadQueueCoroutine = null;
    }

    private void CancelUnloadWait(FieldSceneRuntime runtime)
    {
        if (runtime.UnloadWaitCoroutine == null) return;

        StopCoroutine(runtime.UnloadWaitCoroutine);
        runtime.UnloadWaitCoroutine = null;
    }

    private FieldSceneRuntime GetOrCreateRuntime(Vector2Int chunk)
    {
        if (sceneRuntimeTable.TryGetValue(chunk, out FieldSceneRuntime runtime)) return runtime;

        runtime = new FieldSceneRuntime
        {
            SceneName = GetSceneName(chunk),
            State = FieldSceneState.Unloaded
        };

        sceneRuntimeTable.Add(chunk, runtime);

        return runtime;
    }

    private string GetSceneName(Vector2Int chunk)
    {
        return $"Field_X{chunk.x}_Z{chunk.y}";
    }

    //
    public void StartMapStreaming(Vector3 cameraPosition)
    {
        mapStreaming = true;
        mapChunks.Clear();

        mapCameraChunk = WorldToChunk(cameraPosition);
        AddMapChunks(mapCameraChunk);
    }

    public void UpdateMapStreaming(Vector3 cameraPosition)
    {
        if (!mapStreaming) return;

        Vector2Int nextChunk = WorldToChunk(cameraPosition);
        if (nextChunk == mapCameraChunk) return;

        mapCameraChunk = nextChunk;
        AddMapChunks(mapCameraChunk);
    }

    public void EndMapStreaming()
    {
        if (!mapStreaming) return;

        mapStreaming = false;

        List<Vector2Int> chunks = new List<Vector2Int>(mapChunks);
        mapChunks.Clear();

        foreach (Vector2Int chunk in chunks)
        {
            if (targetChunks.Contains(chunk)) continue;
            TryDelayedUnload(chunk);
        }
    }

    private void AddMapChunks(Vector2Int centerChunk)
    {
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int z = -loadRadius; z <= loadRadius; z++)
            {
                Vector2Int chunk = new Vector2Int(centerChunk.x + x, centerChunk.y + z);

                if (!mapChunks.Add(chunk)) continue;

                TryQueueLoad(chunk);
            }
        }
    }

    private bool KeepChunk(Vector2Int chunk)
    {
        return targetChunks.Contains(chunk) || mapChunks.Contains(chunk);
    }
    //

}