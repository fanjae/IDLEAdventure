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

        // 현재 인벤토리와 장비 장착 상태를 저장 데이터에 반영
        if (InventoryManager.Instance.IsInitialized)
        {
            InventoryManager.Instance.Controller.WriteSaveData(CurrentData);
        }

        // 현재 보유 영웅 상태를 저장 데이터에 반영
        if (HeroManager.Instance.IsInitialized)
        {
            HeroManager.Instance.Controller.WriteSaveData(CurrentData);
        }

        // 현재 재화 상태를 저장 데이터에 반영
        CurrencyManager.Instance.WriteSaveData(CurrentData);

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
        return new GameSaveData
        {
            SavedAtUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }
    protected override void Awake()
    {
        base.Awake();

        // 실제 파일 입출력은 SaveFileService에서 처리
        fileService = new SaveFileService();
    }
}