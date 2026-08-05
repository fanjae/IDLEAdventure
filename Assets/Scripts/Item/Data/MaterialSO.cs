using UnityEngine;

[CreateAssetMenu(fileName = "Material", menuName = "Item/Material")]
public class MaterialSO : ItemSO
{
    public override ItemCategory Category => ItemCategory.Material;
}