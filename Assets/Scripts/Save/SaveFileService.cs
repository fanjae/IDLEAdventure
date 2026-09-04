using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

// 저장 데이터를 JSON 파일로 저장하고 불러오는 파일 입출력 담당 클래스
public sealed class SaveFileService
{
    private const string SaveFileName = "save.json";

    private readonly string savePath;
    private readonly object saveWriteLock = new();
    private long latestSaveRequestId;

    public SaveFileService()
    {
        // 플랫폼별 Unity 영구 데이터 경로에 저장 파일 생성
        savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    // 지정한 경로를 저장 파일 경로로 사용
    public SaveFileService(string savePath)
    {
        this.savePath = savePath;
    }

    // 저장 파일 존재 여부 확인
    public bool Exists()
    {
        return File.Exists(savePath);
    }

    // GameSaveData를 JSON으로 직렬화하여 파일에 즉시 저장
    public void Save(GameSaveData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        // 즉시 저장 요청 이후 기존 비동기 저장이 덮어쓰지 못하도록 저장 요청 번호 갱신
        long requestId = Interlocked.Increment(ref latestSaveRequestId);
        string json = JsonConvert.SerializeObject(data, Formatting.None);

        lock (saveWriteLock)
        {
            if (requestId != Volatile.Read(ref latestSaveRequestId))
            {
                return;
            }

            File.WriteAllText(savePath, json);
        }
    }

    // GameSaveData 직렬화와 파일 저장을 백그라운드 스레드에서 처리
    public async UniTask SaveAsync(GameSaveData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        // 나중에 요청된 저장만 최종 파일에 반영할 수 있도록 요청 번호 생성
        long requestId = Interlocked.Increment(ref latestSaveRequestId);

        await UniTask.SwitchToThreadPool();

        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.None);

            lock (saveWriteLock)
            {
                // 더 최신 저장 요청이 존재하면 현재 저장 파일 쓰기는 생략
                if (requestId != Volatile.Read(ref latestSaveRequestId))
                {
                    return;
                }

                File.WriteAllText(savePath, json);
            }
        }
        finally
        {
            // SaveManager의 후속 처리는 Unity 메인 스레드에서 실행
            await UniTask.SwitchToMainThread();
        }
    }

    // 저장 파일을 읽어 GameSaveData로 역직렬화
    public bool TryLoad(out GameSaveData saveData)
    {
        saveData = null;

        if (!Exists())
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(savePath);

            // 저장 파일의 내용이 비어있는 경우 로드하지 않음
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            saveData = JsonConvert.DeserializeObject<GameSaveData>(json);

            // 역직렬화 결과가 없는 경우 로드하지 않음
            if (saveData == null)
            {
                return false;
            }

            return true;
        }
        catch (IOException exception)
        {
            Debug.LogError($"[SaveFileService] 저장 파일을 읽지 못했습니다. {exception.Message}");
            return false;
        }
        catch (JsonException exception)
        {
            Debug.LogError($"[SaveFileService] 저장 파일 형식이 올바르지 않습니다. {exception.Message}");
            return false;
        }
    }
}
