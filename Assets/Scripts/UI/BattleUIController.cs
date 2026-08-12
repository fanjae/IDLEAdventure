using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 배틀 씬 화면 전환 담당함
// 진행 순서: 스테이지 선택 → 영웅 선택 → 영웅 배치 → 전투 → 전투 결과임
public class BattleUIController : MonoBehaviour
{
    public static BattleUIController Instance { get; private set; }

    [Header("전체 화면 패널")]
    [SerializeField] private GameObject stageSelectPanel;
    [SerializeField] private GameObject heroSelectPanel;
    [SerializeField] private GameObject formationPanel;
    [SerializeField] private GameObject battleHudPanel;

    [Header("팝업")]
    [SerializeField] private GameObject pausePopup;
    [SerializeField] private GameObject settingsPopup;
    [SerializeField] private GameObject battleResultPopup;

    [Header("씬 이동")]
    [SerializeField] private string mainSceneName = "MainScene";

    private readonly Stack<GameObject> pageHistory = new Stack<GameObject>();
    private GameObject[] pagePanels;
    private GameObject currentPage;
    private int selectedStageNumber = 1;

    private void Awake()
    {
        Instance = this;

        pagePanels = new[]
        {
            stageSelectPanel,
            heroSelectPanel,
            formationPanel,
            battleHudPanel
        };

        if (stageSelectPanel == null)
        {
            Debug.LogError("Stage Select Panel is not assigned.", this);
            return;
        }

        OpenStageSelect();
        ClosePausePopup();
        CloseSettingsPopup();
        CloseBattleResultPopup();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // 스테이지 선택 화면 엶
    public void OpenStageSelect()
    {
        pageHistory.Clear();
        ShowPage(stageSelectPanel);
        ClosePausePopup();
    }

    // 스테이지 번호 저장하고 영웅 선택 화면으로 넘김
    public void SelectStage(int stageNumber)
    {
        selectedStageNumber = Mathf.Max(1, stageNumber);
        OpenHeroSelection();
    }

    // 배치할 영웅 고르는 화면 엶
    public void OpenHeroSelection()
    {
        OpenPage(heroSelectPanel);
    }

    // 선택한 영웅 배치하는 화면 엶
    public void OpenFormation()
    {
        OpenPage(formationPanel);
    }

    // 현재 배치 확정하고 전투 HUD 엶
    public void StartBattle()
    {
        OpenPage(battleHudPanel);
    }

    // 전투 HUD 위에 결과 팝업 엶
    public void ShowBattleResult()
    {
        if (battleResultPopup != null)
            battleResultPopup.SetActive(true);

        ClosePausePopup();
    }

    // 현재 스테이지 같은 배치로 재도전함
    public void RetryBattle()
    {
        pageHistory.Clear();
        ShowPage(battleHudPanel);
        CloseBattleResultPopup();
        ClosePausePopup();
    }

    // 다음 스테이지로 영웅 선택부터 다시 진행함
    public void ChallengeNextStage()
    {
        selectedStageNumber++;
        pageHistory.Clear();
        ShowPage(heroSelectPanel);
        CloseBattleResultPopup();
        ClosePausePopup();
    }

    // 일시정지 팝업 엶
    public void OpenPausePopup()
    {
        if (pausePopup != null)
            pausePopup.SetActive(true);
    }

    // 일시정지 팝업 닫음
    public void ClosePausePopup()
    {
        if (pausePopup != null)
            pausePopup.SetActive(false);
    }

    // 설정 팝업 엶
    public void OpenSettingsPopup()
    {
        if (settingsPopup != null)
            settingsPopup.SetActive(true);
    }

    // 설정 팝업 닫음
    public void CloseSettingsPopup()
    {
        if (settingsPopup != null)
            settingsPopup.SetActive(false);
    }

    // 전투 결과 팝업 닫음
    public void CloseBattleResultPopup()
    {
        if (battleResultPopup != null)
            battleResultPopup.SetActive(false);
    }

    // 팝업 먼저 닫고 이전 화면으로 돌아감
    // 스테이지 선택 화면이면 메인 씬으로 돌아감
    public void GoBack()
    {
        if (settingsPopup != null && settingsPopup.activeSelf)
        {
            CloseSettingsPopup();
            return;
        }

        if (battleResultPopup != null && battleResultPopup.activeSelf)
        {
            CloseBattleResultPopup();
            return;
        }

        if (pausePopup != null && pausePopup.activeSelf)
        {
            ClosePausePopup();
            return;
        }

        if (pageHistory.Count > 0)
        {
            ShowPage(pageHistory.Pop());
            return;
        }

        ReturnToMain();
    }

    // 메인 씬으로 넘김
    public void ReturnToMain()
    {
        if (string.IsNullOrWhiteSpace(mainSceneName))
        {
            Debug.LogError("Main scene name is not configured.", this);
            return;
        }

        SceneManager.LoadScene(mainSceneName);
    }

    public int GetSelectedStageNumber()
    {
        return selectedStageNumber;
    }

    private void OpenPage(GameObject targetPage)
    {
        if (targetPage == null)
        {
            Debug.LogWarning("The target battle UI panel is not assigned.", this);
            return;
        }

        if (targetPage == currentPage)
            return;

        if (currentPage != null)
            pageHistory.Push(currentPage);

        ShowPage(targetPage);
        ClosePausePopup();
    }

    private void ShowPage(GameObject targetPage)
    {
        foreach (GameObject page in pagePanels)
        {
            if (page != null)
                page.SetActive(page == targetPage);
        }

        currentPage = targetPage;
    }
}
