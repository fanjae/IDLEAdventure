using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    private int gold;

    public int Gold => gold;

    public void SetGold(int amount)
    {
        gold = amount;
    }
}
/// <summary>
/// 임시 저장용 클래스. <br/>
/// 저장 파트가 구현되면 지울 스크립트. <br/>
/// 방치 보상 테스트를 위해 임시로 PlayerPrefs를 이용한 저장 구현. <br/>
/// 현재 저장 목록 <br/>
/// Gold
/// </summary>
public class TestSaveManager : Singleton<TestSaveManager>
{
    private SaveData currentSaveData = new SaveData();

    public SaveData CurrentSaveData => currentSaveData;

    protected override void Awake()
    {
        base.Awake();

        LoadGame();
    }
    
    public void SaveGame()
    {
        PlayerPrefs.SetInt("Test_GoldData", currentSaveData.Gold);
        PlayerPrefs.Save();
        Debug.Log("[Test | PlayerPrebs] 현재 골드 저장");
    }
    public void LoadGame()
    {
        currentSaveData.SetGold(PlayerPrefs.GetInt("Test_GoldData", 0));
    }
}