using UnityEngine;

// 전투 진행 상태에 따른 하단 HUD 표시 관리
public sealed class BattleBottomHUDController : MonoBehaviour
{
    [SerializeField] private GameObject battleBottomHUD;

    private void Start()
    {
        HideBattleHUD();

        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없습니다.");
            return;
        }

        BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        BattleManager.Instance.OnBattleEnded += HandleBattleEnded;

        // 이미 전투가 시작된 이후 활성화된 경우 현재 상태에 맞춰 갱신
        if (BattleManager.Instance.IsBattleRunning)
        {
            ShowBattleHUD();
        }
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
    }

    // 전투 시작 시 하단 전투 HUD 표시
    private void HandleBattleStarted()
    {
        ShowBattleHUD();
    }

    // 전투 종료 시 하단 전투 HUD 숨김
    private void HandleBattleEnded(UnitTeam winner)
    {
        HideBattleHUD();
    }

    private void ShowBattleHUD()
    {
        if (battleBottomHUD != null)
        {
            battleBottomHUD.SetActive(true);
        }
    }

    private void HideBattleHUD()
    {
        if (battleBottomHUD != null)
        {
            battleBottomHUD.SetActive(false);
        }
    }
}