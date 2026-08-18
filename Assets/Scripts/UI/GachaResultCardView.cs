using TMPro;
using UnityEngine;

// 결과 오버레이에 표시할 영웅 한 장의 텍스트 골격임
public sealed class GachaResultCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text heroNameText;
    [SerializeField] private TMP_Text stateText;

    // 영웅 이름과 획득 상태 문구를 결과 카드에 표시함
    public void Bind(GachaPullResult result, string heroName)
    {
        if (heroNameText != null)
        {
            heroNameText.text = heroName;
        }

        if (stateText == null)
        {
            return;
        }

        string state = result.Rarity == GachaRarity.Tier2 ? "2티어" : "1티어";
        if (result.IsPickup) state += " · 픽업";
        if (result.IsPity) state += " · 천장";
        if (result.IsDuplicate) state += $" · 중복 +{result.ConvertedGold} 골드";
        stateText.text = state;
    }
}
