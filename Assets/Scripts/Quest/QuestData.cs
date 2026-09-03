using UnityEngine;

/// <summary>
/// 퀘스트 데이터를 담을 SO 클래스. <br/>
/// 담고있는 정보 <br/>
/// Quest ID, Name, Type, Target, TargetPrefab, Dialogue
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Game Data/Quest/QuestData")]
public class QuestData : ScriptableObject
{
    // 퀘스트 기본 정보들
    [Header("Quest Info")]
    [SerializeField] private int questId;
    [SerializeField] private string questName;
    [SerializeField] private QuestType questType;
    [SerializeField] private QuestKind questKind;
    // 퀘스트 목표 위치
    [Header("Target Data")]
    [SerializeField] private Vector3 arrivePosition;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private GameObject interactablePrefab;
    // 퀴스트 대사
    [Header("Dialogue Data")]
    [SerializeField] private QuestDialogueData dialogueData;
    // 채집 정보
    [Header("Gathering Data")]
    [SerializeField] private int targetCount = 1;
    [SerializeField] private float spawnRadius = 5.0f;
    [SerializeField] private float spawnDistance = 1.5f;

    [Header("Reward Data")]
    [SerializeField] private QuestRewardData rewardData;

    // 프로퍼티
    public int QuestId => questId;
    public string QuestName => questName;
    public QuestType QuestType => questType;
    public QuestKind QuestKind => questKind;

    public Vector3 ArrivePosition => arrivePosition;
    public Vector3 SpawnPosition => spawnPosition;
    public GameObject InteractablePrefab => interactablePrefab;

    public int TargetCount => targetCount;
    public float SpawnRadius => spawnRadius;
    public float MinSpawnDistance => spawnDistance;

    public QuestDialogueData DialogueData => dialogueData;
    public QuestRewardData RewardData => rewardData;
}