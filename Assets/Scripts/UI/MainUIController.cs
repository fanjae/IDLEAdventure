using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 메인 UI 페이지 전환, 뒤로가기, 팝업 표시 담당함
// 영웅 배치는 전투 씬에서 진행함
public class MainUIController : MonoBehaviour
{
    public static MainUIController Instance { get; private set; }

    [Header("Full Pages")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject heroesDictionaryPanel;
    [SerializeField] private GameObject gachaPanel;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject resonancePanel;
    [SerializeField] private GameObject shopPanel;

    [Header("Modal Popups")]
    [SerializeField] private GameObject idleRewardsPopup;
    [SerializeField] private GameObject settingsPopup;

    [Header("Scene Navigation")]
    [SerializeField] private string adventureBattleSceneName = "BattleScene";

    private GameObject[] pagePanels;
    private GameObject[] popupPanels;
    private readonly Stack<GameObject> pageHistory = new Stack<GameObject>();
    private GameObject currentPage;

    private void Awake()
    {
        Instance = this;

        pagePanels = new[]
        {
            homePanel,
            heroesDictionaryPanel,
            gachaPanel,
            equipmentPanel,
            resonancePanel,
            shopPanel
        };

        popupPanels = new[]
        {
            idleRewardsPopup,
            settingsPopup
        };

        pageHistory.Clear();
        ShowPage(homePanel);
        CloseAllPopups();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // 새 전체 페이지 열고 현재 페이지는 뒤로가기 기록에 넣음
    private void OpenPage(GameObject targetPage)
    {
        if (targetPage == null || currentPage == targetPage)
            return;

        if (currentPage != null)
            pageHistory.Push(currentPage);

        ShowPage(targetPage);
        CloseAllPopups();
    }

    // 페이지 기록 없이 대상 페이지만 표시함
    private void ShowPage(GameObject targetPage)
    {
        foreach (GameObject page in pagePanels)
        {
            if (page != null)
                page.SetActive(page == targetPage);
        }

        currentPage = targetPage;
    }

    private void OpenPopup(GameObject targetPopup)
    {
        if (targetPopup != null)
            targetPopup.SetActive(true);
    }

    // 열린 팝업 먼저 닫고 없으면 직전 전체 페이지로 돌아감
    public void GoBack()
    {
        if (CloseActivePopup())
            return;

        if (pageHistory.Count > 0)
        {
            ShowPage(pageHistory.Pop());
            return;
        }

        if (currentPage != homePanel)
            ShowPage(homePanel);
    }

    private bool CloseActivePopup()
    {
        for (int i = popupPanels.Length - 1; i >= 0; i--)
        {
            GameObject popup = popupPanels[i];
            if (popup != null && popup.activeSelf)
            {
                popup.SetActive(false);
                return true;
            }
        }

        return false;
    }

    // 홈으로 이동하고 뒤로가기 기록 비움
    public void OpenHome()
    {
        pageHistory.Clear();
        ShowPage(homePanel);
        CloseAllPopups();
    }

    // 기존 영웅 버튼 호환용임. 영웅 사전 열어줌
    public void OpenHeroes() => OpenHeroesDictionary();
    public void OpenHeroesDictionary() => OpenPage(heroesDictionaryPanel);
    public void OpenGacha() => OpenPage(gachaPanel);
    public void OpenEquipment() => OpenPage(equipmentPanel);
    public void OpenResonance() => OpenPage(resonancePanel);
    public void OpenShop() => OpenPage(shopPanel);

    // 모험 버튼은 배틀 씬으로 넘김
    // 배틀 씬에서 스테이지 선택 패널 처음 표시함
    public void OpenAdventure() => LoadBattleScene();

    public void OpenIdleRewards() => OpenPopup(idleRewardsPopup);
    public void OpenSettings() => OpenPopup(settingsPopup);

    public void CloseIdleRewards() => ClosePopup(idleRewardsPopup);
    public void CloseSettings() => ClosePopup(settingsPopup);

    public void ClosePopup(GameObject targetPopup)
    {
        if (targetPopup != null)
            targetPopup.SetActive(false);
    }

    public void CloseAllPopups()
    {
        foreach (GameObject popup in popupPanels)
        {
            if (popup != null)
                popup.SetActive(false);
        }
    }

    // 기존 스테이지 입장 버튼 호환용임
    // OpenAdventure와 같게 배틀 씬으로 넘김
    public void StartSelectedStageBattle()
    {
        LoadBattleScene();
    }

    private void LoadBattleScene()
    {
        if (string.IsNullOrWhiteSpace(adventureBattleSceneName))
        {
            Debug.LogError("Adventure battle scene name is not configured.", this);
            return;
        }

        SceneManager.LoadScene(adventureBattleSceneName);
    }

}
