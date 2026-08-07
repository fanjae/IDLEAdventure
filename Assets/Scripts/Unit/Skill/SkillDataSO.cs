using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Game Data/Skill/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string skillName;

    [Header("스킬 설정")]
    [SerializeField] private SkillEffectType effectType;
    //스킬 계수        1이면 공격력의 100&     2면 공격력의 200%
    [SerializeField, Min(0.0f)] private float damageRatio = 1.5f;
    [SerializeField, Min(0.1f)] private float cooldown = 3.0f;

    
    public string SkillName => skillName;
    public SkillEffectType EffectType => effectType;
    public float DamageRatio => damageRatio;
    public float Cooldown => cooldown;
}
