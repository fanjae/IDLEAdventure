using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 일주일 출석 보상 한 칸의 표시와 수령 버튼 상태를 담당함
public sealed class ShopAttendanceRewardView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text claimLabel;

    // 현재 보상 차례에 맞춰 카드와 버튼을 표시함
    public void Bind(int day, ShopRewardEntry reward, Sprite icon, string state, bool isClaimable, Action claimAction)
    {
        iconImage.sprite = icon;
        dayText.text = $"DAY {day}";
        rewardText.text = GetRewardText(reward);
        stateText.text = state;
        stateText.color = isClaimable ? new Color(0.22f, 0.52f, 0.28f) : new Color(0.45f, 0.38f, 0.31f);
        claimLabel.text = isClaimable ? "수령" : state;
        claimButton.interactable = isClaimable;
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() => claimAction());
    }

    // 보상 데이터를 사용자용 문구로 변환함
    private static string GetRewardText(ShopRewardEntry reward)
    {
        if (reward == null)
            return "보상 없음";

        return reward.RewardType == ShopRewardType.Hero
            ? reward.HeroData == null ? "영웅" : reward.HeroData.UnitName
            : $"{ShopProductCardView.GetCurrencyName(reward.CurrencyType)} x{reward.Amount}";
    }
}
