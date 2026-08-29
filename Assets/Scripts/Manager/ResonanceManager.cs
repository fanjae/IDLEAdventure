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
    }
}