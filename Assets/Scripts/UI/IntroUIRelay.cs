using UnityEngine;

// 인트로 프리팹 버튼과 씬 컨트롤러 사이 중계함
public class IntroUIRelay : MonoBehaviour
{
    public void StartGame()
    {
        if (IntroStartController.Instance == null)
        {
            Debug.LogWarning("IntroStartController를 찾을 수 없습니다.", this);
            return;
        }

        IntroStartController.Instance.StartGame();
    }
}
