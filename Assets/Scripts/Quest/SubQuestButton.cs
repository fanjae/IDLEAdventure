using UnityEngine;

/// <summary>
/// 메인 화면의 서브 퀘스트 UI 클릭 시 호출될 함수. <br/>
/// 수락 된 서브 퀘스트의 이름을 출력하고, 서브 퀘스트 목표로 이동 및 클리어 시 제거된다.
/// </summary>
public class SubQuestButton : QuestButton
{
    private int subQeustId = 0;

    private void Awake()
    {
        if (subQeustId == 0)
        {
            gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnSubQuestChanged += QuestClear;
        }
    }
    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnSubQuestChanged -= QuestClear;
        }
    }

    // 해당하는 슬롯 퀘스트 이름 갱신 함수.
    public void RefreshQuestUI(int id)
    {
        if (questNameText == null) return;

        subQeustId = id;
        QuestData currentQuest = QuestManager.Instance.GetQuestData(subQeustId);

        if (currentQuest != null)
        {
            questNameText.text = currentQuest.QuestName;
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // 서브 퀘스트 UI 클릭 시 호출될 함스.
    // 현재는 목표 위치로 이동만.
    public void OnClickButton()
    {
        if (subQeustId == 0) return;

        QuestMove(subQeustId);
    }

    private void QuestClear()
    {
        if (subQeustId == 0 || QuestManager.Instance == null) return;

        // 수락된 서브 퀘스트 리스트에 ID가 있는지 확인
        if (QuestManager.Instance.AcceptedSubQuestIds.Contains(subQeustId))
        {
            RefreshQuestUI(subQeustId);
        }
        else
        {
            subQeustId = 0;
            gameObject.SetActive(false);
        }
    }
}