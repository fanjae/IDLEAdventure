using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

// 게임 저장 데이터의 생성, 로드 저장 관리
// 외부에서 SaveManager 통해 저장 데이터에 접근
public sealed class SaveManager : Singleton<SaveManager>
{
    private static readonly ProfilerMarker SaveTotalMarker = new("Save.Total");
    private static readonly ProfilerMarker SnapshotMarker = new("Save.Snapshot");

    private readonly HashSet<ISaveDataWriter> saveDataWriters = new();

    private SaveFileService fileService;
    private bool isSaving;
    private bool saveRequested;

    public GameSaveData CurrentData { get; private set; }

    // 현재 게임 상태 저장에 참여할 객체 등록
    public void RegisterWriter(ISaveDataWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        saveDataWriters.Add(writer);
    }

    // 현재 게임 상태 저장 대상에서 객체 제거
    public void UnregisterWriter(ISaveDataWriter writer)
    {
        if (writer == null)
        {
            return;
        }

        saveDataWriters.Remove(writer);
    }

    // 기존 저장 호출을 비동기 저장 요청으로 연결
    public void Save()
    {
        RequestSave();
    }

    // 일반 플레이 중 현재 게임 데이터 저장 요청
    public void RequestSave()
    {
        if (CurrentData == null)
        {
            throw new InvalidOperationException("저장 데이터가 초기화되지 않았습니다.");
        }

        // 저장 중 추가 요청이 들어오면 현재 저장 완료 후 최신 상태를 다시 저장
        saveRequested = true;

        if (isSaving)
        {
            return;
        }

        SaveLoopAsync().Forget();
    }

    // 종료 또는 백그라운드 전환 전 현재 게임 데이터를 즉시 저장
    public void SaveImmediate()
    {
        if (CurrentData == null)
        {
            return;
        }

        // 즉시 저장 이후 기존 비동기 저장 요청이 다시 실행되지 않도록 요청 상태 초기화
        saveRequested = false;

        using ProfilerMarker.AutoScope saveScope = SaveTotalMarker.Auto();

        WriteCurrentState();

        GameSaveData snapshot;

        // 비동기 저장과 동일한 독립 저장 데이터 생성
        using (SnapshotMarker.Auto())
        {
            snapshot = GameSaveDataSnapshot.Create(CurrentData);
        }

        fileService.Save(snapshot);
    }

    // 연속된 저장 요청을 하나씩 처리하고 중복 요청은 최신 상태 기준으로 다시 저장
    private async UniTaskVoid SaveLoopAsync()
    {
        isSaving = true;

        try
        {
            while (saveRequested)
            {
                saveRequested = false;

                GameSaveData snapshot;

                // Unity 런타임 데이터 수집과 저장용 복사본 생성은 메인 스레드에서 처리
                using (SaveTotalMarker.Auto())
                {
                    WriteCurrentState();

                    using (SnapshotMarker.Auto())
                    {
                        snapshot = GameSaveDataSnapshot.Create(CurrentData);
                    }
                }

                // JSON 직렬화와 파일 쓰기는 백그라운드 스레드에서 처리
                await fileService.SaveAsync(snapshot);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveManager] 비동기 저장에 실패했습니다. {exception.Message}");
        }
        finally
        {
            isSaving = false;

            // 저장 종료 시점에 새 요청이 존재하면 저장 다시 시작
            if (saveRequested)
            {
                RequestSave();
            }
        }
    }

    // 현재 게임 상태를 저장 데이터에 반영
    private void WriteCurrentState()
    {
        if (CurrentData == null)
        {
            throw new InvalidOperationException("저장 데이터가 초기화되지 않았습니다.");
        }

        // 등록된 시스템의 현재 런타임 상태를 저장 데이터에 반영
        foreach (ISaveDataWriter writer in saveDataWriters)
        {
            writer.WriteSaveData(CurrentData);
        }

        // 저장 시점을 UTC Unix Time 기준으로 갱신
        CurrentData.SavedAtUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

        SaveImmediate();
    }

    // 앱이 정상 종료를 요청할 때 현재 게임 데이터 저장
    private bool HandleWantsToQuit()
    {
        Debug.Log("[SaveManager] 정상 종료 저장 실행");

        if (CurrentData != null)
        {
            SaveImmediate();
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
