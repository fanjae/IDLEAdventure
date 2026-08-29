using UnityEngine;

/// <summary>
/// 메인 화면의 메인 퀘스트 UI 클릭 시 호출될 함수. <br/>
/// 현재 진행 중인 퀘스트 이름을 출력하고, 클릭 시 해당 위치로 이동하며, 클리어 시 갱신된다.
/// </summary>
public class MainQuestButton : QuestButton
{
    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnMainQuestChanged += RefreshQuestName;
        }
        RefreshQuestName();
    }
    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnMainQuestChanged -= RefreshQuestName;
        }
    }

    // 메인 퀘스트 이름 갱신 함수.
    public void RefreshQuestName()
    {
        if (QuestManager.Instance == null) return;
        if (questNameText == null) return;

        int mainQuestId = QuestManager.Instance.CurrentMainQuestId;
        QuestData currentQuest = QuestManager.Instance.GetQuestData(mainQuestId);

        if (currentQuest != null)
        {
            questNameText.text = currentQuest.QuestName;
        }
        else
        {
            questNameText.text = "None";
        }
    }

    // 메인 퀘스트 UI 클릭 시 호출될 함수.
    // 현재는 목표 위치로 이동만.
    public void OnClickQuestButton()
    {
        if (QuestManager.Instance == null) return;

        int mainQuestId = QuestManager.Instance.CurrentMainQuestId;

        QuestMove(mainQuestId);
    }
}