using UnityEngine;

// 메인과 배틀에서 같이 쓰는 설정 확인 버튼 중계
public class SettingsPopupRelay : MonoBehaviour
{
    public void Confirm()
    {
        if (BattleUIController.Instance != null)
        {
            BattleUIController.Instance.GoBack();
            return;
        }

        if (MainUIController.Instance != null)
        {
            MainUIController.Instance.CloseSettings();
            return;
        }

        Debug.LogWarning("설정 팝업을 닫을 UI 컨트롤러를 찾을 수 없습니다.", this);
    }
}
