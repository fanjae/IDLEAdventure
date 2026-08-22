using System;
using System.Collections.Generic;
using UnityEngine;

// 업적 목록을 한 곳에서 제공하는 데이터베이스 에셋임
[CreateAssetMenu(fileName = "AchievementDatabase", menuName = "Game Data/Achievement/Database")]
public sealed class AchievementDatabaseSO : ScriptableObject
{
    [SerializeField] private List<AchievementDefinitionSO> definitions = new();

    public IReadOnlyList<AchievementDefinitionSO> Definitions => definitions;

    // 등록된 업적 정의인지 참조와 ID 기준으로 확인함
    public bool Contains(AchievementDefinitionSO definition)
    {
        return definition != null && definitions.Contains(definition);
    }

    // 초기화 전에 빈 ID와 중복 ID가 없는지 검증함
    public bool TryValidate(out string errorMessage)
    {
        HashSet<string> ids = new(StringComparer.Ordinal);

        foreach (AchievementDefinitionSO definition in definitions)
        {
            if (definition == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(definition.AchievementId))
            {
                errorMessage = $"업적 ID가 비어 있음: {definition.name}";
                return false;
            }

            if (!ids.Add(definition.AchievementId))
            {
                errorMessage = $"중복 업적 ID 있음: {definition.AchievementId}";
                return false;
            }
        }

        errorMessage = string.Empty;
        return true;
    }
}
