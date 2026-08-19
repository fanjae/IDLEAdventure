using TMPro;
using UnityEngine;

/// <summary>
/// 메인 퀘스트 진행을 테스트해보기 위한 클래스. <br/>
/// 메인 퀘스트 UI의 Text 내용을 현재 진행 중인 퀘스트 이름으로 출력하고, 클릭 시 해당 퀘스트 위치로 이동 명령. <br/>
/// 가지고 있는 정보 <br/>
/// QuestNameText, AdventurePlayerStateMachine
/// </summary>
public class QuestTest : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private TMP_Text questNameText;

    [Header("Player Move")]
    [SerializeField] AdventurePlayerStateMachine playerStateMachine;

    private void OnEnable()
    {
        RefreshQuestName();
    }

    // 메인 퀘스트 UI 갱신 함수.
    // 현재 진행 중인 메인 퀘스트 데이터를 받아와 이름을 출력한다.
    private void RefreshQuestName()
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
    // 메인 퀘스트 UI 클릭 함수.
    // 메인 퀘스트 데이터에 저장되어 있는 위치로 이동.
    // 다이얼로그 데이터 유무 확인 후 출력 함수 전달.
    public void OnClickMainQuestButton()
    {
        if (QuestManager.Instance == null) return;

        int mainQuestId = QuestManager.Instance.CurrentMainQuestId;
        QuestData currentQuest = QuestManager.Instance.GetQuestData(mainQuestId);

        if (currentQuest == null)
        {
            Debug.Log("현재 진행 중인 메인 퀘스트가 없습니다.");
            return;
        }

        if (playerStateMachine == null)
        {
            Debug.Log("플레이어 컴포넌트가 연결되어 있지 않습니다.");
            return;
        }

        playerStateMachine.ChangeState(playerStateMachine.PlayerAutoState);

        // playerStateMachine을 통해 자동이동 명령 전달
        // PlayerAutoState가 도착했을 때 호출될 함수에서 사용될 함수를 람다식 + Action을 통해 전달
        playerStateMachine.PlayerAutoState.SetTarget(currentQuest.Target, () =>
        {
            if (currentQuest.DialogueData != null)
            {
                foreach (var line in currentQuest.DialogueData.DialogueDatas)
                {
                    Debug.Log($"[{line.SpeakerName}]: {line.DialogueText}");
                }
            }

            QuestManager.Instance.ClearQuest(mainQuestId);
            RefreshQuestName();
        });
    }
}