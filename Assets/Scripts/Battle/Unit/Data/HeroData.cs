using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Game Data/Unit/Hero Data")]
public class HeroData : UnitDataSO
{
    [Header("영웅 정보")]
    [SerializeField] private HeroClassType classType;
    [SerializeField] private HeroRole role;

    [Header("스킬 정보")]
    [SerializeField] private SkillDataSO skillData;

    [Header("UI 정보")]
    [SerializeField] private Sprite portrait;
    
    public HeroClassType ClassType => classType;
    public HeroRole Role => role;
    public SkillDataSO SkillData => skillData;
    public Sprite Portrait => portrait;

}
