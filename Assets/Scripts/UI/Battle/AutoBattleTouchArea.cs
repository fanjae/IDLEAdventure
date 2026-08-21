using UnityEngine;
using UnityEngine.EventSystems;

// 자동전투 중 전투 화면 클릭 감지
public sealed class AutoBattleTouchArea : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AutoBattleController autoBattleController;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!StageRuntimeData.IsAutoBattle)
        {
            return;
        }

        if (autoBattleController == null)
        {
            return;
        }

        autoBattleController.ShowStopAutoBattlePanel();
    }
}