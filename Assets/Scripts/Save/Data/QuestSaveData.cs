using System;
using System.Collections.Generic;

[Serializable]
public sealed class QuestSaveData
{
    public int CurrentMainQuestId { get; set; } = 1000;
    public List<int> AcceptedSubQuestIds { get; set; } = new();
    public List<int> ClearedSubQuestIds { get; set; } = new();
}