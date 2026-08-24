using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class DpsManager : MonoBehaviour
{
    public static DpsManager Instance { get; private set; }

    private const float DpsWindow = 5.0f;

    private struct DamageRecord
    {
        public float time;
        public int damage;

        public DamageRecord(float time, int damage)
        {
            this.time = time;
            this.damage = damage;
        }
    }
    private class DpsData
    {
        public readonly Queue<DamageRecord> records = new Queue<DamageRecord>();
        public int recentDamage;
    }

    private readonly Dictionary<BattleUnit, DpsData> dpsDatas = new Dictionary<BattleUnit, DpsData>();
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
        dpsDatas.Clear();
        battleStartTime = Time.time;
    }
    public void AddDamage(BattleUnit attacker, int damage)
    {
        if (attacker == null || damage <= 0) return;
        //플레이어 영웅의 피해만 기록
        if (attacker.Team != UnitTeam.Hero) return;
        if (!dpsDatas.TryGetValue(attacker, out DpsData data))
        {
            data = new DpsData();
            dpsDatas.Add(attacker, data);
        }
        RemoveOldRecords(data);
        data.records.Enqueue(new DamageRecord(Time.time, damage));
        data.recentDamage += damage;
    }
    public float GetDps(BattleUnit unit)
    {
        if (unit == null) return 0.0f;
        if (!dpsDatas.TryGetValue(unit, out DpsData data)) return 0.0f;

        RemoveOldRecords(data);

        float battleTime = Time.time - battleStartTime;
        float calculateTime = Mathf.Min(DpsWindow, battleTime);
        if (calculateTime <= 0.0f) return 0.0f;
        return data.recentDamage / calculateTime;
    }
    private void RemoveOldRecords(DpsData data)
    {
        float minTime = Time.time - DpsWindow;
        while (data.records.Count > 0 && data.records.Peek().time < minTime)
        {
            DamageRecord oldRecord = data.records.Dequeue();
            data.recentDamage -= oldRecord.damage;
        }
        if (data.recentDamage < 0) data.recentDamage = 0;
    }
}
