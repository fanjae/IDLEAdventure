using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Game Data/Skill/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    [Header("식별 정보")]
    [SerializeField] private string skillName;

    [Header("UI 표시 정보")]
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField, TextArea(2, 4)] private string description;

    [Header("스킬 설정")]
    [SerializeField] private SkillEffectType effectType;
    //스킬 계수        1이면 공격력의 100&     2면 공격력의 200%
    [SerializeField, Min(0.0f)] private float damageRatio = 1.5f;
    [SerializeField, Min(0.1f)] private float cooldown = 6.0f;

    [Header("스킬 안전 종료 시간")]
    //SkillEnd 애니메이션 이벤트 누락 시 스킬 상태를 복구하기 위한 시간
    [SerializeField, Min(0.1f)] private float skillSafetyDuration = 10.0f;

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

    [Header("광역 피해 스킬")]
    [SerializeField] private AreaSkillDamage areaDamagePrefab;
    [SerializeField, Min(0.1f)] private float areaRadius = 2.5f;

    [Header("휠윈드 스킬")]
    [SerializeField] private GameObject whirlwindPrefab;
    [SerializeField, Min(0.1f)] private float whirlwindDuration = 3.0f;
    [SerializeField, Min(0.05f)] private float whirlwindHitInterval = 0.2f;

    [Header("레이저 스킬")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField, Min(0.1f)] private float laserDuration = 3.0f;
    [SerializeField, Min(0.05f)] private float laserHitInterval = 0.2f;

    public string SkillName => skillName;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string Description => description;

    public SkillEffectType EffectType => effectType;
    public float DamageRatio => damageRatio;
    public float Cooldown => cooldown;

    public float SkillSafetyDuration => skillSafetyDuration;

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

    public AreaSkillDamage AreaDamagePrefab => areaDamagePrefab;
    public float AreaRadius => areaRadius;

    public GameObject WhirlwindPrefab => whirlwindPrefab;
    public float WhirlwindDuration => whirlwindDuration;
    public float WhirlwindHitInterval => whirlwindHitInterval;

    public GameObject LaserPrefab => laserPrefab;
    public float LaserDuration => laserDuration;
    public float LaserHitInterval => laserHitInterval;
}
