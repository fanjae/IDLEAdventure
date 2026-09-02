using System;
using System.Collections.Generic;

[Serializable]
public sealed class FieldObjectSaveData
{
    public List<int> OpenedChestIds { get; set; } = new();
    public List<int> DefeatedEnemyIds { get; set; } = new();
}