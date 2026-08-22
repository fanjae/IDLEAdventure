using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 퀘스트 현황판에 나타날 슬롯 한 칸을 담당해줄 클래스.
/// </summary>
public class QuestPanelSlot : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text isAcceptedText;

    private int questId;
    private bool isAccepted;
    private Action<int, bool> onClicked;

    // 현 슬롯에 들어갈 데이터를 세팅해주는 함수.
    public void SetSlotUI(int questId, bool isAccepted, Action<int, bool> onClicked)
    {
        if (questNameText == null) return;
        if (isAcceptedText == null) return;

        this.questId = questId;
        this.isAccepted = isAccepted;
        this.onClicked = onClicked;

        QuestData data = QuestManager.Instance.GetQuestData(questId);
        if (data == null) return;

        questNameText.text = data.QuestName;
        isAcceptedText.text = isAccepted ? "[Accepted]" : "[Acceptable]";
    }
    // 현 슬롯이 클릭되었을 때 호출될 함수.
    // 슬롯을 세팅할 때 받아놓은 함수를 실행한다.
    public void OnSlotClick()
    {
        if (questId == 0) return;

        onClicked?.Invoke(questId, isAccepted);
    }
}