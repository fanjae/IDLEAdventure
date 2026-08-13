using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class FormationManager : MonoBehaviour
{
    [Header("슬롯 시스템")]
    [SerializeField] private SlotBoard slotBoard;
    [SerializeField] private SlotDragController slotDragController;
    [SerializeField] private SlotHighlightController slotHighlightController;

    [Header("영웅 전투 프리팹")]
    [SerializeField] private List<GameObject> heroPrefabs = new();

    [Header("소환된 영웅 부모")]
    [SerializeField] private Transform heroRoot;

    [Header("배치 설정")]
    [SerializeField, Min(1)] private int maxHeroCount = 5;
    [SerializeField] private float spawnHeight = 0.1f;
    [SerializeField] private Vector3 spawnRotation = Vector3.zero;

    private readonly Dictionary<HeroData, GameObject> heroPrefabByData = new();
    private readonly Dictionary<HeroData, GameObject> placedHeroesByData = new();

    private bool canFormation = true;

    public bool CanFormation
    {
        get
        {
            if (!canFormation)
            {
                return false;
            }

            if (BattleManager.Instance != null && BattleManager.Instance.IsBattleRunning)
            {
                return false;
            }

            return true;
        }
    }

    public int PlacedHeroCount => placedHeroesByData.Count;

    private void Awake()
    {
        if (slotBoard == null)
        {
            throw new Exception("FormationManager의 Slot Board가 연결되지 않음");
        }

        if (heroRoot == null)
        {
            throw new Exception("FormationManager의 Hero Root가 연결되어 않음");
        }

        BuildHeroPrefabMap();
    }

    private void Start()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        }
        else //
        {
            Debug.LogWarning("FormationManager: BattleManager가 Scene에 없음");
        }

        if (slotDragController != null)
        {
            slotDragController.enabled = CanFormation;
        }
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        }
    }

    public void ToggleHero(HeroData heroData)
    {
        if (!CanFormation)
        {
            Debug.Log("전투 중");
            return;
        }

        if (heroData == null)
        {
            Debug.LogError("HeroData가 없음");
            return;
        }

        if (placedHeroesByData.TryGetValue(heroData, out GameObject placedHero))
        {
            RemoveHero(heroData, placedHero);

            return;
        }

        if (PlacedHeroCount >= maxHeroCount)
        {
            Debug.LogWarning($"영웅은 최대 {maxHeroCount}명까지");

            return;
        }

        if (!heroPrefabByData.TryGetValue(heroData, out GameObject heroPrefab))
        {
            Debug.LogError($"{heroData.UnitName}에 대응하는 Hero Prefab을 찾을 수 없음");

            return;
        }

        int emptySlot = slotBoard.FindEmptySlot();

        if (emptySlot == -1)
        {
            Debug.LogWarning("비어 있는 영웅 배치 슬롯이 없습니다.");

            return;
        }

        PlaceHero(heroData, heroPrefab, emptySlot);
    }


    public void ClearHeroes()
    {
        if (!CanFormation)
        {
            Debug.Log("전투 중에는 영웅 배치를 변경할 수 없음");

            return;
        }

        slotDragController?.CancelCurrentDrag();

        List<GameObject> heroes = new List<GameObject>(placedHeroesByData.Values);

        foreach (GameObject hero in heroes)
        {
            if (hero == null)
            {
                continue;
            }

            slotBoard.Remove(hero);
            Destroy(hero);
        }

        placedHeroesByData.Clear();

        Debug.Log("모든 영웅 배치를 해제");
    }

    public bool IsHeroPlaced(HeroData heroData)
    {
        if (heroData == null)
        {
            return false;
        }

        return placedHeroesByData.ContainsKey(heroData);
    }

    public int GetHeroSlot(HeroData heroData)
    {
        if (heroData == null)
        {
            return -1;
        }

        if (!placedHeroesByData.TryGetValue(heroData, out GameObject hero))
        {
            return -1;
        }

        return slotBoard.FindObj(hero);
    }

    private void PlaceHero(HeroData heroData, GameObject heroPrefab, int slotNumber)
    {
        if (!slotBoard.IsSlot(slotNumber))
        {
            Debug.LogError($"{slotNumber}번 슬롯이 올바르지 않습니다.");

            return;
        }

        if (!slotBoard.IsEmpty(slotNumber))
        {
            Debug.LogWarning($"{slotNumber}번 슬롯은 이미 사용 중입니다.");

            return;
        }

        Vector3 slotLocalOffset = Vector3.up * spawnHeight;

        Vector3 spawnPosition = slotBoard.GetSlotPosition(slotNumber, slotLocalOffset);

        Quaternion spawnQuaternion = Quaternion.Euler(spawnRotation);

        GameObject hero = Instantiate(heroPrefab, spawnPosition, spawnQuaternion,heroRoot);

        hero.name = $"{heroData.UnitName}_Slot{slotNumber}";

        if (!slotBoard.Place(hero, slotNumber, slotLocalOffset))
        {
            Destroy(hero);
            return;
        }

        placedHeroesByData.Add(heroData, hero);

        slotHighlightController.Refresh();

        Debug.Log($"영웅 배치: Hero={heroData.UnitName}, Slot={slotNumber}");
    }

    private void RemoveHero(HeroData heroData, GameObject hero)
    {
        if (hero == null)
        {
            return;
        }

        slotDragController?.CancelCurrentDrag();

        int slotNumber = slotBoard.FindObj(hero);

        slotBoard.Remove(hero);

        placedHeroesByData.Remove(heroData);

        slotHighlightController.Refresh();

        Destroy(hero);

        Debug.Log($"영웅 배치 해제: Hero={heroData.UnitName}, Slot={slotNumber}");
    }

    private void HandleBattleStarted()
    {
        slotDragController?.CancelCurrentDrag();

        canFormation = false;

        Debug.Log("전투가 시작, 영웅 배치 off");
    }

    private void BuildHeroPrefabMap()
    {
        heroPrefabByData.Clear();

        foreach (GameObject prefab in heroPrefabs)
        {
            if (prefab == null)
            {
                throw new Exception("Hero Prefabs에 비어 있는 Element가 있습니다.");
            }

            BattleUnit battleUnit = prefab.GetComponent<BattleUnit>();

            if (battleUnit == null)
            {
                throw new Exception($"{prefab.name} Prefab에 BattleUnit이 없습니다.");
            }

            if (battleUnit.Team != UnitTeam.Hero)
            {
                throw new Exception($"{prefab.name} Prefab의 BattleUnit Team이 Hero가 아닙니다.");
            }

            HeroData heroData = battleUnit.UnitData as HeroData;

            if (heroData == null)
            {
                throw new Exception($"{prefab.name} Prefab의 BattleUnit UnitData가 HeroData가 아닙니다.");
            }

            if (heroPrefabByData.ContainsKey(heroData))
            {
                throw new Exception($"{heroData.UnitName}에 해당하는 Hero Prefab이 중복 등록되어 있습니다.");
            }

            heroPrefabByData.Add(heroData, prefab);
        }
    }
}