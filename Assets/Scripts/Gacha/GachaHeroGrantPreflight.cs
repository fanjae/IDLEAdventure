using System;
using System.Collections.Generic;

// 가챠 다중 소환의 신규 영웅 지급 가능 여부를 사전 검증함
public static class GachaHeroGrantPreflight
{
    // 요청한 영웅 모두가 유효하고 아직 보유하지 않은 서로 다른 영웅인지 확인
    public static bool CanGrantAll(
        HeroController heroController,
        HeroDatabaseSO heroDatabase,
        IReadOnlyList<string> heroIds)
    {
        if (heroController == null || heroDatabase == null || heroIds == null)
        {
            return false;
        }

        HashSet<string> pendingHeroIds = new(StringComparer.Ordinal);
        foreach (string heroId in heroIds)
        {
            if (!heroDatabase.TryGetHero(heroId, out _) ||
                heroController.ContainsHero(heroId) ||
                !pendingHeroIds.Add(heroId))
            {
                return false;
            }
        }

        return true;
    }
}
