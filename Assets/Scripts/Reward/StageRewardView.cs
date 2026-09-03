using System;
using TMPro;
using UnityEngine;

// 표기될 재화 정보 설정용 구조체.
[Serializable]
public struct RewardUISet
{
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private string currencyName;
    [SerializeField] private TMP_Text currencyUIText;

    // 프로퍼티
    public CurrencyType CurrencyType => currencyType;
    public string CurrencyName => currencyName;
    public TMP_Text CurrencyUIText => currencyUIText;
}
/// <summary>
/// 스테이지 클리어 보상을 출력하는 클래스.
/// </summary>
public class StageRewardView : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private RewardUISet[] rewardUISets;

    [Header("StageClearRewardData")]
    [SerializeField] private StageClearRewardTest rewardData;

    // 2026.09.03 스테이지 결과 패널 비활성화 중에도 보상 이벤트를 받을 수 있도록
    // UI 활성 상태와 이벤트 구독 생명주기를 분리
    private void Awake()
    {
        // 2026.09.04 결과 패널 Prefab에서 Scene 보상 컴포넌트를 직접 참조할 수 없어 런타임 연결
        if (rewardData == null)
        {
            rewardData = FindFirstObjectByType<StageClearRewardTest>();
        }

        if (rewardData != null)
        {
            rewardData.OnStageRewardGiven += UpdateUI;
        }
    }

    private void OnDestroy()
    {
        if (rewardData != null)
        {
            rewardData.OnStageRewardGiven -= UpdateUI;
        }
    }

    // UI 갱신 함수.
    private void UpdateUI(int stageId, StageRewardData rewardData)
    {
        foreach (var uiSet in rewardUISets)
        {
            if (uiSet.CurrencyUIText == null) continue;

            int amount = 0;
            string rewardType = uiSet.CurrencyType.ToString();

            if (rewardData.Rewards.TryGetValue(rewardType, out IReward reward))
            {
                amount = (int)reward.RewardValue;
            }

            uiSet.CurrencyUIText.text = $"{uiSet.CurrencyName}: {amount}";
        }
    }
}
