using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Game Data/Unit/Hero Data")]
public class HeroData : UnitDataSO
{
    [Header("영웅 정보")]
    [SerializeField] private HeroClassType classType;
    [SerializeField] private HeroRole role;

    [Header("스킬 정보")]
    [SerializeField] private SkillDataSO skillData;
    [SerializeField] private SkillDataSO passiveSkillData;

    [Header("UI 정보")]
    [SerializeField] private Sprite portrait;

    [Header("전투 UI")]
    [SerializeField] private Sprite battlePortrait;
    
    public HeroClassType ClassType => classType;
    public HeroRole Role => role;
    public SkillDataSO SkillData => skillData;
    public SkillDataSO PassiveSkillData => passiveSkillData;
    public Sprite Portrait => portrait;
    public Sprite BattlePortrait => battlePortrait;

}
