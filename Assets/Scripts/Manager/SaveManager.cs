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

        // 저장 시점을 UTC Unix Time 기준으로 갱신
        CurrentData.SavedAtUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        fileService.Save(CurrentData);
    }

    // 저장 파일이 존재하면 기존 데이터를 불러오고, 없으면 신규 데이터 생성
    public void Initialize()
    {
        if (fileService.Exists())
        {
            CurrentData = fileService.Load();
            return;
        }

        CurrentData = CreateNewData();
    }

    // 최초 실행 시 사용할 기본 저장 데이터 생성
    private GameSaveData CreateNewData()
    {
        return new GameSaveData
        {
            Version = 1,
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