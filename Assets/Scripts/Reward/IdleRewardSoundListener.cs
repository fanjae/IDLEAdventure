using UnityEngine;

public class IdleRewardSoundListener : MonoBehaviour
{
    [Header("IdleReward SFX")]
    [SerializeField] private AudioClip IdleRewardSFX;

    public void PlayIdleRewardSound()
    {
        if (IdleRewardSFX != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(IdleRewardSFX);
        }
    }
}
