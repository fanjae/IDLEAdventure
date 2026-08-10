using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroStartController : MonoBehaviour
{
    public static IntroStartController Instance { get; private set; }

    [SerializeField] private string mainSceneName = "MainScene";

    private bool isLoading;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartGame()
    {
        // 중복 입력 막음
        if (isLoading) return;
        
        isLoading = true;
        SceneManager.LoadScene(mainSceneName);
    }
}
