using UnityEngine;

/// <summary>
/// Global Singleton Class. <br/>
/// 사용 및 주의사항 <br/>
/// 1. 직접 하이어라키에 객체를 만들어 컴포넌트를 넣는 방식으로 사용하지 않는다. <br/>
///  ㄴ Find 메서드를 제거하기 위함. <br/>
/// 2. 상속받은 클래스에 초기 생성 함수 추가 <br/>
/// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] <br/>
/// private static void InitializeWhenSceneStart() <br/>
/// { <br/>
/// var init = Instace; <br/>
/// } <br/>
///  ㄴ 객체를 하이어라키에 미리 생성해두지 않기 때문에 런타임 중 생성을 해줘야 하는데, 게임 시작 로딩 때 생성할 예정이나
///  아직 로딩이 없기에 씬 실행 시 강제로 초기화 함수를 실행시켜 객체를 생성한다. <br/>
///  ㄴ 매니저나 실행과 동시에 하나 생성되어 존재해야 하는 싱글톤 객체를 만든 것이라면 Core 폴더의 Bootstrapper 스크립트의 실행 함수에 추가해주면 생성 호출이 된다.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 실제 싱글톤 객체를 저장하는 변수
    private static T instance;

    // 스레드 환경에서도 안전하게 객체를 생성하기 위한 읽기 전용 변수
    // 빈 객체를 생성해 저장함으로 해당 객체를 가지고 있는지 여부를 판단하기 위함
    private static readonly object isLock = new object();

    // 앱이 종료 중인지 확인하는 변수
    private static bool isQuitting = false;

    // 외부에서 접근할 프로퍼티
    public static T Instance
    {
        get
        {
            // 앱이 종료 중이라면 새로운 객체를 생성하지 않고 null 반환
            if (isQuitting) return null;
            // 비동기 작업 시 동시에 여러 객체가 생성될 수 있는 상황 방지
            // static으로 선언해둔 isLock 변수를 통해 검사를 진행
            // 동시에 여러 객체가 실행하려고 하면 하나만 실행하고 다른 실행 호출은 대기
            // 실행된 작업이 끝나면 대기 중이던 실행 진입
            lock (isLock)
            {
                // 객체가 없다면
                if (instance == null)
                {
                    // 새 객체를 생성
                    GameObject obj = new GameObject(typeof(T).Name);
                    // T 컴포넌트 추가
                    instance = obj.AddComponent<T>();
                    Debug.Log($"{instance.name} 객체 생성 완료.");
                }
                return instance;
            }
        }
    }

    protected virtual void Awake()
    {
        // 씬 시작 시 false로 초기화
        isQuitting = false;

        // 아직 싱글톤 객체가 없다면
        if (instance == null)
        {
            // 현재 객체를 T 타입으로 변환 후 싱글톤으로 등록
            instance = this as T;
            // 씬이 바뀌어도 삭제되지 않도록 해주는 메서드
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // 이미 해당 타입의 싱글톤이 존재한다면, 새로 만들어지는 객체 제거
            Destroy(gameObject);
        }
    }

    // 객체가 파괴될 때
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // 앱이 종료될 때
    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }
}