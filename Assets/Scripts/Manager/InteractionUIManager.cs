using System;
using UnityEngine;

public enum InteractType
{
    None,
    NPC, Chest, Enemy,
    Length
}

public class InteractionUIManager : LocalSingleton<InteractionUIManager>
{
    [Header("UI Component")]
    [SerializeField] private GameObject npcInteractionButton;
    [SerializeField] private GameObject chestInteractionButton;
    [SerializeField] private GameObject enemyInteractionButton;

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

    public void OnClickNpcTalkButton()
    {
        currentInteraction?.Invoke();
        HideAllButtons();
    }

    public void OnClickChestOpenButton()
    {
        currentInteraction?.Invoke();
        HideAllButtons();
    }

    public void OnClickEnemyBattleButton()
    {
        currentInteraction?.Invoke();
        HideAllButtons();
    }

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
    }
}
