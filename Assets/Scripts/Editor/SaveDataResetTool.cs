using System.IO;
using UnityEditor;
using UnityEngine;

// 실제 저장 파일 초기화를 위한 에디터 전용 툴
public static class SaveDataResetTool
{
    private const string SaveFileName = "save.json";

    // 실제 게임 저장 파일 삭제
    [MenuItem("Tools/ResetSaveData")]
    public static void ResetSaveData()
    {
        // 실제 저장 파일 경로
        string savePath = Path.Combine(Application.persistentDataPath, SaveFileName);

        // 저장 파일이 없는 경우 삭제하지 않음
        if (!File.Exists(savePath))
        {
            Debug.Log("[SaveDataResetTool] 삭제할 저장 데이터가 없습니다.");
            return;
        }

        // 저장 파일 삭제
        File.Delete(savePath);

        Debug.Log($"[SaveDataResetTool] 저장 데이터를 초기화했습니다. Path: {savePath}");
    }
}