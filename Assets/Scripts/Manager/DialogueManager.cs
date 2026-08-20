using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 퀘스트 대사 출력을 위한 매니저 클래스. <br/>
/// 싱글톤 상속은 받지 않았지만, 기본적으로 싱글톤 형태를 취한다. <br/>
/// 단, 탐험 씬에만 존재한다.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Component")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialogueButton;

    private Queue<DialogueData> dialogueQueue = new Queue<DialogueData>();
    private Action onColpleteDialogue;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        if (dialogueButton != null)
        {
            dialogueButton.SetActive(false);
        }
    }

    // 대사 시작 함수.
    // 매개 변수로 전달 받은 대사 출력 함수를 호출.
    // 대사 출력이 끝났다면 해당 액션 변수 초기화.
    public void StartDialogue(QuestDialogueData data, Action onComplete)
    {
        if (data == null || data.DialogueDatas.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        onColpleteDialogue = onComplete;
        dialogueQueue.Clear();

        foreach (var line in data.DialogueDatas)
        {
            dialogueQueue.Enqueue(line);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        if (dialogueButton != null)
        {
            dialogueButton.SetActive(true);
        }
        NextDialogue();
    }

    // 다이얼로그에 들어있는 다음 대사 출력 함수.
    public void NextDialogue()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueData currentLine = dialogueQueue.Dequeue();
        if (speakerNameText != null)
        {
            speakerNameText.text = currentLine.SpeakerName;
        }
        if (dialogueText != null)
        {
            dialogueText.text = currentLine.DialogueText;
        }
    }

    // 다이얼로그 마지막 대사가 출력된 후 UI 종료 함수.
    private void EndDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        if (dialogueButton != null)
        {
            dialogueButton.SetActive(false);
        }

        onColpleteDialogue?.Invoke();
        onColpleteDialogue = null;
    }
}