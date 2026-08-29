using UnityEngine;

// 하나의 업적 목표와 표시 정보를 정의하는 데이터 에셋임
[CreateAssetMenu(fileName = "Achievement", menuName = "Game Data/Achievement/Definition")]
public sealed class AchievementDefinitionSO : ScriptableObject
{
    [Header("식별 정보")]
    [SerializeField] private string achievementId = "gacha_pull_10";
    [SerializeField] private string displayName = "첫 소환사";
    [TextArea] [SerializeField] private string description;

    [Header("진행 조건")]
    [SerializeField] private AchievementMetric metric;
    [Min(1)] [SerializeField] private int targetValue = 1;

    [Header("분류")]
    [SerializeField] private AchievementCategory category = AchievementCategory.PartyGrowth;

    [Header("보상")]
    [SerializeField] private CurrencyType rewardCurrency = CurrencyType.None;
    [Min(0)] [SerializeField] private int rewardAmount;

    
[Header("표시")]
    [SerializeField] private int displayOrder;
    [SerializeField] private Sprite icon;

    public string AchievementId => achievementId;
    public string DisplayName => displayName;
    public string Description => description;
    public AchievementMetric Metric => metric;
    public int TargetValue => Mathf.Max(1, targetValue);
    public AchievementCategory Category => category;
    public int DisplayOrder => displayOrder;
    public Sprite Icon => icon;
    public CurrencyType RewardCurrency => rewardCurrency;
    public int RewardAmount => Mathf.Max(0, rewardAmount);
    public bool HasReward => rewardCurrency != CurrencyType.None && RewardAmount > 0;
}
