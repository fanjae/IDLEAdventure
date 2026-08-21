using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class FormationManager : MonoBehaviour
{
    [Header("슬롯 시스템")]
    [SerializeField] private SlotBoard slotBoard;
    [SerializeField] private SlotDragController slotDragController;
    [SerializeField] private SlotHighlightController slotHighlightController;

    // 0820 추가
    [Header("배치 UI")]
    [SerializeField] private FormationHeroListController heroListController;

    [Header("소환된 영웅 부모")]
    [SerializeField] private Transform heroRoot;

    [Header("배치 설정")]
    [SerializeField, Min(1)] private int maxHeroCount = 5;
    [SerializeField] private float spawnHeight = 0.1f;
    [SerializeField] private Vector3 spawnRotation = Vector3.zero;

    private readonly Dictionary<HeroData, GameObject> placedHeroesByData = new();
    private bool isRestoringFormation;

    private void Awake()
    {
        if (slotBoard == null)
        {
            throw new Exception("FormationManager의 Slot Board가 연결 안됨");
        }

        if (heroRoot == null)
        {
            throw new Exception("FormationManager의 Hero Root가 연결 안됨");
        }
    }

    // 08020 수정 (heroListController를 연결하도록 교체)
    private void Start()
    {
        if (heroListController == null)
        {
            throw new Exception("FormationManager의 Hero List Controller가 연결 안됨");
        }

        heroListController.OnHeroSelected += HandleFormationHeroSelected;

        // 영웅 슬롯 위치 변경 이벤트 연결
        if (slotDragController != null)
        {
            slotDragController.OnPlacementChanged += HandlePlacementChanged;
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        }

        // 저장된 영웅 배치 복원
        RestoreFormation();
    }

    // 08020 수정 (heroListController를 연결하도록 교체)
    private void OnDestroy()
    {
        if (heroListController != null)
        {
            heroListController.OnHeroSelected -= HandleFormationHeroSelected;
        }

        if (slotDragController != null)
        {
            slotDragController.OnPlacementChanged -= HandlePlacementChanged;
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        }
    }

    private void HandleFormationHeroSelected(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        if (!HeroManager.Instance.Controller.TryGetHero(heroId, out OwnedHeroData ownedHero))
        {
            return;
        }

        ToggleHero(ownedHero.HeroData);
    }

    //영웅 소환
    private void ToggleHero(HeroData heroData)
    {
        if (placedHeroesByData.TryGetValue(heroData, out GameObject placedHero))
        {
            RemoveHero(heroData, placedHero);
            return;
        }

        if (placedHeroesByData.Count >= maxHeroCount)
        {
            Debug.LogWarning($"영웅은 최대 {maxHeroCount}명까지 배치할 수 있음");
            return;
        }

        GameObject heroPrefab = heroData.BattlePrefab;

        if (heroPrefab == null)
        {
            Debug.LogError($"{heroData.UnitName}의 BattlePrefab이 설정되어 있지 않음");
            return;
        }

        // 빈 슬롯 찾기
        int emptySlot = slotBoard.FindEmptySlot();

        if (emptySlot == -1)
        {
            Debug.LogWarning("비어 있는 영웅 배치 슬롯이 없음");
            return;
        }

        PlaceHero(heroData, heroPrefab, emptySlot);
    }

    private void PlaceHero(HeroData heroData, GameObject heroPrefab, int slotNumber)
    {
        Vector3 slotLocalOffset = Vector3.up * spawnHeight;

        Vector3 spawnPosition = slotBoard.GetSlotPosition(slotNumber, slotLocalOffset);

        Quaternion spawnQuaternion = Quaternion.Euler(spawnRotation);

        GameObject hero = Instantiate(heroPrefab, spawnPosition, spawnQuaternion, heroRoot);

        hero.name = $"{heroData.UnitName}_Slot{slotNumber}";

        if (!slotBoard.Place(hero, slotNumber, slotLocalOffset))
        {
            Destroy(hero);
            return;
        }

        placedHeroesByData.Add(heroData, hero);

        slotHighlightController?.Refresh();

        // 배치 복원 중이 아닌 경우 현재 상태 저장
        if (!isRestoringFormation)
        {
            WriteFormationData();
        }

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

        slotHighlightController?.Refresh();

        // 현재 영웅 배치 상태 저장
        WriteFormationData();

        Destroy(hero);

        Debug.Log($"영웅 배치 해제: Hero={heroData.UnitName}, Slot={slotNumber}");
    }

    public void StartBattle()
    {
        if (placedHeroesByData.Count <= 0)
        {
            Debug.LogWarning("배치된 영웅이 없음");
            return;
        }

        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없음");
            return;
        }

        // 전투 시작 전 현재 영웅 배치 저장
        WriteFormationData();
        SaveManager.Instance.Save();

        BattleManager.Instance.StartBattle();
    }

    public void ClearHeroes()
    {
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

        slotHighlightController?.Refresh();

        // 비워진 영웅 배치 상태 저장
        WriteFormationData();

        Debug.Log("모든 영웅 배치를 해제");
    }

    private void HandleBattleStarted()
    {
        slotDragController?.CancelCurrentDrag();

        if (slotDragController != null)
        {
            slotDragController.enabled = false;
        }

        slotHighlightController?.HideSlots();

        Debug.Log("전투 시작: 진행 중인 영웅 드래그를 취소");
    }

    // 현재 영웅 배치 상태를 저장 데이터에 반영 (0821 추가)
    private void WriteFormationData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null)
        {
            return;
        }

        FormationSaveData formation = new();

        foreach (KeyValuePair<HeroData, GameObject> pair in placedHeroesByData)
        {
            HeroData heroData = pair.Key;
            GameObject hero = pair.Value;

            if (heroData == null || hero == null)
            {
                continue;
            }

            int slotNumber = slotBoard.FindObj(hero);

            if (slotNumber == -1)
            {
                continue;
            }

            formation.Slots.Add(new FormationSlotSaveData
            {
                SlotNumber = slotNumber,
                HeroId = heroData.UnitID
            });
        }

        SaveManager.Instance.CurrentData.Formation = formation;
    }

    // 저장된 영웅 배치 복원 (0821 추가)
    private void RestoreFormation()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null)
        {
            return;
        }

        FormationSaveData formation = SaveManager.Instance.CurrentData.Formation;

        if (formation == null || formation.Slots == null)
        {
            return;
        }

        isRestoringFormation = true;

        try
        {
            foreach (FormationSlotSaveData slotData in formation.Slots)
            {
                if (slotData == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(slotData.HeroId))
                {
                    continue;
                }

                if (!slotBoard.IsSlot(slotData.SlotNumber))
                {
                    continue;
                }

                if (!slotBoard.IsEmpty(slotData.SlotNumber))
                {
                    continue;
                }

                if (!HeroManager.Instance.Controller.TryGetHero(slotData.HeroId, out OwnedHeroData ownedHero))
                {
                    continue;
                }

                HeroData heroData = ownedHero.HeroData;

                if (heroData == null)
                {
                    continue;
                }

                if (heroData.BattlePrefab == null)
                {
                    continue;
                }

                if (placedHeroesByData.ContainsKey(heroData))
                {
                    continue;
                }

                if (placedHeroesByData.Count >= maxHeroCount)
                {
                    break;
                }

                PlaceHero(heroData, heroData.BattlePrefab, slotData.SlotNumber);
            }
        }
        finally
        {
            isRestoringFormation = false;
        }
    }

    // 영웅 슬롯 위치 변경 시 현재 배치 상태 저장 (0821 추가)
    private void HandlePlacementChanged()
    {
        WriteFormationData();
    }
}