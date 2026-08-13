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

    [Header("스킬 안전 종료 시간")]
    //SkillEnd 애니메이션 이벤트 누락 대비용
    [SerializeField, Min(0.1f)] private float actionDuration = 10.0f;

    [Header("배리어 설정")]
    [SerializeField] private GameObject barrierVfxPrefab;
    [SerializeField, Min(1)] private int blockCount = 5;

    [Header("투사체 스킬 설정")]
    [SerializeField] private SkillProjectile projectilePrefab;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 10.0f;

    [Header("힐 스킬")]
    [SerializeField] private GameObject healCastVfxPrefab;
    [SerializeField] private GameObject healTargetVfxPrefab;
    [SerializeField, Min(0.1f)] private float healVfxDuration = 1.5f;

    [Header("공격력 버프 스킬")]
    [SerializeField] private GameObject buffVfxPrefab;
    [SerializeField, Min(1)] private int attackBuff = 25;
    [SerializeField, Min(0.1f)] private float buffDuration = 6.0f;

    public string SkillName => skillName;
    public SkillEffectType EffectType => effectType;
    public float DamageRatio => damageRatio;
    public float Cooldown => cooldown;
    public float ActionDuration => actionDuration;

    public int BlockCount => blockCount;
    public GameObject BarrierVfxPrefab => barrierVfxPrefab;

    public SkillProjectile ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;

    public GameObject HealCastVfxPrefab => healCastVfxPrefab;
    public GameObject HealTargetVfxPrefab => healTargetVfxPrefab;
    public float HealVfxDuration => healVfxDuration;

    public GameObject BuffVfxPrefab => buffVfxPrefab;
    public int AttackBuff => attackBuff;
    public float BuffDuration => buffDuration;

}
