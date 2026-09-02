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

    // 2026.09.02 저장된 상자 획득 이력이 있으면 필드에서 제거
    private void Start()
    {
        if (!SaveManager.TryGetExistingInstance(out SaveManager saveManager) || saveManager.CurrentData == null)
        {
            return;
        }

        saveManager.CurrentData.FieldObjects ??= new FieldObjectSaveData();

        if (saveManager.CurrentData.FieldObjects.OpenedChestIds.Contains(chestId))
        {
            Destroy(gameObject);
        }
    }

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

        // 2026.09.02 획득한 필드 상자를 저장하여 다시 생성되지 않도록 처리
        if (SaveManager.TryGetExistingInstance(out SaveManager saveManager) && saveManager.CurrentData != null)
        {
            saveManager.CurrentData.FieldObjects ??= new FieldObjectSaveData();

            if (!saveManager.CurrentData.FieldObjects.OpenedChestIds.Contains(chestId))
            {
                saveManager.CurrentData.FieldObjects.OpenedChestIds.Add(chestId);
            }

            saveManager.Save();
        }

        Destroy(gameObject);
    }
}
