using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Stage Clear 결과 화면의 3D 캐릭터 연출을 담당한다.
public sealed class StageClearCharacterPanelController : MonoBehaviour
{
    private const int CharacterLayer = 31;

    [Header("표시 영역")]
    [SerializeField] private RawImage characterView;
    [SerializeField] private Transform characterStageRoot;

    [Header("승리 캐릭터: 영웅 데이터와 동일한 대응")]
    [SerializeField] private GameObject tankerCharacter;
    [SerializeField] private GameObject mageCharacter;
    [SerializeField] private GameObject bruiserCharacter;
    [SerializeField] private GameObject rangerCharacter;
    [SerializeField] private GameObject healerCharacter;

    [Header("패배 캐릭터")]
    [SerializeField] private GameObject loseCharacter;

    [Header("연출 설정")]
    [SerializeField, Min(0.1f)] private float characterScale = 1.8f;
    [SerializeField, Min(0f)] private float walkSpeed = 1.4f;
    [SerializeField] private Color cameraBackground = new Color(0f, 0f, 0f, 0f);

    private readonly List<GameObject> spawnedCharacters = new();
    private Camera characterCamera;
    private RenderTexture renderTexture;
    private bool showingVictory;

    private void Awake()
    {
        // Overlay Canvas 아래에 둔 컨테이너를 월드 공간 스테이지로 분리한다.
        // 캐릭터는 별도 카메라가 RenderTexture로 CharacterView에 출력한다.
        if (characterStageRoot != null)
        {
            characterStageRoot.SetParent(null, true);
            characterStageRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            characterStageRoot.localScale = Vector3.one;
        }

        EnsureCamera();
        ClearView();
    }

    private void Start()
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogError("StageClearCharacterPanelController: BattleManager가 없습니다.", this);
        }
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    private void Update()
    {
        if (!showingVictory || spawnedCharacters.Count == 0)
        {
            return;
        }

        for (int i = 0; i < spawnedCharacters.Count; i++)
        {
            GameObject character = spawnedCharacters[i];
            if (character == null)
            {
                continue;
            }

            Vector3 position = character.transform.position;
            position.x += walkSpeed * Time.deltaTime;
            if (position.x > 6.5f)
            {
                position.x = -6.5f;
            }

            character.transform.position = position;
        }
    }

    public void ShowResult(UnitTeam winner)
    {
        // 자동전투 승리에는 결과창 자체가 열리지 않으므로 연출도 열지 않는다.
        if (StageRuntimeData.IsAutoBattle && winner == UnitTeam.Hero)
        {
            return;
        }

        if (winner == UnitTeam.Hero)
        {
            ShowVictoryCharacters();
        }
        else
        {
            ShowLoseCharacter();
        }
    }

    private void ShowVictoryCharacters()
    {
        ClearSpawnedCharacters();
        showingVictory = true;

        List<GameObject> prefabs = GetDeployedVictoryPrefabs();
        if (prefabs.Count == 0)
        {
            // 데이터가 아직 준비되지 않은 경우에도 공식 5종을 모두 보여준다.
            AddIfAssigned(prefabs, tankerCharacter);
            AddIfAssigned(prefabs, mageCharacter);
            AddIfAssigned(prefabs, bruiserCharacter);
            AddIfAssigned(prefabs, rangerCharacter);
            AddIfAssigned(prefabs, healerCharacter);
        }

        float spacing = prefabs.Count <= 1 ? 0f : Mathf.Min(1.8f, 7f / (prefabs.Count - 1));
        float startX = -(prefabs.Count - 1) * spacing * 0.5f;

        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject character = SpawnCharacter(prefabs[i],new Vector3(startX + spacing * i, 0f, 0f),Quaternion.Euler(0f, 90f, 0f));
            if (character != null)
            {
                spawnedCharacters.Add(character);
            }
        }

        SetViewActive(spawnedCharacters.Count > 0);
    }

    private void ShowLoseCharacter()
    {
        ClearSpawnedCharacters();
        showingVictory = false;

        GameObject character = SpawnCharacter(loseCharacter, Vector3.zero, Quaternion.Euler(0f, 180f, 0f));
        if (character != null)
        {
            spawnedCharacters.Add(character);
        }

        SetViewActive(character != null);
    }

    private List<GameObject> GetDeployedVictoryPrefabs()
    {
        List<GameObject> result = new();

        if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null)
        {
            return result;
        }

        FormationSaveData formation = SaveManager.Instance.CurrentData.Formation;
        if (formation == null || formation.Slots == null)
        {
            return result;
        }

        List<FormationSlotSaveData> sortedSlots = new(formation.Slots);
        sortedSlots.Sort((a, b) => a.SlotNumber.CompareTo(b.SlotNumber));

        foreach (FormationSlotSaveData slotData in sortedSlots)
        {
            if (slotData == null || string.IsNullOrEmpty(slotData.HeroId))
            {
                continue;
            }

            GameObject prefab = GetCharacterPrefab(slotData.HeroId);
            AddIfAssigned(result, prefab);
        }

        return result;
    }

    private GameObject GetCharacterPrefab(string heroId)
    {
        return heroId switch
        {
            "Hero_Tanker" => tankerCharacter,
            "Hero_Mage" => mageCharacter,
            "Hero_Bruiser" => bruiserCharacter,
            "Hero_Ranger" => rangerCharacter,
            "Hero_Healer" => healerCharacter,
            _ => null
        };
    }

    private GameObject SpawnCharacter(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null || characterStageRoot == null)
        {
            return null;
        }

        GameObject character = Instantiate(prefab, position, rotation, characterStageRoot);
        character.name = prefab.name + "_StageClear";
        character.transform.localScale = Vector3.one * characterScale;
        SetLayerRecursively(character, CharacterLayer);
        return character;
    }

    private void EnsureCamera()
    {
        GameObject cameraObject = new("StageClearCharacterCamera");
        cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 2.3f, -11f), Quaternion.identity);
        characterCamera = cameraObject.AddComponent<Camera>();
        characterCamera.clearFlags = CameraClearFlags.SolidColor;
        characterCamera.backgroundColor = cameraBackground;
        characterCamera.cullingMask = 1 << CharacterLayer;
        characterCamera.orthographic = true;
        characterCamera.orthographicSize = 3.1f;
        characterCamera.nearClipPlane = 0.1f;
        characterCamera.farClipPlane = 100f;
        characterCamera.transform.LookAt(new Vector3(0f, 1f, 0f));
        cameraObject.SetActive(false);

        renderTexture = new RenderTexture(1024, 384, 16, RenderTextureFormat.ARGB32)
        {
            name = "StageClearCharacterRT",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            antiAliasing = 1
        };
        renderTexture.Create();
        characterCamera.targetTexture = renderTexture;

        if (characterView != null)
        {
            characterView.texture = renderTexture;
            characterView.color = Color.white;
            characterView.raycastTarget = false;
        }
    }

    private void ClearView()
    {
        showingVictory = false;
        ClearSpawnedCharacters();
        SetViewActive(false);
    }

    private void ClearSpawnedCharacters()
    {
        for (int i = 0; i < spawnedCharacters.Count; i++)
        {
            if (spawnedCharacters[i] != null)
            {
                Destroy(spawnedCharacters[i]);
            }
        }

        spawnedCharacters.Clear();
    }

    private void SetViewActive(bool active)
    {
        if (characterView != null)
        {
            characterView.gameObject.SetActive(active);
        }

        if (characterCamera != null)
        {
            characterCamera.gameObject.SetActive(active);
        }
    }

    private static void AddIfAssigned(List<GameObject> list, GameObject prefab)
    {
        if (prefab != null && !list.Contains(prefab))
        {
            list.Add(prefab);
        }
    }

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
