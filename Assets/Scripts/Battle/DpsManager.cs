using System.Collections.Generic;
using UnityEngine;

public class DpsManager : MonoBehaviour
{
    public static DpsManager Instance { get; private set; }

    private const float DpsWindow = 5.0f;

    private class DamageRecord
    {
        public float time;
        public int damage;

        public DamageRecord(float time, int damage)
        {
            this.time = time;
            this.damage = damage;
        }
    }

    private readonly Dictionary<BattleUnit, List<DamageRecord>> damageRecords = new Dictionary<BattleUnit, List<DamageRecord>>();
    private float battleStartTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        }
    }
    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        }
        if (Instance == this) Instance = null;
    }

    private void HandleBattleStarted()
    {
        damageRecords.Clear();
        battleStartTime = Time.time;
    }
    public void AddDamage(BattleUnit attacker, int damage)
    {
        if (attacker == null || damage <= 0) return;
        //플레이어 영웅의 피해만 기록
        if (attacker.Team != UnitTeam.Hero) return;
        if (!damageRecords.TryGetValue(attacker, out List<DamageRecord> records))
        {
            records = new List<DamageRecord>();
            damageRecords.Add(attacker, records);
        }
        records.Add(new DamageRecord(Time.time, damage));
    }
    public float GetDps(BattleUnit unit)
    {
        if (unit == null) return 0.0f;
        if (!damageRecords.TryGetValue(unit, out List<DamageRecord> records)) return 0.0f;

        float minTime = Time.time - DpsWindow;
        int totalDamage = 0;
        for (int i = records.Count - 1; i >= 0; i--)
        {
            if (records[i].time < minTime)
            {
                records.RemoveAt(i);
                continue;
            }
            totalDamage += records[i].damage;
        }
        float battleTime = Time.time - battleStartTime;
        float calculateTime = Mathf.Min(DpsWindow, battleTime);
        if (calculateTime <= 0.0f) return 0.0f;
        return totalDamage / calculateTime;
    }
}
