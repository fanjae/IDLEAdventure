using System.Collections.Generic;
using UnityEngine;

public class DpsPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject toggleButton;
    [SerializeField] private Transform content;
    [SerializeField] private DpsRowUI rowPrefab;

    [SerializeField, Min(0.1f)] private float refreshInterval = 0.25f;

    private float nextRefreshTime;

    private readonly List<DpsRowUI> rows = new List<DpsRowUI>();

    private bool isOpen = true;
    private bool isBattleActive;

    private void Start()
    {
        // 2026.09.04 DPS UI는 실제 전투 중에만 표시
        SetBattleUIVisible(false);

        if (BattleManager.Instance == null)
        {
            return;
        }

        BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        BattleManager.Instance.OnBattleEnded += HandleBattleEnded;

        if (BattleManager.Instance.IsBattleRunning)
        {
            HandleBattleStarted();
        }
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
    }

    private void Update()
    {
        if (!isBattleActive || !isOpen) return;
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

        isBattleActive = true;
        isOpen = true;

        SetBattleUIVisible(true);
        CreateRows(BattleManager.Instance.HeroUnits);

        nextRefreshTime = 0f;
    }

    // 2026.09.04 전투 결과 연출 및 결과 패널 표시 중 DPS UI를 숨김
    private void HandleBattleEnded(UnitTeam winner)
    {
        isBattleActive = false;
        isOpen = false;

        SetBattleUIVisible(false);
        ClearRows();
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

    private void SetBattleUIVisible(bool visible)
    {
        if (toggleButton != null)
        {
            toggleButton.SetActive(visible);
        }

        if (panel != null)
        {
            panel.SetActive(visible && isOpen);
        }
    }

    public void TogglePanel()
    {
        if (!isBattleActive)
        {
            return;
        }

        isOpen = !isOpen;

        if (panel != null)
        {
            panel.SetActive(isOpen);
        }

        if (isOpen)
        {
            nextRefreshTime = 0f;
        }
    }
}