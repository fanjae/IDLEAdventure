using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 종료 옵션 패널 표시 및 게임 종료를 관리하는 클래스
public sealed class ExitOptionController : MonoBehaviour
{
    [Header("Exit UI")]
    [SerializeField] private GameObject exitOptionPanel;
    [SerializeField] private UIPanelTransition exitOptionTransition;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button exitButton;

    // 종료 옵션 패널이 열려있는지 확인
    private bool isExitPanelOpen;

    // 게임 종료 처리 중인지 확인
    private bool isExiting;

    // 종료 옵션 UI 초기화 및 버튼 이벤트 등록
    private void Awake()
    {
        // 필요한 UI 참조가 설정되지 않은 경우 기능 비활성화
        if (exitOptionPanel == null || exitOptionTransition == null || cancelButton == null || exitButton == null)
        {
            Debug.LogError("ExitOptionController의 UI 참조가 설정되지 않았습니다.", this);
            enabled = false;
            return;
        }

        // 시작 시 종료 옵션 패널을 닫힌 상태로 초기화
        exitOptionPanel.SetActive(false);
        isExitPanelOpen = false;

        // 종료 옵션 버튼 이벤트 등록
        cancelButton.onClick.AddListener(CloseExitOptionPanel);
        exitButton.onClick.AddListener(ExitGame);
    }

    // ESC 입력에 따라 종료 옵션 패널 열기 및 닫기
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        // 키보드 입력이 없거나 ESC 입력이 아닌 경우 처리하지 않음
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        // 현재 패널 상태에 따라 열기 또는 닫기
        if (isExitPanelOpen)
        {
            CloseExitOptionPanel();
        }
        else
        {
            OpenExitOptionPanel();
        }
    }

    // 등록한 버튼 이벤트 해제
    private void OnDestroy()
    {
        cancelButton?.onClick.RemoveListener(CloseExitOptionPanel);
        exitButton?.onClick.RemoveListener(ExitGame);
    }

    // 종료 옵션 패널 열기
    public void OpenExitOptionPanel()
    {
        // 종료 처리 중이거나 이미 패널이 열려있는 경우 처리하지 않음
        if (isExiting || isExitPanelOpen)
        {
            return;
        }

        isExitPanelOpen = true;
        exitOptionPanel.SetActive(true);

        // 왼쪽에서 오른쪽으로 이동하면서 페이드인
        exitOptionTransition.PlayOpen();
    }

    // 종료 옵션 패널 닫기
    public void CloseExitOptionPanel()
    {
        // 패널이 닫혀있는 경우 처리하지 않음
        if (!isExitPanelOpen)
        {
            return;
        }

        isExitPanelOpen = false;

        // 닫기 연출이 끝난 뒤 패널 비활성화
        exitOptionTransition.PlayClose(() =>
        {
            exitOptionPanel.SetActive(false);
        });
    }

    // 현재 데이터를 저장한 뒤 게임 종료
    private void ExitGame()
    {
        // 중복 종료 요청 방지
        if (isExiting)
        {
            return;
        }

        isExiting = true;

        // 저장 가능한 데이터가 없는 경우 종료하지 않음
        if (!SaveManager.TryGetExistingInstance(out SaveManager saveManager) ||
            saveManager.CurrentData == null)
        {
            Debug.LogError("저장 데이터가 없어 게임을 종료하지 않습니다.", this);
            isExiting = false;
            return;
        }

        // 현재 게임 데이터 저장
        saveManager.Save();

        #if UNITY_EDITOR
        // 에디터에서는 Play Mode 종료
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 빌드에서는 게임 종료
        Application.Quit();
        #endif
    }
}