using UnityEngine;

public sealed class SceneSoundController : MonoBehaviour
{
    [Header("씬 BGM")]
    [SerializeField] private AudioClip bgm;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0f;

    private AudioSource bgmSource;

    private void Awake()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
    }

    private void Start()
    {
        if (SoundManager.TryGetExistingInstance(out SoundManager soundManager))
        {
            soundManager.OnBgmVolumeChanged += HandleBgmVolumeChanged;
        }

        PlayBgm();
    }

    private void OnDestroy()
    {
        if (SoundManager.TryGetExistingInstance(out SoundManager soundManager))
        {
            soundManager.OnBgmVolumeChanged -= HandleBgmVolumeChanged;
        }
    }

    private void PlayBgm()
    {
        if (bgm == null) return;

        float optionVolume = 1f;
        if (SoundManager.TryGetExistingInstance(out SoundManager soundManager)) optionVolume = soundManager.BgmVolume;

        bgmSource.clip = bgm;
        bgmSource.volume = bgmVolume * optionVolume;
        bgmSource.Play();
    }

    // 옵션 BGM 음량 변경값을 현재 재생 중인 BGM에 반영
    private void HandleBgmVolumeChanged(float optionVolume)
    {
        bgmSource.volume = bgmVolume * optionVolume;
    }
}