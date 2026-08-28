using System;
using UnityEngine;

// 메인 UI 프리팹 버튼과 MainUIController 사이 중계
// OnClick은 이 컴포넌트 메서드만 참조
public class MainUIRelay : MonoBehaviour
{
    private void WithController(Action<MainUIController> action)
    {
        if (MainUIController.Instance == null)
        {
            Debug.LogWarning("MainUIController를 찾을 수 없습니다.", this);
            return;
        }

        action(MainUIController.Instance);
    }

    public void OpenHome() => WithController(controller => controller.OpenHome());
    public void OpenHeroes() => WithController(controller => controller.OpenHeroes());
    public void OpenGacha() => WithController(controller => controller.OpenGacha());
    public void OpenEquipment() => WithController(controller => controller.OpenEquipment());
    public void OpenResonance() => WithController(controller => controller.OpenResonance());
    public void OpenShop() => WithController(controller => controller.OpenShop());
    public void OpenAdventure() => WithController(controller => controller.OpenAdventure());
    public void OpenIdleRewards() => WithController(controller => controller.OpenIdleRewards());
    public void OpenSettings() => WithController(controller => controller.OpenSettings());
    public void CloseIdleRewards() => WithController(controller => controller.CloseIdleRewards());
    public void CloseSettings() => WithController(controller => controller.CloseSettings());
    public void GoBack() => WithController(controller => controller.GoBack());
}
