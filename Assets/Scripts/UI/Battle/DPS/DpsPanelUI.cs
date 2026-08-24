using System.Collections.Generic;
using UnityEngine;

public class DpsPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform content;
    [SerializeField] private DpsRowUI rowPrefab;

    [SerializeField, Min(0.1f)] private float refreshInterval = 0.25f;

    private float nextRefreshTime;

    private readonly List<DpsRowUI> rows = new List<DpsRowUI>();

    private bool isOpen = true;

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
    }
    private void Update()
    {
        if (!isOpen) return;
        if (Time.unscaledTime < nextRefreshTime) return;

        nextRefreshTime = Time.unscaledTime + refreshInterval;
        RefreshRows();
    }

    private void RefreshRows()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] == null) continue;
            rows[i].Refresh();
        }
    }
    private void HandleBattleStarted()
    {
        if (BattleManager.Instance == null) return;
        CreateRows(BattleManager.Instance.HeroUnits);
    }
    private void CreateRows(IReadOnlyList<BattleUnit> heroes)
    {
        ClearRows();
        for (int i = 0; i < heroes.Count; i++)
        {
            BattleUnit hero = heroes[i];
            if (hero == null) continue;
            if (!hero.gameObject.activeInHierarchy) continue;

            DpsRowUI row = Instantiate(rowPrefab, content);
            row.Initialize(hero);
            rows.Add(row);
        }
    }
    private void ClearRows()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] != null) Destroy(rows[i].gameObject);
        }
        rows.Clear();
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;
        if (panel != null) panel.SetActive(isOpen);
        if (isOpen) nextRefreshTime = 0.0f;
    }
}
