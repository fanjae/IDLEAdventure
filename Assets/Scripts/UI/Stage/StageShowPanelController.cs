using TMPro;
using UnityEngine;

// 현재 스테이지 정보 UI 관리
public sealed class StageShowPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text stageInfoText;

    private void OnEnable()
    {
        UpdateStageInfo();
    }

    // 현재 선택된 스테이지 정보 갱신
    private void UpdateStageInfo()
    {
        int stageId = StageRuntimeData.SelectedStageId;

        if (stageId < 1)
        {
            stageInfoText.text = "스테이지 -";
            return;
        }

        stageInfoText.text = $"스테이지 {stageId}";
    }
}