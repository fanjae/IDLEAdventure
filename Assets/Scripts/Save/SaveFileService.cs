using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// 저장 데이터를 JSON 파일로 저장하고 불러오는 파일 입출력 담당 클래스
public sealed class SaveFileService
{
    private const string SaveFileName = "save.json";

    private readonly string savePath;

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

    // GameSaveData를 JSON으로 직렬화하여 파일에 저장
    public void Save(GameSaveData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(savePath, json);
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