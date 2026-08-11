using System;
using UnityEngine;

// 배틀 UI 프리팹 버튼과 BattleUIController 사이 중계
public class BattleUIRelay : MonoBehaviour
{
    private void WithController(Action<BattleUIController> action)
    {
        if (BattleUIController.Instance == null)
        {
            Debug.LogWarning("BattleUIController를 찾을 수 없습니다.", this);
            return;
        }

        action(BattleUIController.Instance);
    }

    public void SelectStage(int stageNumber) => WithController(controller => controller.SelectStage(stageNumber));
    public void OpenStageSelect() => WithController(controller => controller.OpenStageSelect());
    public void OpenHeroSelection() => WithController(controller => controller.OpenHeroSelection());
    public void OpenFormation() => WithController(controller => controller.OpenFormation());
    public void StartBattle() => WithController(controller => controller.StartBattle());
    public void ShowBattleResult() => WithController(controller => controller.ShowBattleResult());
    public void RetryBattle() => WithController(controller => controller.RetryBattle());
    public void ChallengeNextStage() => WithController(controller => controller.ChallengeNextStage());
    public void OpenPausePopup() => WithController(controller => controller.OpenPausePopup());
    public void ClosePausePopup() => WithController(controller => controller.ClosePausePopup());
    public void OpenSettingsPopup() => WithController(controller => controller.OpenSettingsPopup());
    public void CloseSettingsPopup() => WithController(controller => controller.CloseSettingsPopup());
    public void CloseBattleResultPopup() => WithController(controller => controller.CloseBattleResultPopup());
    public void GoBack() => WithController(controller => controller.GoBack());
    public void ReturnToMain() => WithController(controller => controller.ReturnToMain());
}
