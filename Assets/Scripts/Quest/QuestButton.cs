using TMPro;
using Unity.VisualScripting;
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
        if (QuestManager.Instance == null || PathManager.Instance == null) return;
        if (id == 0 || playerStateMachine == null) return;

        QuestData currentQuest = QuestManager.Instance.GetQuestData(id);
        if (currentQuest == null) return;

        QuestManager.Instance.DestroyNPC();

        PathManager.Instance.ShowLine(currentQuest.ArrivePosition);

        playerStateMachine.ChangeState(playerStateMachine.PlayerAutoState);

        // playerStateMachine을 통해 자동이동 명령 전달
        // PlayerAutoState가 도착했을 때 호출될 함수에서 사용될 함수를 람다식 + Action을 통해 전달
        playerStateMachine.PlayerAutoState.SetTarget(currentQuest.ArrivePosition, () =>
        {
            PathManager.Instance.HideLine();

            if (currentQuest.NPCPrefab != null)
            {
                GameObject spawnTarget = Instantiate(currentQuest.NPCPrefab, currentQuest.SpawnPosition, Quaternion.identity);
                
                if (spawnTarget.TryGetComponent<QuestNPCInteraction>(out var npc))
                {
                    npc.Initialize(id);
                    QuestManager.Instance.SetQuestNPC(npc);
                }
            }
            else
            {
                QuestManager.Instance.ClearQuest(id);
            }
        });
    }
}