using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("전투 설정")]
    [SerializeField] private bool autoStart = true;

    private readonly List<BattleUnit> heroUnits = new();
    private readonly List<BattleUnit> enemyUnits = new();

    public event Action OnBattleStarted;
    public event Action<UnitTeam> OnBattleEnded;

    public bool IsBattleRunning { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator Start()
    {
        if (!autoStart) yield break;

        // 모든 BattleUnit의 Start와 등록이 끝난 뒤 전투 시작
        yield return null;

        StartBattle();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterUnit(BattleUnit unit)
    {
        if (unit == null) return;

        List<BattleUnit> teamUnits = unit.Team == UnitTeam.Hero? heroUnits : enemyUnits;
        if (!teamUnits.Contains(unit)) teamUnits.Add(unit);//같은 유닛 중복 등록 방지
    }
    public void UnregisterUnit(BattleUnit unit)
    {
        if (unit == null) return;

        heroUnits.Remove(unit);
        enemyUnits.Remove(unit);
    }

    public void StartBattle()
    {
        if (IsBattleRunning) return;

        RemoveInvalidUnits();

        if (!HasAliveUnit(heroUnits))
        {
            Debug.LogWarning("전투에 참가할 영웅이 없습니다.");
            return;
        }
        if (!HasAliveUnit(enemyUnits))
        {
            Debug.LogWarning("전투에 참가할 적이 없습니다.");
            return;
        }

        IsBattleRunning = true;
        OnBattleStarted?.Invoke();
    }

    public void StopBattle()
    {
        IsBattleRunning = false;
    }

    public BattleUnit GetClosestTarget(BattleUnit requester)
    {
        if (requester == null || requester.IsDead) return null;

        List<BattleUnit> targetCandidates = requester.Team == UnitTeam.Hero ? enemyUnits : heroUnits;
        BattleUnit closestTarget = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = targetCandidates.Count - 1; i >= 0; i--)
        {
            BattleUnit candidate = targetCandidates[i];

            if (candidate == null)
            {
                targetCandidates.RemoveAt(i);
                continue;
            }
            if (!candidate.gameObject.activeInHierarchy || candidate.IsDead)
            {
                continue;
            }

            Vector3 direction = candidate.transform.position - requester.transform.position;
            //높이 차이는 제외하고 수평 거리만 비교
            direction.y = 0f;
            float distanceSqr = direction.sqrMagnitude;
            if (distanceSqr >= closestDistanceSqr) continue;

            closestDistanceSqr = distanceSqr;
            closestTarget = candidate;
        }

        return closestTarget;
    }

    public void NotifyUnitDead(BattleUnit deadUnit)
    {
        if (!IsBattleRunning || deadUnit == null) return;

        CheckBattleResult();
    }

    private void CheckBattleResult()
    {
        bool hasAliveHero = HasAliveUnit(heroUnits);
        bool hasAliveEnemy = HasAliveUnit(enemyUnits);

        Debug.Log($"전투 결과 / " + $"생존 영웅 : {hasAliveHero}, " + $"생존 적 : {hasAliveEnemy}");

        if (hasAliveHero && hasAliveEnemy) return;

        IsBattleRunning = false;

        UnitTeam winner = hasAliveHero ? UnitTeam.Hero : UnitTeam.Enemy;

        Debug.Log($"전투 종료 / 승리 진영: {winner}");

        OnBattleEnded?.Invoke(winner);
    }

    private bool HasAliveUnit(List<BattleUnit> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            BattleUnit unit = units[i];

            if (unit != null && unit.gameObject.activeInHierarchy && !unit.IsDead) return true;
        }

        return false;
    }

    private void RemoveInvalidUnits()
    {
        heroUnits.RemoveAll(unit => unit == null);
        enemyUnits.RemoveAll(unit => unit == null);
    }

    public BattleUnit GetLowestHpAlly(BattleUnit requester)
    {
        if (requester == null || requester.IsDead) return null;

        List<BattleUnit> allies = requester.Team == UnitTeam.Hero ? heroUnits : enemyUnits;
        BattleUnit lowestHpAlly = null;
        float lowestHpRatio = 1.0f;

        for (int i = allies.Count - 1; i >= 0; i--)
        {
            BattleUnit ally = allies[i];

            if (ally == null)
            {
                allies.RemoveAt(i);
                continue;
            }
            if (!ally.gameObject.activeInHierarchy || ally.IsDead || ally.MaxHp <= 0) continue;
            //이미 최대 체력이면 힐 대상에서 제외
            if (ally.CurrentHp >= ally.MaxHp) continue;

            float hpRatio = (float)ally.CurrentHp / ally.MaxHp;
            if (hpRatio >= lowestHpRatio) continue;

            lowestHpRatio = hpRatio;
            lowestHpAlly = ally;
        }

        return lowestHpAlly;
    }
}
