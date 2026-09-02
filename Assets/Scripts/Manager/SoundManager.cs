using UnityEngine;

// 게임 전체 BGM 및 SFX 재생과 음량 관리
public sealed class SoundManager : Singleton<SoundManager>
{
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    // 저장된 옵션값을 사운드 설정에 반영
    public void Initialize(OptionSaveData optionData)
    {
        if (optionData == null)
        {
            SetBgmVolume(1f);
            SetSfxVolume(1f);
            return;
        }

        SetBgmVolume(optionData.BgmVolume);
        SetSfxVolume(optionData.SfxVolume);
    }

    // BGM 재생
    public void PlayBgm(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // 현재 BGM 정지
    public void StopBgm()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    // SFX 1회 재생
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    // BGM 음량 설정
    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    // SFX 음량 설정
    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // BGM 전용 AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;

        // SFX 전용 AudioSource
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
    }
}