using System;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private GameObject selectionNoticePanel;
    [SerializeField] private TMP_Text selectionNoticeText;

    //
    [SerializeField] private GameObject victory;
    [SerializeField] private GameObject defeat;
    //

    [Header("씬 이동")]
    [SerializeField] private string mainSceneName = "MainScene";

    //
    [SerializeField] private string battleSceneName = "BattleScene 2";
    //

    private readonly Stack<GameObject> pageHistory = new Stack<GameObject>();
    private readonly List<string> expeditionHeroIds = new List<string>();
    private GameObject[] pagePanels;
    private GameObject currentPage;
    private int selectedStageNumber = 1;
    private BattleHeroRosterPresenter heroSelectRoster;
    private BattleHeroRosterPresenter formationRoster;
    private string selectedFormationHeroId;
    private float selectionNoticeHideTime;

    public event Action<string> OnFormationHeroSelected;

    public string SelectedFormationHeroId => selectedFormationHeroId;
    public IReadOnlyList<string> ExpeditionHeroIds => expeditionHeroIds;

    //
    private static readonly List<string> runtimeHeroIds = new List<string>();
    //

    private void Awake()
    {
        Debug.Log($"[BattleUIController Awake] Scene = {gameObject.scene.name}");
        Instance = this;

        pagePanels = new[]
        {
            stageSelectPanel,
            heroSelectPanel,
            formationPanel,
            battleHudPanel
        };


        Debug.Log($"StageSelectPanel null = {stageSelectPanel == null}");
        Debug.Log($"FormationPanel null = {formationPanel == null}");

        //ConfigureHeroRosters();
        //
        //selectionNoticePanel?.SetActive(false);
        if (selectionNoticePanel != null)
        {
            selectionNoticePanel.SetActive(false);
        }
        //

        //
        //if (stageSelectPanel == null)
        //{
        //    Debug.LogError("Stage Select Panel is not assigned.", this);
        //    return;
        //}

        if (stageSelectPanel != null)
        {
            Debug.Log("BattleScene 진입 - Formation 켜지냐");
            ConfigureHeroRosters();
            OpenStageSelect();
        }
        else if (formationPanel != null)
        {
            ShowPage(formationPanel);
            ConfigureHeroRosters();

            //formationPanel.transform.parent.gameObject.SetActive(true);


            expeditionHeroIds.Clear();
            expeditionHeroIds.AddRange(runtimeHeroIds);

            formationRoster?.SetHeroFilter(expeditionHeroIds);
            formationRoster?.SetSelectedHeroIds(null);

            Debug.Log($"BattleScene Formation 진입 완료 / 영웅 수: {expeditionHeroIds.Count}");

        }

        //



       // OpenStageSelect();
        ClosePausePopup();
        CloseSettingsPopup();
        CloseBattleResultPopup();
    }

    private void OnEnable()
    {
        ConfigureHeroRosters();
    }

    private void OnDestroy()
    {
        UnsubscribeFromHeroRosters();

        //
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
        //


        if (Instance == this)
            Instance = null;
    }

    private void OnDisable()
    {
        UnsubscribeFromHeroRosters();
    }



    private void Start()
    {
        //
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded += HandleBattleEnded;
        }
        //

        if (formationPanel == null)
        {
            return;
        }

        Debug.Log(
            $"[Start 이후] Formation activeSelf = {formationPanel.activeSelf}, " +
            $"activeInHierarchy = {formationPanel.activeInHierarchy}",
            formationPanel);
    }





    private void UnsubscribeFromHeroRosters()
    {
        if (heroSelectRoster != null)
        {
            heroSelectRoster.OnSelectionChanged -= HandleExpeditionSelectionChanged;
            heroSelectRoster.OnSelectionDenied -= ShowSelectionNotice;
        }

        //if (formationRoster != null)
        //{
        //    formationRoster.OnSelectionChanged -= HandleFormationSelectionChanged;
        //}

        //
        if (formationRoster != null)
        {
            formationRoster.OnHeroClicked -= HandleFormationHeroClicked;
        }
        //
    }

    //
    private void HandleBattleStarted()
    {
        ShowPage(battleHudPanel);
    }
    //

    //

    private void HandleBattleEnded(UnitTeam winner)
    {
        if (victory != null)
        {
            victory.SetActive(winner == UnitTeam.Hero);
        }

        if (defeat != null)
        {
            defeat.SetActive(winner == UnitTeam.Enemy);
        }

        ShowBattleResult();
    }

    //

    // 스테이지 선택 화면 엶
    public void OpenStageSelect()
    {
        //
        if (stageSelectPanel == null)
        {
            return;
        }

        //

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
        heroSelectRoster?.ClearHeroFilter();
        heroSelectRoster?.SetSelectedHeroIds(expeditionHeroIds);
        OpenPage(heroSelectPanel);
    }

    // 선택한 영웅 배치하는 화면 엶
    public void OpenFormation()
    {
        if (expeditionHeroIds.Count == 0)
        {
            ShowSelectionNotice("최소 1명의 영웅을 원정대에 편성해야 합니다.");
            return;
        }

        //
        if (formationPanel == null)
        {
            StageRuntimeData.SelectStage(selectedStageNumber);

            runtimeHeroIds.Clear();
            runtimeHeroIds.AddRange(expeditionHeroIds);

            SceneManager.LoadScene(battleSceneName);
            return;
        }
        //

        formationRoster?.SetHeroFilter(expeditionHeroIds);
        formationRoster?.SetSelectedHeroIds(null);
        SetSelectedFormationHero(null);
        OpenPage(formationPanel);
    }

    // 현재 배치 확정하고 전투 HUD 엶
    public void StartBattle()
    {
        //OpenPage(battleHudPanel);

        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager를 찾을 수 없습니다.", this);
            return;
        }

        BattleManager.Instance.StartBattle();

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
        OpenHeroSelection();
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
        //foreach (GameObject page in pagePanels)
        //{
        //    if (page != null)
        //        page.SetActive(page == targetPage);
        //}

        //currentPage = targetPage;

        Debug.Log($"[ShowPage 호출] Target = {(targetPage != null ? targetPage.name : "NULL")}");

        foreach (GameObject page in pagePanels)
        {
            if (page == null)
            {
                continue;
            }

            bool isActive = page == targetPage;

            Debug.Log($"[ShowPage] {page.name} → {isActive}");

            page.SetActive(isActive);
        }

        currentPage = targetPage;
    }

    private void ConfigureHeroRosters()
    {
        UnsubscribeFromHeroRosters();

        heroSelectRoster = heroSelectPanel != null
            ? heroSelectPanel.GetComponent<BattleHeroRosterPresenter>()
            : null;
        formationRoster = formationPanel != null
            ? formationPanel.GetComponent<BattleHeroRosterPresenter>()
            : null;

        if (heroSelectRoster != null)
        {
            heroSelectRoster.OnSelectionChanged += HandleExpeditionSelectionChanged;
            heroSelectRoster.OnSelectionDenied += ShowSelectionNotice;
        }

        //if (formationRoster != null)
        //{
        //    formationRoster.OnSelectionChanged += HandleFormationSelectionChanged;
        //}

        //
        if (formationRoster != null)
        {
            formationRoster.OnHeroClicked += HandleFormationHeroClicked;
        }
        //
    }


    private void HandleExpeditionSelectionChanged(IReadOnlyList<string> selectedHeroIds)
    {
        expeditionHeroIds.Clear();
        expeditionHeroIds.AddRange(selectedHeroIds);
    }


    //
    private void HandleFormationHeroClicked(string heroId)
    {
        OnFormationHeroSelected?.Invoke(heroId);
    }
    //
    

    //
    //private void HandleFormationSelectionChanged(IReadOnlyList<string> selectedHeroIds)
    //{
    //    SetSelectedFormationHero(selectedHeroIds.Count > 0 ? selectedHeroIds[0] : null);
    //}

    private void SetSelectedFormationHero(string heroId)
    {
        //selectedFormationHeroId = heroId;

        //if (!string.IsNullOrEmpty(heroId))
        //{
        //    OnFormationHeroSelected?.Invoke(heroId);
        //}
        if (selectedFormationHeroId == heroId)
        {
            return;
        }

        selectedFormationHeroId = heroId;
        OnFormationHeroSelected?.Invoke(selectedFormationHeroId);
    }
    //

    private void Update()
    {
        if (selectionNoticePanel != null && selectionNoticePanel.activeSelf && Time.unscaledTime >= selectionNoticeHideTime)
        {
            selectionNoticePanel.SetActive(false);
        }
    }

    private void ShowSelectionNotice(string message)
    {
        if (selectionNoticePanel == null || selectionNoticeText == null)
        {
            return;
        }

        selectionNoticeText.text = message;
        selectionNoticePanel.SetActive(true);
        selectionNoticeHideTime = Time.unscaledTime + 2f;
    }
}
