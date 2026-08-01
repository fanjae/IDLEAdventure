using UnityEngine;

public class UnitDataSO : ScriptableObject
{
    [Header("기본 정보")]//영웅 데이터
    [SerializeField] private string unitID;//추후에 int로 교체할 수도 있음.(삭제할 가능성도 있음)
    [SerializeField] private string unitName;

    [Header("전투 능력치")]
    [SerializeField, Min(1)] private int maxHp = 100;
    [SerializeField, Min(1)] private int attack = 10; 
    [SerializeField, Min(0)] private int defense = 3;

    [Header("전투 설정")]
    [SerializeField, Min(0.1f)] private float attackSpeed = 1.0f;
    [SerializeField, Min(0.1f)] private float attackRange = 1.5f;
    [SerializeField, Min(0.1f)] private float moveSpeed = 3.0f;

    [Header("공격 타입")]
    //[SerializeField] private AttackType //공격 타입(근거리, 원거리)

    public string UnitID => unitID;
    public string UnitName => unitName;

    public int MaxHp => maxHp;
    public int Attack => attack;
    public int Defense => defense;

    public float AttackSpeed => attackSpeed;
    public float AttackRange => attackRange;
    public float MoveSpeed => moveSpeed;

    //public AttackType //공격 타입(근거리, 원거리)
}
