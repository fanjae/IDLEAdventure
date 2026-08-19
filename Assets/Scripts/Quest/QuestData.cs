using UnityEngine;

/// <summary>
/// 퀘스트 데이터를 담을 SO 클래스. <br/>
/// 담고있는 정보 <br/>
/// Quest ID, Name, Type, Target, Dialogue
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Game Data/Quest/QuestData")]
public class QuestData : ScriptableObject
{
    // 퀘스트 기본 정보들
    [Header("Quest Info")]
    [SerializeField] private int qeustId;
    [SerializeField] private string questName;
    [SerializeField] private QuestType questType;
    // 퀘스트 목표 위치
    [Header("Target Data")]
    [SerializeField] private Vector3 target;
    // 퀴스트 대사
    [Header("Dialogue Data")]
    [SerializeField] private QuestDialogueData dialogueData;
    
    // 프로퍼티
    public int QuestId => qeustId;
    public string QuestName => questName;
    public QuestType QuestType => questType;

    public Vector3 Target => target;

    public QuestDialogueData DialogueData => dialogueData;
}