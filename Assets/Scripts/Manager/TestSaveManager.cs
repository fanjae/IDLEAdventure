using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    [Header("Save Data")]
    [SerializeField] private int[] currencyDatas = new int[(int)CurrencyType.Length];
    [SerializeField] private long lastGetIdleRewardTime;    // 시간은 int로 표현하기에 크기가 커서 long 사용

    public int[] CurrencyDatas => currencyDatas;
    public long LastGetIdleRewardTime => lastGetIdleRewardTime;

    public void SetCurrency(CurrencyType type, int amount)
    {
        currencyDatas[(int)type] = amount;
    }
    public void SetLastIdleRewardTime(long time)
    {
        lastGetIdleRewardTime = time;
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
        for (int i = 0; i < (int)CurrencyType.Length; i++)
        {
            string key = $"Test_Currency_{i}";
            PlayerPrefs.SetInt(key, currentSaveData.CurrencyDatas[i]);
        }
        Debug.Log("[Test | PlayerPrebs] 현재 재화 저장");

        PlayerPrefs.SetString("Text_LastGetIdleRewardTime", currentSaveData.LastGetIdleRewardTime.ToString());
        Debug.Log("[Test | PlayerPrefs] 최근 방치 보상 획득 시간 저장");
        
        PlayerPrefs.Save();
    }
    public void LoadGame()
    {
        for (int i = 0; i < (int)CurrencyType.Length; i++)
        {
            string key = $"Test_Currency_{i}";
            int amount = PlayerPrefs.GetInt(key, 0);

            currentSaveData.SetCurrency((CurrencyType)i, amount);
        }

        string tempSaveData = PlayerPrefs.GetString("Text_LastGetIdleRewardTime", string.Empty);
        if (!string.IsNullOrEmpty(tempSaveData))
        {
            currentSaveData.SetLastIdleRewardTime(Convert.ToInt64(tempSaveData));
        }
    }
}