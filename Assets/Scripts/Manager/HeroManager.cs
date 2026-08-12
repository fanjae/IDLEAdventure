using System;
using UnityEngine;

// 게임 전체에서 사용할 HeroController 인스턴스 관리
public sealed class HeroManager : Singleton<HeroManager>
{
    // 실제 보유 영웅 관련 기능을 처리할 컨트롤러
    private HeroController controller;

    // 외부에서 사용할 Controller 반환
    public HeroController Controller
    {
        get
        {
            if (controller == null)
            {
                throw new InvalidOperationException("HeroManager가 초기화되지 않았습니다.");
            }

            return controller;
        }
    }

    // 초기화 체크
    public bool IsInitialized => controller != null;

    // HeroDatabaseSO를 이용해 게임에서 사용할 HeroController 생성
    public void Initialize(HeroDatabaseSO heroDatabase)
    {
        if (heroDatabase == null)
        {
            throw new ArgumentNullException(nameof(heroDatabase));
        }

        if (IsInitialized)
        {
            return;
        }

        controller = new HeroController(heroDatabase);
    }
}