using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인스펙터 출력을 위한 구조체.
/// </summary>
[Serializable]
public struct QuestCurrencyRewardInfo
{
    [SerializeField] private CurrencyType type;
    [SerializeField] private int amount;

    public CurrencyType Type => type;
    public int Amount => amount;
}

/// <summary>
/// 퀘스트 보상 데이터를 담을 SO 클래스. <br/>
/// 일단은 재화만.
/// </summary>
[CreateAssetMenu(fileName = "NewQuestReward", menuName = "Game Data/Quest/QuestReward")]
public class QuestRewardData : ScriptableObject
{
    [Header("CurrencyReward")]
    [SerializeField] private List<QuestCurrencyRewardInfo> currencyRewards = new List<QuestCurrencyRewardInfo>();

    // 프로퍼티
    public List<QuestCurrencyRewardInfo> CurrencyRewards => currencyRewards;

    // 재화 제공 함수를 활용하는 SO에 지정된 보상 지급 함수.
    public void GiveReward()
    {
        foreach (var info in currencyRewards)
        {
            IReward reward = new CurrencyReward(info.Type, info.Amount);
            reward.GiveReward(info.Amount);
        }
    }
}