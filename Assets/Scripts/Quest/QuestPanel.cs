using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 퀘스트 패널 전체를 관리하는 클래스.
/// </summary>
public class QuestPanel : MonoBehaviour
{
    private enum OpenSource
    {
        Main,
        AllMenu
    }

    // Quest Panel 이벤트(0828 UI 연결 용으로 추가)
    public event Action OnClosed;

    // 2026.09.04 필드 Quest UI 종료 후 MainBottomPanel 복구를 위해 추가
    public event Action OnMainClosed;

    private OpenSource openSource = OpenSource.Main;

    [Header("UI Component")]
    [SerializeField] private GameObject questPanel;

    // 2026.09.04 퀘스트 패널 오픈/클로즈 연출 적용
    [SerializeField] private UIPanelTransition panelTransition;

    [Header("Main Quest")]
    [SerializeField] private TMP_Text mainQuestNameText;
    [SerializeField] private MainQuestButton mainQuestButton;

    [Header("Sub Quest")]
    [SerializeField] private Transform subQuestParent;
    [SerializeField] private GameObject subQuestSlot;
    [SerializeField] private SubQuestButton[] subQuestButtons;

    private List<GameObject> subQuestSlots = new List<GameObject>();

    private void Awake()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnSubQuestChanged += RefreshPanel;
        }

        RefreshPanel();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnSubQuestChanged -= RefreshPanel;
        }
    }

    // 2026.09.04 메인 화면에서 퀘스트 패널 진입
    public void OpenQuestPanel()
    {
        openSource = OpenSource.Main;

        if (questPanel == null)
        {
            return;
        }

        questPanel.SetActive(true);
        RefreshPanel();
        panelTransition?.PlayOpen();
    }

    // 2026.09.04 전체 메뉴에서 퀘스트 패널 진입
    public void OpenQuestPanelFromAllMenu()
    {
        openSource = OpenSource.AllMenu;

        if (questPanel == null)
        {
            return;
        }

        questPanel.SetActive(true);
        RefreshPanel();
        panelTransition?.PlayOpen();
    }

    // 기존 외부 참조 호환용
    public void CloseQuestPanelToAllMenu()
    {
        openSource = OpenSource.AllMenu;
        CloseQuestPanelWithSource();
    }

    public void CloseQuestPanel()
    {
        if (questPanel == null)
        {
            return;
        }

        if (panelTransition == null)
        {
            questPanel.SetActive(false);
            return;
        }

        panelTransition.PlayClose(() => questPanel.SetActive(false));
    }

    // 2026.09.04 퀘스트 진입 경로에 따른 종료 처리
    public void OnClickCloseButton()
    {
        CloseQuestPanelWithSource();
    }

    // 퀘스트 현황 패널에서 메인 퀘스트 버튼을 눌렀을 때 호출될 함수.
    // 패널을 닫고 메인 퀘스트 UI 클릭 함수 호출.
    public void OnClickMainQuest()
    {
        if (QuestManager.Instance == null) return;

        int mainQuestId = QuestManager.Instance.CurrentMainQuestId;
        QuestData mainQuestData = QuestManager.Instance.GetQuestData(mainQuestId);
        if (mainQuestData == null) return;

        CloseQuestPanelWithSource();

        if (mainQuestButton != null)
        {
            mainQuestButton.OnClickQuestButton();
        }
    }

    // 퀘스트 현황판에서 서브 퀘스트를 클릭했을 때 호출될 함수.
    // 최대 수락 가능한 만큼 수락을 하고, 서브 퀘스트 UI 클릭 함수 호출.
    public void OnClickSubQeustSlot(int id, bool isAccepted)
    {
        if (QuestManager.Instance == null) return;

        if (isAccepted)
        {
            CloseQuestPanelWithSource();
            AcceptSubQuest(id);
        }
        else
        {
            if (QuestManager.Instance.AcceptedSubQuestIds.Count >= 2)
            {
                Debug.Log("더 이상 퀘스트 수락이 불가능합니다.");
                return;
            }

            QuestManager.Instance.AcceptSubQuest(id);

            CloseQuestPanelWithSource();
            AcceptSubQuest(id);
        }
    }

    // 서브 퀘스트 수락 함수.
    // 메인 화면의 서브 퀘스트 버튼 상태를 갱신하고 클릭 명령을 내린다.
    private void AcceptSubQuest(int id)
    {
        for (int i = 0; i < subQuestButtons.Length; i++)
        {
            if (!subQuestButtons[i].gameObject.activeSelf)
            {
                subQuestButtons[i].RefreshQuestUI(id);
                subQuestButtons[i].OnClickButton();
                return;
            }
        }
    }

    // 서브 퀘스트가 클리어 됐을 때 슬롯 제거 함수.
    private void ClearSubQuestSlots()
    {
        foreach (var slot in subQuestSlots)
        {
            if (slot != null)
            {
                Destroy(slot);
            }
        }

        subQuestSlots.Clear();
    }

    // 퀘스트 현황판을 갱신하는 함수.
    public void RefreshPanel()
    {
        if (QuestManager.Instance == null) return;
        if (mainQuestNameText == null) return;

        int mainQuestId = QuestManager.Instance.CurrentMainQuestId;
        QuestData mainQuestData = QuestManager.Instance.GetQuestData(mainQuestId);

        if (mainQuestData != null)
        {
            mainQuestNameText.text = mainQuestData.QuestName;
        }
        else
        {
            mainQuestNameText.text = "None";
        }

        ClearSubQuestSlots();

        List<int> acceptedSubQuestIds = QuestManager.Instance.AcceptedSubQuestIds;
        foreach (int subQuestId in acceptedSubQuestIds)
        {
            RefreshSubQuestSlot(subQuestId, true);
        }

        List<int> acceptableSubQuestIds = QuestManager.Instance.AcceptableSubQuestIds;
        foreach (int subQuestId in acceptableSubQuestIds)
        {
            RefreshSubQuestSlot(subQuestId, false);
        }
    }

    // 서브 퀘스트 슬롯을 갱신하는 함수.
    // 수락한 퀘스트를 메인 화면에 추가한다.
    private void RefreshSubQuestSlot(int id, bool isAccepted)
    {
        if (subQuestSlot == null || subQuestParent == null) return;

        GameObject slot = Instantiate(subQuestSlot, subQuestParent);
        subQuestSlots.Add(slot);

        QuestPanelSlot slotScript = slot.GetComponent<QuestPanelSlot>();
        if (slotScript == null) return;

        slotScript.SetSlotUI(id, isAccepted, (selectedId, accepted) =>
        {
            OnClickSubQeustSlot(selectedId, accepted);
        });
    }

    // 2026.09.04 퀘스트 패널 종료 연출 및 진입 화면 복구
    private void CloseQuestPanelWithSource()
    {
        if (questPanel == null)
        {
            return;
        }

        if (panelTransition == null)
        {
            CompleteCloseWithSource();
            return;
        }

        panelTransition.PlayClose(CompleteCloseWithSource);
    }

    // 2026.09.04 닫기 연출 완료 후 패널 비활성화 및 이전 화면 복구
    private void CompleteCloseWithSource()
    {
        questPanel.SetActive(false);

        if (openSource == OpenSource.AllMenu)
        {
            OnClosed?.Invoke();
            return;
        }

        OnMainClosed?.Invoke();
    }
}