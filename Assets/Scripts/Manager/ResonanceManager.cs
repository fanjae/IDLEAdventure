using System;

// 게임 전체에서 사용할 ResonanceController 인스턴스 관리
public sealed class ResonanceManager : Singleton<ResonanceManager>
{
    // 실제 공명 관련 기능을 처리할 컨트롤러
    private ResonanceController controller;

    // 외부에서 사용할 Controller 반환
    public ResonanceController Controller
    {
        get
        {
            if (controller == null)
            {
                throw new InvalidOperationException("ResonanceManager가 초기화되지 않았습니다.");
            }

            return controller;
        }
    }

    // 초기화 체크
    public bool IsInitialized => controller != null;

    // HeroController를 이용해 게임에서 사용할 ResonanceController 생성
    public void Initialize(HeroController heroController)
    {
        if (heroController == null)
        {
            throw new ArgumentNullException(nameof(heroController));
        }

        if (IsInitialized)
        {
            return;
        }

        // 공명 관련 기능을 처리할 컨트롤러 생성
        controller = new ResonanceController(heroController);

        // 공명 상태를 저장 대상으로 등록
        SaveManager.Instance.RegisterWriter(controller);
    }

    protected override void OnDestroy()
    {
        if (controller != null)
        {
            controller.Dispose();
        }

        // 종료 순서상 SaveManager가 먼저 제거되었을 수 있으므로 기존 인스턴스가 있을 때만 해제
        if (controller != null && SaveManager.TryGetExistingInstance(out SaveManager saveManager))
        {
            saveManager.UnregisterWriter(controller);
        }

        base.OnDestroy();
    }
}