using UnityEditor;
using UnityEngine;

/// <summary>
/// 임시 저장 데이터 초기화 클래스.
/// </summary>
public class PlayerPrefReset
{
    [MenuItem("Tools/ResetPlayerPrefsData")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}