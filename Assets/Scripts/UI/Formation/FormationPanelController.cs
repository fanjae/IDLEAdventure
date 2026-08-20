using UnityEngine;
using UnityEngine.SceneManagement;

// 전투 배치 패널 UI 관리
public sealed class FormationPanelController : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "ItemandSaveTestMainScene";

    // 메인 화면으로 이동
    public void ReturnToMain()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}