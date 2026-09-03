using System.Collections.Generic;
using UnityEngine;

public class BattleSoundController : MonoBehaviour
{
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

    void Start()
    {
        if (BattleManager.Instance == null) return;

        BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        BattleManager.Instance.OnBattleEnded += HandleBattleEnded;
    }
    private void OnDestroy()
    {
        if (BattleManager.Instance == null) return;

        BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
    }

    private void HandleBattleStarted()
    {
        if (SoundManager.Instance == null) return;

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
                SoundManager.Instance.SetBgmVolume(fieldBossBattleVolume);
                SoundManager.Instance.PlayBgm(fieldBossBattleBgm);
                return;
            }
            //일반 보스
            if (enemy.UnitData is EnemyData enemyData && enemyData.IsBoss) hasBoss = true;
        }
        
        if (hasBoss)
        {
            SoundManager.Instance.SetBgmVolume(bossBattleVolume);
            SoundManager.Instance.PlayBgm(bossBattleBgm);
            return;
        }

        SoundManager.Instance.SetBgmVolume(normalBattleVolume);
        SoundManager.Instance.PlayBgm(normalBattleBgm);
    }
    private void HandleBattleEnded(UnitTeam winner)
    {
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.StopBgm();

        if (winner == UnitTeam.Hero)
        {
            SoundManager.Instance.SetSfxVolume(victoryVolume);
            SoundManager.Instance.PlaySfx(victorySfx);
        }
        else
        {
            SoundManager.Instance.SetSfxVolume(defeatVolume);
            SoundManager.Instance.PlaySfx(defeatSfx);
        }
    }
}
