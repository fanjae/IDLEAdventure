using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class StageSelectController : MonoBehaviour
{
    [SerializeField]
    private string battleSceneName = "StageTestScene";

    public void SelectStage(int stageId)
    {
        StageRuntimeData.SelectStage(stageId);

        Debug.Log($"스테이지 선택: {stageId}");

        SceneManager.LoadScene(battleSceneName);
    }
}