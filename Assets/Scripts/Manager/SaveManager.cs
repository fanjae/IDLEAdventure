using System;
using UnityEngine;

// 게임 저장 데이터의 생성, 로드 저장 관리
// 외부에서 SaveManager 통해 저장 데이터에 접근
public sealed class SaveManager : Singleton<SaveManager>
{
    private SaveFileService fileService;

    public GameSaveData CurrentData { get; private set; }

    // 현재 게임 데이터를 저장 파일에 반영
    public void Save()
    {
        if (CurrentData == null)
        {
            throw new InvalidOperationException("저장 데이터가 초기화되지 않았습니다.");
        }

        // 현재 생성되어 있는 인벤토리와 장비 장착 상태를 저장 데이터에 반영
        if (InventoryManager.TryGetExistingInstance(out InventoryManager inventoryManager) && inventoryManager.IsInitialized)
        {
            inventoryManager.Controller.WriteSaveData(CurrentData);
        }

        // 현재 생성되어 있는 보유 영웅 상태를 저장 데이터에 반영
        if (HeroManager.TryGetExistingInstance(out HeroManager heroManager) && heroManager.IsInitialized)
        {
            heroManager.Controller.WriteSaveData(CurrentData);
        }

        // 현재 생성되어 있는 공명 상태를 저장 데이터에 반영
        if (ResonanceManager.TryGetExistingInstance(out ResonanceManager resonanceManager) && resonanceManager.IsInitialized)
        {
            resonanceManager.Controller.WriteSaveData(CurrentData);
        }

        // 현재 생성되어 있는 재화 상태를 저장 데이터에 반영
        if (CurrencyManager.TryGetExistingInstance(out CurrencyManager currencyManager))
        {
            currencyManager.WriteSaveData(CurrentData);
        }

        // 저장 시점을 UTC Unix Time 기준으로 갱신
        CurrentData.SavedAtUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        fileService.Save(CurrentData);
    }

    // 저장 파일을 불러오고 사용할 수 없는 경우 신규 데이터 생성
    public void Initialize()
    {
        if (fileService.TryLoad(out GameSaveData saveData))
        {
            CurrentData = saveData;
            return;
        }

        CurrentData = CreateNewData();
    }

    // 최초 실행 시 사용할 기본 저장 데이터 생성
    private GameSaveData CreateNewData()
    {
        /* 1차 빌드 때 기본 영웅에 대한 지급 문제로 추후 재주석
        return new GameSaveData
        {
            SavedAtUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        }; */

        GameSaveData saveData = new()
        {
            SavedAtUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // 최초 플레이 시 기본 영웅 지급
        saveData.Heroes.OwnedHeroes.Add(new OwnedHeroSaveData { HeroId = "Hero_Tanker", Level = 1 });
        saveData.Heroes.OwnedHeroes.Add(new OwnedHeroSaveData { HeroId = "Hero_Ranger", Level = 1 });
        saveData.Heroes.OwnedHeroes.Add(new OwnedHeroSaveData { HeroId = "Hero_Healer", Level = 1 });

        return saveData;
    }

    // 앱이 백그라운드로 전환될 때 현재 게임 데이터 저장
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus || CurrentData == null)
        {
            return;
        }

        Save();
    }

    // 앱이 정상 종료를 요청할 때 현재 게임 데이터 저장
    private bool HandleWantsToQuit()
    {
        Debug.Log("[SaveManager] 정상 종료 저장 실행");

        if (CurrentData != null)
        {
            Save();
        }

        return true;
    }

    protected override void Awake()
    {
        base.Awake();

        // 실제 파일 입출력은 SaveFileService에서 처리
        fileService = new SaveFileService();

        // 앱이 정상 종료를 요청하면 현재 게임 상태 저장
        Application.wantsToQuit += HandleWantsToQuit;
    }

    protected override void OnDestroy()
    {
        // 정상 종료 저장 이벤트 등록 해제
        Application.wantsToQuit -= HandleWantsToQuit;

        base.OnDestroy();
    }

    // 앱이 종료될 때 싱글톤 종료 상태 반영
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }
}