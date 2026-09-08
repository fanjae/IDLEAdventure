using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BattlePauseController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseAllPanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text stageInfoText;

    [Header("씬 이동")]
    [SerializeField] private string mainSceneName = "Filed_Persistent";

    private bool isPaused;

    private void Start()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseBattle);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(ResumeBattle);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(GiveUpBattle);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 전투 시작 전에는 일시정지 UI 숨김
        if (pauseAllPanel != null)
        {
            pauseAllPanel.SetActive(false);
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded += HandleBattleEnded;

            // 이미 전투가 시작된 이후 Start가 호출되는 상황까지 대응
            if (BattleManager.Instance.IsBattleRunning)
            {
                HandleBattleStarted();
            }
        }

        isPaused = false;
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseBattle);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(ResumeBattle);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(GiveUpBattle);
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }

        RestoreTimeScale();
    }

    private void HandleBattleStarted()
    {
        if (pauseAllPanel != null)
        {
            pauseAllPanel.SetActive(true);
        }
    }

    private void HandleBattleEnded(UnitTeam winner)
    {
        RestoreTimeScale();
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseAllPanel != null)
        {
            pauseAllPanel.SetActive(false);
        }
    }

    public void PauseBattle()
    {
        if (isPaused || BattleManager.Instance == null || !BattleManager.Instance.IsBattleRunning)
        {
            return;
        }

        RefreshStageInfoText();

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeBattle()
    {
        if (!isPaused)
        {
            return;
        }

        RestoreTimeScale();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        isPaused = false;
    }

    public void GiveUpBattle()
    {
        RestoreTimeScale();

        StageRuntimeData.StopAutoBattle();

        if (StageRuntimeData.IsFieldEnemyBattle)
        {
            FieldEnemyRuntimeData.ClearEnemyData();
            StageRuntimeData.StopFieldEnemyBattle();
        }

        SceneManager.LoadScene(mainSceneName);
    }

    private void RefreshStageInfoText()
    {
        if (stageInfoText == null)
        {
            return;
        }

        int currentStageId = StageRuntimeData.SelectedStageId;
        stageInfoText.text = $"현재 진행 스테이지 : {currentStageId}";
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
    }
}