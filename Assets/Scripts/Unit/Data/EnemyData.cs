using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Unit/Enemy Data")]
public class EnemyData : UnitDataSO
{
    [Header("적 정보")]
    [SerializeField] private bool isBoss;

    public bool IsBoss => isBoss;
}
