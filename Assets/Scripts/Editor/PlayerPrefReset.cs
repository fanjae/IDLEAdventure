using UnityEditor;
using UnityEngine;

public class PlayerPrefReset
{

    [MenuItem("Tools/ResetPlayerPrefsData")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}