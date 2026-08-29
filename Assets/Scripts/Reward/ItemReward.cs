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

    public ItemReward(string itemID, float rewardValue)
    {
        this.itemID = itemID;
        this.rewardValue = rewardValue;
    }

    public void GiveReward(int amount)
    {
        // 인벤토리의 Add 함수 등 아이템 획득용 함수 호출 예정.
        // Ex) for문을 통해 rewardValue값 만큼 반복 (rewardValue 개수 지급)
        // Ex) InventoryManager.Instance.AddItem(RandomItemSelect(), 1);

        for (int i = 0; i < amount; i++)
        {
            int giveItemId = RandomItemSelect();
            InventoryManager.Instance.Controller.TryAcquireEquipment(giveItemId, out string giveItemName);
            Debug.Log($"[{giveItemName}] 획득");
        }
        
        Debug.Log($"{itemID} 획득 x{amount}");
    }
    // 최종 이야기된 아이템 지급 방식인 랜덤 지급 방식을 적용할 함수.
    // min ~ max 연속된 아이템 ID 중 랜덤 숫자 선택 및 반환 예정.
    // 자리수 별로 최소 ~ 최대 랜덤 값으로 선택.
    // 어차피 = 으로 대입할 거라 0으로 초기화는 필요 없을 것 같지만 뭐 일단은...
    private int RandomItemSelect()
    {
        int itemId = 1000;
        int temp = 0;
        // 직업군 선택
        temp = Random.Range(1, 6) * 100;
        itemId += temp;
        temp = 0;
        // 부위 선택
        temp = Random.Range(1, 6) * 10;
        itemId += temp;
        temp = 0;
        // 장비 레벨(등급?) 선택
        temp = Random.Range(1, 1);
        itemId += temp;
        temp = 0;

        return itemId;
    }
}