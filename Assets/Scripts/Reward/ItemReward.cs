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
        // 인벤토리의 Add 함수 등 아이템 획득용 함수 호출하면 될듯.
        // 아이템 ID 값을 기반으로 해당 아이템 획득 가능한 함수 필요.
        // Ex) InventpryManager.Instance.AddItem(itemID, amount);
        // 물론 꼭 이런 형태는 아니어도 됨.
        // 이 부분에 대해 싱글톤 패턴을 통한 매니저로 관리할지, 인스펙터를 통해 컴포넌트 연결을 해주는 것이 좋을지도 이야기 필요.
        Debug.Log($"{itemID} 획득 x{amount}");
    }
}