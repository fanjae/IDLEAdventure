using System;
using System.Collections.Generic;

//[CsvMin(1)]
//[CsvMin(0, false)]
//[CsvMax(100, false)]
//[CsvRange(0, 100)]
//[CsvOptional]
//[CsvIgnore]
//[CsvColumn("stage_id","ㅁㅇㄹㄴㄹ")]
[Serializable]
public sealed class StageData
{
    [CsvMin(1)]
    public int stageId;

    [CsvMin(1)]
    public int act;

    public string mapName;

    public string fieldName;

    [CsvIgnore]
    public List<StageEnemyData> enemies = new();
}

[Serializable]
public sealed class StageEnemyData
{
    [CsvMin(1)]
    public int stageId;

    [CsvMin(1)]
    public int slotNumber;

    public string enemyId;

    [CsvMin(1)]
    public int enemyLevel;

    [CsvMin(1)]
    public int maxHp;
}