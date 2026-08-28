using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 자동전투 진행 관리
public sealed class AutoBattleController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button autoBattleButton;
    [SerializeField] private FormationPanelController formationPanelController;
    [SerializeField] private GameObject stopAutoBattlePanel;
    [SerializeField] private TMP_Text stageInfoText;
    [SerializeField] private Image autoBattleTouchAreaImage;

    [Header("전투 설정")]
    [SerializeField] private float stageTransitionDelay = 0.7f;

    [Header("씬")]
    [SerializeField] private string battleSceneName = "NewUIBattleScene";

    private void Start()
    {
        if (autoBattleButton != null)
        {
            autoBattleButton.onClick.AddListener(StartAutoBattle);
        }

        if (stopAutoBattlePanel != null)
        {
            stopAutoBattlePanel.SetActive(false);
        }

        SetAutoBattleTouchAreaActive(StageRuntimeData.IsAutoBattle);

        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없습니다.");
            return;
        }

        BattleManager.Instance.OnBattleEnded += HandleBattleEnded;

        // 이전 스테이지에서 자동전투 중이었다면 초기화 후 전투 시작
        if (StageRuntimeData.IsAutoBattle)
        {
            if (formationPanelController != null)
            {
                formationPanelController.CloseFormationPanel();
            }

            StartCoroutine(StartBattleAfterInitialization());
        }
    }

    private void OnDestroy()
    {
        if (autoBattleButton != null)
        {
            autoBattleButton.onClick.RemoveListener(StartAutoBattle);
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
    }

    // 자동전투 시작
    private void StartAutoBattle()
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없습니다.");
            return;
        }

        StageRuntimeData.StartAutoBattle();

        if (formationPanelController != null)
        {
            formationPanelController.CloseFormationPanel();
        }

        SetAutoBattleTouchAreaActive(StageRuntimeData.IsAutoBattle);

        BattleManager.Instance.StartBattle();
    }

    // 전투 종료 처리
    private void HandleBattleEnded(UnitTeam winner)
    {
        if (!StageRuntimeData.IsAutoBattle)
        {
            return;
        }

        SetAutoBattleTouchAreaActive(false);

        // 패배하면 자동전투 종료
        if (winner == UnitTeam.Enemy)
        {
            StopAutoBattle();
            return;
        }

        StartCoroutine(MoveNextStageAfterDelay());
    }

    // 승리 후 다음 스테이지 진행
    private IEnumerator MoveNextStageAfterDelay()
    {
        yield return new WaitForSeconds(stageTransitionDelay);

        if (!StageRuntimeData.IsAutoBattle)
        {
            yield break;
        }

        MoveNextStage();
    }

    // 다음 스테이지 이동
    private void MoveNextStage()
    {
        if (StageDatabase.Instance == null)
        {
            Debug.LogError("StageDatabase가 없습니다.");
            StopAutoBattle();
            return;
        }

        int currentStageId = StageRuntimeData.SelectedStageId;
        int nextStageId = currentStageId + 1;

        if (!StageDatabase.Instance.TryGetStage(nextStageId, out _))
        {
            StopAutoBattle();
            return;
        }

        StageRuntimeData.SelectStage(nextStageId);
        SceneManager.LoadScene(battleSceneName);
    }

    // 자동전투 종료 확인창 표시
    public void ShowStopAutoBattlePanel()
    {
        if (!StageRuntimeData.IsAutoBattle)
        {
            return;
        }

        SetAutoBattleTouchAreaActive(false);

        RefreshStageInfoText();

        if (stopAutoBattlePanel != null)
        {
            stopAutoBattlePanel.SetActive(true);
        }
    }

    // 자동전투 계속 진행
    public void ContinueAutoBattle()
    {
        if (stopAutoBattlePanel != null)
        {
            stopAutoBattlePanel.SetActive(false);
        }

        if (StageRuntimeData.IsAutoBattle && BattleManager.Instance != null && BattleManager.Instance.IsBattleRunning)
        {
            SetAutoBattleTouchAreaActive(true);
        }
    }

    // 자동전투 종료
    public void StopAutoBattle()
    {
        StageRuntimeData.StopAutoBattle();

        if (stopAutoBattlePanel != null)
        {
            stopAutoBattlePanel.SetActive(false);
        }

        SetAutoBattleTouchAreaActive(false);
    }

    // 전투 유닛 초기화 이후 자동전투 시작
    private IEnumerator StartBattleAfterInitialization()
    {
        yield return null;

        if (!StageRuntimeData.IsAutoBattle)
        {
            yield break;
        }

        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없습니다.");
            yield break;
        }

        BattleManager.Instance.StartBattle();
    }

    // 자동전투 화면 클릭 영역 활성화 설정
    private void SetAutoBattleTouchAreaActive(bool active)
    {
        if (autoBattleTouchAreaImage == null)
        {
            return;
        }

        autoBattleTouchAreaImage.gameObject.SetActive(active);
        autoBattleTouchAreaImage.raycastTarget = active;
    }

    // 현재 진행 중인 스테이지 정보 갱신
    private void RefreshStageInfoText()
    {
        if (stageInfoText == null)
        {
            return;
        }

        int currentStageId = StageRuntimeData.SelectedStageId;
        stageInfoText.text = $"현재 진행 스테이지 : {currentStageId}";
    }
}