using UnityEngine;

public abstract class UnitDataSO : ScriptableObject
{
    [Header("기본 정보")]//영웅 데이터
    [SerializeField] private string unitID;//추후에 int로 교체할 수도 있음.(삭제할 가능성도 있음)
    [SerializeField] private string unitName;

    [Header("기본 능력치")]
    [SerializeField, Min(1)] private int maxHp = 100;
    [SerializeField, Min(1)] private int attack = 10;
    [SerializeField, Min(0)] private int defense = 3;

    [Header("레벨 당 성장치")]
    [SerializeField, Min(0)] private int hpPerLevel = 10;
    [SerializeField, Min(0)] private int attackPerLevel = 2;
    [SerializeField, Min(0)] private int defensePerLevel = 1;

    [Header("전투 설정")]
    [SerializeField, Min(0.1f)] private float attackSpeed = 1.0f; //기본 공격속도
    [SerializeField, Min(0.1f)] private float attackRange = 1.5f;
    [SerializeField, Min(0.1f)] private float moveSpeed = 3.0f;

    [Header("공격 타입")]
    [SerializeField] private AttackType attackType = AttackType.Melee;

    [Header("전투 설정")]
    [SerializeField] private GameObject battlePrefab;

    //기본 정보
    public string UnitID => unitID;
    public string UnitName => unitName;
    //기본 능력치
    public int MaxHp => maxHp;
    public int Attack => attack;
    public int Defense => defense;
    //레벨 당 성장치
    public int HpPerLevel => hpPerLevel;
    public int AttackPerLevel => attackPerLevel;
    public int DefensePerLevel => defensePerLevel;
    //전투 설정
    public float AttackSpeed => attackSpeed;
    public float AttackRange => attackRange;
    public float MoveSpeed => moveSpeed;
    //공격 타입
    public AttackType AttackType => attackType;


    public GameObject BattlePrefab => battlePrefab;
}
