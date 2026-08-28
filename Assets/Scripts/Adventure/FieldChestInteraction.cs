using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ChestRewardInfo
{
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private int amount;

    public CurrencyType CurrencyType => currencyType;
    public int Amount => amount;
}

[RequireComponent(typeof(Collider))]
public class FieldChestInteraction : MonoBehaviour
{
    [Header("Chest Info")]
    [SerializeField] private int chestId;

    [Header("Reward Setting")]
    [SerializeField] private List<ChestRewardInfo> chestRewards = new List<ChestRewardInfo>();

    private bool isOpened = false;

    // 저장 파트가 들어오면 저장된 데이터를 받아와 획득한 상자면 바로 지워버리는? 방식으로 진행하면 될듯.
    //private void Start()
    //{
        
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (isOpened) return;

        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(true, InteractType.Chest, OnInteractChest);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isOpened) return;

        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(false, InteractType.Chest);
        }
    }

    private void OnInteractChest()
    {
        if (isOpened) return;
        isOpened = true;

        foreach (ChestRewardInfo info in chestRewards)
        {
            IReward reward = new CurrencyReward(info.CurrencyType, info.Amount);
            reward.GiveReward(info.Amount);
            Debug.Log($"상자 보상 획득 | [{info.CurrencyType}] + {info.Amount}");
        }

        Destroy(gameObject);
    }
}
