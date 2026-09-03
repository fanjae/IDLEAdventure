using System;
using UnityEngine;

/// <summary>
/// 상호작용 종류 열거형.
/// </summary>
public enum InteractType
{
    None,
    NPC, Chest, Enemy, Gather,
    Length
}

/// <summary>
/// 상호작용 UI 관련 매니저 클래스.
/// </summary>
public class InteractionUIManager : LocalSingleton<InteractionUIManager>
{
    [Header("UI Component")]
    [SerializeField] private GameObject npcInteractionButton;
    [SerializeField] private GameObject chestInteractionButton;
    [SerializeField] private GameObject enemyInteractionButton;
    [SerializeField] private GameObject gatherInteractionButton;

    private Action currentInteraction;
    private InteractType currentInteractType = InteractType.None;

    protected override void Awake()
    {
        base.Awake(); 
        
        HideAllButtons();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    // 알맞은 상호작용 버튼 출력 함수.
    public void SetInteractable(bool isEnter, InteractType type, Action onClickAction = null)
    {
        HideAllButtons();

        if (isEnter)
        {
            currentInteraction = onClickAction;
            currentInteractType = type;

            switch (type)
            {
                case InteractType.NPC:
                    if (npcInteractionButton != null)
                    {
                        npcInteractionButton.SetActive(true);
                    }
                    break;
                case InteractType.Chest:
                    if (chestInteractionButton != null)
                    {
                        chestInteractionButton.SetActive(true);
                    }
                    break;
                case InteractType.Enemy:
                    if (enemyInteractionButton != null)
                    {
                        enemyInteractionButton.SetActive(true);
                    }
                    break;
                case InteractType.Gather:
                    if (gatherInteractionButton != null)
                    {
                        gatherInteractionButton.SetActive(true);
                    }
                    break;
            }
        }
        else
        {
            if (currentInteractType == type)
            {
                currentInteraction = null;
                currentInteractType = InteractType.None;
            }
        }
    }
    // NPC 상호작용 클릭 함수.
    public void OnClickNpcTalkButton()
    {
        currentInteraction?.Invoke();
        HideAllButtons();
    }
    // Chest 상호작용 클릭 함수.
    public void OnClickChestOpenButton()
    {
        currentInteraction?.Invoke();
        HideAllButtons();
    }
    // Enemy 상호작용 클릭 함수.
    public void OnClickEnemyBattleButton()
    {
        currentInteraction?.Invoke();
        HideAllButtons();
    }
    // Gather 상호작용 클릭 함수.
    public void OnClickGatherButton()
    {
        currentInteraction?.Invoke();
        HideAllButtons();
    }
    // UI 가리기 함수.
    private void HideAllButtons()
    {
        if (npcInteractionButton != null) 
        { 
            npcInteractionButton.SetActive(false);
        }
        if (chestInteractionButton != null)
        {
            chestInteractionButton.SetActive(false);
        }
        if (enemyInteractionButton != null)
        {
            enemyInteractionButton.SetActive(false);
        }
        if (gatherInteractionButton != null)
        {
            gatherInteractionButton.SetActive(false);
        }
    }
}
