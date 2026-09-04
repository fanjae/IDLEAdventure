using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroLevelUpCostDatabase", menuName = "Game/Hero/Hero Level Up Cost Database")]
public sealed class HeroLevelUpCostDatabaseSO : ScriptableObject
{
    [SerializeField] private List<HeroLevelUpCostData> costs = new List<HeroLevelUpCostData>();

    // 비용 데이터의 마지막 현재 레벨 다음 레벨을 최대 레벨로 사용
    public int MaxLevel
    {
        get
        {
            if (costs == null || costs.Count == 0)
            {
                return 1;
            }

            int maxCurrentLevel = 0;

            foreach (HeroLevelUpCostData cost in costs)
            {
                if (cost != null && cost.Level > maxCurrentLevel)
                {
                    maxCurrentLevel = cost.Level;
                }
            }

            return maxCurrentLevel + 1;
        }
    }

    // 현재 레벨을 기준으로 다음 레벨에 필요한 비용 반환
    public bool TryGetCost(int currentLevel, out HeroLevelUpCostData cost)
    {
        cost = costs.Find(data => data.Level == currentLevel);
        return cost != null;
    }
}
