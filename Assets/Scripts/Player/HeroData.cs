using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Game Data/Unit/Hero Data")]
public class HeroData : UnitDataSO
{
    [Header("영웅 정보")]
    [SerializeField] private HeroRole role;

    public HeroRole Role => role;
}
