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
        PlayBgm();
    }

    private void PlayBgm()
    {
        if (bgm == null) return;

        float volume = 1f;
        if (SoundManager.Instance != null) volume = SoundManager.Instance.BgmVolume;

        bgmSource.clip = bgm;
        bgmSource.volume = bgmVolume * volume;
        bgmSource.Play();
    }
}