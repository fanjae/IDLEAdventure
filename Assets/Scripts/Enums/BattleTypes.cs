public enum UnitTeam // 아군, 적군
{
    Hero,
    Enemy
}
public enum UnitState // 상태
{
    Idle,
    Move,
    Attack,
    Skill,
    Dead
}
public enum HeroClassType // 클래스 장비 효과를 공유하기 위한 장비 분류하는 녀석
{                         // 프로젝트에서 실제 역할군 확정되면 조정 예정.
    Tank,
    Warrior,
    Mage,
    Marksman,
    Support,
    Rogue
}
public enum HeroRole // 전투에서 탱,딜,힐   행동 목적을 구분하는 녀석
{
    Tanker,
    Dealer,
    Healer
}
public enum AttackType // 공격 방식(근거리, 원거리)
{
    Melee,
    Ranged
}
public enum SkillEffectType
{
    Damage,
    Heal,
    Barrier,
    ProjectileDamage,
    Buff
}