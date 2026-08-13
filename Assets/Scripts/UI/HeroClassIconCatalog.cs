using UnityEngine;

[CreateAssetMenu(fileName = "HeroClassIconCatalog", menuName = "Game Data/UI/Hero Class Icon Catalog")]
public sealed class HeroClassIconCatalog : ScriptableObject
{
    [SerializeField] private Sprite tank;
    [SerializeField] private Sprite warrior;
    [SerializeField] private Sprite mage;
    [SerializeField] private Sprite marksman;
    [SerializeField] private Sprite support;
    [SerializeField] private Sprite rogue;

    public Sprite GetIcon(HeroClassType heroClass)
    {
        return heroClass switch
        {
            HeroClassType.Tank => tank,
            HeroClassType.Warrior => warrior,
            HeroClassType.Mage => mage,
            HeroClassType.Marksman => marksman,
            HeroClassType.Support => support,
            HeroClassType.Rogue => rogue,
            _ => null
        };
    }
}
