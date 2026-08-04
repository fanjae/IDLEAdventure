using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Game Data/Unit/Hero Data")]
public class HeroData : UnitDataSO
{
    [Header("영웅 정보")]
    [SerializeField] private HeroClassType classType;
    [SerializeField] private HeroRole role;

    public HeroClassType ClassType => classType;
    public HeroRole Role => role;
}
