using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("전투 설정")]
    [SerializeField] private bool autoStart = true;

    private readonly List<BattleUnit> heroes = new();
    private readonly List<BattleUnit> enemies = new();

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

        // 모든 BattleUnit의 Start와 등록이 끝날 때까지 대기
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

        List<BattleUnit> units = unit.Team == UnitTeam.Hero? heroes : enemies;
        if (!units.Contains(unit)) units.Add(unit);//같은 유닛 중복 등록 방지
    }
    public void UnregisterUnit(BattleUnit unit)
    {
        if (unit == null) return;

        heroes.Remove(unit);
        enemies.Remove(unit);
    }

    public void StartBattle()
    {
        if (IsBattleRunning) return;

        RemoveInvalidUnits();

        if (!HasAliveUnit(heroes))
        {
            Debug.LogWarning("전투에 참가할 영웅이 없습니다.");
            return;
        }
        if (!HasAliveUnit(enemies))
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

        List<BattleUnit> candidates = requester.Team == UnitTeam.Hero ? enemies : heroes;
        BattleUnit closestTarget = null;
        float closestDistance = float.MaxValue;

        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            BattleUnit candidate = candidates[i];

            if (candidate == null)
            {
                candidates.RemoveAt(i);
                continue;
            }
            if (!candidate.gameObject.activeInHierarchy || candidate.IsDead)
            {
                continue;
            }

            Vector3 dir = candidate.transform.position - requester.transform.position;
            dir.y = 0f;
            float distance = dir.sqrMagnitude;
            if (distance >= closestDistance) continue;

            closestDistance = distance;
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
        bool hasAliveHero = HasAliveUnit(heroes);
        bool hasAliveEnemy = HasAliveUnit(enemies);

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
        heroes.RemoveAll(unit => unit == null);
        enemies.RemoveAll(unit => unit == null);
    }

    public BattleUnit GetLowestHpAlly(BattleUnit requester)
    {
        if (requester == null || requester.IsDead) return null;

        List<BattleUnit> allies = requester.Team == UnitTeam.Hero ? heroes : enemies;
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
            if (ally.CurrentHp >= ally.MaxHp) continue;

            float hpRatio = (float)ally.CurrentHp / ally.MaxHp;
            if (hpRatio >= lowestHpRatio) continue;

            lowestHpRatio = hpRatio;
            lowestHpAlly = ally;
        }

        return lowestHpAlly;
    }
}
