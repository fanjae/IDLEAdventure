using System.Collections.Generic;
using UnityEngine;

public class BattleSoundController : MonoBehaviour
{
    public static BattleSoundController Instance { get; private set; }

    [Header("전투 BGM")]
    [SerializeField] private AudioClip normalBattleBgm;
    [SerializeField, Range(0.0f, 1.0f)] private float normalBattleVolume = 0.3f;

    [SerializeField] private AudioClip bossBattleBgm;
    [SerializeField, Range(0.0f, 1.0f)] private float bossBattleVolume = 0.3f;

    [SerializeField] private AudioClip fieldBossBattleBgm;
    [SerializeField, Range(0.0f, 1.0f)] private float fieldBossBattleVolume = 0.3f;

    [Header("전투 결과 SFX")]
    [SerializeField] private AudioClip victorySfx;
    [SerializeField, Range(0.0f, 1.0f)] private float victoryVolume = 0.5f;

    [SerializeField] private AudioClip defeatSfx;
    [SerializeField, Range(0.0f, 1.0f)] private float defeatVolume = 0.5f;

    [Header("기본 공격 SFX")]
    [SerializeField] private AudioClip basicHitSfx;
    [SerializeField, Range(0.0f, 1.0f)] private float basicHitVolume = 0.2f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        Instance = this;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0.0f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0.0f;
    }
    void Start()
    {
        if (BattleManager.Instance == null) return;

        BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        BattleManager.Instance.OnBattleEnded += HandleBattleEnded;
    }
    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
        if (Instance == this) Instance = null;
    }

    private void HandleBattleStarted()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.StopBgm();

        PlayBattleBgm();
    }
    private void PlayBattleBgm()
    {
        IReadOnlyList<BattleUnit> enemies = BattleManager.Instance.EnemyUnits;
        bool hasBoss = false;
        for (int i = 0; i < enemies.Count; i++)
        {
            BattleUnit enemy = enemies[i];
            if (enemy == null) continue;

            //필드 보스
            if (enemy.GetComponent<FieldBossBehaviorTree>() != null)
            {
                PlayBgm(fieldBossBattleBgm, fieldBossBattleVolume);
                return;
            }
            //일반 보스
            if (enemy.UnitData is EnemyData enemyData && enemyData.IsBoss) hasBoss = true;
        }

        if (hasBoss)
        {
            PlayBgm(bossBattleBgm, bossBattleVolume);
            return;
        }

        PlayBgm(normalBattleBgm, normalBattleVolume);
    }
    private void PlayBgm(AudioClip clip, float volume)
    {
        if (clip == null) return;

        float optionVolume = 1.0f;
        if (SoundManager.Instance != null) optionVolume = SoundManager.Instance.BgmVolume;

        bgmSource.clip = clip;
        bgmSource.volume = volume * optionVolume;
        bgmSource.Play();
    }
    private void HandleBattleEnded(UnitTeam winner)
    {
        bgmSource.Stop();
        bgmSource.clip = null;

        if (winner == UnitTeam.Hero)
        {
            PlaySfx(victorySfx, victoryVolume);
        }
        else
        {
            PlaySfx(defeatSfx, defeatVolume);
        }
    }
    public void PlayBasicHit()
    {
        PlayCombatSfx(basicHitSfx, basicHitVolume);
    }
    public void PlayCombatSfx(AudioClip clip, float volume)
    {
        PlaySfx(clip, volume);
    }
    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null) return;

        float optionVolume = 1.0f;
        if (SoundManager.Instance != null) optionVolume = SoundManager.Instance.SfxVolume;

        sfxSource.PlayOneShot(clip, volume * optionVolume);
    }
}
