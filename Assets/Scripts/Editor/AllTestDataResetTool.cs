using UnityEditor;
using UnityEngine;

// 테스트 과정에서 사용하는 데이터를 전체 초기화하기 위한 에디터 전용 툴
public static class AllTestDataResetTool
{
    // PlayerPrefs와 실제 저장 파일 전체 초기화
    [MenuItem("Tools/ResetAllTestData")]
    public static void ResetAllTestData()
    {
        // 전체 데이터 삭제 전 확인
        bool confirmed = EditorUtility.DisplayDialog("전체 테스트 데이터 초기화","PlayerPrefs와 실제 저장 데이터를 모두 삭제하시겠습니까?","초기화","취소");

        if (!confirmed)
        {
            return;
        }

        // PlayerPrefs 데이터 초기화
        PlayerPrefReset.ClearPlayerPrefs();

        // 실제 저장 파일 초기화
        SaveDataResetTool.ResetSaveData();

        Debug.Log("[AllTestDataResetTool] 전체 테스트 데이터를 초기화했습니다.");
    }
}