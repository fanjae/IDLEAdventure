using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 진행을 관리해줄 클래스. <br/>
/// 퀘스트 수락 - 진행 - 클리어 흐름을 전반적으로 담당한다. <br/>
/// 특정 경로의 SO 파일들을 읽어와 퀘스트 딕셔너리에 추가해 관리한다. <br/>
/// 가지고 있는 정보 <br/>
/// Main + Sub QuestDatas, CurrentMainQuestID, AcceptSubQuestIDs
///
/// <br>추가적으로 확인해볼 부분들.</br><br/>
/// 1. 현재는 직접 매개 변수로 퀘스트 ID를 받아오는 방식으로 해당 ID를 가진 퀘스트의 수락/클리어 수행을 하도록 구현을 해두었는데,
/// 실제 사용을 할 때 어떻게 될 지를 테스트 해봐야 함수 로직을 확정할 수 있을 것 같다. <br/>
/// 2. 배틀씬으로 넘어가 전투를 승리했을 때 퀘스트 완료 하는 로직 구상 필요 <br/>
/// 3. 보상은 퀘스트 ID에 따라 메인과 서브 구분이 자연스럽게 되니까 클리어 조건문 모두 통과하고 보상 지급 함수를 추가하면 좋아보이긴 하는데... 어떨까?
/// </summary>
public class QuestManager : Singleton<QuestManager>
{
    private Dictionary<int, QuestData> questDatas = new Dictionary<int, QuestData>();

    [SerializeField] private int currentMainQuestId;
    [SerializeField] private List<int> acceptSubQuestIds = new List<int>();

    // 프로퍼티
    public int CurrentMainQuestId => currentMainQuestId;
    public List<int> AcceptSubQuestIds => acceptSubQuestIds;

    protected override void Awake()
    {
        base.Awake();

        InitializeQuests();
    }

    // 퀘스트 초기화 함수.
    // SO로 생성해둔 퀘스트 데이터를 questDatas에 받아온다.
    private void InitializeQuests()
    {
        // 경로 저장
        string[] questPaths = { "GameData/Quests/Main", "GameData/Quests/Sub" };
        // 각 경로의 SO 데이터들을 저장
        foreach (string path in questPaths)
        {
            QuestData[] quests = Resources.LoadAll<QuestData>(path);
            if (quests == null || quests.Length == 0)
            {
                Debug.Log("퀘스트 데이터를 받아오지 못 했습니다.");
                continue;
            }
            // 받아온 퀘스트 데이터들을 questDats에 저장
            foreach (QuestData quest in quests)
            {
                if (!questDatas.ContainsKey(quest.QuestId))
                {
                    questDatas.Add(quest.QuestId, quest);
                }
                else
                {
                    Debug.Log($"{quest.QuestId} (은)는 중복된 퀘스트 ID입니다.");
                }
            }
        }
        // 코드가 중복된 것 같아서 합쳐봤는데 합친 코드에서 오류가 생기면 복구하기 위해 남겨둠. >>> 못 접어두나?
        //// 지정된 경로에서 메인 퀘스트 데이터 받아오기.
        //QuestData[] mainQuests = Resources.LoadAll<QuestData>("GameData/Quests/Main");
        //if (mainQuests == null)
        //{
        //    Debug.Log("메인 퀘스트를 받아오지 못 했습니다.");
        //    return;
        //}
        //// 받아온 퀘스트 데이터를 퀘스트 정보를 담아둘 딕셔너리에 퀘스트 ID를 Key값으로 저장.
        //foreach (QuestData quest in mainQuests)
        //{
        //    if (!questDatas.ContainsKey(quest.QuestId))
        //    {
        //        questDatas.Add(quest.QuestId, quest);
        //    }
        //    else
        //    {
        //        Debug.Log($"{quest.QuestId} (은)는 중복된 퀘스트 Id입니다.");
        //    }
        //}
        //// 지정된 경로에서 서브 퀘스트 데이터 받아오기.
        //QuestData[] subQuests = Resources.LoadAll<QuestData>("GameData/Quests/Sub");
        //if (subQuests == null)
        //{
        //    Debug.Log("서브 퀘스트를 받아오지 못 했습니다.");
        //    return;
        //}
        //// 받아온 퀘스트 데이터를 퀘스트 정보를 담아둘 딕셔너리에 퀘스트 ID를 Key값으로 저장.
        //foreach (QuestData quest in subQuests)
        //{
        //    if (!questDatas.ContainsKey(quest.QuestId))
        //    {
        //        questDatas.Add(quest.QuestId, quest);
        //    }
        //    else
        //    {
        //        Debug.Log($"{quest.QuestId} (은)는 중복된 퀘스트 Id입니다.");
        //    }
        //}
        
        LoadSaveData();
        Debug.Log("퀘스트 초기화 완료.");
    }

    // 퀘스트 정보를 받아오는 함수.
    // 퀘스트 ID를 매개 변수로 받아와 해당 ID값을 가진 퀘스트 데이터를 반환한다.
    public QuestData GetQuestData(int id)
    {
        if (questDatas.TryGetValue(id, out QuestData quest))
        {
            return quest;
        }

        Debug.Log("해당 ID 값을 가진 퀘스트가 없습니다.");
        return null;
    }
    // 퀘스트 수락 함수.
    // 퀘스트 수락은 서브 퀘스트만 존재한다.
    // 메인 퀘스트는 게임 시작과 함께 자동 수락되고, 이후 클리어 시 자동으로 다음 퀘스트가 수락되는 형태를 취하게끔 구현할 생각.
    // 수락 하려는 서브 퀘스트 ID를 매개 변수로 받아와 해당 ID값을 acceptSubQuestIds에 담는다.
    public void AcceptSubQuest(int id)
    {
        QuestData quest = GetQuestData(id);
        if (quest == null) return;

        if (quest.QuestType == QuestType.Sub)
        {
            if (!acceptSubQuestIds.Contains(id))
            {
                acceptSubQuestIds.Add(id);
                UpdateSaveData();
                Debug.Log($"{id} 서브 퀘스트를 수락했습니다.");
            }
        }
    }
    // 퀘스트 클리어 함수.
    // 클리어 할 퀘스트가 Main인지 Sub인지 확인하고,
    // Main이라면 바로 다음 번호의 퀘스트로 진행.
    // Sub라면 해당 퀘스트 ID를 수락 중인 퀘스트 리스트에서 제거.
    // 보상 부분은 어디에 추가할지 아직 고민 중.
    public void ClearQuest(int id)
    {
        QuestData quest = GetQuestData(id);
        if (quest == null) return;

        if (quest.QuestType == QuestType.Main)
        {
            if (currentMainQuestId == id)
            {
                Debug.Log($"{id} 메인 퀘스트를 클리어 했습니다.");

                // 보상 수령은 여기서 하는 게 좋을까?
                currentMainQuestId++;

                Debug.Log($"{id} 메인 퀘스트를 자동 수락했습니다.");
            }
            else
            {
                Debug.Log("완료 시도하는 퀘스트와 ID가 다릅니다.");
            }
        }

        else if (quest.QuestType == QuestType.Sub)
        {
            if (acceptSubQuestIds.Contains(id))
            {
                Debug.Log($"{id} 서브 퀘스트를 클리어 했습니다.");

                // 마찬가지로 보상 수령은 여기서 하는 게 좋을까?
                acceptSubQuestIds.Remove(id);
            }
            else
            {
                Debug.Log("완료 시도하는 퀘스트와 ID가 다릅니다.");
            }
        }

        else
        {
            Debug.Log("유효한 퀘스트 타입이 아닙니다.");
            return;
        }

        // 아니면 외부 데이터 + 퀘스트 ID값을 통해 보상을 제공할 테니 모든 걸 빠져나온 후 최종적으로 지급하는 게 좋을까?
        
        UpdateSaveData();
    }

    // 테스트용 퀘스트 진행도 저장 함수.
    private void LoadSaveData()
    {
        if (TestSaveManager.Instance != null && TestSaveManager.Instance.CurrentSaveData != null)
        {
            currentMainQuestId = TestSaveManager.Instance.CurrentSaveData.CurrentMainQuestId;
            acceptSubQuestIds = new List<int>(TestSaveManager.Instance.CurrentSaveData.AcceptSubQuestIds);
            Debug.Log($"퀘스트 진행도 로드 완료.");
        }
    }
    // 테스트용 퀘스트 진행도 불러오는 함수.
    private void UpdateSaveData()
    {
        if (TestSaveManager.Instance != null)
        {
            TestSaveManager.Instance.CurrentSaveData.SetQuestData(currentMainQuestId, acceptSubQuestIds);
            TestSaveManager.Instance.SaveGame();
        }
    }
}