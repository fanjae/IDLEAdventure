using TMPro;
using UnityEngine;

/// <summary>
/// 메인, 서브 퀘스트 버튼이 공통으로 사용할 함수 정의를 위한 추상 클래스.
/// </summary>
public abstract class QuestButton : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] protected TMP_Text questNameText;

    [Header("Player")]
    [SerializeField] AdventurePlayerStateMachine playerStateMachine;

    // 퀘스트 위치로 이동 명령을 내리는 함수.
    protected virtual void QuestMove(int id)
    {
        if (QuestManager.Instance == null || id == 0) return;

        QuestData currentQuest = QuestManager.Instance.GetQuestData(id);
        if (currentQuest == null) return;

        if (playerStateMachine == null) return;

        playerStateMachine.ChangeState(playerStateMachine.PlayerAutoState);

        // playerStateMachine을 통해 자동이동 명령 전달
        // PlayerAutoState가 도착했을 때 호출될 함수에서 사용될 함수를 람다식 + Action을 통해 전달
        playerStateMachine.PlayerAutoState.SetTarget(currentQuest.Target, () =>
        {
            GameObject spawnTarget = null;

            if (currentQuest.TargetPrefab != null)
            {
                spawnTarget = Instantiate(currentQuest.TargetPrefab, currentQuest.Target, Quaternion.identity);
            }

            QuestDialogue(id, currentQuest, spawnTarget);
        });
    }
    // 퀘스트 대사를 출력해주는 함수.
    protected virtual void QuestDialogue(int id, QuestData data, GameObject spawnTarget)
    {
        if (data.DialogueData != null && DialogueManager.Instance != null)
        {
            // 대화가 끝났을 때 실행될 함수 람다식 + Action을 통해 전달.
            DialogueManager.Instance.StartDialogue(data.DialogueData, () =>
            {
                if (spawnTarget != null)
                {
                    Destroy(spawnTarget);
                }

                // 대화가 끝났다면 클리어
                QuestManager.Instance.ClearQuest(id);
                QuestClear();
            });
        }
        else
        {
            // 대사가 없는데 NPC 데이터가 들어있다면 제거 후 클리어
            if (spawnTarget != null)
            {
                Destroy(spawnTarget);
            }

            QuestManager.Instance.ClearQuest(id);
            QuestClear();
        }
    }
    // 각 퀘스트가 클리어 됐을 때 추가로 할 게 있을 때 사용할 함수.
    protected abstract void QuestClear();
}