using UnityEngine;
using UnityEngine.SceneManagement;

// 전투 배치 패널 UI 관리
public sealed class FormationPanelController : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "Filed_Persistent";

    // 전투 시작 시 배치 패널 닫음
    public void CloseFormationPanel()
    {
        gameObject.SetActive(false);
    }

    // 메인 화면으로 이동
    public void ReturnToMain()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}