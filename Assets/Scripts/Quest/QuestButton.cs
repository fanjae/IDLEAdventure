using System.Collections.Generic;
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
        if (QuestManager.Instance == null || PathManager.Instance == null) return;
        if (id == 0 || playerStateMachine == null) return;

        QuestData currentQuest = QuestManager.Instance.GetQuestData(id);
        if (currentQuest == null) return;

        QuestManager.Instance.DestroyQuestTarget();

        PathManager.Instance.ShowLine(currentQuest.ArrivePosition);

        playerStateMachine.ChangeState(playerStateMachine.PlayerAutoState);

        // playerStateMachine을 통해 자동이동 명령 전달
        // PlayerAutoState가 도착했을 때 호출될 함수에서 사용될 함수를 람다식 + Action을 통해 전달
        playerStateMachine.PlayerAutoState.SetTarget(currentQuest.ArrivePosition, () =>
        {
            PathManager.Instance.HideLine();

            // 생성할 객체가 존재할 때
            if (currentQuest.InteractablePrefab != null)
            {
                // 퀘스트 종류가 채집이라면,
                if (currentQuest.QuestKind == QuestKind.Gather)
                {
                    // 생성할 위치들을 담을 리스트 생성.
                    List<Vector3> spawnedPositions = new List<Vector3>();
                    int spawnedCount = 0;   // 생성 된 객체 카운트.
                    int maxRetry = 100;     // 생성 시도 최대 횟수.

                    // 객체 생성.
                    while (spawnedCount < currentQuest.TargetCount)
                    {
                        int retryCount = 0;         // 생성 시도 횟수.
                        bool positionFound = false; // 생성 위치를 찾았는지 여부.
                        Vector3 randomPos = Vector3.zero;   // 랜덤 생성할 위치값을 담을 변수.

                        // 최대 시도 횟수만큼 생성 시도. (위치를 찾았다면 탈출)
                        while (retryCount < maxRetry && !positionFound)
                        {
                            // 위치 랜덤 생성.
                            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * currentQuest.SpawnRadius;
                            randomPos = currentQuest.SpawnPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
                            // 이전 위치와 겹치는지 확인
                            bool isOverlapping = false;
                            foreach (Vector3 pos in spawnedPositions)
                            {
                                if (Vector3.Distance(pos, randomPos) < currentQuest.MinSpawnDistance)
                                {
                                    isOverlapping = true;
                                    break;
                                }
                            }
                            // 위치를 잘 찾았다면 탈출.
                            if (!isOverlapping) positionFound = true;
                            retryCount++;
                        }
                        // 찾은 위치에 생성.
                        if (positionFound)
                        {
                            GameObject spawnTarget = Instantiate(currentQuest.InteractablePrefab, randomPos, Quaternion.identity);

                            if (spawnTarget.TryGetComponent<QuestInteractableObject>(out var target))
                            {
                                target.Initialize(id);
                                QuestManager.Instance.SetQuestTarget(target);
                            }

                            spawnedPositions.Add(randomPos);
                            spawnedCount++;
                        }
                        // 최대 시도 횟수만큼 돌려도 위치를 찾지 못 했다면 생성 실패.
                        else
                        {
                            Debug.LogWarning("채집물을 스폰할 공간이 부족합니다.");
                            break;
                        }
                    }
                }
                // 퀘스트 종류가 대화 or 전투라면
                // 스폰 위치에 객체 생성.
                else 
                {
                    Vector3 lookDirection = currentQuest.ArrivePosition - currentQuest.SpawnPosition;
                    lookDirection.y = 0.0f;

                    Quaternion spawnRotation = Quaternion.identity;
                    if (lookDirection.sqrMagnitude > 0.001f)
                    {
                        spawnRotation = Quaternion.LookRotation(lookDirection);
                    }

                    GameObject spawnTarget = Instantiate(currentQuest.InteractablePrefab, currentQuest.SpawnPosition, spawnRotation);

                    if (spawnTarget.TryGetComponent<QuestInteractableObject>(out var target))
                    {
                        target.Initialize(id);
                        QuestManager.Instance.SetQuestTarget(target);
                    }
                }
            }
            else
            {
                QuestManager.Instance.ClearQuest(id);
            }
        });
    }
}