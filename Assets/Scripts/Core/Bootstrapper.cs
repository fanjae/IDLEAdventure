using UnityEngine;

/// <summary>
/// 게임 실행 시 불러와야 할 것들 호출하는 클래스. <br/>
/// 보통 매니저들이 들어올 것으로 보인다. <br/>
/// 현재 추가된 목록 <br/>
/// CurrencyManager
/// </summary>
public class Bootstrapper
{
    // 씬 실행 시 호출
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeWhenSceneStart()
    {
        Debug.Log("초기 생성 호출.");

        CurrencyManager currency = CurrencyManager.Instance;
        TestSaveManager testSaveManager = TestSaveManager.Instance;

        Debug.Log("초기 호출 완료.");
    }
}