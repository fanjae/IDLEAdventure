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

    public string areaName;

    public string environmentId;

    public string battlefieldId;

    [CsvIgnore]
    public List<StageEnemyData> enemies = new();

    [CsvIgnore]
    public List<FirstClearRewardData> firstClearRewards = new();
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

[Serializable]
public sealed class FirstClearRewardData
{
    [CsvMin(1)]
    public int stageId;

    public string rewardId;

    [CsvMin(1)]
    public int amount;
}