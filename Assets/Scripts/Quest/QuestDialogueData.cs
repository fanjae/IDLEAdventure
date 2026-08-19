using System;
using UnityEngine;

/// <summary>
/// 다이얼로그가 가질 기본 정보들을 담은 구조체. <br/>
/// 분리해 버릴까...? <br/>
/// 가지고 있는 정보 <br/>
/// SpeakerName, DialogueText
/// </summary>
[Serializable]
public struct DialogueData
{
    [SerializeField] private string speakerName;
    [TextArea(3, 5)]
    [SerializeField] private string dialogueText;

    // 프로퍼티
    public string SpeakerName => speakerName;
    public string DialogueText => dialogueText;
}

/// <summary>
/// 퀘스트 대사를 담을 SO 클래스. <br/>
/// 가지고 있는 정보 <br/>
/// Dialogue Id, Datas(DialogueDatas)
/// </summary>
[CreateAssetMenu(fileName = "NewQuestDialogue", menuName = "Game Data/Quest/QuestDialogueData")]
public class QuestDialogueData : ScriptableObject
{
    [SerializeField] private string dialogueId;
    [SerializeField] private DialogueData[] dialogueDatas;

    // 프로퍼티
    public string DialogueId => dialogueId;
    public DialogueData[] DialogueDatas => dialogueDatas;
}