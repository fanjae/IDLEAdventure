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
    [SerializeField, Min(0.1f)] private float cooldown = 6.0f;

    [Header("스킬 애니메이션 시간")]
    [SerializeField, Min(0.1f)] private float actionDuration = 2.4f; //스킬 애니메이션 길이보다 0.05~0.1정도 크게??. 

    [Header("배리어 설정")]
    [SerializeField, Min(1)] private int blockCount = 5;
    [SerializeField] private GameObject barrierVfxPrefab;

    [Header("투사체 스킬 설정")]
    [SerializeField] private SkillProjectile projectilePrefab;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 10.0f;

    
    public string SkillName => skillName;
    public SkillEffectType EffectType => effectType;
    public float DamageRatio => damageRatio;
    public float Cooldown => cooldown;
    public int BlockCount => blockCount;
    public GameObject BarrierVfxPrefab => barrierVfxPrefab;
    public float ActionDuration => actionDuration;
    public SkillProjectile ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;

}
