using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 재화가 아닌 아이템 보상 클래스. <br/>
/// 여기에 아이템 지급은 아이템 파트에게 ID값으로 해당 아이템을 획득할 수 있도록 요청.
/// 아이템 ID를 숫자로 할지 문자열로 할지 확인,
/// 결과에 따라 CSV 파일 및 ID값 자료형 수정 필요.
/// 혹은, CSV에 문자열로 작성하고 아이템 관리 파트에서 문자열에 대응되는 숫자 ID 변환? 맵핑? 작업 요청.
/// </summary>
public class ItemReward : IReward
{
    private string itemID;    // CSV에 들어가 있는 문자열 ID
    private float rewardValue;    // 보상량

    // 프로퍼티
    public string RewardID => itemID;
    public float RewardValue => rewardValue;

    public static event Action<List<int>> OnItemRewardIds;

    public ItemReward(string itemID, float rewardValue)
    {
        this.itemID = itemID;
        this.rewardValue = rewardValue;
    }

    public void GiveReward(int amount)
    {
        List<int> equipItemIds = new List<int>();

        for (int i = 0; i < amount; i++)
        {
            int giveItemId = RandomItemSelect();
            InventoryManager.Instance.Controller.TryAcquireEquipment(giveItemId, out string giveIteminstId);
            Debug.Log($"[{giveItemId}] 획득");

            equipItemIds.Add(giveItemId);
        }
        
        Debug.Log($"{itemID} 획득 x{amount}");

        OnItemRewardIds?.Invoke(equipItemIds);
    }

    private int RandomItemSelect()
    {
        int itemId = 1000;
        int temp = 0;
        // 직업군 선택
        temp = UnityEngine.Random.Range(1, 6) * 100;
        itemId += temp;
        temp = 0;
        // 부위 선택
        temp = UnityEngine.Random.Range(1, 6) * 10;
        itemId += temp;
        temp = 0;
        // 장비 레벨(등급?) 선택
        temp = UnityEngine.Random.Range(1, 1);
        itemId += temp;
        temp = 0;
        
        return itemId;
    }
}