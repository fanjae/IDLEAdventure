using UnityEngine;

public class QuestSoundListener : MonoBehaviour
{
    [Header("Quest SFX")]
    [SerializeField] private AudioClip questClearSFX;

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCleared += PlayClearSound;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCleared -= PlayClearSound;
        }
    }

    private void PlayClearSound(QuestData clearedQuest)
    {
        if (questClearSFX != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(questClearSFX);
        }
    }
}
