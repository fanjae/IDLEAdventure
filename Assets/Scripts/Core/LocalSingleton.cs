using UnityEngine;

/// <summary>
/// 한 씬에만 존재할 싱글톤 객체를 위한 싱글톤 클래스.
/// </summary>
public class LocalSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    private static readonly object isLock = new object();

    // 앱이 종료 중인지 확인하는 변수
    private static bool isQuitting = false;

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

        if (instance == null)
        {
            instance = this as T;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
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